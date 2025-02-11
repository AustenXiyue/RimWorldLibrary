using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Windows.Controls;

internal class GridViewColumnCollectionChangedEventArgs : NotifyCollectionChangedEventArgs
{
	private int _actualIndex = -1;

	private ReadOnlyCollection<GridViewColumn> _clearedColumns;

	private GridViewColumn _column;

	private string _propertyName;

	internal int ActualIndex => _actualIndex;

	internal ReadOnlyCollection<GridViewColumn> ClearedColumns => _clearedColumns;

	internal GridViewColumn Column => _column;

	internal string PropertyName => _propertyName;

	internal GridViewColumnCollectionChangedEventArgs(GridViewColumn column, string propertyName)
		: base(NotifyCollectionChangedAction.Reset)
	{
		_column = column;
		_propertyName = propertyName;
	}

	internal GridViewColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, GridViewColumn[] clearedColumns)
		: base(action)
	{
		_clearedColumns = Array.AsReadOnly(clearedColumns);
	}

	internal GridViewColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, GridViewColumn changedItem, int index, int actualIndex)
		: base(action, changedItem, index)
	{
		_actualIndex = actualIndex;
	}

	internal GridViewColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, GridViewColumn newItem, GridViewColumn oldItem, int index, int actualIndex)
		: base(action, newItem, oldItem, index)
	{
		_actualIndex = actualIndex;
	}

	internal GridViewColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, GridViewColumn changedItem, int index, int oldIndex, int actualIndex)
		: base(action, changedItem, index, oldIndex)
	{
		_actualIndex = actualIndex;
	}
}
