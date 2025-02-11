using MS.Internal;

namespace System.Windows.Media.Imaging;

/// <summary>Defines several color palettes that are commonly used by bitmap images.</summary>
public static class BitmapPalettes
{
	private static BitmapPalette[] s_transparentPalettes;

	private static BitmapPalette[] s_opaquePalettes;

	private const int c_maxPalettes = 64;

	/// <summary>Gets a value that represents a black-and-white color palette. This palette consists of 2 colors total.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette BlackAndWhite => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedBW, hasAlpha: false);

	/// <summary>Gets a value that represents a black, white, and transparent color palette. This palette consists of 3 colors total.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette BlackAndWhiteTransparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedBW, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 8 primary colors and 16 system colors, with duplicate colors removed. There are a total of 16 colors in this palette, which are the same as the system palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone8 => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone8, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 8 primary colors and 16 system colors, with duplicate colors removed and 1 additional transparent color. There are a total of 17 colors in this palette. </summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone8Transparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone8, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 27 primary colors and 16 system colors, with duplicate colors removed. There are a total of 35 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone27 => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone27, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 27 primary colors and 16 system colors, with duplicate colors removed and 1 additional transparent color. There are a total of 36 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone27Transparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone27, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 64 primary colors and 16 system colors, with duplicate colors removed. There are a total of 72 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone64 => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone64, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 64 primary colors and 16 system colors, with duplicate colors removed and 1 additional transparent color. There are a total of 73 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone64Transparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone64, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 125 primary colors and 16 system colors, with duplicate colors removed. There are a total of 133 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone125 => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone125, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 125 primary colors, 16 system colors, and 1 additional transparent color. Duplicate colors in the palette are removed. There are a total of 134 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone125Transparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone125, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 216 primary colors and 16 system colors, with duplicate colors removed. There are a total of 224 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone216 => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone216, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 216 primary colors, 16 system colors, and 1 additional transparent color. Duplicate colors in the palette are removed. There are a total of 225 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone216Transparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone216, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 252 primary colors and 16 system colors, with duplicate colors removed. There are a total of 256 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone252 => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone252, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 252 primary colors, 16 system colors, and 1 additional transparent color. Duplicate colors in the palette are removed. There are a total of 256 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone252Transparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone252, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 256 primary colors and 16 system colors, with duplicate colors removed. There are a total of 256 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone256 => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone256, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 256 primary colors, 16 system colors, and 1 additional transparent color that replaces the final color in the sequence. Duplicate colors in the palette are removed. There are a total of 256 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Halftone256Transparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone256, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 4 shades of gray, ranging from black to gray to white. This palette contains 4 total colors.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Gray4 => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedGray4, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 4 shades of gray, ranging from black to gray to white with an additional transparent color. This palette contains 5 total colors.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Gray4Transparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedGray4, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 16 shades of gray. The palette ranges from black to gray to white. This palette contains 16 total colors.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Gray16 => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedGray16, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 16 shades of gray. The palette ranges from black to gray to white with an additional transparent color. This palette contains 17 total colors.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Gray16Transparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedGray16, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 256 shades of gray, ranging from black to gray to white. This palette contains 256 total colors.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Gray256 => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedGray256, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 256 shades of gray, ranging from black to gray to white with an additional transparent color. This palette contains 257 total colors.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette Gray256Transparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedGray256, hasAlpha: true);

	/// <summary>Gets a value that represents a color palette that contains 216 primary colors and 16 system colors, with duplicate colors removed. There are a total of 224 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette WebPalette => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone216, hasAlpha: false);

	/// <summary>Gets a value that represents a color palette that contains 216 primary colors and 16 system colors, with duplicate colors removed and 1 additional transparent color. There are a total of 225 colors in this palette.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapPalette" />.</returns>
	public static BitmapPalette WebPaletteTransparent => FromMILPaletteType(WICPaletteType.WICPaletteTypeFixedHalftone216, hasAlpha: true);

	private static BitmapPalette[] transparentPalettes
	{
		get
		{
			if (s_transparentPalettes == null)
			{
				s_transparentPalettes = new BitmapPalette[64];
			}
			return s_transparentPalettes;
		}
	}

	private static BitmapPalette[] opaquePalettes
	{
		get
		{
			if (s_opaquePalettes == null)
			{
				s_opaquePalettes = new BitmapPalette[64];
			}
			return s_opaquePalettes;
		}
	}

	internal static BitmapPalette FromMILPaletteType(WICPaletteType type, bool hasAlpha)
	{
		BitmapPalette[] array = ((!hasAlpha) ? opaquePalettes : transparentPalettes);
		BitmapPalette bitmapPalette = array[(int)type];
		if (bitmapPalette == null)
		{
			lock (array)
			{
				bitmapPalette = array[(int)type];
				if (bitmapPalette == null)
				{
					bitmapPalette = (array[(int)type] = new BitmapPalette(type, hasAlpha));
				}
			}
		}
		return bitmapPalette;
	}
}
