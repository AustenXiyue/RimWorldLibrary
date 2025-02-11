using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>A type converter that is used to construct a markup extension from a <see cref="T:System.Windows.TemplateBindingExpression" /> instance during serialization. </summary>
public class TemplateBindingExpressionConverter : TypeConverter
{
	/// <summary>Returns whether this converter can convert the object to the specified type, using the specified context. </summary>
	/// <returns>true if this converter can perform the requested conversion; otherwise, false. Only a <paramref name="destinationType" /> of <see cref="T:System.Windows.Markup.MarkupExtension" /> returns true.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> implementation that provides a format context. </param>
	/// <param name="destinationType">The desired type of the conversion's output.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(MarkupExtension))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the given value object to a <see cref="T:System.Windows.Markup.MarkupExtension" /> type.</summary>
	/// <returns>The converted value. </returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> implementation that provides a format context. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> object. If a null reference is passed, the current culture is assumed. </param>
	/// <param name="value">The value to convert.</param>
	/// <param name="destinationType">The desired type to convert to.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(MarkupExtension))
		{
			return ((value as TemplateBindingExpression) ?? throw new ArgumentException(SR.Format(SR.MustBeOfType, "value", "TemplateBindingExpression"))).TemplateBindingExtension;
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TemplateBindingExpressionConverter" /> class.</summary>
	public TemplateBindingExpressionConverter()
	{
	}
}
