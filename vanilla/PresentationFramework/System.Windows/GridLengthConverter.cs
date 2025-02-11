using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Windows.Markup;
using MS.Internal;

namespace System.Windows;

/// <summary>Converts instances of other types to and from <see cref="T:System.Windows.GridLength" /> instances. </summary>
public class GridLengthConverter : TypeConverter
{
	/// <summary>Determines whether a class can be converted from a given type to an instance of <see cref="T:System.Windows.GridLength" />.</summary>
	/// <returns>true if the converter can convert from the specified type to an instance of <see cref="T:System.Windows.GridLength" />; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="sourceType">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		TypeCode typeCode = Type.GetTypeCode(sourceType);
		if ((uint)(typeCode - 7) <= 8u || typeCode == TypeCode.String)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.GridLength" /> can be converted to a different type. </summary>
	/// <returns>true if the converter can convert this instance of <see cref="T:System.Windows.GridLength" /> to the specified type; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="destinationType">The desired type that this instance of <see cref="T:System.Windows.GridLength" /> is being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		if (!(destinationType == typeof(InstanceDescriptor)))
		{
			return destinationType == typeof(string);
		}
		return true;
	}

	/// <summary>Attempts to convert a specified object to an instance of <see cref="T:System.Windows.GridLength" />. </summary>
	/// <returns>The instance of <see cref="T:System.Windows.GridLength" /> that is created from the converted <paramref name="source" />.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="cultureInfo">Cultural specific information that should be respected during conversion.</param>
	/// <param name="source">The object being converted.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> object is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="source" /> object is not null and is not a valid type that can be converted to a <see cref="T:System.Windows.GridLength" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		if (source != null)
		{
			if (source is string)
			{
				return FromString((string)source, cultureInfo);
			}
			double num = Convert.ToDouble(source, cultureInfo);
			GridUnitType type;
			if (double.IsNaN(num))
			{
				num = 1.0;
				type = GridUnitType.Auto;
			}
			else
			{
				type = GridUnitType.Pixel;
			}
			return new GridLength(num, type);
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.GridLength" /> to a specified type. </summary>
	/// <returns>The object that is created from the converted instance of <see cref="T:System.Windows.GridLength" />.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="cultureInfo">Cultural specific information that should be respected during conversion.</param>
	/// <param name="value">The instance of <see cref="T:System.Windows.GridLength" /> to convert.</param>
	/// <param name="destinationType">The type that this instance of <see cref="T:System.Windows.GridLength" /> is converted to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is not one of the valid types for conversion.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null.</exception>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value != null && value is GridLength gridLength)
		{
			if (destinationType == typeof(string))
			{
				return ToString(gridLength, cultureInfo);
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(GridLength).GetConstructor(new Type[2]
				{
					typeof(double),
					typeof(GridUnitType)
				}), new object[2] { gridLength.Value, gridLength.GridUnitType });
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	internal static string ToString(GridLength gl, CultureInfo cultureInfo)
	{
		switch (gl.GridUnitType)
		{
		case GridUnitType.Auto:
			return "Auto";
		case GridUnitType.Star:
			if (!DoubleUtil.IsOne(gl.Value))
			{
				return Convert.ToString(gl.Value, cultureInfo) + "*";
			}
			return "*";
		default:
			return Convert.ToString(gl.Value, cultureInfo);
		}
	}

	internal static GridLength FromString(string s, CultureInfo cultureInfo)
	{
		XamlGridLengthSerializer.FromString(s, cultureInfo, out var value, out var unit);
		return new GridLength(value, unit);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.GridLengthConverter" /> class. </summary>
	public GridLengthConverter()
	{
	}
}
