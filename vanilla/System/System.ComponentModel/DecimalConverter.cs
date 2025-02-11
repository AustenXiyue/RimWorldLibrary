using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert <see cref="T:System.Decimal" /> objects to and from various other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class DecimalConverter : BaseNumberConverter
{
	internal override bool AllowHex => false;

	internal override Type TargetType => typeof(decimal);

	/// <summary>Gets a value indicating whether this converter can convert an object to the given destination type using the context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you wish to convert to. </param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the given value object to a <see cref="T:System.Decimal" /> using the arguments.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">An optional <see cref="T:System.Globalization.CultureInfo" />. If not supplied, the current culture is assumed. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the value to. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationType" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(InstanceDescriptor) && value is decimal)
		{
			object[] arguments = new object[1] { decimal.GetBits((decimal)value) };
			MemberInfo constructor = typeof(decimal).GetConstructor(new Type[1] { typeof(int[]) });
			if (constructor != null)
			{
				return new InstanceDescriptor(constructor, arguments);
			}
			return null;
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	internal override object FromString(string value, int radix)
	{
		return Convert.ToDecimal(value, CultureInfo.CurrentCulture);
	}

	internal override object FromString(string value, NumberFormatInfo formatInfo)
	{
		return decimal.Parse(value, NumberStyles.Float, formatInfo);
	}

	internal override object FromString(string value, CultureInfo culture)
	{
		return decimal.Parse(value, culture);
	}

	internal override string ToString(object value, NumberFormatInfo formatInfo)
	{
		return ((decimal)value).ToString("G", formatInfo);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DecimalConverter" /> class. </summary>
	public DecimalConverter()
	{
	}
}
