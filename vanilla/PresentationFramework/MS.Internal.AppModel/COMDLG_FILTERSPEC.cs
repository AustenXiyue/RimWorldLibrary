using System.Runtime.InteropServices;

namespace MS.Internal.AppModel;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct COMDLG_FILTERSPEC
{
	[MarshalAs(UnmanagedType.LPWStr)]
	public string pszName;

	[MarshalAs(UnmanagedType.LPWStr)]
	public string pszSpec;
}
