namespace System.Diagnostics.Contracts;

/// <summary>Specifies that a class is a contract for a type.</summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ContractClassForAttribute : Attribute
{
	private Type _typeIAmAContractFor;

	/// <summary>Gets the type that this code contract applies to. </summary>
	/// <returns>The type that this contract applies to.</returns>
	public Type TypeContractsAreFor => _typeIAmAContractFor;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractClassForAttribute" /> class, specifying the type the current class is a contract for. </summary>
	/// <param name="typeContractsAreFor">The type the current class is a contract for.</param>
	public ContractClassForAttribute(Type typeContractsAreFor)
	{
		_typeIAmAContractFor = typeContractsAreFor;
	}
}
