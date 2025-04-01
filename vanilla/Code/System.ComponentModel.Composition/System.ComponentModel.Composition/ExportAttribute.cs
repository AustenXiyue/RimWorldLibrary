namespace System.ComponentModel.Composition;

/// <summary>Specifies that a type, property, field, or method provides a particular export.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public class ExportAttribute : Attribute
{
	/// <summary>Gets the contract name that is used to export the type or member marked with this attribute.</summary>
	/// <returns>The contract name that is used to export the type or member marked with this attribute. The default value is an empty string ("").</returns>
	public string ContractName { get; private set; }

	/// <summary>Gets the contract type that is exported by the member that this attribute is attached to.</summary>
	/// <returns>The type of export that is be provided. The default value is null, which means that the type will be obtained by looking at the type on the member that this export is attached to. </returns>
	public Type ContractType { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ExportAttribute" /> class, exporting the type or member marked with this attribute under the default contract name.</summary>
	public ExportAttribute()
		: this(null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ExportAttribute" /> class, exporting the type or member marked with this attribute under a contract name derived from the specified type.</summary>
	/// <param name="contractType">A type from which to derive the contract name that is used to export the type or member marked with this attribute, or null to use the default contract name.</param>
	public ExportAttribute(Type contractType)
		: this(null, contractType)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ExportAttribute" /> class, exporting the type or member marked with this attribute under the specified contract name.</summary>
	/// <param name="contractName">The contract name that is used to export the type or member marked with this attribute, or null or an empty string ("") to use the default contract name.</param>
	public ExportAttribute(string contractName)
		: this(contractName, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ExportAttribute" /> class, exporting the specified type under the specified contract name.</summary>
	/// <param name="contractName">The contract name that is used to export the type or member marked with this attribute, or null or an empty string ("") to use the default contract name.</param>
	/// <param name="contractType">The type to export.</param>
	public ExportAttribute(string contractName, Type contractType)
	{
		ContractName = contractName;
		ContractType = contractType;
	}
}
