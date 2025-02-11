using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using MS.Internal;
using MS.Internal.Data;
using MS.Internal.Utility;

namespace System.Windows.Data;

/// <summary>Holds an existing collection structure, such as an <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> or a <see cref="T:System.Data.DataSet" />, to be used inside a <see cref="T:System.Windows.Data.CompositeCollection" />.</summary>
public class CollectionContainer : DependencyObject, INotifyCollectionChanged, IWeakEventListener
{
	/// <summary>Identifies the <see cref="P:System.Windows.Data.CollectionContainer.Collection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Data.CollectionContainer.Collection" /> dependency property.</returns>
	public static readonly DependencyProperty CollectionProperty;

	private TraceLog _traceLog;

	private ICollectionView _view;

	private IndexedEnumerable _viewList;

	/// <summary>Gets or sets the collection to add. </summary>
	/// <returns>The collection to add. The default is an empty collection.</returns>
	public IEnumerable Collection
	{
		get
		{
			return (IEnumerable)GetValue(CollectionProperty);
		}
		set
		{
			SetValue(CollectionProperty, value);
		}
	}

	internal ICollectionView View => _view;

	internal int ViewCount
	{
		get
		{
			if (View == null)
			{
				return 0;
			}
			if (View is CollectionView collectionView)
			{
				return collectionView.Count;
			}
			if (View is ICollection collection)
			{
				return collection.Count;
			}
			if (ViewList != null)
			{
				return ViewList.Count;
			}
			return 0;
		}
	}

	internal bool ViewIsEmpty
	{
		get
		{
			if (View == null)
			{
				return true;
			}
			ICollectionView view = View;
			if (view != null)
			{
				return view.IsEmpty;
			}
			if (View is ICollection collection)
			{
				return collection.Count == 0;
			}
			if (ViewList != null)
			{
				return ViewList?.IsEmpty ?? (ViewList.Count == 0);
			}
			return true;
		}
	}

	private IndexedEnumerable ViewList
	{
		get
		{
			if (_viewList == null && View != null)
			{
				_viewList = new IndexedEnumerable(View);
			}
			return _viewList;
		}
	}

	/// <summary>Occurs when the continaed collection has changed.</summary>
	event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
	{
		add
		{
			CollectionChanged += value;
		}
		remove
		{
			CollectionChanged -= value;
		}
	}

	/// <summary>Occurs when the contained collection changes.</summary>
	protected virtual event NotifyCollectionChangedEventHandler CollectionChanged;

	static CollectionContainer()
	{
		CollectionProperty = DependencyProperty.Register("Collection", typeof(IEnumerable), typeof(CollectionContainer), new FrameworkPropertyMetadata(OnCollectionPropertyChanged));
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.CollectionContainer.Collection" /> property should be persisted. </summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeCollection()
	{
		if (Collection == null)
		{
			return false;
		}
		if (Collection is ICollection { Count: 0 })
		{
			return false;
		}
		IEnumerator enumerator = Collection.GetEnumerator();
		bool result = enumerator.MoveNext();
		if (enumerator is IDisposable disposable)
		{
			disposable.Dispose();
		}
		return result;
	}

	internal object ViewItem(int index)
	{
		Invariant.Assert(index >= 0 && View != null);
		if (View is CollectionView collectionView)
		{
			return collectionView.GetItemAt(index);
		}
		if (ViewList != null)
		{
			return ViewList[index];
		}
		return null;
	}

	internal int ViewIndexOf(object item)
	{
		if (View == null)
		{
			return -1;
		}
		if (View is CollectionView collectionView)
		{
			return collectionView.IndexOf(item);
		}
		if (ViewList != null)
		{
			return ViewList.IndexOf(item);
		}
		return -1;
	}

	internal void GetCollectionChangedSources(int level, Action<int, object, bool?, List<string>> format, List<string> sources)
	{
		format(level, this, false, sources);
		if (_view != null)
		{
			if (_view is CollectionView collectionView)
			{
				collectionView.GetCollectionChangedSources(level + 1, format, sources);
			}
			else
			{
				format(level + 1, _view, true, sources);
			}
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Data.CollectionContainer.CollectionChanged" /> event.</summary>
	/// <param name="args">The event data.</param>
	protected virtual void OnContainedCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, args);
		}
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the listener handled the event; otherwise, false.</returns>
	/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method. This only recognizes manager objects of type <see cref="T:System.Collections.Specialized.CollectionChangedEventManager" />.</param>
	/// <param name="sender">Object that originated the event.</param>
	/// <param name="e">Event data.</param>
	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return ReceiveWeakEvent(managerType, sender, e);
	}

	/// <summary>Handles events from the centralized event table.</summary>
	/// <returns>true if the listener handled the event; otherwise, false.</returns>
	/// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method. This only recognizes manager objects of type <see cref="T:System.Collections.Specialized.CollectionChangedEventManager" />.</param>
	/// <param name="sender">The object that originated the event.</param>
	/// <param name="e">The event data.</param>
	protected virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
	{
		return false;
	}

	private static object OnGetCollection(DependencyObject d)
	{
		return ((CollectionContainer)d).Collection;
	}

	private static void OnCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((CollectionContainer)d).HookUpToCollection((IEnumerable)e.NewValue, shouldRaiseChangeEvent: true);
	}

	private void HookUpToCollection(IEnumerable newCollection, bool shouldRaiseChangeEvent)
	{
		_viewList = null;
		if (View != null)
		{
			CollectionChangedEventManager.RemoveHandler(View, OnCollectionChanged);
			if (_traceLog != null)
			{
				_traceLog.Add("Unsubscribe to CollectionChange from {0}", TraceLog.IdFor(View));
			}
		}
		if (newCollection != null)
		{
			_view = CollectionViewSource.GetDefaultCollectionView(newCollection, this);
		}
		else
		{
			_view = null;
		}
		if (View != null)
		{
			CollectionChangedEventManager.AddHandler(View, OnCollectionChanged);
			if (_traceLog != null)
			{
				_traceLog.Add("Subscribe to CollectionChange from {0}", TraceLog.IdFor(View));
			}
		}
		if (shouldRaiseChangeEvent)
		{
			OnContainedCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}

	private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		OnContainedCollectionChanged(e);
	}

	private void InitializeTraceLog()
	{
		_traceLog = new TraceLog(20);
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Windows.Data.CollectionContainer" /> class.</summary>
	public CollectionContainer()
	{
	}
}
