using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Windows;

/// <summary>Converts from parsed XAML to <see cref="T:System.Windows.DynamicResourceExtension" /> and supports dynamic resource references made from XAML. </summary>
public class DynamicResourceExtensionConverter : TypeConverter
{
	/// <summary>Returns a value indicating whether this converter can convert an object to the given destination type using the context. </summary>
	/// <returns>true if <paramref name="destinationType" /> is type of <see cref="T:System.ComponentModel.Design.Serialization.InstanceDescriptor" />; otherwise, false.</returns>
	/// <param name="context">Context in which the provided type should be evaluated.</param>
	/// <param name="destinationType">The type of the destination/output of conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the specified object to another type.</summary>
	/// <returns>The returned converted object. Cast this to the requested type. Ordinarily this should be cast to <see cref="T:System.ComponentModel.Design.Serialization.InstanceDescriptor" />.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object that provides a format context.</param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> object that specifies the culture to represent the number. </param>
	/// <param name="value">Value to be converted. This is expected to be type <see cref="T:System.Windows.DynamicResourceExtension" />.</param>
	/// <param name="destinationType">Type that should be converted to. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> could not be assigned as <see cref="T:System.Windows.DynamicResourceExtension" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!(value is DynamicResourceExtension dynamicResourceExtension))
			{
				throw new ArgumentException(SR.Format(SR.MustBeOfType, "value", "DynamicResourceExtension"), "value");
			}
			return new InstanceDescriptor(typeof(DynamicResourceExtension).GetConstructor(new Type[1] { typeof(object) }), new object[1] { dynamicResourceExtension.ResourceKey });
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DynamicResourceExtensionConverter" /> class.</summary>
	public DynamicResourceExtensionConverter()
	{
	}
}
