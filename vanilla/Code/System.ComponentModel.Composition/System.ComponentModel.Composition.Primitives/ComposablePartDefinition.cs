using System.Collections.Generic;
using System.Linq;

namespace System.ComponentModel.Composition.Primitives;

/// <summary>Defines an abstract base class for composable part definitions, which describe and enable the creation of <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> objects.</summary>
public abstract class ComposablePartDefinition
{
	internal static readonly IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> _EmptyExports = Enumerable.Empty<Tuple<ComposablePartDefinition, ExportDefinition>>();

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects that describe the objects exported by the part defined by this <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> object.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects that describe the exported objects provided by <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> objects created by the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" />.</returns>
	public abstract IEnumerable<ExportDefinition> ExportDefinitions { get; }

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> objects that describe the imports required by the part defined by this <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> object.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> objects that describe the imports required by <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> objects created by the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" />.</returns>
	public abstract IEnumerable<ImportDefinition> ImportDefinitions { get; }

	/// <summary>Gets a collection of the metadata for this <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> object.</summary>
	/// <returns>A collection that contains the metadata for the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" />. The default is an empty, read-only <see cref="T:System.Collections.Generic.IDictionary`2" /> object.</returns>
	public virtual IDictionary<string, object> Metadata => MetadataServices.EmptyMetadata;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> class.</summary>
	protected ComposablePartDefinition()
	{
	}

	/// <summary>Creates a new instance of a part that the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> describes.</summary>
	/// <returns>The created part.</returns>
	public abstract ComposablePart CreatePart();

	internal virtual IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
	{
		List<Tuple<ComposablePartDefinition, ExportDefinition>> list = null;
		foreach (ExportDefinition exportDefinition in ExportDefinitions)
		{
			if (definition.IsConstraintSatisfiedBy(exportDefinition))
			{
				if (list == null)
				{
					list = new List<Tuple<ComposablePartDefinition, ExportDefinition>>();
				}
				list.Add(new Tuple<ComposablePartDefinition, ExportDefinition>(this, exportDefinition));
			}
		}
		IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> enumerable = list;
		return enumerable ?? _EmptyExports;
	}

	internal virtual ComposablePartDefinition GetGenericPartDefinition()
	{
		return null;
	}
}
