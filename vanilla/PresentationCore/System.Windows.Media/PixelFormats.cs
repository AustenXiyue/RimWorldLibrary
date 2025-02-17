using MS.Internal;

namespace System.Windows.Media;

/// <summary>Represents the collection of supported pixel formats. </summary>
public static class PixelFormats
{
	/// <summary>Gets the pixel format that is best suited for the particular operation. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.PixelFormat" /> best suited for the particular operation.</returns>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Windows.Media.PixelFormat" /> properties are accessed.</exception>
	public static PixelFormat Default => new PixelFormat(PixelFormatEnum.Default);

	/// <summary>Gets the pixel format specifying a paletted bitmap with 2 colors. </summary>
	/// <returns>The pixel format which specifying a paletted bitmap with 2 colors.</returns>
	public static PixelFormat Indexed1 => new PixelFormat(PixelFormatEnum.Indexed1);

	/// <summary>Gets the pixel format specifying a paletted bitmap with 4 colors. </summary>
	/// <returns>The pixel format which specifying a paletted bitmap with 4 colors.</returns>
	public static PixelFormat Indexed2 => new PixelFormat(PixelFormatEnum.Indexed2);

	/// <summary>Gets the pixel format specifying a paletted bitmap with 16 colors. </summary>
	/// <returns>The pixel format which specifying a paletted bitmap with 16 colors.</returns>
	public static PixelFormat Indexed4 => new PixelFormat(PixelFormatEnum.Indexed4);

	/// <summary>Gets the pixel format specifying a paletted bitmap with 256 colors. </summary>
	/// <returns>The pixel format which specifying a paletted bitmap with 256 colors.</returns>
	public static PixelFormat Indexed8 => new PixelFormat(PixelFormatEnum.Indexed8);

