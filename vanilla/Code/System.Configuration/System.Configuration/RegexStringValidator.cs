using System.Text.RegularExpressions;

namespace System.Configuration;

/// <summary>Provides validation of a string based on the rules provided by a regular expression.</summary>
public class RegexStringValidator : ConfigurationValidatorBase
{
	private string regex;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.RegexStringValidator" /> class. </summary>
	/// <param name="regex">A string that specifies a regular expression.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="regex" /> is null or an empty string ("").</exception>
	public RegexStringValidator(string regex)
	{
		this.regex = regex;
	}

	/// <summary>Determines whether the type of the object can be validated.</summary>
	/// <returns>true if the <paramref name="type" /> parameter matches a string; otherwise, false. </returns>
	/// <param name="type">The type of object.</param>
	public override bool CanValidate(Type type)
	{
		return type == typeof(string);
	}

	/// <summary>Determines whether the value of an object is valid.</summary>
	/// <param name="value">The value of an object.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> does not conform to the parameters of the <see cref="T:System.Text.RegularExpressions.Regex" /> class.</exception>
	public override void Validate(object value)
	{
		if (!Regex.IsMatch((string)value, regex))
		{
			throw new ArgumentException("The string must match the regexp `{0}'", regex);
		}
	}
}
