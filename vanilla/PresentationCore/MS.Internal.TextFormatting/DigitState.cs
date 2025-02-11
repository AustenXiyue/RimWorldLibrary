using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal class DigitState
{
	private CultureInfo _lastTraditionalCulture;

	private NumberSubstitutionMethod _lastMethod;

	private CultureInfo _lastNumberCulture;

	private CultureInfo _digitCulture;

	private bool _contextual;

	internal CultureInfo DigitCulture => _digitCulture;

	internal bool RequiresNumberSubstitution => _digitCulture != null;

	internal bool Contextual => _contextual;

	internal static NumberSubstitutionMethod GetResolvedSubstitutionMethod(TextRunProperties properties, CultureInfo digitCulture, out bool ignoreUserOverride)
	{
		ignoreUserOverride = true;
		NumberSubstitutionMethod numberSubstitutionMethod = NumberSubstitutionMethod.European;
		if (digitCulture != null)
		{
			NumberSubstitutionMethod method;
			CultureInfo numberCulture = GetNumberCulture(properties, out method, out ignoreUserOverride);
			if (numberCulture != null)
			{
				if (method == NumberSubstitutionMethod.AsCulture)
				{
					method = numberCulture.NumberFormat.DigitSubstitution switch
					{
						DigitShapes.Context => NumberSubstitutionMethod.Context, 
						DigitShapes.NativeNational => NumberSubstitutionMethod.NativeNational, 
						_ => NumberSubstitutionMethod.European, 
					};
				}
				numberSubstitutionMethod = method;
				if (numberSubstitutionMethod == NumberSubstitutionMethod.Context)
				{
					numberSubstitutionMethod = NumberSubstitutionMethod.Traditional;
				}
			}
		}
		return numberSubstitutionMethod;
	}

	internal void SetTextRunProperties(TextRunProperties properties)
	{
		NumberSubstitutionMethod method;
		bool ignoreUserOverride;
		CultureInfo numberCulture = GetNumberCulture(properties, out method, out ignoreUserOverride);
		if (numberCulture != _lastNumberCulture || method != _lastMethod)
		{
			_lastNumberCulture = numberCulture;
			_lastMethod = method;
			_digitCulture = GetDigitCulture(numberCulture, method, out _contextual);
		}
	}

	private static CultureInfo GetNumberCulture(TextRunProperties properties, out NumberSubstitutionMethod method, out bool ignoreUserOverride)
	{
		ignoreUserOverride = true;
		NumberSubstitution numberSubstitution = properties.NumberSubstitution;
		if (numberSubstitution == null)
		{
			method = NumberSubstitutionMethod.AsCulture;
			return CultureMapper.GetSpecificCulture(properties.CultureInfo);
		}
		method = numberSubstitution.Substitution;
		switch (numberSubstitution.CultureSource)
		{
		case NumberCultureSource.Text:
			return CultureMapper.GetSpecificCulture(properties.CultureInfo);
		case NumberCultureSource.User:
			ignoreUserOverride = false;
			return CultureInfo.CurrentCulture;
		case NumberCultureSource.Override:
			return numberSubstitution.CultureOverride;
		default:
			return null;
		}
	}

	private CultureInfo GetDigitCulture(CultureInfo numberCulture, NumberSubstitutionMethod method, out bool contextual)
	{
		contextual = false;
		if (numberCulture == null)
		{
			return null;
		}
		if (method == NumberSubstitutionMethod.AsCulture)
		{
			switch (numberCulture.NumberFormat.DigitSubstitution)
			{
			case DigitShapes.Context:
				method = NumberSubstitutionMethod.Context;
				break;
			case DigitShapes.NativeNational:
				method = NumberSubstitutionMethod.NativeNational;
				break;
			default:
				return null;
			}
		}
		switch (method)
		{
		case NumberSubstitutionMethod.Context:
			if (IsArabic(numberCulture) || IsFarsi(numberCulture))
			{
				contextual = true;
				return GetTraditionalCulture(numberCulture);
			}
			return null;
		case NumberSubstitutionMethod.NativeNational:
			if (!HasLatinDigits(numberCulture))
			{
				return numberCulture;
			}
			return null;
		case NumberSubstitutionMethod.Traditional:
			return GetTraditionalCulture(numberCulture);
		default:
			return null;
		}
	}

	private static bool HasLatinDigits(CultureInfo culture)
	{
		string[] nativeDigits = culture.NumberFormat.NativeDigits;
		for (int i = 0; i < 10; i++)
		{
			string text = nativeDigits[i];
			if (text.Length != 1 || text[0] != (ushort)(48 + i))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsArabic(CultureInfo culture)
	{
		return (culture.LCID & 0xFF) == 1;
	}

	private static bool IsFarsi(CultureInfo culture)
	{
		return (culture.LCID & 0xFF) == 41;
	}

	private CultureInfo GetTraditionalCulture(CultureInfo numberCulture)
	{
		int lCID = numberCulture.LCID;
		if (_lastTraditionalCulture != null && _lastTraditionalCulture.LCID == lCID)
		{
			return _lastTraditionalCulture;
		}
		CultureInfo cultureInfo = null;
		switch (lCID & 0xFF)
		{
		case 1:
			cultureInfo = CreateTraditionalCulture(numberCulture, 1632, arabic: true);
			break;
		case 30:
			cultureInfo = CreateTraditionalCulture(numberCulture, 3664, arabic: false);
			break;
		case 32:
		case 41:
			cultureInfo = CreateTraditionalCulture(numberCulture, 1776, arabic: true);
			break;
		case 57:
			cultureInfo = CreateTraditionalCulture(numberCulture, 2406, arabic: false);
			break;
		case 69:
			cultureInfo = CreateTraditionalCulture(numberCulture, 2534, arabic: false);
			break;
		case 70:
			switch (lCID)
			{
			case 1094:
				cultureInfo = CreateTraditionalCulture(numberCulture, 2662, arabic: false);
				break;
			case 2118:
				cultureInfo = CreateTraditionalCulture(numberCulture, 1776, arabic: true);
				break;
			}
			break;
		case 71:
			cultureInfo = CreateTraditionalCulture(numberCulture, 2790, arabic: false);
			break;
		case 72:
			cultureInfo = CreateTraditionalCulture(numberCulture, 2918, arabic: false);
			break;
		case 73:
			cultureInfo = CreateTraditionalCulture(numberCulture, 3046, arabic: false);
			break;
		case 74:
			cultureInfo = CreateTraditionalCulture(numberCulture, 3174, arabic: false);
			break;
		case 75:
			cultureInfo = CreateTraditionalCulture(numberCulture, 3302, arabic: false);
			break;
		case 76:
			cultureInfo = CreateTraditionalCulture(numberCulture, 3430, arabic: false);
			break;
		case 77:
			cultureInfo = CreateTraditionalCulture(numberCulture, 2534, arabic: false);
			break;
		case 78:
		case 79:
			cultureInfo = CreateTraditionalCulture(numberCulture, 2406, arabic: false);
			break;
		case 80:
			if (lCID == 2128)
			{
				cultureInfo = CreateTraditionalCulture(numberCulture, 6160, arabic: false);
			}
			break;
		case 81:
			cultureInfo = CreateTraditionalCulture(numberCulture, 3872, arabic: false);
			break;
		case 83:
			cultureInfo = CreateTraditionalCulture(numberCulture, 6112, arabic: false);
			break;
		case 84:
			cultureInfo = CreateTraditionalCulture(numberCulture, 3792, arabic: false);
			break;
		case 85:
			cultureInfo = CreateTraditionalCulture(numberCulture, 4160, arabic: false);
			break;
		case 87:
			cultureInfo = CreateTraditionalCulture(numberCulture, 2406, arabic: false);
			break;
		case 88:
			cultureInfo = CreateTraditionalCulture(numberCulture, 2534, arabic: false);
			break;
		case 89:
			switch (lCID)
			{
			case 1113:
				cultureInfo = CreateTraditionalCulture(numberCulture, 2406, arabic: false);
				break;
			case 2137:
				cultureInfo = CreateTraditionalCulture(numberCulture, 1776, arabic: true);
				break;
			}
			break;
		case 95:
			if (lCID == 1119)
			{
				cultureInfo = CreateTraditionalCulture(numberCulture, 1632, arabic: true);
			}
			break;
		case 96:
			switch (lCID)
			{
			case 1120:
				cultureInfo = CreateTraditionalCulture(numberCulture, 1776, arabic: true);
				break;
			case 2144:
				cultureInfo = CreateTraditionalCulture(numberCulture, 2406, arabic: false);
				break;
			}
			break;
		case 97:
			cultureInfo = CreateTraditionalCulture(numberCulture, 2406, arabic: false);
			break;
		case 99:
			cultureInfo = CreateTraditionalCulture(numberCulture, 1776, arabic: true);
			break;
		case 140:
			cultureInfo = CreateTraditionalCulture(numberCulture, 1776, arabic: true);
			break;
		}
		if (cultureInfo == null)
		{
			if (!HasLatinDigits(numberCulture))
			{
				cultureInfo = numberCulture;
			}
		}
		else
		{
			_lastTraditionalCulture = cultureInfo;
		}
		return cultureInfo;
	}

	private CultureInfo CreateTraditionalCulture(CultureInfo numberCulture, int firstDigit, bool arabic)
	{
		CultureInfo cultureInfo = (CultureInfo)numberCulture.Clone();
		string[] array = new string[10];
		if (firstDigit < 65536)
		{
			for (int i = 0; i < 10; i++)
			{
				array[i] = ((char)(firstDigit + i)).ToString();
			}
		}
		else
		{
			Span<char> span = stackalloc char[2];
			for (int j = 0; j < 10; j++)
			{
				int num = firstDigit + j - 65536;
				span[0] = (char)((num >> 10) | 0xD800);
				span[1] = (char)((num & 0x3FF) | 0xDC00);
				array[j] = new string(span);
			}
		}
		cultureInfo.NumberFormat.NativeDigits = array;
		if (arabic)
		{
			cultureInfo.NumberFormat.PercentSymbol = "٪";
			cultureInfo.NumberFormat.NumberDecimalSeparator = "٫";
			cultureInfo.NumberFormat.NumberGroupSeparator = "٬";
		}
		else
		{
			cultureInfo.NumberFormat.PercentSymbol = "%";
			cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
			cultureInfo.NumberFormat.NumberGroupSeparator = ",";
		}
		return cultureInfo;
	}
}
