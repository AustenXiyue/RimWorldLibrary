using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition;

/// <summary>A factory that creates new instances of a part that provides the specified export.</summary>
/// <typeparam name="T">The type of the export.</typeparam>
public class ExportFactory<T>
{
	private Func<Tuple<T, Action>> _exportLifetimeContextCreator;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ExportFactory`1" /> class.</summary>
	/// <param name="exportLifetimeContextCreator">A function that returns the exported value and an <see cref="T:System.Action" /> that releases it.</param>
	public ExportFactory(Func<Tuple<T, Action>> exportLifetimeContextCreator)
	{
		if (exportLifetimeContextCreator == null)
		{
			throw new ArgumentNullException("exportLifetimeContextCreator");
		}
		_exportLifetimeContextCreator = exportLifetimeContextCreator;
	}

	/// <summary>Creates an instance of the factory's export type.</summary>
	/// <returns>A valid instance of the factory's exported type.</returns>
	public ExportLifetimeContext<T> CreateExport()
	{
		Tuple<T, Action> tuple = _exportLifetimeContextCreator();
		return new ExportLifetimeContext<T>(tuple.Item1, tuple.Item2);
	}

	internal bool IncludeInScopedCatalog(ComposablePartDefinition composablePartDefinition)
	{
		return OnFilterScopedCatalog(composablePartDefinition);
	}

	protected virtual bool OnFilterScopedCatalog(ComposablePartDefinition composablePartDefinition)
	{
		return true;
	}
}
/// <summary>A factory that creates new instances of a part that provides the specified export, with attached metadata.</summary>
/// <typeparam name="T">The type of the created part.</typeparam>
/// <typeparam name="TMetadata">The type of the created part's metadata.</typeparam>
public class ExportFactory<T, TMetadata> : ExportFactory<T>
{
	private readonly TMetadata _metadata;

	/// <summary>Gets the metadata to be attached to the created parts.</summary>
	/// <returns>A metadata object that will be attached to the created parts.</returns>
	public TMetadata Metadata => _metadata;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ExportFactory`2" /> class. </summary>
	/// <param name="exportLifetimeContextCreator">A function that returns the exported value and an <see cref="T:System.Action" /> that releases it.</param>
	/// <param name="metadata">The metadata to attach to the created parts.</param>
	public ExportFactory(Func<Tuple<T, Action>> exportLifetimeContextCreator, TMetadata metadata)
		: base(exportLifetimeContextCreator)
	{
		_metadata = metadata;
	}
}
