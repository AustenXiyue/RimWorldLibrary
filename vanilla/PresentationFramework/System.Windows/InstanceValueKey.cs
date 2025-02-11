namespace System.Windows;

internal class InstanceValueKey
{
	private int _childIndex;

	private int _dpIndex;

	private int _index;

	internal InstanceValueKey(int childIndex, int dpIndex, int index)
	{
		_childIndex = childIndex;
		_dpIndex = dpIndex;
		_index = index;
	}

	public override bool Equals(object o)
	{
		if (o is InstanceValueKey instanceValueKey)
		{
			if (_childIndex == instanceValueKey._childIndex && _dpIndex == instanceValueKey._dpIndex)
			{
				return _index == instanceValueKey._index;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 20000 * _childIndex + 20 * _dpIndex + _index;
	}
}
