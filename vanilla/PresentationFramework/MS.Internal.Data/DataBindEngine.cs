using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace MS.Internal.Data;

internal class DataBindEngine : DispatcherObject
{
	private class Task
	{
		public enum Status
		{
			Pending,
			Running,
			Completed,
			Retry,
			Cancelled
		}

		public IDataBindEngineClient client;

		public TaskOps op;

		public Status status;

		public Task Next;

		public Task PreviousForClient;

		public Task(IDataBindEngineClient c, TaskOps o, Task previousForClient)
		{
			client = c;
			op = o;
			PreviousForClient = previousForClient;
			status = Status.Pending;
		}

		public void Run(bool lastChance)
		{
			this.status = Status.Running;
			Status status = Status.Completed;
			switch (op)
			{
			case TaskOps.TransferValue:
				client.TransferValue();
				break;
			case TaskOps.UpdateValue:
				client.UpdateValue();
				break;
			case TaskOps.RaiseTargetUpdatedEvent:
				client.OnTargetUpdated();
				break;
			case TaskOps.AttachToContext:
				if (!client.AttachToContext(lastChance) && !lastChance)
				{
					status = Status.Retry;
				}
				break;
			case TaskOps.VerifySourceReference:
				client.VerifySourceReference(lastChance);
				break;
			}
			this.status = status;
		}
	}

