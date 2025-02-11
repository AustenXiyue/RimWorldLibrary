using System.Globalization;

namespace System.Windows;

/// <summary>Provides a set of static predefined <see cref="T:System.Windows.FontWeight" /> values.</summary>
public static class FontWeights
{
	/// <summary>Specifies a "Thin" font weight.</summary>
	/// <returns>A value that represents a "Thin" font weight.</returns>
	public static FontWeight Thin => new FontWeight(100);

	/// <summary>Specifies an "Extra-light" font weight.</summary>
	/// <returns>A value that represents an "Extra-light" font weight.</returns>
	public static FontWeight ExtraLight => new FontWeight(200);

	/// <summary>Specifies an "Ultra-light" font weight.</summary>
	/// <returns>A value that represents an "Ultra-light" font weight.</returns>
	public static FontWeight UltraLight => new FontWeight(200);

	/// <summary>Specifies a "Light" font weight.</summary>
	/// <returns>A value that represents a "Light" font weight.</returns>
	public static FontWeight Light => new FontWeight(300);

	/// <summary>Specifies a "Normal" font weight.</summary>
	/// <returns>A value that represents a "Normal" font weight.</returns>
	public static FontWeight Normal => new FontWeight(400);

	/// <summary>Specifies a "Regular" font weight.</summary>
	/// <returns>A value that represents a "Regular" font weight.</returns>
	public static FontWeight Regular => new FontWeight(400);

	/// <summary>Specifies a "Medium" font weight.</summary>
	/// <returns>A value that represents a "Medium" font weight.</returns>
	public static FontWeight Medium => new FontWeight(500);

	/// <summary>Specifies a "Demi-bold" font weight.</summary>
	/// <returns>A value that represents a "Demi-bold" font weight.</returns>
	public static FontWeight DemiBold => new FontWeight(600);

	/// <summary>Specifies a "Semi-bold" font weight.</summary>
	/// <returns>A value that represents a "Semi-bold" font weight.</returns>
	public static FontWeight SemiBold => new FontWeight(600);

	/// <summary>Specifies a "Bold" font weight.</summary>
	/// <returns>A value that represents a "Bold" font weight.</returns>
	public static FontWeight Bold => new FontWeight(700);

	/// <summary>Specifies an "Extra-bold" font weight.</summary>
	/// <returns>A value that represents an "Extra-bold" font weight.</returns>
	public static FontWeight ExtraBold => new FontWeight(800);

	/// <summary>Specifies an "Ultra-bold" font weight.</summary>
	/// <returns>A value that represents an "Ultra-bold" font weight.</returns>
	public static FontWeight UltraBold => new FontWeight(800);

	/// <summary>Specifies a "Black" font weight.</summary>
	/// <returns>A value that represents a "Black" font weight.</returns>
	public static FontWeight Black => new FontWeight(900);

	/// <summary>Specifies a "Heavy" font weight.</summary>
	/// <returns>A value that represents a "Heavy" font weight.</returns>
	public static FontWeight Heavy => new FontWeight(900);

	/// <summary>Specifies an "Extra-black" font weight.</summary>
	/// <returns>A value that represents an "Extra-black" font weight.</returns>
	public static FontWeight ExtraBlack => new FontWeight(950);

	/// <summary>Specifies an "Ultra-black" font weight.</summary>
	/// <returns>A value that represents an "Ultra-black" font weight.</returns>
	public static FontWeight UltraBlack => new FontWeight(950);

	internal static bool FontWeightStringToKnownWeight(string s, IFormatProvider provider, ref FontWeight fontWeight)
	{
		switch (s.Length)
		{
		case 4:
			if (s.Equals("Bold", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = Bold;
				return true;
			}
			if (s.Equals("Thin", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = Thin;
				return true;
			}
			break;
		case 5:
			if (s.Equals("Black", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = Black;
				return true;
			}
			if (s.Equals("Light", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = Light;
				return true;
			}
			if (s.Equals("Heavy", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = Heavy;
				return true;
			}
			break;
		case 6:
			if (s.Equals("Normal", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = Normal;
				return true;
			}
			if (s.Equals("Medium", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = Medium;
				return true;
			}
			break;
		case 7:
			if (s.Equals("Regular", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = Regular;
				return true;
			}
			break;
		case 8:
			if (s.Equals("SemiBold", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = SemiBold;
				return true;
			}
			if (s.Equals("DemiBold", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = DemiBold;
				return true;
			}
			break;
		case 9:
			if (s.Equals("ExtraBold", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = ExtraBold;
				return true;
			}
			if (s.Equals("UltraBold", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = UltraBold;
				return true;
			}
			break;
		case 10:
			if (s.Equals("ExtraLight", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = ExtraLight;
				return true;
			}
			if (s.Equals("UltraLight", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = UltraLight;
				return true;
			}
			if (s.Equals("ExtraBlack", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = ExtraBlack;
				return true;
			}
			if (s.Equals("UltraBlack", StringComparison.OrdinalIgnoreCase))
			{
				fontWeight = UltraBlack;
				return true;
			}
			break;
		}
		if (int.TryParse(s, NumberStyles.Integer, provider, out var result))
		{
			fontWeight = FontWeight.FromOpenTypeWeight(result);
			return true;
		}
		return false;
	}

	internal static bool FontWeightToString(int weight, out string convertedValue)
	{
		switch (weight)
		{
		case 100:
			convertedValue = "Thin";
			return true;
		case 200:
			convertedValue = "ExtraLight";
			return true;
		case 300:
			convertedValue = "Light";
			return true;
		case 400:
			convertedValue = "Normal";
			return true;
		case 500:
			convertedValue = "Medium";
			return true;
		case 600:
			convertedValue = "SemiBold";
			return true;
		case 700:
			convertedValue = "Bold";
			return true;
		case 800:
			convertedValue = "ExtraBold";
			return true;
		case 900:
			convertedValue = "Black";
			return true;
		case 950:
			convertedValue = "ExtraBlack";
			return true;
		default:
			convertedValue = null;
			return false;
		}
	}
}
