using System.ComponentModel;
using System.Globalization;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Converts a <see cref="T:System.Windows.Media.Transform" /> object to or from another object type. </summary>
public sealed class TransformConverter : TypeConverter
{
	/// <summary>Determines whether this class can convert an object of a specified type to a <see cref="T:System.Windows.Media.Transform" /> type. </summary>
	/// <returns>true if conversion is possible; otherwise, false.</returns>
	/// <param name="context">The conversion context.</param>
	/// <param name="sourceType">The type from which to convert.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Determines whether this class can convert an object of a specified type to the specified destination type. </summary>
	/// <returns>true if conversion is possible; otherwise, false.</returns>
	/// <param name="context">The conversion context.</param>
	/// <param name="destinationType">The destination type.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			if (context != null && context.Instance != null)
			{
				if (!(context.Instance is Transform))
				{
					throw new ArgumentException(SR.Format(SR.General_Expected_Type, "Transform"), "context");
				}
				return ((Transform)context.Instance).CanSerializeToString();
			}
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts from an object of a specified type to a <see cref="T:System.Windows.Media.Transform" /> object. </summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Transform" /> object.</returns>
	/// <param name="context">The conversion context.</param>
	/// <param name="culture">The culture information that applies to the conversion.</param>
	/// <param name="value">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or cannot be converted to a <see cref="T:System.Windows.Media.Transform" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return Transform.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the specified <see cref="T:System.Windows.Media.Transform" /> to the specified type by using the specified context and culture information. </summary>
	/// <returns>An object that represents the converted <paramref name="value" />.</returns>
	/// <param name="context">The conversion context.</param>
	/// <param name="culture">The culture information that applies to the conversion.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.Transform" /> to convert.</param>
	/// <param name="destinationType">The destination type that the <paramref name="value" /> object is converted to.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or is not a <see cref="T:System.Windows.Media.Transform" />.-or-<paramref name="destinationType" /> is not a valid destination type.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Transform)
		{
			Transform transform = (Transform)value;
			if (destinationType == typeof(string))
			{
				if (context != null && context.Instance != null && !transform.CanSerializeToString())
				{
					throw new NotSupportedException(SR.Converter_ConvertToNotSupported);
				}
				return transform.ConvertToString(null, culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TransformConverter" /> class as a <see cref="T:System.ComponentModel.TypeConverter" /> object. </summary>
	public TransformConverter()
	{
	}
}
