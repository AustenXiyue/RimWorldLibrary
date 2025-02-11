using System.Windows.Media;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

internal class TextServicesDisplayAttribute
{
	private const int AlphaShift = 24;

	private const int RedShift = 16;

	private const int GreenShift = 8;

	private const int BlueShift = 0;

	private const int Win32RedShift = 0;

	private const int Win32GreenShift = 8;

	private const int Win32BlueShift = 16;

	private MS.Win32.UnsafeNativeMethods.TF_DISPLAYATTRIBUTE _attr;

	internal MS.Win32.UnsafeNativeMethods.TF_DA_LINESTYLE LineStyle => _attr.lsStyle;

	internal bool IsBoldLine => _attr.fBoldLine;

	internal MS.Win32.UnsafeNativeMethods.TF_DA_ATTR_INFO AttrInfo => _attr.bAttr;

	internal TextServicesDisplayAttribute(MS.Win32.UnsafeNativeMethods.TF_DISPLAYATTRIBUTE attr)
	{
		_attr = attr;
	}

	internal bool IsEmptyAttribute()
	{
		if (_attr.crText.type != 0 || _attr.crBk.type != 0 || _attr.crLine.type != 0 || _attr.lsStyle != 0)
		{
			return false;
		}
		return true;
	}

	internal void Apply(ITextPointer start, ITextPointer end)
	{
	}

	internal static Color GetColor(MS.Win32.UnsafeNativeMethods.TF_DA_COLOR dacolor, ITextPointer position)
	{
		if (dacolor.type == MS.Win32.UnsafeNativeMethods.TF_DA_COLORTYPE.TF_CT_SYSCOLOR)
		{
			return GetSystemColor(dacolor.indexOrColorRef);
		}
		if (dacolor.type == MS.Win32.UnsafeNativeMethods.TF_DA_COLORTYPE.TF_CT_COLORREF)
		{
			uint num = (uint)FromWin32Value(dacolor.indexOrColorRef);
			return Color.FromArgb((byte)((num & 0xFF000000u) >> 24), (byte)((num & 0xFF0000) >> 16), (byte)((num & 0xFF00) >> 8), (byte)(num & 0xFF));
		}
		Invariant.Assert(position != null, "position can't be null");
		return ((SolidColorBrush)position.GetValue(TextElement.ForegroundProperty)).Color;
	}

	internal Color GetLineColor(ITextPointer position)
	{
		return GetColor(_attr.crLine, position);
	}

	private static int Encode(int alpha, int red, int green, int blue)
	{
		return (red << 16) | (green << 8) | blue | (alpha << 24);
	}

	private static int FromWin32Value(int value)
	{
		return Encode(255, value & 0xFF, (value >> 8) & 0xFF, (value >> 16) & 0xFF);
	}

	private static Color GetSystemColor(int index)
	{
		uint num = (uint)FromWin32Value(SafeNativeMethods.GetSysColor(index));
		return Color.FromArgb((byte)((num & 0xFF000000u) >> 24), (byte)((num & 0xFF0000) >> 16), (byte)((num & 0xFF00) >> 8), (byte)(num & 0xFF));
	}
}
