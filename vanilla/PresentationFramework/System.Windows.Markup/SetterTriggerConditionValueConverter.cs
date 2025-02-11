using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Baml2006;
using System.Xaml;
using System.Xaml.Schema;

namespace System.Windows.Markup;

/// <summary>Provides type conversion analogous behavior for <see cref="T:System.Windows.Setter" />, <see cref="T:System.Windows.Trigger" /> and <see cref="T:System.Windows.Condition" /> types that deal with <see cref="T:System.Windows.DependencyProperty" /> values. This converter only supports ConvertFrom.</summary>
public sealed class SetterTriggerConditionValueConverter : TypeConverter
{
	/// <summary>Returns a value that indicates whether the converter can convert from a source object to a side-effect-produced <see cref="T:System.Windows.Setter" />, <see cref="T:System.Windows.Trigger" /> or <see cref="T:System.Windows.Condition" /> . </summary>
	/// <returns>true if the converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">The type to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string) || sourceType == typeof(byte[]))
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether the converter can convert to the specified destination type. Always returns false.</summary>
	/// <returns>Always returns false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="destinationType">The type to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return false;
	}

	/// <summary>Converts the converted source value if an underlying type converter can be obtained from context. Otherwise returns an unconverted source.</summary>
	/// <returns>The converter object, or possibly an unconverted source.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="source">The object to convert.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="context" /> or <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">No <see cref="T:System.Xaml.IXamlSchemaContextProvider" /> service available.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
	{
		return ResolveValue(context, null, culture, source);
	}

	/// <summary>Converts the specified object to the specified type. Always throws an exception.</summary>
	/// <returns>Always throws an exception.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert to.</param>
	/// <exception cref="T:System.InvalidOperationException">Thrown in all cases.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		throw GetConvertToException(value, destinationType);
	}

	internal static object ResolveValue(ITypeDescriptorContext serviceProvider, DependencyProperty property, CultureInfo culture, object source)
	{
		if (serviceProvider == null)
		{
			throw new ArgumentNullException("serviceProvider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (!(source is byte[]) && !(source is string) && !(source is Stream))
		{
			return source;
		}
		XamlSchemaContext schemaContext = ((serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider) ?? throw new NotSupportedException(SR.Format(SR.ParserCannotConvertPropertyValue, "Value", typeof(object).FullName))).SchemaContext;
		if (property != null)
		{
			XamlMember xamlMember = schemaContext.GetXamlType(property.OwnerType).GetMember(property.Name);
			if (xamlMember == null)
			{
				xamlMember = schemaContext.GetXamlType(property.OwnerType).GetAttachableMember(property.Name);
			}
			XamlValueConverter<TypeConverter> xamlValueConverter = null;
			if (xamlMember != null)
			{
				if (xamlMember.Type.UnderlyingType.IsEnum && schemaContext is Baml2006SchemaContext)
				{
					xamlValueConverter = XamlReader.BamlSharedSchemaContext.GetTypeConverter(xamlMember.Type.UnderlyingType);
				}
				else
				{
					xamlValueConverter = xamlMember.TypeConverter;
					if (xamlValueConverter == null)
					{
						xamlValueConverter = xamlMember.Type.TypeConverter;
					}
				}
			}
			else
			{
				xamlValueConverter = schemaContext.GetXamlType(property.PropertyType).TypeConverter;
			}
			if (xamlValueConverter.ConverterType == null)
			{
				return source;
			}
			TypeConverter typeConverter = null;
			if (xamlMember != null && xamlMember.Type.UnderlyingType == typeof(bool))
			{
				if (!(source is string))
				{
					if (source is byte[])
					{
						if (source is byte[] array && array.Length == 1)
						{
							return array[0] != 0;
						}
						throw new NotSupportedException(SR.Format(SR.ParserCannotConvertPropertyValue, "Value", typeof(object).FullName));
					}
					throw new NotSupportedException(SR.Format(SR.ParserCannotConvertPropertyValue, "Value", typeof(object).FullName));
				}
				typeConverter = new BooleanConverter();
			}
			else
			{
				typeConverter = xamlValueConverter.ConverterInstance;
			}
			return typeConverter.ConvertFrom(serviceProvider, culture, source);
		}
		return source;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.SetterTriggerConditionValueConverter" /> class. </summary>
	public SetterTriggerConditionValueConverter()
	{
	}
}
