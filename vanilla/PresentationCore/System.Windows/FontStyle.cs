using System.ComponentModel;

namespace System.Windows;

/// <summary>Defines a structure that represents the style of a font face as normal, italic, or oblique.</summary>
[TypeConverter(typeof(FontStyleConverter))]
[Localizability(LocalizationCategory.None)]
public struct FontStyle : IFormattable
{
	private int _style;

	internal FontStyle(int style)
	{
		_style = style;
	}

	/// <summary>Compares two instances of <see cref="T:System.Windows.FontStyle" /> for equality.</summary>
	/// <returns>true to show the specified <see cref="T:System.Windows.FontStyle" /> objects are equal; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontStyle" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontStyle" /> to compare.</param>
	public static bool operator ==(FontStyle left, FontStyle right)
	{
		return left._style == right._style;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontStyle" /> to determine inequality.</summary>
	/// <returns>false to show <paramref name="left" /> is equal to <paramref name="right" />; otherwise, true.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontStyle" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontStyle" /> to compare.</param>
	public static bool operator !=(FontStyle left, FontStyle right)
	{
		return !(left == right);
	}

	/// <summary>Compares a <see cref="T:System.Windows.FontStyle" /> with the current <see cref="T:System.Windows.FontStyle" /> instance for equality.</summary>
	/// <returns>true to show the two instances are equal; otherwise, false.</returns>
	/// <param name="obj">An instance of <see cref="T:System.Windows.FontStyle" /> to compare for equality.</param>
	public bool Equals(FontStyle obj)
	{
		return this == obj;
	}

	/// <summary>Compares an <see cref="T:System.Object" /> with the current <see cref="T:System.Windows.FontStyle" /> instance for equality.</summary>
	/// <returns>true to show the two instances are equal; otherwise, false.</returns>
	/// <param name="obj">An <see cref="T:System.Object" /> value that represents the <see cref="T:System.Windows.FontStyle" /> to compare for equality.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is FontStyle))
		{
			return false;
		}
		return this == (FontStyle)obj;
	}

	/// <summary>Retrieves the hash code for this object. </summary>
	/// <returns>A 32-bit hash code, which is a signed integer.</returns>
	public override int GetHashCode()
	{
		return _style;
	}

	/// <summary>Creates a <see cref="T:System.String" /> that represents the current <see cref="T:System.Windows.FontStyle" /> object and is based on the <see cref="P:System.Globalization.CultureInfo.CurrentCulture" /> property information.</summary>
	/// <returns>A <see cref="T:System.String" /> that represents the value of the <see cref="T:System.Windows.FontStyle" /> object, such as "Normal", "Italic", or "Oblique".</returns>
	public override string ToString()
	{
		return ConvertToString(null, null);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IFormattable.ToString(System.String,System.IFormatProvider)" />.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the value of the current instance in the specified format.</returns>
	/// <param name="format">The <see cref="T:System.String" /> specifying the format to use.-or- null to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="provider">The <see cref="T:System.IFormatProvider" /> to use to format the value.-or- null to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		return ConvertToString(format, provider);
	}

	internal int GetStyleForInternalConstruction()
	{
		return _style;
	}

	private string ConvertToString(string format, IFormatProvider provider)
	{
		if (_style == 0)
		{
			return "Normal";
		}
		if (_style == 1)
		{
			return "Oblique";
		}
		return "Italic";
	}
}
