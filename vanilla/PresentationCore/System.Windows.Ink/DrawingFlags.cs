namespace System.Windows.Ink;

[Flags]
internal enum DrawingFlags
{
	Polyline = 0,
	FitToCurve = 1,
	SubtractiveTransparency = 2,
	IgnorePressure = 4,
	AntiAliased = 0x10,
	IgnoreRotation = 0x20,
	IgnoreAngle = 0x40
}
