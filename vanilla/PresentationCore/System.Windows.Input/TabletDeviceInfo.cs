using System.Collections.ObjectModel;
using MS.Internal;
using MS.Win32.Penimc;

namespace System.Windows.Input;

internal class TabletDeviceInfo
{
	public SecurityCriticalDataClass<IPimcTablet3> PimcTablet;

	public int Id;

	public string Name;

	public string PlugAndPlayId;

	public TabletDeviceSizeInfo SizeInfo;

	public TabletHardwareCapabilities HardwareCapabilities;

	public TabletDeviceType DeviceType;

	public ReadOnlyCollection<StylusPointProperty> StylusPointProperties;

	public int PressureIndex;

	public StylusDeviceInfo[] StylusDevicesInfo;

	public uint WispTabletKey { get; set; }
}
