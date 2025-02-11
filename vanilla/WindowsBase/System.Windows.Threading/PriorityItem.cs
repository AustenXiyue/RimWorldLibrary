namespace System.Windows.Threading;

internal class PriorityItem<T>
{
	private T _data;

	private PriorityItem<T> _sequentialPrev;

	private PriorityItem<T> _sequentialNext;

	private PriorityChain<T> _chain;

	private PriorityItem<T> _priorityPrev;

	private PriorityItem<T> _priorityNext;

	public T Data => _data;

	public bool IsQueued => _chain != null;

	internal PriorityItem<T> SequentialPrev
	{
		get
		{
			return _sequentialPrev;
		}
		set
		{
			_sequentialPrev = value;
		}
	}

	internal PriorityItem<T> SequentialNext
	{
		get
		{
			return _sequentialNext;
		}
		set
		{
			_sequentialNext = value;
		}
	}

	internal PriorityChain<T> Chain
	{
		get
		{
			return _chain;
		}
		set
		{
			_chain = value;
		}
	}

	internal PriorityItem<T> PriorityPrev
	{
		get
		{
			return _priorityPrev;
		}
		set
		{
			_priorityPrev = value;
		}
	}

	internal PriorityItem<T> PriorityNext
	{
		get
		{
			return _priorityNext;
		}
		set
		{
			_priorityNext = value;
		}
	}

	public PriorityItem(T data)
	{
		_data = data;
	}
}
