using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace System.Collections.Concurrent;

/// <summary>Represents a thread-safe last in-first out (LIFO) collection.</summary>
/// <typeparam name="T">The type of the elements contained in the stack.</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(IProducerConsumerCollectionDebugView<>))]
public class ConcurrentStack<T> : IProducerConsumerCollection<T>, IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
{
	[Serializable]
	private class Node
	{
		internal readonly T _value;

		internal Node _next;

		internal Node(T value)
		{
			_value = value;
			_next = null;
		}
	}

	private volatile Node _head;

	private const int BACKOFF_MAX_YIELDS = 8;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> is empty.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> is empty; otherwise, false.</returns>
	public bool IsEmpty => _head == null;

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</returns>
	public int Count
	{
		get
		{
			int num = 0;
			for (Node node = _head; node != null; node = node._next)
			{
				num++;
			}
			return num;
		}
	}

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized with the SyncRoot.</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized with the SyncRoot; otherwise, false. For <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />, this property always returns false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />. This property is not supported.</summary>
	/// <returns>Returns null (Nothing in Visual Basic).</returns>
	/// <exception cref="T:System.NotSupportedException">The SyncRoot property is not supported</exception>
	object ICollection.SyncRoot
	{
		get
		{
			throw new NotSupportedException("The SyncRoot property may not be used for the synchronization of concurrent collections.");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> class.</summary>
	public ConcurrentStack()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> class that contains elements copied from the specified collection</summary>
	/// <param name="collection">The collection whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="collection" /> argument is null.</exception>
	public ConcurrentStack(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		InitializeFromCollection(collection);
	}

	private void InitializeFromCollection(IEnumerable<T> collection)
	{
		Node node = null;
		using (IEnumerator<T> enumerator = collection.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				node = new Node(enumerator.Current)
				{
					_next = node
				};
			}
		}
		_head = node;
	}

	/// <summary>Removes all objects from the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</summary>
	public void Clear()
	{
		_head = null;
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional. -or- <paramref name="array" /> does not have zero-based indexing. -or- <paramref name="index" /> is equal to or greater than the length of the <paramref name="array" /> -or- The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. -or- The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		((ICollection)ToList()).CopyTo(array, index);
	}

	/// <summary>Copies the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> is equal to or greater than the length of the <paramref name="array" /> -or- The number of elements in the source <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(T[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		ToList().CopyTo(array, index);
	}

	/// <summary>Inserts an object at the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</summary>
	/// <param name="item">The object to push onto the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />. The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
	public void Push(T item)
	{
		Node node = new Node(item);
		node._next = _head;
		if (Interlocked.CompareExchange(ref _head, node, node._next) != node._next)
		{
			PushCore(node, node);
		}
	}

	/// <summary>Inserts multiple objects at the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> atomically.</summary>
	/// <param name="items">The objects to push onto the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="items" /> is a null reference (Nothing in Visual Basic).</exception>
	public void PushRange(T[] items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		PushRange(items, 0, items.Length);
	}

	/// <summary>Inserts multiple objects at the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> atomically.</summary>
	/// <param name="items">The objects to push onto the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</param>
	/// <param name="startIndex">The zero-based offset in <paramref name="items" /> at which to begin inserting elements onto the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</param>
	/// <param name="count">The number of elements to be inserted onto the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="items" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="count" /> is negative. Or <paramref name="startIndex" /> is greater than or equal to the length of <paramref name="items" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> + <paramref name="count" /> is greater than the length of <paramref name="items" />.</exception>
	public void PushRange(T[] items, int startIndex, int count)
	{
		ValidatePushPopRangeInput(items, startIndex, count);
		if (count != 0)
		{
			Node node;
			Node node2 = (node = new Node(items[startIndex]));
			for (int i = startIndex + 1; i < startIndex + count; i++)
			{
				node2 = new Node(items[i])
				{
					_next = node2
				};
			}
			node._next = _head;
			if (Interlocked.CompareExchange(ref _head, node2, node._next) != node._next)
			{
				PushCore(node2, node);
			}
		}
	}

	private void PushCore(Node head, Node tail)
	{
		SpinWait spinWait = default(SpinWait);
		do
		{
			spinWait.SpinOnce();
			tail._next = _head;
		}
		while (Interlocked.CompareExchange(ref _head, head, tail._next) != tail._next);
		if (CDSCollectionETWBCLProvider.Log.IsEnabled())
		{
			CDSCollectionETWBCLProvider.Log.ConcurrentStack_FastPushFailed(spinWait.Count);
		}
	}

