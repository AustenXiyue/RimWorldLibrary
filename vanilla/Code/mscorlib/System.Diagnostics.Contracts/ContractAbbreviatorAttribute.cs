namespace System.Diagnostics.Contracts;

/// <summary>Defines abbreviations that you can use in place of the full contract syntax.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[Conditional("CONTRACTS_FULL")]
public sealed class ContractAbbreviatorAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractAbbreviatorAttribute" /> class.</summary>
	public ContractAbbreviatorAttribute()
	{
	}
}
