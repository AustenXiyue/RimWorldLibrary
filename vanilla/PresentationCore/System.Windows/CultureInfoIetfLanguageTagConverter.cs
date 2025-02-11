using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Windows;

/// <summary>Converts instances of <see cref="T:System.Globalization.CultureInfo" /> to and from other data types.</summary>
public class CultureInfoIetfLanguageTagConverter : TypeConverter
{
	/// <summary>Determines whether <see cref="T:System.Windows.CultureInfoIetfLanguageTagConverter" /> can convert from a given type.</summary>
	/// <returns>true if <see cref="T:System.Windows.CultureInfoIetfLanguageTagConverter" /> can convert; otherwise, false. </returns>
	/// <param name="typeDescriptorContext">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> value.</param>
	/// <param name="sourceType">The <see cref="T:System.Type" /> being queried for conversion support.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		return sourceType == typeof(string);
	}

	/// <summary>Determines whether <see cref="T:System.Windows.CultureInfoIetfLanguageTagConverter" /> can convert to a given type.</summary>
	/// <returns>true if <see cref="T:System.Windows.CultureInfoIetfLanguageTagConverter" /> can convert; otherwise, false. </returns>
	/// <param name="typeDescriptorContext">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> value.</param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> being queried for conversion support.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		if (!(destinationType == typeof(InstanceDescriptor)))
		{
			return destinationType == typeof(string);
		}
		return true;
	}

	/// <summary>Converts a given object type to a <see cref="T:System.Globalization.CultureInfo" /> object.</summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> object.</returns>
	/// <param name="typeDescriptorContext">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> value.</param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> object whose value is respected during conversion.</param>
	/// <param name="source">The <see cref="T:System.Type" /> being converted.</param>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		if (source is string name)
		{
			return CultureInfo.GetCultureInfoByIetfLanguageTag(name);
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Converts a <see cref="T:System.Globalization.CultureInfo" /> object to a given object type.</summary>
	/// <returns>A converted CultureInfo object to a given object type.</returns>
	/// <param name="typeDescriptorContext">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> value.</param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> object whose value is respected during conversion.</param>
	/// <param name="value">The object that represents the <see cref="T:System.Globalization.CultureInfo" /> to convert.</param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> of the returned converted object.</param>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value is CultureInfo cultureInfo2)
		{
			if (destinationType == typeof(string))
			{
				return cultureInfo2.IetfLanguageTag;
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(CultureInfo).GetMethod("GetCultureInfo", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, new Type[1] { typeof(string) }, null), new object[1] { cultureInfo2.Name });
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.CultureInfoIetfLanguageTagConverter" /> class.</summary>
	public CultureInfoIetfLanguageTagConverter()
	{
	}
}
