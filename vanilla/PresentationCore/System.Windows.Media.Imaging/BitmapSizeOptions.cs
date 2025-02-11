using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Defines size-related attributes of a cached bitmap image. A bitmap is scaled based on values that are defined by this class.</summary>
public class BitmapSizeOptions
{
	private bool _preservesAspectRatio;

	private int _pixelWidth;

	private int _pixelHeight;

	private Rotation _rotationAngle;

	/// <summary>Gets a value that determines whether the aspect ratio of the original bitmap image is preserved.</summary>
	/// <returns>true if the original aspect ratio is maintained; otherwise, false.</returns>
	public bool PreservesAspectRatio => _preservesAspectRatio;

	/// <summary>The width, in pixels, of the bitmap image.</summary>
	/// <returns>The width of the bitmap.</returns>
	public int PixelWidth => _pixelWidth;

	/// <summary>The height, in pixels, of the bitmap image.</summary>
	/// <returns>The height of the bitmap.</returns>
	public int PixelHeight => _pixelHeight;

	/// <summary>Gets a value that represents the rotation angle that is applied to a bitmap. </summary>
	/// <returns>The rotation angle that is applied to the image.</returns>
	public Rotation Rotation => _rotationAngle;

	internal bool DoesScale
	{
		get
		{
			if (_pixelWidth == 0)
			{
				return _pixelHeight != 0;
			}
			return true;
		}
	}

	internal WICBitmapTransformOptions WICTransformOptions
	{
		get
		{
			WICBitmapTransformOptions result = WICBitmapTransformOptions.WICBitmapTransformRotate0;
			switch (_rotationAngle)
			{
			case Rotation.Rotate0:
				result = WICBitmapTransformOptions.WICBitmapTransformRotate0;
				break;
			case Rotation.Rotate90:
				result = WICBitmapTransformOptions.WICBitmapTransformRotate90;
				break;
			case Rotation.Rotate180:
				result = WICBitmapTransformOptions.WICBitmapTransformRotate180;
				break;
			case Rotation.Rotate270:
				result = WICBitmapTransformOptions.WICBitmapTransformRotate270;
				break;
			}
			return result;
		}
	}

	private BitmapSizeOptions()
	{
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Imaging.BitmapSizeOptions" /> with empty sizing properties.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapSizeOptions" />.</returns>
	public static BitmapSizeOptions FromEmptyOptions()
	{
		return new BitmapSizeOptions
		{
			_rotationAngle = Rotation.Rotate0,
			_preservesAspectRatio = true,
			_pixelHeight = 0,
			_pixelWidth = 0
		};
	}

	/// <summary>Initializes an instance of <see cref="T:System.Windows.Media.Imaging.BitmapSizeOptions" /> that preserves the aspect ratio of the source bitmap and specifies an initial <see cref="P:System.Windows.Media.Imaging.BitmapSizeOptions.PixelHeight" />.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapSizeOptions" />.</returns>
	/// <param name="pixelHeight">The height, in pixels, of the resulting bitmap.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Occurs when <paramref name="pixelHeight" /> is less than zero.</exception>
	public static BitmapSizeOptions FromHeight(int pixelHeight)
	{
		if (pixelHeight <= 0)
		{
			throw new ArgumentOutOfRangeException("pixelHeight", SR.ParameterMustBeGreaterThanZero);
		}
		return new BitmapSizeOptions
		{
			_rotationAngle = Rotation.Rotate0,
			_preservesAspectRatio = true,
			_pixelHeight = pixelHeight,
			_pixelWidth = 0
		};
	}

	/// <summary>Initializes an instance of <see cref="T:System.Windows.Media.Imaging.BitmapSizeOptions" /> that preserves the aspect ratio of the source bitmap and specifies an initial <see cref="P:System.Windows.Media.Imaging.BitmapSizeOptions.PixelWidth" />.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.Imaging.BitmapSizeOptions" />.</returns>
	/// <param name="pixelWidth">The width, in pixels, of the resulting bitmap.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Occurs when <paramref name="pixelWidth" /> is less than zero.</exception>
	public static BitmapSizeOptions FromWidth(int pixelWidth)
	{
		if (pixelWidth <= 0)
		{
			throw new ArgumentOutOfRangeException("pixelWidth", SR.ParameterMustBeGreaterThanZero);
		}
		return new BitmapSizeOptions
		{
			_rotationAngle = Rotation.Rotate0,
			_preservesAspectRatio = true,
			_pixelWidth = pixelWidth,
			_pixelHeight = 0
		};
	}

	/// <summary>Initializes an instance of <see cref="T:System.Windows.Media.Imaging.BitmapSizeOptions" /> that does not preserve the original bitmap aspect ratio.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Imaging.BitmapSizeOptions" />.</returns>
	/// <param name="pixelWidth">The width, in pixels, of the resulting bitmap.</param>
	/// <param name="pixelHeight">The height, in pixels, of the resulting bitmap.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Occurs when <paramref name="pixelWidth" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Occurs when <paramref name="pixelHeight" /> is less than zero.</exception>
	public static BitmapSizeOptions FromWidthAndHeight(int pixelWidth, int pixelHeight)
	{
		if (pixelWidth <= 0)
		{
			throw new ArgumentOutOfRangeException("pixelWidth", SR.ParameterMustBeGreaterThanZero);
		}
		if (pixelHeight <= 0)
		{
			throw new ArgumentOutOfRangeException("pixelHeight", SR.ParameterMustBeGreaterThanZero);
		}
		return new BitmapSizeOptions
		{
			_rotationAngle = Rotation.Rotate0,
			_preservesAspectRatio = false,
			_pixelWidth = pixelWidth,
			_pixelHeight = pixelHeight
		};
	}

	/// <summary>Initializes an instance of <see cref="T:System.Windows.Media.Imaging.BitmapSizeOptions" /> that preserves the aspect ratio of the source bitmap and specifies an initial <see cref="T:System.Windows.Media.Imaging.Rotation" /> to apply.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Imaging.BitmapSizeOptions" />.</returns>
	/// <param name="rotation">The initial rotation value to apply. Only 90 degree increments are supported.</param>
	public static BitmapSizeOptions FromRotation(Rotation rotation)
	{
		if ((uint)rotation > 3u)
		{
			throw new ArgumentException(SR.Image_SizeOptionsAngle, "rotation");
		}
		return new BitmapSizeOptions
		{
			_rotationAngle = rotation,
			_preservesAspectRatio = true,
			_pixelWidth = 0,
			_pixelHeight = 0
		};
	}

	internal void GetScaledWidthAndHeight(uint width, uint height, out uint newWidth, out uint newHeight)
	{
		if (_pixelWidth == 0 && _pixelHeight != 0)
		{
			newWidth = (uint)(_pixelHeight * width / height);
			newHeight = (uint)_pixelHeight;
		}
		else if (_pixelWidth != 0 && _pixelHeight == 0)
		{
			newWidth = (uint)_pixelWidth;
			newHeight = (uint)(_pixelWidth * height / width);
		}
		else if (_pixelWidth != 0 && _pixelHeight != 0)
		{
			newWidth = (uint)_pixelWidth;
			newHeight = (uint)_pixelHeight;
		}
		else
		{
			newWidth = width;
			newHeight = height;
		}
	}
}
