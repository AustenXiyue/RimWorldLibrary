using System.Windows.Threading;

namespace MS.Internal.Data;

internal class DataBindOperation
{
	private DispatcherOperationCallback _method;

	private object _arg;

	private int _cost;

	public int Cost
	{
		get
		{
			return _cost;
		}
		set
		{
			_cost = value;
		}
	}

	public DataBindOperation(DispatcherOperationCallback method, object arg, int cost = 1)
	{
		_method = method;
		_arg = arg;
		_cost = cost;
	}

	public void Invoke()
	{
		_method(_arg);
	}
}
