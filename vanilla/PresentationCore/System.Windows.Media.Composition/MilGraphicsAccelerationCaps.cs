using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MilGraphicsAccelerationCaps
{
	internal int TierValue;

	internal int HasWDDMSupport;

	internal uint PixelShaderVersion;

	internal uint VertexShaderVersion;

	internal uint MaxTextureWidth;

	internal uint MaxTextureHeight;

	internal int WindowCompatibleMode;

	internal uint BitsPerPixel;

	internal uint HasSSE2Support;

	internal uint MaxPixelShader30InstructionSlots;
}
