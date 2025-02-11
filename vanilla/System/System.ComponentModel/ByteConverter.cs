using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a type converter to convert 8-bit unsigned integer objects to and from various other representations.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class ByteConverter : BaseNumberConverter
{
	internal override Type TargetType => typeof(byte);

	internal override object FromString(string value, int radix)
	{
		return Convert.ToByte(value, radix);
	}

	internal override object FromString(string value, NumberFormatInfo formatInfo)
	{
		return byte.Parse(value, NumberStyles.Integer, formatInfo);
	}

	internal override object FromString(string value, CultureInfo culture)
	{
		return byte.Parse(value, culture);
	}

	internal override string ToString(object value, NumberFormatInfo formatInfo)
	{
		return ((byte)value).ToString("G", formatInfo);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ByteConverter" /> class. </summary>
	public ByteConverter()
	{
	}
}
