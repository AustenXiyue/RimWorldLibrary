using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Windows.Markup;

internal class StaticExtensionConverter : TypeConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			if (!(value is StaticExtension staticExtension))
			{
				throw new ArgumentException(System.SR.Format(System.SR.MustBeOfType, "value", "StaticExtension"));
			}
			return new InstanceDescriptor(typeof(StaticExtension).GetConstructor(new Type[1] { typeof(string) }), new object[1] { staticExtension.Member });
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
