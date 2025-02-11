using System.Runtime.InteropServices;

namespace MS.Internal;

internal struct BitmapTransformCaps
{
	private int nSize;

	private int cMinInputs;

	private int cMaxInputs;

	[MarshalAs(UnmanagedType.Bool)]
	private bool fSupportMultiFormat;

	[MarshalAs(UnmanagedType.Bool)]
	private bool fAuxiliaryData;

	[MarshalAs(UnmanagedType.Bool)]
	private bool fSupportMultiOutput;

	[MarshalAs(UnmanagedType.Bool)]
	private bool fSupportBanding;

	[MarshalAs(UnmanagedType.Bool)]
	private bool fSupportMultiResolution;
}
