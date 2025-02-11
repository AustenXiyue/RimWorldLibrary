using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Media3D;

/// <summary> Converts instances of other types to and from a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
public sealed class Point3DConverter : TypeConverter
{
	/// <summary> Determines whether a class can be converted from a given type to an instance of a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.  </summary>
	/// <returns>Indicates whether the type can be converted to a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.ValueMeaningfalseThe converter cannot convert from the provided type.trueThe converter can convert the provided type to a <see cref="T:System.Windows.Media.Media3D.Point3D" />.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="sourceType">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary> Determines whether an instance of a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure can be converted to a different type. </summary>
	/// <returns>Indicates whether this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure can be converted to <paramref name="destinationType" />.ValueMeaningfalseThe converter cannot convert this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to the specified type.trueThe converter can convert this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to the specified type.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="destinationType">The desired type that this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure is being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary> Attempts to convert a specified object to a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure created from the converted <paramref name="value" />.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="culture">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The object being converted.</param>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return Point3D.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary> Attempts to convert a <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to a specified type. </summary>
	/// <returns>The object created from converting this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="culture">The CultureInfo which is respected when converting.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure to convert.</param>
	/// <param name="destinationType">The type this <see cref="T:System.Windows.Media.Media3D.Point3D" /> structure is converted to.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Point3D point3D && destinationType == typeof(string))
		{
			return point3D.ConvertToString(null, culture);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary> Initializes a new instance of <see cref="T:System.Windows.Media.Media3D.Point3DConverter" />. </summary>
	public Point3DConverter()
	{
	}
}
