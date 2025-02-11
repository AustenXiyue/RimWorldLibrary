using System.Windows.Markup;

namespace System.Windows.Media;

/// <summary>Converts instances of <see cref="T:System.String" /> to and from instances of <see cref="T:System.Windows.Media.FontFamily" />.</summary>
public class FontFamilyValueSerializer : ValueSerializer
{
	/// <summary>Determines if conversion from a given <see cref="T:System.String" /> to an instance of <see cref="T:System.Windows.Media.FontFamily" /> is possible.</summary>
	/// <returns>true if <paramref name="value" /> can be converted; otherwise, false.</returns>
	/// <param name="value">String to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	/// <summary>Converts a <see cref="T:System.String" /> into a <see cref="T:System.Windows.Media.FontFamily" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.FontFamily" /> based on the supplied <paramref name="value" />.</returns>
	/// <param name="value">
	///   <see cref="T:System.String" /> value to convert into a <see cref="T:System.Windows.Media.FontFamily" />.</param>
	/// <param name="context">Context information used for conversion.</param>
	/// <exception cref="T:System.NotSupportedException">Occurs when <paramref name="value" /> is null or equal to <see cref="F:System.String.Empty" />.</exception>
	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		if (string.IsNullOrEmpty(value))
		{
			throw GetConvertFromException(value);
		}
		return new FontFamily(value);
	}

	/// <summary>Determines if an instance of <see cref="T:System.Windows.Media.FontFamily" /> can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="value">Instance of <see cref="T:System.Windows.Media.FontFamily" /> to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (value is FontFamily { Source: not null } fontFamily)
		{
			return fontFamily.Source.Length != 0;
		}
		return false;
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Media.FontFamily" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of the supplied <see cref="T:System.Windows.Media.FontFamily" /> object.</returns>
	/// <param name="value">Instance of <see cref="T:System.Windows.Media.FontFamily" /> to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	/// <exception cref="T:System.NotSupportedException">Occurs when <paramref name="value" /> is null or equal to <see cref="F:System.String.Empty" />.</exception>
	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (!(value is FontFamily { Source: not null } fontFamily))
		{
			throw GetConvertToException(value, typeof(string));
		}
		return fontFamily.Source;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.FontFamilyValueSerializer" /> class.</summary>
	public FontFamilyValueSerializer()
	{
	}
}
