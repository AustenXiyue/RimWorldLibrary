using System.Runtime.InteropServices;

namespace MS.Internal.IO.Packaging;

internal struct STATPROPSTG
{
	[MarshalAs(UnmanagedType.LPWStr)]
	private string lpwstrName;

	private uint propid;

	private VARTYPE vt;
}
