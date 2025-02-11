namespace System.Windows.Media.Imaging;

/// <summary>Specifies whether a Portable Network Graphics (PNG) format image is interlaced during encoding.</summary>
public enum PngInterlaceOption
{
	/// <summary>The <see cref="T:System.Windows.Media.Imaging.PngBitmapEncoder" /> determines whether the image should be interlaced.</summary>
	Default,
	/// <summary>The resulting bitmap image is interlaced.</summary>
	On,
	/// <summary>The resulting bitmap image is not interlaced.</summary>
	Off
}
