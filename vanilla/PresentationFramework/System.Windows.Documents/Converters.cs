using System.Globalization;
using System.Text;
using System.Windows.Media;

namespace System.Windows.Documents;

internal static class Converters
{
	internal static double HalfPointToPositivePx(double halfPoint)
	{
		return TwipToPositivePx(halfPoint * 10.0);
	}

	internal static double TwipToPx(double twip)
	{
		return twip / 1440.0 * 96.0;
	}

	internal static double TwipToPositivePx(double twip)
	{
		double num = twip / 1440.0 * 96.0;
		if (num < 0.0)
		{
			num = 0.0;
		}
		return num;
	}

	internal static double TwipToPositiveVisiblePx(double twip)
	{
		double num = twip / 1440.0 * 96.0;
		if (num < 0.0)
		{
			num = 0.0;
		}
		if (twip > 0.0 && num < 1.0)
		{
			num = 1.0;
		}
		return num;
	}

	internal static string TwipToPxString(double twip)
	{
		return TwipToPx(twip).ToString("f2", CultureInfo.InvariantCulture);
	}

	internal static string TwipToPositivePxString(double twip)
	{
		return TwipToPositivePx(twip).ToString("f2", CultureInfo.InvariantCulture);
	}

	internal static string TwipToPositiveVisiblePxString(double twip)
	{
		return TwipToPositiveVisiblePx(twip).ToString("f2", CultureInfo.InvariantCulture);
	}

	internal static double PxToPt(double px)
	{
		return px / 96.0 * 72.0;
	}

	internal static long PxToTwipRounded(double px)
	{
		double num = px / 96.0 * 1440.0;
		if (num < 0.0)
		{
			return (long)(num - 0.5);
		}
		return (long)(num + 0.5);
	}

	internal static long PxToHalfPointRounded(double px)
	{
		double num = px / 96.0 * 1440.0 / 10.0;
		if (num < 0.0)
		{
			return (long)(num - 0.5);
		}
		return (long)(num + 0.5);
	}

	internal static bool StringToDouble(ReadOnlySpan<char> s, ref double d)
	{
		bool result = true;
		d = 0.0;
		try
		{
			d = double.Parse(s, CultureInfo.InvariantCulture);
		}
		catch (OverflowException)
		{
			result = false;
		}
		catch (FormatException)
		{
			result = false;
		}
		return result;
	}

	internal static bool StringToInt(ReadOnlySpan<char> s, ref int i)
	{
		bool result = true;
		i = 0;
		try
		{
			i = int.Parse(s, CultureInfo.InvariantCulture);
		}
		catch (OverflowException)
		{
			result = false;
		}
		catch (FormatException)
		{
			result = false;
		}
		return result;
	}

	internal static string StringToXMLAttribute(string s)
	{
		if (!s.Contains('"'))
		{
			return s;
		}
		return s.Replace("\"", "&quot;");
	}

	internal static bool HexStringToInt(ReadOnlySpan<char> s, ref int i)
	{
		bool result = true;
		i = 0;
		try
		{
			i = int.Parse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
		}
		catch (OverflowException)
		{
			result = false;
		}
		catch (FormatException)
		{
			result = false;
		}
		return result;
	}

	internal static string MarkerStyleToString(MarkerStyle ms)
	{
		return ms switch
		{
			MarkerStyle.MarkerArabic => "Decimal", 
			MarkerStyle.MarkerUpperRoman => "UpperRoman", 
			MarkerStyle.MarkerLowerRoman => "LowerRoman", 
			MarkerStyle.MarkerUpperAlpha => "UpperLatin", 
			MarkerStyle.MarkerLowerAlpha => "LowerLatin", 
			MarkerStyle.MarkerOrdinal => "Decimal", 
			MarkerStyle.MarkerCardinal => "Decimal", 
			MarkerStyle.MarkerHidden => "None", 
			MarkerStyle.MarkerBullet => "Disc", 
			_ => "Decimal", 
		};
	}

