using System;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
[Flags]
internal enum ShutDownEvents : ushort
{
	DomainUnload = 1,
	ProcessExit = 2,
	DispatcherShutdown = 4,
	AppDomain = 3,
	All = 7
}
