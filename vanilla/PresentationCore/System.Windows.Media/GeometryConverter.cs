using System.ComponentModel;
using System.Globalization;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Converts instances of other types to and from instances of <see cref="T:System.Windows.Media.Geometry" />.</summary>
public sealed class GeometryConverter : TypeConverter
{
	/// <summary>Indicates whether an object can be converted from a given type to an instance of a <see cref="T:System.Windows.Media.Geometry" />. </summary>
	/// <returns>true if object of the specified type can be converted to a <see cref="T:System.Windows.Media.Geometry" />; otherwise, false.</returns>
	/// <param name="context">Context information required for conversion.</param>
	/// <param name="sourceType">The source <see cref="T:System.Type" /> that is being queried for conversion support.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Determines whether instances of <see cref="T:System.Windows.Media.Geometry" /> can be converted to the specified type. </summary>
	/// <returns>true if instances of <see cref="T:System.Windows.Media.Geometry" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
	/// <param name="context">Context information required for conversion.</param>
	/// <param name="destinationType">The desired type this <see cref="T:System.Windows.Media.Geometry" /> is being evaluated to be converted to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			if (context != null && context.Instance != null)
			{
				if (!(context.Instance is Geometry))
				{
					throw new ArgumentException(SR.Format(SR.General_Expected_Type, "Geometry"), "context");
				}
				return ((Geometry)context.Instance).CanSerializeToString();
			}
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the specified object to a <see cref="T:System.Windows.Media.Geometry" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> created from converting <paramref name="value" />.</returns>
	/// <param name="context">Context information required for conversion.</param>
	/// <param name="culture">Cultural information respected during conversion.</param>
	/// <param name="value">The object being converted.</param>
	/// <exception cref="T:System.NotSupportedException">Thrown if <paramref name="value" /> is null or is not a valid type which can be converted to a <see cref="T:System.Windows.Media.Geometry" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return Geometry.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the specified <see cref="T:System.Windows.Media.Geometry" /> to the specified type.</summary>
	/// <returns>The object created from converting this <see cref="T:System.Windows.Media.Geometry" />.</returns>
	/// <param name="context">Context information required for conversion.</param>
	/// <param name="culture">Cultural information respected during conversion.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.Geometry" /> to convert.</param>
	/// <param name="destinationType">The type to convert the <see cref="T:System.Windows.Media.Geometry" /> to.</param>
	/// <exception cref="T:System.NotSupportedException">Thrown if <paramref name="value" /> is null or is not a <see cref="T:System.Windows.Media.Geometry" />, or if the <paramref name="destinationType" /> cannot be converted into a <see cref="T:System.Windows.Media.Geometry" />.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Geometry)
		{
			Geometry geometry = (Geometry)value;
			if (destinationType == typeof(string))
			{
				if (context != null && context.Instance != null && !geometry.CanSerializeToString())
				{
					throw new NotSupportedException(SR.Converter_ConvertToNotSupported);
				}
				return geometry.ConvertToString(null, culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Geometry" /> class.</summary>
	public GeometryConverter()
	{
	}
}
