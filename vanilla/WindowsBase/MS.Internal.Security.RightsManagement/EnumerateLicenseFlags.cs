using System;

namespace MS.Internal.Security.RightsManagement;

[Flags]
internal enum EnumerateLicenseFlags : uint
{
	Machine = 1u,
	GroupIdentity = 2u,
	GroupIdentityName = 4u,
	GroupIdentityLid = 8u,
	SpecifiedGroupIdentity = 0x10u,
	Eul = 0x20u,
	EulLid = 0x40u,
	ClientLicensor = 0x80u,
	ClientLicensorLid = 0x100u,
	SpecifiedClientLicensor = 0x200u,
	RevocationList = 0x400u,
	RevocationListLid = 0x800u,
	Expired = 0x1000u
}
