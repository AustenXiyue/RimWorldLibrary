using System.Collections;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Markup;
using MS.Internal.Documents;

namespace System.Windows.Documents;

/// <summary>A block-level flow content element that provides a grid-based presentation organized by rows and columns.</summary>
[ContentProperty("RowGroups")]
public class Table : Block, IAddChild, IAcceptInsertion
{
	private class TableChildrenCollectionEnumeratorSimple : IEnumerator, ICloneable
	{
		private enum ChildrenTypes
		{
			BeforeFirst,
			Columns,
			RowGroups,
			AfterLast
		}

		private Table _table;

		private int _version;

		private IEnumerator _columns;

		private IEnumerator _rowGroups;

		private ChildrenTypes _currentChildType;

		private object _currentChild;

		public object Current
		{
			get
			{
				if (_currentChildType == ChildrenTypes.BeforeFirst)
				{
					throw new InvalidOperationException(SR.EnumeratorNotStarted);
				}
				if (_currentChildType == ChildrenTypes.AfterLast)
				{
					throw new InvalidOperationException(SR.EnumeratorReachedEnd);
				}
				return _currentChild;
			}
		}

		internal TableChildrenCollectionEnumeratorSimple(Table table)
		{
			_table = table;
			_version = _table._version;
			_columns = ((IEnumerable)_table._columns).GetEnumerator();
			_rowGroups = ((IEnumerable)_table._rowGroups).GetEnumerator();
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public bool MoveNext()
		{
			if (_version != _table._version)
			{
				throw new InvalidOperationException(SR.EnumeratorVersionChanged);
			}
			if (_currentChildType != ChildrenTypes.Columns && _currentChildType != ChildrenTypes.RowGroups)
			{
				_currentChildType++;
			}
			object obj = null;
			while (_currentChildType < ChildrenTypes.AfterLast)
			{
				switch (_currentChildType)
				{
				case ChildrenTypes.Columns:
					if (_columns.MoveNext())
					{
						obj = _columns.Current;
					}
					break;
				case ChildrenTypes.RowGroups:
					if (_rowGroups.MoveNext())
					{
						obj = _rowGroups.Current;
					}
					break;
				}
				if (obj != null)
				{
					_currentChild = obj;
					break;
				}
				_currentChildType++;
			}
			return _currentChildType != ChildrenTypes.AfterLast;
		}

		public void Reset()
		{
			if (_version != _table._version)
			{
				throw new InvalidOperationException(SR.EnumeratorVersionChanged);
			}
			_columns.Reset();
			_rowGroups.Reset();
			_currentChildType = ChildrenTypes.BeforeFirst;
			_currentChild = null;
		}
	}

	private TableColumnCollection _columns;

	private TableRowGroupCollection _rowGroups;

	private int _rowGroupInsertionIndex;

	private const double c_defaultCellSpacing = 2.0;

	private int _columnCount;

	private int _version;

	private bool _initializing;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Table.CellSpacing" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Table.CellSpacing" /> dependency property.</returns>
	public static readonly DependencyProperty CellSpacingProperty;

	/// <summary>Gets an enumerator that can be used to iterate the logical children of the <see cref="T:System.Windows.Documents.Table" />.</summary>
	/// <returns>An enumerator for the logical children of the <see cref="T:System.Windows.Documents.Table" />.</returns>
	protected internal override IEnumerator LogicalChildren => new TableChildrenCollectionEnumeratorSimple(this);

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TableColumnCollection" /> object that contains the columns hosted by the table.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TableColumnCollection" /> object that contains the columns (represented by <see cref="T:System.Windows.Documents.TableColumn" /> objects) hosted by the table. Note that this number might not be the actual number of columns rendered in the table. It is the <see cref="T:System.Windows.Documents.TableCell" /> objects in a table that determine how many columns are actually rendered.This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public TableColumnCollection Columns => _columns;

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TableRowGroupCollection" /> collection object that contains the row groups hosted by the table.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TableRowGroupCollection" /> collection object that contains the row groups (represented by <see cref="T:System.Windows.Documents.TableRowGroup" /> objects) hosted by the table.This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public TableRowGroupCollection RowGroups => _rowGroups;