	private readonly struct ValueConverterTableKey : IEquatable<ValueConverterTableKey>
	{
		private readonly Type _sourceType;

		private readonly Type _targetType;

		private readonly bool _targetToSource;

		public ValueConverterTableKey(Type sourceType, Type targetType, bool targetToSource)
		{
			_sourceType = sourceType;
			_targetType = targetType;
			_targetToSource = targetToSource;
		}

		public override int GetHashCode()
		{
			return _sourceType.GetHashCode() + _targetType.GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (o is ValueConverterTableKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public bool Equals(ValueConverterTableKey other)
		{
			if (_sourceType == other._sourceType && _targetType == other._targetType)
			{
				return _targetToSource == other._targetToSource;
			}
			return false;
		}
	}

	private sealed class DataBindEngineShutDownListener : ShutDownListener
	{
		public DataBindEngineShutDownListener(DataBindEngine target)
			: base(target)
		{
		}

		internal override void OnShutDown(object target, object sender, EventArgs e)
		{
			((DataBindEngine)target).OnShutDown();
		}
	}

	private HybridDictionary _mostRecentTask;

	private Task _head;

	private Task _tail;

	private UIElement _layoutElement;

	private ViewManager _viewManager = new ViewManager();

	private CommitManager _commitManager = new CommitManager();

	private Dictionary<ValueConverterTableKey, IValueConverter> _valueConverterTable = new Dictionary<ValueConverterTableKey, IValueConverter>();

	private PathParser _pathParser = new PathParser();

	private IAsyncDataDispatcher _defaultAsyncDataDispatcher;

	private HybridDictionary _asyncDispatchers;

	private ValueConverterContext _valueConverterContext = new ValueConverterContext();

	private bool _cleanupEnabled = true;

	private ValueTable _valueTable = new ValueTable();

	private AccessorTable _accessorTable = new AccessorTable();

	private int _cleanupRequests;

	private CleanupHelper _cleanupHelper;

	private Queue<DataBindOperation> _crossThreadQueue = new Queue<DataBindOperation>();

	private readonly object _crossThreadQueueLock = new object();

	private int _crossThreadCost;

	private DispatcherOperation _crossThreadDispatcherOperation;

	internal const int CrossThreadThreshold = 50000;

	[ThreadStatic]
	private static DataBindEngine _currentEngine;

	internal PathParser PathParser => _pathParser;

	internal ValueConverterContext ValueConverterContext => _valueConverterContext;

	internal AccessorTable AccessorTable => _accessorTable;

	internal bool IsShutDown => _viewManager == null;

	internal bool CleanupEnabled
	{
		get
		{
			return _cleanupEnabled;
		}
		set
		{
			_cleanupEnabled = value;
			WeakEventManager.SetCleanupEnabled(value);
		}
	}

	internal IAsyncDataDispatcher AsyncDataDispatcher
	{
		get
		{
			if (_defaultAsyncDataDispatcher == null)
			{
				_defaultAsyncDataDispatcher = new DefaultAsyncDataDispatcher();
			}
			return _defaultAsyncDataDispatcher;
		}
	}

	internal static DataBindEngine CurrentDataBindEngine
	{
		get
		{
			if (_currentEngine == null)
			{
				_currentEngine = new DataBindEngine();
			}
			return _currentEngine;
		}
	}

	internal ViewManager ViewManager => _viewManager;

	internal CommitManager CommitManager
	{
		get
		{
			if (!_commitManager.IsEmpty)
			{
				ScheduleCleanup();
			}
			return _commitManager;
		}
	}

	private DataBindEngine()
	{
		new DataBindEngineShutDownListener(this);
		_head = new Task(null, TaskOps.TransferValue, null);
		_tail = _head;
		_mostRecentTask = new HybridDictionary();
		_cleanupHelper = new CleanupHelper(DoCleanup);
	}

	internal void AddTask(IDataBindEngineClient c, TaskOps op)
	{
		if (_mostRecentTask != null)
		{
			if (_head == _tail)
			{
				RequestRun();
			}
			Task previousForClient = (Task)_mostRecentTask[c];
			Task task = new Task(c, op, previousForClient);
			_tail.Next = task;
			_tail = task;
			_mostRecentTask[c] = task;
			if (op == TaskOps.AttachToContext && _layoutElement == null && (_layoutElement = c.TargetElement as UIElement) != null)
			{
				_layoutElement.LayoutUpdated += OnLayoutUpdated;
			}
		}
	}

	internal void CancelTask(IDataBindEngineClient c, TaskOps op)
	{
		if (_mostRecentTask == null)
		{
			return;
		}
		for (Task task = (Task)_mostRecentTask[c]; task != null; task = task.PreviousForClient)
		{
			if (task.op == op && task.status == Task.Status.Pending)
			{
				task.status = Task.Status.Cancelled;
				break;
			}
		}
	}

	internal void CancelTasks(IDataBindEngineClient c)
	{
		if (_mostRecentTask == null)
		{
			return;
		}
		for (Task task = (Task)_mostRecentTask[c]; task != null; task = task.PreviousForClient)
		{
			Invariant.Assert(task.client == c, "task list is corrupt");
			if (task.status == Task.Status.Pending)
			{
				task.status = Task.Status.Cancelled;
			}
		}
		_mostRecentTask.Remove(c);
	}

	internal object Run(object arg)
	{
		bool flag = (bool)arg;
		Task task = (flag ? null : new Task(null, TaskOps.TransferValue, null));
		Task task2 = task;
		if (_layoutElement != null)
		{
			_layoutElement.LayoutUpdated -= OnLayoutUpdated;
			_layoutElement = null;
		}
		if (IsShutDown)
		{
			return null;
		}
		Task task3 = null;
		for (Task task4 = _head.Next; task4 != null; task4 = task3)
		{
			task4.PreviousForClient = null;
			if (task4.status == Task.Status.Pending)
			{
				task4.Run(flag);
				task3 = task4.Next;
				if (task4.status == Task.Status.Retry && !flag)
				{
					task4.status = Task.Status.Pending;
					task2.Next = task4;
					task2 = task4;
					task2.Next = null;
				}
			}
			else
			{
				task3 = task4.Next;
			}
		}
		_head.Next = null;
		_tail = _head;
		_mostRecentTask.Clear();
		if (!flag)
		{
			Task head = _head;
			_head = null;
			for (Task next = task.Next; next != null; next = next.Next)
			{
				AddTask(next.client, next.op);
			}
			_head = head;
		}
		return null;
	}

	internal ViewRecord GetViewRecord(object collection, CollectionViewSource key, Type collectionViewType, bool createView, Func<object, object> GetSourceItem)
	{
		if (IsShutDown)
		{
			return null;
		}
		ViewRecord viewRecord = _viewManager.GetViewRecord(collection, key, collectionViewType, createView, GetSourceItem);
		if (viewRecord != null && !viewRecord.IsInitialized)
		{
			ScheduleCleanup();
		}
		return viewRecord;
	}

	internal void RegisterCollectionSynchronizationCallback(IEnumerable collection, object context, CollectionSynchronizationCallback synchronizationCallback)
	{
		_viewManager.RegisterCollectionSynchronizationCallback(collection, context, synchronizationCallback);
	}

	internal IValueConverter GetDefaultValueConverter(Type sourceType, Type targetType, bool targetToSource)
	{
		ValueConverterTableKey key = new ValueConverterTableKey(sourceType, targetType, targetToSource);
		if (!_valueConverterTable.TryGetValue(key, out var value))
		{
			value = DefaultValueConverter.Create(sourceType, targetType, targetToSource, this);
			if (value != null)
			{
				_valueConverterTable.Add(key, value);
			}
		}
		return value;
	}

	internal void AddAsyncRequest(DependencyObject target, AsyncDataRequest request)
	{
		if (target != null)
		{
			IAsyncDataDispatcher asyncDataDispatcher = AsyncDataDispatcher;
			if (_asyncDispatchers == null)
			{
				_asyncDispatchers = new HybridDictionary(1);
			}
			_asyncDispatchers[asyncDataDispatcher] = null;
			asyncDataDispatcher.AddRequest(request);
		}
	}

	internal object GetValue(object item, PropertyDescriptor pd, bool indexerIsNext)
	{
		return _valueTable.GetValue(item, pd, indexerIsNext);
	}

	internal void RegisterForCacheChanges(object item, object descriptor)
	{
		PropertyDescriptor propertyDescriptor = descriptor as PropertyDescriptor;
		if (item != null && propertyDescriptor != null && ValueTable.ShouldCache(item, propertyDescriptor))
		{
			_valueTable.RegisterForChanges(item, propertyDescriptor, this);
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

	private bool DoCleanup(bool forceCleanup)
	{
		if (CleanupEnabled || forceCleanup)
		{
			return DoCleanup();
		}
		return false;
	}

	internal bool Cleanup()
	{
		if (!BaseAppContextSwitches.EnableCleanupSchedulingImprovements)
		{
			return DoCleanup();
		}
		return _cleanupHelper.DoCleanup(forceCleanup: true);
	}

	private bool DoCleanup()
	{
		bool flag = false;
		if (!IsShutDown)
		{
			flag = _viewManager.Purge() || flag;
			flag = WeakEventManager.Cleanup() || flag;
			flag = _valueTable.Purge() || flag;
			flag = _commitManager.Purge() || flag;
		}
		return flag;
	}

	internal DataBindOperation Marshal(DispatcherOperationCallback method, object arg, int cost = 1)
	{
		DataBindOperation dataBindOperation = new DataBindOperation(method, arg, cost);
		lock (_crossThreadQueueLock)
		{
			_crossThreadQueue.Enqueue(dataBindOperation);
			_crossThreadCost += cost;
			if (_crossThreadDispatcherOperation == null)
			{
				_crossThreadDispatcherOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(ProcessCrossThreadRequests));
			}
		}
		return dataBindOperation;
	}

	internal void ChangeCost(DataBindOperation op, int delta)
	{
		lock (_crossThreadQueueLock)
		{
			op.Cost += delta;
			_crossThreadCost += delta;
		}
	}

	private void ProcessCrossThreadRequests()
	{
		if (IsShutDown)
		{
			return;
		}
		try
		{
			long ticks = DateTime.Now.Ticks;
			do
			{
				DataBindOperation dataBindOperation;
				lock (_crossThreadQueueLock)
				{
					if (_crossThreadQueue.Count > 0)
					{
						dataBindOperation = _crossThreadQueue.Dequeue();
						_crossThreadCost -= dataBindOperation.Cost;
					}
					else
					{
						dataBindOperation = null;
					}
				}
				if (dataBindOperation == null)
				{
					break;
				}
				dataBindOperation.Invoke();
			}
			while (DateTime.Now.Ticks - ticks <= 50000);
		}
		finally
		{
			lock (_crossThreadQueueLock)
			{
				if (_crossThreadQueue.Count > 0)
				{
					_crossThreadDispatcherOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(ProcessCrossThreadRequests));
				}
				else
				{
					_crossThreadDispatcherOperation = null;
					_crossThreadCost = 0;
				}
			}
		}
	}

	private void RequestRun()
	{
		base.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new DispatcherOperationCallback(Run), false);
		base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(Run), true);
	}

	private object CleanupOperation(object arg)
	{
		Interlocked.Exchange(ref _cleanupRequests, 0);
		if (!_cleanupEnabled)
		{
			return null;
		}
		Cleanup();
		return null;
	}

	private void OnShutDown()
	{
		_viewManager = null;
		_commitManager = null;
		_valueConverterTable = null;
		_mostRecentTask = null;
		_head = (_tail = null);
		_crossThreadQueue.Clear();
		HybridDictionary hybridDictionary = Interlocked.Exchange(ref _asyncDispatchers, null);
		if (hybridDictionary != null)
		{
			foreach (object key in hybridDictionary.Keys)
			{
				if (key is IAsyncDataDispatcher asyncDataDispatcher)
				{
					asyncDataDispatcher.CancelAllRequests();
				}
			}
		}
		_defaultAsyncDataDispatcher = null;
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		Run(false);
	}
}
