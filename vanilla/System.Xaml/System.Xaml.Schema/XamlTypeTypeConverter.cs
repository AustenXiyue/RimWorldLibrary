using System.ComponentModel;
using System.Globalization;

namespace System.Xaml.Schema;

/// <summary>Converts a <see cref="T:System.Xaml.XamlType" /> object to and from a string that represents the type name. This functionality is used for XAML extensibility by markup definitions, via <see cref="T:System.Windows.Markup.PropertyDefinition" />.</summary>
public class XamlTypeTypeConverter : TypeConverter
{
	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Xaml.XamlType" />, using the specified context.</summary>
	/// <returns>true if this converter can perform the operation; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="sourceType">The type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string);
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Xaml.XamlType" />, using the specified context.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> cannot be converted.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		string text = value as string;
		if (context != null && text != null)
		{
			XamlType xamlType = ConvertStringToXamlType(context, text);
			if (xamlType != null)
			{
				return xamlType;
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Xaml.XamlType" /> can be converted to the specified type, using the specified context.</summary>
	/// <returns>true if this converter can perform the operation; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(string);
	}

	/// <summary>Attempts to convert a <see cref="T:System.Xaml.XamlType" /> to the specified type, using the specified context.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert the object to.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> cannot be converted.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		XamlType xamlType = value as XamlType;
		if (context != null && xamlType != null && destinationType == typeof(string))
		{
			string text = ConvertXamlTypeToString(context, xamlType);
			if (text != null)
			{
				return text;
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	internal static string ConvertXamlTypeToString(ITypeDescriptorContext context, XamlType xamlType)
	{
		INamespacePrefixLookup service = GetService<INamespacePrefixLookup>(context);
		if (service == null)
		{
			return null;
		}
		return new XamlTypeName(xamlType).ToString(service);
	}

	private static XamlType ConvertStringToXamlType(ITypeDescriptorContext context, string typeName)
	{
		IXamlNamespaceResolver service = GetService<IXamlNamespaceResolver>(context);
		if (service == null)
		{
			return null;
		}
		XamlTypeName typeName2 = XamlTypeName.Parse(typeName, service);
		IXamlSchemaContextProvider service2 = GetService<IXamlSchemaContextProvider>(context);
		if (service2 == null)
		{
			return null;
		}
		if (service2.SchemaContext == null)
		{
			return null;
		}
		return GetXamlTypeOrUnknown(service2.SchemaContext, typeName2);
	}

	private static TService GetService<TService>(ITypeDescriptorContext context) where TService : class
	{
		return context.GetService(typeof(TService)) as TService;
	}

	private static XamlType GetXamlTypeOrUnknown(XamlSchemaContext schemaContext, XamlTypeName typeName)
	{
		XamlType xamlType = schemaContext.GetXamlType(typeName);
		if (xamlType != null)
		{
			return xamlType;
		}
		XamlType[] array = null;
		if (typeName.HasTypeArgs)
		{
			array = new XamlType[typeName.TypeArguments.Count];
			for (int i = 0; i < typeName.TypeArguments.Count; i++)
			{
				array[i] = GetXamlTypeOrUnknown(schemaContext, typeName.TypeArguments[i]);
			}
		}
		return new XamlType(typeName.Namespace, typeName.Name, array, schemaContext);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlTypeTypeConverter" /> class.</summary>
	public XamlTypeTypeConverter()
	{
	}
}
