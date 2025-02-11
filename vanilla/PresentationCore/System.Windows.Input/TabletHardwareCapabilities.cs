namespace System.Windows.Input;

/// <summary>Defines values that specify the hardware capabilities of a tablet device, including desktop digitizers and mice. </summary>
[Serializable]
[Flags]
public enum TabletHardwareCapabilities
{
	/// <summary>Indicates the tablet device cannot provide this information.</summary>
	None = 0,
	/// <summary>Indicates the digitizer is integrated with the display.</summary>
	Integrated = 1,
	/// <summary>Indicates the stylus must be in physical contact with the tablet device to report its position.</summary>
	StylusMustTouch = 2,
	/// <summary>Indicates the tablet device can generate in-air packets when the stylus is in the physical detection range (proximity) of the tablet device.</summary>
	HardProximity = 4,
	/// <summary>Indicates the tablet device can uniquely identify the active stylus.</summary>
	StylusHasPhysicalIds = 8,
	/// <summary>Indicates that the tablet device can detect the amount of pressure the user applies when using the stylus.</summary>
	SupportsPressure = 0x40000000
}
