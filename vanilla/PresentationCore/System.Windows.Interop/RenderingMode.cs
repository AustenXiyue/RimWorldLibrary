namespace System.Windows.Interop;

internal enum RenderingMode
{
	Default = 0,
	Software = 1,
	Hardware = 2,
	HardwareReference = 16777218,
	DisableMultimonDisplayClipping = 16384,
	IsDisableMultimonDisplayClippingValid = 32768,
	DisableDirtyRectangles = 65536
}
