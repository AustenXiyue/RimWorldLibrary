using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a base type converter for nonfloating-point numerical types.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class BaseNumberConverter : TypeConverter
{
	internal virtual bool AllowHex => true;

	internal abstract Type TargetType { get; }

	internal abstract object FromString(string value, int radix);

	internal abstract object FromString(string value, NumberFormatInfo formatInfo);

	internal abstract object FromString(string value, CultureInfo culture);

	internal virtual Exception FromStringError(string failedText, Exception innerException)
	{
		return new Exception(global::SR.GetString("{0} is not a valid value for {1}.", failedText, TargetType.Name), innerException);
	}

	internal abstract string ToString(object value, NumberFormatInfo formatInfo);

	/// <summary>Determines if this converter can convert an object in the given source type to the native type of the converter.</summary>
	/// <returns>true if this converter can perform the operation; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type from which you want to convert. </param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Converts the given object to the converter's native type.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that specifies the culture to represent the number. </param>
	/// <param name="value">The object to convert. </param>
	/// <exception cref="T:System.Exception">
	///   <paramref name="value" /> is not a valid value for the target type.</exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string text = ((string)value).Trim();
			try
			{
				if (AllowHex && text[0] == '#')
				{
					return FromString(text.Substring(1), 16);
				}
				if ((AllowHex && text.StartsWith("0x")) || text.StartsWith("0X") || text.StartsWith("&h") || text.StartsWith("&H"))
				{
					return FromString(text.Substring(2), 16);
				}
				if (culture == null)
				{
					culture = CultureInfo.CurrentCulture;
				}
				NumberFormatInfo formatInfo = (NumberFormatInfo)culture.GetFormat(typeof(NumberFormatInfo));
				return FromString(text, formatInfo);
			}
			catch (Exception innerException)
			{
				throw FromStringError(text, innerException);
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the specified object to another type.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that specifies the culture to represent the number. </param>
	/// <param name="value">The object to convert. </param>
	/// <param name="destinationType">The type to convert the object to. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string) && value != null && TargetType.IsInstanceOfType(value))
		{
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)culture.GetFormat(typeof(NumberFormatInfo));
			return ToString(value, numberFormatInfo);
		}
		if (destinationType.IsPrimitive)
		{
			return Convert.ChangeType(value, destinationType, culture);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Returns a value indicating whether this converter can convert an object to the given destination type using the context.</summary>
	/// <returns>true if this converter can perform the operation; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="t">A <see cref="T:System.Type" /> that represents the type to which you want to convert. </param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
	{
		if (base.CanConvertTo(context, t) || t.IsPrimitive)
		{
			return true;
		}
		return false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.BaseNumberConverter" /> class.</summary>
	protected BaseNumberConverter()
	{
	}
}
