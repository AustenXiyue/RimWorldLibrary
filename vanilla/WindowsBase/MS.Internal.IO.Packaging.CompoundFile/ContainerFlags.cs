using System;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

[MS.Internal.WindowsBase.FriendAccessAllowed]
[Flags]
internal enum ContainerFlags
{
	HostInBrowser = 1,
	Writable = 2,
	Metro = 8,
	ExecuteInstrumentation = 0x10
}
