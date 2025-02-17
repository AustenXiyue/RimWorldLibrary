using System.Runtime.InteropServices;

namespace Standard;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214F2-0000-0000-C000-000000000046")]
internal interface IEnumIDList
{
	[PreserveSig]
	HRESULT Next(uint celt, out nint rgelt, out int pceltFetched);

	[PreserveSig]
	HRESULT Skip(uint celt);

	void Reset();

	void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumIDList ppenum);
}
