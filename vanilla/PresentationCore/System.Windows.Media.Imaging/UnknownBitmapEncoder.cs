using MS.Internal;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

internal sealed class UnknownBitmapEncoder : BitmapEncoder
{
	private Guid _containerFormat;

	internal override Guid ContainerFormat => _containerFormat;

	public UnknownBitmapEncoder(Guid containerFormat)
		: base(isBuiltIn: true)
	{
		_containerFormat = containerFormat;
		_supportsPreview = true;
		_supportsGlobalThumbnail = true;
		_supportsGlobalMetadata = false;
		_supportsFrameThumbnails = true;
		_supportsMultipleFrames = true;
		_supportsFrameMetadata = true;
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
