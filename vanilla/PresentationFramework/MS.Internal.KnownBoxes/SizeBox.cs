using System;
using System.Windows;

namespace MS.Internal.KnownBoxes;

internal class SizeBox
{
	private double _width;

	private double _height;

	internal double Width
	{
		get
		{
			return _width;
		}
		set
		{
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Rect_WidthAndHeightCannotBeNegative);
			}
			_width = value;
		}
	}

	internal double Height
	{
		get
		{
			return _height;
		}
		set
		{
			if (value < 0.0)
			{
				throw new ArgumentException(SR.Rect_WidthAndHeightCannotBeNegative);
			}
			_height = value;
		}
	}

	internal SizeBox(double width, double height)
	{
		if (width < 0.0 || height < 0.0)
		{
			throw new ArgumentException(SR.Rect_WidthAndHeightCannotBeNegative);
		}
		_width = width;
		_height = height;
	}

	internal SizeBox(Size size)
		: this(size.Width, size.Height)
	{
	}
}
