using System.Runtime.InteropServices;

namespace System.Windows.Media.Composition;

[StructLayout(LayoutKind.Explicit)]
internal struct MilRectD
{
	[FieldOffset(0)]
	internal double _left;

	[FieldOffset(8)]
	internal double _top;

	[FieldOffset(16)]
	internal double _right;

	[FieldOffset(24)]
	internal double _bottom;

	internal static MilRectD Empty => new MilRectD(0.0, 0.0, 0.0, 0.0);

	internal static MilRectD NaN => new MilRectD(double.NaN, double.NaN, double.NaN, double.NaN);

	internal Rect AsRect
	{
		get
		{
			if (_right >= _left && _bottom >= _top)
			{
				return new Rect(_left, _top, _right - _left, _bottom - _top);
			}
			return Rect.Empty;
		}
	}

	internal MilRectD(double left, double top, double right, double bottom)
	{
		_left = left;
		_top = top;
		_right = right;
		_bottom = bottom;
	}
}
