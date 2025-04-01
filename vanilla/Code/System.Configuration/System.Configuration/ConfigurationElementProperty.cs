namespace System.Configuration;

/// <summary>Specifies the property of a configuration element. This class cannot be inherited.</summary>
public sealed class ConfigurationElementProperty
{
	private ConfigurationValidatorBase validator;

	/// <summary>Gets a <see cref="T:System.Configuration.ConfigurationValidatorBase" /> object used to validate the <see cref="T:System.Configuration.ConfigurationElementProperty" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConfigurationValidatorBase" /> object.</returns>
	public ConfigurationValidatorBase Validator => validator;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationElementProperty" /> class, based on a supplied parameter.</summary>
	/// <param name="validator">A <see cref="T:System.Configuration.ConfigurationValidatorBase" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="validator" /> is null.</exception>
	public ConfigurationElementProperty(ConfigurationValidatorBase validator)
	{
		this.validator = validator;
	}
}
