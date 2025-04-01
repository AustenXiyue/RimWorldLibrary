namespace System.Configuration;

/// <summary>Provides validation of an object. This class cannot be inherited.</summary>
public sealed class DefaultValidator : ConfigurationValidatorBase
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.DefaultValidator" /> class. </summary>
	public DefaultValidator()
	{
	}

	/// <summary>Determines whether an object can be validated, based on type.</summary>
	/// <returns>true for all types being validated. </returns>
	/// <param name="type">The object type.</param>
	public override bool CanValidate(Type type)
	{
		return true;
	}

	/// <summary>Determines whether the value of an object is valid. </summary>
	/// <param name="value">The object value.</param>
	public override void Validate(object value)
	{
	}
}
