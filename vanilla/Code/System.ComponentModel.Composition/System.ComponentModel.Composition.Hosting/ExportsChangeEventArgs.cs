using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Composition.Hosting.ExportProvider.ExportsChanging" /> and <see cref="E:System.ComponentModel.Composition.Hosting.ExportProvider.ExportsChanged" /> event. </summary>
public class ExportsChangeEventArgs : EventArgs
{
	private readonly IEnumerable<ExportDefinition> _addedExports;

	private readonly IEnumerable<ExportDefinition> _removedExports;

	private IEnumerable<string> _changedContractNames;

	/// <summary>Gets the exports that were added in this change.</summary>
	/// <returns>A collection of the exports that were added.</returns>
	public IEnumerable<ExportDefinition> AddedExports => _addedExports;

	/// <summary>Gets the exports that were removed in the change.</summary>
	/// <returns>A collection of the removed exports.</returns>
	public IEnumerable<ExportDefinition> RemovedExports => _removedExports;

	/// <summary>Gets the contract names that were altered in the change.</summary>
	/// <returns>A collection of the altered contract names.</returns>
	public IEnumerable<string> ChangedContractNames
	{
		get
		{
			if (_changedContractNames == null)
			{
				_changedContractNames = (from export in AddedExports.Concat(RemovedExports)
					select export.ContractName).Distinct().ToArray();
			}
			return _changedContractNames;
		}
	}

	/// <summary>Gets the composition transaction of the change, if any.</summary>
	/// <returns>A reference to the composition transaction associated with the change, or null if no transaction is being used.</returns>
	public AtomicComposition AtomicComposition { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.ExportsChangeEventArgs" /> class. </summary>
	/// <param name="addedExports">The events that were added.</param>
	/// <param name="removedExports">The events that were removed.</param>
	/// <param name="atomicComposition">The composition transaction that contains the change.</param>
	public ExportsChangeEventArgs(IEnumerable<ExportDefinition> addedExports, IEnumerable<ExportDefinition> removedExports, AtomicComposition atomicComposition)
	{
		Requires.NotNull(addedExports, "addedExports");
		Requires.NotNull(removedExports, "removedExports");
		_addedExports = addedExports.AsArray();
		_removedExports = removedExports.AsArray();
		AtomicComposition = atomicComposition;
	}
}
