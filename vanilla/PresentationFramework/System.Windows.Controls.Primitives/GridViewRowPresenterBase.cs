using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Media;
using MS.Internal.Controls;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents the base class for classes that define the layout for a row of data where different data items are displayed in different columns.</summary>
public abstract class GridViewRowPresenterBase : FrameworkElement, IWeakEventListener
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.GridViewRowPresenterBase.Columns" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(GridViewColumnCollection), typeof(GridViewRowPresenterBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, ColumnsPropertyChanged));

	internal const double c_PaddingHeaderMinWidth = 2.0;

	private UIElementCollection _uiElementCollection;

	private bool _needUpdateVisualTree = true;

	private List<double> _desiredWidthList;

	/// <summary>Gets or sets a <see cref="T:System.Windows.Controls.GridViewColumnCollection" />. </summary>
	/// <returns>A collection of <see cref="T:System.Windows.Controls.GridViewColumn" /> objects that display data. The default is null.</returns>
	public GridViewColumnCollection Columns
	{
		get
		{
			return (GridViewColumnCollection)GetValue(ColumnsProperty);
		}
		set
		{
			SetValue(ColumnsProperty, value);
		}
	}

	/// <summary>Gets an enumerator for the logical children of a row.</summary>
	/// <returns>The <see cref="T:System.Collections.IEnumerator" /> for the logical children of this row. </returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			if (InternalChildren.Count == 0)
			{
				return EmptyEnumerator.Instance;
			}
			return InternalChildren.GetEnumerator();
		}
	}

	/// <summary>Gets the number of visual children for a row. </summary>
	/// <returns>The number of visual children for the current row. </returns>
	protected override int VisualChildrenCount
	{
		get
		{
			if (_uiElementCollection == null)
			{
				return 0;
			}
			return _uiElementCollection.Count;
		}
	}

	internal List<double> DesiredWidthList
	{
		get
		{
			return _desiredWidthList;
		}
		private set
		{
			_desiredWidthList = value;
		}
	}

	internal bool NeedUpdateVisualTree
	{
		get
		{
			return _needUpdateVisualTree;
		}
		set
		{
			_needUpdateVisualTree = value;
		}
	}

	internal UIElementCollection InternalChildren
	{
		get
		{
			if (_uiElementCollection == null)
			{
				_uiElementCollection = new UIElementCollection(this, this);
			}
			return _uiElementCollection;
		}
	}

	private bool IsPresenterVisualReady
	{
		get
		{
			if (base.IsInitialized)
			{
				return !NeedUpdateVisualTree;
			}
			return false;
		}
	}

	/// <summary>Returns a string representation of a <see cref="T:System.Windows.Controls.Primitives.GridViewRowPresenterBase" /> object.</summary>
	/// <returns>A string that contains the type of the object and the number of columns.</returns>
	public override string ToString()
	{
		return SR.Format(SR.ToStringFormatString_GridViewRowPresenterBase, GetType(), (Columns != null) ? Columns.Count : 0);
	}

	/// <summary>Gets the visual child in the row item at the specified index.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Visual" /> object that contains the child at the specified index.</returns>
	/// <param name="index">The index of the child.</param>
	protected override Visual GetVisualChild(int index)
	{
		if (_uiElementCollection == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _uiElementCollection[index];
	}

	internal virtual void OnColumnCollectionChanged(GridViewColumnCollectionChangedEventArgs e)
	{
		if (DesiredWidthList == null)
		{
			return;
		}
		if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
		{
			if (DesiredWidthList.Count > e.ActualIndex)
			{
				DesiredWidthList.RemoveAt(e.ActualIndex);
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			DesiredWidthList = null;
		}
	}

	internal abstract void OnColumnPropertyChanged(GridViewColumn column, string propertyName);

	internal void EnsureDesiredWidthList()
	{
		GridViewColumnCollection columns = Columns;
		if (columns != null)
		{
			int count = columns.Count;
			if (DesiredWidthList == null)
			{
				DesiredWidthList = new List<double>(count);
			}
			int num = count - DesiredWidthList.Count;
			for (int i = 0; i < num; i++)
			{
				DesiredWidthList.Add(double.NaN);
			}
		}
	}

	private static void ColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GridViewRowPresenterBase gridViewRowPresenterBase = (GridViewRowPresenterBase)d;
		GridViewColumnCollection gridViewColumnCollection = (GridViewColumnCollection)e.OldValue;
		if (gridViewColumnCollection != null)
		{
			InternalCollectionChangedEventManager.RemoveHandler(gridViewColumnCollection, gridViewRowPresenterBase.ColumnCollectionChanged);
			if (!gridViewColumnCollection.InViewMode && gridViewColumnCollection.Owner == gridViewRowPresenterBase.GetStableAncester())
			{
				gridViewColumnCollection.Owner = null;
			}
		}
		GridViewColumnCollection gridViewColumnCollection2 = (GridViewColumnCollection)e.NewValue;
		if (gridViewColumnCollection2 != null)
		{
			InternalCollectionChangedEventManager.AddHandler(gridViewColumnCollection2, gridViewRowPresenterBase.ColumnCollectionChanged);
			if (!gridViewColumnCollection2.InViewMode && gridViewColumnCollection2.Owner == null)
			{
				gridViewColumnCollection2.Owner = gridViewRowPresenterBase.GetStableAncester();
			}
		}
		gridViewRowPresenterBase.NeedUpdateVisualTree = true;
		gridViewRowPresenterBase.InvalidateMeasure();
	}

	private FrameworkElement GetStableAncester()
	{
		ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(base.TemplatedParent);
		if (itemsControl == null)
		{
			return this;
		}
		return itemsControl;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>true if the listener handled the event.</returns>
	/// <param name="managerType"> The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method.</param>
	/// <param name="sender"> Object that originated the event.</param>
	/// <param name="args"> Event data.</param>
	bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs args)
	{
		return false;
	}

	private void ColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
	{
		if (arg is GridViewColumnCollectionChangedEventArgs gridViewColumnCollectionChangedEventArgs && IsPresenterVisualReady)
		{
			if (gridViewColumnCollectionChangedEventArgs.Column != null)
			{
				OnColumnPropertyChanged(gridViewColumnCollectionChangedEventArgs.Column, gridViewColumnCollectionChangedEventArgs.PropertyName);
			}
			else
			{
				OnColumnCollectionChanged(gridViewColumnCollectionChangedEventArgs);
			}
		}
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Windows.Controls.Primitives.GridViewRowPresenterBase" /> class.</summary>
	protected GridViewRowPresenterBase()
	{
	}
}
