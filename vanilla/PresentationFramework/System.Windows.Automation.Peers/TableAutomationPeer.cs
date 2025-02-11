using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Documents.Table" /> types to UI Automation.</summary>
public class TableAutomationPeer : TextElementAutomationPeer, IGridProvider
{
	private int _rowCount;

	private int _columnCount;

	/// <summary>Gets the total number of rows in a grid.</summary>
	/// <returns>The total number of rows in a grid.</returns>
	int IGridProvider.RowCount => _rowCount;

	/// <summary>Gets the total number of columns in a grid.</summary>
	/// <returns>The total number of columns in a grid.</returns>
	int IGridProvider.ColumnCount => _columnCount;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TableAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Documents.Table" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableAutomationPeer" />.</param>
	public TableAutomationPeer(Table owner)
		: base(owner)
	{
		_rowCount = GetRowCount();
		_columnCount = GetColumnCount();
	}

	/// <summary>Gets the control pattern for the <see cref="T:System.Windows.Documents.Table" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableAutomationPeer" />.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Grid" />, this method returns a this pointer; otherwise, this method returns null.</returns>
	/// <param name="patternInterface">A value from the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Grid)
		{
			return this;
		}
		return base.GetPattern(patternInterface);
	}

	/// <summary>Gets the control type for the <see cref="T:System.Windows.Documents.Table" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType" />.</summary>
	/// <returns>The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Table" /> enumeration value.</returns>
	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Table;
	}

	/// <summary>Gets the name of the <see cref="T:System.Windows.Documents.Table" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableAutomationPeer" />. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.</summary>
	/// <returns>A string that contains "Table".</returns>
	protected override string GetClassNameCore()
	{
		return "Table";
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Documents.Table" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TableAutomationPeer" /> is understood by the end user as interactive or as contributing to the logical structure of the control in the GUI. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsControlElement" />.</summary>
	/// <returns>true.</returns>
	protected override bool IsControlElementCore()
	{
		if (!base.IncludeInvisibleElementsInControlView)
		{
			return base.IsTextViewVisible == true;
		}
		return true;
	}

	internal void OnStructureInvalidated()
	{
		int rowCount = GetRowCount();
		if (rowCount != _rowCount)
		{
			RaisePropertyChangedEvent(GridPatternIdentifiers.RowCountProperty, _rowCount, rowCount);
			_rowCount = rowCount;
		}
		int columnCount = GetColumnCount();
		if (columnCount != _columnCount)
		{
			RaisePropertyChangedEvent(GridPatternIdentifiers.ColumnCountProperty, _columnCount, columnCount);
			_columnCount = columnCount;
		}
	}

	private int GetRowCount()
	{
		int num = 0;
		foreach (TableRowGroup item in (IEnumerable<TableRowGroup>)((Table)base.Owner).RowGroups)
		{
			num += item.Rows.Count;
		}
		return num;
	}

	private int GetColumnCount()
	{
		return ((Table)base.Owner).ColumnCount;
	}

	/// <summary>Retrieves the UI Automation provider for the specified cell.</summary>
	/// <returns>The UI Automation provider for the specified cell.</returns>
	/// <param name="row"> The ordinal number of the row of interest.</param>
	/// <param name="column"> The ordinal number of the column of interest.</param>
	IRawElementProviderSimple IGridProvider.GetItem(int row, int column)
	{
		if (row < 0 || row >= ((IGridProvider)this).RowCount)
		{
			throw new ArgumentOutOfRangeException("row");
		}
		if (column < 0 || column >= ((IGridProvider)this).ColumnCount)
		{
			throw new ArgumentOutOfRangeException("column");
		}
		int num = 0;
		foreach (TableRowGroup item in (IEnumerable<TableRowGroup>)((Table)base.Owner).RowGroups)
		{
			if (num + item.Rows.Count < row)
			{
				num += item.Rows.Count;
				continue;
			}
			foreach (TableRow item2 in (IEnumerable<TableRow>)item.Rows)
			{
				if (num == row)
				{
					foreach (TableCell item3 in (IEnumerable<TableCell>)item2.Cells)
					{
						if (item3.ColumnIndex <= column && item3.ColumnIndex + item3.ColumnSpan > column)
						{
							return ProviderFromPeer(ContentElementAutomationPeer.CreatePeerForElement(item3));
						}
					}
					TableCell[] spannedCells = item2.SpannedCells;
					foreach (TableCell tableCell in spannedCells)
					{
						if (tableCell.ColumnIndex <= column && tableCell.ColumnIndex + tableCell.ColumnSpan > column)
						{
							return ProviderFromPeer(ContentElementAutomationPeer.CreatePeerForElement(tableCell));
						}
					}
				}
				else
				{
					num++;
				}
			}
		}
		return null;
	}
}
