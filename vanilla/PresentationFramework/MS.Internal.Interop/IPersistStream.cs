using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Interop;

[ComImport]
[ComVisible(true)]
[Guid("00000109-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPersistStream
{
	void GetClassID(out Guid pClassID);

	[PreserveSig]
	int IsDirty();

	void Load(IStream pstm);

	void Save(IStream pstm, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

	void GetSizeMax(out long pcbSize);
}
