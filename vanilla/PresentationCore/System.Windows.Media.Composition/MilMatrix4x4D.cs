using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MilMatrix4x4D
{
	internal double M_11;

	internal double M_12;

	internal double M_13;

	internal double M_14;

	internal double M_21;

	internal double M_22;

	internal double M_23;

	internal double M_24;

	internal double M_31;

	internal double M_32;

	internal double M_33;

	internal double M_34;

	internal double M_41;

	internal double M_42;

	internal double M_43;

	internal double M_44;
}
