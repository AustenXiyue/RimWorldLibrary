using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert 64-bit unsigned integer objects to and from other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class UInt64Converter : BaseNumberConverter
{
	internal override Type TargetType => typeof(ulong);

	internal override object FromString(string value, int radix)
	{
		return Convert.ToUInt64(value, radix);
	}

	internal override object FromString(string value, NumberFormatInfo formatInfo)
	{
		return ulong.Parse(value, NumberStyles.Integer, formatInfo);
	}

	internal override object FromString(string value, CultureInfo culture)
	{
		return ulong.Parse(value, culture);
	}

	internal override string ToString(object value, NumberFormatInfo formatInfo)
	{
		return ((ulong)value).ToString("G", formatInfo);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.UInt64Converter" /> class. </summary>
	public UInt64Converter()
	{
	}
}
