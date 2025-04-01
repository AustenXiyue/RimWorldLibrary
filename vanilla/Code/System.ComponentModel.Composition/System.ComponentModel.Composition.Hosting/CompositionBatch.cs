using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Represents a set of <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> objects which will be added or removed from the container in a single transactional composition.</summary>
public class CompositionBatch
{
	private class SingleExportComposablePart : ComposablePart
	{
		private readonly Export _export;

		public override IDictionary<string, object> Metadata => MetadataServices.EmptyMetadata;

		public override IEnumerable<ExportDefinition> ExportDefinitions => new ExportDefinition[1] { _export.Definition };

		public override IEnumerable<ImportDefinition> ImportDefinitions => Enumerable.Empty<ImportDefinition>();

		public SingleExportComposablePart(Export export)
		{
			Assumes.NotNull(export);
			_export = export;
		}

		public override object GetExportedValue(ExportDefinition definition)
		{
			Requires.NotNull(definition, "definition");
			if (definition != _export.Definition)
			{
				throw ExceptionBuilder.CreateExportDefinitionNotOnThisComposablePart("definition");
			}
			return _export.Value;
		}

		public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
		{
			Requires.NotNull(definition, "definition");
			Requires.NotNullOrNullElements(exports, "exports");
			throw ExceptionBuilder.CreateImportDefinitionNotOnThisComposablePart("definition");
		}
	}

	private object _lock = new object();

	private bool _copyNeededForAdd;

	private bool _copyNeededForRemove;

	private List<ComposablePart> _partsToAdd;

	private ReadOnlyCollection<ComposablePart> _readOnlyPartsToAdd;

	private List<ComposablePart> _partsToRemove;

	private ReadOnlyCollection<ComposablePart> _readOnlyPartsToRemove;

	/// <summary>Gets the collection of <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> objects to be added.</summary>
	/// <returns>A collection of parts to be added.</returns>
	public ReadOnlyCollection<ComposablePart> PartsToAdd
	{
		get
		{
			lock (_lock)
			{
				_copyNeededForAdd = true;
				return _readOnlyPartsToAdd;
			}
		}
	}

	/// <summary>Gets the collection of <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> objects to be removed.</summary>
	/// <returns>A collection of parts to be removed.</returns>
	public ReadOnlyCollection<ComposablePart> PartsToRemove
	{
		get
		{
			lock (_lock)
			{
				_copyNeededForRemove = true;
				return _readOnlyPartsToRemove;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionBatch" /> class.</summary>
	public CompositionBatch()
		: this(null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionBatch" /> class with the specified parts for addition and removal.</summary>
	/// <param name="partsToAdd">A collection of <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> objects to add.</param>
	/// <param name="partsToRemove">A collection of <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> objects to remove.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="partsToAdd" /> is null.-or-<paramref name="partsToRemove" /> is null.</exception>
	public CompositionBatch(IEnumerable<ComposablePart> partsToAdd, IEnumerable<ComposablePart> partsToRemove)
	{
		_partsToAdd = new List<ComposablePart>();
		if (partsToAdd != null)
		{
			foreach (ComposablePart item in partsToAdd)
			{
				if (item == null)
				{
					throw ExceptionBuilder.CreateContainsNullElement("partsToAdd");
				}
				_partsToAdd.Add(item);
			}
		}
		_readOnlyPartsToAdd = _partsToAdd.AsReadOnly();
		_partsToRemove = new List<ComposablePart>();
		if (partsToRemove != null)
		{
			foreach (ComposablePart item2 in partsToRemove)
			{
				if (item2 == null)
				{
					throw ExceptionBuilder.CreateContainsNullElement("partsToRemove");
				}
				_partsToRemove.Add(item2);
			}
		}
		_readOnlyPartsToRemove = _partsToRemove.AsReadOnly();
	}

	/// <summary>Adds the specified part to the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionBatch" /> object.</summary>
	/// <param name="part">The part to add.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="part" /> is null.</exception>
	public void AddPart(ComposablePart part)
	{
		Requires.NotNull(part, "part");
		lock (_lock)
		{
			if (_copyNeededForAdd)
			{
				_partsToAdd = new List<ComposablePart>(_partsToAdd);
				_readOnlyPartsToAdd = _partsToAdd.AsReadOnly();
				_copyNeededForAdd = false;
			}
			_partsToAdd.Add(part);
		}
	}

	/// <summary>Puts the specified part on the list of parts to remove.</summary>
	/// <param name="part">The part to be removed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="part" /> is null.</exception>
	public void RemovePart(ComposablePart part)
	{
		Requires.NotNull(part, "part");
		lock (_lock)
		{
			if (_copyNeededForRemove)
			{
				_partsToRemove = new List<ComposablePart>(_partsToRemove);
				_readOnlyPartsToRemove = _partsToRemove.AsReadOnly();
				_copyNeededForRemove = false;
			}
			_partsToRemove.Add(part);
		}
	}

	/// <summary>Adds the specified export to the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionBatch" /> object.</summary>
	/// <returns>The part added.</returns>
	/// <param name="export">The export to add to the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionBatch" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="export" /> is null.</exception>
	public ComposablePart AddExport(Export export)
	{
		Requires.NotNull(export, "export");
		ComposablePart composablePart = new SingleExportComposablePart(export);
		AddPart(composablePart);
		return composablePart;
	}
}
