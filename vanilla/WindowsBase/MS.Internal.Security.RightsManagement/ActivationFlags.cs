using System;

namespace MS.Internal.Security.RightsManagement;

[Flags]
internal enum ActivationFlags : uint
{
	Machine = 1u,
	GroupIdentity = 2u,
	Temporary = 4u,
	Cancel = 8u,
	Silent = 0x10u,
	SharedGroupIdentity = 0x20u,
	Delayed = 0x40u
}
