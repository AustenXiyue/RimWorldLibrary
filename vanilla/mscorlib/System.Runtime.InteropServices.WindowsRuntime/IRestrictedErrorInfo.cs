namespace System.Runtime.InteropServices.WindowsRuntime;

[ComImport]
[Guid("82BA7092-4C88-427D-A7BC-16DD93FEB67E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IRestrictedErrorInfo
{
	void GetErrorDetails([MarshalAs(UnmanagedType.BStr)] out string description, out int error, [MarshalAs(UnmanagedType.BStr)] out string restrictedDescription, [MarshalAs(UnmanagedType.BStr)] out string capabilitySid);

	void GetReference([MarshalAs(UnmanagedType.BStr)] out string reference);
}
