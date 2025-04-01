namespace System.Runtime.InteropServices.ComTypes;

/// <summary>Represents a generalized Clipboard format. </summary>
public struct FORMATETC
{
	/// <summary>Specifies the particular clipboard format of interest.</summary>
	[MarshalAs(UnmanagedType.U2)]
	public short cfFormat;

	/// <summary>Specifies one of the <see cref="T:System.Runtime.InteropServices.ComTypes.DVASPECT" /> enumeration constants that indicates how much detail should be contained in the rendering.</summary>
	[MarshalAs(UnmanagedType.U4)]
	public DVASPECT dwAspect;

	/// <summary>Specifies part of the aspect when the data must be split across page boundaries. </summary>
	public int lindex;

	/// <summary>Specifies a pointer to a DVTARGETDEVICE structure containing information about the target device that the data is being composed for. </summary>
	public IntPtr ptd;

	/// <summary>Specifies one of the <see cref="T:System.Runtime.InteropServices.ComTypes.TYMED" /> enumeration constants, which indicates the type of storage medium used to transfer the object's data. </summary>
	[MarshalAs(UnmanagedType.U4)]
	public TYMED tymed;
}
