using System.ComponentModel;
using System.Globalization;

namespace System.Configuration;

/// <summary>Converts between type and string values. This class cannot be inherited.</summary>
public sealed class TypeNameConverter : ConfigurationConverterBase
{
	/// <summary>Converts a <see cref="T:System.String" /> object to a <see cref="T:System.Type" /> object.</summary>
	/// <returns>The <see cref="T:System.Type" /> that represents the <paramref name="data" /> parameter.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object used during conversion.</param>
	/// <param name="data">The <see cref="T:System.String" /> object to convert.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Type" /> value cannot be resolved.</exception>
	public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
	{
		return Type.GetType((string)data);
	}

	/// <summary>Converts a <see cref="T:System.Type" /> object to a <see cref="T:System.String" /> object.</summary>
	/// <returns>The <see cref="T:System.String" /> that represents the <paramref name="value" /> parameter. </returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object used during conversion.</param>
	/// <param name="value">The value to convert to.</param>
	/// <param name="type">The type to convert to.</param>
	public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
	{
		if (value == null)
		{
			return null;
		}
		if (!(value is Type))
		{
			throw new ArgumentException("value");
		}
		return ((Type)value).AssemblyQualifiedName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.TypeNameConverter" /> class.</summary>
	public TypeNameConverter()
	{
	}
}
