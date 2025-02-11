using System.ComponentModel;
using System.Globalization;

namespace System.Configuration;

/// <summary>Converts between a string and an enumeration type. </summary>
public sealed class GenericEnumConverter : ConfigurationConverterBase
{
	private Type typeEnum;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.GenericEnumConverter" /> class.</summary>
	/// <param name="typeEnum">The enumeration type to convert.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="typeEnum" /> is null.</exception>
	public GenericEnumConverter(Type typeEnum)
	{
		if (typeEnum == null)
		{
			throw new ArgumentNullException("typeEnum");
		}
		this.typeEnum = typeEnum;
	}

	/// <summary>Converts a <see cref="T:System.String" /> to an <see cref="T:System.Enum" /> type.</summary>
	/// <returns>The <see cref="T:System.Enum" /> type that represents the <paramref name="data" /> parameter.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object used during conversion.</param>
	/// <param name="data">The <see cref="T:System.String" /> object to convert.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="data" /> is null or an empty string ("").- or -<paramref name="data" /> starts with a numeric character.- or -<paramref name="data" /> includes white space.</exception>
	public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
	{
		if (data == null)
		{
			throw new ArgumentException();
		}
		return Enum.Parse(typeEnum, (string)data);
	}

	/// <summary>Converts an <see cref="T:System.Enum" /> type to a <see cref="T:System.String" /> value.</summary>
	/// <returns>The <see cref="T:System.String" /> that represents the <paramref name="value" /> parameter.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object used during conversion.</param>
	/// <param name="value">The value to convert to.</param>
	/// <param name="type">The type to convert to.</param>
	public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
	{
		return value.ToString();
	}
}
