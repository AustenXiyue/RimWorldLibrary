using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.PtsTable;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class CellParaClient : SubpageParaClient
{
	private double _arrangeHeight;

	private TableParaClient _tableParaClient;

	internal TableCell Cell => CellParagraph.Cell;

	internal Table Table => Cell.Table;

	internal CellParagraph CellParagraph => (CellParagraph)_paragraph;

	internal int ColumnIndex => Cell.ColumnIndex;

	internal double ArrangeHeight
	{
		set
		{
			_arrangeHeight = value;
		}
	}

	internal TableParaClient TableParaClient => _tableParaClient;

	internal CellParaClient(CellParagraph cellParagraph, TableParaClient tableParaClient)
		: base(cellParagraph)
	{
		_tableParaClient = tableParaClient;
	}

	internal void Arrange(int du, int dv, PTS.FSRECT rcTable, FlowDirection tableFlowDirection, PageContext pageContext)
	{
		CalculatedColumn[] calculatedColumns = _tableParaClient.CalculatedColumns;
		double internalCellSpacing = Table.InternalCellSpacing;
		double num = 0.0 - internalCellSpacing;
		int num2 = Cell.ColumnIndex + Cell.ColumnSpan - 1;
		do
		{
			num += calculatedColumns[num2].DurWidth + internalCellSpacing;
		}
		while (--num2 >= ColumnIndex);
		if (tableFlowDirection != base.PageFlowDirection)
		{
			PTS.FSRECT rectPage = pageContext.PageRect;
			PTS.Validate(PTS.FsTransformRectangle(PTS.FlowDirectionToFswdir(base.PageFlowDirection), ref rectPage, ref rcTable, PTS.FlowDirectionToFswdir(tableFlowDirection), out rcTable));
		}
		_rect.u = du + rcTable.u;
		_rect.v = dv + rcTable.v;
		_rect.du = TextDpi.ToTextDpi(num);
		_rect.dv = TextDpi.ToTextDpi(_arrangeHeight);
		if (tableFlowDirection != base.PageFlowDirection)
		{
			PTS.FSRECT rectPage2 = pageContext.PageRect;
			PTS.Validate(PTS.FsTransformRectangle(PTS.FlowDirectionToFswdir(tableFlowDirection), ref rectPage2, ref _rect, PTS.FlowDirectionToFswdir(base.PageFlowDirection), out _rect));
		}
		_flowDirectionParent = tableFlowDirection;
		_flowDirection = (FlowDirection)base.Paragraph.Element.GetValue(FrameworkElement.FlowDirectionProperty);
		_pageContext = pageContext;
		OnArrange();
		if (_paraHandle.Value != IntPtr.Zero)
		{
			PTS.Validate(PTS.FsClearUpdateInfoInSubpage(base.PtsContext.Context, _paraHandle.Value), base.PtsContext);
		}
	}

	internal void ValidateVisual()
	{
		ValidateVisual(PTS.FSKUPDATE.fskupdNew);
	}

	internal void FormatCellFinite(Size subpageSize, nint breakRecordIn, bool isEmptyOk, uint fswdir, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, out PTS.FSFMTR fsfmtr, out int dvrUsed, out nint breakRecordOut)
	{
		if (CellParagraph.StructuralCache.DtrList != null && breakRecordIn != IntPtr.Zero)
		{
			CellParagraph.InvalidateStructure(TextContainerHelper.GetCPFromElement(CellParagraph.StructuralCache.TextContainer, CellParagraph.Element, ElementEdge.BeforeStart));
		}
		PTS.FSPAP fspap = default(PTS.FSPAP);
		CellParagraph.GetParaProperties(ref fspap);
		PTS.FSRECT fsrcToFill = default(PTS.FSRECT);
		fsrcToFill.u = (fsrcToFill.v = 0);
		fsrcToFill.du = TextDpi.ToTextDpi(subpageSize.Width);
		fsrcToFill.dv = TextDpi.ToTextDpi(subpageSize.Height);
		bool condition = breakRecordIn != IntPtr.Zero;
		CellParagraph.FormatParaFinite(this, breakRecordIn, PTS.FromBoolean(condition: true), IntPtr.Zero, PTS.FromBoolean(isEmptyOk), PTS.FromBoolean(condition), fswdir, ref fsrcToFill, null, PTS.FSKCLEAR.fskclearNone, fsksuppresshardbreakbeforefirstparaIn, out fsfmtr, out var pfspara, out breakRecordOut, out dvrUsed, out var _, out var pmcsclientOut, out var _, out var _);
		if (pmcsclientOut != IntPtr.Zero)
		{
			MarginCollapsingState marginCollapsingState = base.PtsContext.HandleToObject(pmcsclientOut) as MarginCollapsingState;
			PTS.ValidateHandle(marginCollapsingState);
			dvrUsed += marginCollapsingState.Margin;
			marginCollapsingState.Dispose();
			pmcsclientOut = IntPtr.Zero;
		}
		_paraHandle.Value = pfspara;
	}

	internal void FormatCellBottomless(uint fswdir, double width, out PTS.FSFMTRBL fmtrbl, out int dvrUsed)
	{
		if (CellParagraph.StructuralCache.DtrList != null)
		{
			CellParagraph.InvalidateStructure(TextContainerHelper.GetCPFromElement(CellParagraph.StructuralCache.TextContainer, CellParagraph.Element, ElementEdge.BeforeStart));
		}
		PTS.FSPAP fspap = default(PTS.FSPAP);
		CellParagraph.GetParaProperties(ref fspap);
		CellParagraph.FormatParaBottomless(this, PTS.FromBoolean(condition: false), fswdir, 0, TextDpi.ToTextDpi(width), 0, null, PTS.FSKCLEAR.fskclearNone, PTS.FromBoolean(condition: true), out fmtrbl, out var pfspara, out dvrUsed, out var _, out var pmcsclientOut, out var _, out var _, out var _);
		if (pmcsclientOut != IntPtr.Zero)
		{
			MarginCollapsingState marginCollapsingState = base.PtsContext.HandleToObject(pmcsclientOut) as MarginCollapsingState;
			PTS.ValidateHandle(marginCollapsingState);
			dvrUsed += marginCollapsingState.Margin;
			marginCollapsingState.Dispose();
			pmcsclientOut = IntPtr.Zero;
		}
		_paraHandle.Value = pfspara;
	}

	internal void UpdateBottomlessCell(uint fswdir, double width, out PTS.FSFMTRBL fmtrbl, out int dvrUsed)
	{
		PTS.FSPAP fspap = default(PTS.FSPAP);
		CellParagraph.GetParaProperties(ref fspap);
		CellParagraph.UpdateBottomlessPara(_paraHandle.Value, this, PTS.FromBoolean(condition: false), fswdir, 0, TextDpi.ToTextDpi(width), 0, null, PTS.FSKCLEAR.fskclearNone, PTS.FromBoolean(condition: true), out fmtrbl, out dvrUsed, out var _, out var pmcsclientOut, out var _, out var _, out var _);
		if (pmcsclientOut != IntPtr.Zero)
		{
			MarginCollapsingState marginCollapsingState = base.PtsContext.HandleToObject(pmcsclientOut) as MarginCollapsingState;
			PTS.ValidateHandle(marginCollapsingState);
			dvrUsed += marginCollapsingState.Margin;
			marginCollapsingState.Dispose();
			pmcsclientOut = IntPtr.Zero;
		}
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition, Rect visibleRect)
	{
		Geometry geometry = null;
		if (endPosition.CompareTo(Cell.StaticElementEnd) >= 0)
		{
			geometry = new RectangleGeometry(_rect.FromTextDpi());
		}
		else
		{
			SubpageParagraphResult subpageParagraphResult = (SubpageParagraphResult)CreateParagraphResult();
			ReadOnlyCollection<ColumnResult> columns = subpageParagraphResult.Columns;
			Transform transform = new TranslateTransform(0.0 - TextDpi.FromTextDpi(base.ContentRect.u), 0.0 - TextDpi.FromTextDpi(base.ContentRect.v));
			visibleRect = transform.TransformBounds(visibleRect);
			transform = null;
			geometry = TextDocumentView.GetTightBoundingGeometryFromTextPositionsHelper(columns[0].Paragraphs, subpageParagraphResult.FloatingElements, startPosition, endPosition, 0.0, visibleRect);
			if (geometry != null)
			{
				Rect viewport = new Rect(0.0, 0.0, TextDpi.FromTextDpi(base.ContentRect.du), TextDpi.FromTextDpi(base.ContentRect.dv));
				CaretElement.ClipGeometryByViewport(ref geometry, viewport);
				transform = new TranslateTransform(TextDpi.FromTextDpi(base.ContentRect.u), TextDpi.FromTextDpi(base.ContentRect.v));
				CaretElement.AddTransformToGeometry(geometry, transform);
			}
		}
		return geometry;
	}

	internal double CalculateCellWidth(TableParaClient tableParaClient)
	{
		CalculatedColumn[] calculatedColumns = tableParaClient.CalculatedColumns;
		double internalCellSpacing = Table.InternalCellSpacing;
		double num = 0.0 - internalCellSpacing;
		int num2 = Cell.ColumnIndex + Cell.ColumnSpan - 1;
		do
		{
			num += calculatedColumns[num2].DurWidth + internalCellSpacing;
		}
		while (--num2 >= Cell.ColumnIndex);
		return num;
	}
}
