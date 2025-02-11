using System.ComponentModel;
using System.Globalization;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Converts instances of other types to and from a <see cref="T:System.Windows.Media.PathFigureCollection" />.</summary>
public sealed class PathFigureCollectionConverter : TypeConverter
{
	/// <summary>Indicates whether an object can be converted from a given type to an instance of a <see cref="T:System.Windows.Media.PathFigureCollection" />.</summary>
	/// <returns>true if object of the specified type can be converted to a <see cref="T:System.Windows.Media.PathFigureCollection" />; otherwise, false.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="sourceType">The source <see cref="T:System.Type" /> that is being queried for conversion support.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Determines whether instances of <see cref="T:System.Windows.Media.PathFigureCollection" /> can be converted to the specified type.</summary>
	/// <returns>true if instances of <see cref="T:System.Windows.Media.PathFigureCollection" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="destinationType">The desired type this <see cref="T:System.Windows.Media.PathFigureCollection" /> is being evaluated to be converted to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			if (context != null && context.Instance != null)
			{
				if (!(context.Instance is PathFigureCollection))
				{
					throw new ArgumentException(SR.Format(SR.General_Expected_Type, "PathFigureCollection"), "context");
				}
				return ((PathFigureCollection)context.Instance).CanSerializeToString();
			}
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the specified object to a <see cref="T:System.Windows.Media.PathFigureCollection" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.PathFigureCollection" /> created from converting <paramref name="value" />.</returns>
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
			return PathFigureCollection.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the specified <see cref="T:System.Windows.Media.PathFigureCollection" /> to the specified type.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="culture">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.PathFigureCollection" /> to convert.</param>
	/// <param name="destinationType">The type to convert the <see cref="T:System.Windows.Media.PathFigureCollection" /> to.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is PathFigureCollection)
		{
			PathFigureCollection pathFigureCollection = (PathFigureCollection)value;
			if (destinationType == typeof(string))
			{
				if (context != null && context.Instance != null && !pathFigureCollection.CanSerializeToString())
				{
					throw new NotSupportedException(SR.Converter_ConvertToNotSupported);
				}
				return pathFigureCollection.ConvertToString(null, culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PathFigureCollection" /> class.</summary>
	public PathFigureCollectionConverter()
	{
	}
}
