namespace System.Configuration;

/// <summary>Serves as the base class for the <see cref="N:System.Configuration" /> validator attribute types.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class ConfigurationValidatorAttribute : Attribute
{
	private Type validatorType;

	private ConfigurationValidatorBase instance;

	/// <summary>Gets the validator attribute instance.</summary>
	/// <returns>The current <see cref="T:System.Configuration.ConfigurationValidatorBase" />.</returns>
	public virtual ConfigurationValidatorBase ValidatorInstance
	{
		get
		{
			if (instance == null)
			{
				instance = (ConfigurationValidatorBase)Activator.CreateInstance(validatorType);
			}
			return instance;
		}
	}

	/// <summary>Gets the type of the validator attribute.</summary>
	/// <returns>The <see cref="T:System.Type" /> of the current validator attribute instance.</returns>
	public Type ValidatorType => validatorType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationValidatorAttribute" /> class.</summary>
	protected ConfigurationValidatorAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationValidatorAttribute" /> class using the specified validator type.</summary>
	/// <param name="validator">The validator type to use when creating an instance of <see cref="T:System.Configuration.ConfigurationValidatorAttribute" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="validator" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="validator" /> is not derived from <see cref="T:System.Configuration.ConfigurationValidatorBase" />.</exception>
	public ConfigurationValidatorAttribute(Type validator)
	{
		validatorType = validator;
	}
}
