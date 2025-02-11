namespace System.Windows;

[Flags]
internal enum InternalFlags2 : uint
{
	R0 = 1u,
	R1 = 2u,
	R2 = 4u,
	R3 = 8u,
	R4 = 0x10u,
	R5 = 0x20u,
	R6 = 0x40u,
	R7 = 0x80u,
	R8 = 0x100u,
	R9 = 0x200u,
	RA = 0x400u,
	RB = 0x800u,
	RC = 0x1000u,
	RD = 0x2000u,
	RE = 0x4000u,
	RF = 0x8000u,
	TreeHasLoadedChangeHandler = 0x100000u,
	IsLoadedCache = 0x200000u,
	IsStyleSetFromGenerator = 0x400000u,
	IsParentAnFE = 0x800000u,
	IsTemplatedParentAnFE = 0x1000000u,
	HasStyleChanged = 0x2000000u,
	HasTemplateChanged = 0x4000000u,
	HasStyleInvalidated = 0x8000000u,
	IsRequestingExpression = 0x10000000u,
	HasMultipleInheritanceContexts = 0x20000000u,
	BypassLayoutPolicies = 0x80000000u,
	Default = 0xFFFFu
}
