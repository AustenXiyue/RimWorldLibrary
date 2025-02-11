using System.Collections.ObjectModel;
using System.ComponentModel;

namespace System.Windows.Data;

/// <summary>Represents a group created by a <see cref="T:System.Windows.Data.CollectionView" /> object based on the <see cref="P:System.Windows.Data.CollectionView.GroupDescriptions" />.</summary>
public abstract class CollectionViewGroup : INotifyPropertyChanged
{
	private object _name;

	private ObservableCollection<object> _itemsRW;

	private ReadOnlyObservableCollection<object> _itemsRO;

	private int _itemCount;

	/// <summary>Gets the name of this group.</summary>
	/// <returns>The name of this group which is the common value of the property used to divide items into groups.</returns>
	public object Name => _name;

	/// <summary>Gets the immediate items contained in this group.</summary>
	/// <returns>A read-only collection of the immediate items in this group. This is either a collection of subgroups or a collection of items if this group does not have any subgroups.</returns>
	public ReadOnlyObservableCollection<object> Items => _itemsRO;

	/// <summary>Gets the number of items in the subtree under this group.</summary>
	/// <returns>The number of items (leaves) in the subtree under this group.</returns>
	public int ItemCount => _itemCount;

	/// <summary>Gets a value that indicates whether this group has any subgroups.</summary>
	/// <returns>true if this group is at the bottom level and does not have any subgroups; otherwise, false.</returns>
	public abstract bool IsBottomLevel { get; }

	/// <summary>Gets the immediate items contained in this group.</summary>
	/// <returns>A collection of immediate items in this group. This is either a collection of subgroups or a collection of items if this group does not have any subgroups.</returns>
	protected ObservableCollection<object> ProtectedItems => _itemsRW;

	/// <summary>Gets and sets the number of items in the subtree under this group.</summary>
	/// <returns>The number of items (leaves) in the subtree under this group</returns>
	protected int ProtectedItemCount
	{
		get
		{
			return _itemCount;
		}
		set
		{
			_itemCount = value;
			OnPropertyChanged(new PropertyChangedEventArgs("ItemCount"));
		}
	}

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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.CollectionViewGroup" /> class with the name of the group.</summary>
	/// <param name="name">The name of this group.</param>
	protected CollectionViewGroup(object name)
	{
		_name = name;
		_itemsRW = new ObservableCollection<object>();
		_itemsRO = new ReadOnlyObservableCollection<object>(_itemsRW);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Data.CollectionViewGroup.PropertyChanged" /> event using the provided arguments.</summary>
	/// <param name="e">Arguments of the event being raised.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}
}
