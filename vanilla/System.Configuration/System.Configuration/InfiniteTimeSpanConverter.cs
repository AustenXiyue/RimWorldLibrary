using System.ComponentModel;
using System.Globalization;

namespace System.Configuration;

/// <summary>Converts between a string and the standard infinite <see cref="T:System.TimeSpan" /> value.</summary>
public sealed class InfiniteTimeSpanConverter : ConfigurationConverterBase
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.InfiniteTimeSpanConverter" /> class.</summary>
	public InfiniteTimeSpanConverter()
	{
	}

	/// <summary>Converts a <see cref="T:System.String" /> to a <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>The <see cref="F:System.TimeSpan.MaxValue" />, if the <paramref name="data" /> parameter is the <see cref="T:System.String" /> infinite; otherwise, the <see cref="T:System.TimeSpan" /> representing the <paramref name="data" /> parameter in minutes.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object used during conversion.</param>
	/// <param name="data">The <see cref="T:System.String" /> object to convert.</param>
	public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
	{
		if ((string)data == "Infinite")
		{
			return TimeSpan.MaxValue;
		}
		return TimeSpan.Parse((string)data);
	}

	/// <summary>Converts a <see cref="T:System.TimeSpan" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>The <see cref="T:System.String" /> "infinite", if the <paramref name="value" /> parameter is <see cref="F:System.TimeSpan.MaxValue" />; otherwise, the <see cref="T:System.String" /> representing the <paramref name="value" /> parameter in minutes.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> used during object conversion.</param>
	/// <param name="value">The value to convert.</param>
	/// <param name="type">The conversion type.</param>
	public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
	{
		if (value.GetType() != typeof(TimeSpan))
		{
			throw new ArgumentException();
		}
		if ((TimeSpan)value == TimeSpan.MaxValue)
		{
			return "Infinite";
		}
		return value.ToString();
	}
}
