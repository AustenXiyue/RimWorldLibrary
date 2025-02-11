using System;

namespace MS.Internal.Security.RightsManagement;

[Flags]
internal enum ServiceType : uint
{
	Activation = 1u,
	Certification = 2u,
	Publishing = 4u,
	ClientLicensor = 8u
}
