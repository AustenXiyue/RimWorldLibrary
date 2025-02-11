namespace System.Diagnostics.Contracts;

/// <summary>Marks a method as being the invariant method for a class.</summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ContractInvariantMethodAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractInvariantMethodAttribute" /> class. </summary>
	public ContractInvariantMethodAttribute()
	{
	}
}
