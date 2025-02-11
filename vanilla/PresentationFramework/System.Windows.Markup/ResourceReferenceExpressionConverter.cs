using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Markup;

/// <summary>Converts instances of ResourceReferenceExpression to and from other types. </summary>
public class ResourceReferenceExpressionConverter : ExpressionConverter
{
	/// <summary>Returns a value that indicates whether the converter can convert from a source object to a ResourceReferenceExpression object. </summary>
	/// <returns>true if the converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">The type to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Returns a value that indicates whether the converter can convert a ResourceReferenceExpression object to the specified destination type. </summary>
	/// <returns>true if the converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="destinationType">The type to convert to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(MarkupExtension))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the specified value to the ResourceReferenceExpression type. </summary>
	/// <returns>The converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="value">The object to convert.</param>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the specified ResourceReferenceExpression object to the specified type.</summary>
	/// <returns>The converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> must be of type ResourceReferenceExpression.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (!(value is ResourceReferenceExpression resourceReferenceExpression))
		{
			throw new ArgumentException(SR.Format(SR.MustBeOfType, "value", "ResourceReferenceExpression"));
		}
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(MarkupExtension))
		{
			return new DynamicResourceExtension(resourceReferenceExpression.ResourceKey);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ResourceReferenceExpressionConverter" /> class. </summary>
	public ResourceReferenceExpressionConverter()
	{
	}
}
