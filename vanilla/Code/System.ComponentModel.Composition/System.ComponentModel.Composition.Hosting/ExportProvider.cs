using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Linq;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Retrieves exports which match a specified <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> object.</summary>
public abstract class ExportProvider
{
	private static readonly Export[] EmptyExports = new Export[0];

	/// <summary>Occurs when the exports in the <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> change.</summary>
	public event EventHandler<ExportsChangeEventArgs> ExportsChanged;

	/// <summary>Occurs when the provided exports are changing.</summary>
	public event EventHandler<ExportsChangeEventArgs> ExportsChanging;

	/// <summary>Returns the export with the contract name derived from the specified type parameter. If there is not exactly one matching export, an exception is thrown.</summary>
	/// <returns>The export with the contract name derived from the specified type parameter.</returns>
	/// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`1" /> object to return. The contract name is also derived from this type parameter.</typeparam>
	/// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero <see cref="T:System.Lazy`1" /> objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.-or-There is more than one <see cref="T:System.Lazy`1" /> object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	public Lazy<T> GetExport<T>()
	{
		return GetExport<T>(null);
	}

	/// <summary>Returns the export with the specified contract name. If there is not exactly one matching export, an exception is thrown.</summary>
	/// <returns>The export with the specified contract name.</returns>
	/// <param name="contractName">The contract name of the <see cref="T:System.Lazy`1" /> object to return, or null or an empty string ("") to use the default contract name.</param>
	/// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`1" /> object to return.</typeparam>
	/// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero <see cref="T:System.Lazy`1" /> objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.-or-There is more than one <see cref="T:System.Lazy`1" /> object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	public Lazy<T> GetExport<T>(string contractName)
	{
		return GetExportCore<T>(contractName);
	}

	/// <summary>Returns the export with the contract name derived from the specified type parameter. If there is not exactly one matching export, an exception is thrown.</summary>
	/// <returns>System.Lazy`2</returns>
	/// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`2" /> object to return. The contract name is also derived from this type parameter.</typeparam>
	/// <typeparam name="TMetadataView">The type of the metadata view of the <see cref="T:System.Lazy`2" /> object to return.</typeparam>
	/// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero <see cref="T:System.Lazy`2" /> objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.-or-There is more than one <see cref="T:System.Lazy`2" /> object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="TMetadataView" /> is not a valid metadata view type.</exception>
	public Lazy<T, TMetadataView> GetExport<T, TMetadataView>()
	{
		return GetExport<T, TMetadataView>(null);
	}

	/// <summary>Returns the export with the specified contract name. If there is not exactly one matching export, an exception is thrown.</summary>
	/// <returns>The export with the specified contract name.</returns>
	/// <param name="contractName">The contract name of the <see cref="T:System.Lazy`2" /> object to return, or null or an empty string ("") to use the default contract name.</param>
	/// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`2" /> object to return.</typeparam>
	/// <typeparam name="TMetadataView">The type of the metadata view of the <see cref="T:System.Lazy`2" /> object to return.</typeparam>
	/// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero <see cref="T:System.Lazy`2" /> objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.-or-There is more than one <see cref="T:System.Lazy`2" /> object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="TMetadataView" /> is not a valid metadata view type.</exception>
	public Lazy<T, TMetadataView> GetExport<T, TMetadataView>(string contractName)
	{
		return GetExportCore<T, TMetadataView>(contractName);
	}

	/// <summary>Gets all the exports with the specified contract name.</summary>
	/// <returns>A collection of all the <see cref="T:System.Lazy`2" /> objects for the contract matching <paramref name="contractName" />.</returns>
	/// <param name="type">The type parameter of the <see cref="T:System.Lazy`2" /> objects to return.</param>
	/// <param name="metadataViewType">The type of the metadata view of the <see cref="T:System.Lazy`2" /> objects to return.</param>
	/// <param name="contractName">The contract name of the <see cref="T:System.Lazy`2" /> object to return, or null or an empty string ("") to use the default contract name.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="metadataViewType" /> is not a valid metadata view type.</exception>
	public IEnumerable<Lazy<object, object>> GetExports(Type type, Type metadataViewType, string contractName)
	{
		IEnumerable<Export> exportsCore = GetExportsCore(type, metadataViewType, contractName, ImportCardinality.ZeroOrMore);
		Collection<Lazy<object, object>> collection = new Collection<Lazy<object, object>>();
		Func<Export, Lazy<object, object>> func = ExportServices.CreateSemiStronglyTypedLazyFactory(type, metadataViewType);
		foreach (Export item in exportsCore)
		{
			collection.Add(func(item));
		}
		return collection;
	}

