using System;
using System.Runtime.InteropServices;

namespace MS.Win32.Penimc;

[ComImport]
[Guid("75C6AAEE-2BA4-4008-B523-4F1E033FF049")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPimcContext3
{
	void ShutdownComm();

	void GetPacketDescriptionInfo(out int cProps, out int cButtons);

	void GetPacketPropertyInfo(int iProp, out Guid guid, out int iMin, out int iMax, out int iUnits, out float flResolution);

	void GetPacketButtonInfo(int iButton, out Guid guid);

	void GetLastSystemEventData(out int evt, out int modifier, out int character, out int x, out int y, out int stylusMode, out int buttonState);
}
