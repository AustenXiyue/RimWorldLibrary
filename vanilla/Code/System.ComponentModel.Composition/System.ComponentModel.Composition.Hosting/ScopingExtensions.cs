using System.ComponentModel.Composition.Primitives;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Defines static convenience methods for scoping.</summary>
public static class ScopingExtensions
{
	/// <summary>Gets a value that indicates whether the specified part exports the specified contract.</summary>
	/// <returns>true if <paramref name="part" /> exports the specified contract; otherwise, false.</returns>
	/// <param name="part">The part to search.</param>
	/// <param name="contractName">The name of the contract.</param>
	public static bool Exports(this ComposablePartDefinition part, string contractName)
	{
		Requires.NotNull(part, "part");
		Requires.NotNull(contractName, "contractName");
		foreach (ExportDefinition exportDefinition in part.ExportDefinitions)
		{
			if (StringComparers.ContractName.Equals(contractName, exportDefinition.ContractName))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Determines whether the specified part imports the specified contract.</summary>
	/// <returns>true if <paramref name="part" /> imports the specified contract; otherwise, false.</returns>
	/// <param name="part">The part to search.</param>
	/// <param name="contractName">The name of the contract.</param>
	public static bool Imports(this ComposablePartDefinition part, string contractName)
	{
		Requires.NotNull(part, "part");
		Requires.NotNull(contractName, "contractName");
		foreach (ImportDefinition importDefinition in part.ImportDefinitions)
		{
			if (StringComparers.ContractName.Equals(contractName, importDefinition.ContractName))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Determines whether the specified part imports the specified contract with the specified cardinality.</summary>
	/// <returns>true if <paramref name="part" /> imports a contract that has the specified name and cardinality; otherwise, false.</returns>
	/// <param name="part">The part to search.</param>
	/// <param name="contractName">The name of the contract.</param>
	/// <param name="importCardinality">The cardinality of the contract.</param>
	public static bool Imports(this ComposablePartDefinition part, string contractName, ImportCardinality importCardinality)
	{
		Requires.NotNull(part, "part");
		Requires.NotNull(contractName, "contractName");
		foreach (ImportDefinition importDefinition in part.ImportDefinitions)
		{
			if (StringComparers.ContractName.Equals(contractName, importDefinition.ContractName) && importDefinition.Cardinality == importCardinality)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Gets a value that indicates whether the specified part contains metadata that has the specified key.</summary>
	/// <returns>true if <paramref name="part" /> contains metadata that has the specified key; otherwise, false.</returns>
	/// <param name="part">The part to search.</param>
	/// <param name="key">The metadata key.</param>
	public static bool ContainsPartMetadataWithKey(this ComposablePartDefinition part, string key)
	{
		Requires.NotNull(part, "part");
		Requires.NotNull(key, "key");
		return part.Metadata.ContainsKey(key);
	}

	/// <summary>Gets a value that indicates whether the specified part contains metadata that has the specified key and value.</summary>
	/// <returns>true if <paramref name="part" /> contains metadata that has the specified key, value type, and value; otherwise, false.</returns>
	/// <param name="part">The part to search.</param>
	/// <param name="key">The metadata key.</param>
	/// <param name="value">The metadata value.</param>
	/// <typeparam name="T">The type of the metadata value.</typeparam>
	public static bool ContainsPartMetadata<T>(this ComposablePartDefinition part, string key, T value)
	{
		Requires.NotNull(part, "part");
		Requires.NotNull(key, "key");
		object value2 = null;
		if (part.Metadata.TryGetValue(key, out value2))
		{
			return value?.Equals(value2) ?? (value2 == null);
		}
		return false;
	}

	/// <summary>Filters the specified catalog with the specified filter function.</summary>
	/// <returns>A new catalog filtered by using the specified filter.</returns>
	/// <param name="catalog">The catalog to filter.</param>
	/// <param name="filter">The filter function.</param>
	public static FilteredCatalog Filter(this ComposablePartCatalog catalog, Func<ComposablePartDefinition, bool> filter)
	{
		Requires.NotNull(catalog, "catalog");
		Requires.NotNull(filter, "filter");
		return new FilteredCatalog(catalog, filter);
	}
}
