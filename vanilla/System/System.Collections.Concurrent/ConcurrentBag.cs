using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace System.Collections.Concurrent;

/// <summary>Represents a thread-safe, unordered collection of objects.</summary>
/// <typeparam name="T">The type of the elements to be stored in the collection.</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(IProducerConsumerCollectionDebugView<>))]
public class ConcurrentBag<T> : IProducerConsumerCollection<T>, IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
{
	private sealed class WorkStealingQueue
	{
		private const int InitialSize = 32;

		private const int StartIndex = 0;

		private volatile int _headIndex;

		private volatile int _tailIndex;

		private volatile T[] _array = new T[32];

		private volatile int _mask = 31;

		private int _addTakeCount;

		private int _stealCount;

		internal volatile int _currentOp;

		internal bool _frozen;

		internal readonly WorkStealingQueue _nextQueue;

		internal readonly int _ownerThreadId;

		internal bool IsEmpty => _headIndex >= _tailIndex;

		internal int DangerousCount => _addTakeCount - _stealCount;

		internal WorkStealingQueue(WorkStealingQueue nextQueue)
		{
			_ownerThreadId = Environment.CurrentManagedThreadId;
			_nextQueue = nextQueue;
		}

		internal void LocalPush(T item)
		{
			bool lockTaken = false;
			try
			{
				Interlocked.Exchange(ref _currentOp, 1);
				int num = _tailIndex;
				if (num == int.MaxValue)
				{
					_currentOp = 0;
					lock (this)
					{
						_headIndex &= _mask;
						num = (_tailIndex &= _mask);
						_currentOp = 1;
					}
				}
				if (!_frozen && num < _headIndex + _mask)
				{
					_array[num & _mask] = item;
					_tailIndex = num + 1;
				}
				else
				{
					_currentOp = 0;
					Monitor.Enter(this, ref lockTaken);
					int headIndex = _headIndex;
					int num2 = _tailIndex - _headIndex;
					if (num2 >= _mask)
					{
						T[] array = new T[_array.Length << 1];
						int num3 = headIndex & _mask;
						if (num3 == 0)
						{
							Array.Copy(_array, 0, array, 0, _array.Length);
						}
						else
						{
							Array.Copy(_array, num3, array, 0, _array.Length - num3);
							Array.Copy(_array, 0, array, _array.Length - num3, num3);
						}
						_array = array;
						_headIndex = 0;
						num = (_tailIndex = num2);
						_mask = (_mask << 1) | 1;
					}
					_array[num & _mask] = item;
					_tailIndex = num + 1;
					_addTakeCount -= _stealCount;
					_stealCount = 0;
				}
				checked
				{
					_addTakeCount++;
				}
			}
			finally
			{
				_currentOp = 0;
				if (lockTaken)
				{
					Monitor.Exit(this);
				}
			}
		}

		internal void LocalClear()
		{
			lock (this)
			{
				if (_headIndex < _tailIndex)
				{
					_headIndex = (_tailIndex = 0);
					_addTakeCount = (_stealCount = 0);
					Array.Clear(_array, 0, _array.Length);
				}
			}
		}

		internal bool TryLocalPop(out T result)
		{
			int tailIndex = _tailIndex;
			if (_headIndex >= tailIndex)
			{
				result = default(T);
				return false;
			}
			bool lockTaken = false;
			try
			{
				_currentOp = 2;
				Interlocked.Exchange(ref _tailIndex, --tailIndex);
				if (!_frozen && _headIndex < tailIndex)
				{
					int num = tailIndex & _mask;
					result = _array[num];
					_array[num] = default(T);
					_addTakeCount--;
					return true;
				}
				_currentOp = 0;
				Monitor.Enter(this, ref lockTaken);
				if (_headIndex <= tailIndex)
				{
					int num2 = tailIndex & _mask;
					result = _array[num2];
					_array[num2] = default(T);
					_addTakeCount--;
					return true;
				}
				_tailIndex = tailIndex + 1;
				result = default(T);
				return false;
			}
			finally
			{
				_currentOp = 0;
				if (lockTaken)
				{
					Monitor.Exit(this);
				}
			}
		}

		internal bool TryLocalPeek(out T result)
		{
			int tailIndex = _tailIndex;
			if (_headIndex < tailIndex)
			{
				lock (this)
				{
					if (_headIndex < tailIndex)
					{
						result = _array[(tailIndex - 1) & _mask];
						return true;
					}
				}
			}
			result = default(T);
			return false;
		}

