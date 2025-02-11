using System.Runtime.InteropServices;

namespace MS.Win32.Penimc;

[ComImport]
[Guid("BD2C38C2-E064-41D0-A999-940F526219C2")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPimcManager3
{
	void GetTabletCount(out uint count);

	void GetTablet(uint tablet, out IPimcTablet3 IPimcTablet);
}
