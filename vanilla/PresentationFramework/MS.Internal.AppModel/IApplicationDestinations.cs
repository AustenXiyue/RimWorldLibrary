using System.Runtime.InteropServices;

namespace MS.Internal.AppModel;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("12337d35-94c6-48a0-bce7-6a9c69d4d600")]
internal interface IApplicationDestinations
{
	void SetAppID([MarshalAs(UnmanagedType.LPWStr)] string pszAppID);

	void RemoveDestination([MarshalAs(UnmanagedType.IUnknown)] object punk);

	void RemoveAllDestinations();
}
