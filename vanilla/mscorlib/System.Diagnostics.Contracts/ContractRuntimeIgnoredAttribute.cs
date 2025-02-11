namespace System.Diagnostics.Contracts;

/// <summary>Identifies a member that has no run-time behavior.</summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ContractRuntimeIgnoredAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractRuntimeIgnoredAttribute" /> class. </summary>
	public ContractRuntimeIgnoredAttribute()
	{
	}
}
