using System.Windows.Markup;

namespace System.Windows.Media.Converters;

/// <summary>Converts instances of <see cref="T:System.String" /> to and from instances of <see cref="T:System.Windows.Media.Brush" />.</summary>
public class BrushValueSerializer : ValueSerializer
{
	/// <summary>Determines if conversion from a given <see cref="T:System.String" /> to an instance of <see cref="T:System.Windows.Media.Brush" /> is possible.</summary>
	/// <returns>true if the value can be converted; otherwise, false. </returns>
	/// <param name="value">String to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	/// <summary>Determines if an instance of <see cref="T:System.Windows.Media.Brush" /> can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="value">Instance of <see cref="T:System.Windows.Media.Brush" /> to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	/// <exception cref="T:System.ArgumentException">Occurs when <paramref name="value" /> is not a <see cref="T:System.Windows.Media.Brush" />.</exception>
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (!(value is Brush))
		{
			return false;
		}
		return ((Brush)value).CanSerializeToString();
	}

	/// <summary>Converts a <see cref="T:System.String" /> into a <see cref="T:System.Windows.Media.Brush" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Brush" /> based on the supplied <paramref name="value" />.</returns>
	/// <param name="value">
	///   <see cref="T:System.String" /> value to convert into a <see cref="T:System.Windows.Media.Brush" />.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		if (value != null)
		{
			return Brush.Parse(value, context);
		}
		return base.ConvertFromString(value, context);
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Media.Brush" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of the supplied <see cref="T:System.Windows.Media.Brush" /> object.</returns>
	/// <param name="value">Instance of <see cref="T:System.Windows.Media.Brush" /> to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (value is Brush)
		{
			Brush brush = (Brush)value;
			if (!brush.CanSerializeToString())
			{
				return base.ConvertToString(value, context);
			}
			return brush.ConvertToString(null, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS);
		}
		return base.ConvertToString(value, context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Converters.BrushValueSerializer" /> class.</summary>
	public BrushValueSerializer()
	{
	}
}
