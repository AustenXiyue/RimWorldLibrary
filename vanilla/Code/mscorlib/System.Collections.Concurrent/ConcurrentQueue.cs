using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Collections.Concurrent;

/// <summary>Represents a thread-safe first in-first out (FIFO) collection.</summary>
/// <typeparam name="T">The type of the elements contained in the queue.</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(IProducerConsumerCollectionDebugView<>))]
public class ConcurrentQueue<T> : IProducerConsumerCollection<T>, IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
{
	[DebuggerDisplay("Capacity = {Capacity}")]
	private sealed class Segment
	{
		[StructLayout(LayoutKind.Auto)]
		[DebuggerDisplay("Item = {Item}, SequenceNumber = {SequenceNumber}")]
		internal struct Slot
		{
			public T Item;

			public int SequenceNumber;
		}

		internal readonly Slot[] _slots;

		internal readonly int _slotsMask;

		internal PaddedHeadAndTail _headAndTail;

		internal bool _preservedForObservation;

		internal bool _frozenForEnqueues;

		internal Segment _nextSegment;

		internal int Capacity => _slots.Length;

		internal int FreezeOffset => _slots.Length * 2;

		public Segment(int boundedLength)
		{
			_slots = new Slot[boundedLength];
			_slotsMask = boundedLength - 1;
			for (int i = 0; i < _slots.Length; i++)
			{
				_slots[i].SequenceNumber = i;
			}
		}

		internal void EnsureFrozenForEnqueues()
		{
			if (_frozenForEnqueues)
			{
				return;
			}
			_frozenForEnqueues = true;
			SpinWait spinWait = default(SpinWait);
			while (true)
			{
				int num = Volatile.Read(ref _headAndTail.Tail);
				if (Interlocked.CompareExchange(ref _headAndTail.Tail, num + FreezeOffset, num) != num)
				{
					spinWait.SpinOnce();
					continue;
				}
				break;
			}
		}

		public bool TryDequeue(out T item)
		{
			SpinWait spinWait = default(SpinWait);
			while (true)
			{
				int num = Volatile.Read(ref _headAndTail.Head);
				int num2 = num & _slotsMask;
				int num3 = Volatile.Read(ref _slots[num2].SequenceNumber) - (num + 1);
				if (num3 == 0)
				{
					if (Interlocked.CompareExchange(ref _headAndTail.Head, num + 1, num) == num)
					{
						item = _slots[num2].Item;
						if (!Volatile.Read(ref _preservedForObservation))
						{
							_slots[num2].Item = default(T);
							Volatile.Write(ref _slots[num2].SequenceNumber, num + _slots.Length);
						}
						return true;
					}
				}
				else if (num3 < 0)
				{
					bool frozenForEnqueues = _frozenForEnqueues;
					int num4 = Volatile.Read(ref _headAndTail.Tail);
					if (num4 - num <= 0 || (frozenForEnqueues && num4 - FreezeOffset - num <= 0))
					{
						break;
					}
				}
				spinWait.SpinOnce();
			}
			item = default(T);
			return false;
		}

		public bool TryPeek(out T result, bool resultUsed)
		{
			if (resultUsed)
			{
				_preservedForObservation = true;
				Interlocked.MemoryBarrier();
			}
			SpinWait spinWait = default(SpinWait);
			while (true)
			{
				int num = Volatile.Read(ref _headAndTail.Head);
				int num2 = num & _slotsMask;
				int num3 = Volatile.Read(ref _slots[num2].SequenceNumber) - (num + 1);
				if (num3 == 0)
				{
					result = (resultUsed ? _slots[num2].Item : default(T));
					return true;
				}
				if (num3 < 0)
				{
					bool frozenForEnqueues = _frozenForEnqueues;
					int num4 = Volatile.Read(ref _headAndTail.Tail);
					if (num4 - num <= 0 || (frozenForEnqueues && num4 - FreezeOffset - num <= 0))
					{
						break;
					}
				}
				spinWait.SpinOnce();
			}
			result = default(T);
			return false;
		}

