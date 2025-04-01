using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition;

/// <summary>Specifies that a property, field, or parameter value should be provided by the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.object</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class ImportAttribute : Attribute, IAttributedImport
{
	/// <summary>Gets the contract name of the export to import.</summary>
	/// <returns>The contract name of the export to import. The default is an empty string ("").</returns>
	public string ContractName { get; private set; }

	/// <summary>Gets the type of the export to import.</summary>
	/// <returns>The type of the export to import.</returns>
	public Type ContractType { get; private set; }

	/// <summary>Gets or sets a value that indicates whether the property, field, or parameter will be set to its type's default value when an export with the contract name is not present in the container.</summary>
	/// <returns>true if the property, field, or parameter will be set to its type's default value when there is no export with the <see cref="P:System.ComponentModel.Composition.ImportAttribute.ContractName" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />; otherwise, false. The default is false.</returns>
	public bool AllowDefault { get; set; }

	/// <summary>Gets or sets a value that indicates whether the property or field will be recomposed when exports with a matching contract have changed in the container.</summary>
	/// <returns>true if the property or field allows recomposition when exports with a matching <see cref="P:System.ComponentModel.Composition.ImportAttribute.ContractName" /> are added or removed from the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />; otherwise, false. The default is false.</returns>
	public bool AllowRecomposition { get; set; }

	/// <summary>Gets or sets a value that indicates that the importer requires a specific <see cref="T:System.ComponentModel.Composition.CreationPolicy" /> for the exports used to satisfy this import. </summary>
	/// <returns>One of the following values:<see cref="F:System.ComponentModel.Composition.CreationPolicy.Any" />, if the importer does not require a specific <see cref="T:System.ComponentModel.Composition.CreationPolicy" />. This is the default.<see cref="F:System.ComponentModel.Composition.CreationPolicy.Shared" /> to require that all used exports be shared by all parts in the container.<see cref="F:System.ComponentModel.Composition.CreationPolicy.NonShared" /> to require that all used exports be non-shared in a container. In this case, each part receives their own instance.</returns>
	public CreationPolicy RequiredCreationPolicy { get; set; }

	/// <summary>Gets or sets a value that specifies the scopes from which this import may be satisfied.</summary>
	/// <returns>A value that specifies the scopes from which this import may be satisfied.</returns>
	public ImportSource Source { get; set; }

	ImportCardinality IAttributedImport.Cardinality
	{
		get
		{
			if (AllowDefault)
			{
				return ImportCardinality.ZeroOrOne;
			}
			return ImportCardinality.ExactlyOne;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ImportAttribute" /> class, importing the export with the default contract name.</summary>
	public ImportAttribute()
		: this((string)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ImportAttribute" /> class, importing the export with the contract name derived from the specified type.</summary>
	/// <param name="contractType">The type to derive the contract name of the export from, or null to use the default contract name.</param>
	public ImportAttribute(Type contractType)
		: this(null, contractType)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ImportAttribute" /> class, importing the export with the specified contract name.</summary>
	/// <param name="contractName">The contract name of the export to import, or null or an empty string ("") to use the default contract name.</param>
	public ImportAttribute(string contractName)
		: this(contractName, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ImportAttribute" /> class, importing the export with the specified contract name and type.</summary>
	/// <param name="contractName">The contract name of the export to import, or null or an empty string ("") to use the default contract name.</param>
	/// <param name="contractType">The type of the export to import.</param>
	public ImportAttribute(string contractName, Type contractType)
	{
		ContractName = contractName;
		ContractType = contractType;
	}
}
