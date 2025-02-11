using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Windows.Media.Animation;

/// <summary>Converts instances of <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> to and from other data types.</summary>
public sealed class RepeatBehaviorConverter : TypeConverter
{
	private const char _iterationCharacter = 'x';

	/// <summary>Determines whether or not conversion from a specified data type is possible.</summary>
	/// <returns>true if conversion is supported; otherwise, false.</returns>
	/// <param name="td">Context information required for conversion.</param>
	/// <param name="t">Type to evaluate for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext td, Type t)
	{
		if (t == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines if conversion to a specified type is possible. </summary>
	/// <returns>true if conversion is possible; otherwise, false.</returns>
	/// <param name="context">Context information required for conversion.</param>
	/// <param name="destinationType">Destination type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Converts a given string value to an instance of <see cref="T:System.Windows.Media.Animation.RepeatBehaviorConverter" />.</summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> object based on <paramref name="value" />.</returns>
	/// <param name="td">Context information required for conversion.</param>
	/// <param name="cultureInfo">Cultural information to respect during conversion.</param>
	/// <param name="value">Object being evaluated for conversion.</param>
	public override object ConvertFrom(ITypeDescriptorContext td, CultureInfo cultureInfo, object value)
	{
		string text = value as string;
		if (text != null)
		{
			text = text.Trim();
			if (text == "Forever")
			{
				return RepeatBehavior.Forever;
			}
			if (text.Length > 0 && text[text.Length - 1] == 'x')
			{
				string value2 = text.TrimEnd('x');
				return new RepeatBehavior((double)TypeDescriptor.GetConverter(typeof(double)).ConvertFrom(td, cultureInfo, value2));
			}
		}
		return new RepeatBehavior((TimeSpan)TypeDescriptor.GetConverter(typeof(TimeSpan)).ConvertFrom(td, cultureInfo, text));
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> to a supported destination type.</summary>
	/// <returns>The only supported destination types are <see cref="T:System.String" /> and <see cref="T:System.ComponentModel.Design.Serialization.InstanceDescriptor" />.</returns>
	/// <param name="context">Context information required for conversion.</param>
	/// <param name="cultureInfo">Cultural information to respect during conversion.</param>
	/// <param name="value">Object being evaluated for conversion.</param>
	/// <param name="destinationType">Destination type being evaluated for conversion.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (value is RepeatBehavior && destinationType != null)
		{
			RepeatBehavior repeatBehavior = (RepeatBehavior)value;
			if (destinationType == typeof(InstanceDescriptor))
			{
				if (repeatBehavior == RepeatBehavior.Forever)
				{
					return new InstanceDescriptor(typeof(RepeatBehavior).GetProperty("Forever"), null);
				}
				if (repeatBehavior.HasCount)
				{
					return new InstanceDescriptor(typeof(RepeatBehavior).GetConstructor(new Type[1] { typeof(double) }), new object[1] { repeatBehavior.Count });
				}
				if (repeatBehavior.HasDuration)
				{
					return new InstanceDescriptor(typeof(RepeatBehavior).GetConstructor(new Type[1] { typeof(TimeSpan) }), new object[1] { repeatBehavior.Duration });
				}
			}
			else if (destinationType == typeof(string))
			{
				return repeatBehavior.InternalToString(null, cultureInfo);
			}
		}
		return base.ConvertTo(context, cultureInfo, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.RepeatBehaviorConverter" /> class.</summary>
	public RepeatBehaviorConverter()
	{
	}
}