	/// <summary>Gets the black and white pixel format which displays one bit of data per pixel as either black or white. </summary>
	/// <returns>The pixel format Black-and-White. </returns>
	public static PixelFormat BlackWhite => new PixelFormat(PixelFormatEnum.BlackWhite);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Gray2" /> pixel format which displays a 2 bits-per-pixel grayscale channel, allowing 4 shades of gray.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Gray2" /> pixel format.</returns>
	public static PixelFormat Gray2 => new PixelFormat(PixelFormatEnum.Gray2);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Gray4" /> pixel format which displays a 4 bits-per-pixel grayscale channel, allowing 16 shades of gray. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Gray4" /> pixel format.</returns>
	public static PixelFormat Gray4 => new PixelFormat(PixelFormatEnum.Gray4);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Gray8" /> pixel format which displays an 8 bits-per-pixel grayscale channel, allowing 256 shades of gray. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Gray8" /> pixel format.</returns>
	public static PixelFormat Gray8 => new PixelFormat(PixelFormatEnum.Gray8);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Bgr555" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Bgr555" /> is a sRGB format with 16 bits per pixel (BPP). Each color channel (blue, green, and red) is allocated 5 bits per pixel (BPP).</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Bgr555" /> pixel format </returns>
	public static PixelFormat Bgr555 => new PixelFormat(PixelFormatEnum.Bgr555);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Bgr565" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Bgr565" /> is a sRGB format with 16 bits per pixel (BPP). Each color channel (blue, green, and red) is allocated 5, 6, and 5 bits per pixel (BPP) respectively.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Bgr565" /> pixel format.</returns>
	public static PixelFormat Bgr565 => new PixelFormat(PixelFormatEnum.Bgr565);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Rgb128Float" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Rgb128Float" /> is a ScRGB format with 128 bits per pixel (BPP). Each color channel is allocated 32 BPP. This format has a gamma of 1.0.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Rgb128Float" /> pixel format.</returns>
	public static PixelFormat Rgb128Float => new PixelFormat(PixelFormatEnum.Rgb128Float);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Bgr24" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Bgr24" /> is a sRGB format with 24 bits per pixel (BPP). Each color channel (blue, green, and red) is allocated 8 bits per pixel (BPP). </summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Bgr24" /> pixel format.</returns>
	public static PixelFormat Bgr24 => new PixelFormat(PixelFormatEnum.Bgr24);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Rgb24" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Rgb24" /> is a sRGB format with 24 bits per pixel (BPP). Each color channel (red, green, and blue) is allocated 8 bits per pixel (BPP). </summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Rgb24" /> pixel format.</returns>
	public static PixelFormat Rgb24 => new PixelFormat(PixelFormatEnum.Rgb24);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Bgr101010" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Bgr101010" /> is a sRGB format with 32 bits per pixel (BPP). Each color channel (blue, green, and red) is allocated 10 bits per pixel (BPP).</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Bgr101010" /> pixel format.</returns>
	public static PixelFormat Bgr101010 => new PixelFormat(PixelFormatEnum.Bgr101010);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Bgr32" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Bgr32" /> is a sRGB format with 32 bits per pixel (BPP). Each color channel (blue, green, and red) is allocated 8 bits per pixel (BPP).</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Bgr32" /> pixel format.</returns>
	public static PixelFormat Bgr32 => new PixelFormat(PixelFormatEnum.Bgr32);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Bgra32" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Bgra32" /> is a sRGB format with 32 bits per pixel (BPP). Each channel (blue, green, red, and alpha) is allocated 8 bits per pixel (BPP).</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Bgra32" /> pixel format.</returns>
	public static PixelFormat Bgra32 => new PixelFormat(PixelFormatEnum.Bgra32);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Pbgra32" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Pbgra32" /> is a sRGB format with 32 bits per pixel (BPP). Each channel (blue, green, red, and alpha) is allocated 8 bits per pixel (BPP). Each color channel is pre-multiplied by the alpha value. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Pbgra32" /> pixel format.</returns>
	public static PixelFormat Pbgra32 => new PixelFormat(PixelFormatEnum.Pbgra32);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Rgb48" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Rgb48" /> is a sRGB format with 48 bits per pixel (BPP). Each color channel (red, green, and blue) is allocated 16 bits per pixel (BPP). This format has a gamma of 1.0.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Rgb48" /> pixel format.</returns>
	public static PixelFormat Rgb48 => new PixelFormat(PixelFormatEnum.Rgb48);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Rgba64" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Rgba64" /> is an sRGB format with 64 bits per pixel (BPP). Each channel (red, green, blue, and alpha) is allocated 16 bits per pixel (BPP). This format has a gamma of 1.0.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Rgba64" /> pixel format.</returns>
	public static PixelFormat Rgba64 => new PixelFormat(PixelFormatEnum.Rgba64);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Prgba64" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Prgba64" /> is a sRGB format with 64 bits per pixel (BPP). Each channel (blue, green, red, and alpha) is allocated 32 bits per pixel (BPP). Each color channel is pre-multiplied by the alpha value. This format has a gamma of 1.0.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Prgba64" /> pixel format.</returns>
	public static PixelFormat Prgba64 => new PixelFormat(PixelFormatEnum.Prgba64);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Gray16" /> pixel format which displays a 16 bits-per-pixel grayscale channel, allowing 65536 shades of gray. This format has a gamma of 1.0. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Gray16" /> pixel format.</returns>
	public static PixelFormat Gray16 => new PixelFormat(PixelFormatEnum.Gray16);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Gray32Float" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Gray32Float" /> displays a 32 bits per pixel (BPP) grayscale channel, allowing over 4 billion shades of gray. This format has a gamma of 1.0.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Gray32Float" /> pixel format.</returns>
	public static PixelFormat Gray32Float => new PixelFormat(PixelFormatEnum.Gray32Float);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Rgba128Float" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Rgba128Float" /> is a ScRGB format with 128 bits per pixel (BPP). Each color channel is allocated 32 bits per pixel (BPP). This format has a gamma of 1.0.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Rgba128Float" /> pixel format.</returns>
	public static PixelFormat Rgba128Float => new PixelFormat(PixelFormatEnum.Rgba128Float);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Prgba128Float" /> pixel format. <see cref="P:System.Windows.Media.PixelFormats.Prgba128Float" /> is a ScRGB format with 128 bits per pixel (BPP). Each channel (red, green, blue, and alpha) is allocated 32 bits per pixel (BPP). Each color channel is pre-multiplied by the alpha value. This format has a gamma of 1.0.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.PixelFormats.Prgba128Float" /> pixel format.</returns>
	public static PixelFormat Prgba128Float => new PixelFormat(PixelFormatEnum.Prgba128Float);

	/// <summary>Gets the <see cref="P:System.Windows.Media.PixelFormats.Cmyk32" /> pixel format which displays 32 bits per pixel (BPP) with each color channel (cyan, magenta, yellow, and black) allocated 8 bits per pixel (BPP). </summary>
	/// <returns>The CMYK32 pixel format.</returns>
	public static PixelFormat Cmyk32 => new PixelFormat(PixelFormatEnum.Cmyk32);
}
