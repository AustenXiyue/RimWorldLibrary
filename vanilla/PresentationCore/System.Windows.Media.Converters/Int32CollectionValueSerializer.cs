using System.Windows.Markup;

namespace System.Windows.Media.Converters;

/// <summary>Converts instances of <see cref="T:System.String" /> to and from instances of <see cref="T:System.Windows.Media.Int32Collection" />.</summary>
public class Int32CollectionValueSerializer : ValueSerializer
{
	/// <summary>Converts an instance of <see cref="T:System.Windows.Media.Int32Collection" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of the supplied <see cref="T:System.Windows.Media.Int32Collection" /> object.</returns>
	/// <param name="value">Instance of <see cref="T:System.Windows.Media.Int32Collection" /> to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	/// <summary>Converts a <see cref="T:System.String" /> into a <see cref="T:System.Windows.Media.Int32Collection" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Int32Collection" /> based on the supplied <paramref name="value" />.</returns>
	/// <param name="value">
	///   <see cref="T:System.String" /> value to convert into a <see cref="T:System.Windows.Media.Int32Collection" />.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (!(value is Int32Collection))
		{
			return false;
		}
		return true;
	}

	/// <summary>Determines if an instance of <see cref="T:System.Windows.Media.Int32Collection" /> can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="value">Instance of <see cref="T:System.Windows.Media.Int32Collection" /> to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	/// <exception cref="T:System.ArgumentException">Occurs when <paramref name="value" /> is not a <see cref="T:System.Windows.Media.Int32Collection" />.</exception>
	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		if (value != null)
		{
			return Int32Collection.Parse(value);
		}
		return base.ConvertFromString(value, context);
	}

	/// <summary>Determines if conversion from a given <see cref="T:System.String" /> to an instance of <see cref="T:System.Windows.Media.Int32Collection" /> is possible.</summary>
	/// <returns>true if the value can be converted; otherwise, false.</returns>
	/// <param name="value">String to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (value is Int32Collection)
		{
			return ((Int32Collection)value).ConvertToString(null, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS);
		}
		return base.ConvertToString(value, context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Converters.Int32CollectionValueSerializer" /> class.</summary>
	public Int32CollectionValueSerializer()
	{
	}
}
