using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MS.Internal.Interop;

[ComImport]
[ComVisible(true)]
[Guid("00000109-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPersistStreamWithArrays
{
	void GetClassID(out Guid pClassID);

	[PreserveSig]
	int IsDirty();

	void Load(System.Runtime.InteropServices.ComTypes.IStream pstm);

	void Save(System.Runtime.InteropServices.ComTypes.IStream pstm, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

	void GetSizeMax(out long pcbSize);
}
