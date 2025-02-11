using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.SelectedCellsChanged" /> event. </summary>
public class SelectedCellsChangedEventArgs : EventArgs
{
	private IList<DataGridCellInfo> _addedCells;

	private IList<DataGridCellInfo> _removedCells;

	/// <summary>Gets the cells that were added to the selection.</summary>
	/// <returns>The added cells.</returns>
	public IList<DataGridCellInfo> AddedCells => _addedCells;

	/// <summary>Gets the list of cells removed from the selection.</summary>
	/// <returns>The list of cells removed.</returns>
	public IList<DataGridCellInfo> RemovedCells => _removedCells;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.SelectedCellsChangedEventArgs" /> class with the specified cells added to and removed from the selection. </summary>
	/// <param name="addedCells">The cells added to the selection.</param>
	/// <param name="removedCells">The cells removed from the selection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="addedCells" /> or <paramref name="removedCells" /> is null.</exception>
	public SelectedCellsChangedEventArgs(List<DataGridCellInfo> addedCells, List<DataGridCellInfo> removedCells)
	{
		if (addedCells == null)
		{
			throw new ArgumentNullException("addedCells");
		}
		if (removedCells == null)
		{
			throw new ArgumentNullException("removedCells");
		}
		_addedCells = addedCells.AsReadOnly();
		_removedCells = removedCells.AsReadOnly();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.SelectedCellsChangedEventArgs" /> class with the specified cells added to and removed from the selection.</summary>
	/// <param name="addedCells">The cells added to the selection.</param>
	/// <param name="removedCells">The cells removed from the selection.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="addedCells" /> or <paramref name="removedCells" /> is null.</exception>
	public SelectedCellsChangedEventArgs(ReadOnlyCollection<DataGridCellInfo> addedCells, ReadOnlyCollection<DataGridCellInfo> removedCells)
	{
		if (addedCells == null)
		{
			throw new ArgumentNullException("addedCells");
		}
		if (removedCells == null)
		{
			throw new ArgumentNullException("removedCells");
		}
		_addedCells = addedCells;
		_removedCells = removedCells;
	}

	internal SelectedCellsChangedEventArgs(DataGrid owner, VirtualizedCellInfoCollection addedCells, VirtualizedCellInfoCollection removedCells)
	{
		_addedCells = ((addedCells != null) ? addedCells : VirtualizedCellInfoCollection.MakeEmptyCollection(owner));
		_removedCells = ((removedCells != null) ? removedCells : VirtualizedCellInfoCollection.MakeEmptyCollection(owner));
	}
}
