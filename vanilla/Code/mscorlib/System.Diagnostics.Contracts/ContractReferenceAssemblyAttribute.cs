namespace System.Diagnostics.Contracts;

/// <summary>Specifies that an assembly is a reference assembly that contains contracts.</summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ContractReferenceAssemblyAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractReferenceAssemblyAttribute" /> class. </summary>
	public ContractReferenceAssemblyAttribute()
	{
	}
}
