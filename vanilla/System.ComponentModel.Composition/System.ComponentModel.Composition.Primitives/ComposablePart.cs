using System.Collections.Generic;

namespace System.ComponentModel.Composition.Primitives;

/// <summary>Defines the abstract base class for composable parts, which import objects and produce exported objects.</summary>
public abstract class ComposablePart
{
	/// <summary>Gets a collection of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects that describe the exported objects provided by the part.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects that describe the exported objects provided by the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" />.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object has been disposed of.</exception>
	public abstract IEnumerable<ExportDefinition> ExportDefinitions { get; }

	/// <summary>Gets a collection of the <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> objects that describe the imported objects required by the part.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> objects that describe the imported objects required by the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" />.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object has been disposed of.</exception>
	public abstract IEnumerable<ImportDefinition> ImportDefinitions { get; }

	/// <summary>Gets the metadata of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object.</summary>
	/// <returns>The metadata of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object. The default is an empty, read-only <see cref="T:System.Collections.Generic.IDictionary`2" /> object. </returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object has been disposed of.</exception>
	public virtual IDictionary<string, object> Metadata => MetadataServices.EmptyMetadata;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> class.</summary>
	protected ComposablePart()
	{
	}

	/// <summary>Called when all the imports of the part have been set, and exports can be retrieved.</summary>
	public virtual void Activate()
	{
	}

	/// <summary>Gets the exported object described by the specified <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> object.</summary>
	/// <returns>The exported object described by <paramref name="definition" />.</returns>
	/// <param name="definition">One of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects from the <see cref="P:System.ComponentModel.Composition.Primitives.ComposablePart.ExportDefinitions" /> property that describes the exported object to return.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="definition" /> is null.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException">An error occurred getting the exported object described by the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="definition" /> did not originate from the <see cref="P:System.ComponentModel.Composition.Primitives.ComposablePart.ExportDefinitions" /> property on the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">One or more prerequisite imports, indicated by <see cref="P:System.ComponentModel.Composition.Primitives.ImportDefinition.IsPrerequisite" />, have not been set.</exception>
	public abstract object GetExportedValue(ExportDefinition definition);

	/// <summary>Sets the import described by the specified <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> object to be satisfied by the specified exports.</summary>
	/// <param name="definition">One of the objects from the <see cref="P:System.ComponentModel.Composition.Primitives.ComposablePart.ImportDefinitions" /> property that specifies the import to be set.</param>
	/// <param name="exports">A collection of <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects of which to set the import described by <paramref name="definition" />.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="definition" /> is null. -or- <paramref name="exports" /> is null.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.Primitives.ComposablePartException">An error occurred setting the import described by the <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> object.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="definition" /> did not originate from the <see cref="P:System.ComponentModel.Composition.Primitives.ComposablePart.ImportDefinitions" /> property on the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" />. -or- <paramref name="exports" /> contains an element that is null. -or- <paramref name="exports" /> is empty and <see cref="P:System.ComponentModel.Composition.Primitives.ImportDefinition.Cardinality" /> is <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne" />. -or- <paramref name="exports" /> contains more than one element and <see cref="P:System.ComponentModel.Composition.Primitives.ImportDefinition.Cardinality" /> is <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ZeroOrOne" /> or <see cref="F:System.ComponentModel.Composition.Primitives.ImportCardinality.ExactlyOne" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.ComponentModel.Composition.Primitives.ComposablePart.SetImport(System.ComponentModel.Composition.Primitives.ImportDefinition,System.Collections.Generic.IEnumerable{System.ComponentModel.Composition.Primitives.Export})" /> has been previously called and <see cref="P:System.ComponentModel.Composition.Primitives.ImportDefinition.IsRecomposable" /> is false.</exception>
	public abstract void SetImport(ImportDefinition definition, IEnumerable<Export> exports);
}
