using System.ComponentModel;
using System.Globalization;
using System.Xaml;

namespace System.Windows.Markup;

/// <summary>Provides type conversion to convert a string name into an object reference to the object with that name, or to return the name of an object from the object graph.</summary>
public class NameReferenceConverter : TypeConverter
{
	/// <summary>Returns whether this converter can convert an object of one type to another object. </summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false. </returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from. </param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Converts the provided object to another object, using the specified context and culture information. </summary>
	/// <returns>The returned object, which is potentially any object that is type-mapped in the relevant backing assemblies and capable of being declared in XAML with a XAML name reference.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
	/// <param name="value">The reference name string to convert.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="value" /> is a null string or empty string.-or-<see cref="T:System.Xaml.IXamlNameResolver" /> service is missing or invalid.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="context" /> is null.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		ArgumentNullException.ThrowIfNull(context, "context");
		IXamlNameResolver xamlNameResolver = (IXamlNameResolver)context.GetService(typeof(IXamlNameResolver));
		if (xamlNameResolver == null)
		{
			throw new InvalidOperationException(System.SR.MissingNameResolver);
		}
		string text = value as string;
		if (string.IsNullOrEmpty(text))
		{
			throw new InvalidOperationException(System.SR.MustHaveName);
		}
		object obj = xamlNameResolver.Resolve(text);
		if (obj == null)
		{
			string[] names = new string[1] { text };
			obj = xamlNameResolver.GetFixupToken(names, canAssignDirectly: true);
		}
		return obj;
	}

	/// <summary>Returns a value that indicates whether the converter can convert an object to the specified destination type. </summary>
	/// <returns>true if the converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="destinationType">The type to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (context == null || !(context.GetService(typeof(IXamlNameProvider)) is IXamlNameProvider))
		{
			return false;
		}
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts an object to the specified type. This is intended to return XAML reference names for objects in an object graph.</summary>
	/// <returns>The reference name of the input <paramref name="value" /> object.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
	/// <param name="value">The object to retrieve the reference name for.</param>
	/// <param name="destinationType">The type to return. You should always reference the <see cref="T:System.String" /> type.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="T:System.Xaml.IXamlNameProvider" /> service is missing or invalid.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="context" /> is null.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		ArgumentNullException.ThrowIfNull(context, "context");
		return (((IXamlNameProvider)context.GetService(typeof(IXamlNameProvider))) ?? throw new InvalidOperationException(System.SR.MissingNameProvider)).GetName(value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.NameReferenceConverter" /> class.</summary>
	public NameReferenceConverter()
	{
	}
}
