namespace System.Configuration;

/// <summary>Declaratively instructs the .NET Framework to perform validation on a configuration property. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SubclassTypeValidatorAttribute : ConfigurationValidatorAttribute
{
	private Type baseClass;

	private ConfigurationValidatorBase instance;

	/// <summary>Gets the base type of the object being validated.</summary>
	/// <returns>The base type of the object being validated.</returns>
	public Type BaseClass => baseClass;

	/// <summary>Gets the validator attribute instance.</summary>
	/// <returns>The current <see cref="T:System.Configuration.ConfigurationValidatorBase" /> instance.</returns>
	public override ConfigurationValidatorBase ValidatorInstance
	{
		get
		{
			if (instance == null)
			{
				instance = new SubclassTypeValidator(baseClass);
			}
			return instance;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SubclassTypeValidatorAttribute" /> class. </summary>
	/// <param name="baseClass">The base class type.</param>
	public SubclassTypeValidatorAttribute(Type baseClass)
	{
		this.baseClass = baseClass;
	}
}
