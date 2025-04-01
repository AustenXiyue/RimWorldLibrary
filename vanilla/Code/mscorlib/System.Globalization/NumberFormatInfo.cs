using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

namespace System.Globalization;

/// <summary>Provides culture-specific information for formatting and parsing numeric values. </summary>
[Serializable]
[ComVisible(true)]
public sealed class NumberFormatInfo : ICloneable, IFormatProvider
{
	private static volatile NumberFormatInfo invariantInfo;

	internal int[] numberGroupSizes = new int[1] { 3 };

	internal int[] currencyGroupSizes = new int[1] { 3 };

	internal int[] percentGroupSizes = new int[1] { 3 };

	internal string positiveSign = "+";

	internal string negativeSign = "-";

	internal string numberDecimalSeparator = ".";

	internal string numberGroupSeparator = ",";

	internal string currencyGroupSeparator = ",";

	internal string currencyDecimalSeparator = ".";

	internal string currencySymbol = "¤";

	internal string ansiCurrencySymbol;

	internal string nanSymbol = "NaN";

	internal string positiveInfinitySymbol = "Infinity";

	internal string negativeInfinitySymbol = "-Infinity";

	internal string percentDecimalSeparator = ".";

	internal string percentGroupSeparator = ",";

	internal string percentSymbol = "%";

	internal string perMilleSymbol = "‰";

	[OptionalField(VersionAdded = 2)]
	internal string[] nativeDigits = new string[10] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

	[OptionalField(VersionAdded = 1)]
	internal int m_dataItem;

	internal int numberDecimalDigits = 2;

	internal int currencyDecimalDigits = 2;

	internal int currencyPositivePattern;

	internal int currencyNegativePattern;

	internal int numberNegativePattern = 1;

	internal int percentPositivePattern;

	internal int percentNegativePattern;

	internal int percentDecimalDigits = 2;

	[OptionalField(VersionAdded = 2)]
	internal int digitSubstitution = 1;

	internal bool isReadOnly;

	[OptionalField(VersionAdded = 1)]
	internal bool m_useUserOverride;

	[OptionalField(VersionAdded = 2)]
	internal bool m_isInvariant;

	[OptionalField(VersionAdded = 1)]
	internal bool validForParseAsNumber = true;

	[OptionalField(VersionAdded = 1)]
	internal bool validForParseAsCurrency = true;

	private const NumberStyles InvalidNumberStyles = ~(NumberStyles.Any | NumberStyles.AllowHexSpecifier);

	/// <summary>Gets a read-only <see cref="T:System.Globalization.NumberFormatInfo" /> object that is culture-independent (invariant).</summary>
	/// <returns>A read-only  object that is culture-independent (invariant).</returns>
	public static NumberFormatInfo InvariantInfo
	{
		get
		{
			if (invariantInfo == null)
			{
				invariantInfo = ReadOnly(new NumberFormatInfo
				{
					m_isInvariant = true
				});
			}
			return invariantInfo;
		}
	}

