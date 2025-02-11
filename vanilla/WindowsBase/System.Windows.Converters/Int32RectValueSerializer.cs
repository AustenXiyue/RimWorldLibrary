using System.Windows.Markup;

namespace System.Windows.Converters;

/// <summary>Converts instances of <see cref="T:System.String" /> to and from instances of <see cref="T:System.Windows.Int32Rect" />.</summary>
public class Int32RectValueSerializer : ValueSerializer
{
	/// <summary>Determines whether the specified <see cref="T:System.String" /> can be converted to an instance of <see cref="T:System.Windows.Int32Rect" />.</summary>
	/// <returns>Always returns true.</returns>
	/// <param name="value">String to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Int32Rect" /> can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="value">The object to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (!(value is Int32Rect))
		{
			return false;
		}
		return true;
	}

	/// <summary>Converts a <see cref="T:System.String" /> into a <see cref="T:System.Windows.Int32Rect" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Int32Rect" /> based on the supplied <paramref name="value" />.</returns>
	/// <param name="value">The string to convert.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		if (value != null)
		{
			return Int32Rect.Parse(value);
		}
		return base.ConvertFromString(value, context);
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Int32Rect" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A string representation of the specified <see cref="T:System.Windows.Int32Rect" />.</returns>
	/// <param name="value">The object to convert into a string.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (value is Int32Rect int32Rect)
		{
			return int32Rect.ConvertToString(null, TypeConverterHelper.InvariantEnglishUS);
		}
		return base.ConvertToString(value, context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Converters.Int32RectValueSerializer" /> class.</summary>
	public Int32RectValueSerializer()
	{
	}
}
