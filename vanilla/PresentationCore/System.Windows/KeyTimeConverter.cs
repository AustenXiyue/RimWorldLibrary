using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Windows.Media.Animation;

namespace System.Windows;

/// <summary>Converts instances of <see cref="T:System.Windows.Media.Animation.KeyTime" /> to and from other types.</summary>
public class KeyTimeConverter : TypeConverter
{
	/// <summary>Determines whether an object can be converted from a given type to an instance of a <see cref="T:System.Windows.Media.Animation.KeyTime" />. </summary>
	/// <returns>true if this type can be converted; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">Contextual information required for conversion.</param>
	/// <param name="type">Type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type type)
	{
		if (type == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(typeDescriptorContext, type);
	}

	/// <summary>Determines if a given type can be converted to an instance of <see cref="T:System.Windows.Media.Animation.KeyTime" />. </summary>
	/// <returns>true if this type can be converted; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">Contextual information required for conversion.</param>
	/// <param name="type">Type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type type)
	{
		if (type == typeof(InstanceDescriptor) || type == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(typeDescriptorContext, type);
	}

	/// <summary>Attempts to convert a given object to an instance of <see cref="T:System.Windows.Media.Animation.KeyTime" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Animation.KeyTime" />, based on the supplied <paramref name="value" />.</returns>
	/// <param name="typeDescriptorContext">Context information required for conversion.</param>
	/// <param name="cultureInfo">Cultural information that is respected during conversion.</param>
	/// <param name="value">The object being converted to an instance of <see cref="T:System.Windows.Media.Animation.KeyTime" />.</param>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value)
	{
		if (value is string text)
		{
			string text2 = text.Trim();
			if (text2 == "Uniform")
			{
				return KeyTime.Uniform;
			}
			if (text2 == "Paced")
			{
				return KeyTime.Paced;
			}
			if (text2[text2.Length - 1] == '%')
			{
				text2 = text2.TrimEnd('%');
				double num = (double)TypeDescriptor.GetConverter(typeof(double)).ConvertFrom(typeDescriptorContext, cultureInfo, text2);
				if (num == 0.0)
				{
					return KeyTime.FromPercent(0.0);
				}
				if (num == 100.0)
				{
					return KeyTime.FromPercent(1.0);
				}
				return KeyTime.FromPercent(num / 100.0);
			}
			return KeyTime.FromTimeSpan((TimeSpan)TypeDescriptor.GetConverter(typeof(TimeSpan)).ConvertFrom(typeDescriptorContext, cultureInfo, text2));
		}
		return base.ConvertFrom(typeDescriptorContext, cultureInfo, value);
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.Media.Animation.KeyTime" /> to another type.</summary>
	/// <returns>A new object, based on <paramref name="value" />.</returns>
	/// <param name="typeDescriptorContext">Context information required for conversion.</param>
	/// <param name="cultureInfo">Cultural information that is respected during conversion.</param>
	/// <param name="value">
	///   <see cref="T:System.Windows.Media.Animation.KeyTime" /> value to convert from.</param>
	/// <param name="destinationType">Type being evaluated for conversion.</param>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (value != null && value is KeyTime keyTime)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				switch (keyTime.Type)
				{
				case KeyTimeType.Percent:
					return new InstanceDescriptor(typeof(KeyTime).GetMethod("FromPercent", new Type[1] { typeof(double) }), new object[1] { keyTime.Percent });
				case KeyTimeType.TimeSpan:
					return new InstanceDescriptor(typeof(KeyTime).GetMethod("FromTimeSpan", new Type[1] { typeof(TimeSpan) }), new object[1] { keyTime.TimeSpan });
				case KeyTimeType.Uniform:
					return new InstanceDescriptor(typeof(KeyTime).GetProperty("Uniform"), null);
				case KeyTimeType.Paced:
					return new InstanceDescriptor(typeof(KeyTime).GetProperty("Paced"), null);
				}
			}
			else if (destinationType == typeof(string))
			{
				switch (keyTime.Type)
				{
				case KeyTimeType.Uniform:
					return "Uniform";
				case KeyTimeType.Paced:
					return "Paced";
				case KeyTimeType.Percent:
				{
					string text = (string)TypeDescriptor.GetConverter(typeof(double)).ConvertTo(typeDescriptorContext, cultureInfo, keyTime.Percent * 100.0, destinationType);
					Span<char> span = stackalloc char[1] { '%' };
					return string.Concat(text, span);
				}
				case KeyTimeType.TimeSpan:
					return TypeDescriptor.GetConverter(typeof(TimeSpan)).ConvertTo(typeDescriptorContext, cultureInfo, keyTime.TimeSpan, destinationType);
				}
			}
		}
		return base.ConvertTo(typeDescriptorContext, cultureInfo, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.KeyTimeConverter" /> class.</summary>
	public KeyTimeConverter()
	{
	}
}
