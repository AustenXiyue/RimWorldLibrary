namespace System.Diagnostics.Contracts;

/// <summary>Specifies that a field can be used in method contracts when the field has less visibility than the method. </summary>
[AttributeUsage(AttributeTargets.Field)]
[Conditional("CONTRACTS_FULL")]
public sealed class ContractPublicPropertyNameAttribute : Attribute
{
	private string _publicName;

	/// <summary>Gets the property name to be applied to the field.</summary>
	/// <returns>The property name to be applied to the field.</returns>
	public string Name => _publicName;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractPublicPropertyNameAttribute" /> class. </summary>
	/// <param name="name">The property name to apply to the field.</param>
	public ContractPublicPropertyNameAttribute(string name)
	{
		_publicName = name;
	}
}
