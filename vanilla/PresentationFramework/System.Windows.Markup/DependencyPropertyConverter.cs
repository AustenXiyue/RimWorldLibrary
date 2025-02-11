using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Baml2006;
using System.Windows.Controls;
using System.Xaml;

namespace System.Windows.Markup;

/// <summary>Converts from a string to a <see cref="T:System.Windows.DependencyProperty" /> object.</summary>
public sealed class DependencyPropertyConverter : TypeConverter
{
	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Windows.DependencyProperty" />.</summary>
	/// <returns>true if this converter can perform the operation; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="sourceType">The type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string) || sourceType == typeof(byte[]))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.DependencyProperty" /> can be converted to the specified type.</summary>
	/// <returns>true if this converter can perform the operation; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return false;
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.DependencyProperty" />, using the specified context.</summary>
	/// <returns>The converted object. If the conversion is successful, this is a <see cref="T:System.Windows.DependencyProperty" />.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="source">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> cannot be converted.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="context" /> or <paramref name="source" /> are null.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		DependencyProperty dependencyProperty = ResolveProperty(context, null, source);
		if (dependencyProperty != null)
		{
			return dependencyProperty;
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.DependencyProperty" /> to the specified type, using the specified context. Always throws an exception.</summary>
	/// <returns>Always throws an exception.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert the object to.</param>
	/// <exception cref="T:System.NotSupportedException">In all cases.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		throw GetConvertToException(value, destinationType);
	}

	internal static DependencyProperty ResolveProperty(IServiceProvider serviceProvider, string targetName, object source)
	{
		Type type = null;
		string text = null;
		if (source is DependencyProperty result)
		{
			return result;
		}
		if (source is byte[] array)
		{
			Baml2006SchemaContext baml2006SchemaContext = (serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider).SchemaContext as Baml2006SchemaContext;
			if (array.Length == 2)
			{
				short propertyId = (short)(array[0] | (array[1] << 8));
				return baml2006SchemaContext.GetDependencyProperty(propertyId);
			}
			using BinaryReader binaryReader = new BinaryReader(new MemoryStream(array));
			type = baml2006SchemaContext.GetXamlType(binaryReader.ReadInt16()).UnderlyingType;
			text = binaryReader.ReadString();
		}
		else
		{
			if (!(source is string text2))
			{
				throw new NotSupportedException(SR.Format(SR.ParserCannotConvertPropertyValue, "Property", typeof(DependencyProperty).FullName));
			}
			string text3 = text2.Trim();
			if (text3.Contains("."))
			{
				int num = text3.LastIndexOf('.');
				string qualifiedTypeName = text3.Substring(0, num);
				text = text3.Substring(num + 1);
				type = (serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver).Resolve(qualifiedTypeName);
			}
			else
			{
				int num2 = text3.LastIndexOf(':');
				text = text3.Substring(num2 + 1);
			}
		}
		if (type == null && targetName != null)
		{
			IAmbientProvider ambientProvider = serviceProvider.GetService(typeof(IAmbientProvider)) as IAmbientProvider;
			type = GetTypeFromName((serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider).SchemaContext, ambientProvider, targetName);
		}
		if (type == null)
		{
			XamlSchemaContext schemaContext = ((serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider) ?? throw new NotSupportedException(SR.Format(SR.ParserCannotConvertPropertyValue, "Property", typeof(DependencyProperty).FullName))).SchemaContext;
			XamlType xamlType = schemaContext.GetXamlType(typeof(Style));
			XamlType xamlType2 = schemaContext.GetXamlType(typeof(FrameworkTemplate));
			XamlType xamlType3 = schemaContext.GetXamlType(typeof(DataTemplate));
			XamlType xamlType4 = schemaContext.GetXamlType(typeof(ControlTemplate));
			List<XamlType> list = new List<XamlType>();
			list.Add(xamlType);
			list.Add(xamlType2);
			list.Add(xamlType3);
			list.Add(xamlType4);
			XamlMember member = xamlType.GetMember("TargetType");
			XamlMember member2 = xamlType2.GetMember("Template");
			XamlMember member3 = xamlType4.GetMember("TargetType");
			AmbientPropertyValue firstAmbientValue = ((serviceProvider.GetService(typeof(IAmbientProvider)) as IAmbientProvider) ?? throw new NotSupportedException(SR.Format(SR.ParserCannotConvertPropertyValue, "Property", typeof(DependencyProperty).FullName))).GetFirstAmbientValue(list, member, member2, member3);
			if (firstAmbientValue != null)
			{
				if (firstAmbientValue.Value is Type)
				{
					type = (Type)firstAmbientValue.Value;
				}
				else
				{
					if (!(firstAmbientValue.Value is TemplateContent))
					{
						throw new NotSupportedException(SR.Format(SR.ParserCannotConvertPropertyValue, "Property", typeof(DependencyProperty).FullName));
					}
					type = (firstAmbientValue.Value as TemplateContent).OwnerTemplate.TargetTypeInternal;
				}
			}
		}
		if (type != null && text != null)
		{
			return DependencyProperty.FromName(text, type);
		}
		throw new NotSupportedException(SR.Format(SR.ParserCannotConvertPropertyValue, "Property", typeof(DependencyProperty).FullName));
	}

	private static Type GetTypeFromName(XamlSchemaContext schemaContext, IAmbientProvider ambientProvider, string target)
	{
		XamlType xamlType = schemaContext.GetXamlType(typeof(FrameworkTemplate));
		XamlMember member = xamlType.GetMember("Template");
		if (ambientProvider.GetFirstAmbientValue(new XamlType[1] { xamlType }, member).Value is TemplateContent templateContent)
		{
			return templateContent.GetTypeForName(target).UnderlyingType;
		}
		return null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.DependencyPropertyConverter" /> class.</summary>
	public DependencyPropertyConverter()
	{
	}
}
