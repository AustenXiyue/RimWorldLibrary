using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert multiline strings to a simple string.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class MultilineStringConverter : TypeConverter
{
	/// <summary>Converts the given value object to the specified type, using the specified context and culture information.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" />  that provides a format context.</param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" />. If null is passed, the current culture is assumed.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the value parameter to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string) && value is string)
		{
			return global::SR.GetString("(Text)");
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Returns a collection of properties for the type of array specified by the <paramref name="value" /> parameter, using the specified context and attributes.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> with the properties that are exposed for this data type, or null if there are no properties.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" />  that provides a format context.</param>
	/// <param name="value">An <see cref="T:System.Object" /> that specifies the type of array for which to get properties.</param>
	/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that is used as a filter.</param>
	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		return null;
	}

	/// <summary>Returns whether this object supports properties, using the specified context.</summary>
	/// <returns>true if <see cref="Overload:System.ComponentModel.MultilineStringConverter.GetProperties" /> should be called to find the properties of this object; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" />  that provides a format context.</param>
	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		return false;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MultilineStringConverter" /> class. </summary>
	public MultilineStringConverter()
	{
	}
}
