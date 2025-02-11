namespace System.Windows.Controls.Primitives;

/// <summary>Used within the template of a <see cref="T:System.Windows.Controls.DataGrid" /> to specify the location in the control's visual tree where the rows are to be added. </summary>
public class DataGridRowsPresenter : VirtualizingStackPanel
{
	private DataGrid _owner;

	private Size _availableSize;

	internal Size AvailableSize => _availableSize;

	internal DataGrid Owner
	{
		get
		{
			if (_owner == null)
			{
				_owner = ItemsControl.GetItemsOwner(this) as DataGrid;
			}
			return _owner;
		}
	}

	internal void InternalBringIndexIntoView(int index)
	{
		BringIndexIntoView(index);
	}

	/// <param name="oldIsItemsHost">The old property value.</param>
	/// <param name="newIsItemsHost">The new property value.</param>
	protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
	{
		base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);
		if (newIsItemsHost)
		{
			DataGrid owner = Owner;
			if (owner != null)
			{
				IItemContainerGenerator itemContainerGenerator = owner.ItemContainerGenerator;
				if (itemContainerGenerator != null && itemContainerGenerator == itemContainerGenerator.GetItemContainerGeneratorForPanel(this))
				{
					owner.InternalItemsHost = this;
				}
			}
		}
		else
		{
			if (_owner != null && _owner.InternalItemsHost == this)
			{
				_owner.InternalItemsHost = null;
			}
			_owner = null;
		}
	}

	/// <param name="oldViewportSize">The old size of the viewport.</param>
	/// <param name="newViewportSize">The new size of the viewport.</param>
	protected override void OnViewportSizeChanged(Size oldViewportSize, Size newViewportSize)
	{
		DataGrid owner = Owner;
		if (owner != null)
		{
			ScrollContentPresenter internalScrollContentPresenter = owner.InternalScrollContentPresenter;
			if (internalScrollContentPresenter == null || internalScrollContentPresenter.CanContentScroll)
			{
				owner.OnViewportSizeChanged(oldViewportSize, newViewportSize);
			}
		}
	}

	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the desired size of the element.</returns>
	/// <param name="constraint">An upper limit <see cref="T:System.Windows.Size" /> that should not be exceeded.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		_availableSize = constraint;
		return base.MeasureOverride(constraint);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.VirtualizingStackPanel.CleanUpVirtualizedItem" /> event for rows that have scrolled out of view. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnCleanUpVirtualizedItem(CleanUpVirtualizedItemEventArgs e)
	{
		base.OnCleanUpVirtualizedItem(e);
		if (e.UIElement != null && Validation.GetHasError(e.UIElement))
		{
			e.Cancel = true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DataGridRowsPresenter" /> class. </summary>
	public DataGridRowsPresenter()
	{
	}
}
