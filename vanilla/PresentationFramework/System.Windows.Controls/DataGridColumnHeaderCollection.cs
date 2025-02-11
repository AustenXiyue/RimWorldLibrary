using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.Windows.Controls;

internal class DataGridColumnHeaderCollection : IEnumerable, INotifyCollectionChanged, IDisposable
{
	private class ColumnHeaderCollectionEnumerator : IEnumerator, IDisposable
	{
		private int _current;

		private bool _columnsChanged;

		private ObservableCollection<DataGridColumn> _columns;

		public object Current
		{
			get
			{
				if (IsValid)
				{
					return _columns[_current]?.Header;
				}
				throw new InvalidOperationException();
			}
		}

		private bool HasChanged => _columnsChanged;

		private bool IsValid
		{
			get
			{
				if (_columns != null && _current >= 0 && _current < _columns.Count)
				{
					return !HasChanged;
				}
				return false;
			}
		}

		public ColumnHeaderCollectionEnumerator(ObservableCollection<DataGridColumn> columns)
		{
			if (columns != null)
			{
				_columns = columns;
				_columns.CollectionChanged += OnColumnsChanged;
			}
			_current = -1;
		}

		public bool MoveNext()
		{
			if (HasChanged)
			{
				throw new InvalidOperationException();
			}
			if (_columns != null && _current < _columns.Count - 1)
			{
				_current++;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			if (HasChanged)
			{
				throw new InvalidOperationException();
			}
			_current = -1;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			if (_columns != null)
			{
				_columns.CollectionChanged -= OnColumnsChanged;
			}
		}

		private void OnColumnsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_columnsChanged = true;
		}
	}

	private ObservableCollection<DataGridColumn> _columns;

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	public DataGridColumnHeaderCollection(ObservableCollection<DataGridColumn> columns)
	{
		_columns = columns;
		if (_columns != null)
		{
			_columns.CollectionChanged += OnColumnsChanged;
		}
	}

	public DataGridColumn ColumnFromIndex(int index)
	{
		if (index >= 0 && index < _columns.Count)
		{
			return _columns[index];
		}
		return null;
	}

	internal void NotifyHeaderPropertyChanged(DataGridColumn column, DependencyPropertyChangedEventArgs e)
	{
		NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue, _columns.IndexOf(column));
		FireCollectionChanged(args);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		if (_columns != null)
		{
			_columns.CollectionChanged -= OnColumnsChanged;
		}
	}

	public IEnumerator GetEnumerator()
	{
		return new ColumnHeaderCollectionEnumerator(_columns);
	}

	private void OnColumnsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		FireCollectionChanged(e.Action switch
		{
			NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(e.Action, HeadersFromColumns(e.NewItems), e.NewStartingIndex), 
			NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(e.Action, HeadersFromColumns(e.OldItems), e.OldStartingIndex), 
			NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(e.Action, HeadersFromColumns(e.OldItems), e.NewStartingIndex, e.OldStartingIndex), 
			NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(e.Action, HeadersFromColumns(e.NewItems), HeadersFromColumns(e.OldItems), e.OldStartingIndex), 
			_ => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), 
		});
	}

	private void FireCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, args);
		}
	}

	private static object[] HeadersFromColumns(IList columns)
	{
		object[] array = new object[columns.Count];
		for (int i = 0; i < columns.Count; i++)
		{
			if (columns[i] is DataGridColumn dataGridColumn)
			{
				array[i] = dataGridColumn.Header;
			}
			else
			{
				array[i] = null;
			}
		}
		return array;
	}
}
