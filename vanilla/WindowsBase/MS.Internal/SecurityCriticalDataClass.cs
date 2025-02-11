using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class SecurityCriticalDataClass<T>
{
	private T _value;

	internal T Value => _value;

	internal SecurityCriticalDataClass(T value)
	{
		_value = value;
	}
}
