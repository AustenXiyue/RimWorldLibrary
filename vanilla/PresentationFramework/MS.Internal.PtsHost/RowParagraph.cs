using System;
using System.Windows;
using System.Windows.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class RowParagraph : BaseParagraph
{
	private CellParagraph[] _cellParagraphs;

	private CellParagraph[] _spannedCells;

	internal TableRow Row => (TableRow)base.Element;

	internal Table Table => Row.Table;

	internal CellParagraph[] Cells => _cellParagraphs;

	internal RowParagraph(DependencyObject element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
	}

	public override void Dispose()
	{
		if (_cellParagraphs != null)
		{
			for (int i = 0; i < _cellParagraphs.Length; i++)
			{
				_cellParagraphs[i].Dispose();
			}
		}
		_cellParagraphs = null;
		base.Dispose();
	}

	internal override void GetParaProperties(ref PTS.FSPAP fspap)
	{
		Invariant.Assert(condition: false);
	}

	internal override void CreateParaclient(out nint paraClientHandle)
	{
		Invariant.Assert(condition: false);
		paraClientHandle = IntPtr.Zero;
	}

	internal override void UpdGetParaChange(out PTS.FSKCHANGE fskch, out int fNoFurtherChanges)
	{
		base.UpdGetParaChange(out fskch, out fNoFurtherChanges);
		fskch = PTS.FSKCHANGE.fskchNew;
	}

	internal void GetRowProperties(uint fswdirTable, out PTS.FSTABLEROWPROPS rowprops)
	{
		bool num = Row.Index == Row.RowGroup.Rows.Count - 1;
		GetRowHeight(out var fskrowheight, out var dvrAboveBelow);
		rowprops = default(PTS.FSTABLEROWPROPS);
		rowprops.fskrowbreak = PTS.FSKROWBREAKRESTRICTION.fskrowbreakAnywhere;
		rowprops.fskrowheight = fskrowheight;
		rowprops.dvrRowHeightRestriction = 0;
		rowprops.dvrAboveRow = dvrAboveBelow;
		rowprops.dvrBelowRow = dvrAboveBelow;
		int num2 = TextDpi.ToTextDpi(Table.InternalCellSpacing);
		MbpInfo mbpInfo = MbpInfo.FromElement(Table, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (Row.Index == 0 && Table.IsFirstNonEmptyRowGroup(Row.RowGroup.Index))
		{
			rowprops.dvrAboveTopRow = mbpInfo.BPTop + num2 / 2;
		}
		else
		{
			rowprops.dvrAboveTopRow = dvrAboveBelow;
		}
		if (num && Table.IsLastNonEmptyRowGroup(Row.RowGroup.Index))
		{
			rowprops.dvrBelowBottomRow = mbpInfo.BPBottom + num2 / 2;
		}
		else
		{
			rowprops.dvrBelowBottomRow = dvrAboveBelow;
		}
		rowprops.dvrAboveRowBreak = num2 / 2;
		rowprops.dvrBelowRowBreak = num2 / 2;
		rowprops.cCells = Row.FormatCellCount;
	}

	internal void FInterruptFormattingTable(int dvr, out int fInterrupt)
	{
		fInterrupt = 0;
	}

	internal unsafe void CalcHorizontalBBoxOfRow(int cCells, nint* rgnmCell, nint* rgpfsCell, out int urBBox, out int durBBox)
	{
		urBBox = 0;
		durBBox = 0;
		for (int i = 0; i < cCells; i++)
		{
			if (rgpfsCell[i] != IntPtr.Zero)
			{
				CellParaClient cellParaClient = base.PtsContext.HandleToObject(rgpfsCell[i]) as CellParaClient;
				PTS.ValidateHandle(cellParaClient);
				durBBox = TextDpi.ToTextDpi(cellParaClient.TableParaClient.TableDesiredWidth);
				break;
			}
		}
	}

	internal unsafe void GetCells(int cCells, nint* rgnmCell, PTS.FSTABLEKCELLMERGE* rgkcellmerge)
	{
		Invariant.Assert(cCells == Row.FormatCellCount);
		Invariant.Assert(cCells >= Row.Cells.Count);
		int num = 0;
		for (int i = 0; i < Row.Cells.Count; i++)
		{
			if (Row.Cells[i].RowSpan == 1)
			{
				rgnmCell[num] = _cellParagraphs[i].Handle;
				rgkcellmerge[num] = PTS.FSTABLEKCELLMERGE.fskcellmergeNo;
				num++;
			}
		}
		Invariant.Assert(cCells == num + _spannedCells.Length);
		if (_spannedCells.Length == 0)
		{
			return;
		}
		bool flag = Row.Index == Row.RowGroup.Rows.Count - 1;
		for (int j = 0; j < _spannedCells.Length; j++)
		{
			TableCell cell = _spannedCells[j].Cell;
			rgnmCell[num] = _spannedCells[j].Handle;
			if (cell.RowIndex == Row.Index)
			{
				rgkcellmerge[num] = ((!flag) ? PTS.FSTABLEKCELLMERGE.fskcellmergeFirst : PTS.FSTABLEKCELLMERGE.fskcellmergeNo);
			}
			else if (Row.Index - cell.RowIndex + 1 < cell.RowSpan)
			{
				rgkcellmerge[num] = (flag ? PTS.FSTABLEKCELLMERGE.fskcellmergeLast : PTS.FSTABLEKCELLMERGE.fskcellmergeMiddle);
			}
			else
			{
				rgkcellmerge[num] = PTS.FSTABLEKCELLMERGE.fskcellmergeLast;
			}
			num++;
		}
	}

	internal void CalculateRowSpans()
	{
		RowParagraph previousRow = null;
		if (Row.Index != 0 && Previous != null)
		{
			previousRow = (RowParagraph)Previous;
		}
		Invariant.Assert(_cellParagraphs == null);
		_cellParagraphs = new CellParagraph[Row.Cells.Count];
		for (int i = 0; i < Row.Cells.Count; i++)
		{
			_cellParagraphs[i] = new CellParagraph(Row.Cells[i], base.StructuralCache);
		}
		Invariant.Assert(_spannedCells == null);
		if (Row.SpannedCells != null)
		{
			_spannedCells = new CellParagraph[Row.SpannedCells.Length];
		}
		else
		{
			_spannedCells = Array.Empty<CellParagraph>();
		}
		for (int j = 0; j < _spannedCells.Length; j++)
		{
			_spannedCells[j] = FindCellParagraphForCell(previousRow, Row.SpannedCells[j]);
		}
	}

	internal void GetRowHeight(out PTS.FSKROWHEIGHTRESTRICTION fskrowheight, out int dvrAboveBelow)
	{
		bool flag = Row.Index == Row.RowGroup.Rows.Count - 1;
		if (Row.HasRealCells || (flag && _spannedCells.Length != 0))
		{
			fskrowheight = PTS.FSKROWHEIGHTRESTRICTION.fskrowheightNatural;
			dvrAboveBelow = TextDpi.ToTextDpi(Table.InternalCellSpacing / 2.0);
		}
		else
		{
			fskrowheight = PTS.FSKROWHEIGHTRESTRICTION.fskrowheightExactNoBreak;
			dvrAboveBelow = 0;
		}
	}

	private CellParagraph FindCellParagraphForCell(RowParagraph previousRow, TableCell cell)
	{
		for (int i = 0; i < _cellParagraphs.Length; i++)
		{
			if (_cellParagraphs[i].Cell == cell)
			{
				return _cellParagraphs[i];
			}
		}
		if (previousRow != null)
		{
			for (int j = 0; j < previousRow._spannedCells.Length; j++)
			{
				if (previousRow._spannedCells[j].Cell == cell)
				{
					return previousRow._spannedCells[j];
				}
			}
		}
		Invariant.Assert(condition: false, "Structural integrity for table not correct.");
		return null;
	}
}
