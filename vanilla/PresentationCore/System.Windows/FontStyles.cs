namespace System.Windows;

/// <summary>Provides a set of static predefined <see cref="T:System.Windows.FontStyle" /> values.</summary>
public static class FontStyles
{
	/// <summary>Specifies a normal <see cref="T:System.Windows.FontStyle" />.</summary>
	/// <returns>A value that represents a normal <see cref="T:System.Windows.FontStyle" />.</returns>
	public static FontStyle Normal => new FontStyle(0);

	/// <summary>Specifies an oblique <see cref="T:System.Windows.FontStyle" />.</summary>
	/// <returns>A value that represents an oblique <see cref="T:System.Windows.FontStyle" />.</returns>
	public static FontStyle Oblique => new FontStyle(1);

	/// <summary>Specifies an italic <see cref="T:System.Windows.FontStyle" />.</summary>
	/// <returns>A value that represents an italic <see cref="T:System.Windows.FontStyle" />.</returns>
	public static FontStyle Italic => new FontStyle(2);

	internal static bool FontStyleStringToKnownStyle(string s, IFormatProvider provider, ref FontStyle fontStyle)
	{
		if (s.Equals("Normal", StringComparison.OrdinalIgnoreCase))
		{
			fontStyle = Normal;
			return true;
		}
		if (s.Equals("Italic", StringComparison.OrdinalIgnoreCase))
		{
			fontStyle = Italic;
			return true;
		}
		if (s.Equals("Oblique", StringComparison.OrdinalIgnoreCase))
		{
			fontStyle = Oblique;
			return true;
		}
		return false;
	}
}
