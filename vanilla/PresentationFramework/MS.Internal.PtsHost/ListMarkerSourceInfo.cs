using System;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class ListMarkerSourceInfo
{
	private static char NumberSuffix = '.';

	private static string DecimalNumerics = "0123456789";

	private static string LowerLatinNumerics = "abcdefghijklmnopqrstuvwxyz";

	private static string UpperLatinNumerics = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	private static string LargestRomanMarkerUpper = "MMMDCCCLXXXVIII";

	private static string LargestRomanMarkerLower = "mmmdccclxxxviii";

	private static string[][] RomanNumerics = new string[2][]
	{
		new string[4] { "m??", "cdm", "xlc", "ivx" },
		new string[4] { "M??", "CDM", "XLC", "IVX" }
	};

	private static int[] RomanNumericSizeIncrements = new int[12]
	{
		1, 2, 3, 8, 18, 28, 38, 88, 188, 288,
		388, 888
	};

	private ListMarkerSourceInfo()
	{
	}

	internal static Thickness CalculatePadding(List list, double lineHeight, double pixelsPerDip)
	{
		return new Thickness((double)((int)((GetFormattedMarker(list, pixelsPerDip).Width + 1.5 * lineHeight) / lineHeight) + 1) * lineHeight, 0.0, 0.0, 0.0);
	}

	private static FormattedText GetFormattedMarker(List list, double pixelsPerDip)
	{
		string textToFormat = "";
		if (IsKnownSymbolMarkerStyle(list.MarkerStyle))
		{
			switch (list.MarkerStyle)
			{
			case TextMarkerStyle.Disc:
				textToFormat = "\u009f";
				break;
			case TextMarkerStyle.Circle:
				textToFormat = "¡";
				break;
			case TextMarkerStyle.Square:
				textToFormat = "q";
				break;
			case TextMarkerStyle.Box:
				textToFormat = "§";
				break;
			}
			Typeface modifiedTypeface = DynamicPropertyReader.GetModifiedTypeface(list, new FontFamily("Wingdings"));
			return new FormattedText(textToFormat, DynamicPropertyReader.GetCultureInfo(list), list.FlowDirection, modifiedTypeface, list.FontSize, list.Foreground, pixelsPerDip);
		}
		if (IsKnownIndexMarkerStyle(list.MarkerStyle))
		{
			int startIndex = list.StartIndex;
			Invariant.Assert(startIndex > 0);
			int count = list.ListItems.Count;
			int num = ((int.MaxValue - count >= startIndex) ? ((count == 0) ? startIndex : (startIndex + count - 1)) : int.MaxValue);
			switch (list.MarkerStyle)
			{
			case TextMarkerStyle.Decimal:
				textToFormat = ConvertNumberToString(num, oneBased: false, DecimalNumerics);
				break;
			case TextMarkerStyle.LowerLatin:
				textToFormat = ConvertNumberToString(num, oneBased: true, LowerLatinNumerics);
				break;
			case TextMarkerStyle.UpperLatin:
				textToFormat = ConvertNumberToString(num, oneBased: true, UpperLatinNumerics);
				break;
			case TextMarkerStyle.LowerRoman:
				textToFormat = GetStringForLargestRomanMarker(startIndex, num, uppercase: false);
				break;
			case TextMarkerStyle.UpperRoman:
				textToFormat = GetStringForLargestRomanMarker(startIndex, num, uppercase: true);
				break;
			}
			return new FormattedText(textToFormat, DynamicPropertyReader.GetCultureInfo(list), list.FlowDirection, DynamicPropertyReader.GetTypeface(list), list.FontSize, list.Foreground, pixelsPerDip);
		}
		textToFormat = "\u009f";
		Typeface modifiedTypeface2 = DynamicPropertyReader.GetModifiedTypeface(list, new FontFamily("Wingdings"));
		return new FormattedText(textToFormat, DynamicPropertyReader.GetCultureInfo(list), list.FlowDirection, modifiedTypeface2, list.FontSize, list.Foreground, pixelsPerDip);
	}

	private static string ConvertNumberToString(int number, bool oneBased, string numericSymbols)
	{
		if (oneBased)
		{
			number--;
		}
		Invariant.Assert(number >= 0);
		int length = numericSymbols.Length;
		if (number < length)
		{
			return new string(stackalloc char[2]
			{
				numericSymbols[number],
				NumberSuffix
			});
		}
		int num = (oneBased ? 1 : 0);
		int num2 = 1;
		long num3 = length;
		long num4 = length;
		while (number >= num3)
		{
			num4 *= length;
			num3 = num4 + num3 * num;
			num2++;
		}
		return string.Create(num2 + 1, (numericSymbols, number, length, num), delegate(Span<char> result, (string numericSymbols, int number, int b, int disjoint) state)
		{
			result[result.Length - 1] = NumberSuffix;
			for (int num5 = result.Length - 2; num5 >= 0; num5--)
			{
				state.number = Math.DivRem(state.number, state.b, out var result2) - state.disjoint;
				result[num5] = state.numericSymbols[result2];
			}
		});
	}

	private static string GetStringForLargestRomanMarker(int startIndex, int highestIndex, bool uppercase)
	{
		int num = 0;
		if (highestIndex > 3999)
		{
			if (!uppercase)
			{
				return LargestRomanMarkerLower;
			}
			return LargestRomanMarkerUpper;
		}
		num = GetIndexForLargestRomanMarker(startIndex, highestIndex);
		return ConvertNumberToRomanString(num, uppercase);
	}

	private static int GetIndexForLargestRomanMarker(int startIndex, int highestIndex)
	{
		int num = 0;
		if (startIndex == 1)
		{
			int num2 = highestIndex / 1000;
			highestIndex %= 1000;
			for (int i = 0; i < RomanNumericSizeIncrements.Length; i++)
			{
				Invariant.Assert(highestIndex >= RomanNumericSizeIncrements[i]);
				if (highestIndex == RomanNumericSizeIncrements[i])
				{
					num = highestIndex;
					break;
				}
				Invariant.Assert(highestIndex > RomanNumericSizeIncrements[i]);
				if (i >= RomanNumericSizeIncrements.Length - 1 || highestIndex < RomanNumericSizeIncrements[i + 1])
				{
					num = RomanNumericSizeIncrements[i];
					break;
				}
			}
			if (num2 > 0)
			{
				num = num2 * 1000 + num;
			}
		}
		else
		{
			int num3 = 0;
			for (int j = startIndex; j <= highestIndex; j++)
			{
				string text = ConvertNumberToRomanString(j, uppercase: true);
				if (text.Length > num3)
				{
					num = j;
					num3 = text.Length;
				}
			}
		}
		Invariant.Assert(num > 0);
		return num;
	}

	private static string ConvertNumberToRomanString(int number, bool uppercase)
	{
		Invariant.Assert(number <= 3999);
		StringBuilder stringBuilder = new StringBuilder();
		AddRomanNumeric(stringBuilder, number / 1000, RomanNumerics[uppercase ? 1u : 0u][0]);
		number %= 1000;
		AddRomanNumeric(stringBuilder, number / 100, RomanNumerics[uppercase ? 1u : 0u][1]);
		number %= 100;
		AddRomanNumeric(stringBuilder, number / 10, RomanNumerics[uppercase ? 1u : 0u][2]);
		number %= 10;
		AddRomanNumeric(stringBuilder, number, RomanNumerics[uppercase ? 1u : 0u][3]);
		stringBuilder.Append(NumberSuffix);
		return stringBuilder.ToString();
	}

	private static void AddRomanNumeric(StringBuilder builder, int number, string oneFiveTen)
	{
		switch (number)
		{
		case 4:
		case 9:
			builder.Append(oneFiveTen[0]);
			break;
		case 1:
		case 2:
		case 3:
		case 5:
		case 6:
		case 7:
		case 8:
			break;
		default:
			return;
		}
		if (number == 9)
		{
			builder.Append(oneFiveTen[2]);
			return;
		}
		if (number >= 4)
		{
			builder.Append(oneFiveTen[1]);
		}
		int num = number % 5;
		while (num > 0 && num < 4)
		{
			builder.Append(oneFiveTen[0]);
			num--;
		}
	}

	private static bool IsKnownSymbolMarkerStyle(TextMarkerStyle markerStyle)
	{
		if (markerStyle != TextMarkerStyle.Disc && markerStyle != TextMarkerStyle.Circle && markerStyle != TextMarkerStyle.Square)
		{
			return markerStyle == TextMarkerStyle.Box;
		}
		return true;
	}

	private static bool IsKnownIndexMarkerStyle(TextMarkerStyle markerStyle)
	{
		if (markerStyle != TextMarkerStyle.Decimal && markerStyle != TextMarkerStyle.LowerLatin && markerStyle != TextMarkerStyle.UpperLatin && markerStyle != TextMarkerStyle.LowerRoman)
		{
			return markerStyle == TextMarkerStyle.UpperRoman;
		}
		return true;
	}
}
