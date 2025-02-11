using System.Runtime.InteropServices;

namespace MS.Internal;

internal struct MILAVInstruction
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct nested_u
	{
		[FieldOffset(0)]
		[MarshalAs(UnmanagedType.Bool)]
		internal bool m_fValue;

		[FieldOffset(0)]
		internal double m_dblValue;

		[FieldOffset(0)]
		internal int m_iValue;

		[FieldOffset(0)]
		internal long m_lValue;

		[FieldOffset(0)]
		internal float m_flValue;
	}

	internal nint m_pMedia;

	internal MILAVInstructionType m_instType;

	internal nested_u u;
}
