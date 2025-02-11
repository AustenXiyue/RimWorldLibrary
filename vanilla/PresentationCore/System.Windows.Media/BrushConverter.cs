using System.ComponentModel;
using System.Globalization;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary> Used to convert a <see cref="T:System.Windows.Media.Brush" /> object to or from another object type. </summary>
public sealed class BrushConverter : TypeConverter
{
	/// <summary> Determines whether this class can convert an object of a given type to a <see cref="T:System.Windows.Media.Brush" /> object. </summary>
	/// <returns>Returns true if conversion is possible (object is string type); otherwise, false.</returns>
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

	/// <summary> Determines whether this class can convert an object of a given type to the specified destination type. </summary>
	/// <returns>Returns true if conversion is possible; otherwise, false.</returns>
	/// <param name="context">The conversion context.</param>
	/// <param name="destinationType">The destination type.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			if (context != null && context.Instance != null)
			{
				if (!(context.Instance is Brush))
				{
					throw new ArgumentException(SR.Format(SR.General_Expected_Type, "Brush"), "context");
				}
				return ((Brush)context.Instance).CanSerializeToString();
			}
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary> Converts from an object of a given type to a <see cref="T:System.Windows.Media.Brush" /> object. </summary>
	/// <returns>Returns a new <see cref="T:System.Windows.Media.Brush" /> object if successful; otherwise, NULL.</returns>
	/// <param name="context">The conversion context.</param>
	/// <param name="culture">The culture information that applies to the conversion.</param>
	/// <param name="value">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is NULL or cannot be converted to a <see cref="T:System.Windows.Media.Brush" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string value2)
		{
			return Brush.Parse(value2, context);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary> Converts a <see cref="T:System.Windows.Media.Brush" /> object to a specified type, using the specified context and culture information. </summary>
	/// <returns>An object that represents the converted <paramref name="value" />.</returns>
	/// <param name="context">The conversion context.</param>
	/// <param name="culture">The current culture information.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.Brush" /> to convert.</param>
	/// <param name="destinationType">The destination type that the <paramref name="value " />object is converted to.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is NULL or it is not a <see cref="T:System.Windows.Media.Brush" />-or-<paramref name="destinationType" /> is not a valid destination type.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Brush)
		{
			Brush brush = (Brush)value;
			if (destinationType == typeof(string))
			{
				if (context != null && context.Instance != null && !brush.CanSerializeToString())
				{
					throw new NotSupportedException(SR.Converter_ConvertToNotSupported);
				}
				return brush.ConvertToString(null, culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.BrushConverter" /> class. </summary>
	public BrushConverter()
	{
	}
}
