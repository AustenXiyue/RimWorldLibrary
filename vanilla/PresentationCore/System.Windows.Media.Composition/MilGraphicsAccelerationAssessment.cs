using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MilGraphicsAccelerationAssessment
{
	internal uint VideoMemoryBandwidth;

	internal uint VideoMemorySize;
}
