using System.Collections.Generic;

namespace System.Windows.Threading;

internal class PriorityQueue<T>
{
	private SortedList<int, PriorityChain<T>> _priorityChains;

	private Stack<PriorityChain<T>> _cacheReusableChains;

	private PriorityItem<T> _head;

	private PriorityItem<T> _tail;

	private int _count;

	public DispatcherPriority MaxPriority
	{
		get
		{
			int count = _priorityChains.Count;
			if (count > 0)
			{
				return (DispatcherPriority)_priorityChains.Keys[count - 1];
			}
			return DispatcherPriority.Invalid;
		}
	}

	public PriorityQueue()
	{
		_priorityChains = new SortedList<int, PriorityChain<T>>();
		_cacheReusableChains = new Stack<PriorityChain<T>>(10);
		_head = (_tail = null);
		_count = 0;
	}

	public PriorityItem<T> Enqueue(DispatcherPriority priority, T data)
	{
		PriorityChain<T> chain = GetChain(priority);
		PriorityItem<T> priorityItem = new PriorityItem<T>(data);
		InsertItemInSequentialChain(priorityItem, _tail);
		InsertItemInPriorityChain(priorityItem, chain, chain.Tail);
		return priorityItem;
	}

	public T Dequeue()
	{
		int count = _priorityChains.Count;
		if (count > 0)
		{
			PriorityItem<T> head = _priorityChains.Values[count - 1].Head;
			RemoveItem(head);
			return head.Data;
		}
		throw new InvalidOperationException();
	}

	public T Peek()
	{
		T result = default(T);
		int count = _priorityChains.Count;
		if (count > 0)
		{
			return _priorityChains.Values[count - 1].Head.Data;
		}
		return result;
	}

	public void RemoveItem(PriorityItem<T> item)
	{
		_ = item.Chain;
		RemoveItemFromPriorityChain(item);
		RemoveItemFromSequentialChain(item);
	}

	public void ChangeItemPriority(PriorityItem<T> item, DispatcherPriority priority)
	{
		RemoveItemFromPriorityChain(item);
		PriorityChain<T> chain = GetChain(priority);
		InsertItemInPriorityChain(item, chain);
	}

	private PriorityChain<T> GetChain(DispatcherPriority priority)
	{
		PriorityChain<T> value = null;
		int count = _priorityChains.Count;
		if (count > 0)
		{
			if (priority == (DispatcherPriority)_priorityChains.Keys[0])
			{
				value = _priorityChains.Values[0];
			}
			else if (priority == (DispatcherPriority)_priorityChains.Keys[count - 1])
			{
				value = _priorityChains.Values[count - 1];
			}
			else if ((int)priority > _priorityChains.Keys[0] && (int)priority < _priorityChains.Keys[count - 1])
			{
				_priorityChains.TryGetValue((int)priority, out value);
			}
		}
		if (value == null)
		{
			if (_cacheReusableChains.Count > 0)
			{
				value = _cacheReusableChains.Pop();
				value.Priority = priority;
			}
			else
			{
				value = new PriorityChain<T>(priority);
			}
			_priorityChains.Add((int)priority, value);
		}
		return value;
	}

	private void InsertItemInPriorityChain(PriorityItem<T> item, PriorityChain<T> chain)
	{
		if (chain.Head == null)
		{
			InsertItemInPriorityChain(item, chain, null);
			return;
		}
		PriorityItem<T> priorityItem = null;
		priorityItem = item.SequentialPrev;
		while (priorityItem != null && priorityItem.Chain != chain)
		{
			priorityItem = priorityItem.SequentialPrev;
		}
		InsertItemInPriorityChain(item, chain, priorityItem);
	}

	internal void InsertItemInPriorityChain(PriorityItem<T> item, PriorityChain<T> chain, PriorityItem<T> after)
	{
		item.Chain = chain;
		if (after == null)
		{
			if (chain.Head != null)
			{
				chain.Head.PriorityPrev = item;
				item.PriorityNext = chain.Head;
				chain.Head = item;
			}
			else
			{
				PriorityItem<T> head = (chain.Tail = item);
				chain.Head = head;
			}
		}
		else
		{
			item.PriorityPrev = after;
			if (after.PriorityNext != null)
			{
				item.PriorityNext = after.PriorityNext;
				after.PriorityNext.PriorityPrev = item;
				after.PriorityNext = item;
			}
			else
			{
				after.PriorityNext = item;
				chain.Tail = item;
			}
		}
		chain.Count++;
	}

	private void RemoveItemFromPriorityChain(PriorityItem<T> item)
	{
		if (item.PriorityPrev != null)
		{
			item.PriorityPrev.PriorityNext = item.PriorityNext;
		}
		else
		{
			item.Chain.Head = item.PriorityNext;
		}
		if (item.PriorityNext != null)
		{
			item.PriorityNext.PriorityPrev = item.PriorityPrev;
		}
		else
		{
			item.Chain.Tail = item.PriorityPrev;
		}
		PriorityItem<T> priorityPrev = (item.PriorityNext = null);
		item.PriorityPrev = priorityPrev;
		item.Chain.Count--;
		if (item.Chain.Count == 0)
		{
			if (item.Chain.Priority == (DispatcherPriority)_priorityChains.Keys[_priorityChains.Count - 1])
			{
				_priorityChains.RemoveAt(_priorityChains.Count - 1);
			}
			else
			{
				_priorityChains.Remove((int)item.Chain.Priority);
			}
			if (_cacheReusableChains.Count < 10)
			{
				item.Chain.Priority = DispatcherPriority.Invalid;
				_cacheReusableChains.Push(item.Chain);
			}
		}
		item.Chain = null;
	}

	internal void InsertItemInSequentialChain(PriorityItem<T> item, PriorityItem<T> after)
	{
		if (after == null)
		{
			if (_head != null)
			{
				_head.SequentialPrev = item;
				item.SequentialNext = _head;
				_head = item;
			}
			else
			{
				_head = (_tail = item);
			}
		}
		else
		{
			item.SequentialPrev = after;
			if (after.SequentialNext != null)
			{
				item.SequentialNext = after.SequentialNext;
				after.SequentialNext.SequentialPrev = item;
				after.SequentialNext = item;
			}
			else
			{
				after.SequentialNext = item;
				_tail = item;
			}
		}
		_count++;
	}

	private void RemoveItemFromSequentialChain(PriorityItem<T> item)
	{
		if (item.SequentialPrev != null)
		{
			item.SequentialPrev.SequentialNext = item.SequentialNext;
		}
		else
		{
			_head = item.SequentialNext;
		}
		if (item.SequentialNext != null)
		{
			item.SequentialNext.SequentialPrev = item.SequentialPrev;
		}
		else
		{
			_tail = item.SequentialPrev;
		}
		PriorityItem<T> sequentialPrev = (item.SequentialNext = null);
		item.SequentialPrev = sequentialPrev;
		_count--;
	}
}
