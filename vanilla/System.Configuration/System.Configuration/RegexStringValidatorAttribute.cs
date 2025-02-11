namespace System.Configuration;

/// <summary>Declaratively instructs the .NET Framework to perform string validation on a configuration property using a regular expression. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RegexStringValidatorAttribute : ConfigurationValidatorAttribute
{
	private string regex;

	private ConfigurationValidatorBase instance;

	/// <summary>Gets the string used to perform regular-expression validation.</summary>
	/// <returns>The string containing the regular expression used to filter the string assigned to the decorated configuration-element property.</returns>
	public string Regex => regex;

	/// <summary>Gets an instance of the <see cref="T:System.Configuration.RegexStringValidator" /> class.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationValidatorBase" /> validator instance.</returns>
	public override ConfigurationValidatorBase ValidatorInstance
	{
		get
		{
			if (instance == null)
			{
				instance = new RegexStringValidator(regex);
			}
			return instance;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.RegexStringValidatorAttribute" /> object.</summary>
	/// <param name="regex">The string to use for regular expression validation.</param>
	public RegexStringValidatorAttribute(string regex)
	{
		this.regex = regex;
	}
}
