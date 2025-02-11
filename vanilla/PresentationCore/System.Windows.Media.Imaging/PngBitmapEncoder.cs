using MS.Internal;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Defines an encoder that is used to encode Portable Network Graphics (PNG) format images. </summary>
public sealed class PngBitmapEncoder : BitmapEncoder
{
	private Guid _containerFormat = MILGuidData.GUID_ContainerFormatPng;

	private const PngInterlaceOption c_defaultInterlaceOption = PngInterlaceOption.Default;

	private PngInterlaceOption _interlaceOption;

	/// <summary>Gets or sets a value that indicates whether the Portable Network Graphics (PNG) bitmap should interlace.</summary>
	/// <returns>One of the <see cref="P:System.Windows.Media.Imaging.PngBitmapEncoder.Interlace" /> values. The default is <see cref="F:System.Windows.Media.Imaging.PngInterlaceOption.Default" />.</returns>
	public PngInterlaceOption Interlace
	{
		get
		{
			return _interlaceOption;
		}
		set
		{
			_interlaceOption = value;
		}
	}

	internal override Guid ContainerFormat => _containerFormat;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.PngBitmapEncoder" /> class.</summary>
	public PngBitmapEncoder()
		: base(isBuiltIn: true)
	{
		_supportsPreview = false;
		_supportsGlobalThumbnail = false;
		_supportsGlobalMetadata = false;
		_supportsFrameThumbnails = false;
		_supportsMultipleFrames = false;
		_supportsFrameMetadata = true;
	}

	internal override void SetupFrame(SafeMILHandle frameEncodeHandle, SafeMILHandle encoderOptions)
	{
		PROPBAG2 propBag = default(PROPBAG2);
		PROPVARIANT propValue = default(PROPVARIANT);
		if (_interlaceOption != 0)
		{
			try
			{
				propBag.Init("InterlaceOption");
				propValue.Init(_interlaceOption == PngInterlaceOption.On);
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
