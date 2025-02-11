using System;

namespace MS.Internal.Security.RightsManagement;

[Flags]
internal enum AcquireLicenseFlags : uint
{
	NonSilent = 1u,
	NoPersist = 2u,
	Cancel = 4u,
	FetchAdvisory = 8u,
	NoUI = 0x10u
}
