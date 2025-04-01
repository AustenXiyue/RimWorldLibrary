using System.ComponentModel;

namespace System.Configuration;

/// <summary>The base class for the configuration converter types.</summary>
public abstract class ConfigurationConverterBase : TypeConverter
{
	/// <summary>Determines whether the conversion is allowed.</summary>
	/// <returns>true if the conversion is allowed; otherwise, false.</returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversions.</param>
	/// <param name="type">The <see cref="T:System.Type" /> to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext ctx, Type type)
	{
		if (type == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(ctx, type);
	}

	/// <summary>Determines whether the conversion is allowed.</summary>
	/// <returns>true if the conversion is allowed; otherwise, false. </returns>
	/// <param name="ctx">The <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> object used for type conversion.</param>
	/// <param name="type">The type to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext ctx, Type type)
	{
		if (type == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(ctx, type);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationConverterBase" /> class.</summary>
	protected ConfigurationConverterBase()
	{
	}
}
