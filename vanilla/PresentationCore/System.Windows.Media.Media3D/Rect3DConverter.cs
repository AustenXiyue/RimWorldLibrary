using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Media3D;

/// <summary>Converts instances of other types to and from instances of <see cref="T:System.Windows.Media.Media3D.Rect3D" />.</summary>
public sealed class Rect3DConverter : TypeConverter
{
	/// <summary>Gets a value that indicates whether this type converter can convert from a given type. </summary>
	/// <returns>True if this converter can convert from the provided type, false otherwise.</returns>
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
	/// <returns>True if this converter can convert to the provided type, false otherwise.</returns>
	/// <param name="context">ITypeDescriptorContext for this call.</param>
	/// <param name="destinationType">Type being queried for support.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the specified object to a <see cref="T:System.Windows.Media.Media3D.Rect3D" />. </summary>
	/// <returns>Rect3D that was constructed.</returns>
	/// <param name="context">ITypeDescriptorContext for this call.</param>
	/// <param name="culture">CultureInfo to be respected when converting.</param>
	/// <param name="value">Object to convert to an instance of Rect3D.</param>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return Rect3D.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.Media.Media3D.Rect3D" /> to the given type.</summary>
	/// <returns>Object that was constructed.</returns>
	/// <param name="context">The ITypeDescriptorContext for this call.</param>
	/// <param name="culture">The CultureInfo which is respected when converting.</param>
	/// <param name="value">The object to convert to an instance of <paramref name="destinationType" />.</param>
	/// <param name="destinationType">The type to which the Rect3D instance will be converted.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Rect3D rect3D && destinationType == typeof(string))
		{
			return rect3D.ConvertToString(null, culture);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Windows.Media.Media3D.Rect3DConverter" /> class.</summary>
	public Rect3DConverter()
	{
	}
}
