namespace Mono.Cecil;

internal enum SecurityAction : ushort
{
	Request = 1,
	Demand,
	Assert,
	Deny,
	PermitOnly,
	LinkDemand,
	InheritDemand,
	RequestMinimum,
	RequestOptional,
	RequestRefuse,
	PreJitGrant,
	PreJitDeny,
	NonCasDemand,
	NonCasLinkDemand,
	NonCasInheritance
}
