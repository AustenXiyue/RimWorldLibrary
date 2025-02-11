using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

/// <summary>A flow content element that defines a cell of content within a <see cref="T:System.Windows.Documents.Table" />.</summary>
[ContentProperty("Blocks")]
public class TableCell : TextElement, IIndexedChild<TableRow>
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableCell.Padding" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableCell.Padding" /> dependency property.</returns>
	public static readonly DependencyProperty PaddingProperty = Block.PaddingProperty.AddOwner(typeof(TableCell), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableCell.BorderThickness" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableCell.BorderThickness" /> dependency property.</returns>
	public static readonly DependencyProperty BorderThicknessProperty = Block.BorderThicknessProperty.AddOwner(typeof(TableCell), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableCell.BorderBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableCell.BorderBrush" /> dependency property.</returns>
	public static readonly DependencyProperty BorderBrushProperty = Block.BorderBrushProperty.AddOwner(typeof(TableCell), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableCell.TextAlignment" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableCell.TextAlignment" /> dependency property.</returns>
	public static readonly DependencyProperty TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(typeof(TableCell));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableCell.FlowDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableCell.FlowDirection" /> dependency property.</returns>
	public static readonly DependencyProperty FlowDirectionProperty = Block.FlowDirectionProperty.AddOwner(typeof(TableCell));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableCell.LineHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableCell.LineHeight" /> dependency property.</returns>
	public static readonly DependencyProperty LineHeightProperty = Block.LineHeightProperty.AddOwner(typeof(TableCell));

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableCell.LineStackingStrategy" />  dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableCell.LineStackingStrategy" /> dependency property.</returns>
	public static readonly DependencyProperty LineStackingStrategyProperty = Block.LineStackingStrategyProperty.AddOwner(typeof(TableCell));

	private int _parentIndex;

	private int _columnIndex;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableCell.ColumnSpan" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableCell.ColumnSpan" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.Register("ColumnSpan", typeof(int), typeof(TableCell), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, OnColumnSpanChanged), IsValidColumnSpan);

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.TableCell.RowSpan" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.TableCell.RowSpan" /> dependency property.</returns>
	public static readonly DependencyProperty RowSpanProperty = DependencyProperty.Register("RowSpan", typeof(int), typeof(TableCell), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, OnRowSpanChanged), IsValidRowSpan);

	/// <summary>Gets a <see cref="T:System.Windows.Documents.BlockCollection" /> containing the top-level <see cref="T:System.Windows.Documents.Block" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.TableCell" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Documents.BlockCollection" /> containing the <see cref="T:System.Windows.Documents.Block" /> elements that comprise the contents of the <see cref="T:System.Windows.Documents.TableCell" />This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public BlockCollection Blocks => new BlockCollection(this, isOwnerParent: true);

	/// <summary>Gets or sets the number of columns that the <see cref="T:System.Windows.Documents.TableCell" /> should span.  </summary>
	/// <returns>The number of columns the <see cref="T:System.Windows.Documents.TableCell" /> should span.The default value is 1 (no spanning).</returns>
	public int ColumnSpan
	{
		get
		{
			return (int)GetValue(ColumnSpanProperty);
		}
		set
		{
			SetValue(ColumnSpanProperty, value);
		}
	}

	/// <summary>Gets or sets the number of rows that the <see cref="T:System.Windows.Documents.TableCell" /> should span.  </summary>
	/// <returns>The number of rows the <see cref="T:System.Windows.Documents.TableCell" /> should span.The default value is 1 (no spanning).</returns>
	public int RowSpan
	{
		get
		{
			return (int)GetValue(RowSpanProperty);
		}
		set
		{
			SetValue(RowSpanProperty, value);
		}
	}

	/// <summary>Gets or sets the padding thickness for the element.  </summary>
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure specifying the amount of padding to apply, in device independent pixels.The default value is a uniform thickness of zero (0.0).</returns>
	public Thickness Padding
	{
		get
		{
			return (Thickness)GetValue(PaddingProperty);
		}
		set
		{
			SetValue(PaddingProperty, value);
		}
	}

	/// <summary>Gets or sets the border thickness for the element.  </summary>
	/// <returns>A <see cref="T:System.Windows.Thickness" /> structure specifying the amount of border to apply, in device independent pixels.The default value is a uniform thickness of zero (0.0).</returns>
	public Thickness BorderThickness
	{
		get
		{
			return (Thickness)GetValue(BorderThicknessProperty);
		}
		set
		{
			SetValue(BorderThicknessProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Brush" /> to use when painting the element's border.  </summary>
	/// <returns>The brush used to apply to the element's border.The default value is a null brush.</returns>
	public Brush BorderBrush
	{
		get
		{
			return (Brush)GetValue(BorderBrushProperty);
		}
		set
		{
			SetValue(BorderBrushProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates the horizontal alignment of text content.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.TextAlignment" /> enumerations specifying the desired alignment.The default value is <see cref="F:System.Windows.TextAlignment.Left" />.</returns>
	public TextAlignment TextAlignment
	{
		get
		{
			return (TextAlignment)GetValue(TextAlignmentProperty);
		}
		set
		{
			SetValue(TextAlignmentProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the relative direction for flow of content within a <see cref="T:System.Windows.Documents.TableCell" /> element.  </summary>
	/// <returns>A member of the <see cref="T:System.Windows.FlowDirection" /> enumeration specifying the relative flow direction.  Getting this property returns the currently effective flow direction.  Setting this property causes the contents of the <see cref="T:System.Windows.Documents.TableCell" /> element to re-flow in the indicated direction.The default value is <see cref="F:System.Windows.FlowDirection.LeftToRight" />.</returns>
	public FlowDirection FlowDirection
	{
		get
		{
			return (FlowDirection)GetValue(FlowDirectionProperty);
		}
		set
		{
			SetValue(FlowDirectionProperty, value);
		}
	}

	/// <summary>Gets or sets the height of each line of content.  </summary>
	/// <returns>A double value specifying the height of line in device independent pixels.  <see cref="P:System.Windows.Documents.TableCell.LineHeight" /> must be equal to or greater than 0.0034 and equal to or less then 160000.A value of <see cref="F:System.Double.NaN" /> (equivalent to an attribute value of "Auto") causes the line height to be determined automatically from the current font characteristics.  The default value is <see cref="F:System.Double.NaN" />.</returns>
	/// <exception cref="T:System.ArgumentException">Raised if an attempt is made to set <see cref="P:System.Windows.Controls.TextBlock.LineHeight" /> to a non-positive value.</exception>
	[TypeConverter(typeof(LengthConverter))]
	public double LineHeight
	{
		get
		{
			return (double)GetValue(LineHeightProperty);
		}
		set
		{
			SetValue(LineHeightProperty, value);
		}
	}

	/// <summary>Gets or sets the mechanism by which a line box is determined for each line of text within the <see cref="T:System.Windows.Documents.TableCell" />.  </summary>
	/// <returns>The mechanism by which a line box is determined for each line of text within the <see cref="T:System.Windows.Documents.TableCell" />. The default value is <see cref="F:System.Windows.LineStackingStrategy.MaxHeight" />.</returns>
	public LineStackingStrategy LineStackingStrategy
	{
		get
		{
			return (LineStackingStrategy)GetValue(LineStackingStrategyProperty);
		}
		set
		{
			SetValue(LineStackingStrategyProperty, value);
		}
	}

	int IIndexedChild<TableRow>.Index
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

	internal TableRow Row => base.Parent as TableRow;

	internal Table Table
	{
		get
		{
			if (Row == null)
			{
				return null;
			}
			return Row.Table;
		}
	}

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

	internal int RowIndex => Row.Index;

	internal int RowGroupIndex => Row.RowGroup.Index;

	internal int ColumnIndex
	{
		get
		{
			return _columnIndex;
		}
		set
		{
			_columnIndex = value;
		}
	}

	internal override bool IsIMEStructuralElement => true;

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Documents.TableCell" /> class.</summary>
	public TableCell()
	{
		PrivateInitialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.TableCell" /> class, taking a specified <see cref="T:System.Windows.Documents.Block" /> object as the initial contents of the new <see cref="T:System.Windows.Documents.TableCell" />.</summary>
	/// <param name="blockItem">A <see cref="T:System.Windows.Documents.Block" /> object specifying the initial contents of the new <see cref="T:System.Windows.Documents.TableCell" />.</param>
	public TableCell(Block blockItem)
	{
		PrivateInitialize();
		if (blockItem == null)
		{
			throw new ArgumentNullException("blockItem");
		}
		Blocks.Add(blockItem);
	}

	internal override void OnNewParent(DependencyObject newParent)
	{
		DependencyObject parent = base.Parent;
		TableRow tableRow = newParent as TableRow;
		if (newParent != null && tableRow == null)
		{
			throw new InvalidOperationException(SR.Format(SR.TableInvalidParentNodeType, newParent.GetType().ToString()));
		}
		if (parent != null)
		{
			((TableRow)parent).Cells.InternalRemove(this);
		}
		base.OnNewParent(newParent);
		if (tableRow != null && tableRow.Cells != null)
		{
			tableRow.Cells.InternalAdd(this);
		}
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Documents.TableCell" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Documents.TableCell" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new TableCellAutomationPeer(this);
	}

	void IIndexedChild<TableRow>.OnEnterParentTree()
	{
		OnEnterParentTree();
	}

	void IIndexedChild<TableRow>.OnExitParentTree()
	{
		OnExitParentTree();
	}

	void IIndexedChild<TableRow>.OnAfterExitParentTree(TableRow parent)
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

	internal void OnAfterExitParentTree(TableRow row)
	{
		if (row.Table != null)
		{
			row.Table.OnStructureChanged();
		}
	}

	internal void ValidateStructure(int columnIndex)
	{
		Invariant.Assert(columnIndex >= 0);
		_columnIndex = columnIndex;
	}

	private void PrivateInitialize()
	{
		_parentIndex = -1;
		_columnIndex = -1;
	}

	private static bool IsValidRowSpan(object value)
	{
		int num = (int)value;
		if (num >= 1)
		{
			return num <= 1000000;
		}
		return false;
	}

	private static bool IsValidColumnSpan(object value)
	{
		int num = (int)value;
		if (num >= 1)
		{
			return num <= 1000;
		}
		return false;
	}

	private static void OnColumnSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TableCell tableCell = (TableCell)d;
		if (tableCell.Table != null)
		{
			tableCell.Table.OnStructureChanged();
		}
		if (ContentElementAutomationPeer.FromElement(tableCell) is TableCellAutomationPeer tableCellAutomationPeer)
		{
			tableCellAutomationPeer.OnColumnSpanChanged((int)e.OldValue, (int)e.NewValue);
		}
	}

	private static void OnRowSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TableCell tableCell = (TableCell)d;
		if (tableCell.Table != null)
		{
			tableCell.Table.OnStructureChanged();
		}
		if (ContentElementAutomationPeer.FromElement(tableCell) is TableCellAutomationPeer tableCellAutomationPeer)
		{
			tableCellAutomationPeer.OnRowSpanChanged((int)e.OldValue, (int)e.NewValue);
		}
	}
}
