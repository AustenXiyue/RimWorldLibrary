using System.Collections.Specialized;

namespace System.Windows.Controls;

internal sealed class SelectedCellsCollection : VirtualizedCellInfoCollection
{
	internal SelectedCellsCollection(DataGrid owner)
		: base(owner)
	{
	}

	internal bool GetSelectionRange(out int minColumnDisplayIndex, out int maxColumnDisplayIndex, out int minRowIndex, out int maxRowIndex)
	{
		if (base.IsEmpty)
		{
			minColumnDisplayIndex = -1;
			maxColumnDisplayIndex = -1;
			minRowIndex = -1;
			maxRowIndex = -1;
			return false;
		}
		GetBoundingRegion(out minColumnDisplayIndex, out minRowIndex, out maxColumnDisplayIndex, out maxRowIndex);
		return true;
	}

	protected override void OnCollectionChanged(NotifyCollectionChangedAction action, VirtualizedCellInfoCollection oldItems, VirtualizedCellInfoCollection newItems)
	{
		base.Owner.OnSelectedCellsChanged(action, oldItems, newItems);
	}
}
