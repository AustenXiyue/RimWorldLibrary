using System.ComponentModel;
using System.Globalization;

namespace System.Windows;

/// <summary>Converts instances of other types to and from instances of <see cref="T:System.Windows.Rect" />.</summary>
public sealed class RectConverter : TypeConverter
{
	/// <summary>Determines whether an object can be converted from a given type to an instance of <see cref="T:System.Windows.Rect" />.</summary>
	/// <returns>true if the type can be converted to a <see cref="T:System.Windows.Rect" />; otherwise, false.</returns>
	/// <param name="context">Provides contextual information required for conversion.</param>
	/// <param name="sourceType">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Determines whether a <see cref="T:System.Windows.Rect" /> can be converted to the specified type. </summary>
	/// <returns>true if a <see cref="T:System.Windows.Rect" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
	/// <param name="context">Provides contextual information required for conversion.</param>
	/// <param name="destinationType">The desired type this <see cref="T:System.Windows.Rect" /> is being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.Rect" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Rect" /> created from converting <paramref name="value" />.</returns>
	/// <param name="context">Provides contextual information required for conversion.</param>
	/// <param name="culture">Cultural information which is respected when converting.</param>
	/// <param name="value">The object being converted.</param>
	/// <exception cref="T:System.NotSupportedException">Thrown if the specified object is NULL or is a type that cannot be converted to a <see cref="T:System.Windows.Rect" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return Rect.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary> Attempts to convert a <see cref="T:System.Windows.Rect" /> to the specified type. </summary>
	/// <returns>The object created from converting this <see cref="T:System.Windows.Rect" />.</returns>
	/// <param name="context">Provides contextual information required for conversion.</param>
	/// <param name="culture">Cultural information which is respected during conversion.</param>
	/// <param name="value">The <see cref="T:System.Windows.Rect" /> to convert.</param>
	/// <param name="destinationType">The type to convert this <see cref="T:System.Windows.Rect" /> to.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null.- or - <paramref name="value" /> is not a <see cref="T:System.Windows.Rect" />.- or - The <paramref name="destinationType" /> is not one of the valid types for conversion.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Rect rect && destinationType == typeof(string))
		{
			return rect.ConvertToString(null, culture);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.RectConverter" /> class. </summary>
	public RectConverter()
	{
	}
}
