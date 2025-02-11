using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Converts instances of various types to and from instances of the <see cref="T:System.Windows.Controls.DataGridLength" /> class.</summary>
public class DataGridLengthConverter : TypeConverter
{
	private static string[] _unitStrings = new string[5] { "auto", "px", "sizetocells", "sizetoheader", "*" };

	private const int NumDescriptiveUnits = 3;

	private static string[] _nonStandardUnitStrings = new string[3] { "in", "cm", "pt" };

	private static double[] _pixelUnitFactors = new double[3] { 96.0, 37.79527559055118, 1.3333333333333333 };

	/// <summary>Determines whether an instance of the specified type can be converted to an instance of the <see cref="T:System.Windows.Controls.DataGridLength" /> class.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An object that provides a format context.</param>
	/// <param name="sourceType">The type to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		TypeCode typeCode = Type.GetTypeCode(sourceType);
		if ((uint)(typeCode - 6) <= 9u || typeCode == TypeCode.String)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of the <see cref="T:System.Windows.Controls.DataGridLength" /> class can be converted to an instance of the specified type.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An object that provides a format context.</param>
	/// <param name="destinationType">The type to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (!(destinationType == typeof(string)))
		{
			return destinationType == typeof(InstanceDescriptor);
		}
		return true;
	}

	/// <summary>Converts the specified object to an instance of the <see cref="T:System.Windows.Controls.DataGridLength" /> class.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="context">An object that provides a format context.</param>
	/// <param name="culture">The object to use as the current culture.</param>
	/// <param name="value">The value to convert.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a valid type that can be converted to type <see cref="T:System.Windows.Controls.DataGridLength" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value != null)
		{
			if (value is string s)
			{
				return ConvertFromString(s, culture);
			}
			double num = Convert.ToDouble(value, culture);
			DataGridLengthUnitType type;
			if (double.IsNaN(num))
			{
				num = 1.0;
				type = DataGridLengthUnitType.Auto;
			}
			else
			{
				type = DataGridLengthUnitType.Pixel;
			}
			if (!double.IsInfinity(num))
			{
				return new DataGridLength(num, type);
			}
		}
		throw GetConvertFromException(value);
	}

	/// <summary>Converts an instance of the <see cref="T:System.Windows.Controls.DataGridLength" /> class to an instance of the specified type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="context">An object that provides a format context.</param>
	/// <param name="culture">The object to use as the current culture.</param>
	/// <param name="value">The <see cref="T:System.Windows.Controls.DataGridLength" /> to convert.</param>
	/// <param name="destinationType">The type to convert the value to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.Windows.Controls.DataGridLength" /> or <paramref name="destinationType" /> is not a valid conversion type.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value != null && value is DataGridLength length)
		{
			if (destinationType == typeof(string))
			{
				return ConvertToString(length, culture);
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(DataGridLength).GetConstructor(new Type[2]
				{
					typeof(double),
					typeof(DataGridLengthUnitType)
				}), new object[2] { length.Value, length.UnitType });
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	internal static string ConvertToString(DataGridLength length, CultureInfo cultureInfo)
	{
		switch (length.UnitType)
		{
		case DataGridLengthUnitType.Auto:
		case DataGridLengthUnitType.SizeToCells:
		case DataGridLengthUnitType.SizeToHeader:
			return length.UnitType.ToString();
		case DataGridLengthUnitType.Star:
			if (!DoubleUtil.IsOne(length.Value))
			{
				return Convert.ToString(length.Value, cultureInfo) + "*";
			}
			return "*";
		default:
			return Convert.ToString(length.Value, cultureInfo);
		}
	}

	private static DataGridLength ConvertFromString(string s, CultureInfo cultureInfo)
	{
		string text = s.Trim().ToLowerInvariant();
		for (int i = 0; i < 3; i++)
		{
			string text2 = _unitStrings[i];
			if (text == text2)
			{
				return new DataGridLength(1.0, (DataGridLengthUnitType)i);
			}
		}
		double value = 0.0;
		DataGridLengthUnitType dataGridLengthUnitType = DataGridLengthUnitType.Pixel;
		int length = text.Length;
		int num = 0;
		double num2 = 1.0;
		int num3 = _unitStrings.Length;
		for (int j = 3; j < num3; j++)
		{
			string text3 = _unitStrings[j];
			if (text.EndsWith(text3, StringComparison.Ordinal))
			{
				num = text3.Length;
				dataGridLengthUnitType = (DataGridLengthUnitType)j;
				break;
			}
		}
		if (num == 0)
		{
			num3 = _nonStandardUnitStrings.Length;
			for (int k = 0; k < num3; k++)
			{
				string text4 = _nonStandardUnitStrings[k];
				if (text.EndsWith(text4, StringComparison.Ordinal))
				{
					num = text4.Length;
					num2 = _pixelUnitFactors[k];
					break;
				}
			}
		}
		if (length == num)
		{
			if (dataGridLengthUnitType == DataGridLengthUnitType.Star)
			{
				value = 1.0;
			}
		}
		else
		{
			value = double.Parse(text.AsSpan(0, length - num), cultureInfo) * num2;
		}
		return new DataGridLength(value, dataGridLengthUnitType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridLengthConverter" /> class. </summary>
	public DataGridLengthConverter()
	{
	}
}
