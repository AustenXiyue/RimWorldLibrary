using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Threading;

/// <summary>Provides a mechanism for executing a method at specified intervals. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[ComVisible(true)]
public sealed class Timer : MarshalByRefObject, IDisposable
{
	private sealed class TimerComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			if (!(x is Timer timer))
			{
				return -1;
			}
			if (!(y is Timer timer2))
			{
				return 1;
			}
			long num = timer.next_run - timer2.next_run;
			if (num == 0L)
			{
				if (x != y)
				{
					return -1;
				}
				return 0;
			}
			if (num <= 0)
			{
				return -1;
			}
			return 1;
		}
	}

	private sealed class Scheduler
	{
		private static Scheduler instance;

		private SortedList list;

		private ManualResetEvent changed;

		public static Scheduler Instance => instance;

		static Scheduler()
		{
			instance = new Scheduler();
		}

		private Scheduler()
		{
			changed = new ManualResetEvent(initialState: false);
			list = new SortedList(new TimerComparer(), 1024);
			Thread thread = new Thread(SchedulerThread);
			thread.IsBackground = true;
			thread.Start();
		}

		public void Remove(Timer timer)
		{
			if (timer.next_run == 0L || timer.next_run == long.MaxValue)
			{
				return;
			}
			lock (this)
			{
				InternalRemove(timer);
			}
		}

		public void Change(Timer timer, long new_next_run)
		{
			bool flag = false;
			lock (this)
			{
				InternalRemove(timer);
				if (new_next_run == long.MaxValue)
				{
					timer.next_run = new_next_run;
					return;
				}
				if (!timer.disposed)
				{
					timer.next_run = new_next_run;
					Add(timer);
					flag = list.GetByIndex(0) == timer;
				}
			}
			if (flag)
			{
				changed.Set();
			}
		}

		private int FindByDueTime(long nr)
		{
			int i = 0;
			int num = list.Count - 1;
			if (num < 0)
			{
				return -1;
			}
			if (num < 20)
			{
				for (; i <= num; i++)
				{
					Timer timer = (Timer)list.GetByIndex(i);
					if (timer.next_run == nr)
					{
						return i;
					}
					if (timer.next_run > nr)
					{
						return -1;
					}
				}
				return -1;
			}
			while (i <= num)
			{
				int num2 = i + (num - i >> 1);
				Timer timer2 = (Timer)list.GetByIndex(num2);
				if (nr == timer2.next_run)
				{
					return num2;
				}
				if (nr > timer2.next_run)
				{
					i = num2 + 1;
				}
				else
				{
					num = num2 - 1;
				}
			}
			return -1;
		}

		private void Add(Timer timer)
		{
			int num = FindByDueTime(timer.next_run);
			if (num != -1)
			{
				bool flag = ((long.MaxValue - timer.next_run > 20000) ? true : false);
				do
				{
					num++;
					if (flag)
					{
						timer.next_run++;
					}
					else
					{
						timer.next_run--;
					}
				}
				while (num < list.Count && ((Timer)list.GetByIndex(num)).next_run == timer.next_run);
			}
			list.Add(timer, timer);
		}

		private int InternalRemove(Timer timer)
		{
			int num = list.IndexOfKey(timer);
			if (num >= 0)
			{
				list.RemoveAt(num);
			}
			return num;
		}

		private static void TimerCB(object o)
		{
			Timer timer = (Timer)o;
			timer.callback(timer.state);
		}

		private void SchedulerThread()
		{
			Thread.CurrentThread.Name = "Timer-Scheduler";
			List<Timer> list = new List<Timer>(512);
			while (true)
			{
				int num = -1;
				long timeMonotonic = GetTimeMonotonic();
				lock (this)
				{
					changed.Reset();
					int num2 = this.list.Count;
					int num3;
					for (num3 = 0; num3 < num2; num3++)
					{
						Timer timer = (Timer)this.list.GetByIndex(num3);
						if (timer.next_run > timeMonotonic)
						{
							break;
						}
						this.list.RemoveAt(num3);
						num2--;
						num3--;
						ThreadPool.UnsafeQueueUserWorkItem(TimerCB, timer);
						long period_ms = timer.period_ms;
						long due_time_ms = timer.due_time_ms;
						if (period_ms == -1 || ((period_ms == 0L || period_ms == -1) && due_time_ms != -1))
						{
							timer.next_run = long.MaxValue;
						}
						else
						{
							timer.next_run = GetTimeMonotonic() + 10000 * timer.period_ms;
							list.Add(timer);
						}
					}
					num2 = list.Count;
					for (num3 = 0; num3 < num2; num3++)
					{
						Timer timer2 = list[num3];
						Add(timer2);
					}
					list.Clear();
					ShrinkIfNeeded(list, 512);
					int capacity = this.list.Capacity;
					num2 = this.list.Count;
					if (capacity > 1024 && num2 > 0 && capacity / num2 > 3)
					{
						this.list.Capacity = num2 * 2;
					}
					long num4 = long.MaxValue;
					if (this.list.Count > 0)
					{
						num4 = ((Timer)this.list.GetByIndex(0)).next_run;
					}
					num = -1;
					if (num4 != long.MaxValue)
					{
						long num5 = (num4 - GetTimeMonotonic()) / 10000;
						if (num5 > int.MaxValue)
						{
							num = 2147483646;
						}
						else
						{
							num = (int)num5;
							if (num < 0)
							{
								num = 0;
							}
						}
					}
				}
				changed.WaitOne(num);
			}
		}

		private void ShrinkIfNeeded(List<Timer> list, int initial)
		{
			int capacity = list.Capacity;
			int count = list.Count;
			if (capacity > initial && count > 0 && capacity / count > 3)
			{
				list.Capacity = count * 2;
			}
		}
	}

	private static readonly Scheduler scheduler = Scheduler.Instance;

	private TimerCallback callback;

	private object state;

	private long due_time_ms;

	private long period_ms;

	private long next_run;

	private bool disposed;

	private const long MaxValue = 4294967294L;

	/// <summary>Initializes a new instance of the Timer class, using a 32-bit signed integer to specify the time interval.</summary>
	/// <param name="callback">A <see cref="T:System.Threading.TimerCallback" /> delegate representing a method to be executed. </param>
	/// <param name="state">An object containing information to be used by the callback method, or null. </param>
	/// <param name="dueTime">The amount of time to delay before <paramref name="callback" /> is invoked, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from starting. Specify zero (0) to start the timer immediately. </param>
	/// <param name="period">The time interval between invocations of <paramref name="callback" />, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="callback" /> parameter is null. </exception>
	public Timer(TimerCallback callback, object state, int dueTime, int period)
	{
		Init(callback, state, dueTime, period);
	}

	/// <summary>Initializes a new instance of the Timer class, using 64-bit signed integers to measure time intervals.</summary>
	/// <param name="callback">A <see cref="T:System.Threading.TimerCallback" /> delegate representing a method to be executed. </param>
	/// <param name="state">An object containing information to be used by the callback method, or null. </param>
	/// <param name="dueTime">The amount of time to delay before <paramref name="callback" /> is invoked, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from starting. Specify zero (0) to start the timer immediately. </param>
	/// <param name="period">The time interval between invocations of <paramref name="callback" />, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is greater than 4294967294. </exception>
	public Timer(TimerCallback callback, object state, long dueTime, long period)
	{
		Init(callback, state, dueTime, period);
	}

	/// <summary>Initializes a new instance of the Timer class, using <see cref="T:System.TimeSpan" /> values to measure time intervals.</summary>
	/// <param name="callback">A <see cref="T:System.Threading.TimerCallback" /> delegate representing a method to be executed. </param>
	/// <param name="state">An object containing information to be used by the callback method, or null. </param>
	/// <param name="dueTime">The <see cref="T:System.TimeSpan" /> representing the amount of time to delay before the <paramref name="callback" /> parameter invokes its methods. Specify negative one (-1) milliseconds to prevent the timer from starting. Specify zero (0) to start the timer immediately. </param>
	/// <param name="period">The time interval between invocations of the methods referenced by <paramref name="callback" />. Specify negative one (-1) milliseconds to disable periodic signaling. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of milliseconds in the value of <paramref name="dueTime" /> or <paramref name="period" /> is negative and not equal to <see cref="F:System.Threading.Timeout.Infinite" />, or is greater than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="callback" /> parameter is null. </exception>
	public Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
	{
		Init(callback, state, (long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);
	}

	/// <summary>Initializes a new instance of the Timer class, using 32-bit unsigned integers to measure time intervals.</summary>
	/// <param name="callback">A <see cref="T:System.Threading.TimerCallback" /> delegate representing a method to be executed. </param>
	/// <param name="state">An object containing information to be used by the callback method, or null. </param>
	/// <param name="dueTime">The amount of time to delay before <paramref name="callback" /> is invoked, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from starting. Specify zero (0) to start the timer immediately. </param>
	/// <param name="period">The time interval between invocations of <paramref name="callback" />, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="callback" /> parameter is null. </exception>
	[CLSCompliant(false)]
	public Timer(TimerCallback callback, object state, uint dueTime, uint period)
	{
		Init(callback, state, (dueTime == uint.MaxValue) ? (-1L) : ((long)dueTime), (period == uint.MaxValue) ? (-1L) : ((long)period));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Timer" /> class with an infinite period and an infinite due time, using the newly created <see cref="T:System.Threading.Timer" /> object as the state object. </summary>
	/// <param name="callback">A <see cref="T:System.Threading.TimerCallback" /> delegate representing a method to be executed.</param>
	public Timer(TimerCallback callback)
	{
		Init(callback, this, -1L, -1L);
	}

	private void Init(TimerCallback callback, object state, long dueTime, long period)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		this.callback = callback;
		this.state = state;
		Change(dueTime, period, first: true);
	}

	/// <summary>Changes the start time and the interval between method invocations for a timer, using 32-bit signed integers to measure time intervals.</summary>
	/// <returns>true if the timer was successfully updated; otherwise, false.</returns>
	/// <param name="dueTime">The amount of time to delay before the invoking the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from restarting. Specify zero (0) to restart the timer immediately. </param>
	/// <param name="period">The time interval between invocations of the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Timer" /> has already been disposed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <filterpriority>2</filterpriority>
	public bool Change(int dueTime, int period)
	{
		return Change(dueTime, period, first: false);
	}

	/// <summary>Changes the start time and the interval between method invocations for a timer, using <see cref="T:System.TimeSpan" /> values to measure time intervals.</summary>
	/// <returns>true if the timer was successfully updated; otherwise, false.</returns>
	/// <param name="dueTime">A <see cref="T:System.TimeSpan" /> representing the amount of time to delay before invoking the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed. Specify negative one (-1) milliseconds to prevent the timer from restarting. Specify zero (0) to restart the timer immediately. </param>
	/// <param name="period">The time interval between invocations of the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed. Specify negative one (-1) milliseconds to disable periodic signaling. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Timer" /> has already been disposed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter, in milliseconds, is less than -1. </exception>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter, in milliseconds, is greater than 4294967294. </exception>
	/// <filterpriority>2</filterpriority>
	public bool Change(TimeSpan dueTime, TimeSpan period)
	{
		return Change((long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds, first: false);
	}

	/// <summary>Changes the start time and the interval between method invocations for a timer, using 32-bit unsigned integers to measure time intervals.</summary>
	/// <returns>true if the timer was successfully updated; otherwise, false.</returns>
	/// <param name="dueTime">The amount of time to delay before the invoking the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from restarting. Specify zero (0) to restart the timer immediately. </param>
	/// <param name="period">The time interval between invocations of the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Timer" /> has already been disposed. </exception>
	/// <filterpriority>2</filterpriority>
	[CLSCompliant(false)]
	public bool Change(uint dueTime, uint period)
	{
		long dueTime2 = ((dueTime == uint.MaxValue) ? (-1L) : ((long)dueTime));
		long period2 = ((period == uint.MaxValue) ? (-1L) : ((long)period));
		return Change(dueTime2, period2, first: false);
	}

	/// <summary>Releases all resources used by the current instance of <see cref="T:System.Threading.Timer" />.</summary>
	/// <filterpriority>2</filterpriority>
	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			scheduler.Remove(this);
		}
	}

	/// <summary>Changes the start time and the interval between method invocations for a timer, using 64-bit signed integers to measure time intervals.</summary>
	/// <returns>true if the timer was successfully updated; otherwise, false.</returns>
	/// <param name="dueTime">The amount of time to delay before the invoking the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to prevent the timer from restarting. Specify zero (0) to restart the timer immediately. </param>
	/// <param name="period">The time interval between invocations of the callback method specified when the <see cref="T:System.Threading.Timer" /> was constructed, in milliseconds. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to disable periodic signaling. </param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Threading.Timer" /> has already been disposed. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is less than -1. </exception>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="dueTime" /> or <paramref name="period" /> parameter is greater than 4294967294. </exception>
	/// <filterpriority>2</filterpriority>
	public bool Change(long dueTime, long period)
	{
		return Change(dueTime, period, first: false);
	}

	private bool Change(long dueTime, long period, bool first)
	{
		if (dueTime > 4294967294u)
		{
			throw new ArgumentOutOfRangeException("dueTime", "Due time too large");
		}
		if (period > 4294967294u)
		{
			throw new ArgumentOutOfRangeException("period", "Period too large");
		}
		if (dueTime < -1)
		{
			throw new ArgumentOutOfRangeException("dueTime");
		}
		if (period < -1)
		{
			throw new ArgumentOutOfRangeException("period");
		}
		if (disposed)
		{
			throw new ObjectDisposedException(null, Environment.GetResourceString("Cannot access a disposed object."));
		}
		due_time_ms = dueTime;
		period_ms = period;
		long new_next_run;
		if (dueTime == 0L)
		{
			new_next_run = 0L;
		}
		else if (dueTime < 0)
		{
			new_next_run = long.MaxValue;
			if (first)
			{
				next_run = new_next_run;
				return true;
			}
		}
		else
		{
			new_next_run = dueTime * 10000 + GetTimeMonotonic();
		}
		scheduler.Change(this, new_next_run);
		return true;
	}

	/// <summary>Releases all resources used by the current instance of <see cref="T:System.Threading.Timer" /> and signals when the timer has been disposed of.</summary>
	/// <returns>true if the function succeeds; otherwise, false.</returns>
	/// <param name="notifyObject">The <see cref="T:System.Threading.WaitHandle" /> to be signaled when the Timer has been disposed of. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="notifyObject" /> parameter is null. </exception>
	/// <filterpriority>2</filterpriority>
	public bool Dispose(WaitHandle notifyObject)
	{
		if (notifyObject == null)
		{
			throw new ArgumentNullException("notifyObject");
		}
		Dispose();
		NativeEventCalls.SetEvent(notifyObject.SafeWaitHandle);
		return true;
	}

	internal void KeepRootedWhileScheduled()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern long GetTimeMonotonic();
}
