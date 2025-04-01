using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert <see cref="T:System.DateTime" /> objects to and from various other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class DateTimeConverter : TypeConverter
{
	/// <summary>Gets a value indicating whether this converter can convert an object in the given source type to a <see cref="T:System.DateTime" /> using the specified context.</summary>
	/// <returns>true if this object can perform the conversion; otherwise, false.</returns>
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
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the given value object to a <see cref="T:System.DateTime" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted <paramref name="value" />.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">An optional <see cref="T:System.Globalization.CultureInfo" />. If not supplied, the current culture is assumed. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="value" /> is not a valid value for the target type. </exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string text = ((string)value).Trim();
			if (text.Length == 0)
			{
				return DateTime.MinValue;
			}
			try
			{
				DateTimeFormatInfo dateTimeFormatInfo = null;
				if (culture != null)
				{
					dateTimeFormatInfo = (DateTimeFormatInfo)culture.GetFormat(typeof(DateTimeFormatInfo));
				}
				if (dateTimeFormatInfo != null)
				{
					return DateTime.Parse(text, dateTimeFormatInfo);
				}
				return DateTime.Parse(text, culture);
			}
			catch (FormatException innerException)
			{
				throw new FormatException(global::SR.GetString("{0} is not a valid value for {1}.", (string)value, "DateTime"), innerException);
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the given value object to a <see cref="T:System.DateTime" /> using the arguments.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted <paramref name="value" />.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">An optional <see cref="T:System.Globalization.CultureInfo" />. If not supplied, the current culture is assumed. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the value to. </param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is DateTime dateTime)
		{
			if (dateTime == DateTime.MinValue)
			{
				return string.Empty;
			}
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}
			DateTimeFormatInfo dateTimeFormatInfo = null;
			dateTimeFormatInfo = (DateTimeFormatInfo)culture.GetFormat(typeof(DateTimeFormatInfo));
			if (culture == CultureInfo.InvariantCulture)
			{
				if (dateTime.TimeOfDay.TotalSeconds == 0.0)
				{
					return dateTime.ToString("yyyy-MM-dd", culture);
				}
				return dateTime.ToString(culture);
			}
			string text = ((dateTime.TimeOfDay.TotalSeconds != 0.0) ? (dateTimeFormatInfo.ShortDatePattern + " " + dateTimeFormatInfo.ShortTimePattern) : dateTimeFormatInfo.ShortDatePattern);
			return dateTime.ToString(text, CultureInfo.CurrentCulture);
		}
		if (destinationType == typeof(InstanceDescriptor) && value is DateTime dateTime2)
		{
			if (dateTime2.Ticks == 0L)
			{
				ConstructorInfo constructor = typeof(DateTime).GetConstructor(new Type[1] { typeof(long) });
				if (constructor != null)
				{
					return new InstanceDescriptor(constructor, new object[1] { dateTime2.Ticks });
				}
			}
			ConstructorInfo constructor2 = typeof(DateTime).GetConstructor(new Type[7]
			{
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int)
			});
			if (constructor2 != null)
			{
				return new InstanceDescriptor(constructor2, new object[7] { dateTime2.Year, dateTime2.Month, dateTime2.Day, dateTime2.Hour, dateTime2.Minute, dateTime2.Second, dateTime2.Millisecond });
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DateTimeConverter" /> class.</summary>
	public DateTimeConverter()
	{
	}
}
