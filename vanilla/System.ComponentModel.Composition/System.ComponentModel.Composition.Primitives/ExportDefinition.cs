using System.Collections.Generic;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Primitives;

/// <summary>Describes the contract that a particular <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> object satisfies.</summary>
public class ExportDefinition
{
	private readonly IDictionary<string, object> _metadata = MetadataServices.EmptyMetadata;

	private readonly string _contractName;

	/// <summary>Gets the contract name.</summary>
	/// <returns>The contract name of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> object.</returns>
	/// <exception cref="T:System.NotImplementedException">The property was not overridden by a derived class.</exception>
	public virtual string ContractName
	{
		get
		{
			if (_contractName != null)
			{
				return _contractName;
			}
			throw ExceptionBuilder.CreateNotOverriddenByDerived("ContractName");
		}
	}

	/// <summary>Gets the contract metadata.</summary>
	/// <returns>The metadata of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" />. The default is an empty, read-only <see cref="T:System.Collections.Generic.IDictionary`2" /> object.</returns>
	public virtual IDictionary<string, object> Metadata => _metadata;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> class.</summary>
	protected ExportDefinition()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> class with the specified contract name and metadata.</summary>
	/// <param name="contractName">The contract name of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> object.</param>
	/// <param name="metadata">The metadata of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> or null to set the <see cref="P:System.ComponentModel.Composition.Primitives.ExportDefinition.Metadata" /> property to an empty, read-only <see cref="T:System.Collections.Generic.IDictionary`2" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="contractName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="contractName" /> is an empty string ("").</exception>
	public ExportDefinition(string contractName, IDictionary<string, object> metadata)
	{
		Requires.NotNullOrEmpty(contractName, "contractName");
		_contractName = contractName;
		if (metadata != null)
		{
			_metadata = metadata.AsReadOnly();
		}
	}

	/// <summary>Returns a string representation of the export definition.</summary>
	/// <returns>A string representation of the export definition.</returns>
	public override string ToString()
	{
		return ContractName;
	}
}
