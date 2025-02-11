using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Windows;

/// <summary>Converts instances of other types to and from instances of a <see cref="T:System.Double" /> that represent an object's length.</summary>
public class LengthConverter : TypeConverter
{
	private static string[] PixelUnitStrings = new string[4] { "px", "in", "cm", "pt" };

	private static double[] PixelUnitFactors = new double[4] { 1.0, 96.0, 37.79527559055118, 1.3333333333333333 };

	/// <summary>Determines whether conversion is possible from a specified type to a <see cref="T:System.Double" /> that represents an object's length. </summary>
	/// <returns>true if conversion is possible; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">Provides contextual information about a component.</param>
	/// <param name="sourceType">Identifies the data type to evaluate for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		TypeCode typeCode = Type.GetTypeCode(sourceType);
		if ((uint)(typeCode - 7) <= 8u || typeCode == TypeCode.String)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether conversion is possible to a specified type from a <see cref="T:System.Double" /> that represents an object's length. </summary>
	/// <returns>true if conversion to the <paramref name="destinationType" /> is possible; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">Provides contextual information about a component.</param>
	/// <param name="destinationType">Identifies the data type to evaluate for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Converts instances of other data types into instances of <see cref="T:System.Double" /> that represent an object's length. </summary>
	/// <returns>An instance of <see cref="T:System.Double" /> that is the value of the conversion.</returns>
	/// <param name="typeDescriptorContext">Provides contextual information about a component.</param>
	/// <param name="cultureInfo">Represents culture-specific information that is maintained during a conversion.</param>
	/// <param name="source">Identifies the object that is being converted to <see cref="T:System.Double" />.</param>
	/// <exception cref="T:System.ArgumentNullException">Occurs if the <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">Occurs if the <paramref name="source" /> is not null and is not a valid type for conversion.</exception>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		if (source != null)
		{
			if (source is string)
			{
				return FromString((string)source, cultureInfo);
			}
			return Convert.ToDouble(source, cultureInfo);
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Converts other types into instances of <see cref="T:System.Double" /> that represent an object's length. </summary>
	/// <returns>A new <see cref="T:System.Object" /> that is the value of the conversion.</returns>
	/// <param name="typeDescriptorContext">Describes context information of a component, such as its container and <see cref="T:System.ComponentModel.PropertyDescriptor" />.</param>
	/// <param name="cultureInfo">Identifies culture-specific information, including the writing system and the calendar that is used.</param>
	/// <param name="value">Identifies the <see cref="T:System.Object" /> that is being converted.</param>
	/// <param name="destinationType">The data type that this instance of <see cref="T:System.Double" /> is being converted to.</param>
	/// <exception cref="T:System.ArgumentNullException">Occurs if the <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">Occurs if the <paramref name="value" /> is not null and is not a <see cref="T:System.Windows.Media.Brush" />, or the <paramref name="destinationType" /> is not valid.</exception>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value != null && value is double num)
		{
			if (destinationType == typeof(string))
			{
				if (double.IsNaN(num))
				{
					return "Auto";
				}
				return Convert.ToString(num, cultureInfo);
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(double).GetConstructor(new Type[1] { typeof(double) }), new object[1] { num });
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	internal static double FromString(string s, CultureInfo cultureInfo)
	{
		string text = s.Trim();
		string text2 = text.ToLowerInvariant();
		int length = text2.Length;
		int num = 0;
		double num2 = 1.0;
		if (text2 == "auto")
		{
			return double.NaN;
		}
		for (int i = 0; i < PixelUnitStrings.Length; i++)
		{
			if (text2.EndsWith(PixelUnitStrings[i], StringComparison.Ordinal))
			{
				num = PixelUnitStrings[i].Length;
				num2 = PixelUnitFactors[i];
				break;
			}
		}
		text = text.Substring(0, length - num);
		try
		{
			return Convert.ToDouble(text, cultureInfo) * num2;
		}
		catch (FormatException)
		{
			throw new FormatException(SR.Format(SR.LengthFormatError, text));
		}
	}

	internal static string ToString(double l, CultureInfo cultureInfo)
	{
		if (double.IsNaN(l))
		{
			return "Auto";
		}
		return Convert.ToString(l, cultureInfo);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.LengthConverter" /> class. </summary>
	public LengthConverter()
	{
	}
}
