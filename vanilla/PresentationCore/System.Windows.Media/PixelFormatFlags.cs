namespace System.Windows.Media;

[Flags]
internal enum PixelFormatFlags
{
	BitsPerPixelMask = 0xFF,
	BitsPerPixelUndefined = 0,
	BitsPerPixel1 = 1,
	BitsPerPixel2 = 2,
	BitsPerPixel4 = 4,
	BitsPerPixel8 = 8,
	BitsPerPixel16 = 0x10,
	BitsPerPixel24 = 0x18,
	BitsPerPixel32 = 0x20,
	BitsPerPixel48 = 0x30,
	BitsPerPixel64 = 0x40,
	BitsPerPixel96 = 0x60,
	BitsPerPixel128 = 0x80,
	IsGray = 0x100,
	IsCMYK = 0x200,
	IsSRGB = 0x400,
	IsScRGB = 0x800,
	Premultiplied = 0x1000,
	ChannelOrderMask = 0x1E000,
	ChannelOrderRGB = 0x2000,
	ChannelOrderBGR = 0x4000,
	ChannelOrderARGB = 0x8000,
	ChannelOrderABGR = 0x10000,
	Palettized = 0x20000,
	NChannelAlpha = 0x40000,
	IsNChannel = 0x80000
}
