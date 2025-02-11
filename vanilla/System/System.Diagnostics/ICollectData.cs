using System.Runtime.InteropServices;

namespace System.Diagnostics;

/// <summary>Prepares performance data for the performance DLL the system loads when working with performance counters.</summary>
/// <filterpriority>2</filterpriority>
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("73386977-D6FD-11D2-BED5-00C04F79E3AE")]
public interface ICollectData
{
	/// <summary>Called by the performance DLL's close performance data function.</summary>
	/// <filterpriority>2</filterpriority>
	void CloseData();

	/// <summary>Collects the performance data for the performance DLL.</summary>
	/// <param name="id">The call index. </param>
	/// <param name="valueName">A pointer to a Unicode string list with the requested object identifiers. </param>
	/// <param name="data">A pointer to the data buffer. </param>
	/// <param name="totalBytes">A pointer to a number of bytes. </param>
	/// <param name="res">When this method returns, contains a <see cref="T:System.IntPtr" /> to the first byte after the data, -1 for an error, or -2 if a larger buffer is required. This parameter is passed uninitialized.</param>
	/// <filterpriority>2</filterpriority>
	[return: MarshalAs(UnmanagedType.I4)]
	void CollectData([In][MarshalAs(UnmanagedType.I4)] int id, [In][MarshalAs(UnmanagedType.SysInt)] IntPtr valueName, [In][MarshalAs(UnmanagedType.SysInt)] IntPtr data, [In][MarshalAs(UnmanagedType.I4)] int totalBytes, [MarshalAs(UnmanagedType.SysInt)] out IntPtr res);
}
