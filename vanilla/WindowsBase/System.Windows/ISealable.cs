using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal interface ISealable
{
	bool CanSeal { get; }

	bool IsSealed { get; }

	void Seal();
}
