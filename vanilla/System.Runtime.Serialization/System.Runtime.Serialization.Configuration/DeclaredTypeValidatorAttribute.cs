using System.Configuration;

namespace System.Runtime.Serialization.Configuration;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class DeclaredTypeValidatorAttribute : ConfigurationValidatorAttribute
{
	public override ConfigurationValidatorBase ValidatorInstance => new DeclaredTypeValidator();
}
