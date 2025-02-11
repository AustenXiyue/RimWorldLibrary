using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media;

/// <summary>Converts instances of other types to and from a <see cref="T:System.Windows.Media.PointCollection" />. </summary>
public sealed class PointCollectionConverter : TypeConverter
{
	/// <summary>Determines whether an object can be converted from a specified type to an instance of a <see cref="T:System.Windows.Media.PointCollection" />.  </summary>
	/// <returns>true if the type can be converted to a <see cref="T:System.Windows.Media.PointCollection" />; otherwise, false.</returns>
	/// <param name="context">The context information of a type.</param>
	/// <param name="sourceType">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Determines whether an instance of a <see cref="T:System.Windows.Media.PointCollection" /> can be converted to a different type. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.PointCollection" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
	/// <param name="context">The context information of a type.</param>
	/// <param name="destinationType">The needed type for which you are evaluating this <see cref="T:System.Windows.Media.PointCollection" /> for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.Media.PointCollection" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.PointCollection" /> that is created from converting <paramref name="value" />.</returns>
	/// <param name="context">The context information of a type.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> of the type you want to convert.</param>
	/// <param name="value">The object being converted.</param>
	/// <exception cref="T:System.NotSupportedException">The specified object is null or is a type that cannot be converted to a <see cref="T:System.Windows.Media.PointCollection" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return PointCollection.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.Media.PointCollection" /> to a specified type. </summary>
	/// <returns>The object that is created from converting this <see cref="T:System.Windows.Media.PointCollection" />.</returns>
	/// <param name="context">The context information of a type.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> of the type you want to convert.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.PointCollection" /> to convert.</param>
	/// <param name="destinationType">The type to convert this <see cref="T:System.Windows.Media.PointCollection" /> to.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or is not a <see cref="T:System.Windows.Media.PointCollection" />; or the <paramref name="destinationType" /> is not one of the valid types for conversion.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is PointCollection)
		{
			PointCollection pointCollection = (PointCollection)value;
			if (destinationType == typeof(string))
			{
				return pointCollection.ConvertToString(null, culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PointCollectionConverter" /> class. </summary>
	public PointCollectionConverter()
	{
	}
}
