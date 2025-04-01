using System.ComponentModel;
using System.Globalization;

namespace System.Configuration;

/// <summary>Converts a time span expressed in minutes. </summary>
public class TimeSpanMinutesConverter : ConfigurationConverterBase
{
	/// <summary>Converts a <see cref="T:System.String" /> to a <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>The <see cref="T:System.TimeSpan" /> representing the <paramref name="data" /> parameter in minutes.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object used during conversion.</param>
	/// <param name="data">The <see cref="T:System.String" /> object to convert.</param>
	public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
	{
		return TimeSpan.FromMinutes(long.Parse((string)data));
	}

	/// <summary>Converts a <see cref="T:System.TimeSpan" /> to a <see cref="T:System.String" />. </summary>
	/// <returns>The <see cref="T:System.String" /> representing the <paramref name="value" /> parameter in minutes.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object used during conversion.</param>
	/// <param name="value">The value to convert to.</param>
	/// <param name="type">The type to convert to.</param>
	public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
	{
		if (value.GetType() != typeof(TimeSpan))
		{
			throw new ArgumentException();
		}
		return ((long)((TimeSpan)value).TotalMinutes).ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.TimeSpanMinutesConverter" /> class.</summary>
	public TimeSpanMinutesConverter()
	{
	}
}
