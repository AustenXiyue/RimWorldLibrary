namespace System.Windows.Threading;

internal class PriorityChain<T>
{
	private PriorityItem<T> _head;

	private PriorityItem<T> _tail;

	private DispatcherPriority _priority;

	private int _count;

	public DispatcherPriority Priority
	{
		get
		{
			return _priority;
		}
		set
		{
			_priority = value;
		}
	}

	public int Count
	{
		get
		{
			return _count;
		}
		set
		{
			_count = value;
		}
	}

	public PriorityItem<T> Head
	{
		get
		{
			return _head;
		}
		set
		{
			_head = value;
		}
	}

	public PriorityItem<T> Tail
	{
		get
		{
			return _tail;
		}
		set
		{
			_tail = value;
		}
	}

	public PriorityChain(DispatcherPriority priority)
	{
		_priority = priority;
	}
}
