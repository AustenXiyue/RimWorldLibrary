using System.Globalization;

namespace System.Windows;

/// <summary>Provides a set of static predefined <see cref="T:System.Windows.FontStretch" /> values.</summary>
public static class FontStretches
{
	/// <summary>Specifies an ultra-condensed <see cref="T:System.Windows.FontStretch" />.</summary>
	/// <returns>A value that represents an ultra-condensed <see cref="T:System.Windows.FontStretch" />.</returns>
	public static FontStretch UltraCondensed => new FontStretch(1);

	/// <summary>Specifies an extra-condensed <see cref="T:System.Windows.FontStretch" />.</summary>
	/// <returns>A value that represents an extra-condensed <see cref="T:System.Windows.FontStretch" />.</returns>
	public static FontStretch ExtraCondensed => new FontStretch(2);

	/// <summary>Specifies a condensed <see cref="T:System.Windows.FontStretch" />.</summary>
	/// <returns>A value that represents a condensed <see cref="T:System.Windows.FontStretch" />.</returns>
	public static FontStretch Condensed => new FontStretch(3);

	/// <summary>Specifies a semi-condensed <see cref="T:System.Windows.FontStretch" />.</summary>
	/// <returns>A value that represents a semi-condensed <see cref="T:System.Windows.FontStretch" />.</returns>
	public static FontStretch SemiCondensed => new FontStretch(4);

	/// <summary>Specifies a normal <see cref="T:System.Windows.FontStretch" />.</summary>
	/// <returns>A value that represents a normal <see cref="T:System.Windows.FontStretch" />.</returns>
	public static FontStretch Normal => new FontStretch(5);

	/// <summary>Specifies a medium <see cref="T:System.Windows.FontStretch" />.</summary>
	/// <returns>A value that represents a medium <see cref="T:System.Windows.FontStretch" />.</returns>
	public static FontStretch Medium => new FontStretch(5);

	/// <summary>Specifies a semi-expanded <see cref="T:System.Windows.FontStretch" />.</summary>
	/// <returns>A value that represents a semi-expanded <see cref="T:System.Windows.FontStretch" />.</returns>
	public static FontStretch SemiExpanded => new FontStretch(6);

	/// <summary>Specifies an expanded <see cref="T:System.Windows.FontStretch" />.</summary>
	/// <returns>A value that represents an expanded <see cref="T:System.Windows.FontStretch" />.</returns>
	public static FontStretch Expanded => new FontStretch(7);

	/// <summary>Specifies an extra-expanded <see cref="T:System.Windows.FontStretch" />.</summary>
	/// <returns>A value that represents an extra-expanded <see cref="T:System.Windows.FontStretch" />.</returns>
	public static FontStretch ExtraExpanded => new FontStretch(8);

	/// <summary>Specifies an ultra-expanded <see cref="T:System.Windows.FontStretch" />.</summary>
	/// <returns>A value that represents an ultra-expanded <see cref="T:System.Windows.FontStretch" />.</returns>
	public static FontStretch UltraExpanded => new FontStretch(9);

	internal static bool FontStretchStringToKnownStretch(string s, IFormatProvider provider, ref FontStretch fontStretch)
	{
		switch (s.Length)
		{
		case 6:
			if (s.Equals("Normal", StringComparison.OrdinalIgnoreCase))
			{
				fontStretch = Normal;
				return true;
			}
			if (s.Equals("Medium", StringComparison.OrdinalIgnoreCase))
			{
				fontStretch = Medium;
				return true;
			}
			break;
		case 8:
			if (s.Equals("Expanded", StringComparison.OrdinalIgnoreCase))
			{
				fontStretch = Expanded;
				return true;
			}
			break;
		case 9:
			if (s.Equals("Condensed", StringComparison.OrdinalIgnoreCase))
			{
				fontStretch = Condensed;
				return true;
			}
			break;
		case 12:
			if (s.Equals("SemiExpanded", StringComparison.OrdinalIgnoreCase))
			{
				fontStretch = SemiExpanded;
				return true;
			}
			break;
		case 13:
			if (s.Equals("SemiCondensed", StringComparison.OrdinalIgnoreCase))
			{
				fontStretch = SemiCondensed;
				return true;
			}
			if (s.Equals("ExtraExpanded", StringComparison.OrdinalIgnoreCase))
			{
				fontStretch = ExtraExpanded;
				return true;
			}
			if (s.Equals("UltraExpanded", StringComparison.OrdinalIgnoreCase))
			{
				fontStretch = UltraExpanded;
				return true;
			}
			break;
		case 14:
			if (s.Equals("UltraCondensed", StringComparison.OrdinalIgnoreCase))
			{
				fontStretch = UltraCondensed;
				return true;
			}
			if (s.Equals("ExtraCondensed", StringComparison.OrdinalIgnoreCase))
			{
				fontStretch = ExtraCondensed;
				return true;
			}
			break;
		}
		if (int.TryParse(s, NumberStyles.Integer, provider, out var result))
		{
			fontStretch = FontStretch.FromOpenTypeStretch(result);
			return true;
		}
		return false;
	}

	internal static bool FontStretchToString(int stretch, out string convertedValue)
	{
		switch (stretch)
		{
		case 1:
			convertedValue = "UltraCondensed";
			return true;
		case 2:
			convertedValue = "ExtraCondensed";
			return true;
		case 3:
			convertedValue = "Condensed";
			return true;
		case 4:
			convertedValue = "SemiCondensed";
			return true;
		case 5:
			convertedValue = "Normal";
			return true;
		case 6:
			convertedValue = "SemiExpanded";
			return true;
		case 7:
			convertedValue = "Expanded";
			return true;
		case 8:
			convertedValue = "ExtraExpanded";
			return true;
		case 9:
			convertedValue = "UltraExpanded";
			return true;
		default:
			convertedValue = null;
			return false;
		}
	}
}
