using System.Runtime.InteropServices;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("b4db1657-70d7-485e-8e3e-6fcb5a5c1802")]
internal interface IModalWindow
{
	[PreserveSig]
	MS.Internal.Interop.HRESULT Show(nint parent);
}
