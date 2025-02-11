using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media;

/// <summary>Converts an <see cref="T:System.Windows.Media.Int32Collection" /> to and from other data types.</summary>
public sealed class Int32CollectionConverter : TypeConverter
{
	/// <summary>Determines if the converter can convert an object of the given type to an instance of <see cref="T:System.Windows.Media.Int32Collection" />.</summary>
	/// <returns>true if the converter can convert the provided type to an instance of <see cref="T:System.Windows.Media.Int32Collection" />; otherwise, false.</returns>
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

	/// <summary>Determines if the converter can convert an <see cref="T:System.Windows.Media.Int32Collection" /> to a given data type.</summary>
	/// <returns>true if an <see cref="T:System.Windows.Media.Int32Collection" /> can convert to <paramref name="destinationType" />; otherwise false.</returns>
	/// <param name="context">The context information of a type.</param>
	/// <param name="destinationType">The desired type to evaluate the conversion to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert a specified object to an <see cref="T:System.Windows.Media.Int32Collection" /> instance.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Int32Collection" />.</returns>
	/// <param name="context">Context information used for conversion.</param>
	/// <param name="culture">Cultural information that is respected during conversion.</param>
	/// <param name="value">The object being converted.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or the type invalid.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return Int32Collection.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.Media.Int32Collection" /> to a specified type.</summary>
	/// <returns>A new instance of the <paramref name="destinationType" />.</returns>
	/// <param name="context">Context information used for conversion.</param>
	/// <param name="culture">Cultural information that is respected during conversion.</param>
	/// <param name="value">
	///   <see cref="T:System.Windows.Media.Int32Collection" /> to convert.</param>
	/// <param name="destinationType">Type being evaluated for conversion.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or the type invalid.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Int32Collection)
		{
			Int32Collection int32Collection = (Int32Collection)value;
			if (destinationType == typeof(string))
			{
				return int32Collection.ConvertToString(null, culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Int32Collection" /> class.</summary>
	public Int32CollectionConverter()
	{
	}
}
