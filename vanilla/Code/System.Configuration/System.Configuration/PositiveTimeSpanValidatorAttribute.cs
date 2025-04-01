namespace System.Configuration;

/// <summary>Declaratively instructs the .NET Framework to perform time validation on a configuration property. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PositiveTimeSpanValidatorAttribute : ConfigurationValidatorAttribute
{
	private ConfigurationValidatorBase instance;

	/// <summary>Gets an instance of the <see cref="T:System.Configuration.PositiveTimeSpanValidator" /> class.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationValidatorBase" /> validator instance. </returns>
	public override ConfigurationValidatorBase ValidatorInstance
	{
		get
		{
			if (instance == null)
			{
				instance = new PositiveTimeSpanValidator();
			}
			return instance;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.PositiveTimeSpanValidatorAttribute" /> class. </summary>
	public PositiveTimeSpanValidatorAttribute()
	{
	}
}
