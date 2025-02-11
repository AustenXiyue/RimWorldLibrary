using System.Windows.Controls;
using System.Windows.Media;
using MS.Internal.Documents;

namespace System.Windows.Documents;

/// <summary>A flow content element that defines a column within a <see cref="T:System.Windows.Documents.Table" />.</summary>
public class TableColumn : FrameworkContentElement, IIndexedChild<Table>
{
	private int _parentIndex;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableColumn.Width" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableColumn.Width" /> dependency property.</returns>
	public static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width", typeof(GridLength), typeof(TableColumn), new FrameworkPropertyMetadata(new GridLength(0.0, GridUnitType.Auto), FrameworkPropertyMetadataOptions.AffectsMeasure, OnWidthChanged), IsValidWidth);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableColumn.Background" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableColumn.Background" /> dependency property.</returns>
	public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(TableColumn), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnBackgroundChanged));

	/// <summary>Gets or sets the width of a <see cref="T:System.Windows.Documents.TableColumn" /> element. The <see cref="P:System.Windows.Documents.TableColumn.Width" /> property measures the sum of the <see cref="T:System.Windows.Documents.TableColumn" /> content, padding, and border from side to side.  </summary>
	/// <returns>The width of the <see cref="T:System.Windows.Documents.TableColumn" /> element, as a <see cref="T:System.Windows.GridLength" />.</returns>
	public GridLength Width
	{
		get
		{
			return (GridLength)GetValue(WidthProperty);
		}
		set
		{
			SetValue(WidthProperty, value);
		}
	}

	/// <summary>Gets or sets the background <see cref="T:System.Windows.Media.Brush" /> used to fill the content of the <see cref="T:System.Windows.Documents.TableColumn" />.  </summary>
	/// <returns>The background <see cref="T:System.Windows.Media.Brush" /> used to fill the content of the <see cref="T:System.Windows.Documents.TableColumn" />.</returns>
	public Brush Background
	{
		get
		{
			return (Brush)GetValue(BackgroundProperty);
		}
		set
		{
			SetValue(BackgroundProperty, value);
		}
	}

	int IIndexedChild<Table>.Index
	{
		get
		{
			return Index;
		}
		set
		{
			Index = value;
		}
	}

	internal Table Table => base.Parent as Table;

	internal int Index
	{
		get
		{
			return _parentIndex;
		}
		set
		{
			_parentIndex = value;
		}
	}

	internal static GridLength DefaultWidth => new GridLength(0.0, GridUnitType.Auto);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.TableColumn" /> class. </summary>
	public TableColumn()
	{
		_parentIndex = -1;
	}

	void IIndexedChild<Table>.OnEnterParentTree()
	{
		OnEnterParentTree();
	}

	void IIndexedChild<Table>.OnExitParentTree()
	{
		OnExitParentTree();
	}

	void IIndexedChild<Table>.OnAfterExitParentTree(Table parent)
	{
	}

	internal void OnEnterParentTree()
	{
		Table.InvalidateColumns();
	}

	internal void OnExitParentTree()
	{
		Table.InvalidateColumns();
	}

	private static bool IsValidWidth(object value)
	{
		GridLength gridLength = (GridLength)value;
		if ((gridLength.GridUnitType == GridUnitType.Pixel || gridLength.GridUnitType == GridUnitType.Star) && gridLength.Value < 0.0)
		{
			return false;
		}
		double num = Math.Min(1000000, 3500000);
		if (gridLength.GridUnitType == GridUnitType.Pixel && gridLength.Value > num)
		{
			return false;
		}
		return true;
	}

	private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TableColumn)d).Table?.InvalidateColumns();
	}

	private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TableColumn)d).Table?.InvalidateColumns();
	}
}
