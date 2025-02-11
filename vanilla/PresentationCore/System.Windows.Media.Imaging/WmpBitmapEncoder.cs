using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Defines an encoder that is used to encode Microsoft Windows Media Photo images.</summary>
public sealed class WmpBitmapEncoder : BitmapEncoder
{
	private Guid _containerFormat = MILGuidData.GUID_ContainerFormatWmp;

	private const bool c_defaultLossless = false;

	private bool _lossless;

	private const float c_defaultImageQualityLevel = 0.9f;

	private float _imagequalitylevel = 0.9f;

	private const WICBitmapTransformOptions c_defaultTransformation = WICBitmapTransformOptions.WICBitmapTransformRotate0;

	private WICBitmapTransformOptions _transformation;

	private const bool c_defaultUseCodecOptions = false;

	private bool _usecodecoptions;

	private const byte c_defaultQualityLevel = 10;

	private byte _qualitylevel = 10;

	private const byte c_defaultSubsamplingLevel = 3;

	private byte _subsamplinglevel = 3;

	private const byte c_defaultOverlapLevel = 1;

	private byte _overlaplevel = 1;

	private const short c_defaultHorizontalTileSlices = 0;

	private short _horizontaltileslices;

	private const short c_defaultVerticalTileSlices = 0;

	private short _verticaltileslices;

	private const bool c_defaultFrequencyOrder = true;

	private bool _frequencyorder = true;

	private const bool c_defaultInterleavedAlpha = false;

	private bool _interleavedalpha;

	private const byte c_defaultAlphaQualityLevel = 1;

	private byte _alphaqualitylevel = 1;

	private const bool c_defaultCompressedDomainTranscode = true;

	private bool _compresseddomaintranscode = true;

	private const byte c_defaultImageDataDiscardLevel = 0;

	private byte _imagedatadiscardlevel;

	private const byte c_defaultAlphaDataDiscardLevel = 0;

	private byte _alphadatadiscardlevel;

	private const bool c_defaultIgnoreOverlap = false;

	private bool _ignoreoverlap;

