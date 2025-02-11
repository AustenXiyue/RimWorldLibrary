using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.PtsTable;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class TableParaClient : BaseParaClient
{
	private struct CellParaClientEntry
	{
		internal CellParaClient cellParaClient;

		internal PTS.FSKUPDATE fskupdCell;
	}

	private bool _isFirstChunk;

	private bool _isLastChunk;

	private PTS.FSRECT _columnRect;

	private CalculatedColumn[] _calculatedColumns;

	private double _durMinWidth;

	private double _durMaxWidth;

	private double _previousAutofitWidth;

	private double _previousTableWidth;

	internal TableParagraph TableParagraph => (TableParagraph)_paragraph;

	internal Table Table => TableParagraph.Table;

	internal double TableDesiredWidth
	{
		get
		{
			double num = 0.0;
			CalculatedColumn[] calculatedColumns = CalculatedColumns;
			for (int i = 0; i < calculatedColumns.Length; i++)
			{
				num += calculatedColumns[i].DurWidth + Table.InternalCellSpacing;
			}
			return num;
		}
	}

	internal CalculatedColumn[] CalculatedColumns => _calculatedColumns;

	internal double AutofitWidth => _previousAutofitWidth;

	internal override bool IsFirstChunk => _isFirstChunk;

	internal override bool IsLastChunk => _isLastChunk;

	internal TableParaClient(TableParagraph paragraph)
		: base(paragraph)
	{
	}

	protected override void OnArrange()
	{
		base.OnArrange();
		_columnRect = base.Paragraph.StructuralCache.CurrentArrangeContext.ColumnRect;
		CalculatedColumn[] calculatedColumns = CalculatedColumns;
		if (!QueryTableDetails(out var arrayTableRowDesc, out var fskupdTable, out var rect) || fskupdTable == PTS.FSKUPDATE.fskupdNoChange || fskupdTable == PTS.FSKUPDATE.fskupdShifted)
		{
			return;
		}
		_rect = rect;
		UpdateChunkInfo(arrayTableRowDesc);
		MbpInfo mbpInfo = MbpInfo.FromElement(TableParagraph.Element, TableParagraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (base.ParentFlowDirection != base.PageFlowDirection)
		{
			PTS.FSRECT rectPage = _pageContext.PageRect;
			PTS.Validate(PTS.FsTransformRectangle(PTS.FlowDirectionToFswdir(base.ParentFlowDirection), ref rectPage, ref _rect, PTS.FlowDirectionToFswdir(base.PageFlowDirection), out _rect));
			mbpInfo.MirrorMargin();
		}
		_rect.u += mbpInfo.MarginLeft;
		_rect.du -= mbpInfo.MarginLeft + mbpInfo.MarginRight;
		int num = GetTableOffsetFirstRowTop() + TextDpi.ToTextDpi(Table.InternalCellSpacing) / 2;
		for (int i = 0; i < arrayTableRowDesc.Length; i++)
		{
			if (((arrayTableRowDesc[i].fsupdinf.fskupd != 0) ? arrayTableRowDesc[i].fsupdinf.fskupd : fskupdTable) == PTS.FSKUPDATE.fskupdNoChange)
			{
				num += arrayTableRowDesc[i].u.dvrRow;
				continue;
			}
			QueryRowDetails(arrayTableRowDesc[i].pfstablerow, out var arrayFsCell, out var _, out var arrayTableCellMerge);
			for (int j = 0; j < arrayFsCell.Length; j++)
			{
				if (arrayFsCell[j] != IntPtr.Zero && (i == 0 || (arrayTableCellMerge[j] != PTS.FSTABLEKCELLMERGE.fskcellmergeMiddle && arrayTableCellMerge[j] != PTS.FSTABLEKCELLMERGE.fskcellmergeLast)))
				{
					CellParaClient cellParaClient = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[j]);
					double urOffset = calculatedColumns[cellParaClient.ColumnIndex].UrOffset;
					cellParaClient.Arrange(TextDpi.ToTextDpi(urOffset), num, _rect, base.ThisFlowDirection, _pageContext);
				}
			}
			num += arrayTableRowDesc[i].u.dvrRow;
			if (i == 0 && IsFirstChunk)
			{
				num -= mbpInfo.BPTop;
			}
		}
	}

	internal override void ValidateVisual(PTS.FSKUPDATE fskupdInherited)
	{
		Invariant.Assert(fskupdInherited != PTS.FSKUPDATE.fskupdInherited);
		Invariant.Assert(TableParagraph.Table != null && CalculatedColumns != null);
		_ = TableParagraph.Table;
		Visual.Clip = new RectangleGeometry(_columnRect.FromTextDpi());
		if (!QueryTableDetails(out var arrayTableRowDesc, out var fskupdTable, out var _))
		{
			_visual.Children.Clear();
			return;
		}
		MbpInfo mbpInfo = MbpInfo.FromElement(TableParagraph.Element, TableParagraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			mbpInfo.MirrorBP();
		}
		if (fskupdTable == PTS.FSKUPDATE.fskupdInherited)
		{
			fskupdTable = fskupdInherited;
		}
		switch (fskupdTable)
		{
		case PTS.FSKUPDATE.fskupdNoChange:
			return;
		case PTS.FSKUPDATE.fskupdShifted:
			fskupdTable = PTS.FSKUPDATE.fskupdNew;
			break;
		}
		VisualCollection children = _visual.Children;
		if (fskupdTable == PTS.FSKUPDATE.fskupdNew)
		{
			children.Clear();
		}
		Brush backgroundBrush = (Brush)base.Paragraph.Element.GetValue(TextElement.BackgroundProperty);
		using (DrawingContext dc = _visual.RenderOpen())
		{
			Rect tableContentRect = GetTableContentRect(mbpInfo).FromTextDpi();
			_visual.DrawBackgroundAndBorderIntoContext(dc, backgroundBrush, mbpInfo.BorderBrush, mbpInfo.Border, _rect.FromTextDpi(), IsFirstChunk, IsLastChunk);
			DrawColumnBackgrounds(dc, tableContentRect);
			DrawRowGroupBackgrounds(dc, arrayTableRowDesc, tableContentRect, mbpInfo);
			DrawRowBackgrounds(dc, arrayTableRowDesc, tableContentRect, mbpInfo);
		}
		TableRow tableRow = null;
		for (int i = 0; i < arrayTableRowDesc.Length; i++)
		{
			PTS.FSKUPDATE fSKUPDATE = ((arrayTableRowDesc[i].fsupdinf.fskupd != 0) ? arrayTableRowDesc[i].fsupdinf.fskupd : fskupdTable);
			RowParagraph rowParagraph = (RowParagraph)base.PtsContext.HandleToObject(arrayTableRowDesc[i].fsnmRow);
			TableRow row = rowParagraph.Row;
			if (fSKUPDATE == PTS.FSKUPDATE.fskupdNew)
			{
				RowVisual visual = new RowVisual(row);
				children.Insert(i, visual);
			}
			else
			{
				SynchronizeRowVisualsCollection(children, i, row);
			}
			Invariant.Assert(((RowVisual)children[i]).Row == row);
			if (fSKUPDATE == PTS.FSKUPDATE.fskupdNew || fSKUPDATE == PTS.FSKUPDATE.fskupdChangeInside)
			{
				if (rowParagraph.Row.HasForeignCells && (tableRow == null || tableRow.RowGroup != row.RowGroup))
				{
					ValidateRowVisualComplex((RowVisual)children[i], arrayTableRowDesc[i].pfstablerow, CalculatedColumns.Length, fSKUPDATE, CalculatedColumns);
				}
				else
				{
					ValidateRowVisualSimple((RowVisual)children[i], arrayTableRowDesc[i].pfstablerow, fSKUPDATE, CalculatedColumns);
				}
			}
			tableRow = row;
		}
		if (children.Count > arrayTableRowDesc.Length)
		{
			children.RemoveRange(arrayTableRowDesc.Length, children.Count - arrayTableRowDesc.Length);
		}
	}

	internal override void UpdateViewport(ref PTS.FSRECT viewport)
	{
		if (!QueryTableDetails(out var arrayTableRowDesc, out var _, out var _))
		{
			return;
		}
		for (int i = 0; i < arrayTableRowDesc.Length; i++)
		{
			QueryRowDetails(arrayTableRowDesc[i].pfstablerow, out var arrayFsCell, out var _, out var _);
			for (int j = 0; j < arrayFsCell.Length; j++)
			{
				if (arrayFsCell[j] != IntPtr.Zero)
				{
					((CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[j])).UpdateViewport(ref viewport);
				}
			}
		}
	}

	internal override IInputElement InputHitTest(PTS.FSPOINT pt)
	{
		IInputElement inputElement = null;
		if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var rect))
		{
			int num = GetTableOffsetFirstRowTop() + rect.v;
			for (int i = 0; i < arrayTableRowDesc.Length; i++)
			{
				if (pt.v >= num && pt.v <= num + arrayTableRowDesc[i].u.dvrRow)
				{
					QueryRowDetails(arrayTableRowDesc[i].pfstablerow, out var arrayFsCell, out var _, out var _);
					for (int j = 0; j < arrayFsCell.Length; j++)
					{
						if (arrayFsCell[j] != IntPtr.Zero)
						{
							CellParaClient cellParaClient = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[j]);
							_ = cellParaClient.Rect;
							if (cellParaClient.Rect.Contains(pt))
							{
								inputElement = cellParaClient.InputHitTest(pt);
								break;
							}
						}
					}
					break;
				}
				num += arrayTableRowDesc[i].u.dvrRow;
			}
		}
		if (inputElement == null && _rect.Contains(pt))
		{
			inputElement = TableParagraph.Table;
		}
		return inputElement;
	}

	internal override List<Rect> GetRectangles(ContentElement e, int start, int length)
	{
		List<Rect> rectangles = new List<Rect>();
		if (TableParagraph.Table == e)
		{
			GetRectanglesForParagraphElement(out rectangles);
		}
		else
		{
			rectangles = new List<Rect>();
			if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var _))
			{
				for (int i = 0; i < arrayTableRowDesc.Length; i++)
				{
					QueryRowDetails(arrayTableRowDesc[i].pfstablerow, out var arrayFsCell, out var _, out var _);
					for (int j = 0; j < arrayFsCell.Length; j++)
					{
						if (arrayFsCell[j] == IntPtr.Zero)
						{
							continue;
						}
						CellParaClient cellParaClient = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[j]);
						if (start < cellParaClient.Paragraph.ParagraphEndCharacterPosition)
						{
							rectangles = cellParaClient.GetRectangles(e, start, length);
							Invariant.Assert(rectangles != null);
							if (rectangles.Count != 0)
							{
								break;
							}
						}
					}
					if (rectangles.Count != 0)
					{
						break;
					}
				}
			}
		}
		Invariant.Assert(rectangles != null);
		return rectangles;
	}

	internal override ParagraphResult CreateParagraphResult()
	{
		return new TableParagraphResult(this);
	}

	internal override TextContentRange GetTextContentRange()
	{
		TextContentRange textContentRange = null;
		if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var _))
		{
			textContentRange = new TextContentRange();
			UpdateChunkInfo(arrayTableRowDesc);
			TextElement textElement = base.Paragraph.Element as TextElement;
			if (_isFirstChunk)
			{
				textContentRange.Merge(TextContainerHelper.GetTextContentRangeForTextElementEdge(textElement, ElementEdge.BeforeStart));
			}
			for (int i = 0; i < arrayTableRowDesc.Length; i++)
			{
				TableRow row = ((RowParagraph)base.PtsContext.HandleToObject(arrayTableRowDesc[i].fsnmRow)).Row;
				PTS.Validate(PTS.FsQueryTableObjRowDetails(base.PtsContext.Context, arrayTableRowDesc[i].pfstablerow, out var ptableorowdetails));
				if (ptableorowdetails.fskboundaryAbove != PTS.FSKTABLEROWBOUNDARY.fsktablerowboundaryBreak)
				{
					if (row.Index == 0)
					{
						textContentRange.Merge(TextContainerHelper.GetTextContentRangeForTextElementEdge(row.RowGroup, ElementEdge.BeforeStart));
					}
					textContentRange.Merge(TextContainerHelper.GetTextContentRangeForTextElementEdge(row, ElementEdge.BeforeStart));
				}
				QueryRowDetails(arrayTableRowDesc[i].pfstablerow, out var arrayFsCell, out var _, out var _);
				for (int j = 0; j < arrayFsCell.Length; j++)
				{
					if (arrayFsCell[j] != IntPtr.Zero)
					{
						CellParaClient cellParaClient = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[j]);
						textContentRange.Merge(cellParaClient.GetTextContentRange());
					}
				}
				if (ptableorowdetails.fskboundaryBelow != PTS.FSKTABLEROWBOUNDARY.fsktablerowboundaryBreak)
				{
					textContentRange.Merge(TextContainerHelper.GetTextContentRangeForTextElementEdge(row, ElementEdge.AfterEnd));
					if (row.Index == row.RowGroup.Rows.Count - 1)
					{
						textContentRange.Merge(TextContainerHelper.GetTextContentRangeForTextElementEdge(row.RowGroup, ElementEdge.AfterEnd));
					}
				}
			}
			if (_isLastChunk)
			{
				textContentRange.Merge(TextContainerHelper.GetTextContentRangeForTextElementEdge(textElement, ElementEdge.AfterEnd));
			}
		}
		if (textContentRange == null)
		{
			textContentRange = TextContainerHelper.GetTextContentRangeForTextElement(TableParagraph.Table);
		}
		return textContentRange;
	}

	internal CellParaClient GetCellParaClientFromPoint(Point point, bool snapToText)
	{
		int num = TextDpi.ToTextDpi(point.X);
		int num2 = TextDpi.ToTextDpi(point.Y);
		CellParaClient cellParaClient = null;
		CellParaClient cellParaClient2 = null;
		int num3 = int.MaxValue;
		if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var _))
		{
			for (int i = 0; i < arrayTableRowDesc.Length; i++)
			{
				if (cellParaClient != null)
				{
					break;
				}
				QueryRowDetails(arrayTableRowDesc[i].pfstablerow, out var arrayFsCell, out var _, out var _);
				for (int j = 0; j < arrayFsCell.Length; j++)
				{
					if (cellParaClient != null)
					{
						break;
					}
					if (arrayFsCell[j] == IntPtr.Zero)
					{
						continue;
					}
					CellParaClient cellParaClient3 = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[j]);
					PTS.FSRECT rect2 = cellParaClient3.Rect;
					if (num >= rect2.u && num <= rect2.u + rect2.du && num2 >= rect2.v && num2 <= rect2.v + rect2.dv)
					{
						cellParaClient = cellParaClient3;
					}
					else if (snapToText)
					{
						int num4 = Math.Min(Math.Abs(rect2.u - num), Math.Abs(rect2.u + rect2.du - num));
						int num5 = Math.Min(Math.Abs(rect2.v - num2), Math.Abs(rect2.v + rect2.dv - num2));
						if (num4 + num5 < num3)
						{
							num3 = num4 + num5;
							cellParaClient2 = cellParaClient3;
						}
					}
				}
			}
		}
		if (snapToText && cellParaClient == null)
		{
			cellParaClient = cellParaClient2;
		}
		return cellParaClient;
	}

	internal ReadOnlyCollection<ParagraphResult> GetChildrenParagraphResults(out bool hasTextContent)
	{
		MbpInfo mbpInfo = MbpInfo.FromElement(TableParagraph.Element, TableParagraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			mbpInfo.MirrorBP();
		}
		Rect rect = GetTableContentRect(mbpInfo).FromTextDpi();
		double num = rect.Y;
		Rect rowRect = rect;
		hasTextContent = false;
		List<ParagraphResult> list = new List<ParagraphResult>(0);
		if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var _))
		{
			for (int i = 0; i < arrayTableRowDesc.Length; i++)
			{
				RowParagraph rowParagraph = (RowParagraph)base.PtsContext.HandleToObject(arrayTableRowDesc[i].fsnmRow);
				rowRect.Y = num;
				rowRect.Height = GetActualRowHeight(arrayTableRowDesc, i, mbpInfo);
				RowParagraphResult rowParagraphResult = new RowParagraphResult(this, i, rowRect, rowParagraph);
				if (rowParagraphResult.HasTextContent)
				{
					hasTextContent = true;
				}
				list.Add(rowParagraphResult);
				num += rowRect.Height;
			}
		}
		return new ReadOnlyCollection<ParagraphResult>(list);
	}

	internal ReadOnlyCollection<ParagraphResult> GetChildrenParagraphResultsForRow(int rowIndex, out bool hasTextContent)
	{
		List<ParagraphResult> list = new List<ParagraphResult>(0);
		hasTextContent = false;
		if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var _))
		{
			QueryRowDetails(arrayTableRowDesc[rowIndex].pfstablerow, out var arrayFsCell, out var _, out var arrayTableCellMerge);
			for (int i = 0; i < arrayFsCell.Length; i++)
			{
				if (arrayFsCell[i] != IntPtr.Zero && (rowIndex == 0 || (arrayTableCellMerge[i] != PTS.FSTABLEKCELLMERGE.fskcellmergeMiddle && arrayTableCellMerge[i] != PTS.FSTABLEKCELLMERGE.fskcellmergeLast)))
				{
					ParagraphResult paragraphResult = ((CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[i])).CreateParagraphResult();
					if (paragraphResult.HasTextContent)
					{
						hasTextContent = true;
					}
					list.Add(paragraphResult);
				}
			}
		}
		return new ReadOnlyCollection<ParagraphResult>(list);
	}

	internal ReadOnlyCollection<ParagraphResult> GetParagraphsFromPoint(Point point, bool snapToText)
	{
		CellParaClient cellParaClientFromPoint = GetCellParaClientFromPoint(point, snapToText);
		List<ParagraphResult> list = new List<ParagraphResult>(0);
		if (cellParaClientFromPoint != null)
		{
			list.Add(cellParaClientFromPoint.CreateParagraphResult());
		}
		return new ReadOnlyCollection<ParagraphResult>(list);
	}

	internal ReadOnlyCollection<ParagraphResult> GetParagraphsFromPosition(ITextPointer position)
	{
		CellParaClient cellParaClientFromPosition = GetCellParaClientFromPosition(position);
		List<ParagraphResult> list = new List<ParagraphResult>(0);
		if (cellParaClientFromPosition != null)
		{
			list.Add(cellParaClientFromPosition.CreateParagraphResult());
		}
		return new ReadOnlyCollection<ParagraphResult>(list);
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition, Rect visibleRect)
	{
		Geometry geometry = null;
		if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var _))
		{
			bool flag = false;
			for (int i = 0; i < arrayTableRowDesc.Length; i++)
			{
				if (flag)
				{
					break;
				}
				QueryRowDetails(arrayTableRowDesc[i].pfstablerow, out var arrayFsCell, out var _, out var _);
				for (int j = 0; j < arrayFsCell.Length; j++)
				{
					if (arrayFsCell[j] != IntPtr.Zero)
					{
						CellParaClient cellParaClient = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[j]);
						if (endPosition.CompareTo(cellParaClient.Cell.ContentStart) <= 0)
						{
							flag = true;
						}
						else if (startPosition.CompareTo(cellParaClient.Cell.ContentEnd) <= 0)
						{
							Geometry tightBoundingGeometryFromTextPositions = cellParaClient.GetTightBoundingGeometryFromTextPositions(startPosition, endPosition, visibleRect);
							CaretElement.AddGeometry(ref geometry, tightBoundingGeometryFromTextPositions);
						}
					}
				}
			}
		}
		if (geometry != null)
		{
			geometry = Geometry.Combine(geometry, Visual.Clip, GeometryCombineMode.Intersect, null);
		}
		return geometry;
	}

	internal CellParaClient GetCellParaClientFromPosition(ITextPointer position)
	{
		if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var _))
		{
			for (int i = 0; i < arrayTableRowDesc.Length; i++)
			{
				QueryRowDetails(arrayTableRowDesc[i].pfstablerow, out var arrayFsCell, out var _, out var _);
				for (int j = 0; j < arrayFsCell.Length; j++)
				{
					if (arrayFsCell[j] != IntPtr.Zero)
					{
						CellParaClient cellParaClient = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[j]);
						if (position.CompareTo(cellParaClient.Cell.ContentStart) >= 0 && position.CompareTo(cellParaClient.Cell.ContentEnd) <= 0)
						{
							return cellParaClient;
						}
					}
				}
			}
		}
		return null;
	}

	internal CellParaClient GetCellAbove(double suggestedX, int rowGroupIndex, int rowIndex)
	{
		int num = TextDpi.ToTextDpi(suggestedX);
		if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var _))
		{
			MbpInfo.FromElement(TableParagraph.Element, TableParagraph.StructuralCache.TextFormatterHost.PixelsPerDip);
			for (int num2 = arrayTableRowDesc.Length - 1; num2 >= 0; num2--)
			{
				QueryRowDetails(arrayTableRowDesc[num2].pfstablerow, out var arrayFsCell, out var _, out var _);
				CellParaClient cellParaClient = null;
				int num3 = int.MaxValue;
				for (int num4 = arrayFsCell.Length - 1; num4 >= 0; num4--)
				{
					if (arrayFsCell[num4] != IntPtr.Zero)
					{
						CellParaClient cellParaClient2 = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[num4]);
						if ((cellParaClient2.Cell.RowIndex + cellParaClient2.Cell.RowSpan - 1 < rowIndex && cellParaClient2.Cell.RowGroupIndex == rowGroupIndex) || cellParaClient2.Cell.RowGroupIndex < rowGroupIndex)
						{
							if (num >= cellParaClient2.Rect.u && num <= cellParaClient2.Rect.u + cellParaClient2.Rect.du)
							{
								return cellParaClient2;
							}
							int num5 = Math.Abs(cellParaClient2.Rect.u + cellParaClient2.Rect.du / 2 - num);
							if (num5 < num3)
							{
								num3 = num5;
								cellParaClient = cellParaClient2;
							}
						}
					}
				}
				if (cellParaClient != null)
				{
					return cellParaClient;
				}
			}
		}
		return null;
	}

	internal CellParaClient GetCellBelow(double suggestedX, int rowGroupIndex, int rowIndex)
	{
		int num = TextDpi.ToTextDpi(suggestedX);
		if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var _))
		{
			for (int i = 0; i < arrayTableRowDesc.Length; i++)
			{
				QueryRowDetails(arrayTableRowDesc[i].pfstablerow, out var arrayFsCell, out var _, out var _);
				CellParaClient cellParaClient = null;
				int num2 = int.MaxValue;
				for (int num3 = arrayFsCell.Length - 1; num3 >= 0; num3--)
				{
					if (arrayFsCell[num3] != IntPtr.Zero)
					{
						CellParaClient cellParaClient2 = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[num3]);
						if ((cellParaClient2.Cell.RowIndex > rowIndex && cellParaClient2.Cell.RowGroupIndex == rowGroupIndex) || cellParaClient2.Cell.RowGroupIndex > rowGroupIndex)
						{
							if (num >= cellParaClient2.Rect.u && num <= cellParaClient2.Rect.u + cellParaClient2.Rect.du)
							{
								return cellParaClient2;
							}
							int num4 = Math.Abs(cellParaClient2.Rect.u + cellParaClient2.Rect.du / 2 - num);
							if (num4 < num2)
							{
								num2 = num4;
								cellParaClient = cellParaClient2;
							}
						}
					}
				}
				if (cellParaClient != null)
				{
					return cellParaClient;
				}
			}
		}
		return null;
	}

	internal CellInfo GetCellInfoFromPoint(Point point)
	{
		CellParaClient cellParaClientFromPoint = GetCellParaClientFromPoint(point, snapToText: true);
		if (cellParaClientFromPoint != null)
		{
			return new CellInfo(this, cellParaClientFromPoint);
		}
		return null;
	}

	internal Rect GetRectangleFromRowEndPosition(ITextPointer position)
	{
		if (QueryTableDetails(out var arrayTableRowDesc, out var _, out var rect))
		{
			int num = GetTableOffsetFirstRowTop() + rect.v;
			for (int i = 0; i < arrayTableRowDesc.Length; i++)
			{
				TableRow row = ((RowParagraph)base.PtsContext.HandleToObject(arrayTableRowDesc[i].fsnmRow)).Row;
				if (((TextPointer)position).CompareTo(row.ContentEnd) == 0)
				{
					return new Rect(TextDpi.FromTextDpi(rect.u + rect.du), TextDpi.FromTextDpi(num), 1.0, TextDpi.FromTextDpi(arrayTableRowDesc[i].u.dvrRow));
				}
				num += arrayTableRowDesc[i].u.dvrRow;
			}
		}
		return System.Windows.Rect.Empty;
	}

	internal void AutofitTable(uint fswdirTrack, int durAvailableSpace, out int durTableWidth)
	{
		double availableWidth = TextDpi.FromTextDpi(durAvailableSpace);
		Autofit(availableWidth, out var tableWidth);
		durTableWidth = TextDpi.ToTextDpi(tableWidth);
	}

	internal void UpdAutofitTable(uint fswdirTrack, int durAvailableSpace, out int durTableWidth, out int fNoChangeInCellWidths)
	{
		double availableWidth = TextDpi.FromTextDpi(durAvailableSpace);
		fNoChangeInCellWidths = Autofit(availableWidth, out var tableWidth);
		durTableWidth = TextDpi.ToTextDpi(tableWidth);
	}

	internal int Autofit(double availableWidth, out double tableWidth)
	{
		int result = 1;
		ValidateCalculatedColumns();
		if (!DoubleUtil.AreClose(availableWidth, _previousAutofitWidth))
		{
			result = ValidateTableWidths(availableWidth, out tableWidth);
		}
		else
		{
			tableWidth = _previousTableWidth;
		}
		_previousAutofitWidth = availableWidth;
		_previousTableWidth = tableWidth;
		return result;
	}

	private void UpdateChunkInfo(PTS.FSTABLEROWDESCRIPTION[] arrayTableRowDesc)
	{
		TableRow row = ((RowParagraph)base.PtsContext.HandleToObject(arrayTableRowDesc[0].fsnmRow)).Row;
		PTS.Validate(PTS.FsQueryTableObjRowDetails(base.PtsContext.Context, arrayTableRowDesc[0].pfstablerow, out var ptableorowdetails));
		_isFirstChunk = ptableorowdetails.fskboundaryAbove == PTS.FSKTABLEROWBOUNDARY.fsktablerowboundaryOuter && row.Index == 0 && Table.IsFirstNonEmptyRowGroup(row.RowGroup.Index);
		row = ((RowParagraph)base.PtsContext.HandleToObject(arrayTableRowDesc[^1].fsnmRow)).Row;
		PTS.Validate(PTS.FsQueryTableObjRowDetails(base.PtsContext.Context, arrayTableRowDesc[^1].pfstablerow, out ptableorowdetails));
		_isLastChunk = ptableorowdetails.fskboundaryBelow == PTS.FSKTABLEROWBOUNDARY.fsktablerowboundaryOuter && row.Index == row.RowGroup.Rows.Count - 1 && Table.IsLastNonEmptyRowGroup(row.RowGroup.Index);
	}

	private unsafe bool QueryTableDetails(out PTS.FSTABLEROWDESCRIPTION[] arrayTableRowDesc, out PTS.FSKUPDATE fskupdTable, out PTS.FSRECT rect)
	{
		PTS.Validate(PTS.FsQueryTableObjDetails(base.PtsContext.Context, _paraHandle.Value, out var pfstableobjdetails));
		fskupdTable = pfstableobjdetails.fskupdTableProper;
		rect = pfstableobjdetails.fsrcTableObj;
		PTS.Validate(PTS.FsQueryTableObjTableProperDetails(base.PtsContext.Context, pfstableobjdetails.pfstableProper, out var pfstabledetailsProper));
		if (pfstabledetailsProper.cRows == 0)
		{
			arrayTableRowDesc = null;
			return false;
		}
		arrayTableRowDesc = new PTS.FSTABLEROWDESCRIPTION[pfstabledetailsProper.cRows];
		fixed (PTS.FSTABLEROWDESCRIPTION* rgtablerowdescr = arrayTableRowDesc)
		{
			PTS.Validate(PTS.FsQueryTableObjRowList(base.PtsContext.Context, pfstableobjdetails.pfstableProper, pfstabledetailsProper.cRows, rgtablerowdescr, out var _));
		}
		return true;
	}

	private unsafe void QueryRowDetails(nint pfstablerow, out nint[] arrayFsCell, out PTS.FSKUPDATE[] arrayUpdate, out PTS.FSTABLEKCELLMERGE[] arrayTableCellMerge)
	{
		PTS.Validate(PTS.FsQueryTableObjRowDetails(base.PtsContext.Context, pfstablerow, out var ptableorowdetails));
		arrayUpdate = new PTS.FSKUPDATE[ptableorowdetails.cCells];
		arrayFsCell = new nint[ptableorowdetails.cCells];
		arrayTableCellMerge = new PTS.FSTABLEKCELLMERGE[ptableorowdetails.cCells];
		if (ptableorowdetails.cCells <= 0)
		{
			return;
		}
		fixed (PTS.FSKUPDATE* rgfskupd = arrayUpdate)
		{
			fixed (nint* rgpfscell = arrayFsCell)
			{
				fixed (PTS.FSTABLEKCELLMERGE* rgkcellmerge = arrayTableCellMerge)
				{
					PTS.Validate(PTS.FsQueryTableObjCellList(base.PtsContext.Context, pfstablerow, ptableorowdetails.cCells, rgfskupd, rgpfscell, rgkcellmerge, out var _));
				}
			}
		}
	}

	private void SynchronizeRowVisualsCollection(VisualCollection rowVisualsCollection, int firstIndex, TableRow row)
	{
		if (((RowVisual)rowVisualsCollection[firstIndex]).Row != row)
		{
			int num = firstIndex;
			int count = rowVisualsCollection.Count;
			while (++num < count && ((RowVisual)rowVisualsCollection[num]).Row != row)
			{
			}
			rowVisualsCollection.RemoveRange(firstIndex, num - firstIndex);
		}
	}

	private void SynchronizeCellVisualsCollection(VisualCollection cellVisualsCollection, int firstIndex, Visual visual)
	{
		if (cellVisualsCollection[firstIndex] != visual)
		{
			int num = firstIndex;
			int count = cellVisualsCollection.Count;
			while (++num < count && cellVisualsCollection[num] != visual)
			{
			}
			cellVisualsCollection.RemoveRange(firstIndex, num - firstIndex);
		}
	}

	private void ValidateRowVisualSimple(RowVisual rowVisual, nint pfstablerow, PTS.FSKUPDATE fskupdRow, CalculatedColumn[] calculatedColumns)
	{
		QueryRowDetails(pfstablerow, out var arrayFsCell, out var arrayUpdate, out var arrayTableCellMerge);
		VisualCollection children = rowVisual.Children;
		int num = 0;
		for (int i = 0; i < arrayFsCell.Length; i++)
		{
			if (arrayFsCell[i] == IntPtr.Zero || arrayTableCellMerge[i] == PTS.FSTABLEKCELLMERGE.fskcellmergeMiddle || arrayTableCellMerge[i] == PTS.FSTABLEKCELLMERGE.fskcellmergeLast)
			{
				continue;
			}
			PTS.FSKUPDATE fSKUPDATE = ((arrayUpdate[i] != 0) ? arrayUpdate[i] : fskupdRow);
			if (fSKUPDATE != PTS.FSKUPDATE.fskupdNoChange)
			{
				CellParaClient cellParaClient = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[i]);
				_ = calculatedColumns[cellParaClient.ColumnIndex].UrOffset;
				cellParaClient.ValidateVisual();
				if (fSKUPDATE == PTS.FSKUPDATE.fskupdNew || VisualTreeHelper.GetParent(cellParaClient.Visual) == null)
				{
					if (VisualTreeHelper.GetParent(cellParaClient.Visual) is Visual visual)
					{
						ContainerVisual obj = visual as ContainerVisual;
						Invariant.Assert(obj != null, "parent should always derives from ContainerVisual");
						obj.Children.Remove(cellParaClient.Visual);
					}
					children.Insert(num, cellParaClient.Visual);
				}
				else
				{
					SynchronizeCellVisualsCollection(children, num, cellParaClient.Visual);
				}
			}
			num++;
		}
		if (children.Count > num)
		{
			children.RemoveRange(num, children.Count - num);
		}
	}

	private void ValidateRowVisualComplex(RowVisual rowVisual, nint pfstablerow, int tableColumnCount, PTS.FSKUPDATE fskupdRow, CalculatedColumn[] calculatedColumns)
	{
		QueryRowDetails(pfstablerow, out var arrayFsCell, out var arrayUpdate, out var _);
		CellParaClientEntry[] array = new CellParaClientEntry[tableColumnCount];
		for (int i = 0; i < arrayFsCell.Length; i++)
		{
			if (arrayFsCell[i] != IntPtr.Zero)
			{
				PTS.FSKUPDATE fskupdCell = ((arrayUpdate[i] != 0) ? arrayUpdate[i] : fskupdRow);
				CellParaClient cellParaClient = (CellParaClient)base.PtsContext.HandleToObject(arrayFsCell[i]);
				int columnIndex = cellParaClient.ColumnIndex;
				array[columnIndex].cellParaClient = cellParaClient;
				array[columnIndex].fskupdCell = fskupdCell;
			}
		}
		VisualCollection children = rowVisual.Children;
		int num = 0;
		for (int j = 0; j < array.Length; j++)
		{
			CellParaClient cellParaClient2 = array[j].cellParaClient;
			if (cellParaClient2 == null)
			{
				continue;
			}
			PTS.FSKUPDATE fskupdCell2 = array[j].fskupdCell;
			if (fskupdCell2 != PTS.FSKUPDATE.fskupdNoChange)
			{
				_ = calculatedColumns[j].UrOffset;
				cellParaClient2.ValidateVisual();
				if (fskupdCell2 == PTS.FSKUPDATE.fskupdNew)
				{
					children.Insert(num, cellParaClient2.Visual);
				}
				else
				{
					SynchronizeCellVisualsCollection(children, num, cellParaClient2.Visual);
				}
			}
			num++;
		}
		if (children.Count > num)
		{
			children.RemoveRange(num, children.Count - num);
		}
	}

	private void DrawColumnBackgrounds(DrawingContext dc, Rect tableContentRect)
	{
		double num = tableContentRect.X;
		Rect rectangle = tableContentRect;
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			for (int num2 = CalculatedColumns.Length - 1; num2 >= 0; num2--)
			{
				Brush brush = ((num2 < Table.Columns.Count) ? Table.Columns[num2].Background : null);
				rectangle.Width = CalculatedColumns[num2].DurWidth + Table.InternalCellSpacing;
				if (brush != null)
				{
					rectangle.X = num;
					dc.DrawRectangle(brush, null, rectangle);
				}
				num += rectangle.Width;
			}
			return;
		}
		for (int i = 0; i < CalculatedColumns.Length; i++)
		{
			Brush brush = ((i < Table.Columns.Count) ? Table.Columns[i].Background : null);
			rectangle.Width = CalculatedColumns[i].DurWidth + Table.InternalCellSpacing;
			if (brush != null)
			{
				rectangle.X = num;
				dc.DrawRectangle(brush, null, rectangle);
			}
			num += rectangle.Width;
		}
	}

	private double GetActualRowHeight(PTS.FSTABLEROWDESCRIPTION[] arrayTableRowDesc, int rowIndex, MbpInfo mbpInfo)
	{
		int num = 0;
		if (IsFirstChunk && rowIndex == 0)
		{
			num = -mbpInfo.BPTop;
		}
		if (IsLastChunk && rowIndex == arrayTableRowDesc.Length - 1)
		{
			num = -mbpInfo.BPBottom;
		}
		return TextDpi.FromTextDpi(arrayTableRowDesc[rowIndex].u.dvrRow + num);
	}

	private void DrawRowGroupBackgrounds(DrawingContext dc, PTS.FSTABLEROWDESCRIPTION[] arrayTableRowDesc, Rect tableContentRect, MbpInfo mbpInfo)
	{
		double num = tableContentRect.Y;
		double num2 = 0.0;
		Rect rectangle = tableContentRect;
		if (arrayTableRowDesc.Length == 0)
		{
			return;
		}
		TableRow row = ((RowParagraph)base.PtsContext.HandleToObject(arrayTableRowDesc[0].fsnmRow)).Row;
		TableRowGroup rowGroup = row.RowGroup;
		Brush brush;
		for (int i = 0; i < arrayTableRowDesc.Length; i++)
		{
			row = ((RowParagraph)base.PtsContext.HandleToObject(arrayTableRowDesc[i].fsnmRow)).Row;
			if (rowGroup != row.RowGroup)
			{
				brush = (Brush)rowGroup.GetValue(TextElement.BackgroundProperty);
				if (brush != null)
				{
					rectangle.Y = num;
					rectangle.Height = num2;
					dc.DrawRectangle(brush, null, rectangle);
				}
				num += num2;
				rowGroup = row.RowGroup;
				num2 = GetActualRowHeight(arrayTableRowDesc, i, mbpInfo);
			}
			else
			{
				num2 += GetActualRowHeight(arrayTableRowDesc, i, mbpInfo);
			}
		}
		brush = (Brush)rowGroup.GetValue(TextElement.BackgroundProperty);
		if (brush != null)
		{
			rectangle.Y = num;
			rectangle.Height = num2;
			dc.DrawRectangle(brush, null, rectangle);
		}
	}

	private void DrawRowBackgrounds(DrawingContext dc, PTS.FSTABLEROWDESCRIPTION[] arrayTableRowDesc, Rect tableContentRect, MbpInfo mbpInfo)
	{
		double num = tableContentRect.Y;
		Rect rectangle = tableContentRect;
		for (int i = 0; i < arrayTableRowDesc.Length; i++)
		{
			Brush brush = (Brush)((RowParagraph)base.PtsContext.HandleToObject(arrayTableRowDesc[i].fsnmRow)).Row.GetValue(TextElement.BackgroundProperty);
			rectangle.Y = num;
			rectangle.Height = GetActualRowHeight(arrayTableRowDesc, i, mbpInfo);
			if (brush != null)
			{
				dc.DrawRectangle(brush, null, rectangle);
			}
			num += rectangle.Height;
		}
	}

	private void ValidateCalculatedColumns()
	{
		int columnCount = Table.ColumnCount;
		if (_calculatedColumns == null)
		{
			_calculatedColumns = new CalculatedColumn[columnCount];
		}
		else if (_calculatedColumns.Length != columnCount)
		{
			CalculatedColumn[] array = new CalculatedColumn[columnCount];
			Array.Copy(_calculatedColumns, array, Math.Min(_calculatedColumns.Length, columnCount));
			_calculatedColumns = array;
		}
		if (_calculatedColumns.Length != 0)
		{
			int i;
			for (i = 0; i < _calculatedColumns.Length && i < Table.Columns.Count; i++)
			{
				_calculatedColumns[i].UserWidth = Table.Columns[i].Width;
			}
			for (; i < _calculatedColumns.Length; i++)
			{
				_calculatedColumns[i].UserWidth = TableColumn.DefaultWidth;
			}
		}
		_durMinWidth = (_durMaxWidth = 0.0);
		for (int j = 0; j < _calculatedColumns.Length; j++)
		{
			switch (_calculatedColumns[j].UserWidth.GridUnitType)
			{
			case GridUnitType.Auto:
				_calculatedColumns[j].ValidateAuto(1.0, 1000000.0);
				break;
			case GridUnitType.Star:
				_calculatedColumns[j].ValidateAuto(1.0, 1000000.0);
				break;
			case GridUnitType.Pixel:
				_calculatedColumns[j].ValidateAuto(_calculatedColumns[j].UserWidth.Value, _calculatedColumns[j].UserWidth.Value);
				break;
			}
			_durMinWidth += _calculatedColumns[j].DurMinWidth;
			_durMaxWidth += _calculatedColumns[j].DurMaxWidth;
		}
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Paragraph.Element, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		double num = Table.InternalCellSpacing * (double)Table.ColumnCount + mbpInfo.Margin.Left + mbpInfo.Border.Left + mbpInfo.Padding.Left + mbpInfo.Padding.Right + mbpInfo.Border.Right + mbpInfo.Margin.Right;
		_durMinWidth += num;
		_durMaxWidth += num;
	}

	private int ValidateTableWidths(double durAvailableWidth, out double durTableWidth)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		bool flag9 = false;
		double internalCellSpacing = Table.InternalCellSpacing;
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Paragraph.Element, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		double num = internalCellSpacing * (double)Table.ColumnCount + TextDpi.FromTextDpi(mbpInfo.MBPLeft + mbpInfo.MBPRight);
		int result = 1;
		durTableWidth = 0.0;
		double num2 = 0.0;
		double num3;
		double num4 = (num3 = 0.0);
		double num5 = 0.0;
		double num6;
		double num7 = (num6 = 0.0);
		double num8 = 0.0;
		double num9 = 1.0;
		double num10;
		for (int i = 0; i < _calculatedColumns.Length; i++)
		{
			if (_calculatedColumns[i].UserWidth.IsAuto)
			{
				num7 += _calculatedColumns[i].DurMinWidth;
				num6 += _calculatedColumns[i].DurMaxWidth;
			}
			else if (_calculatedColumns[i].UserWidth.IsStar)
			{
				num10 = _calculatedColumns[i].UserWidth.Value;
				if (num10 < 0.0)
				{
					num10 = 0.0;
				}
				if (num2 + num10 > 100.0)
				{
					num10 = 100.0 - num2;
					num2 = 100.0;
					_calculatedColumns[i].UserWidth = new GridLength(num10, GridUnitType.Star);
				}
				else
				{
					num2 += num10;
				}
				if (num10 == 0.0)
				{
					num10 = 1.0;
				}
				if (_calculatedColumns[i].DurMaxWidth * num9 > num10 * num8)
				{
					num8 = _calculatedColumns[i].DurMaxWidth;
					num9 = num10;
				}
				num5 += _calculatedColumns[i].DurMinWidth;
			}
			else
			{
				num4 += _calculatedColumns[i].DurMinWidth;
				num3 += _calculatedColumns[i].DurMaxWidth;
			}
		}
		num10 = 100.0 - num2;
		double num11;
		if (flag)
		{
			num11 = durAvailableWidth;
			if (num11 < _durMinWidth && !DoubleUtil.AreClose(num11, _durMinWidth))
			{
				num11 = _durMinWidth;
			}
		}
		else if (!(0.0 < num2))
		{
			num11 = ((_durMaxWidth < durAvailableWidth && !DoubleUtil.AreClose(_durMaxWidth, durAvailableWidth)) ? _durMaxWidth : ((!(_durMinWidth > durAvailableWidth) || DoubleUtil.AreClose(_durMinWidth, durAvailableWidth)) ? durAvailableWidth : _durMinWidth));
		}
		else
		{
			if (0.0 < num10)
			{
				double num12 = (num3 + num6) * num9;
				double num13 = num10 * num8;
				if (num12 > num13 && !DoubleUtil.AreClose(num12, num13))
				{
					num8 = num3 + num6;
					num9 = num10;
				}
			}
			if (0.0 < num10 || DoubleUtil.IsZero(num3 + num6))
			{
				num11 = num8 * 100.0 / num9 + num;
				if (num11 > durAvailableWidth && !DoubleUtil.AreClose(num11, durAvailableWidth))
				{
					num11 = durAvailableWidth;
				}
			}
			else
			{
				num11 = durAvailableWidth;
			}
			if (num11 < _durMinWidth && !DoubleUtil.AreClose(num11, _durMinWidth))
			{
				num11 = _durMinWidth;
			}
		}
		if (num11 > num || DoubleUtil.AreClose(num11, num))
		{
			num11 -= num;
		}
		double num14;
		if (0.0 < num6 + num3 && !DoubleUtil.IsZero(num6 + num3))
		{
			num14 = num10 * num11 / 100.0;
			if (num14 < num4 + num7 && !DoubleUtil.AreClose(num14, num4 + num7))
			{
				num14 = num4 + num7;
			}
			if (num14 > num11 - num5 && !DoubleUtil.AreClose(num14, num11 - num5))
			{
				num14 = num11 - num5;
			}
		}
		else
		{
			num14 = 0.0;
		}
		double num15;
		double num16;
		if (0.0 < num3 && !DoubleUtil.IsZero(num3))
		{
			num15 = num3;
			if (num15 > num14 && !DoubleUtil.AreClose(num15, num14))
			{
				num15 = num14;
			}
			if (0.0 < num6 && !DoubleUtil.IsZero(num6))
			{
				num16 = num7;
				if (num15 + num16 < num14 || DoubleUtil.AreClose(num15 + num16, num14))
				{
					num16 = num14 - num15;
				}
				else
				{
					num15 = num4;
					if (num15 + num16 < num14 || DoubleUtil.AreClose(num15 + num16, num14))
					{
						num15 = num14 - num16;
					}
				}
			}
			else
			{
				num16 = 0.0;
				if (num15 < num14 && !DoubleUtil.AreClose(num15, num14))
				{
					num15 = num14;
				}
			}
		}
		else
		{
			num15 = 0.0;
			if (0.0 < num6 && !DoubleUtil.IsZero(num6))
			{
				num16 = num7;
				if (num16 < num14 && !DoubleUtil.AreClose(num16, num14))
				{
					num16 = num14;
				}
			}
			else
			{
				num16 = 0.0;
			}
		}
		if (num16 > num6 && !DoubleUtil.AreClose(num16, num6))
		{
			flag4 = true;
		}
		else if (DoubleUtil.AreClose(num16, num6))
		{
			flag2 = true;
		}
		else if (DoubleUtil.AreClose(num16, num7))
		{
			flag3 = true;
		}
		else if (num16 < num6 && !DoubleUtil.AreClose(num16, num6))
		{
			flag8 = true;
		}
		if (num15 > num3 && !DoubleUtil.AreClose(num15, num3))
		{
			flag7 = true;
		}
		else if (DoubleUtil.AreClose(num15, num3))
		{
			flag5 = true;
		}
		else if (DoubleUtil.AreClose(num15, num4))
		{
			flag6 = true;
		}
		else if (num15 < num3 && !DoubleUtil.AreClose(num15, num3))
		{
			flag9 = true;
		}
		double num17 = ((0.0 < num11) ? (100.0 * (num11 - num15 - num16) / num11) : 0.0);
		bool flag10 = !DoubleUtil.AreClose(num3, num4);
		durTableWidth = TextDpi.FromTextDpi(mbpInfo.BPLeft);
		for (int j = 0; j < _calculatedColumns.Length; j++)
		{
			if (_calculatedColumns[j].UserWidth.IsAuto)
			{
				_calculatedColumns[j].DurWidth = (flag8 ? (_calculatedColumns[j].DurMaxWidth - (_calculatedColumns[j].DurMaxWidth - _calculatedColumns[j].DurMinWidth) * (num6 - num16) / (num6 - num7)) : (flag4 ? (_calculatedColumns[j].DurMaxWidth + _calculatedColumns[j].DurMaxWidth * (num16 - num6) / num6) : (flag2 ? _calculatedColumns[j].DurMaxWidth : (flag3 ? _calculatedColumns[j].DurMinWidth : ((0.0 < num6 && !DoubleUtil.IsZero(num6)) ? (_calculatedColumns[j].DurMinWidth + _calculatedColumns[j].DurMaxWidth * (num16 - num7) / num6) : 0.0)))));
			}
			else if (_calculatedColumns[j].UserWidth.IsStar)
			{
				num14 = ((0.0 < num2) ? (num11 * (num17 * _calculatedColumns[j].UserWidth.Value / num2) / 100.0) : 0.0);
				num14 -= _calculatedColumns[j].DurMinWidth;
				if (num14 < 0.0 && !DoubleUtil.IsZero(num14))
				{
					num14 = 0.0;
				}
				_calculatedColumns[j].DurWidth = _calculatedColumns[j].DurMinWidth + num14;
			}
			else
			{
				_calculatedColumns[j].DurWidth = ((!flag9) ? (flag7 ? (_calculatedColumns[j].DurMaxWidth + _calculatedColumns[j].DurMaxWidth * (num15 - num3) / num3) : (flag5 ? _calculatedColumns[j].DurMaxWidth : (flag6 ? _calculatedColumns[j].DurMinWidth : ((0.0 < num3 && !DoubleUtil.IsZero(num3)) ? (_calculatedColumns[j].DurMinWidth + _calculatedColumns[j].DurMaxWidth * (num15 - num4) / num3) : 0.0)))) : (flag10 ? (_calculatedColumns[j].DurMaxWidth - (_calculatedColumns[j].DurMaxWidth - _calculatedColumns[j].DurMinWidth) * (num3 - num15) / (num3 - num4)) : (_calculatedColumns[j].DurMaxWidth - _calculatedColumns[j].DurMaxWidth * (num3 - num15) / num3)));
			}
			_calculatedColumns[j].UrOffset = durTableWidth + internalCellSpacing / 2.0;
			durTableWidth += _calculatedColumns[j].DurWidth + internalCellSpacing;
			if (_calculatedColumns[j].PtsWidthChanged == 1)
			{
				result = 0;
			}
		}
		durTableWidth += mbpInfo.Margin.Left + TextDpi.FromTextDpi(mbpInfo.MBPRight);
		return result;
	}

	private PTS.FSRECT GetTableContentRect(MbpInfo mbpInfo)
	{
		int num = (IsFirstChunk ? mbpInfo.BPTop : 0);
		int num2 = (IsLastChunk ? mbpInfo.BPBottom : 0);
		return new PTS.FSRECT(_rect.u + mbpInfo.BPLeft, _rect.v + num, Math.Max(_rect.du - (mbpInfo.BPRight + mbpInfo.BPLeft), 1), Math.Max(_rect.dv - num2 - num, 1));
	}

	private int GetTableOffsetFirstRowTop()
	{
		MbpInfo mbpInfo = MbpInfo.FromElement(TableParagraph.Element, TableParagraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (!IsFirstChunk)
		{
			return 0;
		}
		return mbpInfo.BPTop;
	}
}
