using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MS.Internal.Progressivity;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("e7b92912-c7ca-4629-8f39-0f537cfab57e")]
internal interface IByteRangeDownloaderService
{
	void InitializeByteRangeDownloader([MarshalAs(UnmanagedType.LPWStr)] string url, [MarshalAs(UnmanagedType.LPWStr)] string tempFile, SafeWaitHandle eventHandle);

	void RequestDownloadByteRanges([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] int[] byteRanges, int size);

	void GetDownloadedByteRanges([MarshalAs(UnmanagedType.LPArray)] out int[] byteRanges, [MarshalAs(UnmanagedType.I4)] out int size);

	void ReleaseByteRangeDownloader();
}
