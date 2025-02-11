using System.ComponentModel;
using System.Globalization;

namespace System.Security.Authentication.ExtendedProtection;

/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicyTypeConverter" /> class represents the type converter for extended protection policy used by the server to validate incoming client connections. </summary>
[System.MonoTODO]
public class ExtendedProtectionPolicyTypeConverter : TypeConverter
{
	/// <summary>Returns whether this converter can convert the object to the specified type.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise false.</returns>
	/// <param name="context">The object to convert.</param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you want to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		throw new NotImplementedException();
	}

	/// <summary>Convert the object to the specified type</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted <paramref name="value" /> parameter.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> object. If null is passed, the current culture is assumed.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. This should be a <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> object.</param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the value parameter to.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationType" /> parameter is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The conversion could not be performed.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicyTypeConverter" /> class.</summary>
	public ExtendedProtectionPolicyTypeConverter()
	{
	}
}
