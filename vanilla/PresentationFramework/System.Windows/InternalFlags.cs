namespace System.Windows;

internal enum InternalFlags : uint
{
	HasResourceReferences = 1u,
	HasNumberSubstitutionChanged = 2u,
	HasImplicitStyleFromResources = 4u,
	InheritanceBehavior0 = 8u,
	InheritanceBehavior1 = 0x10u,
	InheritanceBehavior2 = 0x20u,
	IsStyleUpdateInProgress = 0x40u,
	IsThemeStyleUpdateInProgress = 0x80u,
	StoresParentTemplateValues = 0x100u,
	NeedsClipBounds = 0x400u,
	HasWidthEverChanged = 0x800u,
	HasHeightEverChanged = 0x1000u,
	IsInitialized = 0x8000u,
	InitPending = 0x10000u,
	IsResourceParentValid = 0x20000u,
	AncestorChangeInProgress = 0x80000u,
	InVisibilityCollapsedTree = 0x100000u,
	HasStyleEverBeenFetched = 0x200000u,
	HasThemeStyleEverBeenFetched = 0x400000u,
	HasLocalStyle = 0x800000u,
	HasTemplateGeneratedSubTree = 0x1000000u,
	HasLogicalChildren = 0x4000000u,
	IsLogicalChildrenIterationInProgress = 0x8000000u,
	CreatingRoot = 0x10000000u,
	IsRightToLeft = 0x20000000u,
	ShouldLookupImplicitStyles = 0x40000000u,
	PotentiallyHasMentees = 0x80000000u
}