	/// <summary>Gets or sets the amount of spacing between cells in a table.  </summary>
	/// <returns>The amount of spacing between cells in a table, in device independent pixels.The default value is 2.0.</returns>
	[TypeConverter(typeof(LengthConverter))]
	public double CellSpacing
	{
		get
		{
			return (double)GetValue(CellSpacingProperty);
		}
		set
		{
			SetValue(CellSpacingProperty, value);
		}
	}

	internal double InternalCellSpacing => Math.Max(CellSpacing, 0.0);

	int IAcceptInsertion.InsertionIndex
	{
		get
		{
			return InsertionIndex;
		}
		set
		{
			InsertionIndex = value;
		}
	}

	internal int InsertionIndex
	{
		get
		{
			return _rowGroupInsertionIndex;
		}
		set
		{
			_rowGroupInsertionIndex = value;
		}
	}

	internal int ColumnCount => _columnCount;

	internal event EventHandler TableStructureChanged;

	static Table()
	{
		CellSpacingProperty = DependencyProperty.Register("CellSpacing", typeof(double), typeof(Table), new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidCellSpacing);
		Block.MarginProperty.OverrideMetadata(typeof(Table), new FrameworkPropertyMetadata(new Thickness(double.NaN)));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Table" /> class.</summary>
	public Table()
	{
		PrivateInitialize();
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value is TableRowGroup item)
		{
			RowGroups.Add(item);
			return;
		}
		throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(TableRowGroup)), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	public override void BeginInit()
	{
		base.BeginInit();
		_initializing = true;
	}

	public override void EndInit()
	{
		_initializing = false;
		OnStructureChanged();
		base.EndInit();
	}

	/// <summary>Gets a value that indicates whether or not the effective value of the <see cref="P:System.Windows.Documents.Table.Columns" /> property should be serialized during serialization of a <see cref="T:System.Windows.Documents.Table" /> object.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Documents.Table.Columns" /> property should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeColumns()
	{
		return Columns.Count > 0;
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Documents.Table" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Documents.Table" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new TableAutomationPeer(this);
	}

	internal void EnsureColumnCount(int columnCount)
	{
		if (_columnCount < columnCount)
		{
			_columnCount = columnCount;
		}
	}

	internal void OnStructureChanged()
	{
		if (!_initializing)
		{
			if (this.TableStructureChanged != null)
			{
				this.TableStructureChanged(this, EventArgs.Empty);
			}
			ValidateStructure();
			if (ContentElementAutomationPeer.FromElement(this) is TableAutomationPeer tableAutomationPeer)
			{
				tableAutomationPeer.OnStructureInvalidated();
			}
		}
	}

	internal void ValidateStructure()
	{
		if (!_initializing)
		{
			_columnCount = 0;
			for (int i = 0; i < _rowGroups.Count; i++)
			{
				_rowGroups[i].ValidateStructure();
			}
			_version++;
		}
	}

	internal void InvalidateColumns()
	{
		NotifyTypographicPropertyChanged(affectsMeasureOrArrange: true, localValueChanged: true, null);
	}

	internal bool IsFirstNonEmptyRowGroup(int rowGroupIndex)
	{
		for (rowGroupIndex--; rowGroupIndex >= 0; rowGroupIndex--)
		{
			if (RowGroups[rowGroupIndex].Rows.Count > 0)
			{
				return false;
			}
		}
		return true;
	}

	internal bool IsLastNonEmptyRowGroup(int rowGroupIndex)
	{
		for (rowGroupIndex++; rowGroupIndex < RowGroups.Count; rowGroupIndex++)
		{
			if (RowGroups[rowGroupIndex].Rows.Count > 0)
			{
				return false;
			}
		}
		return true;
	}

	private void PrivateInitialize()
	{
		_columns = new TableColumnCollection(this);
		_rowGroups = new TableRowGroupCollection(this);
		_rowGroupInsertionIndex = -1;
	}

	private static bool IsValidCellSpacing(object o)
	{
		double num = (double)o;
		double num2 = Math.Min(1000000, 3500000);
		if (double.IsNaN(num))
		{
			return false;
		}
		if (num < 0.0 || num > num2)
		{
			return false;
		}
		return true;
	}
}
