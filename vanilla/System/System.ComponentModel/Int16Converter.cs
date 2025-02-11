using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert 16-bit signed integer objects to and from other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class Int16Converter : BaseNumberConverter
{
	internal override Type TargetType => typeof(short);

	internal override object FromString(string value, int radix)
	{
		return Convert.ToInt16(value, radix);
	}

	internal override object FromString(string value, CultureInfo culture)
	{
		return short.Parse(value, culture);
	}

	internal override object FromString(string value, NumberFormatInfo formatInfo)
	{
		return short.Parse(value, NumberStyles.Integer, formatInfo);
	}

	internal override string ToString(object value, NumberFormatInfo formatInfo)
	{
		return ((short)value).ToString("G", formatInfo);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Int16Converter" /> class. </summary>
	public Int16Converter()
	{
	}
}
