using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MilRenderOptions
{
	internal MilRenderOptionFlags Flags;

	internal EdgeMode EdgeMode;

	internal MilCompositingMode CompositingMode;

	internal BitmapScalingMode BitmapScalingMode;

	internal ClearTypeHint ClearTypeHint;

	internal TextRenderingMode TextRenderingMode;

	internal TextHintingMode TextHintingMode;
}
