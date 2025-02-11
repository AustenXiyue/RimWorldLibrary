using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System;

/// <summary>Converts a <see cref="T:System.String" /> type to a <see cref="T:System.Uri" /> type, and vice versa.</summary>
public class UriTypeConverter : TypeConverter
{
	/// <summary>Initializes a new instance of the <see cref="T:System.UriTypeConverter" /> class.</summary>
	public UriTypeConverter()
	{
	}

	private bool CanConvert(Type type)
	{
		if (type == typeof(string))
		{
			return true;
		}
		if (type == typeof(Uri))
		{
			return true;
		}
		return type == typeof(InstanceDescriptor);
	}

	/// <summary>Returns whether this converter can convert an object of the given type to the type of this converter.</summary>
	/// <returns>true if <paramref name="sourceType" /> is a <see cref="T:System.String" /> type or a <see cref="T:System.Uri" /> type can be assigned from <paramref name="sourceType" />; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" />  that provides a format context.</param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type that you want to convert from.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="sourceType" /> parameter is null.</exception>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == null)
		{
			throw new ArgumentNullException("sourceType");
		}
		return CanConvert(sourceType);
	}

	/// <summary>Returns whether this converter can convert the object to the specified type, using the specified context.</summary>
	/// <returns>true if <paramref name="destinationType" /> is of type <see cref="T:System.ComponentModel.Design.Serialization.InstanceDescriptor" />, <see cref="T:System.String" />, or <see cref="T:System.Uri" />; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" />  that provides a format context.</param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type that you want to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == null)
		{
			return false;
		}
		return CanConvert(destinationType);
	}

	/// <summary>Converts the given object to the type of this converter, using the specified context and culture information.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" />  that provides a format context.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!CanConvertFrom(context, value.GetType()))
		{
			throw new NotSupportedException(global::Locale.GetText("Cannot convert from value."));
		}
		if (value is Uri)
		{
			return value;
		}
		if (value is string uriString)
		{
			return new Uri(uriString, UriKind.RelativeOrAbsolute);
		}
		if (value is InstanceDescriptor instanceDescriptor)
		{
			return instanceDescriptor.Invoke();
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts a given value object to the specified type, using the specified context and culture information.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" />  that provides a format context.</param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" />. If null is passed, the current culture is assumed.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the <paramref name="value" /> parameter to.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationType" /> parameter is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (!CanConvertTo(context, destinationType))
		{
			throw new NotSupportedException(global::Locale.GetText("Cannot convert to destination type."));
		}
		Uri uri = value as Uri;
		if (uri != null)
		{
			if (destinationType == typeof(string))
			{
				return uri.ToString();
			}
			if (destinationType == typeof(Uri))
			{
				return uri;
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(Uri).GetConstructor(new Type[2]
				{
					typeof(string),
					typeof(UriKind)
				}), new object[2]
				{
					uri.ToString(),
					uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative
				});
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Returns whether the given value object is a <see cref="T:System.Uri" /> or a <see cref="T:System.Uri" /> can be created from it.</summary>
	/// <returns>true if <paramref name="value" /> is a <see cref="T:System.Uri" /> or a <see cref="T:System.String" /> from which a <see cref="T:System.Uri" /> can be created; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" />  that provides a format context.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to test for validity.</param>
	public override bool IsValid(ITypeDescriptorContext context, object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!(value is string))
		{
			return value is Uri;
		}
		return true;
	}
}
