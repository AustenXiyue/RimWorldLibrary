using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct SecurityCriticalData<T>
{
	private T _value;

	internal T Value => _value;

	internal SecurityCriticalData(T value)
	{
		_value = value;
	}
}
