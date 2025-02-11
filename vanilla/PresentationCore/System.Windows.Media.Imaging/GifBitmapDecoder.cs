using System.IO;
using Microsoft.Win32.SafeHandles;
using MS.Internal;

namespace System.Windows.Media.Imaging;

/// <summary>Defines a decoder for Graphics Interchange Format (GIF) encoded images.</summary>
public sealed class GifBitmapDecoder : BitmapDecoder
{
	private GifBitmapDecoder()
	{
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Imaging.GifBitmapDecoder" /> from the specified <see cref="T:System.Uri" /> with the specified <paramref name="createOptions" /> and <paramref name="cacheOption" />.</summary>
	/// <param name="bitmapUri">
	///   <see cref="T:System.Uri" /> that identifies the bitmap to decode.</param>
	/// <param name="createOptions">Initialization options for the bitmap image.</param>
	/// <param name="cacheOption">The cache method to use for the bitmap image.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="bitmapUri" /> value is null.</exception>
	/// <exception cref="T:System.IO.FileFormatException">The <paramref name="bitmapUri" /> is not a Graphics Interchange Format (GIF) encoded image.</exception>
	public GifBitmapDecoder(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(bitmapUri, createOptions, cacheOption, MILGuidData.GUID_ContainerFormatGif)
	{
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Imaging.GifBitmapDecoder" /> from the specified file stream with the specified <paramref name="createOptions" /> and <paramref name="cacheOption" />.</summary>
	/// <param name="bitmapStream">Bitmap stream to decode.</param>
	/// <param name="createOptions">Initialization options for the bitmap image.</param>
	/// <param name="cacheOption">The cache method to use for the bitmap image.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="bitmapStream" /> value is null.</exception>
	/// <exception cref="T:System.IO.FileFormatException">The <paramref name="bitmapStream" /> is not a Graphics Interchange Format (GIF) encoded image.</exception>
	public GifBitmapDecoder(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(bitmapStream, createOptions, cacheOption, MILGuidData.GUID_ContainerFormatGif)
	{
	}

	internal GifBitmapDecoder(SafeMILHandle decoderHandle, BitmapDecoder decoder, Uri baseUri, Uri uri, Stream stream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, bool insertInDecoderCache, bool originalWritable, Stream uriStream, UnmanagedMemoryStream unmanagedMemoryStream, SafeFileHandle safeFilehandle)
		: base(decoderHandle, decoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, originalWritable, uriStream, unmanagedMemoryStream, safeFilehandle)
	{
	}

	internal override void SealObject()
	{
		throw new NotImplementedException();
	}
}