		internal bool TrySteal(out T result, bool take)
		{
			if (_headIndex < _tailIndex)
			{
				lock (this)
				{
					int headIndex = _headIndex;
					if (take)
					{
						Interlocked.Exchange(ref _headIndex, headIndex + 1);
						if (headIndex < _tailIndex)
						{
							int num = headIndex & _mask;
							result = _array[num];
							_array[num] = default(T);
							_stealCount++;
							return true;
						}
						_headIndex = headIndex;
					}
					else if (headIndex < _tailIndex)
					{
						result = _array[headIndex & _mask];
						return true;
					}
				}
			}
			result = default(T);
			return false;
		}

		internal int DangerousCopyTo(T[] array, int arrayIndex)
		{
			int headIndex = _headIndex;
			int dangerousCount = DangerousCount;
			for (int num = arrayIndex + dangerousCount - 1; num >= arrayIndex; num--)
			{
				array[num] = _array[headIndex++ & _mask];
			}
			return dangerousCount;
		}
	}

	internal enum Operation
	{
		None,
		Add,
		Take
	}

	[Serializable]
	private sealed class Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private readonly T[] _array;

		private T _current;

		private int _index;

		public T Current => _current;

		object IEnumerator.Current
		{
			get
			{
				if (_index == 0 || _index == _array.Length + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return Current;
			}
		}

		public Enumerator(T[] array)
		{
			_array = array;
		}

		public bool MoveNext()
		{
			if (_index < _array.Length)
			{
				_current = _array[_index++];
				return true;
			}
			_index = _array.Length + 1;
			return false;
		}

		public void Reset()
		{
			_index = 0;
			_current = default(T);
		}

		public void Dispose()
		{
		}
	}

	private ThreadLocal<WorkStealingQueue> _locals;

	private volatile WorkStealingQueue _workStealingQueues;

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</returns>
	public int Count
	{
		get
		{
			if (_workStealingQueues == null)
			{
				return 0;
			}
			bool lockTaken = false;
			try
			{
				FreezeBag(ref lockTaken);
				return DangerousCount;
			}
			finally
			{
				UnfreezeBag(lockTaken);
			}
		}
	}

