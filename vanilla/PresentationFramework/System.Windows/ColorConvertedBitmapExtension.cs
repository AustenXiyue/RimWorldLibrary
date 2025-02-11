using System.IO;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace System.Windows;

/// <summary>Implements a markup extension that enables <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" /> creation. A <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" /> does not have an embedded profile, the profile instead being based on source and destination values.</summary>
[MarkupExtensionReturnType(typeof(ColorConvertedBitmap))]
public class ColorConvertedBitmapExtension : MarkupExtension
{
	private string _image;

	private string _sourceProfile;

	private Uri _baseUri;

	private string _destinationProfile;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ColorConvertedBitmapExtension" /> class.</summary>
	public ColorConvertedBitmapExtension()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ColorConvertedBitmapExtension" /> class.</summary>
	/// <param name="image">A string that is parsed to determine three URIs: image source, source color context, and destination color context.</param>
	public ColorConvertedBitmapExtension(object image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		string[] array = ((string)image).Split(' ');
		foreach (string text in array)
		{
			if (text.Length <= 0)
			{
				continue;
			}
			if (_image == null)
			{
				_image = text;
				continue;
			}
			if (_sourceProfile == null)
			{
				_sourceProfile = text;
				continue;
			}
			if (_destinationProfile == null)
			{
				_destinationProfile = text;
				continue;
			}
			throw new InvalidOperationException(SR.ColorConvertedBitmapExtensionSyntax);
		}
	}

	/// <summary>Returns an object that should be set on the property where this extension is applied. For <see cref="T:System.Windows.ColorConvertedBitmapExtension" />, this is the completed <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" /> based on the values passed to the constructor.</returns>
	/// <param name="serviceProvider">An object that can provide services for the markup extension. This service is expected to provide results for <see cref="T:System.Windows.Markup.IUriContext" />.</param>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (_image == null)
		{
			throw new InvalidOperationException(SR.ColorConvertedBitmapExtensionNoSourceImage);
		}
		if (_sourceProfile == null)
		{
			throw new InvalidOperationException(SR.ColorConvertedBitmapExtensionNoSourceProfile);
		}
		if (!(serviceProvider.GetService(typeof(IUriContext)) is IUriContext uriContext))
		{
			throw new InvalidOperationException(SR.Format(SR.MarkupExtensionNoContext, GetType().Name, "IUriContext"));
		}
		_baseUri = uriContext.BaseUri;
		Uri resolvedUri = GetResolvedUri(_image);
		Uri resolvedUri2 = GetResolvedUri(_sourceProfile);
		Uri resolvedUri3 = GetResolvedUri(_destinationProfile);
		ColorContext sourceColorContext = new ColorContext(resolvedUri2);
		ColorContext destinationColorContext = ((resolvedUri3 != null) ? new ColorContext(resolvedUri3) : new ColorContext(PixelFormats.Default));
		FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(BitmapDecoder.Create(resolvedUri, BitmapCreateOptions.IgnoreColorProfile | BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.None).Frames[0], PixelFormats.Bgra32, null, 0.0);
		object result = formatConvertedBitmap;
		try
		{
			result = new ColorConvertedBitmap(formatConvertedBitmap, sourceColorContext, destinationColorContext, PixelFormats.Bgra32);
		}
		catch (FileFormatException)
		{
		}
		return result;
	}

	private Uri GetResolvedUri(string uri)
	{
		if (uri == null)
		{
			return null;
		}
		return new Uri(_baseUri, uri);
	}
}
