using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition;

/// <summary>Specifies that a property, field, or parameter should be populated with all matching exports by the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class ImportManyAttribute : Attribute, IAttributedImport
{
	/// <summary>Gets the contract name of the exports to import.</summary>
	/// <returns>The contract name of the exports to import. The default value is an empty string ("").</returns>
	public string ContractName { get; private set; }

	/// <summary>Gets the contract type of the export to import.</summary>
	/// <returns>The type of the export that this import is expecting. The default value is null, which means that the type will be obtained by looking at the type on the member that this import is attached to. If the type is <see cref="T:System.Object" />, the import will match any exported type.</returns>
	public Type ContractType { get; private set; }

	/// <summary>Gets or sets a value indicating whether the decorated property or field will be recomposed when exports that provide the matching contract change.</summary>
	/// <returns>true if the property or field allows for recomposition when exports that provide the same <see cref="P:System.ComponentModel.Composition.ImportManyAttribute.ContractName" /> are added or removed from the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />; otherwise, false.The default value is false.</returns>
	public bool AllowRecomposition { get; set; }

	/// <summary>Gets or sets a value that indicates that the importer requires a specific <see cref="T:System.ComponentModel.Composition.CreationPolicy" /> for the exports used to satisfy this import. </summary>
	/// <returns>One of the following values:<see cref="F:System.ComponentModel.Composition.CreationPolicy.Any" />, if the importer does not require a specific <see cref="T:System.ComponentModel.Composition.CreationPolicy" />. This is the default.<see cref="F:System.ComponentModel.Composition.CreationPolicy.Shared" /> to require that all used exports be shared by all parts in the container.<see cref="F:System.ComponentModel.Composition.CreationPolicy.NonShared" /> to require that all used exports be non-shared in a container. In this case, each part receives their own instance.</returns>
	public CreationPolicy RequiredCreationPolicy { get; set; }

	/// <summary>Gets or sets a value that specifies the scopes from which this import may be satisfied.</summary>
	/// <returns>A value that specifies the scopes from which this import may be satisfied.</returns>
	public ImportSource Source { get; set; }

	ImportCardinality IAttributedImport.Cardinality => ImportCardinality.ZeroOrMore;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ImportManyAttribute" /> class, importing the set of exports with the default contract name.</summary>
	public ImportManyAttribute()
		: this((string)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ImportManyAttribute" /> class, importing the set of exports with the contract name derived from the specified type.</summary>
	/// <param name="contractType">The type to derive the contract name of the exports to import, or null to use the default contract name.</param>
	public ImportManyAttribute(Type contractType)
		: this(null, contractType)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ImportManyAttribute" /> class, importing the set of exports with the specified contract name.</summary>
	/// <param name="contractName">The contract name of the exports to import, or null or an empty string ("") to use the default contract name.</param>
	public ImportManyAttribute(string contractName)
		: this(contractName, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ImportManyAttribute" /> class, importing the set of exports with the specified contract name and contract type.</summary>
	/// <param name="contractName">The contract name of the exports to import, or null or an empty string ("") to use the default contract name.</param>
	/// <param name="contractType">The type of the export to import.</param>
	public ImportManyAttribute(string contractName, Type contractType)
	{
		ContractName = contractName;
		ContractType = contractType;
	}
}
