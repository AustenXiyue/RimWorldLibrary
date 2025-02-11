namespace System.Windows;

[Flags]
internal enum CoreFlags : uint
{
	None = 0u,
	SnapsToDevicePixelsCache = 1u,
	ClipToBoundsCache = 2u,
	MeasureDirty = 4u,
	ArrangeDirty = 8u,
	MeasureInProgress = 0x10u,
	ArrangeInProgress = 0x20u,
	NeverMeasured = 0x40u,
	NeverArranged = 0x80u,
	MeasureDuringArrange = 0x100u,
	IsCollapsed = 0x200u,
	IsKeyboardFocusWithinCache = 0x400u,
	IsKeyboardFocusWithinChanged = 0x800u,
	IsMouseOverCache = 0x1000u,
	IsMouseOverChanged = 0x2000u,
	IsMouseCaptureWithinCache = 0x4000u,
	IsMouseCaptureWithinChanged = 0x8000u,
	IsStylusOverCache = 0x10000u,
	IsStylusOverChanged = 0x20000u,
	IsStylusCaptureWithinCache = 0x40000u,
	IsStylusCaptureWithinChanged = 0x80000u,
	HasAutomationPeer = 0x100000u,
	RenderingInvalidated = 0x200000u,
	IsVisibleCache = 0x400000u,
	AreTransformsClean = 0x800000u,
	IsOpacitySuppressed = 0x1000000u,
	ExistsEventHandlersStore = 0x2000000u,
	TouchesOverCache = 0x4000000u,
	TouchesOverChanged = 0x8000000u,
	TouchesCapturedWithinCache = 0x10000000u,
	TouchesCapturedWithinChanged = 0x20000000u,
	TouchLeaveCache = 0x40000000u,
	TouchEnterCache = 0x80000000u
}
