namespace System.Windows.Media.Imaging;

/// <summary>Specifies the possible compression schemes for Tagged Image File Format (TIFF) bitmap images.</summary>
public enum TiffCompressOption
{
	/// <summary>The <see cref="T:System.Windows.Media.Imaging.TiffBitmapEncoder" /> encoder attempts to save the bitmap with the best possible compression schema. </summary>
	Default,
	/// <summary>The Tagged Image File Format (TIFF) image is not compressed.</summary>
	None,
	/// <summary>The CCITT3 compression schema is used.</summary>
	Ccitt3,
	/// <summary>The CCITT4 compression schema is used.</summary>
	Ccitt4,
	/// <summary>The LZW compression schema is used. </summary>
	Lzw,
	/// <summary>The RLE compression schema is used.</summary>
	Rle,
	/// <summary>Zip compression schema is used. </summary>
	Zip
}