	private static void ValidatePushPopRangeInput(T[] items, int startIndex, int count)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "The count argument must be greater than or equal to zero.");
		}
		int num = items.Length;
		if (startIndex >= num || startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", "The startIndex argument must be greater than or equal to zero.");
		}
		if (num - count < startIndex)
		{
			throw new ArgumentException("The sum of the startIndex and count arguments must be less than or equal to the collection's Count.");
		}
	}

	/// <summary>Attempts to add an object to the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />.</summary>
	/// <returns>true if the object was added successfully; otherwise, false.</returns>
	/// <param name="item">The object to add to the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />. The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
	bool IProducerConsumerCollection<T>.TryAdd(T item)
	{
		Push(item);
		return true;
	}

	/// <summary>Attempts to return an object from the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> without removing it.</summary>
	/// <returns>true if and object was returned successfully; otherwise, false.</returns>
	/// <param name="result">When this method returns, <paramref name="result" /> contains an object from the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />or an unspecified value if the operation failed.</param>
	public bool TryPeek(out T result)
	{
		Node head = _head;
		if (head == null)
		{
			result = default(T);
			return false;
		}
		result = head._value;
		return true;
	}

	/// <summary>Attempts to pop and return the object at the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</summary>
	/// <returns>true if an element was removed and returned from the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> successfully; otherwise, false.</returns>
	/// <param name="result">When this method returns, if the operation was successful, <paramref name="result" /> contains the object removed. If no object was available to be removed, the value is unspecified.</param>
	public bool TryPop(out T result)
	{
		Node head = _head;
		if (head == null)
		{
			result = default(T);
			return false;
		}
		if (Interlocked.CompareExchange(ref _head, head._next, head) == head)
		{
			result = head._value;
			return true;
		}
		return TryPopCore(out result);
	}

	/// <summary>Attempts to pop and return multiple objects from the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> atomically.</summary>
	/// <returns>The number of objects successfully popped from the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> and inserted in <paramref name="items" />.</returns>
	/// <param name="items">The <see cref="T:System.Array" /> to which objects popped from the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> will be added.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="items" /> is a null argument (Nothing in Visual Basic).</exception>
	public int TryPopRange(T[] items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		return TryPopRange(items, 0, items.Length);
	}

	/// <summary>Attempts to pop and return multiple objects from the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> atomically.</summary>
	/// <returns>The number of objects successfully popped from the top of the stack and inserted in <paramref name="items" />.</returns>
	/// <param name="items">The <see cref="T:System.Array" /> to which objects popped from the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> will be added.</param>
	/// <param name="startIndex">The zero-based offset in <paramref name="items" /> at which to begin inserting elements from the top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</param>
	/// <param name="count">The number of elements to be popped from top of the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> and inserted into <paramref name="items" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="items" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="count" /> is negative. Or <paramref name="startIndex" /> is greater than or equal to the length of <paramref name="items" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> + <paramref name="count" /> is greater than the length of <paramref name="items" />.</exception>
	public int TryPopRange(T[] items, int startIndex, int count)
	{
		ValidatePushPopRangeInput(items, startIndex, count);
		if (count == 0)
		{
			return 0;
		}
		Node poppedHead;
		int num = TryPopCore(count, out poppedHead);
		if (num > 0)
		{
			CopyRemovedItems(poppedHead, items, startIndex, num);
		}
		return num;
	}

	private bool TryPopCore(out T result)
	{
		if (TryPopCore(1, out var poppedHead) == 1)
		{
			result = poppedHead._value;
			return true;
		}
		result = default(T);
		return false;
	}

	private int TryPopCore(int count, out Node poppedHead)
	{
		SpinWait spinWait = default(SpinWait);
		int num = 1;
		Random random = null;
		Node head;
		int i;
		while (true)
		{
			head = _head;
			if (head == null)
			{
				if (count == 1 && CDSCollectionETWBCLProvider.Log.IsEnabled())
				{
					CDSCollectionETWBCLProvider.Log.ConcurrentStack_FastPopFailed(spinWait.Count);
				}
				poppedHead = null;
				return 0;
			}
			Node node = head;
			for (i = 1; i < count; i++)
			{
				if (node._next == null)
				{
					break;
				}
				node = node._next;
			}
			if (Interlocked.CompareExchange(ref _head, node._next, head) == head)
			{
				break;
			}
			for (int j = 0; j < num; j++)
			{
				spinWait.SpinOnce();
			}
			if (spinWait.NextSpinWillYield)
			{
				if (random == null)
				{
					random = new Random();
				}
				num = random.Next(1, 8);
			}
			else
			{
				num *= 2;
			}
		}
		if (count == 1 && CDSCollectionETWBCLProvider.Log.IsEnabled())
		{
			CDSCollectionETWBCLProvider.Log.ConcurrentStack_FastPopFailed(spinWait.Count);
		}
		poppedHead = head;
		return i;
	}

	private static void CopyRemovedItems(Node head, T[] collection, int startIndex, int nodesCount)
	{
		Node node = head;
		for (int i = startIndex; i < startIndex + nodesCount; i++)
		{
			collection[i] = node._value;
			node = node._next;
		}
	}

	/// <summary>Attempts to remove and return an object from the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />.</summary>
	/// <returns>true if an element was removed and returned succesfully; otherwise, false.</returns>
	/// <param name="item">When this method returns, if the operation was successful, <paramref name="item" /> contains the object removed. If no object was available to be removed, the value is unspecified.</param>
	bool IProducerConsumerCollection<T>.TryTake(out T item)
	{
		return TryPop(out item);
	}

	/// <summary>Copies the items stored in the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" /> to a new array.</summary>
	/// <returns>A new array containing a snapshot of elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</returns>
	public T[] ToArray()
	{
		Node head = _head;
		if (head != null)
		{
			return ToList(head).ToArray();
		}
		return Array.Empty<T>();
	}

	private List<T> ToList()
	{
		return ToList(_head);
	}

	private List<T> ToList(Node curr)
	{
		List<T> list = new List<T>();
		while (curr != null)
		{
			list.Add(curr._value);
			curr = curr._next;
		}
		return list;
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</summary>
	/// <returns>An enumerator for the <see cref="T:System.Collections.Concurrent.ConcurrentStack`1" />.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		return GetEnumerator(_head);
	}

	private IEnumerator<T> GetEnumerator(Node head)
	{
		for (Node current = head; current != null; current = current._next)
		{
			yield return current._value;
		}
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)this).GetEnumerator();
	}
}
