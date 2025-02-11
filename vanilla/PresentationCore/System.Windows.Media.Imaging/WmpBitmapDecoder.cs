using System.IO;
using Microsoft.Win32.SafeHandles;
using MS.Internal;

namespace System.Windows.Media.Imaging;

/// <summary>Defines a decoder for Microsoft Windows Media Photo encoded images. </summary>
public sealed class WmpBitmapDecoder : BitmapDecoder
{
	private WmpBitmapDecoder()
	{
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Imaging.WmpBitmapDecoder" /> from the specified <see cref="T:System.Uri" /> with the specified <paramref name="createOptions" /> and <paramref name="cacheOption" />.</summary>
	/// <param name="bitmapUri">The <see cref="T:System.Uri" /> that identifies the bitmap to decode.</param>
	/// <param name="createOptions">Initialization options for the bitmap image.</param>
	/// <param name="cacheOption">The caching method for the bitmap image.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="bitmapUri" /> value is null.</exception>
	/// <exception cref="T:System.IO.FileFormatException">The <paramref name="bitmapUri" /> is not a Windows Media Photo encoded image.</exception>
	public WmpBitmapDecoder(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(bitmapUri, createOptions, cacheOption, MILGuidData.GUID_ContainerFormatWmp)
	{
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Imaging.WmpBitmapDecoder" /> from the specified file stream with the specified <paramref name="createOptions" /> and <paramref name="cacheOption" />.</summary>
	/// <param name="bitmapStream">The bitmap stream to decode.</param>
	/// <param name="createOptions">Initialization options for the bitmap image.</param>
	/// <param name="cacheOption">The caching method for the bitmap image.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="bitmapStream" /> value is null.</exception>
	/// <exception cref="T:System.IO.FileFormatException">The <paramref name="bitmapStream" /> is not a Windows Media Photo encoded image.</exception>
	public WmpBitmapDecoder(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(bitmapStream, createOptions, cacheOption, MILGuidData.GUID_ContainerFormatWmp)
	{
	}

	internal WmpBitmapDecoder(SafeMILHandle decoderHandle, BitmapDecoder decoder, Uri baseUri, Uri uri, Stream stream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, bool insertInDecoderCache, bool originalWritable, Stream uriStream, UnmanagedMemoryStream unmanagedMemoryStream, SafeFileHandle safeFilehandle)
		: base(decoderHandle, decoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, originalWritable, uriStream, unmanagedMemoryStream, safeFilehandle)
	{
	}

	internal override void SealObject()
	{
		throw new NotImplementedException();
	}
}
