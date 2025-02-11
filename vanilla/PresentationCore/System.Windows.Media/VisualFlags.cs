namespace System.Windows.Media;

[Flags]
internal enum VisualFlags : uint
{
	None = 0u,
	IsSubtreeDirtyForPrecompute = 1u,
	ShouldPostRender = 2u,
	IsUIElement = 4u,
	IsLayoutSuspended = 8u,
	IsVisualChildrenIterationInProgress = 0x10u,
	Are3DContentBoundsValid = 0x20u,
	FindCommonAncestor = 0x40u,
	IsLayoutIslandRoot = 0x80u,
	UseLayoutRounding = 0x100u,
	VisibilityCache_Visible = 0x200u,
	VisibilityCache_TakesSpace = 0x400u,
	RegisteredForAncestorChanged = 0x800u,
	SubTreeHoldsAncestorChanged = 0x1000u,
	NodeIsCyclicBrushRoot = 0x2000u,
	NodeHasEffect = 0x4000u,
	IsViewport3DVisual = 0x8000u,
	ReentrancyFlag = 0x10000u,
	HasChildren = 0x20000u,
	BitmapEffectEmulationDisabled = 0x40000u,
	DpiScaleFlag1 = 0x80000u,
	DpiScaleFlag2 = 0x100000u,
	TreeLevelBit0 = 0x200000u,
	TreeLevelBit1 = 0x400000u,
	TreeLevelBit2 = 0x800000u,
	TreeLevelBit3 = 0x1000000u,
	TreeLevelBit4 = 0x2000000u,
	TreeLevelBit5 = 0x4000000u,
	TreeLevelBit6 = 0x8000000u,
	TreeLevelBit7 = 0x10000000u,
	TreeLevelBit8 = 0x20000000u,
	TreeLevelBit9 = 0x40000000u,
	TreeLevelBit10 = 0x80000000u
}
