using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;

namespace System.ComponentModel;

/// <summary>Enables collections to have the functionalities of current record management, custom sorting, filtering, and grouping.</summary>
public interface ICollectionView : IEnumerable, INotifyCollectionChanged
{
	/// <summary>Gets or sets the cultural info for any operations of the view that may differ by culture, such as sorting.</summary>
	/// <returns>The culture to use during sorting.</returns>
	CultureInfo Culture { get; set; }

	/// <summary>Returns the underlying collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerable" /> object that is the underlying collection.</returns>
	IEnumerable SourceCollection { get; }

	/// <summary>Gets or sets a callback used to determine if an item is suitable for inclusion in the view.</summary>
	/// <returns>A method used to determine if an item is suitable for inclusion in the view.</returns>
	Predicate<object> Filter { get; set; }

	/// <summary>Gets a value that indicates whether this view supports filtering via the <see cref="P:System.ComponentModel.ICollectionView.Filter" /> property.</summary>
	/// <returns>true if this view support filtering; otherwise, false.</returns>
	bool CanFilter { get; }

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.SortDescription" /> objects that describe how the items in the collection are sorted in the view.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.SortDescription" /> objects that describe how the items in the collection are sorted in the view.</returns>
	SortDescriptionCollection SortDescriptions { get; }

	/// <summary>Gets a value that indicates whether this view supports sorting via the <see cref="P:System.ComponentModel.ICollectionView.SortDescriptions" /> property.</summary>
	/// <returns>true if this view supports sorting; otherwise, false.</returns>
	bool CanSort { get; }

	/// <summary>Gets a value that indicates whether this view supports grouping via the <see cref="P:System.ComponentModel.ICollectionView.GroupDescriptions" /> property.</summary>
	/// <returns>true if this view supports grouping; otherwise, false.</returns>
	bool CanGroup { get; }

	/// <summary>Gets a collection of <see cref="T:System.ComponentModel.GroupDescription" /> objects that describe how the items in the collection are grouped in the view.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.GroupDescription" /> objects that describe how the items in the collection are grouped in the view.</returns>
	ObservableCollection<GroupDescription> GroupDescriptions { get; }

	/// <summary>Gets the top-level groups.</summary>
	/// <returns>A read-only collection of the top-level groups or null if there are no groups.</returns>
	ReadOnlyObservableCollection<object> Groups { get; }

	/// <summary>Returns a value that indicates whether the resulting view is empty.</summary>
	/// <returns>true if the resulting view is empty; otherwise, false.</returns>
	bool IsEmpty { get; }

	/// <summary>Gets the current item in the view.</summary>
	/// <returns>The current item of the view or null if there is no current item.</returns>
	object CurrentItem { get; }

	/// <summary>Gets the ordinal position of the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> within the view.</summary>
	/// <returns>The ordinal position of the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> within the view.</returns>
	int CurrentPosition { get; }

	/// <summary>Gets a value that indicates whether the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> of the view is beyond the end of the collection.</summary>
	/// <returns>Returns true if the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> of the view is beyond the end of the collection; otherwise, false.</returns>
	bool IsCurrentAfterLast { get; }

	/// <summary>Gets a value that indicates whether the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> of the view is beyond the beginning of the collection.</summary>
	/// <returns>Returns true if the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> of the view is beyond the beginning of the collection; otherwise, false.</returns>
	bool IsCurrentBeforeFirst { get; }

	/// <summary>When implementing this interface, raise this event before changing the current item. Event handler can cancel this event.</summary>
	event CurrentChangingEventHandler CurrentChanging;

	/// <summary>When implementing this interface, raise this event after the current item has been changed.</summary>
	event EventHandler CurrentChanged;

	/// <summary>Returns a value that indicates whether a given item belongs to this collection view.</summary>
	/// <returns>true if the item belongs to this collection view; otherwise, false.</returns>
	/// <param name="item">The object to check.</param>
	bool Contains(object item);

	/// <summary>Recreates the view.</summary>
	void Refresh();

	/// <summary>Enters a defer cycle that you can use to merge changes to the view and delay automatic refresh.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> object that you can use to dispose of the calling object.</returns>
	IDisposable DeferRefresh();

	/// <summary>Sets the first item in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" />.</summary>
	/// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	bool MoveCurrentToFirst();

	/// <summary>Sets the last item in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" />.</summary>
	/// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	bool MoveCurrentToLast();

	/// <summary>Sets the item after the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" />.</summary>
	/// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	bool MoveCurrentToNext();

	/// <summary>Sets the item before the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> in the view as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" />.</summary>
	/// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	bool MoveCurrentToPrevious();

	/// <summary>Sets the specified item to be the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> in the view.</summary>
	/// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> is within the view; otherwise, false.</returns>
	/// <param name="item">The item to set as the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" />.</param>
	bool MoveCurrentTo(object item);

	/// <summary>Sets the item at the specified index to be the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> in the view.</summary>
	/// <returns>true if the resulting <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> is an item within the view; otherwise, false.</returns>
	/// <param name="position">The index to set the <see cref="P:System.ComponentModel.ICollectionView.CurrentItem" /> to.</param>
	bool MoveCurrentToPosition(int position);
}
