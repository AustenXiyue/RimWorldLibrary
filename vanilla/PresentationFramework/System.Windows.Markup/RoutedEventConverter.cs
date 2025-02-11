using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Xaml;
using MS.Internal.WindowsBase;

namespace System.Windows.Markup;

/// <summary>Converts a <see cref="T:System.Windows.RoutedEvent" /> object from a string.</summary>
public sealed class RoutedEventConverter : TypeConverter
{
	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Windows.RoutedEvent" />.</summary>
	/// <returns>true if this converter can perform the operation; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="sourceType">The type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.RoutedEvent" /> can be converted to the specified type.</summary>
	/// <returns>Always returns false.</returns>
	/// <param name="typeDescriptorContext">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		return false;
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.RoutedEvent" /> object, using the specified context.</summary>
	/// <returns>The conversion result.</returns>
	/// <param name="typeDescriptorContext">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="cultureInfo">Culture specific information.</param>
	/// <param name="source">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> is not a string or cannot be converted.</exception>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		string text = source as string;
		RoutedEvent routedEvent = null;
		if (text != null)
		{
			text = text.Trim();
			if (typeDescriptorContext != null)
			{
				IXamlTypeResolver xamlTypeResolver = typeDescriptorContext.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
				Type type = null;
				if (xamlTypeResolver != null)
				{
					int num = text.IndexOf('.');
					if (num != -1)
					{
						string qualifiedTypeName = text.Substring(0, num);
						text = text.Substring(num + 1);
						type = xamlTypeResolver.Resolve(qualifiedTypeName);
					}
				}
				if (type == null)
				{
					IXamlSchemaContextProvider xamlSchemaContextProvider = typeDescriptorContext.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider;
					IAmbientProvider ambientProvider = typeDescriptorContext.GetService(typeof(IAmbientProvider)) as IAmbientProvider;
					if (xamlSchemaContextProvider != null && ambientProvider != null)
					{
						XamlType xamlType = xamlSchemaContextProvider.SchemaContext.GetXamlType(typeof(Style));
						List<XamlType> list = new List<XamlType>();
						list.Add(xamlType);
						XamlMember member = xamlType.GetMember("TargetType");
						AmbientPropertyValue firstAmbientValue = ambientProvider.GetFirstAmbientValue(list, member);
						if (firstAmbientValue != null)
						{
							type = firstAmbientValue.Value as Type;
						}
						if (type == null)
						{
							type = typeof(FrameworkElement);
						}
					}
				}
				if (type != null)
				{
					Type type2 = type;
					while (null != type2)
					{
						SecurityHelper.RunClassConstructor(type2);
						type2 = type2.BaseType;
					}
					routedEvent = EventManager.GetRoutedEventFromName(text, type);
				}
			}
		}
		if (routedEvent == null)
		{
			throw GetConvertFromException(source);
		}
		return routedEvent;
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.RoutedEvent" /> to the specified type. Throws an exception in all cases.</summary>
	/// <returns>Always throws an exception.</returns>
	/// <param name="typeDescriptorContext">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="cultureInfo">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert the object to.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> cannot be converted. This is not a functioning converter for a save path..</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> or <paramref name="destinationType" /> are null.</exception>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		throw GetConvertToException(value, destinationType);
	}

	private string ExtractNamespaceString(ref string nameString, ParserContext parserContext)
	{
		int num = nameString.IndexOf(':');
		string text = string.Empty;
		if (num != -1)
		{
			text = nameString.Substring(0, num);
			nameString = nameString.Substring(num + 1);
		}
		return parserContext.XmlnsDictionary[text] ?? throw new ArgumentException(SR.Format(SR.ParserPrefixNSProperty, text, nameString));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.RoutedEventConverter" /> class.</summary>
	public RoutedEventConverter()
	{
	}
}
