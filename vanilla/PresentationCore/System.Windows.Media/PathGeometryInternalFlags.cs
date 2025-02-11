namespace System.Windows.Media;

[Flags]
internal enum PathGeometryInternalFlags
{
	None = 0,
	Invalid = 1,
	Dirty = 2,
	BoundsValid = 4
}
