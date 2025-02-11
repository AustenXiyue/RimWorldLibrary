using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MS.Internal;

internal class WeakEventTable : DispatcherObject
{
	private sealed class WeakEventTableShutDownListener : ShutDownListener
	{
		public WeakEventTableShutDownListener(WeakEventTable target)
			: base(target)
		{
		}

		internal override void OnShutDown(object target, object sender, EventArgs e)
		{
			((WeakEventTable)target).OnShutDown();
		}
	}

	private struct EventKey
	{
		private WeakEventManager _manager;

		private object _source;

		private int _hashcode;

		internal object Source => ((WeakReference)_source).Target;

		internal WeakEventManager Manager => _manager;

		internal EventKey(WeakEventManager manager, object source, bool useWeakRef)
		{
			_manager = manager;
			_source = new WeakReference(source);
			_hashcode = manager.GetHashCode() + RuntimeHelpers.GetHashCode(source);
		}

		internal EventKey(WeakEventManager manager, object source)
		{
			_manager = manager;
			_source = source;
			_hashcode = manager.GetHashCode() + RuntimeHelpers.GetHashCode(source);
		}

		public override int GetHashCode()
		{
			return _hashcode;
		}

		public override bool Equals(object o)
		{
			if (o is EventKey eventKey)
			{
				if (_manager != eventKey._manager || _hashcode != eventKey._hashcode)
				{
					return false;
				}
				object obj = ((_source is WeakReference weakReference) ? weakReference.Target : _source);
				object obj2 = ((eventKey._source is WeakReference weakReference2) ? weakReference2.Target : eventKey._source);
				if (obj != null && obj2 != null)
				{
					return obj == obj2;
				}
				return _source == eventKey._source;
			}
			return false;
		}

		public static bool operator ==(EventKey key1, EventKey key2)
		{
			return key1.Equals(key2);
		}

		public static bool operator !=(EventKey key1, EventKey key2)
		{
			return !key1.Equals(key2);
		}
	}

	private struct EventNameKey
	{
		private Type _eventSourceType;

		private string _eventName;

		public EventNameKey(Type eventSourceType, string eventName)
		{
			_eventSourceType = eventSourceType;
			_eventName = eventName;
		}

		public override int GetHashCode()
		{
			return _eventSourceType.GetHashCode() + _eventName.GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (o is EventNameKey eventNameKey)
			{
				if (_eventSourceType == eventNameKey._eventSourceType)
				{
					return _eventName == eventNameKey._eventName;
				}
				return false;
			}
			return false;
		}

		public static bool operator ==(EventNameKey key1, EventNameKey key2)
		{
			return key1.Equals(key2);
		}

		public static bool operator !=(EventNameKey key1, EventNameKey key2)
		{
			return !key1.Equals(key2);
		}
	}

	private Hashtable _managerTable = new Hashtable();

	private Hashtable _dataTable = new Hashtable();

	private Hashtable _eventNameTable = new Hashtable();

	private ReaderWriterLockWrapper _lock = new ReaderWriterLockWrapper();

	private int _cleanupRequests;

	private bool _cleanupEnabled = true;

	private CleanupHelper _cleanupHelper;

	private bool _inPurge;

	private List<EventKey> _toRemove = new List<EventKey>();

	[ThreadStatic]
	private static WeakEventTable _currentTable;

	internal static WeakEventTable CurrentWeakEventTable
	{
		get
		{
			if (_currentTable == null)
			{
				_currentTable = new WeakEventTable();
			}
			return _currentTable;
		}
	}

	internal IDisposable ReadLock => _lock.ReadLock;

	internal IDisposable WriteLock => _lock.WriteLock;

	internal WeakEventManager this[Type managerType]
	{
		get
		{
			return (WeakEventManager)_managerTable[managerType];
		}
		set
		{
			_managerTable[managerType] = value;
		}
	}

	internal WeakEventManager this[Type eventSourceType, string eventName]
	{
		get
		{
			EventNameKey eventNameKey = new EventNameKey(eventSourceType, eventName);
			return (WeakEventManager)_eventNameTable[eventNameKey];
		}
		set
		{
			EventNameKey eventNameKey = new EventNameKey(eventSourceType, eventName);
			_eventNameTable[eventNameKey] = value;
		}
	}

