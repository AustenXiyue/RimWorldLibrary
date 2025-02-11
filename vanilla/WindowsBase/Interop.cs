using System.IO.Compression;
using System.Runtime.InteropServices;

internal static class Interop
{
	internal static class Zlib
	{
		internal static readonly byte[] ZLibVersion = new byte[6] { 49, 46, 50, 46, 51, 0 };

		[DllImport("System.IO.Compression.Native.dll", EntryPoint = "CompressionNative_DeflateInit2_")]
		internal static extern System.IO.Compression.ZLibNative.ErrorCode DeflateInit2_(ref System.IO.Compression.ZLibNative.ZStream stream, System.IO.Compression.ZLibNative.CompressionLevel level, System.IO.Compression.ZLibNative.CompressionMethod method, int windowBits, int memLevel, System.IO.Compression.ZLibNative.CompressionStrategy strategy);

		[DllImport("System.IO.Compression.Native.dll", EntryPoint = "CompressionNative_Deflate")]
		internal static extern System.IO.Compression.ZLibNative.ErrorCode Deflate(ref System.IO.Compression.ZLibNative.ZStream stream, System.IO.Compression.ZLibNative.FlushCode flush);

		[DllImport("System.IO.Compression.Native.dll", EntryPoint = "CompressionNative_DeflateEnd")]
		internal static extern System.IO.Compression.ZLibNative.ErrorCode DeflateEnd(ref System.IO.Compression.ZLibNative.ZStream stream);

		[DllImport("System.IO.Compression.Native.dll", EntryPoint = "CompressionNative_InflateInit2_")]
		internal static extern System.IO.Compression.ZLibNative.ErrorCode InflateInit2_(ref System.IO.Compression.ZLibNative.ZStream stream, int windowBits);

		[DllImport("System.IO.Compression.Native.dll", EntryPoint = "CompressionNative_Inflate")]
		internal static extern System.IO.Compression.ZLibNative.ErrorCode Inflate(ref System.IO.Compression.ZLibNative.ZStream stream, System.IO.Compression.ZLibNative.FlushCode flush);

		[DllImport("System.IO.Compression.Native.dll", EntryPoint = "CompressionNative_InflateEnd")]
		internal static extern System.IO.Compression.ZLibNative.ErrorCode InflateEnd(ref System.IO.Compression.ZLibNative.ZStream stream);

		[DllImport("System.IO.Compression.Native.dll", EntryPoint = "CompressionNative_Crc32")]
		internal unsafe static extern uint crc32(uint crc, byte* buffer, int len);
	}

	internal static class Libraries
	{
		internal const string Advapi32 = "advapi32.dll";

		internal const string BCrypt = "BCrypt.dll";

		internal const string CoreComm_L1_1_1 = "api-ms-win-core-comm-l1-1-1.dll";

		internal const string Crypt32 = "crypt32.dll";

		internal const string Error_L1 = "api-ms-win-core-winrt-error-l1-1-0.dll";

		internal const string HttpApi = "httpapi.dll";

		internal const string IpHlpApi = "iphlpapi.dll";

		internal const string Kernel32 = "kernel32.dll";

		internal const string Memory_L1_3 = "api-ms-win-core-memory-l1-1-3.dll";

		internal const string Mswsock = "mswsock.dll";

		internal const string NCrypt = "ncrypt.dll";

		internal const string NtDll = "ntdll.dll";

		internal const string Odbc32 = "odbc32.dll";

		internal const string OleAut32 = "oleaut32.dll";

		internal const string PerfCounter = "perfcounter.dll";

		internal const string RoBuffer = "api-ms-win-core-winrt-robuffer-l1-1-0.dll";

		internal const string Secur32 = "secur32.dll";

		internal const string Shell32 = "shell32.dll";

		internal const string SspiCli = "sspicli.dll";

		internal const string User32 = "user32.dll";

		internal const string Version = "version.dll";

		internal const string WebSocket = "websocket.dll";

		internal const string WinHttp = "winhttp.dll";

		internal const string WinMM = "winmm.dll";

		internal const string Ws2_32 = "ws2_32.dll";

		internal const string Wtsapi32 = "wtsapi32.dll";

		internal const string CompressionNative = "System.IO.Compression.Native.dll";
	}
}
