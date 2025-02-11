using System.Runtime.InteropServices;
using MS.Internal.Interop;

namespace MS.Internal.AppModel;

[ComImport]
[Guid("1f681651-1024-4798-af36-119bbe5e5665")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface INativeProgressPage
{
	[PreserveSig]
	MS.Internal.Interop.HRESULT Show();

	[PreserveSig]
	MS.Internal.Interop.HRESULT Hide();

	[PreserveSig]
	MS.Internal.Interop.HRESULT ShowProgressMessage(string message);

	[PreserveSig]
	MS.Internal.Interop.HRESULT SetApplicationName(string appName);

	[PreserveSig]
	MS.Internal.Interop.HRESULT SetPublisherName(string publisherName);

	[PreserveSig]
	MS.Internal.Interop.HRESULT OnDownloadProgress(ulong bytesDownloaded, ulong bytesTotal);
}