	/// <summary>Gets or sets the image quality level.</summary>
	/// <returns>The image quality level. The range is 0 to 1.0 (lossless image quality). The default is 0.9.</returns>
	public float ImageQualityLevel
	{
		get
		{
			return _imagequalitylevel;
		}
		set
		{
			if ((double)value < 0.0 || (double)value > 1.0)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.ParameterMustBeBetween, 0.0, 1.0));
			}
			_imagequalitylevel = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to encode using lossless compression.</summary>
	/// <returns>true to use lossless compression; otherwise, false. The default is false.</returns>
	public bool Lossless
	{
		get
		{
			return _lossless;
		}
		set
		{
			_lossless = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Imaging.Rotation" /> of the image.</summary>
	/// <returns>The rotation of the image.</returns>
	public Rotation Rotation
	{
		get
		{
			if (Rotate90)
			{
				return Rotation.Rotate90;
			}
			if (Rotate180)
			{
				return Rotation.Rotate180;
			}
			if (Rotate270)
			{
				return Rotation.Rotate270;
			}
			return Rotation.Rotate0;
		}
		set
		{
			Rotate90 = false;
			Rotate180 = false;
			Rotate270 = false;
			switch (value)
			{
			case Rotation.Rotate90:
				Rotate90 = true;
				break;
			case Rotation.Rotate180:
				Rotate180 = true;
				break;
			case Rotation.Rotate270:
				Rotate270 = true;
				break;
			case Rotation.Rotate0:
				break;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether to flip the image horizontally.</summary>
	/// <returns>true if the image is to be flipped horizontally; otherwise, false.</returns>
	public bool FlipHorizontal
	{
		get
		{
			return Convert.ToBoolean((int)(_transformation & WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal));
		}
		set
		{
			if (value != FlipHorizontal)
			{
				if (value)
				{
					_transformation |= WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal;
				}
				else
				{
					_transformation &= (WICBitmapTransformOptions)(-9);
				}
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to flip the image vertically.</summary>
	/// <returns>true if the image is to be flipped vertically; otherwise, false.</returns>
	public bool FlipVertical
	{
		get
		{
			return Convert.ToBoolean((int)(_transformation & WICBitmapTransformOptions.WICBitmapTransformFlipVertical));
		}
		set
		{
			if (value != FlipVertical)
			{
				if (value)
				{
					_transformation |= WICBitmapTransformOptions.WICBitmapTransformFlipVertical;
				}
				else
				{
					_transformation &= (WICBitmapTransformOptions)(-17);
				}
			}
		}
	}

	/// <summary>Gets or sets a value that indicates codec options are to be used.</summary>
	/// <returns>true if codec options are to be used; otherwise, false. The default is false.</returns>
	public bool UseCodecOptions
	{
		get
		{
			return _usecodecoptions;
		}
		set
		{
			_usecodecoptions = value;
		}
	}

	/// <summary>Gets or sets the compression quality for the main image.</summary>
	/// <returns>The compression quality for the main image. A value of 1 is considered lossless, and higher values indicate a high compression ratio and lower image quality. The range is 1 to 255. The default is 1.</returns>
	public byte QualityLevel
	{
		get
		{
			return _qualitylevel;
		}
		set
		{
			if (value < 1 || value > byte.MaxValue)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.ParameterMustBeBetween, 1, 255));
			}
			_qualitylevel = value;
		}
	}

	/// <summary>Gets or sets the sub-sampling level for RGB image encoding.</summary>
	/// <returns>The sub-sampling level for RGB image encoding. The range is 0 to 3. The default is 3.ValueDescription04:0:0 encoding. Chroma content is discarded; luminance is preserved.14:2:0 encoding. Chroma resolution is reduced by 1/4 of luminance resolution.24:2:2 encoding. Chroma resolution is reduced to 1/2 of luminance resolution.34:4:4 encoding. Chroma resolution is preserved.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value given is not between 0 and 3.</exception>
	public byte SubsamplingLevel
	{
		get
		{
			return _subsamplinglevel;
		}
		set
		{
			if (value < 0 || value > 3)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.ParameterMustBeBetween, 0, 3));
			}
			_subsamplinglevel = value;
		}
	}

	/// <summary>Gets or sets the overlap processing level.</summary>
	/// <returns>The overlap processing level. The default is 1.ValueDescription0No overlap processing is enabled.1One level of overlap processing is enabled. Encoded values of 4x4 blocks are modified based on the values of neighboring blocks.2Two levels of overlap processing are enabled. In addition to the first level of processing, encoded values of 16x16 macro blocks are modified based on the values of neighboring macro blocks.</returns>
	public byte OverlapLevel
	{
		get
		{
			return _overlaplevel;
		}
		set
		{
			if (value < 0 || value > 2)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.ParameterMustBeBetween, 0, 2));
			}
			_overlaplevel = value;
		}
	}

	/// <summary>Gets or sets the number of horizontal divisions to use during compression encoding. A single division creates two horizontal regions.</summary>
	/// <returns>The number of horizontal divisions to use during compression encoding. The value range is 0 to 4095. The default is 0.</returns>
	public short HorizontalTileSlices
	{
		get
		{
			return _horizontaltileslices;
		}
		set
		{
			if (value < 0 || value > 4096)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.ParameterMustBeBetween, 0, 4096));
			}
			_horizontaltileslices = value;
		}
	}

	/// <summary>Gets or sets the number of vertical divisions to use during compression encoding. A single division creates two vertical regions.</summary>
	/// <returns>The number of vertical divisions to use during compression encoding. The value range is 0 to 4095. The default is 0.</returns>
	public short VerticalTileSlices
	{
		get
		{
			return _verticaltileslices;
		}
		set
		{
			if (value < 0 || value > 4096)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.ParameterMustBeBetween, 0, 4096));
			}
			_verticaltileslices = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to encoding in frequency order.</summary>
	/// <returns>true to encode the image in frequency order; false to encode the image by its spatial orientation. The default is true.</returns>
	public bool FrequencyOrder
	{
		get
		{
			return _frequencyorder;
		}
		set
		{
			_frequencyorder = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to encode the alpha channel data as an additional interleaved channel.</summary>
	/// <returns>true if the image is encoded with an additional interleaved alpha channel; false if planar alpha channel is used. The default is false.</returns>
	public bool InterleavedAlpha
	{
		get
		{
			return _interleavedalpha;
		}
		set
		{
			_interleavedalpha = value;
		}
	}

	/// <summary>Gets or sets the compression quality for a planar alpha channel.</summary>
	/// <returns>The compression quality for a planar alpha channel image. A value of 1 is considered lossless, and increasing values result in higher compression ratios and lower image quality. The value range is 1 to 255. The default is 1.</returns>
	public byte AlphaQualityLevel
	{
		get
		{
			return _alphaqualitylevel;
		}
		set
		{
			if (value < 0 || value > byte.MaxValue)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.ParameterMustBeBetween, 0, 255));
			}
			_alphaqualitylevel = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether compressed domain operations can be used. Compressed domain operations are transformation operations that are done without decoding the image data.</summary>
	/// <returns>true if compressed domain operations can be used; otherwise, false. The default is true.</returns>
	public bool CompressedDomainTranscode
	{
		get
		{
			return _compresseddomaintranscode;
		}
		set
		{
			_compresseddomaintranscode = value;
		}
	}

	/// <summary>Gets or sets the level of image data to discard during a compressed domain transcode.</summary>
	/// <returns>The level of image data to discard during a compressed domain encoding of the image. The value range is 0 (no data discarded) to 3 (HighPass and LowPass discarded). The default is 1.ValueDescription0No image frequency data is discarded.1FlexBits are discarded. The image quality of the image is reduced without changing the effective resolution of the image.2HighPass frequency data band is discarded. The image's effective resolution is reduced by a factor of 4 in both dimensions.3HighPass and LowPass frequency data bands are discarded. The image's effective resolution is reduced by a factor of 16 in both dimensions.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value given is not between 0 and 3.</exception>
	public byte ImageDataDiscardLevel
	{
		get
		{
			return _imagedatadiscardlevel;
		}
		set
		{
			if (value < 0 || value > 3)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.ParameterMustBeBetween, 0, 3));
			}
			_imagedatadiscardlevel = value;
		}
	}

	/// <summary>Gets or sets the level of alpha frequency data to discard during a compressed domain transcode.</summary>
	/// <returns>The level of alpha data to discard when encoding the image. The value range is 0 (no data discarded) to 4 (alpha channel completely discarded). The default is 1.ValueDescription0No image frequency data is discarded.1FlexBits are discarded. The image quality of the image is reduced without changing the effective resolution of the image.2HighPass frequency data band is discarded. The image's effective resolution is reduced by a factor of 4 in both dimensions.3HighPass and LowPass frequency data bands are discarded. The image's effective resolution is reduced by a factor of 16 in both dimensions.4The alpha channel is completely discarded. The pixel format is changed to reflect the removal of the alpha channel.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value given is not between 0 and 4.</exception>
	public byte AlphaDataDiscardLevel
	{
		get
		{
			return _alphadatadiscardlevel;
		}
		set
		{
			if (value < 0 || value > 4)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.ParameterMustBeBetween, 0, 4));
			}
			_alphadatadiscardlevel = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether to ignore region overlap pixels in subregion compressed domain encoding. This feature is not currently implemented.</summary>
	/// <returns>true if overlap is ignored; otherwise, false. The default is false.</returns>
	public bool IgnoreOverlap
	{
		get
		{
			return _ignoreoverlap;
		}
		set
		{
			_ignoreoverlap = value;
		}
	}

	private bool Rotate90
	{
		get
		{
			if (Convert.ToBoolean((int)(_transformation & WICBitmapTransformOptions.WICBitmapTransformRotate90)))
			{
				return !Rotate270;
			}
			return false;
		}
		set
		{
			if (value != Rotate90)
			{
				bool flipHorizontal = FlipHorizontal;
				bool flipVertical = FlipVertical;
				if (value)
				{
					_transformation = WICBitmapTransformOptions.WICBitmapTransformRotate90;
				}
				else
				{
					_transformation = WICBitmapTransformOptions.WICBitmapTransformRotate0;
				}
				FlipHorizontal = flipHorizontal;
				FlipVertical = flipVertical;
			}
		}
	}

	private bool Rotate180
	{
		get
		{
			if (Convert.ToBoolean((int)(_transformation & WICBitmapTransformOptions.WICBitmapTransformRotate180)))
			{
				return !Rotate270;
			}
			return false;
		}
		set
		{
			if (value != Rotate180)
			{
				bool flipHorizontal = FlipHorizontal;
				bool flipVertical = FlipVertical;
				if (value)
				{
					_transformation = WICBitmapTransformOptions.WICBitmapTransformRotate180;
				}
				else
				{
					_transformation = WICBitmapTransformOptions.WICBitmapTransformRotate0;
				}
				FlipHorizontal = flipHorizontal;
				FlipVertical = flipVertical;
			}
		}
	}

	private bool Rotate270
	{
		get
		{
			return Convert.ToBoolean((_transformation & WICBitmapTransformOptions.WICBitmapTransformRotate270) == WICBitmapTransformOptions.WICBitmapTransformRotate270);
		}
		set
		{
			if (value != Rotate270)
			{
				bool flipHorizontal = FlipHorizontal;
				bool flipVertical = FlipVertical;
				if (value)
				{
					_transformation = WICBitmapTransformOptions.WICBitmapTransformRotate270;
				}
				else
				{
					_transformation = WICBitmapTransformOptions.WICBitmapTransformRotate0;
				}
				FlipHorizontal = flipHorizontal;
				FlipVertical = flipVertical;
			}
		}
	}

	internal override Guid ContainerFormat => _containerFormat;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.WmpBitmapEncoder" /> class.</summary>
	public WmpBitmapEncoder()
		: base(isBuiltIn: true)
	{
		_supportsPreview = false;
		_supportsGlobalThumbnail = false;
		_supportsGlobalMetadata = false;
		_supportsMultipleFrames = false;
	}

	internal override void SetupFrame(SafeMILHandle frameEncodeHandle, SafeMILHandle encoderOptions)
	{
		PROPBAG2 propBag = default(PROPBAG2);
		PROPVARIANT propValue = default(PROPVARIANT);
		if (_imagequalitylevel != 0.9f)
		{
			try
			{
				propBag.Init("ImageQuality");
				propValue.Init(_imagequalitylevel);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_transformation != 0)
		{
			try
			{
				propBag.Init("BitmapTransform");
				propValue.Init((byte)_transformation);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_lossless)
		{
			try
			{
				propBag.Init("Lossless");
				propValue.Init(_lossless);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_usecodecoptions)
		{
			try
			{
				propBag.Init("UseCodecOptions");
				propValue.Init(_usecodecoptions);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_qualitylevel != 10)
		{
			try
			{
				propBag.Init("Quality");
				propValue.Init(_qualitylevel);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_subsamplinglevel != 3)
		{
			try
			{
				propBag.Init("Subsampling");
				propValue.Init(_subsamplinglevel);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_overlaplevel != 1)
		{
			try
			{
				propBag.Init("Overlap");
				propValue.Init(_overlaplevel);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_horizontaltileslices != 0)
		{
			try
			{
				propBag.Init("HorizontalTileSlices");
				propValue.Init((ushort)_horizontaltileslices);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_verticaltileslices != 0)
		{
			try
			{
				propBag.Init("VerticalTileSlices");
				propValue.Init((ushort)_verticaltileslices);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (!_frequencyorder)
		{
			try
			{
				propBag.Init("FrequencyOrder");
				propValue.Init(_frequencyorder);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_interleavedalpha)
		{
			try
			{
				propBag.Init("InterleavedAlpha");
				propValue.Init(_interleavedalpha);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_alphaqualitylevel != 1)
		{
			try
			{
				propBag.Init("AlphaQuality");
				propValue.Init(_alphaqualitylevel);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (!_compresseddomaintranscode)
		{
			try
			{
				propBag.Init("CompressedDomainTranscode");
				propValue.Init(_compresseddomaintranscode);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_imagedatadiscardlevel != 0)
		{
			try
			{
				propBag.Init("ImageDataDiscard");
				propValue.Init(_imagedatadiscardlevel);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_alphadatadiscardlevel != 0)
		{
			try
			{
				propBag.Init("AlphaDataDiscard");
				propValue.Init(_alphadatadiscardlevel);
				HRESULT.Check(UnsafeNativeMethods.IPropertyBag2.Write(encoderOptions, 1u, ref propBag, ref propValue));
			}
			finally
			{
				propBag.Clear();
				propValue.Clear();
			}
		}
		if (_ignoreoverlap)
		{
			try
			{
				propBag.Init("IgnoreOverlap");
				propValue.Init(_ignoreoverlap);
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
