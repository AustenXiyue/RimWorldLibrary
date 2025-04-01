using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert <see cref="T:System.DateTimeOffset" /> structures to and from various other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class DateTimeOffsetConverter : TypeConverter
{
	/// <summary>Returns a value that indicates whether an object of the specified source type can be converted to a <see cref="T:System.DateTimeOffset" />.</summary>
	/// <returns>true if the specified type can be converted to a <see cref="T:System.DateTimeOffset" />; otherwise, false.</returns>
	/// <param name="context">The date format context.</param>
	/// <param name="sourceType">The source type to check.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Returns a value that indicates whether a <see cref="T:System.DateTimeOffset" /> can be converted to an object of the specified type.</summary>
	/// <returns>true if a <see cref="T:System.DateTimeOffset" /> can be converted to the specified type; otherwise, false.</returns>
	/// <param name="context">The date format context.</param>
	/// <param name="destinationType">The destination type to check.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the specified object to a <see cref="T:System.DateTimeOffset" />.</summary>
	/// <returns>A <see cref="T:System.DateTimeOffset" /> that represents the specified object.</returns>
	/// <param name="context">The date format context.</param>
	/// <param name="culture">The date culture.</param>
	/// <param name="value">The object to be converted.</param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string text = ((string)value).Trim();
			if (text.Length == 0)
			{
				return DateTimeOffset.MinValue;
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
					return DateTimeOffset.Parse(text, dateTimeFormatInfo);
				}
				return DateTimeOffset.Parse(text, culture);
			}
			catch (FormatException innerException)
			{
				throw new FormatException(global::SR.GetString("{0} is not a valid value for {1}.", (string)value, "DateTimeOffset"), innerException);
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts a <see cref="T:System.DateTimeOffset" /> to an object of the specified type.</summary>
	/// <returns>An object of the specified type that represents the <see cref="T:System.DateTimeOffset" />. </returns>
	/// <param name="context">The date format context.</param>
	/// <param name="culture">The date culture.</param>
	/// <param name="value">The <see cref="T:System.DateTimeOffset" /> to be converted.</param>
	/// <param name="destinationType">The type to convert to.</param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is DateTimeOffset dateTimeOffset)
		{
			if (dateTimeOffset == DateTimeOffset.MinValue)
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
				if (dateTimeOffset.TimeOfDay.TotalSeconds == 0.0)
				{
					return dateTimeOffset.ToString("yyyy-MM-dd zzz", culture);
				}
				return dateTimeOffset.ToString(culture);
			}
			string text = ((dateTimeOffset.TimeOfDay.TotalSeconds != 0.0) ? (dateTimeFormatInfo.ShortDatePattern + " " + dateTimeFormatInfo.ShortTimePattern + " zzz") : (dateTimeFormatInfo.ShortDatePattern + " zzz"));
			return dateTimeOffset.ToString(text, CultureInfo.CurrentCulture);
		}
		if (destinationType == typeof(InstanceDescriptor) && value is DateTimeOffset dateTimeOffset2)
		{
			if (dateTimeOffset2.Ticks == 0L)
			{
				ConstructorInfo constructor = typeof(DateTimeOffset).GetConstructor(new Type[1] { typeof(long) });
				if (constructor != null)
				{
					return new InstanceDescriptor(constructor, new object[1] { dateTimeOffset2.Ticks });
				}
			}
			ConstructorInfo constructor2 = typeof(DateTimeOffset).GetConstructor(new Type[8]
			{
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(int),
				typeof(TimeSpan)
			});
			if (constructor2 != null)
			{
				return new InstanceDescriptor(constructor2, new object[8] { dateTimeOffset2.Year, dateTimeOffset2.Month, dateTimeOffset2.Day, dateTimeOffset2.Hour, dateTimeOffset2.Minute, dateTimeOffset2.Second, dateTimeOffset2.Millisecond, dateTimeOffset2.Offset });
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DateTimeOffsetConverter" /> class. </summary>
	public DateTimeOffsetConverter()
	{
	}
}
