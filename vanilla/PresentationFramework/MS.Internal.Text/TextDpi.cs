using System;
using System.Windows;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.Text;

internal static class TextDpi
{
	private const double _scale = 300.0;

	private const int _maxSizeInt = 1073741822;

	private const double _maxSize = 3579139.4066666667;

	private const int _minSizeInt = 1;

	private const double _minSize = 1.0 / 300.0;

	private const double _maxObjSize = 1193046.4688888888;

	internal static double MinWidth => 1.0 / 300.0;

	internal static double MaxWidth => 3579139.4066666667;

	internal static int ToTextDpi(double d)
	{
		if (DoubleUtil.IsZero(d))
		{
			return 0;
		}
		if (d > 0.0)
		{
			if (d > 3579139.4066666667)
			{
				d = 3579139.4066666667;
			}
			else if (d < 1.0 / 300.0)
			{
				d = 1.0 / 300.0;
			}
		}
		else if (d < -3579139.4066666667)
		{
			d = -3579139.4066666667;
		}
		else if (d > -1.0 / 300.0)
		{
			d = -1.0 / 300.0;
		}
		return (int)Math.Round(d * 300.0);
	}

	internal static double FromTextDpi(int i)
	{
		return (double)i / 300.0;
	}

	internal static PTS.FSPOINT ToTextPoint(Point point)
	{
		PTS.FSPOINT result = default(PTS.FSPOINT);
		result.u = ToTextDpi(point.X);
		result.v = ToTextDpi(point.Y);
		return result;
	}

	internal static PTS.FSVECTOR ToTextSize(Size size)
	{
		PTS.FSVECTOR result = default(PTS.FSVECTOR);
		result.du = ToTextDpi(size.Width);
		result.dv = ToTextDpi(size.Height);
		return result;
	}

	internal static Rect FromTextRect(PTS.FSRECT fsrect)
	{
		return new Rect(FromTextDpi(fsrect.u), FromTextDpi(fsrect.v), FromTextDpi(fsrect.du), FromTextDpi(fsrect.dv));
	}

	internal static void EnsureValidLineOffset(ref double offset)
	{
		if (offset > 3579139.4066666667)
		{
			offset = 3579139.4066666667;
		}
		else if (offset < -3579139.4066666667)
		{
			offset = -3579139.4066666667;
		}
	}

	internal static void SnapToTextDpi(ref Size size)
	{
		size = new Size(FromTextDpi(ToTextDpi(size.Width)), FromTextDpi(ToTextDpi(size.Height)));
	}

	internal static void EnsureValidLineWidth(ref double width)
	{
		if (width > 3579139.4066666667)
		{
			width = 3579139.4066666667;
		}
		else if (width < 1.0 / 300.0)
		{
			width = 1.0 / 300.0;
		}
	}

	internal static void EnsureValidLineWidth(ref Size size)
	{
		if (size.Width > 3579139.4066666667)
		{
			size.Width = 3579139.4066666667;
		}
		else if (size.Width < 1.0 / 300.0)
		{
			size.Width = 1.0 / 300.0;
		}
	}

	internal static void EnsureValidLineWidth(ref int width)
	{
		if (width > 1073741822)
		{
			width = 1073741822;
		}
		else if (width < 1)
		{
			width = 1;
		}
	}

	internal static void EnsureValidPageSize(ref Size size)
	{
		if (size.Width > 3579139.4066666667)
		{
			size.Width = 3579139.4066666667;
		}
		else if (size.Width < 1.0 / 300.0)
		{
			size.Width = 1.0 / 300.0;
		}
		if (size.Height > 3579139.4066666667)
		{
			size.Height = 3579139.4066666667;
		}
		else if (size.Height < 1.0 / 300.0)
		{
			size.Height = 1.0 / 300.0;
		}
	}

	internal static void EnsureValidPageWidth(ref double width)
	{
		if (width > 3579139.4066666667)
		{
			width = 3579139.4066666667;
		}
		else if (width < 1.0 / 300.0)
		{
			width = 1.0 / 300.0;
		}
	}

	internal static void EnsureValidPageMargin(ref Thickness pageMargin, Size pageSize)
	{
		if (pageMargin.Left >= pageSize.Width)
		{
			pageMargin.Right = 0.0;
		}
		if (pageMargin.Left + pageMargin.Right >= pageSize.Width)
		{
			pageMargin.Right = Math.Max(0.0, pageSize.Width - pageMargin.Left - 1.0 / 300.0);
			if (pageMargin.Left + pageMargin.Right >= pageSize.Width)
			{
				pageMargin.Left = pageSize.Width - 1.0 / 300.0;
			}
		}
		if (pageMargin.Top >= pageSize.Height)
		{
			pageMargin.Bottom = 0.0;
		}
		if (pageMargin.Top + pageMargin.Bottom >= pageSize.Height)
		{
			pageMargin.Bottom = Math.Max(0.0, pageSize.Height - pageMargin.Top - 1.0 / 300.0);
			if (pageMargin.Top + pageMargin.Bottom >= pageSize.Height)
			{
				pageMargin.Top = pageSize.Height - 1.0 / 300.0;
			}
		}
	}

	internal static void EnsureValidObjSize(ref Size size)
	{
		if (size.Width > 1193046.4688888888)
		{
			size.Width = 1193046.4688888888;
		}
		if (size.Height > 1193046.4688888888)
		{
			size.Height = 1193046.4688888888;
		}
	}
}
