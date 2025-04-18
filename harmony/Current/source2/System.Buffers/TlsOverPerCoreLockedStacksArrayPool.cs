using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Buffers;

internal sealed class TlsOverPerCoreLockedStacksArrayPool<T> : ArrayPool<T>
{
	private sealed class PerCoreLockedStacks
	{
		private static readonly int s_lockedStackCount = Math.Min(Environment.ProcessorCount, 64);

		private readonly LockedStack[] _perCoreStacks;

		public PerCoreLockedStacks()
		{
			LockedStack[] array = new LockedStack[s_lockedStackCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new LockedStack();
			}
			_perCoreStacks = array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryPush(T[] array)
		{
			LockedStack[] perCoreStacks = _perCoreStacks;
			int num = (int)((uint)EnvironmentEx.CurrentManagedThreadId % (uint)s_lockedStackCount);
			for (int i = 0; i < perCoreStacks.Length; i++)
			{
				if (perCoreStacks[num].TryPush(array))
				{
					return true;
				}
				if (++num == perCoreStacks.Length)
				{
					num = 0;
				}
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[]? TryPop()
		{
			LockedStack[] perCoreStacks = _perCoreStacks;
			int num = (int)((uint)EnvironmentEx.CurrentManagedThreadId % (uint)s_lockedStackCount);
			for (int i = 0; i < perCoreStacks.Length; i++)
			{
				T[] result;
				if ((result = perCoreStacks[num].TryPop()) != null)
				{
					return result;
				}
				if (++num == perCoreStacks.Length)
				{
					num = 0;
				}
			}
			return null;
		}

		public void Trim(int currentMilliseconds, int id, Utilities.MemoryPressure pressure, int bucketSize)
		{
			LockedStack[] perCoreStacks = _perCoreStacks;
			for (int i = 0; i < perCoreStacks.Length; i++)
			{
				perCoreStacks[i].Trim(currentMilliseconds, id, pressure, bucketSize);
			}
		}
	}

	private sealed class LockedStack
	{
		private readonly T[]?[] _arrays = new T[8][];

		private int _count;

		private int _millisecondsTimestamp;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryPush(T[] array)
		{
			bool result = false;
			Monitor.Enter(this);
			T[][] arrays = _arrays;
			int count = _count;
			if ((uint)count < (uint)arrays.Length)
			{
				if (count == 0)
				{
					_millisecondsTimestamp = 0;
				}
				arrays[count] = array;
				_count = count + 1;
				result = true;
			}
			Monitor.Exit(this);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[]? TryPop()
		{
			T[] result = null;
			Monitor.Enter(this);
			T[][] arrays = _arrays;
			int num = _count - 1;
			if ((uint)num < (uint)arrays.Length)
			{
				result = arrays[num];
				arrays[num] = null;
				_count = num;
			}
			Monitor.Exit(this);
			return result;
		}

		public void Trim(int currentMilliseconds, int id, Utilities.MemoryPressure pressure, int bucketSize)
		{
			if (_count == 0)
			{
				return;
			}
			int num = ((pressure == Utilities.MemoryPressure.High) ? 10000 : 60000);
			lock (this)
			{
				if (_count == 0)
				{
					return;
				}
				if (_millisecondsTimestamp == 0)
				{
					_millisecondsTimestamp = currentMilliseconds;
				}
				else
				{
					if (currentMilliseconds - _millisecondsTimestamp <= num)
					{
						return;
					}
					int num2 = 1;
					switch (pressure)
					{
					case Utilities.MemoryPressure.High:
						num2 = 8;
						if (bucketSize > 16384)
						{
							num2++;
						}
						if (Unsafe.SizeOf<T>() > 16)
						{
							num2++;
						}
						if (Unsafe.SizeOf<T>() > 32)
						{
							num2++;
						}
						break;
					case Utilities.MemoryPressure.Medium:
						num2 = 2;
						break;
					}
					while (_count > 0 && num2-- > 0)
					{
						_ = _arrays[--_count];
						_arrays[_count] = null;
					}
					_millisecondsTimestamp = ((_count > 0) ? (_millisecondsTimestamp + num / 4) : 0);
				}
			}
		}
	}

	private struct ThreadLocalArray
	{
		public T[]? Array;

		public int MillisecondsTimeStamp;

		public ThreadLocalArray(T[] array)
		{
			Array = array;
			MillisecondsTimeStamp = 0;
		}
	}

	private const int NumBuckets = 27;

	private const int MaxPerCorePerArraySizeStacks = 64;

	private const int MaxBuffersPerArraySizePerCore = 8;

	[ThreadStatic]
	private static ThreadLocalArray[]? t_tlsBuckets;

	private readonly ConditionalWeakTable<ThreadLocalArray[], object?> _allTlsBuckets = new ConditionalWeakTable<ThreadLocalArray[], object>();

	private readonly PerCoreLockedStacks?[] _buckets = new PerCoreLockedStacks[27];

	private int _trimCallbackCreated;

	private int Id => GetHashCode();

	private PerCoreLockedStacks CreatePerCoreLockedStacks(int bucketIndex)
	{
		PerCoreLockedStacks perCoreLockedStacks = new PerCoreLockedStacks();
		return Interlocked.CompareExchange(ref _buckets[bucketIndex], perCoreLockedStacks, null) ?? perCoreLockedStacks;
	}

	public override T[] Rent(int minimumLength)
	{
		int num = Utilities.SelectBucketIndex(minimumLength);
		ThreadLocalArray[] array = t_tlsBuckets;
		if (array != null && (uint)num < (uint)array.Length)
		{
			T[] array2 = array[num].Array;
			if (array2 != null)
			{
				array[num].Array = null;
				return array2;
			}
		}
		PerCoreLockedStacks[] buckets = _buckets;
		if ((uint)num < (uint)buckets.Length)
		{
			PerCoreLockedStacks perCoreLockedStacks = buckets[num];
			if (perCoreLockedStacks != null)
			{
				T[] array2 = perCoreLockedStacks.TryPop();
				if (array2 != null)
				{
					return array2;
				}
			}
			minimumLength = Utilities.GetMaxSizeForBucket(num);
		}
		else
		{
			if (minimumLength == 0)
			{
				return ArrayEx.Empty<T>();
			}
			if (minimumLength < 0)
			{
				throw new ArgumentOutOfRangeException("minimumLength");
			}
		}
		return new T[minimumLength];
	}

	public override void Return(T[] array, bool clearArray = false)
	{
		if (array == null)
		{
			System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.array);
		}
		int num = Utilities.SelectBucketIndex(array.Length);
		ThreadLocalArray[] array2 = t_tlsBuckets ?? InitializeTlsBucketsAndTrimming();
		if ((uint)num < (uint)array2.Length)
		{
			if (clearArray)
			{
				Array.Clear(array, 0, array.Length);
			}
			if (array.Length != Utilities.GetMaxSizeForBucket(num))
			{
				throw new ArgumentException("Buffer not from this pool", "array");
			}
			ref ThreadLocalArray reference = ref array2[num];
			T[] array3 = reference.Array;
			reference = new ThreadLocalArray(array);
			if (array3 != null)
			{
				(_buckets[num] ?? CreatePerCoreLockedStacks(num)).TryPush(array3);
			}
		}
	}

	public bool Trim()
	{
		int tickCount = Environment.TickCount;
		Utilities.MemoryPressure memoryPressure = Utilities.GetMemoryPressure();
		PerCoreLockedStacks[] buckets = _buckets;
		for (int i = 0; i < buckets.Length; i++)
		{
			buckets[i]?.Trim(tickCount, Id, memoryPressure, Utilities.GetMaxSizeForBucket(i));
		}
		if (memoryPressure == Utilities.MemoryPressure.High)
		{
			foreach (KeyValuePair<ThreadLocalArray[], object> allTlsBucket in _allTlsBuckets)
			{
				Array.Clear(allTlsBucket.Key, 0, allTlsBucket.Key.Length);
			}
		}
		else
		{
			uint num = ((memoryPressure != Utilities.MemoryPressure.Medium) ? 30000u : 15000u);
			uint num2 = num;
			foreach (KeyValuePair<ThreadLocalArray[], object> allTlsBucket2 in _allTlsBuckets)
			{
				ThreadLocalArray[] key = allTlsBucket2.Key;
				for (int j = 0; j < key.Length; j++)
				{
					if (key[j].Array != null)
					{
						int millisecondsTimeStamp = key[j].MillisecondsTimeStamp;
						if (millisecondsTimeStamp == 0)
						{
							key[j].MillisecondsTimeStamp = tickCount;
						}
						else if (tickCount - millisecondsTimeStamp >= num2)
						{
							Interlocked.Exchange(ref key[j].Array, null);
						}
					}
				}
			}
		}
		if (!Environment.HasShutdownStarted)
		{
			return !AppDomain.CurrentDomain.IsFinalizingForUnload();
		}
		return false;
	}

	private ThreadLocalArray[] InitializeTlsBucketsAndTrimming()
	{
		ThreadLocalArray[] array = (t_tlsBuckets = new ThreadLocalArray[27]);
		_allTlsBuckets.Add(array, null);
		if (Interlocked.Exchange(ref _trimCallbackCreated, 1) == 0)
		{
			System.Gen2GcCallback.Register((object s) => ((TlsOverPerCoreLockedStacksArrayPool<T>)s).Trim(), this);
		}
		return array;
	}
}
