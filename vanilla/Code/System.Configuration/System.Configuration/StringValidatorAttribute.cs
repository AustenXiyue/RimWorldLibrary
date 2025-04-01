namespace System.Configuration;

/// <summary>Declaratively instructs the .NET Framework to perform string validation on a configuration property. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class StringValidatorAttribute : ConfigurationValidatorAttribute
{
	private string invalidCharacters;

	private int maxLength = int.MaxValue;

	private int minLength;

	private ConfigurationValidatorBase instance;

	/// <summary>Gets or sets the invalid characters for the property.</summary>
	/// <returns>The string that contains the set of characters that are not allowed for the property.</returns>
	public string InvalidCharacters
	{
		get
		{
			return invalidCharacters;
		}
		set
		{
			invalidCharacters = value;
			instance = null;
		}
	}

	/// <summary>Gets or sets the maximum length allowed for the string to assign to the property.</summary>
	/// <returns>An integer that indicates the maximum allowed length for the string to assign to the property.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The selected value is less than <see cref="P:System.Configuration.StringValidatorAttribute.MinLength" />.</exception>
	public int MaxLength
	{
		get
		{
			return maxLength;
		}
		set
		{
			maxLength = value;
			instance = null;
		}
	}

	/// <summary>Gets or sets the minimum allowed value for the string to assign to the property.</summary>
	/// <returns>An integer that indicates the allowed minimum length for the string to assign to the property.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The selected value is greater than <see cref="P:System.Configuration.StringValidatorAttribute.MaxLength" />.</exception>
	public int MinLength
	{
		get
		{
			return minLength;
		}
		set
		{
			minLength = value;
			instance = null;
		}
	}

	/// <summary>Gets an instance of the <see cref="T:System.Configuration.StringValidator" /> class.</summary>
	/// <returns>A current <see cref="T:System.Configuration.StringValidator" /> settings in a <see cref="T:System.Configuration.ConfigurationValidatorBase" /> validator instance.</returns>
	public override ConfigurationValidatorBase ValidatorInstance
	{
		get
		{
			if (instance == null)
			{
				instance = new StringValidator(minLength, maxLength, invalidCharacters);
			}
			return instance;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.StringValidatorAttribute" /> class.</summary>
	public StringValidatorAttribute()
	{
	}
}
