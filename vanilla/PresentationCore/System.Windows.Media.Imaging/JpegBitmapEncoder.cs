using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Defines an encoder that is used to encode Joint Photographics Experts Group (JPEG) format images.</summary>
public sealed class JpegBitmapEncoder : BitmapEncoder
{
	private Guid _containerFormat = MILGuidData.GUID_ContainerFormatJpeg;

	private const int c_defaultQualityLevel = 75;

	private int _qualityLevel = 75;

	private const WICBitmapTransformOptions c_defaultTransformation = WICBitmapTransformOptions.WICBitmapTransformRotate0;

	private WICBitmapTransformOptions _transformation;

	/// <summary>Gets or sets a value that indicates the quality level of the resulting Joint Photographics Experts Group (JPEG) image.</summary>
	/// <returns>The quality level of the JPEG image. The value range is 1 (lowest quality) to 100 (highest quality) inclusive. </returns>
	public int QualityLevel
	{
		get
		{
			return _qualityLevel;
		}
		set
		{
			if (value < 1 || value > 100)
			{
				throw new ArgumentOutOfRangeException("value", SR.Format(SR.ParameterMustBeBetween, 1, 100));
			}
			_qualityLevel = value;
		}
	}

	/// <summary>Gets or sets a value that represents the degree to which a Joint Photographics Experts Group (JPEG) image is rotated.</summary>
	/// <returns>The degree to which the image is rotated.</returns>
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

	/// <summary>Gets or sets a value that indicates whether a Joint Photographics Experts Group (JPEG) image should be flipped horizontally during encoding.</summary>
	/// <returns>true if the image is flipped horizontally during encoding; otherwise, false.</returns>
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

	/// <summary>Gets or sets a value that indicates whether a Joint Photographics Experts Group (JPEG) image should be flipped vertically during encoding.</summary>
	/// <returns>true if the image is flipped vertically during encoding; otherwise, false.</returns>
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

	internal override Guid ContainerFormat => _containerFormat;

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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.JpegBitmapEncoder" /> class.</summary>
	public JpegBitmapEncoder()
		: base(isBuiltIn: true)
	{
		_supportsPreview = false;
		_supportsGlobalThumbnail = false;
		_supportsGlobalMetadata = false;
		_supportsFrameThumbnails = true;
		_supportsMultipleFrames = false;
		_supportsFrameMetadata = true;
	}

	internal override void SetupFrame(SafeMILHandle frameEncodeHandle, SafeMILHandle encoderOptions)
	{
		PROPBAG2 propBag = default(PROPBAG2);
		PROPVARIANT propValue = default(PROPVARIANT);
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
		if (_qualityLevel != 75)
		{
			try
			{
				propBag.Init("ImageQuality");
				propValue.Init((float)_qualityLevel / 100f);
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
