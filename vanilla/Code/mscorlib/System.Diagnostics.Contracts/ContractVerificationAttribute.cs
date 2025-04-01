namespace System.Diagnostics.Contracts;

/// <summary>Instructs analysis tools to assume the correctness of an assembly, type, or member without performing static verification.</summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
[Conditional("CONTRACTS_FULL")]
public sealed class ContractVerificationAttribute : Attribute
{
	private bool _value;

	/// <summary>Gets the value that indicates whether to verify the contract of the target. </summary>
	/// <returns>true if verification is required; otherwise, false.</returns>
	public bool Value => _value;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractVerificationAttribute" /> class. </summary>
	/// <param name="value">true to require verification; otherwise, false. </param>
	public ContractVerificationAttribute(bool value)
	{
		_value = value;
	}
}
