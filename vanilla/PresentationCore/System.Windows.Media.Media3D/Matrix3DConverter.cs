using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Media3D;

/// <summary>Converts instances of other types to and from instances of <see cref="T:System.Windows.Media.Media3D.Matrix3D" />.</summary>
public sealed class Matrix3DConverter : TypeConverter
{
	/// <summary>Returns a value that indicates whether this type converter can convert from a specified type. </summary>
	/// <returns>True if this converter can convert from the specified type; false otherwise.</returns>
	/// <param name="context">ITypeDescriptorContext for this call.</param>
	/// <param name="sourceType">Type being queried for support.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Gets a value that indicates whether this type converter can convert to the given type. </summary>
	/// <returns>True if this converter can convert to the provided type; false if not.</returns>
	/// <param name="context">The ITypeDescriptorContext for this call.</param>
	/// <param name="destinationType">The Type being queried for support.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert to a Matrix3D from the given object. </summary>
	/// <returns>Matrix3D that was constructed.</returns>
	/// <param name="context">The ITypeDescriptorContext for this call.</param>
	/// <param name="culture">The CultureInfo which is respected when converting.</param>
	/// <param name="value">The object to convert to an instance of Matrix3D.</param>
	/// <exception cref="T:System.NotSupportedException">A NotSupportedException is thrown if the example object is null or is not a valid type which can be converted to a Matrix3D.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return Matrix3D.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> to the given type. </summary>
	/// <returns>Object that was constructed.</returns>
	/// <param name="context">The ITypeDescriptorContext for this call.</param>
	/// <param name="culture">The CultureInfo which is respected when converting.</param>
	/// <param name="value">The object to convert to an instance of <paramref name="destinationType" />.</param>
	/// <param name="destinationType">The type to which the Matrix3D instance will be converted.</param>
	/// <exception cref="T:System.NotSupportedException">Throws NotSupportedException if the example object is null or is not a Matrix3D, or if the destinationType isn't one of the valid destination types.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Matrix3D matrix3D && destinationType == typeof(string))
		{
			return matrix3D.ConvertToString(null, culture);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Windows.Media.Media3D.Matrix3DConverter" /> class.</summary>
	public Matrix3DConverter()
	{
	}
}
