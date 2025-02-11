using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

[StructLayout(LayoutKind.Explicit)]
[MS.Internal.WindowsBase.FriendAccessAllowed]
internal struct PropVariantUnion
{
	[FieldOffset(0)]
	internal sbyte cVal;

	[FieldOffset(0)]
	internal byte bVal;

	[FieldOffset(0)]
	internal short iVal;

	[FieldOffset(0)]
	internal ushort uiVal;

	[FieldOffset(0)]
	internal int lVal;

	[FieldOffset(0)]
	internal uint ulVal;

	[FieldOffset(0)]
	internal int intVal;

	[FieldOffset(0)]
	internal uint uintVal;

	[FieldOffset(0)]
	internal long hVal;

	[FieldOffset(0)]
	internal ulong uhVal;

	[FieldOffset(0)]
	internal float fltVal;

	[FieldOffset(0)]
	internal double dblVal;

	[FieldOffset(0)]
	internal short boolVal;

	[FieldOffset(0)]
	internal int scode;

	[FieldOffset(0)]
	internal CY cyVal;

	[FieldOffset(0)]
	internal double date;

	[FieldOffset(0)]
	internal FILETIME filetime;

	[FieldOffset(0)]
	internal nint puuid;

	[FieldOffset(0)]
	internal nint pclipdata;

	[FieldOffset(0)]
	internal nint bstrVal;

	[FieldOffset(0)]
	internal BSTRBLOB bstrblobVal;

	[FieldOffset(0)]
	internal BLOB blob;

	[FieldOffset(0)]
	internal nint pszVal;

	[FieldOffset(0)]
	internal nint pwszVal;

	[FieldOffset(0)]
	internal nint punkVal;

	[FieldOffset(0)]
	internal nint pdispVal;

	[FieldOffset(0)]
	internal nint pStream;

	[FieldOffset(0)]
	internal nint pStorage;

	[FieldOffset(0)]
	internal nint pVersionedStream;

	[FieldOffset(0)]
	internal nint parray;

	[FieldOffset(0)]
	internal CArray cArray;

	[FieldOffset(0)]
	internal nint pcVal;

	[FieldOffset(0)]
	internal nint pbVal;

	[FieldOffset(0)]
	internal nint piVal;

	[FieldOffset(0)]
	internal nint puiVal;

	[FieldOffset(0)]
	internal nint plVal;

	[FieldOffset(0)]
	internal nint pulVal;

	[FieldOffset(0)]
	internal nint pintVal;

	[FieldOffset(0)]
	internal nint puintVal;

	[FieldOffset(0)]
	internal nint pfltVal;

	[FieldOffset(0)]
	internal nint pdblVal;

	[FieldOffset(0)]
	internal nint pboolVal;

	[FieldOffset(0)]
	internal nint pdecVal;

	[FieldOffset(0)]
	internal nint pscode;

	[FieldOffset(0)]
	internal nint pcyVal;

	[FieldOffset(0)]
	internal nint pdate;

	[FieldOffset(0)]
	internal nint pbstrVal;

	[FieldOffset(0)]
	internal nint ppunkVal;

	[FieldOffset(0)]
	internal nint ppdispVal;

	[FieldOffset(0)]
	internal nint pparray;

	[FieldOffset(0)]
	internal nint pvarVal;
}
