namespace System.Windows.Media.Composition;

[Flags]
internal enum MilRenderOptionFlags
{
	BitmapScalingMode = 1,
	EdgeMode = 2,
	CompositingMode = 4,
	ClearTypeHint = 8,
	TextRenderingMode = 0x10,
	TextHintingMode = 0x20,
	Last = 0x21,
	FORCE_DWORD = -1
}
