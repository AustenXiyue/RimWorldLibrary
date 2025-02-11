using System.ComponentModel;
using System.Globalization;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Converters;

/// <summary>Defines methods used to convert <see cref="T:System.Collections.IList" /> collection members to and from instances of <see cref="T:System.String" />.</summary>
[FriendAccessAllowed]
public abstract class BaseIListConverter : TypeConverter
{
	internal TokenizerHelper _tokenizer;

	internal const char DelimiterChar = ' ';

	/// <summary>Determines if a given type can be converted.</summary>
	/// <returns>true if this type can be converted; otherwise, false.</returns>
	/// <param name="td">Provides contextual information required for conversion.</param>
	/// <param name="t">Type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext td, Type t)
	{
		return t == typeof(string);
	}

	/// <summary>Determines if a given type can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if this type can be converted; otherwise, false.</returns>
	/// <param name="context">Provides contextual information required for conversion.</param>
	/// <param name="destinationType">Type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(string);
	}

	/// <summary>Converts a <see cref="T:System.String" /> to a supported instance of <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the result of the conversion.</returns>
	/// <param name="td">Provides contextual information required for conversion.</param>
	/// <param name="ci">Cultural information to respect during conversion.</param>
	/// <param name="value">String used for conversion.</param>
	/// <exception cref="T:System.ArgumentException">Occurs if value is null and not a <see cref="T:System.String" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext td, CultureInfo ci, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (!(value is string value2))
		{
			throw new ArgumentException(SR.Format(SR.General_BadType, "ConvertFrom"), "value");
		}
		return ConvertFromCore(td, ci, value2);
	}

	/// <summary>Converts a supported instance of <see cref="T:System.Collections.IList" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>String representation of the <see cref="T:System.Collections.IList" /> collection.</returns>
	/// <param name="context">Provides contextual information required for conversion.</param>
	/// <param name="culture">Cultural information to respect during conversion.</param>
	/// <param name="value">Object being evaluated for conversion.</param>
	/// <param name="destinationType">Destination type being evaluated for conversion.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null)
		{
			return ConvertToCore(context, culture, value, destinationType);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	internal abstract object ConvertFromCore(ITypeDescriptorContext td, CultureInfo ci, string value);

	internal abstract object ConvertToCore(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Converters.BaseIListConverter" /> class.</summary>
	protected BaseIListConverter()
	{
	}
}
