using System.ComponentModel;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Describes the degree to which a font has been stretched compared to the normal aspect ratio of that font.</summary>
[TypeConverter(typeof(FontStretchConverter))]
[Localizability(LocalizationCategory.None)]
public struct FontStretch : IFormattable
{
	private int _stretch;

	private int RealStretch => _stretch + 5;

	internal FontStretch(int stretch)
	{
		_stretch = stretch - 5;
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.FontStretch" /> that corresponds to the OpenType usStretchClass value. </summary>
	/// <returns>A new instance of <see cref="T:System.Windows.FontStretch" />.</returns>
	/// <param name="stretchValue">An integer value between one and nine that corresponds to the usStretchValue definition in the OpenType specification. </param>
	public static FontStretch FromOpenTypeStretch(int stretchValue)
	{
		if (stretchValue < 1 || stretchValue > 9)
		{
			throw new ArgumentOutOfRangeException("stretchValue", SR.Format(SR.ParameterMustBeBetween, 1, 9));
		}
		return new FontStretch(stretchValue);
	}

	/// <summary>Returns a value that represents the OpenType usStretchClass for this <see cref="T:System.Windows.FontStretch" /> object. </summary>
	/// <returns>An integer value between 1 and 999 that corresponds to the usStretchClass definition in the OpenType specification.</returns>
	public int ToOpenTypeStretch()
	{
		return RealStretch;
	}

	/// <summary>Compares two instances of <see cref="T:System.Windows.FontStretch" /> objects.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the relationship between the two instances of <see cref="T:System.Windows.FontStretch" />.</returns>
	/// <param name="left">The first <see cref="T:System.Windows.FontStretch" /> object to compare.</param>
	/// <param name="right">The second <see cref="T:System.Windows.FontStretch" /> object to compare.</param>
	public static int Compare(FontStretch left, FontStretch right)
	{
		return left._stretch - right._stretch;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontStretch" /> to determine whether one instance is less than the other. </summary>
	/// <returns>true if <paramref name="left" /> is less than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	public static bool operator <(FontStretch left, FontStretch right)
	{
		return Compare(left, right) < 0;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontStretch" /> to determine whether one instance is less than or equal to the other.</summary>
	/// <returns>true if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	public static bool operator <=(FontStretch left, FontStretch right)
	{
		return Compare(left, right) <= 0;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontStretch" /> to determine if one instance is greater than the other.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">First instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	/// <param name="right">Second instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	public static bool operator >(FontStretch left, FontStretch right)
	{
		return Compare(left, right) > 0;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontStretch" /> to determine whether one instance is greater than or equal to the other.</summary>
	/// <returns>true if <paramref name="left" /> is greater than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	public static bool operator >=(FontStretch left, FontStretch right)
	{
		return Compare(left, right) >= 0;
	}

	/// <summary>Compares two instances of <see cref="T:System.Windows.FontStretch" /> for equality.</summary>
	/// <returns>true when the specified <see cref="T:System.Windows.FontStretch" /> objects are equal; otherwise, false.</returns>
	/// <param name="left">First instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	/// <param name="right">Second instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	public static bool operator ==(FontStretch left, FontStretch right)
	{
		return Compare(left, right) == 0;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontStretch" /> to determine inequality.</summary>
	/// <returns>false if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, true.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontStretch" /> to compare.</param>
	public static bool operator !=(FontStretch left, FontStretch right)
	{
		return !(left == right);
	}

	/// <summary>Compares a <see cref="T:System.Windows.FontStretch" /> object with the current <see cref="T:System.Windows.FontStretch" /> object.</summary>
	/// <returns>true if two instances are equal; otherwise, false.</returns>
	/// <param name="obj">The instance of the <see cref="T:System.Windows.FontStretch" /> object to compare for equality.</param>
	public bool Equals(FontStretch obj)
	{
		return this == obj;
	}

	/// <summary>Compares a <see cref="T:System.Object" /> with the current <see cref="T:System.Windows.FontStretch" /> object.</summary>
	/// <returns>true if two instances are equal; otherwise, false.</returns>
	/// <param name="obj">The instance of the <see cref="T:System.Object" /> to compare for equality.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is FontStretch))
		{
			return false;
		}
		return this == (FontStretch)obj;
	}

	/// <summary>Retrieves the hash code for this object.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value representing the hash code for the object.</returns>
	public override int GetHashCode()
	{
		return RealStretch;
	}

	/// <summary>Creates a <see cref="T:System.String" /> representation of the current <see cref="T:System.Windows.FontStretch" /> object based on the current culture.</summary>
	/// <returns>A <see cref="T:System.String" /> value representation of the object.</returns>
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

	private string ConvertToString(string format, IFormatProvider provider)
	{
		if (!FontStretches.FontStretchToString(RealStretch, out var convertedValue))
		{
			Invariant.Assert(condition: false);
		}
		return convertedValue;
	}
}
