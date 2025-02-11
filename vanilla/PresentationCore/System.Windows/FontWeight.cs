using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Refers to the density of a typeface, in terms of the lightness or heaviness of the strokes.</summary>
[TypeConverter(typeof(FontWeightConverter))]
[Localizability(LocalizationCategory.None)]
public struct FontWeight : IFormattable
{
	private int _weight;

	private int RealWeight => _weight + 400;

	internal FontWeight(int weight)
	{
		_weight = weight - 400;
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.FontWeight" /> that corresponds to the OpenType usWeightClass value.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.FontWeight" />.</returns>
	/// <param name="weightValue">An integer value between 1 and 999 that corresponds to the usWeightClass definition in the OpenType specification.</param>
	public static FontWeight FromOpenTypeWeight(int weightValue)
	{
		if (weightValue < 1 || weightValue > 999)
		{
			throw new ArgumentOutOfRangeException("weightValue", SR.Format(SR.ParameterMustBeBetween, 1, 999));
		}
		return new FontWeight(weightValue);
	}

	/// <summary>Returns a value that represents the OpenType usWeightClass for the <see cref="T:System.Windows.FontWeight" /> object.</summary>
	/// <returns>An integer value between 1 and 999 that corresponds to the usWeightClass definition in the OpenType specification.</returns>
	public int ToOpenTypeWeight()
	{
		return RealWeight;
	}

	/// <summary>Compares two instances of <see cref="T:System.Windows.FontWeight" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that indicates the relationship between the two instances of <see cref="T:System.Windows.FontWeight" />. When the return value is less than zero, <paramref name="left" /> is less than <paramref name="right" />. When this value is zero, it indicates that both operands are equal. When the value is greater than zero, it indicates that <paramref name="left" /> is greater than <paramref name="right" />.</returns>
	/// <param name="left">The first <see cref="T:System.Windows.FontWeight" /> object to compare.</param>
	/// <param name="right">The second <see cref="T:System.Windows.FontWeight" /> object to compare.</param>
	public static int Compare(FontWeight left, FontWeight right)
	{
		return left._weight - right._weight;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontWeight" /> to determine whether one instance is less than the other.</summary>
	/// <returns>true if <paramref name="left" /> is less than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	public static bool operator <(FontWeight left, FontWeight right)
	{
		return Compare(left, right) < 0;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontWeight" /> to determine whether one instance is less than or equal to the other.</summary>
	/// <returns>true if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	public static bool operator <=(FontWeight left, FontWeight right)
	{
		return Compare(left, right) <= 0;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontWeight" /> to determine whether one instance is greater than the other.</summary>
	/// <returns>true if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	public static bool operator >(FontWeight left, FontWeight right)
	{
		return Compare(left, right) > 0;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontWeight" /> to determine whether one instance is greater than or equal to the other.</summary>
	/// <returns>true if <paramref name="left" /> is greater than or equal to <paramref name="right" />; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	public static bool operator >=(FontWeight left, FontWeight right)
	{
		return Compare(left, right) >= 0;
	}

	/// <summary>Compares two instances of <see cref="T:System.Windows.FontWeight" /> for equality.</summary>
	/// <returns>true if the instances of <see cref="T:System.Windows.FontWeight" /> are equal; otherwise, false.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	public static bool operator ==(FontWeight left, FontWeight right)
	{
		return Compare(left, right) == 0;
	}

	/// <summary>Evaluates two instances of <see cref="T:System.Windows.FontWeight" /> to determine inequality.</summary>
	/// <returns>false if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, true.</returns>
	/// <param name="left">The first instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	/// <param name="right">The second instance of <see cref="T:System.Windows.FontWeight" /> to compare.</param>
	public static bool operator !=(FontWeight left, FontWeight right)
	{
		return !(left == right);
	}

	/// <summary>Determines whether the current <see cref="T:System.Windows.FontWeight" /> object is equal to a specified <see cref="T:System.Windows.FontWeight" /> object.</summary>
	/// <returns>true if the two instances are equal; otherwise, false.</returns>
	/// <param name="obj">The instance of <see cref="T:System.Windows.FontWeight" /> to compare for equality.</param>
	public bool Equals(FontWeight obj)
	{
		return this == obj;
	}

	/// <summary>Determines whether the current <see cref="T:System.Windows.FontWeight" /> object is equal to a specified object.</summary>
	/// <returns>true if the two instances are equal; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare for equality.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is FontWeight))
		{
			return false;
		}
		return this == (FontWeight)obj;
	}

	/// <summary>Retrieves the hash code for this object.</summary>
	/// <returns>A 32-bit hash code, which is a signed integer.</returns>
	public override int GetHashCode()
	{
		return RealWeight;
	}

	/// <summary>Returns a text string that represents the value of the <see cref="T:System.Windows.FontWeight" /> object and is based on the <see cref="P:System.Globalization.CultureInfo.CurrentCulture" /> property information.</summary>
	/// <returns>A <see cref="T:System.String" /> that represents the value of the <see cref="T:System.Windows.FontWeight" /> object, such as "Light", "Normal", or "UltraBold".</returns>
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
		if (!FontWeights.FontWeightToString(RealWeight, out var convertedValue))
		{
			return RealWeight.ToString(provider);
		}
		return convertedValue;
	}
}
