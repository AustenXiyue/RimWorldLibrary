using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert Unicode character objects to and from various other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class CharConverter : TypeConverter
{
	/// <summary>Gets a value indicating whether this converter can convert an object in the given source type to a Unicode character object using the specified context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from. </param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Converts the given value object to a Unicode character object using the arguments.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The culture into which <paramref name="value" /> will be converted.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the value to. </param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is char && (char)value == '\0')
		{
			return "";
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Converts the given object to a Unicode character object.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted <paramref name="value" />.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The culture into which <paramref name="value" /> will be converted.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a valid value for the target type. </exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string text = (string)value;
			if (text.Length > 1)
			{
				text = text.Trim();
			}
			if (text != null && text.Length > 0)
			{
				if (text.Length != 1)
				{
					throw new FormatException(global::SR.GetString("{0} is not a valid value for {1}.", text, "Char"));
				}
				return text[0];
			}
			return '\0';
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.CharConverter" /> class. </summary>
	public CharConverter()
	{
	}
}
