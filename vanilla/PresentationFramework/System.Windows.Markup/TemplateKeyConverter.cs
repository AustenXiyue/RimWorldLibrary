using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Markup;

/// <summary>Implements a type converter for <see cref="T:System.Windows.TemplateKey" /> objects, which deliberately have no type conversion pathways. The type converter enforces and reports that behavior.</summary>
public sealed class TemplateKeyConverter : TypeConverter
{
	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Windows.TemplateKey" />.</summary>
	/// <returns>Always returns false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="sourceType">The type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.TemplateKey" /> can be converted to the specified type.</summary>
	/// <returns>Always returns false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return false;
	}

	/// <summary>Attempts to convert the specified object (string) to a <see cref="T:System.Windows.TemplateKey" />.</summary>
	/// <returns>Always throws an exception.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="source">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> cannot be converted.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
	{
		throw GetConvertFromException(source);
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.TemplateKey" /> to the specified type, using the specified context.</summary>
	/// <returns>Always throws an exception.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert the object to.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> cannot be converted.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		throw GetConvertToException(value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.TemplateKeyConverter" /> class.</summary>
	public TemplateKeyConverter()
	{
	}
}
