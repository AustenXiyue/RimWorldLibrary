using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal enum OperationType : byte
{
	Unknown,
	AddChild,
	RemoveChild,
	Inherit,
	ChangeMutableDefaultValue
}
