using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using MS.Internal;

namespace System.ComponentModel;

/// <summary>Provides an abstract base class for types that describe how to divide the items in a collection into groups.</summary>
public abstract class GroupDescription : INotifyPropertyChanged
{
	private ObservableCollection<object> _explicitGroupNames;

	private SortDescriptionCollection _sort;

	private IComparer _customSort;

	/// <summary>Gets the collection of names that are used to initialize a group with a set of subgroups with the given names.</summary>
	/// <returns>The collection of names that are used to initialize a group with a set of subgroups with the given names.</returns>
	public ObservableCollection<object> GroupNames => _explicitGroupNames;

	public SortDescriptionCollection SortDescriptions
	{
		get
		{
			if (_sort == null)
			{
				SetSortDescriptions(new SortDescriptionCollection());
			}
			return _sort;
		}
	}

	public IComparer CustomSort
	{
		get
		{
			return _customSort;
		}
		set
		{
			_customSort = value;
			SetSortDescriptions(null);
			OnPropertyChanged(new PropertyChangedEventArgs("CustomSort"));
		}
	}

	internal SortDescriptionCollection SortDescriptionsInternal => _sort;

	/// <summary>Occurs when a property value changes.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			PropertyChanged += value;
		}
		remove
		{
			PropertyChanged -= value;
		}
	}

	/// <summary>Occurs when a property value changes.</summary>
	protected virtual event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.GroupDescription" /> class.</summary>
	protected GroupDescription()
	{
		_explicitGroupNames = new ObservableCollection<object>();
		_explicitGroupNames.CollectionChanged += OnGroupNamesChanged;
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.GroupDescription.PropertyChanged" /> event.</summary>
	/// <param name="e">Arguments of the event being raised.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}

	/// <summary>Returns whether serialization processes should serialize the effective value of the <see cref="P:System.ComponentModel.GroupDescription.GroupNames" /> property on instances of this class.</summary>
	/// <returns>Returns true if the <see cref="P:System.ComponentModel.GroupDescription.GroupNames" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeGroupNames()
	{
		return _explicitGroupNames.Count > 0;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeSortDescriptions()
	{
		if (_sort != null)
		{
			return _sort.Count > 0;
		}
		return false;
	}

	/// <summary>Returns the group name(s) for the given item.</summary>
	/// <returns>The group name(s) for the given item.</returns>
	/// <param name="item">The item to return group names for.</param>
	/// <param name="level">The level of grouping.</param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to supply to the converter.</param>
	public abstract object GroupNameFromItem(object item, int level, CultureInfo culture);

	/// <summary>Returns a value that indicates whether the group name and the item name match such that the item belongs to the group.</summary>
	/// <returns>true if the names match and the item belongs to the group; otherwise, false.</returns>
	/// <param name="groupName">The name of the group to check.</param>
	/// <param name="itemName">The name of the item to check.</param>
	public virtual bool NamesMatch(object groupName, object itemName)
	{
		return object.Equals(groupName, itemName);
	}

	private void OnGroupNamesChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(new PropertyChangedEventArgs("GroupNames"));
	}

	private void SetSortDescriptions(SortDescriptionCollection descriptions)
	{
		if (_sort != null)
		{
			((INotifyCollectionChanged)_sort).CollectionChanged -= SortDescriptionsChanged;
		}
		bool num = _sort != descriptions;
		_sort = descriptions;
		if (_sort != null)
		{
			Invariant.Assert(_sort.Count == 0, "must be empty SortDescription collection");
			((INotifyCollectionChanged)_sort).CollectionChanged += SortDescriptionsChanged;
		}
		if (num)
		{
			OnPropertyChanged(new PropertyChangedEventArgs("SortDescriptions"));
		}
	}

	private void SortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (_sort.Count > 0 && _customSort != null)
		{
			_customSort = null;
			OnPropertyChanged(new PropertyChangedEventArgs("CustomSort"));
		}
		OnPropertyChanged(new PropertyChangedEventArgs("SortDescriptions"));
	}
}