	/// <summary>Gets all the exports with the contract name derived from the specified type parameter.</summary>
	/// <returns>The <see cref="T:System.Lazy`1" /> objects with the contract name derived from <paramref name="T" />, if found; otherwise, an empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> object.</returns>
	/// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`1" /> objects to return. The contract name is also derived from this type parameter.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	public IEnumerable<Lazy<T>> GetExports<T>()
	{
		return GetExports<T>(null);
	}

	/// <summary>Gets all the exports with the specified contract name.</summary>
	/// <returns>The <see cref="T:System.Lazy`1" /> objects with the specified contract name, if found; otherwise, an empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> object.</returns>
	/// <param name="contractName">The contract name of the <see cref="T:System.Lazy`1" /> objects to return, or null or an empty string ("") to use the default contract name.</param>
	/// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`1" /> objects to return.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	public IEnumerable<Lazy<T>> GetExports<T>(string contractName)
	{
		return GetExportsCore<T>(contractName);
	}

	/// <summary>Gets all the exports with the contract name derived from the specified type parameter.</summary>
	/// <returns>The <see cref="T:System.Lazy`2" /> objects with the contract name derived from <paramref name="T" />, if found; otherwise, an empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> object.</returns>
	/// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`2" /> objects to return. The contract name is also derived from this type parameter.</typeparam>
	/// <typeparam name="TMetadataView">The type of the metadata view of the <see cref="T:System.Lazy`2" /> objects to return.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="TMetadataView" /> is not a valid metadata view type.</exception>
	public IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>()
	{
		return GetExports<T, TMetadataView>(null);
	}

	/// <summary>Gets all the exports with the specified contract name.</summary>
	/// <returns>The <see cref="T:System.Lazy`2" /> objects with the specified contract name if found; otherwise, an empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> object.</returns>
	/// <param name="contractName">The contract name of the <see cref="T:System.Lazy`2" /> objects to return, or null or an empty string ("") to use the default contract name.</param>
	/// <typeparam name="T">The type parameter of the <see cref="T:System.Lazy`2" /> objects to return. The contract name is also derived from this type parameter.</typeparam>
	/// <typeparam name="TMetadataView">The type of the metadata view of the <see cref="T:System.Lazy`2" /> objects to return.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="TMetadataView" /> is not a valid metadata view type.</exception>
	public IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>(string contractName)
	{
		return GetExportsCore<T, TMetadataView>(contractName);
	}

	/// <summary>Returns the exported object with the contract name derived from the specified type parameter. If there is not exactly one matching exported object, an exception is thrown.</summary>
	/// <returns>The exported object with the contract name derived from the specified type parameter.</returns>
	/// <typeparam name="T">The type of the exported object to return. The contract name is also derived from this type parameter.</typeparam>
	/// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero exported objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.-or-There is more than one exported object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">The underlying exported object cannot be cast to <paramref name="T" />.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
	public T GetExportedValue<T>()
	{
		return GetExportedValue<T>(null);
	}

	/// <summary>Returns the exported object with the specified contract name. If there is not exactly one matching exported object, an exception is thrown.</summary>
	/// <returns>The exported object with the specified contract name.</returns>
	/// <param name="contractName">The contract name of the exported object to return, or null or an empty string ("") to use the default contract name.</param>
	/// <typeparam name="T">The type of the exported object to return.</typeparam>
	/// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There are zero exported objects with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.-or-There is more than one exported object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">The underlying exported object cannot be cast to <paramref name="T" />.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
	public T GetExportedValue<T>(string contractName)
	{
		return GetExportedValueCore<T>(contractName, ImportCardinality.ExactlyOne);
	}

	/// <summary>Gets the exported object with the contract name derived from the specified type parameter or the default value for the specified type, or throws an exception if there is more than one matching exported object.</summary>
	/// <returns>The exported object with the contract name derived from <paramref name="T" />, if found; otherwise, the default value for <paramref name="T" />.</returns>
	/// <typeparam name="T">The type of the exported object to return. The contract name is also derived from this type parameter.</typeparam>
	/// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There is more than one exported object with the contract name derived from <paramref name="T" /> in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">The underlying exported object cannot be cast to <paramref name="T" />.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
	public T GetExportedValueOrDefault<T>()
	{
		return GetExportedValueOrDefault<T>(null);
	}

