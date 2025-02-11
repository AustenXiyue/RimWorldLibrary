namespace System.Windows.Media.Composition;

[Flags]
internal enum MILTransparencyFlags
{
	Opaque = 0,
	ConstantAlpha = 1,
	PerPixelAlpha = 2,
	ColorKey = 4,
	FORCE_DWORD = -1
}
