using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using MS.Internal;
using MS.Internal.IO.Packaging;

namespace System.Windows.Documents;

internal class WpfPayload
{
	private const string XamlContentType = "application/vnd.ms-wpf.xaml+xml";

	internal const string ImageBmpContentType = "image/bmp";

	private const string ImageGifContentType = "image/gif";

	private const string ImageJpegContentType = "image/jpeg";

	private const string ImageTiffContentType = "image/tiff";

	private const string ImagePngContentType = "image/png";

	private const string ImageBmpFileExtension = ".bmp";

	private const string ImageGifFileExtension = ".gif";

	private const string ImageJpegFileExtension = ".jpeg";

	private const string ImageJpgFileExtension = ".jpg";

	private const string ImageTiffFileExtension = ".tiff";

	private const string ImagePngFileExtension = ".png";

	private const string XamlRelationshipFromPackageToEntryPart = "http://schemas.microsoft.com/wpf/2005/10/xaml/entry";

	private const string XamlRelationshipFromXamlPartToComponentPart = "http://schemas.microsoft.com/wpf/2005/10/xaml/component";

	private const string XamlPayloadDirectory = "/Xaml";

	private const string XamlEntryName = "/Document.xaml";

	private const string XamlImageName = "/Image";

	private static int _wpfPayloadCount;

	private Package _package;

	private List<Image> _images;

	public Package Package => _package;

	private WpfPayload(Package package)
	{
		_package = package;
	}

	internal static string SaveRange(ITextRange range, ref Stream stream, bool useFlowDocumentAsRoot)
	{
		return SaveRange(range, ref stream, useFlowDocumentAsRoot, preserveTextElements: false);
	}

