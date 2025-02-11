using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media;

/// <summary>Converts instances of other types to and from a <see cref="T:System.Windows.Media.Matrix" />. </summary>
public sealed class MatrixConverter : TypeConverter
{
	/// <summary>Determines whether an object can be converted from a specific type to an instance of a <see cref="T:System.Windows.Media.Matrix" />.  </summary>
	/// <returns>true if the type can be converted to a <see cref="T:System.Windows.Media.Matrix" />; otherwise, false.</returns>
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

	/// <summary>Determines whether an instance of a <see cref="T:System.Windows.Media.Matrix" /> can be converted to a different type. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.Matrix" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
	/// <param name="context">The context information of a type.</param>
	/// <param name="destinationType">The desired type this <see cref="T:System.Windows.Media.Matrix" /> is being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.Media.Matrix" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Matrix" /> created from converting <paramref name="value" />.</returns>
	/// <param name="context">The context information of a type.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The object being converted.</param>
	/// <exception cref="T:System.NotSupportedException">The specified object is null or is a type that cannot be converted to a <see cref="T:System.Windows.Media.Matrix" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string source)
		{
			return Matrix.Parse(source);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.Media.Matrix" /> to a specified type. </summary>
	/// <returns>The object created from converting this <see cref="T:System.Windows.Media.Matrix" />.</returns>
	/// <param name="context">The context information of a type.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.Matrix" /> to convert.</param>
	/// <param name="destinationType">The type to convert this <see cref="T:System.Windows.Media.Matrix" /> to.</param>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="value" /> is null or is not a <see cref="T:System.Windows.Media.Matrix" />, or the <paramref name="destinationType" /> is not a valid conversion type.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Matrix matrix && destinationType == typeof(string))
		{
			return matrix.ConvertToString(null, culture);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.MatrixConverter" />.</summary>
	public MatrixConverter()
	{
	}
}
