using System.ComponentModel;
using System.Globalization;

namespace System.Configuration;

/// <summary>Converts a time span expressed in seconds. </summary>
public class TimeSpanSecondsConverter : ConfigurationConverterBase
{
	/// <summary>Converts a <see cref="T:System.String" /> to a <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>The <see cref="T:System.TimeSpan" /> representing the <paramref name="data" /> parameter in seconds.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object used during conversion.</param>
	/// <param name="data">The <see cref="T:System.String" /> object to convert.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="data" /> cannot be parsed as an integer value.</exception>
	public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
	{
		if (!(data is string))
		{
			throw new ArgumentException("data");
		}
		if (!long.TryParse((string)data, out var result))
		{
			throw new ArgumentException("data");
		}
		return TimeSpan.FromSeconds(result);
	}

	/// <summary>Converts a <see cref="T:System.TimeSpan" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>The <see cref="T:System.String" /> that represents the <paramref name="value" /> parameter in minutes.</returns>
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
		return ((long)((TimeSpan)value).TotalSeconds).ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.TimeSpanSecondsConverter" /> class.</summary>
	public TimeSpanSecondsConverter()
	{
	}
}
