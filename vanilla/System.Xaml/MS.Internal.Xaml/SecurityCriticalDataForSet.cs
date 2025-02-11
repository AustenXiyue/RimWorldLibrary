using System;
using MS.Internal.WindowsBase;

namespace MS.Internal.Xaml;

[Serializable]
[FriendAccessAllowed]
internal struct SecurityCriticalDataForSet<T>
{
	private T _value;

	internal T Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	internal SecurityCriticalDataForSet(T value)
	{
		_value = value;
	}
}
