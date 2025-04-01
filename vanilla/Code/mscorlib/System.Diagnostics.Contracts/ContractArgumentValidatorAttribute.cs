namespace System.Diagnostics.Contracts;

/// <summary>Enables the factoring of legacy if-then-throw code into separate methods for reuse, and provides full control over thrown exceptions and arguments.</summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ContractArgumentValidatorAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractArgumentValidatorAttribute" /> class.</summary>
	public ContractArgumentValidatorAttribute()
	{
	}
}
