using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Windows;

/// <summary>Converts font size values to and from other type representations.</summary>
public class FontSizeConverter : TypeConverter
{
	/// <summary>Determines if conversion from a specified type to a <see cref="T:System.Double" /> value is possible.</summary>
	/// <returns>true if <paramref name="sourceType" /> can be converted to <see cref="T:System.Double" />; otherwise, false.</returns>
	/// <param name="context">Describes context information of a component such as its container and <see cref="T:System.ComponentModel.PropertyDescriptor" />.</param>
	/// <param name="sourceType">Identifies the data type to evaluate for purposes of conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (!(sourceType == typeof(string)) && !(sourceType == typeof(int)) && !(sourceType == typeof(float)))
		{
			return sourceType == typeof(double);
		}
		return true;
	}

	/// <summary>Determines if conversion of a font size value to a specified type is possible.</summary>
	/// <returns>true if this type can be converted; otherwise, false.</returns>
	/// <param name="context">Context information of a component such as its container and <see cref="T:System.ComponentModel.PropertyDescriptor" />.</param>
	/// <param name="destinationType">The data type to evaluate for purposes of conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts a specified type to a <see cref="T:System.Double" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> value that represents the converted font size value.</returns>
	/// <param name="context">Context information of a component such as its container and <see cref="T:System.ComponentModel.PropertyDescriptor" />.</param>
	/// <param name="culture">Cultural specific information, including the writing system and calendar used.</param>
	/// <param name="value">The value which is being converted to a font size value.</param>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string text)
		{
			FromString(text, culture, out var amount);
			return amount;
		}
		if (value is int || value is float || value is double)
		{
			return (double)value;
		}
		return null;
	}

	/// <summary>Converts a <see cref="T:System.Double" /> value to a specified type.</summary>
	/// <returns>A new <see cref="T:System.Object" /> that is the value of the conversion.</returns>
	/// <param name="context">Context information of a component such as its container and <see cref="T:System.ComponentModel.PropertyDescriptor" />.</param>
	/// <param name="culture">Cultural specific information, including writing system and calendar used.</param>
	/// <param name="value">The <see cref="T:System.Object" /> being converted.</param>
	/// <param name="destinationType">The data type this font size value is being converted to.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		double num = (double)value;
		if (destinationType == typeof(string))
		{
			return num.ToString(culture);
		}
		if (destinationType == typeof(int))
		{
			return (int)num;
		}
		if (destinationType == typeof(float))
		{
			return (float)num;
		}
		if (destinationType == typeof(double))
		{
			return num;
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	internal static void FromString(string text, CultureInfo culture, out double amount)
	{
		amount = LengthConverter.FromString(text, culture);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FontSizeConverter" /> class.</summary>
	public FontSizeConverter()
	{
	}
}
