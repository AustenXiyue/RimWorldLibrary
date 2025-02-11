using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal.Documents;
using MS.Internal.PtsTable;

namespace System.Windows.Documents;

/// <summary>Represents a flow content element used to group <see cref="T:System.Windows.Documents.TableRow" /> elements within a <see cref="T:System.Windows.Documents.Table" />.</summary>
[ContentProperty("Rows")]
public class TableRowGroup : TextElement, IAddChild, IIndexedChild<Table>, IAcceptInsertion
{
	private TableRowCollection _rows;

	private int _parentIndex;

	private int _rowInsertionIndex;

	private int _columnCount;

	/// <summary>Gets a <see cref="T:System.Windows.Documents.TableRowCollection" /> that contains the <see cref="T:System.Windows.Documents.TableRow" /> objects that comprise the contents of the <see cref="T:System.Windows.Documents.TableRowGroup" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.TableRowCollection" /> that contains the <see cref="T:System.Windows.Documents.TableRow" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.TableRowGroup" />. This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public TableRowCollection Rows => _rows;

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
			return _rowInsertionIndex;
		}
		set
		{
			_rowInsertionIndex = value;
		}
	}

	internal override bool IsIMEStructuralElement => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.TableRowGroup" /> class.</summary>
	public TableRowGroup()
	{
		Initialize();
	}

	private void Initialize()
	{
		_rows = new TableRowCollection(this);
		_rowInsertionIndex = -1;
		_parentIndex = -1;
	}

	/// <summary>Adds a table row to the <see cref="T:System.Windows.Documents.TableRowGroup" /> collection.</summary>
	/// <param name="value">The <see cref="T:System.Windows.Documents.TableRow" /> to add to the collection.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value is TableRow item)
		{
			Rows.Add(item);
			return;
		}
		throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(TableRow)), "value");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Documents.TableRowGroup.Rows" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeRows()
	{
		return Rows.Count > 0;
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

	internal void OnAfterExitParentTree(Table table)
	{
		table.OnStructureChanged();
	}

	internal void ValidateStructure()
	{
		RowSpanVector rowSpanVector = new RowSpanVector();
		_columnCount = 0;
		for (int i = 0; i < Rows.Count; i++)
		{
			Rows[i].ValidateStructure(rowSpanVector);
			_columnCount = Math.Max(_columnCount, Rows[i].ColumnCount);
		}
		Table.EnsureColumnCount(_columnCount);
	}

	internal override void OnNewParent(DependencyObject newParent)
	{
		DependencyObject parent = base.Parent;
		if (newParent != null && !(newParent is Table))
		{
			throw new InvalidOperationException(SR.Format(SR.TableInvalidParentNodeType, newParent.GetType().ToString()));
		}
		if (parent != null)
		{
			OnExitParentTree();
			((Table)parent).RowGroups.InternalRemove(this);
			OnAfterExitParentTree(parent as Table);
		}
		base.OnNewParent(newParent);
		if (newParent != null)
		{
			((Table)newParent).RowGroups.InternalAdd(this);
			OnEnterParentTree();
		}
	}
}
