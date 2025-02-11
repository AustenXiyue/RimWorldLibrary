using System.Runtime.InteropServices;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214F2-0000-0000-C000-000000000046")]
internal interface IEnumIDList
{
	[PreserveSig]
	MS.Internal.Interop.HRESULT Next(uint celt, out nint rgelt, out int pceltFetched);

	[PreserveSig]
	MS.Internal.Interop.HRESULT Skip(uint celt);

	void Reset();

	[return: MarshalAs(UnmanagedType.Interface)]
	IEnumIDList Clone();
}
