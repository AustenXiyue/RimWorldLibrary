using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Media3D;

/// <summary>Converts instances of other types to and from <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> instances. </summary>
public sealed class Point3DCollectionConverter : TypeConverter
{
	/// <summary>Returns a value that indicates whether the type converter can convert from a specified type. </summary>
	/// <returns>true if the converter can convert from the provided type; otherwise, false.</returns>
	/// <param name="context">The type descriptor context for this call.</param>
	/// <param name="sourceType">The type being queried for support.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Returns a value that indicates whether the type converter can convert to the specified type. </summary>
	/// <returns>true if the converter can convert to the provided type; otherwise, false.</returns>
	/// <param name="context">The type descriptor context for this call.</param>
	/// <param name="destinationType">The type being queried for support.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert to a <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> from the given object. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> that was constructed.</returns>
	/// <param name="context">The type descriptor context for this call.</param>
	/// <param name="culture">The culture information that is respected when converting.</param>
	/// <param name="value">The object to convert to an instance of <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or is not a valid type that can be converted to a <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return Point3DCollection.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> to the specified type </summary>
	/// <returns>The object that was constructed.</returns>
	/// <param name="context">The type descriptor context for this call.</param>
	/// <param name="culture">The culture information that is respected when converting.</param>
	/// <param name="value">The object to convert to an instance of <paramref name="destinationType" />.</param>
	/// <param name="destinationType">Type to which this method will convert the <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> instance.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or is not a <see cref="T:System.Windows.Media.Media3D.Point3DCollection" />.-or-<paramref name="destinationType" /> is not one of the valid destination types.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Point3DCollection)
		{
			Point3DCollection point3DCollection = (Point3DCollection)value;
			if (destinationType == typeof(string))
			{
				return point3DCollection.ConvertToString(null, culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Point3DCollectionConverter" /> class. </summary>
	public Point3DCollectionConverter()
	{
	}
}