	private int DangerousCount
	{
		get
		{
			int num = 0;
			for (WorkStealingQueue workStealingQueue = _workStealingQueues; workStealingQueue != null; workStealingQueue = workStealingQueue._nextQueue)
			{
				num = checked(num + workStealingQueue.DangerousCount);
			}
			return num;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" /> is empty.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" /> is empty; otherwise, false.</returns>
	public bool IsEmpty
	{
		get
		{
			WorkStealingQueue currentThreadWorkStealingQueue = GetCurrentThreadWorkStealingQueue(forceCreate: false);
			if (currentThreadWorkStealingQueue != null)
			{
				if (!currentThreadWorkStealingQueue.IsEmpty)
				{
					return false;
				}
				if (currentThreadWorkStealingQueue._nextQueue == null && currentThreadWorkStealingQueue == _workStealingQueues)
				{
					return true;
				}
			}
			bool lockTaken = false;
			try
			{
				FreezeBag(ref lockTaken);
				for (WorkStealingQueue workStealingQueue = _workStealingQueues; workStealingQueue != null; workStealingQueue = workStealingQueue._nextQueue)
				{
					if (!workStealingQueue.IsEmpty)
					{
						return false;
					}
				}
			}
			finally
			{
				UnfreezeBag(lockTaken);
			}
			return true;
		}
	}

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized with the SyncRoot.</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized with the SyncRoot; otherwise, false. For <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />, this property always returns false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />. This property is not supported.</summary>
	/// <returns>Returns null  (Nothing in Visual Basic).</returns>
	/// <exception cref="T:System.NotSupportedException">The SyncRoot property is not supported.</exception>
	object ICollection.SyncRoot
	{
		get
		{
			throw new NotSupportedException("The SyncRoot property may not be used for the synchronization of concurrent collections.");
		}
	}

	private object GlobalQueuesLock => _locals;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" /> class.</summary>
	public ConcurrentBag()
	{
		_locals = new ThreadLocal<WorkStealingQueue>();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" /> class that contains elements copied from the specified collection.</summary>
	/// <param name="collection">The collection whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> is a null reference (Nothing in Visual Basic).</exception>
	public ConcurrentBag(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection", "The collection argument is null.");
		}
		_locals = new ThreadLocal<WorkStealingQueue>();
		WorkStealingQueue currentThreadWorkStealingQueue = GetCurrentThreadWorkStealingQueue(forceCreate: true);
		foreach (T item in collection)
		{
			currentThreadWorkStealingQueue.LocalPush(item);
		}
	}

	/// <summary>Adds an object to the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</summary>
	/// <param name="item">The object to be added to the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />. The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
	public void Add(T item)
	{
		GetCurrentThreadWorkStealingQueue(forceCreate: true).LocalPush(item);
	}

	/// <summary>Attempts to add an object to the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</summary>
	/// <returns>Always returns true</returns>
	/// <param name="item">The object to be added to the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />. The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
	bool IProducerConsumerCollection<T>.TryAdd(T item)
	{
		Add(item);
		return true;
	}

	/// <summary>Attempts to remove and return an object from the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</summary>
	/// <returns>true if an object was removed successfully; otherwise, false.</returns>
	/// <param name="result">When this method returns, <paramref name="result" /> contains the object removed from the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" /> or the default value of <paramref name="T" /> if the bag is empty.</param>
	public bool TryTake(out T result)
	{
		WorkStealingQueue currentThreadWorkStealingQueue = GetCurrentThreadWorkStealingQueue(forceCreate: false);
		if (currentThreadWorkStealingQueue == null || !currentThreadWorkStealingQueue.TryLocalPop(out result))
		{
			return TrySteal(out result, take: true);
		}
		return true;
	}

	/// <summary>Attempts to return an object from the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" /> without removing it.</summary>
	/// <returns>true if and object was returned successfully; otherwise, false.</returns>
	/// <param name="result">When this method returns, <paramref name="result" /> contains an object from the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" /> or the default value of <paramref name="T" /> if the operation failed.</param>
	public bool TryPeek(out T result)
	{
		WorkStealingQueue currentThreadWorkStealingQueue = GetCurrentThreadWorkStealingQueue(forceCreate: false);
		if (currentThreadWorkStealingQueue == null || !currentThreadWorkStealingQueue.TryLocalPeek(out result))
		{
			return TrySteal(out result, take: false);
		}
		return true;
	}

	private WorkStealingQueue GetCurrentThreadWorkStealingQueue(bool forceCreate)
	{
		WorkStealingQueue workStealingQueue = _locals.Value;
		if (workStealingQueue == null)
		{
			if (!forceCreate)
			{
				return null;
			}
			workStealingQueue = CreateWorkStealingQueueForCurrentThread();
		}
		return workStealingQueue;
	}

	private WorkStealingQueue CreateWorkStealingQueueForCurrentThread()
	{
		lock (GlobalQueuesLock)
		{
			WorkStealingQueue workStealingQueues = _workStealingQueues;
			WorkStealingQueue workStealingQueue = ((workStealingQueues != null) ? GetUnownedWorkStealingQueue() : null);
			if (workStealingQueue == null)
			{
				workStealingQueue = (_workStealingQueues = new WorkStealingQueue(workStealingQueues));
			}
			_locals.Value = workStealingQueue;
			return workStealingQueue;
		}
	}

	private WorkStealingQueue GetUnownedWorkStealingQueue()
	{
		int currentManagedThreadId = Environment.CurrentManagedThreadId;
		for (WorkStealingQueue workStealingQueue = _workStealingQueues; workStealingQueue != null; workStealingQueue = workStealingQueue._nextQueue)
		{
			if (workStealingQueue._ownerThreadId == currentManagedThreadId)
			{
				return workStealingQueue;
			}
		}
		return null;
	}

	private bool TrySteal(out T result, bool take)
	{
		if (CDSCollectionETWBCLProvider.Log.IsEnabled())
		{
			if (take)
			{
				CDSCollectionETWBCLProvider.Log.ConcurrentBag_TryTakeSteals();
			}
			else
			{
				CDSCollectionETWBCLProvider.Log.ConcurrentBag_TryPeekSteals();
			}
		}
		WorkStealingQueue currentThreadWorkStealingQueue = GetCurrentThreadWorkStealingQueue(forceCreate: false);
		if (currentThreadWorkStealingQueue == null)
		{
			return TryStealFromTo(_workStealingQueues, null, out result, take);
		}
		if (!TryStealFromTo(currentThreadWorkStealingQueue._nextQueue, null, out result, take))
		{
			return TryStealFromTo(_workStealingQueues, currentThreadWorkStealingQueue, out result, take);
		}
		return true;
	}

	private bool TryStealFromTo(WorkStealingQueue startInclusive, WorkStealingQueue endExclusive, out T result, bool take)
	{
		for (WorkStealingQueue workStealingQueue = startInclusive; workStealingQueue != endExclusive; workStealingQueue = workStealingQueue._nextQueue)
		{
			if (workStealingQueue.TrySteal(out result, take))
			{
				return true;
			}
		}
		result = default(T);
		return false;
	}

	/// <summary>Copies the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> is equal to or greater than the length of the <paramref name="array" /> -or- the number of elements in the source <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(T[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", "The array argument is null.");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "The index argument must be greater than or equal zero.");
		}
		if (_workStealingQueues == null)
		{
			return;
		}
		bool lockTaken = false;
		try
		{
			FreezeBag(ref lockTaken);
			int dangerousCount = DangerousCount;
			if (index > array.Length - dangerousCount)
			{
				throw new ArgumentException("The number of elements in the collection is greater than the available space from index to the end of the destination array.", "index");
			}
			try
			{
				CopyFromEachQueueToArray(array, index);
			}
			catch (ArrayTypeMismatchException ex)
			{
				throw new InvalidCastException(ex.Message, ex);
			}
		}
		finally
		{
			UnfreezeBag(lockTaken);
		}
	}