	/// <summary>Gets or sets the number of decimal places to use in currency values.</summary>
	/// <returns>The number of decimal places to use in currency values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is 2.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is being set to a value that is less than 0 or greater than 99. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int CurrencyDecimalDigits
	{
		get
		{
			return currencyDecimalDigits;
		}
		set
		{
			if (value < 0 || value > 99)
			{
				throw new ArgumentOutOfRangeException("CurrencyDecimalDigits", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 99));
			}
			VerifyWritable();
			currencyDecimalDigits = value;
		}
	}

	/// <summary>Gets or sets the string to use as the decimal separator in currency values.</summary>
	/// <returns>The string to use as the decimal separator in currency values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is ".".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	/// <exception cref="T:System.ArgumentException">The property is being set to an empty string.</exception>
	public string CurrencyDecimalSeparator
	{
		get
		{
			return currencyDecimalSeparator;
		}
		set
		{
			VerifyWritable();
			VerifyDecimalSeparator(value, "CurrencyDecimalSeparator");
			currencyDecimalSeparator = value;
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Globalization.NumberFormatInfo" /> is read-only; otherwise, false.</returns>
	public bool IsReadOnly => isReadOnly;

	/// <summary>Gets or sets the number of digits in each group to the left of the decimal in currency values.</summary>
	/// <returns>The number of digits in each group to the left of the decimal in currency values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is a one-dimensional array with only one element, which is set to 3.</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.ArgumentException">The property is being set and the array contains an entry that is less than 0 or greater than 9.-or- The property is being set and the array contains an entry, other than the last entry, that is set to 0. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int[] CurrencyGroupSizes
	{
		get
		{
			return (int[])currencyGroupSizes.Clone();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("CurrencyGroupSizes", Environment.GetResourceString("Object cannot be null."));
			}
			VerifyWritable();
			int[] groupSize = (int[])value.Clone();
			CheckGroupSize("CurrencyGroupSizes", groupSize);
			currencyGroupSizes = groupSize;
		}
	}

	/// <summary>Gets or sets the number of digits in each group to the left of the decimal in numeric values.</summary>
	/// <returns>The number of digits in each group to the left of the decimal in numeric values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is a one-dimensional array with only one element, which is set to 3.</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.ArgumentException">The property is being set and the array contains an entry that is less than 0 or greater than 9.-or- The property is being set and the array contains an entry, other than the last entry, that is set to 0. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int[] NumberGroupSizes
	{
		get
		{
			return (int[])numberGroupSizes.Clone();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("NumberGroupSizes", Environment.GetResourceString("Object cannot be null."));
			}
			VerifyWritable();
			int[] groupSize = (int[])value.Clone();
			CheckGroupSize("NumberGroupSizes", groupSize);
			numberGroupSizes = groupSize;
		}
	}

	/// <summary>Gets or sets the number of digits in each group to the left of the decimal in percent values. </summary>
	/// <returns>The number of digits in each group to the left of the decimal in percent values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is a one-dimensional array with only one element, which is set to 3.</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.ArgumentException">The property is being set and the array contains an entry that is less than 0 or greater than 9.-or- The property is being set and the array contains an entry, other than the last entry, that is set to 0. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int[] PercentGroupSizes
	{
		get
		{
			return (int[])percentGroupSizes.Clone();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PercentGroupSizes", Environment.GetResourceString("Object cannot be null."));
			}
			VerifyWritable();
			int[] groupSize = (int[])value.Clone();
			CheckGroupSize("PercentGroupSizes", groupSize);
			percentGroupSizes = groupSize;
		}
	}

	/// <summary>Gets or sets the string that separates groups of digits to the left of the decimal in currency values.</summary>
	/// <returns>The string that separates groups of digits to the left of the decimal in currency values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is ",".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string CurrencyGroupSeparator
	{
		get
		{
			return currencyGroupSeparator;
		}
		set
		{
			VerifyWritable();
			VerifyGroupSeparator(value, "CurrencyGroupSeparator");
			currencyGroupSeparator = value;
		}
	}

	/// <summary>Gets or sets the string to use as the currency symbol.</summary>
	/// <returns>The string to use as the currency symbol. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is "¤". </returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string CurrencySymbol
	{
		get
		{
			return currencySymbol;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("CurrencySymbol", Environment.GetResourceString("String reference not set to an instance of a String."));
			}
			VerifyWritable();
			currencySymbol = value;
		}
	}

	/// <summary>Gets a read-only <see cref="T:System.Globalization.NumberFormatInfo" /> that formats values based on the current culture.</summary>
	/// <returns>A read-only <see cref="T:System.Globalization.NumberFormatInfo" /> based on the culture of the current thread.</returns>
	public static NumberFormatInfo CurrentInfo
	{
		get
		{
			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			if (!currentCulture.m_isInherited)
			{
				NumberFormatInfo numInfo = currentCulture.numInfo;
				if (numInfo != null)
				{
					return numInfo;
				}
			}
			return (NumberFormatInfo)currentCulture.GetFormat(typeof(NumberFormatInfo));
		}
	}

	/// <summary>Gets or sets the string that represents the IEEE NaN (not a number) value.</summary>
	/// <returns>The string that represents the IEEE NaN (not a number) value. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is "NaN".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string NaNSymbol
	{
		get
		{
			return nanSymbol;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("NaNSymbol", Environment.GetResourceString("String reference not set to an instance of a String."));
			}
			VerifyWritable();
			nanSymbol = value;
		}
	}

	/// <summary>Gets or sets the format pattern for negative currency values.</summary>
	/// <returns>The format pattern for negative currency values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is 0, which represents "($n)", where "$" is the <see cref="P:System.Globalization.NumberFormatInfo.CurrencySymbol" /> and <paramref name="n" /> is a number.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is being set to a value that is less than 0 or greater than 15. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int CurrencyNegativePattern
	{
		get
		{
			return currencyNegativePattern;
		}
		set
		{
			if (value < 0 || value > 15)
			{
				throw new ArgumentOutOfRangeException("CurrencyNegativePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 15));
			}
			VerifyWritable();
			currencyNegativePattern = value;
		}
	}

	/// <summary>Gets or sets the format pattern for negative numeric values.</summary>
	/// <returns>The format pattern for negative numeric values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is 1, which represents "-n", where <paramref name="n" /> is a number.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is being set to a value that is less than 0 or greater than 4. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int NumberNegativePattern
	{
		get
		{
			return numberNegativePattern;
		}
		set
		{
			if (value < 0 || value > 4)
			{
				throw new ArgumentOutOfRangeException("NumberNegativePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 4));
			}
			VerifyWritable();
			numberNegativePattern = value;
		}
	}

	/// <summary>Gets or sets the format pattern for positive percent values.</summary>
	/// <returns>The format pattern for positive percent values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is 0, which represents "n %", where "%" is the <see cref="P:System.Globalization.NumberFormatInfo.PercentSymbol" /> and <paramref name="n" /> is a number.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is being set to a value that is less than 0 or greater than 3. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int PercentPositivePattern
	{
		get
		{
			return percentPositivePattern;
		}
		set
		{
			if (value < 0 || value > 3)
			{
				throw new ArgumentOutOfRangeException("PercentPositivePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 3));
			}
			VerifyWritable();
			percentPositivePattern = value;
		}
	}

	/// <summary>Gets or sets the format pattern for negative percent values.</summary>
	/// <returns>The format pattern for negative percent values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is 0, which represents "-n %", where "%" is the <see cref="P:System.Globalization.NumberFormatInfo.PercentSymbol" /> and <paramref name="n" /> is a number.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is being set to a value that is less than 0 or greater than 11. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int PercentNegativePattern
	{
		get
		{
			return percentNegativePattern;
		}
		set
		{
			if (value < 0 || value > 11)
			{
				throw new ArgumentOutOfRangeException("PercentNegativePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 11));
			}
			VerifyWritable();
			percentNegativePattern = value;
		}
	}

	/// <summary>Gets or sets the string that represents negative infinity.</summary>
	/// <returns>The string that represents negative infinity. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is "-Infinity".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string NegativeInfinitySymbol
	{
		get
		{
			return negativeInfinitySymbol;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("NegativeInfinitySymbol", Environment.GetResourceString("String reference not set to an instance of a String."));
			}
			VerifyWritable();
			negativeInfinitySymbol = value;
		}
	}

	/// <summary>Gets or sets the string that denotes that the associated number is negative.</summary>
	/// <returns>The string that denotes that the associated number is negative. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is "-".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string NegativeSign
	{
		get
		{
			return negativeSign;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("NegativeSign", Environment.GetResourceString("String reference not set to an instance of a String."));
			}
			VerifyWritable();
			negativeSign = value;
		}
	}

	/// <summary>Gets or sets the number of decimal places to use in numeric values.</summary>
	/// <returns>The number of decimal places to use in numeric values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is 2.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is being set to a value that is less than 0 or greater than 99. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int NumberDecimalDigits
	{
		get
		{
			return numberDecimalDigits;
		}
		set
		{
			if (value < 0 || value > 99)
			{
				throw new ArgumentOutOfRangeException("NumberDecimalDigits", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 99));
			}
			VerifyWritable();
			numberDecimalDigits = value;
		}
	}

	/// <summary>Gets or sets the string to use as the decimal separator in numeric values.</summary>
	/// <returns>The string to use as the decimal separator in numeric values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is ".".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	/// <exception cref="T:System.ArgumentException">The property is being set to an empty string.</exception>
	public string NumberDecimalSeparator
	{
		get
		{
			return numberDecimalSeparator;
		}
		set
		{
			VerifyWritable();
			VerifyDecimalSeparator(value, "NumberDecimalSeparator");
			numberDecimalSeparator = value;
		}
	}

	/// <summary>Gets or sets the string that separates groups of digits to the left of the decimal in numeric values.</summary>
	/// <returns>The string that separates groups of digits to the left of the decimal in numeric values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is ",".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string NumberGroupSeparator
	{
		get
		{
			return numberGroupSeparator;
		}
		set
		{
			VerifyWritable();
			VerifyGroupSeparator(value, "NumberGroupSeparator");
			numberGroupSeparator = value;
		}
	}

	/// <summary>Gets or sets the format pattern for positive currency values.</summary>
	/// <returns>The format pattern for positive currency values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is 0, which represents "$n", where "$" is the <see cref="P:System.Globalization.NumberFormatInfo.CurrencySymbol" /> and <paramref name="n" /> is a number.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is being set to a value that is less than 0 or greater than 3. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int CurrencyPositivePattern
	{
		get
		{
			return currencyPositivePattern;
		}
		set
		{
			if (value < 0 || value > 3)
			{
				throw new ArgumentOutOfRangeException("CurrencyPositivePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 3));
			}
			VerifyWritable();
			currencyPositivePattern = value;
		}
	}

	/// <summary>Gets or sets the string that represents positive infinity.</summary>
	/// <returns>The string that represents positive infinity. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is "Infinity".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string PositiveInfinitySymbol
	{
		get
		{
			return positiveInfinitySymbol;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PositiveInfinitySymbol", Environment.GetResourceString("String reference not set to an instance of a String."));
			}
			VerifyWritable();
			positiveInfinitySymbol = value;
		}
	}

	/// <summary>Gets or sets the string that denotes that the associated number is positive.</summary>
	/// <returns>The string that denotes that the associated number is positive. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is "+".</returns>
	/// <exception cref="T:System.ArgumentNullException">In a set operation, the value to be assigned is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string PositiveSign
	{
		get
		{
			return positiveSign;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PositiveSign", Environment.GetResourceString("String reference not set to an instance of a String."));
			}
			VerifyWritable();
			positiveSign = value;
		}
	}

	/// <summary>Gets or sets the number of decimal places to use in percent values. </summary>
	/// <returns>The number of decimal places to use in percent values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is 2.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The property is being set to a value that is less than 0 or greater than 99. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public int PercentDecimalDigits
	{
		get
		{
			return percentDecimalDigits;
		}
		set
		{
			if (value < 0 || value > 99)
			{
				throw new ArgumentOutOfRangeException("PercentDecimalDigits", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Valid values are between {0} and {1}, inclusive."), 0, 99));
			}
			VerifyWritable();
			percentDecimalDigits = value;
		}
	}

	/// <summary>Gets or sets the string to use as the decimal separator in percent values. </summary>
	/// <returns>The string to use as the decimal separator in percent values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is ".".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	/// <exception cref="T:System.ArgumentException">The property is being set to an empty string.</exception>
	public string PercentDecimalSeparator
	{
		get
		{
			return percentDecimalSeparator;
		}
		set
		{
			VerifyWritable();
			VerifyDecimalSeparator(value, "PercentDecimalSeparator");
			percentDecimalSeparator = value;
		}
	}

	/// <summary>Gets or sets the string that separates groups of digits to the left of the decimal in percent values. </summary>
	/// <returns>The string that separates groups of digits to the left of the decimal in percent values. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is ",".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string PercentGroupSeparator
	{
		get
		{
			return percentGroupSeparator;
		}
		set
		{
			VerifyWritable();
			VerifyGroupSeparator(value, "PercentGroupSeparator");
			percentGroupSeparator = value;
		}
	}

	/// <summary>Gets or sets the string to use as the percent symbol.</summary>
	/// <returns>The string to use as the percent symbol. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is "%".</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string PercentSymbol
	{
		get
		{
			return percentSymbol;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PercentSymbol", Environment.GetResourceString("String reference not set to an instance of a String."));
			}
			VerifyWritable();
			percentSymbol = value;
		}
	}

	/// <summary>Gets or sets the string to use as the per mille symbol.</summary>
	/// <returns>The string to use as the per mille symbol. The default for <see cref="P:System.Globalization.NumberFormatInfo.InvariantInfo" /> is "‰", which is the Unicode character U+2030.</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is being set to null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The property is being set and the <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only. </exception>
	public string PerMilleSymbol
	{
		get
		{
			return perMilleSymbol;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PerMilleSymbol", Environment.GetResourceString("String reference not set to an instance of a String."));
			}
			VerifyWritable();
			perMilleSymbol = value;
		}
	}

	/// <summary>Gets or sets a string array of native digits equivalent to the Western digits 0 through 9.</summary>
	/// <returns>A string array that contains the native equivalent of the Western digits 0 through 9. The default is an array having the elements "0", "1", "2", "3", "4", "5", "6", "7", "8", and "9".</returns>
	/// <exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only.</exception>
	/// <exception cref="T:System.ArgumentNullException">In a set operation, the value is null.-or-In a set operation, an element of the value array is null.</exception>
	/// <exception cref="T:System.ArgumentException">In a set operation, the value array does not contain 10 elements.-or-In a set operation, an element of the value array does not contain either a single <see cref="T:System.Char" /> object or a pair of <see cref="T:System.Char" /> objects that comprise a surrogate pair.-or-In a set operation, an element of the value array is not a number digit as defined by the Unicode Standard. That is, the digit in the array element does not have the Unicode Number, Decimal Digit (Nd) General Category value.-or-In a set operation, the numeric value of an element in the value array does not correspond to the element's position in the array. That is, the element at index 0, which is the first element of the array, does not have a numeric value of 0, or the element at index 1 does not have a numeric value of 1. </exception>
	[ComVisible(false)]
	public string[] NativeDigits
	{
		get
		{
			return (string[])nativeDigits.Clone();
		}
		set
		{
			VerifyWritable();
			VerifyNativeDigits(value, "NativeDigits");
			nativeDigits = value;
		}
	}

	/// <summary>Gets or sets a value that specifies how the graphical user interface displays the shape of a digit.</summary>
	/// <returns>One of the enumeration values that specifies the culture-specific digit shape.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Globalization.NumberFormatInfo" /> object is read-only.</exception>
	/// <exception cref="T:System.ArgumentException">The value in a set operation is not a valid <see cref="T:System.Globalization.DigitShapes" /> value.</exception>
	[ComVisible(false)]
	public DigitShapes DigitSubstitution
	{
		get
		{
			return (DigitShapes)digitSubstitution;
		}
		set
		{
			VerifyWritable();
			VerifyDigitSubstitution(value, "DigitSubstitution");
			digitSubstitution = (int)value;
		}
	}

	/// <summary>Initializes a new writable instance of the <see cref="T:System.Globalization.NumberFormatInfo" /> class that is culture-independent (invariant).</summary>
	public NumberFormatInfo()
		: this(null)
	{
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext ctx)
	{
		if (numberDecimalSeparator != numberGroupSeparator)
		{
			validForParseAsNumber = true;
		}
		else
		{
			validForParseAsNumber = false;
		}
		if (numberDecimalSeparator != numberGroupSeparator && numberDecimalSeparator != currencyGroupSeparator && currencyDecimalSeparator != numberGroupSeparator && currencyDecimalSeparator != currencyGroupSeparator)
		{
			validForParseAsCurrency = true;
		}
		else
		{
			validForParseAsCurrency = false;
		}
	}

	[OnDeserializing]
	private void OnDeserializing(StreamingContext ctx)
	{
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext ctx)
	{
	}

	private static void VerifyDecimalSeparator(string decSep, string propertyName)
	{
		if (decSep == null)
		{
			throw new ArgumentNullException(propertyName, Environment.GetResourceString("String reference not set to an instance of a String."));
		}
		if (decSep.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Decimal separator cannot be the empty string."));
		}
	}

	private static void VerifyGroupSeparator(string groupSep, string propertyName)
	{
		if (groupSep == null)
		{
			throw new ArgumentNullException(propertyName, Environment.GetResourceString("String reference not set to an instance of a String."));
		}
	}

	private static void VerifyNativeDigits(string[] nativeDig, string propertyName)
	{
		if (nativeDig == null)
		{
			throw new ArgumentNullException(propertyName, Environment.GetResourceString("Array cannot be null."));
		}
		if (nativeDig.Length != 10)
		{
			throw new ArgumentException(Environment.GetResourceString("The NativeDigits array must contain exactly ten members."), propertyName);
		}
		for (int i = 0; i < nativeDig.Length; i++)
		{
			if (nativeDig[i] == null)
			{
				throw new ArgumentNullException(propertyName, Environment.GetResourceString("Found a null value within an array."));
			}
			if (nativeDig[i].Length != 1)
			{
				if (nativeDig[i].Length != 2)
				{
					throw new ArgumentException(Environment.GetResourceString("Each member of the NativeDigits array must be a single text element (one or more UTF16 code points) with a Unicode Nd (Number, Decimal Digit) property indicating it is a digit."), propertyName);
				}
				if (!char.IsSurrogatePair(nativeDig[i][0], nativeDig[i][1]))
				{
					throw new ArgumentException(Environment.GetResourceString("Each member of the NativeDigits array must be a single text element (one or more UTF16 code points) with a Unicode Nd (Number, Decimal Digit) property indicating it is a digit."), propertyName);
				}
			}
			if (CharUnicodeInfo.GetDecimalDigitValue(nativeDig[i], 0) != i && CharUnicodeInfo.GetUnicodeCategory(nativeDig[i], 0) != UnicodeCategory.PrivateUse)
			{
				throw new ArgumentException(Environment.GetResourceString("Each member of the NativeDigits array must be a single text element (one or more UTF16 code points) with a Unicode Nd (Number, Decimal Digit) property indicating it is a digit."), propertyName);
			}
		}
	}

	private static void VerifyDigitSubstitution(DigitShapes digitSub, string propertyName)
	{
		if ((uint)digitSub > 2u)
		{
			throw new ArgumentException(Environment.GetResourceString("The DigitSubstitution property must be of a valid member of the DigitShapes enumeration. Valid entries include Context, NativeNational or None."), propertyName);
		}
	}

	[SecuritySafeCritical]
	internal NumberFormatInfo(CultureData cultureData)
	{
		if (cultureData != null)
		{
			cultureData.GetNFIValues(this);
			if (cultureData.IsInvariantCulture)
			{
				m_isInvariant = true;
			}
		}
	}

	private void VerifyWritable()
	{
		if (isReadOnly)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Instance is read-only."));
		}
	}

	/// <summary>Gets the <see cref="T:System.Globalization.NumberFormatInfo" /> associated with the specified <see cref="T:System.IFormatProvider" />.</summary>
	/// <returns>The <see cref="T:System.Globalization.NumberFormatInfo" /> associated with the specified <see cref="T:System.IFormatProvider" />.</returns>
	/// <param name="formatProvider">The <see cref="T:System.IFormatProvider" /> used to get the <see cref="T:System.Globalization.NumberFormatInfo" />.-or- null to get <see cref="P:System.Globalization.NumberFormatInfo.CurrentInfo" />. </param>
	public static NumberFormatInfo GetInstance(IFormatProvider formatProvider)
	{
		if (formatProvider is CultureInfo { m_isInherited: false } cultureInfo)
		{
			NumberFormatInfo numInfo = cultureInfo.numInfo;
			if (numInfo != null)
			{
				return numInfo;
			}
			return cultureInfo.NumberFormat;
		}
		if (formatProvider is NumberFormatInfo result)
		{
			return result;
		}
		if (formatProvider != null && formatProvider.GetFormat(typeof(NumberFormatInfo)) is NumberFormatInfo result2)
		{
			return result2;
		}
		return CurrentInfo;
	}

	/// <summary>Creates a shallow copy of the <see cref="T:System.Globalization.NumberFormatInfo" /> object.</summary>
	/// <returns>A new object copied from the original <see cref="T:System.Globalization.NumberFormatInfo" /> object.</returns>
	public object Clone()
	{
		NumberFormatInfo obj = (NumberFormatInfo)MemberwiseClone();
		obj.isReadOnly = false;
		return obj;
	}

	internal static void CheckGroupSize(string propName, int[] groupSize)
	{
		for (int i = 0; i < groupSize.Length; i++)
		{
			if (groupSize[i] < 1)
			{
				if (i == groupSize.Length - 1 && groupSize[i] == 0)
				{
					break;
				}
				throw new ArgumentException(Environment.GetResourceString("Every element in the value array should be between one and nine, except for the last element, which can be zero."), propName);
			}
			if (groupSize[i] > 9)
			{
				throw new ArgumentException(Environment.GetResourceString("Every element in the value array should be between one and nine, except for the last element, which can be zero."), propName);
			}
		}
	}

	/// <summary>Gets an object of the specified type that provides a number formatting service.</summary>
	/// <returns>The current <see cref="T:System.Globalization.NumberFormatInfo" />, if <paramref name="formatType" /> is the same as the type of the current <see cref="T:System.Globalization.NumberFormatInfo" />; otherwise, null.</returns>
	/// <param name="formatType">The <see cref="T:System.Type" /> of the required formatting service. </param>
	public object GetFormat(Type formatType)
	{
		if (!(formatType == typeof(NumberFormatInfo)))
		{
			return null;
		}
		return this;
	}

	/// <summary>Returns a read-only <see cref="T:System.Globalization.NumberFormatInfo" /> wrapper.</summary>
	/// <returns>A read-only <see cref="T:System.Globalization.NumberFormatInfo" /> wrapper around <paramref name="nfi" />.</returns>
	/// <param name="nfi">The <see cref="T:System.Globalization.NumberFormatInfo" /> to wrap. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="nfi" /> is null. </exception>
	public static NumberFormatInfo ReadOnly(NumberFormatInfo nfi)
	{
		if (nfi == null)
		{
			throw new ArgumentNullException("nfi");
		}
		if (nfi.IsReadOnly)
		{
			return nfi;
		}
		NumberFormatInfo obj = (NumberFormatInfo)nfi.MemberwiseClone();
		obj.isReadOnly = true;
		return obj;
	}

	internal static void ValidateParseStyleInteger(NumberStyles style)
	{
		if ((style & ~(NumberStyles.Any | NumberStyles.AllowHexSpecifier)) != 0)
		{
			throw new ArgumentException(Environment.GetResourceString("An undefined NumberStyles value is being used."), "style");
		}
		if ((style & NumberStyles.AllowHexSpecifier) != 0 && (style & ~NumberStyles.HexNumber) != 0)
		{
			throw new ArgumentException(Environment.GetResourceString("With the AllowHexSpecifier bit set in the enum bit field, the only other valid bits that can be combined into the enum value must be a subset of those in HexNumber."));
		}
	}

	internal static void ValidateParseStyleFloatingPoint(NumberStyles style)
	{
		if ((style & ~(NumberStyles.Any | NumberStyles.AllowHexSpecifier)) != 0)
		{
			throw new ArgumentException(Environment.GetResourceString("An undefined NumberStyles value is being used."), "style");
		}
		if ((style & NumberStyles.AllowHexSpecifier) != 0)
		{
			throw new ArgumentException(Environment.GetResourceString("The number style AllowHexSpecifier is not supported on floating point data types."));
		}
	}
}