	/// <summary>Gets the exported object with the specified contract name or the default value for the specified type, or throws an exception if there is more than one matching exported object.</summary>
	/// <returns>The exported object with the specified contract name, if found; otherwise, the default value for <paramref name="T" />.</returns>
	/// <param name="contractName">The contract name of the exported object to return, or null or an empty string ("") to use the default contract name.</param>
	/// <typeparam name="T">The type of the exported object to return.</typeparam>
	/// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">There is more than one exported object with the specified contract name in the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">The underlying exported object cannot be cast to <paramref name="T" />.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
	public T GetExportedValueOrDefault<T>(string contractName)
	{
		return GetExportedValueCore<T>(contractName, ImportCardinality.ZeroOrOne);
	}

	/// <summary>Gets all the exported objects with the contract name derived from the specified type parameter.</summary>
	/// <returns>The exported objects with the contract name derived from the specified type parameter, if found; otherwise, an empty <see cref="T:System.Collections.ObjectModel.Collection`1" /> object.</returns>
	/// <typeparam name="T">The type of the exported object to return. The contract name is also derived from this type parameter.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">One or more of the underlying exported objects cannot be cast to <paramref name="T" />.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
	public IEnumerable<T> GetExportedValues<T>()
	{
		return GetExportedValues<T>(null);
	}

	/// <summary>Gets all the exported objects with the specified contract name.</summary>
	/// <returns>The exported objects with the specified contract name, if found; otherwise, an empty <see cref="T:System.Collections.ObjectModel.Collection`1" /> object.</returns>
	/// <param name="contractName">The contract name of the exported objects to return; or null or an empty string ("") to use the default contract name.</param>
	/// <typeparam name="T">The type of the exported object to return.</typeparam>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionContractMismatchException">One or more of the underlying exported values cannot be cast to <paramref name="T" />.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of errors that occurred.</exception>
	public IEnumerable<T> GetExportedValues<T>(string contractName)
	{
		return GetExportedValuesCore<T>(contractName);
	}

	private IEnumerable<T> GetExportedValuesCore<T>(string contractName)
	{
		IEnumerable<Export> exportsCore = GetExportsCore(typeof(T), null, contractName, ImportCardinality.ZeroOrMore);
		Collection<T> collection = new Collection<T>();
		foreach (Export item in exportsCore)
		{
			collection.Add(ExportServices.GetCastedExportedValue<T>(item));
		}
		return collection;
	}

	private T GetExportedValueCore<T>(string contractName, ImportCardinality cardinality)
	{
		Assumes.IsTrue(cardinality.IsAtMostOne());
		Export export = GetExportsCore(typeof(T), null, contractName, cardinality).SingleOrDefault();
		if (export == null)
		{
			return default(T);
		}
		return ExportServices.GetCastedExportedValue<T>(export);
	}

	private IEnumerable<Lazy<T>> GetExportsCore<T>(string contractName)
	{
		IEnumerable<Export> exportsCore = GetExportsCore(typeof(T), null, contractName, ImportCardinality.ZeroOrMore);
		Collection<Lazy<T>> collection = new Collection<Lazy<T>>();
		foreach (Export item in exportsCore)
		{
			collection.Add(ExportServices.CreateStronglyTypedLazyOfT<T>(item));
		}
		return collection;
	}

	private IEnumerable<Lazy<T, TMetadataView>> GetExportsCore<T, TMetadataView>(string contractName)
	{
		IEnumerable<Export> exportsCore = GetExportsCore(typeof(T), typeof(TMetadataView), contractName, ImportCardinality.ZeroOrMore);
		Collection<Lazy<T, TMetadataView>> collection = new Collection<Lazy<T, TMetadataView>>();
		foreach (Export item in exportsCore)
		{
			collection.Add(ExportServices.CreateStronglyTypedLazyOfTM<T, TMetadataView>(item));
		}
		return collection;
	}