	internal static string MarkerStyleToOldRTFString(MarkerStyle ms)
	{
		return ms switch
		{
			MarkerStyle.MarkerBullet => "\\pnlvlblt", 
			MarkerStyle.MarkerCardinal => "\\pnlvlbody\\pncard", 
			MarkerStyle.MarkerUpperAlpha => "\\pnlvlbody\\pnucltr", 
			MarkerStyle.MarkerUpperRoman => "\\pnlvlbody\\pnucrm", 
			MarkerStyle.MarkerLowerAlpha => "\\pnlvlbody\\pnlcltr", 
			MarkerStyle.MarkerLowerRoman => "\\pnlvlbody\\pnlcrm", 
			MarkerStyle.MarkerOrdinal => "\\pnlvlbody\\pnord", 
			_ => "\\pnlvlbody\\pndec", 
		};
	}

	internal static bool ColorToUse(ConverterState converterState, long cb, long cf, long shade, ref Color c)
	{
		ColorTableEntry colorTableEntry = ((cb >= 0) ? converterState.ColorTable.EntryAt((int)cb) : null);
		ColorTableEntry colorTableEntry2 = ((cf >= 0) ? converterState.ColorTable.EntryAt((int)cf) : null);
		if (shade < 0)
		{
			if (colorTableEntry == null)
			{
				return false;
			}
			c = colorTableEntry.Color;
			return true;
		}
		Color color = colorTableEntry?.Color ?? Color.FromArgb(byte.MaxValue, 0, 0, 0);
		Color color2 = colorTableEntry2?.Color ?? Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		if (colorTableEntry2 == null && colorTableEntry == null)
		{
			c = Color.FromArgb(byte.MaxValue, (byte)(255 - 255 * shade / 10000), (byte)(255 - 255 * shade / 10000), (byte)(255 - 255 * shade / 10000));
			return true;
		}
		if (colorTableEntry == null)
		{
			c = Color.FromArgb(byte.MaxValue, (byte)(color2.R + (255 - color2.R) * (10000 - shade) / 10000), (byte)(color2.G + (255 - color2.G) * (10000 - shade) / 10000), (byte)(color2.B + (255 - color2.B) * (10000 - shade) / 10000));
			return true;
		}
		if (colorTableEntry2 == null)
		{
			c = Color.FromArgb(byte.MaxValue, (byte)(color.R - color.R * shade / 10000), (byte)(color.G - color.G * shade / 10000), (byte)(color.B - color.B * shade / 10000));
			return true;
		}
		c = Color.FromArgb(byte.MaxValue, (byte)(color.R * (10000 - shade) / 10000 + color2.R * shade / 10000), (byte)(color.G * (10000 - shade) / 10000 + color2.G * shade / 10000), (byte)(color.B * (10000 - shade) / 10000 + color2.B * shade / 10000));
		return true;
	}

	internal static string AlignmentToString(HAlign a, DirState ds)
	{
		switch (a)
		{
		case HAlign.AlignLeft:
			if (ds == DirState.DirRTL)
			{
				return "Right";
			}
			return "Left";
		case HAlign.AlignRight:
			if (ds == DirState.DirRTL)
			{
				return "Left";
			}
			return "Right";
		case HAlign.AlignCenter:
			return "Center";
		case HAlign.AlignJustify:
			return "Justify";
		default:
			return "";
		}
	}

	internal static string MarkerCountToString(MarkerStyle ms, long nCount)
	{
		StringBuilder sb = new StringBuilder();
		if (nCount < 0)
		{
			nCount = 0L;
		}
		switch (ms)
		{
		case MarkerStyle.MarkerUpperRoman:
		case MarkerStyle.MarkerLowerRoman:
			return MarkerRomanCountToString(sb, ms, nCount);
		case MarkerStyle.MarkerUpperAlpha:
		case MarkerStyle.MarkerLowerAlpha:
			return MarkerAlphaCountToString(sb, ms, nCount);
		case MarkerStyle.MarkerArabic:
		case MarkerStyle.MarkerOrdinal:
		case MarkerStyle.MarkerCardinal:
			return nCount.ToString(CultureInfo.InvariantCulture);
		case MarkerStyle.MarkerNone:
		case MarkerStyle.MarkerHidden:
			return "";
		default:
			return "\\'B7";
		}
	}

