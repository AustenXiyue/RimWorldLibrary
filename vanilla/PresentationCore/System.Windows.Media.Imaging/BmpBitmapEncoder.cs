using MS.Internal;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Defines an encoder that is used to encode bitmap (BMP) format images.</summary>
public sealed class BmpBitmapEncoder : BitmapEncoder
{
	private Guid _containerFormat = MILGuidData.GUID_ContainerFormatBmp;

	internal override Guid ContainerFormat => _containerFormat;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.BmpBitmapEncoder" /> class.</summary>
	public BmpBitmapEncoder()
		: base(isBuiltIn: true)
	{
		_supportsPreview = false;
		_supportsGlobalThumbnail = false;
		_supportsGlobalMetadata = false;
		_supportsFrameThumbnails = false;
		_supportsMultipleFrames = false;
		_supportsFrameMetadata = false;
	}

	internal override void SetupFrame(SafeMILHandle frameEncodeHandle, SafeMILHandle encoderOptions)
	{
		HRESULT.Check(UnsafeNativeMethods.WICBitmapFrameEncode.Initialize(frameEncodeHandle, encoderOptions));
	}

	internal override void SealObject()
	{
		throw new NotImplementedException();
	}
}
