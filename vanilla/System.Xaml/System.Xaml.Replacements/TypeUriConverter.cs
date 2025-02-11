using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Xaml.Replacements;

internal class TypeUriConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == null)
		{
			throw new ArgumentNullException("sourceType");
		}
		if (!(sourceType == typeof(string)))
		{
			return sourceType == typeof(Uri);
		}
		return true;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (!(destinationType == typeof(InstanceDescriptor)) && !(destinationType == typeof(string)))
		{
			return destinationType == typeof(Uri);
		}
		return true;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		Uri uri = value as Uri;
		if (uri != null)
		{
			UriKind uriKind = UriKind.RelativeOrAbsolute;
			if (uri.IsWellFormedOriginalString())
			{
				uriKind = (uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(Uri).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[2]
				{
					typeof(string),
					typeof(UriKind)
				}, null), new object[2] { uri.OriginalString, uriKind });
			}
			if (destinationType == typeof(string))
			{
				return uri.OriginalString;
			}
			if (destinationType == typeof(Uri))
			{
				return new Uri(uri.OriginalString, uriKind);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string uriString)
		{
			if (Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
			{
				return new Uri(uriString, UriKind.Absolute);
			}
			if (Uri.IsWellFormedUriString(uriString, UriKind.Relative))
			{
				return new Uri(uriString, UriKind.Relative);
			}
			return new Uri(uriString, UriKind.RelativeOrAbsolute);
		}
		Uri uri = value as Uri;
		if (uri != null)
		{
			if (uri.IsWellFormedOriginalString())
			{
				return new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
			}
			return new Uri(uri.OriginalString, UriKind.RelativeOrAbsolute);
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override bool IsValid(ITypeDescriptorContext context, object value)
	{
		if (value is string uriString)
		{
			Uri result;
			return Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out result);
		}
		return value is Uri;
	}
}
