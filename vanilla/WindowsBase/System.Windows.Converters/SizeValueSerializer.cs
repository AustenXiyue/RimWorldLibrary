using System.Windows.Markup;

namespace System.Windows.Converters;

/// <summary>Converts instances of <see cref="T:System.String" /> to and from instances of <see cref="T:System.Windows.Size" />.</summary>
public class SizeValueSerializer : ValueSerializer
{
	/// <summary>Determines whether the specified <see cref="T:System.String" /> can be converted to an instance of <see cref="T:System.Windows.Size" />.</summary>
	/// <returns>Always returns true.</returns>
	/// <param name="value">String to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Size" /> can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="value">The object to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (!(value is Size))
		{
			return false;
		}
		return true;
	}

	/// <summary>Converts a <see cref="T:System.String" /> into a <see cref="T:System.Windows.Size" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Size" /> based on the supplied <paramref name="value" />.</returns>
	/// <param name="value">The string to convert.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		if (value != null)
		{
			return Size.Parse(value);
		}
		return base.ConvertFromString(value, context);
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Size" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A string representation of the specified <see cref="T:System.Windows.Size" />.</returns>
	/// <param name="value">The object to convert into a string.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (value is Size size)
		{
			return size.ConvertToString(null, TypeConverterHelper.InvariantEnglishUS);
		}
		return base.ConvertToString(value, context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Converters.SizeValueSerializer" /> class.</summary>
	public SizeValueSerializer()
	{
	}
}
