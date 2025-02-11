using MS.Internal;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Defines an encoder that is used to encode Tagged Image File Format (TIFF) format images.</summary>
public sealed class TiffBitmapEncoder : BitmapEncoder
{
	private Guid _containerFormat = MILGuidData.GUID_ContainerFormatTiff;

	private const TiffCompressOption c_defaultCompressionMethod = TiffCompressOption.Default;

	private TiffCompressOption _compressionMethod;

	/// <summary>Gets or sets a value that indicates the type of compression that is used by this Tagged Image File Format (TIFF) image.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Media.Imaging.TiffCompressOption" /> values. The default is <see cref="F:System.Windows.Media.Imaging.TiffCompressOption.Default" />.</returns>
	public TiffCompressOption Compression
	{
		get
		{
			return _compressionMethod;
		}
		set
		{
			_compressionMethod = value;
		}
	}

	internal override Guid ContainerFormat => _containerFormat;

	internal override bool IsMetadataFixedSize => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.TiffBitmapEncoder" /> class.</summary>
	public TiffBitmapEncoder()
		: base(isBuiltIn: true)
	{
		_supportsPreview = false;
		_supportsGlobalThumbnail = false;
		_supportsGlobalMetadata = false;
		_supportsFrameThumbnails = true;
		_supportsMultipleFrames = true;
		_supportsFrameMetadata = true;
	}

	internal override void SetupFrame(SafeMILHandle frameEncodeHandle, SafeMILHandle encoderOptions)
	{
		PROPBAG2 propBag = default(PROPBAG2);
		PROPVARIANT propValue = default(PROPVARIANT);
		if (_compressionMethod != 0)
		{
			try
			{
				propBag.Init("TiffCompressionMethod");
				propValue.Init((byte)_compressionMethod);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		HRESULT.Check(UnsafeNativeMethods.WICBitmapFrameEncode.Initialize(frameEncodeHandle, encoderOptions));
	}

	internal override void SealObject()
	{
		throw new NotImplementedException();
	}
}
