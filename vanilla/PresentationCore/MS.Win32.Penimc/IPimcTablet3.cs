using System;
using System.Runtime.InteropServices;

namespace MS.Win32.Penimc;

[ComImport]
[Guid("CEB1EF24-BB4E-498B-9DF7-12887ED0EB24")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPimcTablet3
{
	void GetKey(out int key);

	void GetName([MarshalAs(UnmanagedType.LPWStr)] out string name);

	void GetPlugAndPlayId([MarshalAs(UnmanagedType.LPWStr)] out string plugAndPlayId);

	void GetTabletAndDisplaySize(out int tabletWidth, out int tabletHeight, out int displayWidth, out int displayHeight);

	void GetHardwareCaps(out int caps);

	void GetDeviceType(out int devType);

	void RefreshCursorInfo();

	void GetCursorCount(out int cCursors);

	void GetCursorInfo(int iCursor, [MarshalAs(UnmanagedType.LPWStr)] out string sName, out int id, [MarshalAs(UnmanagedType.Bool)] out bool fInverted);

	void GetCursorButtonCount(int iCursor, out int cButtons);

	void GetCursorButtonInfo(int iCursor, int iButton, [MarshalAs(UnmanagedType.LPWStr)] out string sName, out Guid guid);

	void IsPropertySupported(Guid guid, [MarshalAs(UnmanagedType.Bool)] out bool fSupported);

	void GetPropertyInfo(Guid guid, out int min, out int max, out int units, out float resolution);

	void CreateContext(nint handle, [MarshalAs(UnmanagedType.Bool)] bool fEnable, uint timeout, out IPimcContext3 IPimcContext, out int key, out long commHandle);

	void GetPacketDescriptionInfo(out int cProps, out int cButtons);

	void GetPacketPropertyInfo(int iProp, out Guid guid, out int iMin, out int iMax, out int iUnits, out float flResolution);

	void GetPacketButtonInfo(int iButton, out Guid guid);
}
