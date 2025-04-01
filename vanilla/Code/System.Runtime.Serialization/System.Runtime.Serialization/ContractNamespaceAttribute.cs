namespace System.Runtime.Serialization;

/// <summary>Specifies the CLR namespace and XML namespace of the data contract. </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, Inherited = false, AllowMultiple = true)]
public sealed class ContractNamespaceAttribute : Attribute
{
	private string clrNamespace;

	private string contractNamespace;

	/// <summary>Gets or sets the CLR namespace of the data contract type. </summary>
	/// <returns>The CLR-legal namespace of a type.</returns>
	public string ClrNamespace
	{
		get
		{
			return clrNamespace;
		}
		set
		{
			clrNamespace = value;
		}
	}

	/// <summary>Gets the namespace of the data contract members.</summary>
	/// <returns>The namespace of the data contract members.</returns>
	public string ContractNamespace => contractNamespace;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.ContractNamespaceAttribute" /> class using the supplied namespace. </summary>
	/// <param name="contractNamespace">The namespace of the contract.</param>
	public ContractNamespaceAttribute(string contractNamespace)
	{
		this.contractNamespace = contractNamespace;
	}
}
