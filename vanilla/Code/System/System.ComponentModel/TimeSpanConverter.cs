using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert <see cref="T:System.TimeSpan" /> objects to and from other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class TimeSpanConverter : TypeConverter
{
	/// <summary>Gets a value indicating whether this converter can convert an object in the given source type to a <see cref="T:System.TimeSpan" /> using the specified context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you wish to convert from. </param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Gets a value indicating whether this converter can convert an object to the given destination type using the context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you wish to convert to. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a valid value for the target type. </exception>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the given object to a <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">An optional <see cref="T:System.Globalization.CultureInfo" />. If not supplied, the current culture is assumed. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a valid value for the target type. </exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string input = ((string)value).Trim();
			try
			{
				return TimeSpan.Parse(input, culture);
			}
			catch (FormatException innerException)
			{
				throw new FormatException(global::SR.GetString("{0} is not a valid value for {1}.", (string)value, "TimeSpan"), innerException);
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the given object to another type. </summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A formatter context. </param>
	/// <param name="culture">The culture into which <paramref name="value" /> will be converted.</param>
	/// <param name="value">The object to convert. </param>
	/// <param name="destinationType">The type to convert the object to. </param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(InstanceDescriptor) && value is TimeSpan)
		{
			MethodInfo method = typeof(TimeSpan).GetMethod("Parse", new Type[1] { typeof(string) });
			if (method != null)
			{
				return new InstanceDescriptor(method, new object[1] { value.ToString() });
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.TimeSpanConverter" /> class. </summary>
	public TimeSpanConverter()
	{
	}
}
