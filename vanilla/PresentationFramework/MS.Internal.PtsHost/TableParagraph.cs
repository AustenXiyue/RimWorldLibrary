using System;
using System.Windows;
using System.Windows.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal sealed class TableParagraph : BaseParagraph
{
	private BaseParagraph _firstChild;

	internal Table Table => (Table)base.Element;

	internal TableParagraph(DependencyObject element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
		Table.TableStructureChanged += TableStructureChanged;
	}

	public override void Dispose()
	{
		Table.TableStructureChanged -= TableStructureChanged;
		BaseParagraph baseParagraph = _firstChild;
		while (baseParagraph != null)
		{
			BaseParagraph baseParagraph2 = baseParagraph;
			baseParagraph = baseParagraph.Next;
			baseParagraph2.Dispose();
			baseParagraph2.Next = null;
			baseParagraph2.Previous = null;
		}
		_firstChild = null;
		base.Dispose();
	}

	internal override void CollapseMargin(BaseParaClient paraClient, MarginCollapsingState mcs, uint fswdir, bool suppressTopSpace, out int dvr)
	{
		if (suppressTopSpace && (base.StructuralCache.CurrentFormatContext.FinitePage || mcs == null))
		{
			dvr = 0;
			return;
		}
		MbpInfo mbp = MbpInfo.FromElement(Table, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		MarginCollapsingState mcsNew = null;
		MarginCollapsingState.CollapseTopMargin(base.PtsContext, mbp, mcs, out mcsNew, out dvr);
		if (mcsNew != null)
		{
			dvr = mcsNew.Margin;
			mcsNew.Dispose();
			mcsNew = null;
		}
	}

	internal override void GetParaProperties(ref PTS.FSPAP fspap)
	{
		fspap.idobj = PtsHost.TableParagraphId;
		fspap.fKeepWithNext = 0;
		fspap.fBreakPageBefore = 0;
		fspap.fBreakColumnBefore = 0;
	}

	internal override void CreateParaclient(out nint pfsparaclient)
	{
		TableParaClient tableParaClient = new TableParaClient(this);
		pfsparaclient = tableParaClient.Handle;
	}

	internal void GetTableProperties(uint fswdirTrack, out PTS.FSTABLEOBJPROPS fstableobjprops)
	{
		fstableobjprops = default(PTS.FSTABLEOBJPROPS);
		fstableobjprops.fskclear = PTS.FSKCLEAR.fskclearNone;
		fstableobjprops.ktablealignment = PTS.FSKTABLEOBJALIGNMENT.fsktableobjAlignLeft;
		fstableobjprops.fFloat = 0;
		fstableobjprops.fskwr = PTS.FSKWRAP.fskwrBoth;
		fstableobjprops.fDelayNoProgress = 0;
		fstableobjprops.dvrCaptionTop = 0;
		fstableobjprops.dvrCaptionBottom = 0;
		fstableobjprops.durCaptionLeft = 0;
		fstableobjprops.durCaptionRight = 0;
		fstableobjprops.fswdirTable = PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
	}

	internal void GetMCSClientAfterTable(uint fswdirTrack, nint pmcsclientIn, out nint ppmcsclientOut)
	{
		ppmcsclientOut = IntPtr.Zero;
		MbpInfo mbp = MbpInfo.FromElement(Table, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		MarginCollapsingState mcsCurrent = null;
		if (pmcsclientIn != IntPtr.Zero)
		{
			mcsCurrent = base.PtsContext.HandleToObject(pmcsclientIn) as MarginCollapsingState;
		}
		MarginCollapsingState mcsNew = null;
		MarginCollapsingState.CollapseBottomMargin(base.PtsContext, mbp, mcsCurrent, out mcsNew, out var _);
		if (mcsNew != null)
		{
			ppmcsclientOut = mcsNew.Handle;
		}
	}

	internal void GetFirstHeaderRow(int fRepeatedHeader, out int fFound, out nint pnmFirstHeaderRow)
	{
		fFound = 0;
		pnmFirstHeaderRow = IntPtr.Zero;
	}

	internal void GetNextHeaderRow(int fRepeatedHeader, nint nmHeaderRow, out int fFound, out nint pnmNextHeaderRow)
	{
		fFound = 0;
		pnmNextHeaderRow = IntPtr.Zero;
	}

	internal void GetFirstFooterRow(int fRepeatedFooter, out int fFound, out nint pnmFirstFooterRow)
	{
		fFound = 0;
		pnmFirstFooterRow = IntPtr.Zero;
	}

	internal void GetNextFooterRow(int fRepeatedFooter, nint nmFooterRow, out int fFound, out nint pnmNextFooterRow)
	{
		fFound = 0;
		pnmNextFooterRow = IntPtr.Zero;
	}

	internal void GetFirstRow(out int fFound, out nint pnmFirstRow)
	{
		if (_firstChild == null)
		{
			TableRow tableRow = null;
			for (int i = 0; i < Table.RowGroups.Count; i++)
			{
				if (tableRow != null)
				{
					break;
				}
				TableRowGroup tableRowGroup = Table.RowGroups[i];
				if (tableRowGroup.Rows.Count > 0)
				{
					tableRow = tableRowGroup.Rows[0];
					Invariant.Assert(tableRow.Index != -1);
				}
			}
			if (tableRow != null)
			{
				_firstChild = new RowParagraph(tableRow, base.StructuralCache);
				((RowParagraph)_firstChild).CalculateRowSpans();
			}
		}
		if (_firstChild != null)
		{
			fFound = 1;
			pnmFirstRow = _firstChild.Handle;
		}
		else
		{
			fFound = 0;
			pnmFirstRow = IntPtr.Zero;
		}
	}

	internal void GetNextRow(nint nmRow, out int fFound, out nint pnmNextRow)
	{
		BaseParagraph baseParagraph = (RowParagraph)base.PtsContext.HandleToObject(nmRow);
		BaseParagraph baseParagraph2 = baseParagraph.Next;
		if (baseParagraph2 == null)
		{
			TableRow row = ((RowParagraph)baseParagraph).Row;
			TableRowGroup rowGroup = row.RowGroup;
			TableRow tableRow = null;
			int num = row.Index + 1;
			int num2 = rowGroup.Index + 1;
			if (num < rowGroup.Rows.Count)
			{
				tableRow = rowGroup.Rows[num];
			}
			while (tableRow == null && num2 != Table.RowGroups.Count)
			{
				TableRowCollection rows = Table.RowGroups[num2].Rows;
				if (rows.Count > 0)
				{
					tableRow = rows[0];
				}
				num2++;
			}
			if (tableRow != null)
			{
				baseParagraph2 = (baseParagraph.Next = new RowParagraph(tableRow, base.StructuralCache));
				baseParagraph2.Previous = baseParagraph;
				((RowParagraph)baseParagraph2).CalculateRowSpans();
			}
		}
		if (baseParagraph2 != null)
		{
			fFound = 1;
			pnmNextRow = baseParagraph2.Handle;
		}
		else
		{
			fFound = 0;
			pnmNextRow = IntPtr.Zero;
		}
	}

	internal void UpdFChangeInHeaderFooter(out int fHeaderChanged, out int fFooterChanged, out int fRepeatedHeaderChanged, out int fRepeatedFooterChanged)
	{
		fHeaderChanged = 0;
		fRepeatedHeaderChanged = 0;
		fFooterChanged = 0;
		fRepeatedFooterChanged = 0;
	}

	internal void UpdGetFirstChangeInTable(out int fFound, out int fChangeFirst, out nint pnmRowBeforeChange)
	{
		fFound = 1;
		fChangeFirst = 1;
		pnmRowBeforeChange = IntPtr.Zero;
	}

	internal void GetDistributionKind(uint fswdirTable, out PTS.FSKTABLEHEIGHTDISTRIBUTION tabledistr)
	{
		tabledistr = PTS.FSKTABLEHEIGHTDISTRIBUTION.fskdistributeUnchanged;
	}

	internal override void UpdGetParaChange(out PTS.FSKCHANGE fskch, out int fNoFurtherChanges)
	{
		base.UpdGetParaChange(out fskch, out fNoFurtherChanges);
		fskch = PTS.FSKCHANGE.fskchNew;
	}

	internal override bool InvalidateStructure(int startPosition)
	{
		bool result = true;
		for (RowParagraph rowParagraph = _firstChild as RowParagraph; rowParagraph != null; rowParagraph = rowParagraph.Next as RowParagraph)
		{
			if (!InvalidateRowStructure(rowParagraph, startPosition))
			{
				result = false;
			}
		}
		return result;
	}

	internal override void InvalidateFormatCache()
	{
		for (RowParagraph rowParagraph = _firstChild as RowParagraph; rowParagraph != null; rowParagraph = rowParagraph.Next as RowParagraph)
		{
			InvalidateRowFormatCache(rowParagraph);
		}
	}

	private bool InvalidateRowStructure(RowParagraph rowParagraph, int startPosition)
	{
		bool result = true;
		for (int i = 0; i < rowParagraph.Cells.Length; i++)
		{
			CellParagraph cellParagraph = rowParagraph.Cells[i];
			if (cellParagraph.ParagraphEndCharacterPosition < startPosition || !cellParagraph.InvalidateStructure(startPosition))
			{
				result = false;
			}
		}
		return result;
	}

	private void InvalidateRowFormatCache(RowParagraph rowParagraph)
	{
		for (int i = 0; i < rowParagraph.Cells.Length; i++)
		{
			rowParagraph.Cells[i].InvalidateFormatCache();
		}
	}

	private void TableStructureChanged(object sender, EventArgs e)
	{
		for (BaseParagraph baseParagraph = _firstChild; baseParagraph != null; baseParagraph = baseParagraph.Next)
		{
			baseParagraph.Dispose();
		}
		_firstChild = null;
		int num = Table.SymbolCount - 2;
		if (num > 0)
		{
			DirtyTextRange dtr = new DirtyTextRange(Table.ContentStartOffset, num, num);
			base.StructuralCache.AddDirtyTextRange(dtr);
		}
		if (base.StructuralCache.FormattingOwner.Formatter != null)
		{
			base.StructuralCache.FormattingOwner.Formatter.OnContentInvalidated(affectsLayout: true, Table.ContentStart, Table.ContentEnd);
		}
	}
}