	internal object this[WeakEventManager manager, object source]
	{
		get
		{
			EventKey eventKey = new EventKey(manager, source);
			return _dataTable[eventKey];
		}
		set
		{
			EventKey eventKey = new EventKey(manager, source, useWeakRef: true);
			_dataTable[eventKey] = value;
		}
	}

	internal bool IsCleanupEnabled
	{
		get
		{
			return _cleanupEnabled;
		}
		set
		{
			_cleanupEnabled = value;
		}
	}

	private WeakEventTable()
	{
		new WeakEventTableShutDownListener(this);
		_cleanupHelper = new CleanupHelper(DoCleanup);
	}

	internal void Remove(WeakEventManager manager, object source)
	{
		EventKey eventKey = new EventKey(manager, source);
		if (!_inPurge)
		{
			_dataTable.Remove(eventKey);
		}
		else
		{
			_toRemove.Add(eventKey);
		}
	}

	internal void ScheduleCleanup()
	{
		if (!BaseAppContextSwitches.EnableCleanupSchedulingImprovements)
		{
			if (Interlocked.Increment(ref _cleanupRequests) == 1)
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new DispatcherOperationCallback(CleanupOperation), null);
			}
		}
		else
		{
			_cleanupHelper.ScheduleCleanup();
		}
	}

	internal static bool Cleanup()
	{
		if (!BaseAppContextSwitches.EnableCleanupSchedulingImprovements)
		{
			return CurrentWeakEventTable.Purge(purgeAll: false);
		}
		return CurrentWeakEventTable._cleanupHelper.DoCleanup(forceCleanup: true);
	}

	private bool DoCleanup(bool forceCleanup)
	{
		if (IsCleanupEnabled || forceCleanup)
		{
			return Purge(purgeAll: false);
		}
		return false;
	}

	private object CleanupOperation(object arg)
	{
		Interlocked.Exchange(ref _cleanupRequests, 0);
		if (IsCleanupEnabled)
		{
			Purge(purgeAll: false);
		}
		return null;
	}

	private bool Purge(bool purgeAll)
	{
		bool flag = false;
		using (WriteLock)
		{
			if (!BaseAppContextSwitches.EnableWeakEventMemoryImprovements)
			{
				ICollection keys = _dataTable.Keys;
				EventKey[] array = new EventKey[keys.Count];
				keys.CopyTo(array, 0);
				for (int num = array.Length - 1; num >= 0; num--)
				{
					object obj = _dataTable[array[num]];
					if (obj != null)
					{
						object source = array[num].Source;
						flag |= array[num].Manager.PurgeInternal(source, obj, purgeAll);
						if (!purgeAll && source == null)
						{
							_dataTable.Remove(array[num]);
						}
					}
				}
			}
			else
			{
				_inPurge = true;
				IDictionaryEnumerator enumerator = _dataTable.GetEnumerator();
				while (enumerator.MoveNext())
				{
					EventKey item = (EventKey)enumerator.Key;
					object source2 = item.Source;
					flag |= item.Manager.PurgeInternal(source2, enumerator.Value, purgeAll);
					if (!purgeAll && source2 == null)
					{
						_toRemove.Add(item);
					}
				}
				_inPurge = false;
			}
			if (purgeAll)
			{
				_managerTable.Clear();
				_dataTable.Clear();
			}
			else if (_toRemove.Count > 0)
			{
				foreach (EventKey item2 in _toRemove)
				{
					_dataTable.Remove(item2);
				}
				_toRemove.Clear();
				_toRemove.TrimExcess();
			}
		}
		return flag;
	}

	private void OnShutDown()
	{
		if (CheckAccess())
		{
			Purge(purgeAll: true);
			_currentTable = null;
			return;
		}
		bool flag = false;
		if (!BaseAppContextSwitches.DoNotInvokeInWeakEventTableShutdownListener && !base.Dispatcher.HasShutdownFinished)
		{
			try
			{
				base.Dispatcher.Invoke(OnShutDown, DispatcherPriority.Send, CancellationToken.None, TimeSpan.FromMilliseconds(300.0));
				flag = true;
			}
			catch (Exception ex) when (!CriticalExceptions.IsCriticalException(ex))
			{
			}
		}
		if (!flag)
		{
			Purge(purgeAll: true);
		}
	}
}
