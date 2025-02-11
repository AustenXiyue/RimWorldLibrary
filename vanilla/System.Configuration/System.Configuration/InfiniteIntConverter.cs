using System.ComponentModel;
using System.Globalization;

namespace System.Configuration;

/// <summary>Converts between a string and the standard infinite or integer value.</summary>
public sealed class InfiniteIntConverter : ConfigurationConverterBase
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.InfiniteIntConverter" /> class.</summary>
	public InfiniteIntConverter()
	{
	}

	/// <summary>Converts a <see cref="T:System.String" /> to an <see cref="T:System.Int32" />.</summary>
	/// <returns>The <see cref="F:System.Int32.MaxValue" />, if the <paramref name="data" /> parameter is the <see cref="T:System.String" /> "infinite"; otherwise, the <see cref="T:System.Int32" /> representing the <paramref name="data" /> parameter integer value.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object used during conversion.</param>
	/// <param name="data">The <see cref="T:System.String" /> object to convert.</param>
	public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
	{
		if ((string)data == "Infinite")
		{
			return int.MaxValue;
		}
		return Convert.ToInt32((string)data, 10);
	}

	/// <summary>Converts an <see cref="T:System.Int32" />.to a <see cref="T:System.String" />.</summary>
	/// <returns>The <see cref="T:System.String" /> "infinite" if the <paramref name="value" /> is <see cref="F:System.Int32.MaxValue" />; otherwise, the <see cref="T:System.String" /> representing the <paramref name="value" /> parameter.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object used during conversion.</param>
	/// <param name="value">The value to convert to.</param>
	/// <param name="type">The type to convert to.</param>
	public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
	{
		if (value.GetType() != typeof(int))
		{
			throw new ArgumentException();
		}
		if ((int)value == int.MaxValue)
		{
			return "Infinite";
		}
		return value.ToString();
	}
}
