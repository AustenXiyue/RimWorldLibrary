namespace System.Diagnostics.Contracts;

/// <summary>Specifies that a separate type contains the code contracts for this type.</summary>
[Conditional("CONTRACTS_FULL")]
[Conditional("DEBUG")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
public sealed class ContractClassAttribute : Attribute
{
	private Type _typeWithContracts;

	/// <summary>Gets the type that contains the code contracts for this type.</summary>
	/// <returns>The type that contains the code contracts for this type. </returns>
	public Type TypeContainingContracts => _typeWithContracts;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractClassAttribute" /> class. </summary>
	/// <param name="typeContainingContracts">The type that contains the code contracts for this type.</param>
	public ContractClassAttribute(Type typeContainingContracts)
	{
		_typeWithContracts = typeContainingContracts;
	}
}
