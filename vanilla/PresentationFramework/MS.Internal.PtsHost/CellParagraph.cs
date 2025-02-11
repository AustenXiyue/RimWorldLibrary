using System;
using System.Windows;
using System.Windows.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class CellParagraph : SubpageParagraph
{
	internal TableCell Cell => (TableCell)base.Element;

	internal CellParagraph(DependencyObject element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
		_isInterruptible = false;
	}

	internal void FormatCellFinite(TableParaClient tableParaClient, nint pfsbrkcellIn, nint pfsFtnRejector, int fEmptyOK, uint fswdirTable, int dvrAvailable, out PTS.FSFMTR pfmtr, out nint ppfscell, out nint pfsbrkcellOut, out int dvrUsed)
	{
		CellParaClient cellParaClient = new CellParaClient(this, tableParaClient);
		Size subpageSize = new Size(cellParaClient.CalculateCellWidth(tableParaClient), Math.Max(TextDpi.FromTextDpi(dvrAvailable), 0.0));
		cellParaClient.FormatCellFinite(subpageSize, pfsbrkcellIn, PTS.ToBoolean(fEmptyOK), fswdirTable, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA.fsksuppresshardbreakbeforefirstparaNone, out pfmtr, out dvrUsed, out pfsbrkcellOut);
		ppfscell = cellParaClient.Handle;
		if (pfmtr.kstop == PTS.FSFMTRKSTOP.fmtrNoProgressOutOfSpace)
		{
			cellParaClient.Dispose();
			ppfscell = IntPtr.Zero;
			dvrUsed = 0;
		}
		if (dvrAvailable >= dvrUsed)
		{
			return;
		}
		if (PTS.ToBoolean(fEmptyOK))
		{
			cellParaClient?.Dispose();
			if (pfsbrkcellOut != IntPtr.Zero)
			{
				PTS.Validate(PTS.FsDestroySubpageBreakRecord(cellParaClient.PtsContext.Context, pfsbrkcellOut), cellParaClient.PtsContext);
				pfsbrkcellOut = IntPtr.Zero;
			}
			ppfscell = IntPtr.Zero;
			pfmtr.kstop = PTS.FSFMTRKSTOP.fmtrNoProgressOutOfSpace;
			dvrUsed = 0;
		}
		else
		{
			pfmtr.fForcedProgress = 1;
		}
	}

	internal void FormatCellBottomless(TableParaClient tableParaClient, uint fswdirTable, out PTS.FSFMTRBL fmtrbl, out nint ppfscell, out int dvrUsed)
	{
		CellParaClient cellParaClient = new CellParaClient(this, tableParaClient);
		cellParaClient.FormatCellBottomless(fswdirTable, cellParaClient.CalculateCellWidth(tableParaClient), out fmtrbl, out dvrUsed);
		ppfscell = cellParaClient.Handle;
	}

	internal void UpdateBottomlessCell(CellParaClient cellParaClient, TableParaClient tableParaClient, uint fswdirTable, out PTS.FSFMTRBL fmtrbl, out int dvrUsed)
	{
		cellParaClient.UpdateBottomlessCell(fswdirTable, cellParaClient.CalculateCellWidth(tableParaClient), out fmtrbl, out dvrUsed);
	}

	internal void SetCellHeight(CellParaClient cellParaClient, TableParaClient tableParaClient, nint subpageBreakRecord, int fBrokenHere, uint fswdirTable, int dvrActual)
	{
		cellParaClient.ArrangeHeight = TextDpi.FromTextDpi(dvrActual);
	}

	internal void UpdGetCellChange(out int fWidthChanged, out PTS.FSKCHANGE fskchCell)
	{
		fWidthChanged = 1;
		fskchCell = PTS.FSKCHANGE.fskchNew;
	}
}
