using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Windows;

/// <summary>A type converter that is used to construct a <see cref="T:System.Windows.TemplateBindingExtension" /> from an instance during serialization.</summary>
public class TemplateBindingExtensionConverter : TypeConverter
{
	/// <summary>Returns whether this converter can convert the object to the specified type, using the specified context. </summary>
	/// <returns>true if this converter can perform the requested conversion; otherwise, false. Only a <paramref name="destinationType" /> of <see cref="T:System.ComponentModel.Design.Serialization.InstanceDescriptor" /> will return true.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> implementation that provides a format context. </param>
	/// <param name="destinationType">The desired type of the conversion's output.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the given value object to the specified type. </summary>
	/// <returns>The converted value. </returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> implementation that provides a format context. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> object. If a null reference is passed, the current culture is assumed. </param>
	/// <param name="value">The value to convert.</param>
	/// <param name="destinationType">The desired type to convert to.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!(value is TemplateBindingExtension templateBindingExtension))
			{
				throw new ArgumentException(SR.Format(SR.MustBeOfType, "value", "TemplateBindingExtension"), "value");
			}
			return new InstanceDescriptor(typeof(TemplateBindingExtension).GetConstructor(new Type[1] { typeof(DependencyProperty) }), new object[1] { templateBindingExtension.Property });
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TemplateBindingExtensionConverter" /> class.</summary>
	public TemplateBindingExtensionConverter()
	{
	}
}