	internal static string SaveRange(ITextRange range, ref Stream stream, bool useFlowDocumentAsRoot, bool preserveTextElements)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		WpfPayload wpfPayload = new WpfPayload(null);
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		TextRangeSerialization.WriteXaml(new XmlTextWriter(stringWriter), range, useFlowDocumentAsRoot, wpfPayload, preserveTextElements);
		string text = stringWriter.ToString();
		if (stream != null || wpfPayload._images != null)
		{
			if (stream == null)
			{
				stream = new MemoryStream();
			}
			using (wpfPayload.CreatePackage(stream))
			{
				PackagePart packagePart = wpfPayload.CreateWpfEntryPart();
				Stream seekableStream = packagePart.GetSeekableStream();
				using (seekableStream)
				{
					StreamWriter streamWriter = new StreamWriter(seekableStream);
					using (streamWriter)
					{
						streamWriter.Write(text);
					}
				}
				wpfPayload.CreateComponentParts(packagePart);
			}
			Invariant.Assert(wpfPayload._images == null);
		}
		return text;
	}

	internal static MemoryStream SaveImage(BitmapSource bitmapSource, string imageContentType)
	{
		MemoryStream memoryStream = new MemoryStream();
		WpfPayload wpfPayload = new WpfPayload(null);
		using (wpfPayload.CreatePackage(memoryStream))
		{
			int imageIndex = 0;
			string imageReference = GetImageReference(GetImageName(imageIndex, imageContentType));
			PackagePart packagePart = wpfPayload.CreateWpfEntryPart();
			Stream seekableStream = packagePart.GetSeekableStream();
			using (seekableStream)
			{
				StreamWriter streamWriter = new StreamWriter(seekableStream);
				using (streamWriter)
				{
					string value = "<Span xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><InlineUIContainer><Image Width=\"" + bitmapSource.Width + "\" Height=\"" + bitmapSource.Height + "\" ><Image.Source><BitmapImage CacheOption=\"OnLoad\" UriSource=\"" + imageReference + "\"/></Image.Source></Image></InlineUIContainer></Span>";
					streamWriter.Write(value);
				}
			}
			wpfPayload.CreateImagePart(packagePart, bitmapSource, imageContentType, imageIndex);
			return memoryStream;
		}
	}

	internal static object LoadElement(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		object result;
		try
		{
			WpfPayload wpfPayload = OpenWpfPayload(stream);
			using (wpfPayload.Package)
			{
				PackagePart packagePart = wpfPayload.ValidatePayload();
				Uri uri = System.IO.Packaging.PackUriHelper.Create(new Uri("payload://wpf" + Interlocked.Increment(ref _wpfPayloadCount), UriKind.Absolute), packagePart.Uri);
				Uri packageUri = System.IO.Packaging.PackUriHelper.GetPackageUri(uri);
				PackageStore.AddPackage(packageUri, wpfPayload.Package);
				result = XamlReader.Load(parserContext: new ParserContext
				{
					BaseUri = uri
				}, stream: packagePart.GetSeekableStream(), useRestrictiveXamlReader: true);
				PackageStore.RemovePackage(packageUri);
			}
		}
		catch (XamlParseException ex)
		{
			Invariant.Assert(ex != null);
			result = null;
		}
		catch (FileFormatException)
		{
			result = null;
		}
		catch (FileLoadException)
		{
			result = null;
		}
		catch (OutOfMemoryException)
		{
			result = null;
		}
		return result;
	}

	private PackagePart ValidatePayload()
	{
		return GetWpfEntryPart() ?? throw new XamlParseException(SR.TextEditorCopyPaste_EntryPartIsMissingInXamlPackage);
	}

	private BitmapSource GetBitmapSourceFromImage(Image image)
	{
		if (image.Source is BitmapSource)
		{
			return (BitmapSource)image.Source;
		}
		Invariant.Assert(image.Source is DrawingImage);
		DpiScale dpi = image.GetDpi();
		DrawingImage drawingImage = (DrawingImage)image.Source;
		RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)(drawingImage.Width * dpi.DpiScaleX), (int)(drawingImage.Height * dpi.DpiScaleY), 96.0, 96.0, PixelFormats.Default);
		renderTargetBitmap.Render(image);
		return renderTargetBitmap;
	}

	private void CreateComponentParts(PackagePart sourcePart)
	{
		if (_images != null)
		{
			for (int i = 0; i < _images.Count; i++)
			{
				Image image = _images[i];
				string imageContentType = GetImageContentType(image.Source.ToString());
				CreateImagePart(sourcePart, GetBitmapSourceFromImage(image), imageContentType, i);
			}
			_images = null;
		}
	}

	private void CreateImagePart(PackagePart sourcePart, BitmapSource imageSource, string imageContentType, int imageIndex)
	{
		string imageName = GetImageName(imageIndex, imageContentType);
		Uri uri = new Uri("/Xaml" + imageName, UriKind.Relative);
		PackagePart packPart = _package.CreatePart(uri, imageContentType, CompressionOption.NotCompressed);
		sourcePart.CreateRelationship(uri, TargetMode.Internal, "http://schemas.microsoft.com/wpf/2005/10/xaml/component");
		BitmapEncoder bitmapEncoder = GetBitmapEncoder(imageContentType);
		bitmapEncoder.Frames.Add(BitmapFrame.Create(imageSource));
		Stream seekableStream = packPart.GetSeekableStream();
		using (seekableStream)
		{
			bitmapEncoder.Save(seekableStream);
		}
	}

	internal string AddImage(Image image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (image.Source == null)
		{
			throw new ArgumentNullException("image.Source");
		}
		if (string.IsNullOrEmpty(image.Source.ToString()))
		{
			throw new ArgumentException(SR.WpfPayload_InvalidImageSource);
		}
		if (_images == null)
		{
			_images = new List<Image>();
		}
		string text = null;
		string imageContentType = GetImageContentType(image.Source.ToString());
		for (int i = 0; i < _images.Count; i++)
		{
			if (ImagesAreIdentical(GetBitmapSourceFromImage(_images[i]), GetBitmapSourceFromImage(image)))
			{
				Invariant.Assert(imageContentType == GetImageContentType(_images[i].Source.ToString()), "Image content types expected to be consistent: " + imageContentType + " vs. " + GetImageContentType(_images[i].Source.ToString()));
				text = GetImageName(i, imageContentType);
			}
		}
		if (text == null)
		{
			text = GetImageName(_images.Count, imageContentType);
			_images.Add(image);
		}
		return GetImageReference(text);
	}

	private static string GetImageContentType(string imageUriString)
	{
		if (imageUriString.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
		{
			return "image/bmp";
		}
		if (imageUriString.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
		{
			return "image/gif";
		}
		if (imageUriString.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || imageUriString.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
		{
			return "image/jpeg";
		}
		if (imageUriString.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
		{
			return "image/tiff";
		}
		return "image/png";
	}

	private static BitmapEncoder GetBitmapEncoder(string imageContentType)
	{
		switch (imageContentType)
		{
		case "image/bmp":
			return new BmpBitmapEncoder();
		case "image/gif":
			return new GifBitmapEncoder();
		case "image/jpeg":
			return new JpegBitmapEncoder();
		case "image/tiff":
			return new TiffBitmapEncoder();
		case "image/png":
			return new PngBitmapEncoder();
		default:
			Invariant.Assert(condition: false, "Unexpected image content type: " + imageContentType);
			return null;
		}
	}

	private static string GetImageFileExtension(string imageContentType)
	{
		switch (imageContentType)
		{
		case "image/bmp":
			return ".bmp";
		case "image/gif":
			return ".gif";
		case "image/jpeg":
			return ".jpeg";
		case "image/tiff":
			return ".tiff";
		case "image/png":
			return ".png";
		default:
			Invariant.Assert(condition: false, "Unexpected image content type: " + imageContentType);
			return null;
		}
	}

	private static bool ImagesAreIdentical(BitmapSource imageSource1, BitmapSource imageSource2)
	{
		BitmapFrameDecode bitmapFrameDecode = imageSource1 as BitmapFrameDecode;
		BitmapFrameDecode bitmapFrameDecode2 = imageSource2 as BitmapFrameDecode;
		if (bitmapFrameDecode != null && bitmapFrameDecode2 != null && bitmapFrameDecode.Decoder.Frames.Count == 1 && bitmapFrameDecode2.Decoder.Frames.Count == 1 && bitmapFrameDecode.Decoder.Frames[0] == bitmapFrameDecode2.Decoder.Frames[0])
		{
			return true;
		}
		if (imageSource1.Format.BitsPerPixel != imageSource2.Format.BitsPerPixel || imageSource1.PixelWidth != imageSource2.PixelWidth || imageSource1.PixelHeight != imageSource2.PixelHeight || imageSource1.DpiX != imageSource2.DpiX || imageSource1.DpiY != imageSource2.DpiY || imageSource1.Palette != imageSource2.Palette)
		{
			return false;
		}
		int num = (imageSource1.PixelWidth * imageSource1.Format.BitsPerPixel + 7) / 8;
		int num2 = num * (imageSource1.PixelHeight - 1) + num;
		byte[] array = new byte[num2];
		byte[] array2 = new byte[num2];
		imageSource1.CopyPixels(array, num, 0);
		imageSource2.CopyPixels(array2, num, 0);
		for (int i = 0; i < num2; i++)
		{
			if (array[i] != array2[i])
			{
				return false;
			}
		}
		return true;
	}

	internal Stream CreateXamlStream()
	{
		return CreateWpfEntryPart().GetSeekableStream();
	}

	internal Stream CreateImageStream(int imageCount, string contentType, out string imagePartUriString)
	{
		imagePartUriString = GetImageName(imageCount, contentType);
		Uri partUri = new Uri("/Xaml" + imagePartUriString, UriKind.Relative);
		PackagePart packPart = _package.CreatePart(partUri, contentType, CompressionOption.NotCompressed);
		imagePartUriString = GetImageReference(imagePartUriString);
		return packPart.GetSeekableStream();
	}

	internal Stream GetImageStream(string imageSourceString)
	{
		Invariant.Assert(imageSourceString.StartsWith("./", StringComparison.OrdinalIgnoreCase));
		imageSourceString = imageSourceString.Substring(1);
		Uri partUri = new Uri("/Xaml" + imageSourceString, UriKind.Relative);
		return _package.GetPart(partUri).GetSeekableStream();
	}

	private Package CreatePackage(Stream stream)
	{
		Invariant.Assert(_package == null, "Package has been already created or open for this WpfPayload");
		_package = Package.Open(stream, FileMode.Create, FileAccess.ReadWrite);
		return _package;
	}

	internal static WpfPayload CreateWpfPayload(Stream stream)
	{
		return new WpfPayload(Package.Open(stream, FileMode.Create, FileAccess.ReadWrite));
	}

	internal static WpfPayload OpenWpfPayload(Stream stream)
	{
		return new WpfPayload(Package.Open(stream, FileMode.Open, FileAccess.Read));
	}

	private PackagePart CreateWpfEntryPart()
	{
		Uri uri = new Uri("/Xaml/Document.xaml", UriKind.Relative);
		PackagePart result = _package.CreatePart(uri, "application/vnd.ms-wpf.xaml+xml", CompressionOption.Normal);
		_package.CreateRelationship(uri, TargetMode.Internal, "http://schemas.microsoft.com/wpf/2005/10/xaml/entry");
		return result;
	}

	private PackagePart GetWpfEntryPart()
	{
		PackagePart result = null;
		PackageRelationshipCollection relationshipsByType = _package.GetRelationshipsByType("http://schemas.microsoft.com/wpf/2005/10/xaml/entry");
		PackageRelationship packageRelationship = null;
		using (IEnumerator<PackageRelationship> enumerator = relationshipsByType.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				packageRelationship = enumerator.Current;
			}
		}
		if (packageRelationship != null)
		{
			Uri targetUri = packageRelationship.TargetUri;
			result = _package.GetPart(targetUri);
		}
		return result;
	}

	private static string GetImageName(int imageIndex, string imageContentType)
	{
		string imageFileExtension = GetImageFileExtension(imageContentType);
		return "/Image" + (imageIndex + 1) + imageFileExtension;
	}

	private static string GetImageReference(string imageName)
	{
		return "." + imageName;
	}
}
