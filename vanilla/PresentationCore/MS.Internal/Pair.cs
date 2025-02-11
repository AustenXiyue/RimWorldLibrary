namespace MS.Internal;

internal class Pair
{
	private object _first;

	private object _second;

	public object First => _first;

	public object Second => _second;

	public Pair(object first, object second)
	{
		_first = first;
		_second = second;
	}

	public override int GetHashCode()
	{
		return ((_first != null) ? _first.GetHashCode() : 0) ^ ((_second != null) ? _second.GetHashCode() : 0);
	}

	public override bool Equals(object o)
	{
		if (o is Pair pair && ((_first != null) ? _first.Equals(pair._first) : (pair._first == null)))
		{
			if (_second == null)
			{
				return pair._second == null;
			}
			return _second.Equals(pair._second);
		}
		return false;
	}
}
