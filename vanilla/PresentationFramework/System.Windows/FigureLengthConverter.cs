using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Converts instances of other types to and from a <see cref="T:System.Windows.FigureLength" />.</summary>
public class FigureLengthConverter : TypeConverter
{
	/// <summary>Indicates whether an object can be converted from a given type to an instance of a <see cref="T:System.Windows.FigureLength" />.</summary>
	/// <returns>true if object of the specified type can be converted to a <see cref="T:System.Windows.FigureLength" />; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="sourceType">The source <see cref="T:System.Type" /> that is being queried for conversion support.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		TypeCode typeCode = Type.GetTypeCode(sourceType);
		if ((uint)(typeCode - 7) <= 8u || typeCode == TypeCode.String)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether instances of <see cref="T:System.Windows.FigureLength" /> can be converted to the specified type.</summary>
	/// <returns>true if instances of <see cref="T:System.Windows.FigureLength" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="destinationType">The desired type this <see cref="T:System.Windows.FigureLength" /> is being evaluated to be converted to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		if (!(destinationType == typeof(InstanceDescriptor)))
		{
			return destinationType == typeof(string);
		}
		return true;
	}

	/// <summary>Converts the specified object to a <see cref="T:System.Windows.FigureLength" />.</summary>
	/// <returns>The <see cref="T:System.Windows.FigureLength" /> created from converting <paramref name="source" />.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="cultureInfo">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="source">The object being converted.</param>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		if (source != null)
		{
			if (source is string)
			{
				return FromString((string)source, cultureInfo);
			}
			return new FigureLength(Convert.ToDouble(source, cultureInfo));
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Converts the specified <see cref="T:System.Windows.FigureLength" /> to the specified type.</summary>
	/// <returns>The object created from converting this <see cref="T:System.Windows.FigureLength" />.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="cultureInfo">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The <see cref="T:System.Windows.FigureLength" /> to convert.</param>
	/// <param name="destinationType">The type to convert the <see cref="T:System.Windows.FigureLength" /> to.</param>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value != null && value is FigureLength figureLength)
		{
			if (destinationType == typeof(string))
			{
				return ToString(figureLength, cultureInfo);
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(FigureLength).GetConstructor(new Type[2]
				{
					typeof(double),
					typeof(FigureUnitType)
				}), new object[2] { figureLength.Value, figureLength.FigureUnitType });
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	internal static string ToString(FigureLength fl, CultureInfo cultureInfo)
	{
		return fl.FigureUnitType switch
		{
			FigureUnitType.Auto => "Auto", 
			FigureUnitType.Pixel => Convert.ToString(fl.Value, cultureInfo), 
			_ => Convert.ToString(fl.Value, cultureInfo) + " " + fl.FigureUnitType, 
		};
	}

	internal static FigureLength FromString(string s, CultureInfo cultureInfo)
	{
		XamlFigureLengthSerializer.FromString(s, cultureInfo, out var value, out var unit);
		return new FigureLength(value, unit);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FigureLengthConverter" /> class.</summary>
	public FigureLengthConverter()
	{
	}
}
