using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Media3D;

/// <summary>Converts instances of other types to and from instances of <see cref="T:System.Windows.Media.Media3D.Vector3DCollection" />.</summary>
public sealed class Vector3DCollectionConverter : TypeConverter
{
	/// <summary> Returns a value that indicates whether this type converter can convert from a specified type. </summary>
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

	/// <summary>Attempts to convert to a <see cref="T:System.Windows.Media.Media3D.Vector3DCollection" /> from the given object. </summary>
	/// <returns>Vector3DCollection that was constructed.</returns>
	/// <param name="context">The ITypeDescriptorContext for this call.</param>
	/// <param name="culture">The CultureInfo which is respected when converting.</param>
	/// <param name="value">The object to convert to an instance of Vector3DCollection.</param>
	/// <exception cref="T:System.NotSupportedException">A NotSupportedException is thrown if the example object is null or is not a valid type which can be converted to a Vector3DCollection.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return Vector3DCollection.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.Media.Media3D.Vector3DCollection" /> to the given type. </summary>
	/// <returns>The object which was constructoed.</returns>
	/// <param name="context">The ITypeDescriptorContext for this call.</param>
	/// <param name="culture">The CultureInfo which is respected when converting.</param>
	/// <param name="value">The object to convert to an instance of <paramref name="destinationType" />.</param>
	/// <param name="destinationType">The type to which the Matrix3D instance will be converted.</param>
	/// <exception cref="T:System.NotSupportedException">Throws NotSupportedException if the example object is null or is not a Vector3DCollection, or if the destinationType isn't one of the valid destination types.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Vector3DCollection)
		{
			Vector3DCollection vector3DCollection = (Vector3DCollection)value;
			if (destinationType == typeof(string))
			{
				return vector3DCollection.ConvertToString(null, culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Windows.Media.Media3D.Vector3DCollectionConverter" /> class.</summary>
	public Vector3DCollectionConverter()
	{
	}
}
