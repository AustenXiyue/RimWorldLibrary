using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert 16-bit unsigned integer objects to and from other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class UInt16Converter : BaseNumberConverter
{
	internal override Type TargetType => typeof(ushort);

	internal override object FromString(string value, int radix)
	{
		return Convert.ToUInt16(value, radix);
	}

	internal override object FromString(string value, NumberFormatInfo formatInfo)
	{
		return ushort.Parse(value, NumberStyles.Integer, formatInfo);
	}

	internal override object FromString(string value, CultureInfo culture)
	{
		return ushort.Parse(value, culture);
	}

	internal override string ToString(object value, NumberFormatInfo formatInfo)
	{
		return ((ushort)value).ToString("G", formatInfo);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.UInt16Converter" /> class. </summary>
	public UInt16Converter()
	{
	}
}
