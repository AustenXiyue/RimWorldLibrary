using System;

namespace MS.Internal.Security.RightsManagement;

[Flags]
internal enum SignIssuanceLicenseFlags : uint
{
	Online = 1u,
	Offline = 2u,
	Cancel = 4u,
	ServerIssuanceLicense = 8u,
	AutoGenerateKey = 0x10u,
	OwnerLicenseNoPersist = 0x20u
}
