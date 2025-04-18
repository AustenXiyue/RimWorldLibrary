using System.Runtime.InteropServices;

namespace NAudio.Midi;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct MidiInCapabilities
{
	private const int MaxProductNameLength = 32;

	private ushort manufacturerId;

	private ushort productId;

	private uint driverVersion;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	private string productName;

	private int support;

	public Manufacturers Manufacturer => (Manufacturers)manufacturerId;

	public int ProductId => productId;

	public string ProductName => productName;
}
