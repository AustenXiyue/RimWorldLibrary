using System.Collections;
using System.ComponentModel;

namespace System.Windows.Controls.Primitives;

/// <summary>Provides an abstract class for controls that allow multiple items to be selected.</summary>
public abstract class MultiSelector : Selector
{
	/// <summary>Gets or sets a value that indicates whether the multiple items in the <see cref="T:System.Windows.Controls.Primitives.MultiSelector" /> can be selected at a time. </summary>
	/// <returns>true if multiple items in the <see cref="T:System.Windows.Controls.Primitives.MultiSelector" /> can be selected at a time; otherwise, false.</returns>
	protected bool CanSelectMultipleItems
	{
		get
		{
			return base.CanSelectMultiple;
		}
		set
		{
			base.CanSelectMultiple = value;
		}
	}

	/// <summary>Gets the items in the <see cref="T:System.Windows.Controls.Primitives.MultiSelector" /> that are selected.</summary>
	/// <returns>The items in the <see cref="T:System.Windows.Controls.Primitives.MultiSelector" /> that are selected.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IList SelectedItems => base.SelectedItemsImpl;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.Primitives.MultiSelector" /> is currently performing a bulk update to the <see cref="P:System.Windows.Controls.Primitives.MultiSelector.SelectedItems" /> collection.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Primitives.MultiSelector" /> is currently performing a bulk update to the <see cref="P:System.Windows.Controls.Primitives.MultiSelector.SelectedItems" /> collection; otherwise, false.</returns>
	protected bool IsUpdatingSelectedItems => ((SelectedItemCollection)SelectedItems).IsUpdatingSelectedItems;

	/// <summary>Starts a new selection transaction.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Controls.Primitives.MultiSelector.IsUpdatingSelectedItems" /> is true when this method is called.</exception>
	protected void BeginUpdateSelectedItems()
	{
		((SelectedItemCollection)SelectedItems).BeginUpdateSelectedItems();
	}

	/// <summary>Commits the selected items to the <see cref="T:System.Windows.Controls.Primitives.MultiSelector" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Controls.Primitives.MultiSelector.IsUpdatingSelectedItems" /> is false when this method is called.</exception>
	protected void EndUpdateSelectedItems()
	{
		((SelectedItemCollection)SelectedItems).EndUpdateSelectedItems();
	}

	/// <summary>Selects all of the items in the <see cref="T:System.Windows.Controls.Primitives.MultiSelector" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.Windows.Controls.Primitives.MultiSelector.CanSelectMultipleItems" /> is false.</exception>
	public void SelectAll()
	{
		if (CanSelectMultipleItems)
		{
			SelectAllImpl();
			return;
		}
		throw new NotSupportedException(SR.MultiSelectorSelectAll);
	}

	/// <summary>Unselects all of the items in the <see cref="T:System.Windows.Controls.Primitives.MultiSelector" />.</summary>
	public void UnselectAll()
	{
		UnselectAllImpl();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.MultiSelector" /> class. </summary>
	protected MultiSelector()
	{
	}
}