	private int CopyFromEachQueueToArray(T[] array, int index)
	{
		int num = index;
		for (WorkStealingQueue workStealingQueue = _workStealingQueues; workStealingQueue != null; workStealingQueue = workStealingQueue._nextQueue)
		{
			num += workStealingQueue.DangerousCopyTo(array, num);
		}
		return num - index;
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or- <paramref name="array" /> does not have zero-based indexing. -or- <paramref name="index" /> is equal to or greater than the length of the <paramref name="array" /> -or- The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. -or- The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array is T[] array2)
		{
			CopyTo(array2, index);
			return;
		}
		if (array == null)
		{
			throw new ArgumentNullException("array", "The array argument is null.");
		}
		ToArray().CopyTo(array, index);
	}

	/// <summary>Copies the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" /> elements to a new array.</summary>
	/// <returns>A new array containing a snapshot of elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</returns>
	public T[] ToArray()
	{
		if (_workStealingQueues != null)
		{
			bool lockTaken = false;
			try
			{
				FreezeBag(ref lockTaken);
				int dangerousCount = DangerousCount;
				if (dangerousCount > 0)
				{
					T[] array = new T[dangerousCount];
					CopyFromEachQueueToArray(array, 0);
					return array;
				}
			}
			finally
			{
				UnfreezeBag(lockTaken);
			}
		}
		return Array.Empty<T>();
	}

	public void Clear()
	{
		if (_workStealingQueues == null)
		{
			return;
		}
		WorkStealingQueue currentThreadWorkStealingQueue = GetCurrentThreadWorkStealingQueue(forceCreate: false);
		if (currentThreadWorkStealingQueue != null)
		{
			currentThreadWorkStealingQueue.LocalClear();
			if (currentThreadWorkStealingQueue._nextQueue == null && currentThreadWorkStealingQueue == _workStealingQueues)
			{
				return;
			}
		}
		bool lockTaken = false;
		try
		{
			FreezeBag(ref lockTaken);
			for (WorkStealingQueue workStealingQueue = _workStealingQueues; workStealingQueue != null; workStealingQueue = workStealingQueue._nextQueue)
			{
				T result;
				while (workStealingQueue.TrySteal(out result, take: true))
				{
				}
			}
		}
		finally
		{
			UnfreezeBag(lockTaken);
		}
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</summary>
	/// <returns>An enumerator for the contents of the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		return new Enumerator(ToArray());
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</summary>
	/// <returns>An enumerator for the contents of the <see cref="T:System.Collections.Concurrent.ConcurrentBag`1" />.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void FreezeBag(ref bool lockTaken)
	{
		Monitor.Enter(GlobalQueuesLock, ref lockTaken);
		WorkStealingQueue workStealingQueues = _workStealingQueues;
		for (WorkStealingQueue workStealingQueue = workStealingQueues; workStealingQueue != null; workStealingQueue = workStealingQueue._nextQueue)
		{
			Monitor.Enter(workStealingQueue, ref workStealingQueue._frozen);
		}
		Interlocked.MemoryBarrier();
		for (WorkStealingQueue workStealingQueue2 = workStealingQueues; workStealingQueue2 != null; workStealingQueue2 = workStealingQueue2._nextQueue)
		{
			if (workStealingQueue2._currentOp != 0)
			{
				SpinWait spinWait = default(SpinWait);
				do
				{
					spinWait.SpinOnce();
				}
				while (workStealingQueue2._currentOp != 0);
			}
		}
	}

	private void UnfreezeBag(bool lockTaken)
	{
		if (!lockTaken)
		{
			return;
		}
		for (WorkStealingQueue workStealingQueue = _workStealingQueues; workStealingQueue != null; workStealingQueue = workStealingQueue._nextQueue)
		{
			if (workStealingQueue._frozen)
			{
				workStealingQueue._frozen = false;
				Monitor.Exit(workStealingQueue);
			}
		}
		Monitor.Exit(GlobalQueuesLock);
	}
}
