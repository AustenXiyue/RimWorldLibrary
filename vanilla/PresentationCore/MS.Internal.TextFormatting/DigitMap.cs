using System.Globalization;

namespace MS.Internal.TextFormatting;

internal struct DigitMap
{
	private NumberFormatInfo _format;

	private string[] _digits;

	internal int this[int ch]
	{
		get
		{
			if (_format != null && IsDigitOrSymbol(ch))
			{
				uint num = (uint)(ch - 48);
				ch = ((num < 10) ? StringToScalar(_digits[num], ch) : (ch switch
				{
					37 => StringToScalar(_format.PercentSymbol, ch), 
					44 => StringToScalar(_format.NumberGroupSeparator, ch), 
					_ => StringToScalar(_format.NumberDecimalSeparator, ch), 
				}));
			}
			return ch;
		}
	}

	internal DigitMap(CultureInfo digitCulture)
	{
		if (digitCulture != null)
		{
			_format = digitCulture.NumberFormat;
			_digits = _format.NativeDigits;
		}
		else
		{
			_format = null;
			_digits = null;
		}
	}

	internal static int GetFallbackCharacter(int ch)
	{
		return ch switch
		{
			1643 => 44, 
			1644 => 1548, 
			3046 => 48, 
			_ => 0, 
		};
	}

	private static int StringToScalar(string s, int defaultValue)
	{
		if (s.Length == 1)
		{
			return s[0];
		}
		if (s.Length == 2 && IsHighSurrogate(s[0]) && IsLowSurrogate(s[1]))
		{
			return MakeUnicodeScalar(s[0], s[1]);
		}
		return defaultValue;
	}

	internal static bool IsHighSurrogate(int ch)
	{
		if (ch >= 55296)
		{
			return ch < 56320;
		}
		return false;
	}

	internal static bool IsLowSurrogate(int ch)
	{
		if (ch >= 56320)
		{
			return ch < 57344;
		}
		return false;
	}

	internal static bool IsSurrogate(int ch)
	{
		if (!IsHighSurrogate(ch))
		{
			return IsLowSurrogate(ch);
		}
		return true;
	}

	internal static int MakeUnicodeScalar(int hi, int lo)
	{
		return (((hi & 0x3FF) << 10) | (lo & 0x3FF)) + 65536;
	}

	private static bool IsDigitOrSymbol(int ch)
	{
		if ((uint)(ch - 37) <= 20u)
		{
			return ((2095745 >>> ch - 37) & 1) != 0;
		}
		return false;
	}
}
