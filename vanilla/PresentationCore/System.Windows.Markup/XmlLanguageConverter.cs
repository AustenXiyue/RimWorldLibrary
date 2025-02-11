using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Windows.Markup;

/// <summary>Provides type conversion for the <see cref="T:System.Windows.Markup.XmlLanguage" /> class.</summary>
public class XmlLanguageConverter : TypeConverter
{
	/// <summary>Returns whether this converter can convert an object of one type to the <see cref="T:System.Windows.Markup.XmlLanguage" /> type supported by this converter.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A type that represents the type you want to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		return sourceType == typeof(string);
	}

	/// <summary>Returns whether this converter can convert the object to the specified type.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="destinationType">The type you want to convert to. </param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		if (!(destinationType == typeof(InstanceDescriptor)))
		{
			return destinationType == typeof(string);
		}
		return true;
	}

	/// <summary>Converts the specified string value to the <see cref="T:System.Windows.Markup.XmlLanguage" /> type supported by this converter.</summary>
	/// <returns>An object that represents the converted value.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="source">The string to convert.</param>
	/// <exception cref="T:System.InvalidOperationException">Conversion could not be performed.</exception>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		if (source is string ietfLanguageTag)
		{
			return XmlLanguage.GetLanguage(ietfLanguageTag);
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Converts the specified <see cref="T:System.Windows.Markup.XmlLanguage" /> to the specified type.</summary>
	/// <returns>An object that represents the converted value.</returns>
	/// <param name="typeDescriptorContext">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="value">The object to convert. This is expected to be type <see cref="T:System.Windows.Markup.XmlLanguage" />.</param>
	/// <param name="destinationType">A type that represents the type you want to convert to. </param>
	/// <exception cref="T:System.InvalidOperationException">Conversion could not be performed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value is XmlLanguage xmlLanguage)
		{
			if (destinationType == typeof(string))
			{
				return xmlLanguage.IetfLanguageTag;
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(XmlLanguage).GetMethod("GetLanguage", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, new Type[1] { typeof(string) }, null), new object[1] { xmlLanguage.IetfLanguageTag });
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XmlLanguageConverter" /> class.</summary>
	public XmlLanguageConverter()
	{
	}
}
