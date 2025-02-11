using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

namespace System.Configuration;

/// <summary>Converts a comma-delimited string value to and from a <see cref="T:System.Configuration.CommaDelimitedStringCollection" /> object. This class cannot be inherited.</summary>
public sealed class CommaDelimitedStringCollectionConverter : ConfigurationConverterBase
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.CommaDelimitedStringCollectionConverter" /> class. </summary>
	public CommaDelimitedStringCollectionConverter()
	{
	}

	/// <summary>Converts a <see cref="T:System.String" /> object to a <see cref="T:System.Configuration.CommaDelimitedStringCollection" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.CommaDelimitedStringCollection" /> containing the converted value.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> used during conversion.</param>
	/// <param name="data">The comma-separated <see cref="T:System.String" /> to convert.</param>
	public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
	{
		CommaDelimitedStringCollection commaDelimitedStringCollection = new CommaDelimitedStringCollection();
		string[] array = ((string)data).Split(',');
		foreach (string text in array)
		{
			commaDelimitedStringCollection.Add(text.Trim());
		}
		commaDelimitedStringCollection.UpdateStringHash();
		return commaDelimitedStringCollection;
	}

	/// <summary>Converts a <see cref="T:System.Configuration.CommaDelimitedStringCollection" /> object to a <see cref="T:System.String" /> object.</summary>
	/// <returns>The <see cref="T:System.String" /> representing the converted <paramref name="value" /> parameter, which is a <see cref="T:System.Configuration.CommaDelimitedStringCollection" />.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> used during conversion.</param>
	/// <param name="value">The value to convert.</param>
	/// <param name="type">The conversion type.</param>
	public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
	{
		if (value == null)
		{
			return null;
		}
		if (!typeof(StringCollection).IsAssignableFrom(value.GetType()))
		{
			throw new ArgumentException();
		}
		return value.ToString();
	}
}