		public bool TryEnqueue(T item)
		{
			SpinWait spinWait = default(SpinWait);
			while (true)
			{
				int num = Volatile.Read(ref _headAndTail.Tail);
				int num2 = num & _slotsMask;
				int num3 = Volatile.Read(ref _slots[num2].SequenceNumber) - num;
				if (num3 == 0)
				{
					if (Interlocked.CompareExchange(ref _headAndTail.Tail, num + 1, num) == num)
					{
						_slots[num2].Item = item;
						Volatile.Write(ref _slots[num2].SequenceNumber, num + 1);
						return true;
					}
				}
				else if (num3 < 0)
				{
					break;
				}
				spinWait.SpinOnce();
			}
			return false;
		}
	}

	private const int InitialSegmentLength = 32;

	private const int MaxSegmentLength = 1048576;

	private object _crossSegmentLock;

	private volatile Segment _tail;

	private volatile Segment _head;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized with the SyncRoot.</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized with the SyncRoot; otherwise, false. For <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />, this property always returns false.</returns>
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

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> is empty.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> is empty; otherwise, false.</returns>
	public bool IsEmpty
	{
		get
		{
			T result;
			return !TryPeek(out result, resultUsed: false);
		}
	}

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</returns>
	public int Count
	{
		get
		{
			SpinWait spinWait = default(SpinWait);
			Segment head;
			Segment tail;
			int headHead;
			int tailTail;
			while (true)
			{
				head = _head;
				tail = _tail;
				headHead = Volatile.Read(ref head._headAndTail.Head);
				int num = Volatile.Read(ref head._headAndTail.Tail);
				if (head == tail)
				{
					if (head == _head && head == _tail && headHead == Volatile.Read(ref head._headAndTail.Head) && num == Volatile.Read(ref head._headAndTail.Tail))
					{
						return GetCount(head, headHead, num);
					}
				}
				else
				{
					if (head._nextSegment != tail)
					{
						break;
					}
					int num2 = Volatile.Read(ref tail._headAndTail.Head);
					tailTail = Volatile.Read(ref tail._headAndTail.Tail);
					if (head == _head && tail == _tail && headHead == Volatile.Read(ref head._headAndTail.Head) && num == Volatile.Read(ref head._headAndTail.Tail) && num2 == Volatile.Read(ref tail._headAndTail.Head) && tailTail == Volatile.Read(ref tail._headAndTail.Tail))
					{
						return GetCount(head, headHead, num) + GetCount(tail, num2, tailTail);
					}
				}
				spinWait.SpinOnce();
			}
			SnapForObservation(out head, out headHead, out tail, out tailTail);
			return (int)GetCount(head, headHead, tail, tailTail);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> class.</summary>
	public ConcurrentQueue()
	{
		_crossSegmentLock = new object();
		_tail = (_head = new Segment(32));
	}

	private void InitializeFromCollection(IEnumerable<T> collection)
	{
		_crossSegmentLock = new object();
		int num = 32;
		if (collection is ICollection<T> { Count: var count } && count > num)
		{
			num = Math.Min(RoundUpToPowerOf2(count), 1048576);
		}
		_tail = (_head = new Segment(num));
		foreach (T item in collection)
		{
			Enqueue(item);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> class that contains elements copied from the specified collection</summary>
	/// <param name="collection">The collection whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="collection" /> argument is null.</exception>
	public ConcurrentQueue(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		InitializeFromCollection(collection);
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
			throw new ArgumentNullException("array");
		}
		ToArray().CopyTo(array, index);
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)this).GetEnumerator();
	}

	/// <summary>Attempts to add an object to the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />.</summary>
	/// <returns>true if the object was added successfully; otherwise, false.</returns>
	/// <param name="item">The object to add to the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />. The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
	bool IProducerConsumerCollection<T>.TryAdd(T item)
	{
		Enqueue(item);
		return true;
	}

	/// <summary>Attempts to remove and return an object from the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />.</summary>
	/// <returns>true if an element was removed and returned succesfully; otherwise, false.</returns>
	/// <param name="item">When this method returns, if the operation was successful, <paramref name="item" /> contains the object removed. If no object was available to be removed, the value is unspecified.</param>
	bool IProducerConsumerCollection<T>.TryTake(out T item)
	{
		return TryDequeue(out item);
	}

	/// <summary>Copies the elements stored in the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> to a new array.</summary>
	/// <returns>A new array containing a snapshot of elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</returns>
	public T[] ToArray()
	{
		SnapForObservation(out var head, out var headHead, out var tail, out var tailTail);
		T[] array = new T[GetCount(head, headHead, tail, tailTail)];
		using IEnumerator<T> enumerator = Enumerate(head, headHead, tail, tailTail);
		int num = 0;
		while (enumerator.MoveNext())
		{
			array[num++] = enumerator.Current;
		}
		return array;
	}

	private static int GetCount(Segment s, int head, int tail)
	{
		if (head != tail && head != tail - s.FreezeOffset)
		{
			head &= s._slotsMask;
			tail &= s._slotsMask;
			if (head >= tail)
			{
				return s._slots.Length - head + tail;
			}
			return tail - head;
		}
		return 0;
	}

	private static long GetCount(Segment head, int headHead, Segment tail, int tailTail)
	{
		long num = 0L;
		int num2 = ((head == tail) ? tailTail : Volatile.Read(ref head._headAndTail.Tail)) - head.FreezeOffset;
		if (headHead < num2)
		{
			headHead &= head._slotsMask;
			num2 &= head._slotsMask;
			num += ((headHead < num2) ? (num2 - headHead) : (head._slots.Length - headHead + num2));
		}
		if (head != tail)
		{
			for (Segment nextSegment = head._nextSegment; nextSegment != tail; nextSegment = nextSegment._nextSegment)
			{
				num += nextSegment._headAndTail.Tail - nextSegment.FreezeOffset;
			}
			num += tailTail - tail.FreezeOffset;
		}
		return num;
	}

	/// <summary>Copies the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> is equal to or greater than the length of the <paramref name="array" /> -or- The number of elements in the source <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(T[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "The index argument must be greater than or equal zero.");
		}
		SnapForObservation(out var head, out var headHead, out var tail, out var tailTail);
		long count = GetCount(head, headHead, tail, tailTail);
		if (index > array.Length - count)
		{
			throw new ArgumentException("The number of elements in the collection is greater than the available space from index to the end of the destination array.");
		}
		int num = index;
		using IEnumerator<T> enumerator = Enumerate(head, headHead, tail, tailTail);
		while (enumerator.MoveNext())
		{
			array[num++] = enumerator.Current;
		}
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</summary>
	/// <returns>An enumerator for the contents of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		SnapForObservation(out var head, out var headHead, out var tail, out var tailTail);
		return Enumerate(head, headHead, tail, tailTail);
	}

	private void SnapForObservation(out Segment head, out int headHead, out Segment tail, out int tailTail)
	{
		lock (_crossSegmentLock)
		{
			head = _head;
			tail = _tail;
			Segment segment = head;
			while (true)
			{
				segment._preservedForObservation = true;
				if (segment == tail)
				{
					break;
				}
				segment = segment._nextSegment;
			}
			tail.EnsureFrozenForEnqueues();
			headHead = Volatile.Read(ref head._headAndTail.Head);
			tailTail = Volatile.Read(ref tail._headAndTail.Tail);
		}
	}

	private T GetItemWhenAvailable(Segment segment, int i)
	{
		int num = (i + 1) & segment._slotsMask;
		if ((segment._slots[i].SequenceNumber & segment._slotsMask) != num)
		{
			SpinWait spinWait = default(SpinWait);
			while ((Volatile.Read(ref segment._slots[i].SequenceNumber) & segment._slotsMask) != num)
			{
				spinWait.SpinOnce();
			}
		}
		return segment._slots[i].Item;
	}

	private IEnumerator<T> Enumerate(Segment head, int headHead, Segment tail, int tailTail)
	{
		int headTail = ((head == tail) ? tailTail : Volatile.Read(ref head._headAndTail.Tail)) - head.FreezeOffset;
		if (headHead < headTail)
		{
			headHead &= head._slotsMask;
			headTail &= head._slotsMask;
			if (headHead < headTail)
			{
				for (int i = headHead; i < headTail; i++)
				{
					yield return GetItemWhenAvailable(head, i);
				}
			}
			else
			{
				for (int j = headHead; j < head._slots.Length; j++)
				{
					yield return GetItemWhenAvailable(head, j);
				}
				for (int k = 0; k < headTail; k++)
				{
					yield return GetItemWhenAvailable(head, k);
				}
			}
		}
		if (head == tail)
		{
			yield break;
		}
		for (Segment s = head._nextSegment; s != tail; s = s._nextSegment)
		{
			int sTail = s._headAndTail.Tail - s.FreezeOffset;
			for (int l = 0; l < sTail; l++)
			{
				yield return GetItemWhenAvailable(s, l);
			}
		}
		tailTail -= tail.FreezeOffset;
		for (int m = 0; m < tailTail; m++)
		{
			yield return GetItemWhenAvailable(tail, m);
		}
	}

	private static int RoundUpToPowerOf2(int i)
	{
		i--;
		i |= i >> 1;
		i |= i >> 2;
		i |= i >> 4;
		i |= i >> 8;
		i |= i >> 16;
		return i + 1;
	}

	/// <summary>Adds an object to the end of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</summary>
	/// <param name="item">The object to add to the end of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />. The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
	public void Enqueue(T item)
	{
		if (!_tail.TryEnqueue(item))
		{
			EnqueueSlow(item);
		}
	}

	private void EnqueueSlow(T item)
	{
		while (true)
		{
			Segment tail = _tail;
			if (tail.TryEnqueue(item))
			{
				break;
			}
			lock (_crossSegmentLock)
			{
				if (tail == _tail)
				{
					tail.EnsureFrozenForEnqueues();
					_tail = (tail._nextSegment = new Segment(tail._preservedForObservation ? 32 : Math.Min(tail.Capacity * 2, 1048576)));
				}
			}
		}
	}

	/// <summary>Tries to remove and return the object at the beginning of the concurrent queue.</summary>
	/// <returns>true if an element was removed and returned from the beginning of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> successfully; otherwise, false.</returns>
	/// <param name="result">When this method returns, if the operation was successful, <paramref name="result" /> contains the object removed. If no object was available to be removed, the value is unspecified.</param>
	public bool TryDequeue(out T result)
	{
		if (!_head.TryDequeue(out result))
		{
			return TryDequeueSlow(out result);
		}
		return true;
	}

	private bool TryDequeueSlow(out T item)
	{
		while (true)
		{
			Segment head = _head;
			if (head.TryDequeue(out item))
			{
				return true;
			}
			if (head._nextSegment == null)
			{
				item = default(T);
				return false;
			}
			if (head.TryDequeue(out item))
			{
				break;
			}
			lock (_crossSegmentLock)
			{
				if (head == _head)
				{
					_head = head._nextSegment;
				}
			}
		}
		return true;
	}

	/// <summary>Tries to return an object from the beginning of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> without removing it.</summary>
	/// <returns>true if an object was returned successfully; otherwise, false.</returns>
	/// <param name="result">When this method returns, <paramref name="result" /> contains an object from the beginning of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> or an unspecified value if the operation failed.</param>
	public bool TryPeek(out T result)
	{
		return TryPeek(out result, resultUsed: true);
	}

	private bool TryPeek(out T result, bool resultUsed)
	{
		Segment segment = _head;
		while (true)
		{
			Segment segment2 = Volatile.Read(ref segment._nextSegment);
			if (segment.TryPeek(out result, resultUsed))
			{
				return true;
			}
			if (segment2 != null)
			{
				segment = segment2;
			}
			else if (Volatile.Read(ref segment._nextSegment) == null)
			{
				break;
			}
		}
		result = default(T);
		return false;
	}

	public void Clear()
	{
		lock (_crossSegmentLock)
		{
			_tail.EnsureFrozenForEnqueues();
			_tail = (_head = new Segment(32));
		}
	}
}