	private static string MarkerRomanCountToString(StringBuilder sb, MarkerStyle ms, long nCount)
	{
		while (nCount >= 1000)
		{
			sb.Append('M');
			nCount -= 1000;
		}
		long num = nCount / 100;
		long num2 = num;
		if ((ulong)num2 <= 9uL)
		{
			switch (num2)
			{
			case 9L:
				sb.Append("CM");
				break;
			case 8L:
				sb.Append("DCCC");
				break;
			case 7L:
				sb.Append("DCC");
				break;
			case 6L:
				sb.Append("DC");
				break;
			case 5L:
				sb.Append('D');
				break;
			case 4L:
				sb.Append("CD");
				break;
			case 3L:
				sb.Append("CCC");
				break;
			case 2L:
				sb.Append("CC");
				break;
			case 1L:
				sb.Append('C');
				break;
			}
		}
		nCount %= 100;
		num = nCount / 10;
		long num3 = num;
		if ((ulong)num3 <= 9uL)
		{
			switch (num3)
			{
			case 9L:
				sb.Append("XC");
				break;
			case 8L:
				sb.Append("LXXX");
				break;
			case 7L:
				sb.Append("LXX");
				break;
			case 6L:
				sb.Append("LX");
				break;
			case 5L:
				sb.Append('L');
				break;
			case 4L:
				sb.Append("XL");
				break;
			case 3L:
				sb.Append("XXX");
				break;
			case 2L:
				sb.Append("XX");
				break;
			case 1L:
				sb.Append('X');
				break;
			}
		}
		nCount %= 10;
		long num4 = nCount;
		if ((ulong)num4 <= 9uL)
		{
			switch (num4)
			{
			case 9L:
				sb.Append("IX");
				break;
			case 8L:
				sb.Append("VIII");
				break;
			case 7L:
				sb.Append("VII");
				break;
			case 6L:
				sb.Append("VI");
				break;
			case 5L:
				sb.Append('V');
				break;
			case 4L:
				sb.Append("IV");
				break;
			case 3L:
				sb.Append("III");
				break;
			case 2L:
				sb.Append("II");
				break;
			case 1L:
				sb.Append('I');
				break;
			}
		}
		if (ms == MarkerStyle.MarkerUpperRoman)
		{
			return sb.ToString();
		}
		return sb.ToString().ToLower(CultureInfo.InvariantCulture);
	}

	private static string MarkerAlphaCountToString(StringBuilder sb, MarkerStyle ms, long nCount)
	{
		int num = 26;
		int num2 = 676;
		int num3 = 17576;
		int num4 = 456976;
		int num5 = 0;
		while (nCount > num4 + num3 + num2 + num)
		{
			num5++;
			nCount -= num4;
		}
		if (num5 > 0)
		{
			if (num5 > 26)
			{
				num5 = 26;
			}
			sb.Append((char)(65 + (num5 - 1)));
		}
		num5 = 0;
		while (nCount > num3 + num2 + num)
		{
			num5++;
			nCount -= num3;
		}
		if (num5 > 0)
		{
			sb.Append((char)(65 + (num5 - 1)));
		}
		num5 = 0;
		while (nCount > num2 + num)
		{
			num5++;
			nCount -= num2;
		}
		if (num5 > 0)
		{
			sb.Append((char)(65 + (num5 - 1)));
		}
		num5 = 0;
		while (nCount > num)
		{
			num5++;
			nCount -= num;
		}
		if (num5 > 0)
		{
			sb.Append((char)(65 + (num5 - 1)));
		}
		sb.Append((char)(65 + (nCount - 1)));
		if (ms == MarkerStyle.MarkerUpperAlpha)
		{
			return sb.ToString();
		}
		return sb.ToString().ToLower(CultureInfo.InvariantCulture);
	}

	internal static void ByteToHex(byte byteData, out byte firstHexByte, out byte secondHexByte)
	{
		firstHexByte = (byte)((byteData >> 4) & 0xF);
		secondHexByte = (byte)(byteData & 0xF);
		if (firstHexByte >= 0 && firstHexByte <= 9)
		{
			firstHexByte += 48;
		}
		else if (firstHexByte >= 10 && firstHexByte <= 15)
		{
			firstHexByte += 87;
		}
		if (secondHexByte >= 0 && secondHexByte <= 9)
		{
			secondHexByte += 48;
		}
		else if (secondHexByte >= 10 && secondHexByte <= 15)
		{
			secondHexByte += 87;
		}
	}
}
