using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal.Documents;
using MS.Internal.PtsTable;

namespace System.Windows.Documents;

/// <summary>A flow content element that defines a row within a <see cref="T:System.Windows.Documents.Table" />.</summary>
[ContentProperty("Cells")]
public class TableRow : TextElement, IAddChild, IIndexedChild<TableRowGroup>, IAcceptInsertion
{
	[Flags]
	private enum Flags
	{
		HasForeignCells = 0x10,
		HasRealCells = 0x20
	}

	private TableCellCollection _cells;

	private TableCell[] _spannedCells;

	private int _parentIndex;

	private int _cellInsertionIndex;

	private int _columnCount;

	private Flags _flags;

	private int _formatCellCount;

	int IIndexedChild<TableRowGroup>.Index
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

	internal TableRowGroup RowGroup => base.Parent as TableRowGroup;

	internal Table Table
	{
		get
		{
			if (RowGroup == null)
			{
				return null;
			}
			return RowGroup.Table;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TableCellCollection" /> that contains cells of a <see cref="T:System.Windows.Documents.TableRow" />. </summary>
	/// <returns>A collection of child cells.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public TableCellCollection Cells => _cells;

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
			return _cellInsertionIndex;
		}
		set
		{
			_cellInsertionIndex = value;
		}
	}

	internal TableCell[] SpannedCells => _spannedCells;

	internal int ColumnCount => _columnCount;

	internal bool HasForeignCells => CheckFlags(Flags.HasForeignCells);

	internal bool HasRealCells => CheckFlags(Flags.HasRealCells);

	internal int FormatCellCount => _formatCellCount;

	internal override bool IsIMEStructuralElement => true;

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.TableRow" /> class.</summary>
	public TableRow()
	{
		PrivateInitialize();
	}

	/// <summary>
	///   <see cref="P:System.Windows.Documents.TableRow.Cells" /> property to add child <see cref="T:System.Windows.Documents.TableCell" /> elements to a <see cref="T:System.Windows.Documents.TableRow" />.</summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value is TableCell item)
		{
			Cells.Add(item);
			return;
		}
		throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(TableCell)), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	internal override void OnNewParent(DependencyObject newParent)
	{
		DependencyObject parent = base.Parent;
		if (newParent != null && !(newParent is TableRowGroup))
		{
			throw new InvalidOperationException(SR.Format(SR.TableInvalidParentNodeType, newParent.GetType().ToString()));
		}
		if (parent != null)
		{
			((TableRowGroup)parent).Rows.InternalRemove(this);
		}
		base.OnNewParent(newParent);
		if (newParent != null)
		{
			((TableRowGroup)newParent).Rows.InternalAdd(this);
		}
	}

	void IIndexedChild<TableRowGroup>.OnEnterParentTree()
	{
		OnEnterParentTree();
	}

	void IIndexedChild<TableRowGroup>.OnExitParentTree()
	{
		OnExitParentTree();
	}

	void IIndexedChild<TableRowGroup>.OnAfterExitParentTree(TableRowGroup parent)
	{
		OnAfterExitParentTree(parent);
	}

	internal void OnEnterParentTree()
	{
		if (Table != null)
		{
			Table.OnStructureChanged();
		}
	}

	internal void OnExitParentTree()
	{
	}

	internal void OnAfterExitParentTree(TableRowGroup rowGroup)
	{
		if (rowGroup.Table != null)
		{
			Table.OnStructureChanged();
		}
	}

	internal void ValidateStructure(RowSpanVector rowSpanVector)
	{
		SetFlags(!rowSpanVector.Empty(), Flags.HasForeignCells);
		SetFlags(value: false, Flags.HasRealCells);
		_formatCellCount = 0;
		_columnCount = 0;
		rowSpanVector.GetFirstAvailableRange(out var firstAvailableIndex, out var firstOccupiedIndex);
		for (int i = 0; i < _cells.Count; i++)
		{
			TableCell tableCell = _cells[i];
			int columnSpan = tableCell.ColumnSpan;
			int rowSpan = tableCell.RowSpan;
			while (firstAvailableIndex + columnSpan > firstOccupiedIndex)
			{
				rowSpanVector.GetNextAvailableRange(out firstAvailableIndex, out firstOccupiedIndex);
			}
			tableCell.ValidateStructure(firstAvailableIndex);
			if (rowSpan > 1)
			{
				rowSpanVector.Register(tableCell);
			}
			else
			{
				_formatCellCount++;
			}
			firstAvailableIndex += columnSpan;
		}
		_columnCount = firstAvailableIndex;
		bool isLastRowOfAnySpan = false;
		rowSpanVector.GetSpanCells(out _spannedCells, out isLastRowOfAnySpan);
		if (_formatCellCount > 0 || isLastRowOfAnySpan)
		{
			SetFlags(value: true, Flags.HasRealCells);
		}
		_formatCellCount += _spannedCells.Length;
	}

	/// <summary>Returns a value that indicates whether or not the effective value of the <see cref="P:System.Windows.Documents.TableRow.Cells" /> property should be serialized during serialization of a <see cref="T:System.Windows.Documents.TableRow" /> object.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Documents.TableRow.Cells" /> property should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeCells()
	{
		return Cells.Count > 0;
	}

	private void PrivateInitialize()
	{
		_cells = new TableCellCollection(this);
		_parentIndex = -1;
		_cellInsertionIndex = -1;
	}

	private void SetFlags(bool value, Flags flags)
	{
		_flags = (value ? (_flags | flags) : (_flags & ~flags));
	}

	private bool CheckFlags(Flags flags)
	{
		return (_flags & flags) == flags;
	}
}
