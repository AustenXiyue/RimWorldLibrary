using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Windows;

/// <summary>Converts instances of <see cref="T:System.Windows.Duration" /> to and from other type representations.</summary>
public class DurationConverter : TypeConverter
{
	private static TimeSpanConverter _timeSpanConverter;

	/// <summary>Determines if conversion from a given type to an instance of <see cref="T:System.Windows.Duration" /> is possible.</summary>
	/// <returns>true if <paramref name="t" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="td">Context information used for conversion.</param>
	/// <param name="t">Type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext td, Type t)
	{
		if (t == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines if conversion to a specified type is possible.</summary>
	/// <returns>true if <paramref name="destinationType" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">Context information used for conversion.</param>
	/// <param name="destinationType">Type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Converts a given string value to an instance of <see cref="T:System.Windows.Duration" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Duration" />.</returns>
	/// <param name="td">Context information used for conversion.</param>
	/// <param name="cultureInfo">Cultural information that is respected during conversion.</param>
	/// <param name="value">String value to convert to an instance of <see cref="T:System.Windows.Duration" />.</param>
	public override object ConvertFrom(ITypeDescriptorContext td, CultureInfo cultureInfo, object value)
	{
		if (value is string text)
		{
			string text2 = text.Trim();
			if (text2 == "Automatic")
			{
				return Duration.Automatic;
			}
			if (text2 == "Forever")
			{
				return Duration.Forever;
			}
		}
		_ = TimeSpan.Zero;
		if (_timeSpanConverter == null)
		{
			_timeSpanConverter = new TimeSpanConverter();
		}
		return new Duration((TimeSpan)_timeSpanConverter.ConvertFrom(td, cultureInfo, value));
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Duration" /> to another type.</summary>
	/// <returns>A new instance of the <paramref name="destinationType" />.</returns>
	/// <param name="context">Context information used for conversion.</param>
	/// <param name="cultureInfo">Cultural information that is respected during conversion.</param>
	/// <param name="value">Duration value to convert from.</param>
	/// <param name="destinationType">Type being evaluated for conversion.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (destinationType != null && value is Duration duration)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				if (duration.HasTimeSpan)
				{
					return new InstanceDescriptor(typeof(Duration).GetConstructor(new Type[1] { typeof(TimeSpan) }), new object[1] { duration.TimeSpan });
				}
				if (duration == Duration.Forever)
				{
					return new InstanceDescriptor(typeof(Duration).GetProperty("Forever"), null);
				}
				return new InstanceDescriptor(typeof(Duration).GetProperty("Automatic"), null);
			}
			if (destinationType == typeof(string))
			{
				return duration.ToString();
			}
		}
		return base.ConvertTo(context, cultureInfo, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DurationConverter" /> class.</summary>
	public DurationConverter()
	{
	}
}
