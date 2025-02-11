namespace System.Windows.Media;

[Flags]
internal enum VisualProxyFlags : uint
{
	None = 0u,
	IsSubtreeDirtyForRender = 1u,
	IsTransformDirty = 2u,
	IsClipDirty = 4u,
	IsContentDirty = 8u,
	IsOpacityDirty = 0x10u,
	IsOpacityMaskDirty = 0x20u,
	IsOffsetDirty = 0x40u,
	IsClearTypeHintDirty = 0x80u,
	IsGuidelineCollectionDirty = 0x100u,
	IsEdgeModeDirty = 0x200u,
	IsContentConnected = 0x400u,
	IsContentNodeConnected = 0x800u,
	IsConnectedToParent = 0x1000u,
	Viewport3DVisual_IsCameraDirty = 0x2000u,
	Viewport3DVisual_IsViewportDirty = 0x4000u,
	IsBitmapScalingModeDirty = 0x8000u,
	IsDeleteResourceInProgress = 0x10000u,
	IsChildrenZOrderDirty = 0x20000u,
	IsEffectDirty = 0x40000u,
	IsCacheModeDirty = 0x80000u,
	IsScrollableAreaClipDirty = 0x100000u,
	IsTextRenderingModeDirty = 0x200000u,
	IsTextHintingModeDirty = 0x400000u
}