	private Lazy<T, TMetadataView> GetExportCore<T, TMetadataView>(string contractName)
	{
		Export export = GetExportsCore(typeof(T), typeof(TMetadataView), contractName, ImportCardinality.ExactlyOne).SingleOrDefault();
		if (export == null)
		{
			return null;
		}
		return ExportServices.CreateStronglyTypedLazyOfTM<T, TMetadataView>(export);
	}

	private Lazy<T> GetExportCore<T>(string contractName)
	{
		Export export = GetExportsCore(typeof(T), null, contractName, ImportCardinality.ExactlyOne).SingleOrDefault();
		if (export == null)
		{
			return null;
		}
		return ExportServices.CreateStronglyTypedLazyOfT<T>(export);
	}

	private IEnumerable<Export> GetExportsCore(Type type, Type metadataViewType, string contractName, ImportCardinality cardinality)
	{
		Requires.NotNull(type, "type");
		if (string.IsNullOrEmpty(contractName))
		{
			contractName = AttributedModelServices.GetContractName(type);
		}
		if (metadataViewType == null)
		{
			metadataViewType = ExportServices.DefaultMetadataViewType;
		}
		if (!MetadataViewProvider.IsViewTypeValid(metadataViewType))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidMetadataView, metadataViewType.Name));
		}
		ImportDefinition definition = BuildImportDefinition(type, metadataViewType, contractName, cardinality);
		return GetExports(definition, null);
	}

	private static ImportDefinition BuildImportDefinition(Type type, Type metadataViewType, string contractName, ImportCardinality cardinality)
	{
		Assumes.NotNull(type, metadataViewType, contractName);
		IEnumerable<KeyValuePair<string, Type>> requiredMetadata = CompositionServices.GetRequiredMetadata(metadataViewType);
		IDictionary<string, object> importMetadata = CompositionServices.GetImportMetadata(type, null);
		string requiredTypeIdentity = null;
		if (type != typeof(object))
		{
			requiredTypeIdentity = AttributedModelServices.GetTypeIdentity(type);
		}
		return new ContractBasedImportDefinition(contractName, requiredTypeIdentity, requiredMetadata, cardinality, isRecomposable: false, isPrerequisite: true, CreationPolicy.Any, importMetadata);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> class.</summary>
	protected ExportProvider()
	{
	}

	/// <summary>Gets all exports that match the conditions of the specified import definition.</summary>
	/// <returns>A collection of all the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects matching the condition specified by <paramref name="definition" />.</returns>
	/// <param name="definition">The object that defines the conditions of the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to get.</param>
	/// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">
	///   <see cref="P:System.ComponentModel.Composition.Primitives.ImportDefinition.Cardinality" /> is <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne" /> and there are zero <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects that match the conditions of the specified <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" />.-or-<see cref="P:System.ComponentModel.Composition.Primitives.ImportDefinition.Cardinality" /> is <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ZeroOrOne" /> or <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne" /> and there is more than one <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> object that matches the conditions of the specified <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="definition" /> is null.</exception>
	public IEnumerable<Export> GetExports(ImportDefinition definition)
	{
		return GetExports(definition, null);
	}

	/// <summary>Gets all exports that match the conditions of the specified import definition and composition.</summary>
	/// <returns>A collection of all the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects matching the condition specified by <paramref name="definition" /> and <paramref name="atomicComposition" />.</returns>
	/// <param name="definition">The object that defines the conditions of the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to get.</param>
	/// <param name="atomicComposition">The transactional container for the composition.</param>
	/// <exception cref="T:System.ComponentModel.Composition.ImportCardinalityMismatchException">
	///   <see cref="P:System.ComponentModel.Composition.Primitives.ImportDefinition.Cardinality" /> is <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne" /> and there are zero <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects that match the conditions of the specified <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" />.-or-<see cref="P:System.ComponentModel.Composition.Primitives.ImportDefinition.Cardinality" /> is <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ZeroOrOne" /> or <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne" /> and there is more than one <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> object that matches the conditions of the specified <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="definition" /> is null.-or-<paramref name="atomicComposition" /> is null.</exception>
	public IEnumerable<Export> GetExports(ImportDefinition definition, AtomicComposition atomicComposition)
	{
		Requires.NotNull(definition, "definition");
		IEnumerable<Export> exports;
		ExportCardinalityCheckResult exportCardinalityCheckResult = TryGetExportsCore(definition, atomicComposition, out exports);
		switch (exportCardinalityCheckResult)
		{
		case ExportCardinalityCheckResult.Match:
			return exports;
		case ExportCardinalityCheckResult.NoExports:
			throw new ImportCardinalityMismatchException(string.Format(CultureInfo.CurrentCulture, Strings.CardinalityMismatch_NoExports, definition.ToString()));
		default:
			Assumes.IsTrue(exportCardinalityCheckResult == ExportCardinalityCheckResult.TooManyExports);
			throw new ImportCardinalityMismatchException(string.Format(CultureInfo.CurrentCulture, Strings.CardinalityMismatch_TooManyExports, definition.ToString()));
		}
	}

	/// <summary>Gets all the exports that match the conditions of the specified import.</summary>
	/// <returns>trueif <see cref="P:System.ComponentModel.Composition.Primitives.ImportDefinition.Cardinality" /> is <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ZeroOrOne" /> or <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ZeroOrMore" /> and there are zero <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects that match the conditions of the specified <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" />; true if <see cref="P:System.ComponentModel.Composition.Primitives.ImportDefinition.Cardinality" /> is <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ZeroOrOne" /> or <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne" /> and there is exactly one <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> that matches the conditions of the specified <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" />; otherwise, false.</returns>
	/// <param name="definition">The object that defines the conditions of the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to get.</param>
	/// <param name="atomicComposition">The transactional container for the composition.</param>
	/// <param name="exports">When this method returns, contains a collection of <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects that match the conditions defined by <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" />, if found; otherwise, an empty <see cref="T:System.Collections.Generic.IEnumerable`1" /> object. This parameter is passed uninitialized. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="definition" /> is null.</exception>
	public bool TryGetExports(ImportDefinition definition, AtomicComposition atomicComposition, out IEnumerable<Export> exports)
	{
		Requires.NotNull(definition, "definition");
		exports = null;
		return TryGetExportsCore(definition, atomicComposition, out exports) == ExportCardinalityCheckResult.Match;
	}

	/// <summary>Gets all the exports that match the constraint defined by the specified definition.</summary>
	/// <returns>A collection that contains all the exports that match the specified condition.</returns>
	/// <param name="definition">The object that defines the conditions of the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to return.</param>
	/// <param name="atomicComposition">The transactional container for the composition.</param>
	protected abstract IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition);

	/// <summary>Raises the <see cref="E:System.ComponentModel.Composition.Hosting.ExportProvider.ExportsChanged" /> event.</summary>
	/// <param name="e">An <see cref="T:System.ComponentModel.Composition.Hosting.ExportsChangeEventArgs" /> that contains the event data.</param>
	protected virtual void OnExportsChanged(ExportsChangeEventArgs e)
	{
		EventHandler<ExportsChangeEventArgs> eventHandler = this.ExportsChanged;
		if (eventHandler != null)
		{
			CompositionServices.TryFire(eventHandler, this, e).ThrowOnErrors(e.AtomicComposition);
		}
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.Composition.Hosting.ExportProvider.ExportsChanging" /> event.</summary>
	/// <param name="e">An <see cref="T:System.ComponentModel.Composition.Hosting.ExportsChangeEventArgs" /> that contains the event data.</param>
	protected virtual void OnExportsChanging(ExportsChangeEventArgs e)
	{
		EventHandler<ExportsChangeEventArgs> eventHandler = this.ExportsChanging;
		if (eventHandler != null)
		{
			CompositionServices.TryFire(eventHandler, this, e).ThrowOnErrors(e.AtomicComposition);
		}
	}

	private ExportCardinalityCheckResult TryGetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition, out IEnumerable<Export> exports)
	{
		Assumes.NotNull(definition);
		exports = GetExportsCore(definition, atomicComposition);
		ExportCardinalityCheckResult exportCardinalityCheckResult = ExportServices.CheckCardinality(definition, exports);
		if (exportCardinalityCheckResult == ExportCardinalityCheckResult.TooManyExports && definition.Cardinality == ImportCardinality.ZeroOrOne)
		{
			exportCardinalityCheckResult = ExportCardinalityCheckResult.Match;
			exports = null;
		}
		if (exports == null)
		{
			exports = EmptyExports;
		}
		return exportCardinalityCheckResult;
	}
}
