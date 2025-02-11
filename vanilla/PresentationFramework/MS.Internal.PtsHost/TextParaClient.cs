using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class TextParaClient : BaseParaClient
{
	private int _lineIndexFirstVisual = -1;

	internal TextParagraph TextParagraph => (TextParagraph)_paragraph;

	internal bool HasEOP => IsLastChunk;

	internal override bool IsFirstChunk
	{
		get
		{
			PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
			Invariant.Assert(pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull, "Only 'full' text paragraph type is expected.");
			if (pTextDetails.u.full.cLines > 0)
			{
				return pTextDetails.u.full.dcpFirst == 0;
			}
			return false;
		}
	}

	internal override bool IsLastChunk
	{
		get
		{
			bool result = false;
			PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
			Invariant.Assert(pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull, "Only 'full' text paragraph type is expected.");
			if (pTextDetails.u.full.cLines > 0)
			{
				result = ((base.Paragraph.Cch <= 0) ? (pTextDetails.u.full.dcpLim == LineBase.SyntheticCharacterLength) : (pTextDetails.u.full.dcpLim >= base.Paragraph.Cch));
			}
			return result;
		}
	}

	private bool IsOptimalParagraph => TextParagraph.IsOptimalParagraph;

	internal TextParaClient(TextParagraph paragraph)
		: base(paragraph)
	{
	}

	internal override void ValidateVisual(PTS.FSKUPDATE fskupdInherited)
	{
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		VisualCollection children = _visual.Children;
		ContainerVisual visual = _visual;
		bool ignoreUpdateInfo = false;
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull)
		{
			if (children.Count != 0 && !(children[0] is LineVisual))
			{
				children.Clear();
				ignoreUpdateInfo = true;
			}
			if (IsDeferredVisualCreationSupported(ref pTextDetails.u.full))
			{
				if (_lineIndexFirstVisual == -1 && visual.Children.Count > 0)
				{
					ignoreUpdateInfo = true;
				}
				SyncUpdateDeferredLineVisuals(visual.Children, ref pTextDetails.u.full, ignoreUpdateInfo);
			}
			else
			{
				if (_lineIndexFirstVisual != -1)
				{
					_lineIndexFirstVisual = -1;
					visual.Children.Clear();
				}
				if (visual.Children.Count == 0)
				{
					ignoreUpdateInfo = true;
				}
				if (pTextDetails.u.full.cLines > 0)
				{
					if (!PTS.ToBoolean(pTextDetails.u.full.fLinesComposite))
					{
						RenderSimpleLines(visual, ref pTextDetails.u.full, ignoreUpdateInfo);
					}
					else
					{
						RenderCompositeLines(visual, ref pTextDetails.u.full, ignoreUpdateInfo);
					}
				}
				else
				{
					visual.Children.Clear();
				}
			}
			if (pTextDetails.u.full.cAttachedObjects > 0)
			{
				ValidateVisualFloatersAndFigures(fskupdInherited, pTextDetails.u.full.cAttachedObjects);
			}
		}
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			PTS.FSRECT pageRect = _pageContext.PageRect;
			PtsHelper.UpdateMirroringTransform(base.PageFlowDirection, base.ThisFlowDirection, visual, TextDpi.FromTextDpi(2 * pageRect.u + pageRect.du));
		}
	}

	internal override void UpdateViewport(ref PTS.FSRECT viewport)
	{
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		Invariant.Assert(pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull, "Only 'full' text paragraph type is expected.");
		if (IsDeferredVisualCreationSupported(ref pTextDetails.u.full))
		{
			ContainerVisual visual = _visual;
			UpdateViewportSimpleLines(visual, ref pTextDetails.u.full, ref viewport);
		}
		int cAttachedObjects = pTextDetails.u.full.cAttachedObjects;
		if (cAttachedObjects > 0)
		{
			PtsHelper.AttachedObjectListFromParagraph(base.PtsContext, _paraHandle.Value, cAttachedObjects, out var arrayAttachedObjectDesc);
			for (int i = 0; i < arrayAttachedObjectDesc.Length; i++)
			{
				PTS.FSATTACHEDOBJECTDESCRIPTION fSATTACHEDOBJECTDESCRIPTION = arrayAttachedObjectDesc[i];
				BaseParaClient obj = base.PtsContext.HandleToObject(fSATTACHEDOBJECTDESCRIPTION.pfsparaclient) as BaseParaClient;
				PTS.ValidateHandle(obj);
				obj.UpdateViewport(ref viewport);
			}
		}
	}

	internal override IInputElement InputHitTest(PTS.FSPOINT pt)
	{
		IInputElement inputElement = null;
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull)
		{
			PTS.FSPOINT pt2 = pt;
			if (base.ThisFlowDirection != base.PageFlowDirection)
			{
				pt2.u = _pageContext.PageRect.du - pt2.u;
			}
			if (pTextDetails.u.full.cLines > 0)
			{
				inputElement = (PTS.ToBoolean(pTextDetails.u.full.fLinesComposite) ? InputHitTestCompositeLines(pt2, ref pTextDetails.u.full) : InputHitTestSimpleLines(pt2, ref pTextDetails.u.full));
			}
		}
		if (inputElement == null)
		{
			inputElement = base.Paragraph.Element as IInputElement;
		}
		return inputElement;
	}

	internal override List<Rect> GetRectangles(ContentElement e, int start, int length)
	{
		List<Rect> list = new List<Rect>();
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull)
		{
			if (pTextDetails.u.full.cAttachedObjects > 0)
			{
				PtsHelper.AttachedObjectListFromParagraph(base.PtsContext, _paraHandle.Value, pTextDetails.u.full.cAttachedObjects, out var arrayAttachedObjectDesc);
				for (int i = 0; i < arrayAttachedObjectDesc.Length; i++)
				{
					PTS.FSATTACHEDOBJECTDESCRIPTION fSATTACHEDOBJECTDESCRIPTION = arrayAttachedObjectDesc[i];
					BaseParaClient baseParaClient = base.PtsContext.HandleToObject(fSATTACHEDOBJECTDESCRIPTION.pfsparaclient) as BaseParaClient;
					PTS.ValidateHandle(baseParaClient);
					if (start < baseParaClient.Paragraph.ParagraphEndCharacterPosition)
					{
						list = baseParaClient.GetRectangles(e, start, length);
						Invariant.Assert(list != null);
						if (list.Count != 0)
						{
							break;
						}
					}
				}
			}
			if (list.Count == 0 && pTextDetails.u.full.cLines > 0)
			{
				list = (PTS.ToBoolean(pTextDetails.u.full.fLinesComposite) ? GetRectanglesInCompositeLines(e, start, length, ref pTextDetails.u.full) : GetRectanglesInSimpleLines(e, start, length, ref pTextDetails.u.full));
				if (list.Count > 0 && base.ThisFlowDirection != base.PageFlowDirection)
				{
					PTS.FSRECT rectPage = _pageContext.PageRect;
					for (int j = 0; j < list.Count; j++)
					{
						PTS.FSRECT rectTransform = new PTS.FSRECT(list[j]);
						PTS.Validate(PTS.FsTransformRectangle(PTS.FlowDirectionToFswdir(base.ThisFlowDirection), ref rectPage, ref rectTransform, PTS.FlowDirectionToFswdir(base.PageFlowDirection), out rectTransform));
						list[j] = rectTransform.FromTextDpi();
					}
				}
			}
		}
		Invariant.Assert(list != null);
		return list;
	}

	internal override ParagraphResult CreateParagraphResult()
	{
		return new TextParagraphResult(this);
	}

	internal ReadOnlyCollection<LineResult> GetLineResults()
	{
		ReadOnlyCollection<LineResult> result = new ReadOnlyCollection<LineResult>(new List<LineResult>(0));
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull && pTextDetails.u.full.cLines > 0)
		{
			result = (PTS.ToBoolean(pTextDetails.u.full.fLinesComposite) ? LineResultsFromCompositeLines(ref pTextDetails.u.full) : LineResultsFromSimpleLines(ref pTextDetails.u.full));
		}
		return result;
	}

	internal ReadOnlyCollection<ParagraphResult> GetFloaters()
	{
		List<ParagraphResult> list = null;
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull && pTextDetails.u.full.cAttachedObjects > 0)
		{
			PtsHelper.AttachedObjectListFromParagraph(base.PtsContext, _paraHandle.Value, pTextDetails.u.full.cAttachedObjects, out var arrayAttachedObjectDesc);
			list = new List<ParagraphResult>(arrayAttachedObjectDesc.Length);
			for (int i = 0; i < arrayAttachedObjectDesc.Length; i++)
			{
				PTS.FSATTACHEDOBJECTDESCRIPTION fSATTACHEDOBJECTDESCRIPTION = arrayAttachedObjectDesc[i];
				BaseParaClient baseParaClient = base.PtsContext.HandleToObject(fSATTACHEDOBJECTDESCRIPTION.pfsparaclient) as BaseParaClient;
				PTS.ValidateHandle(baseParaClient);
				if (baseParaClient is FloaterParaClient)
				{
					list.Add(baseParaClient.CreateParagraphResult());
				}
			}
		}
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		return new ReadOnlyCollection<ParagraphResult>(list);
	}

	internal ReadOnlyCollection<ParagraphResult> GetFigures()
	{
		List<ParagraphResult> list = null;
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull && pTextDetails.u.full.cAttachedObjects > 0)
		{
			PtsHelper.AttachedObjectListFromParagraph(base.PtsContext, _paraHandle.Value, pTextDetails.u.full.cAttachedObjects, out var arrayAttachedObjectDesc);
			list = new List<ParagraphResult>(arrayAttachedObjectDesc.Length);
			for (int i = 0; i < arrayAttachedObjectDesc.Length; i++)
			{
				PTS.FSATTACHEDOBJECTDESCRIPTION fSATTACHEDOBJECTDESCRIPTION = arrayAttachedObjectDesc[i];
				BaseParaClient baseParaClient = base.PtsContext.HandleToObject(fSATTACHEDOBJECTDESCRIPTION.pfsparaclient) as BaseParaClient;
				PTS.ValidateHandle(baseParaClient);
				if (baseParaClient is FigureParaClient)
				{
					list.Add(baseParaClient.CreateParagraphResult());
				}
			}
		}
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		return new ReadOnlyCollection<ParagraphResult>(list);
	}

	internal override TextContentRange GetTextContentRange()
	{
		int num = 0;
		int num2 = 0;
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		Invariant.Assert(pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull, "Only 'full' text paragraph type is expected.");
		num = pTextDetails.u.full.dcpFirst;
		num2 = pTextDetails.u.full.dcpLim;
		if (HasEOP && num2 > base.Paragraph.Cch)
		{
			ErrorHandler.Assert(num2 == base.Paragraph.Cch + LineBase.SyntheticCharacterLength, ErrorHandler.ParagraphCharacterCountMismatch);
			num2 -= LineBase.SyntheticCharacterLength;
		}
		int paragraphStartCharacterPosition = base.Paragraph.ParagraphStartCharacterPosition;
		TextContentRange textContentRange;
		if (TextParagraph.HasFiguresOrFloaters())
		{
			PTS.FSATTACHEDOBJECTDESCRIPTION[] arrayAttachedObjectDesc = null;
			int cAttachedObjects = pTextDetails.u.full.cAttachedObjects;
			textContentRange = new TextContentRange();
			if (cAttachedObjects > 0)
			{
				PtsHelper.AttachedObjectListFromParagraph(base.PtsContext, _paraHandle.Value, cAttachedObjects, out arrayAttachedObjectDesc);
			}
			TextParagraph.UpdateTextContentRangeFromAttachedObjects(textContentRange, paragraphStartCharacterPosition + num, paragraphStartCharacterPosition + num2, arrayAttachedObjectDesc);
		}
		else
		{
			textContentRange = new TextContentRange(paragraphStartCharacterPosition + num, paragraphStartCharacterPosition + num2, base.Paragraph.StructuralCache.TextContainer);
		}
		return textContentRange;
	}

	internal void GetLineDetails(int dcpLine, out int cchContent, out int cchEllipses)
	{
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		int width = 0;
		bool firstLine = dcpLine == 0;
		int num = 0;
		nint pbrLineIn = IntPtr.Zero;
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull)
		{
			if (pTextDetails.u.full.cLines > 0)
			{
				if (!PTS.ToBoolean(pTextDetails.u.full.fLinesComposite))
				{
					PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref pTextDetails.u.full, out var arrayLineDesc);
					for (int i = 0; i < arrayLineDesc.Length; i++)
					{
						PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
						if (dcpLine == fSLINEDESCRIPTIONSINGLE.dcpFirst)
						{
							width = fSLINEDESCRIPTIONSINGLE.dur;
							num = fSLINEDESCRIPTIONSINGLE.dcpLim;
							pbrLineIn = fSLINEDESCRIPTIONSINGLE.pfsbreakreclineclient;
							break;
						}
					}
				}
				else
				{
					PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref pTextDetails.u.full, out var arrayLineDesc2);
					for (int j = 0; j < arrayLineDesc2.Length; j++)
					{
						PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc2[j];
						if (lineDesc.cElements == 0)
						{
							continue;
						}
						PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
						int k;
						for (k = 0; k < arrayLineElement.Length; k++)
						{
							PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[k];
							if (fSLINEELEMENT.dcpFirst == dcpLine)
							{
								width = fSLINEELEMENT.dur;
								num = fSLINEELEMENT.dcpLim;
								pbrLineIn = fSLINEELEMENT.pfsbreakreclineclient;
								break;
							}
						}
						if (k < arrayLineElement.Length)
						{
							firstLine = j == 0;
							break;
						}
					}
				}
			}
		}
		else
		{
			Invariant.Assert(pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdCached);
			Invariant.Assert(condition: false, "Should not get here. ParaCache is not currently used.");
		}
		Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, clearOnLeft: true, clearOnRight: true, TextParagraph.TextRunCache);
		Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
		if (IsOptimalParagraph)
		{
			formattingContext.LineFormatLengthTarget = num - dcpLine;
		}
		TextParagraph.FormatLineCore(line, pbrLineIn, formattingContext, dcpLine, width, firstLine, dcpLine);
		Invariant.Assert(line.SafeLength == num - dcpLine, "Line length is out of sync");
		cchContent = line.ContentLength;
		cchEllipses = line.GetEllipsesLength();
		line.Dispose();
	}

	internal override int GetFirstTextLineBaseline()
	{
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		Invariant.Assert(pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull, "Only 'full' text paragraph type is expected.");
		Rect rect = System.Windows.Rect.Empty;
		int vrBaseline = 0;
		if (!PTS.ToBoolean(pTextDetails.u.full.fLinesComposite))
		{
			RectFromDcpSimpleLines(0, 0, LogicalDirection.Forward, TextPointerContext.Text, ref pTextDetails.u.full, ref rect, ref vrBaseline);
		}
		else
		{
			RectFromDcpCompositeLines(0, 0, LogicalDirection.Forward, TextPointerContext.Text, ref pTextDetails.u.full, ref rect, ref vrBaseline);
		}
		return vrBaseline;
	}

	internal ITextPointer GetTextPosition(int dcp, LogicalDirection direction)
	{
		return TextContainerHelper.GetTextPointerFromCP(base.Paragraph.StructuralCache.TextContainer, dcp + base.Paragraph.ParagraphStartCharacterPosition, direction);
	}

	internal Rect GetRectangleFromTextPosition(ITextPointer position)
	{
		Rect rect = System.Windows.Rect.Empty;
		int num = base.Paragraph.StructuralCache.TextContainer.Start.GetOffsetToPosition((TextPointer)position) - base.Paragraph.ParagraphStartCharacterPosition;
		int originalDcp = num;
		if (position.LogicalDirection == LogicalDirection.Backward && num > 0)
		{
			num--;
		}
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull && pTextDetails.u.full.cLines > 0)
		{
			int vrBaseline = 0;
			if (!PTS.ToBoolean(pTextDetails.u.full.fLinesComposite))
			{
				RectFromDcpSimpleLines(num, originalDcp, position.LogicalDirection, position.GetPointerContext(position.LogicalDirection), ref pTextDetails.u.full, ref rect, ref vrBaseline);
			}
			else
			{
				RectFromDcpCompositeLines(num, originalDcp, position.LogicalDirection, position.GetPointerContext(position.LogicalDirection), ref pTextDetails.u.full, ref rect, ref vrBaseline);
			}
		}
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			PTS.FSRECT rectPage = _pageContext.PageRect;
			PTS.FSRECT rectTransform = new PTS.FSRECT(rect);
			PTS.Validate(PTS.FsTransformRectangle(PTS.FlowDirectionToFswdir(base.ThisFlowDirection), ref rectPage, ref rectTransform, PTS.FlowDirectionToFswdir(base.PageFlowDirection), out rectTransform));
			rect = rectTransform.FromTextDpi();
		}
		return rect;
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition, double paragraphTopSpace, Rect visibleRect)
	{
		Geometry geometry = null;
		Geometry geometry2 = null;
		int offset = startPosition.Offset;
		int paragraphStartCharacterPosition = base.Paragraph.ParagraphStartCharacterPosition;
		int dcpStart = Math.Max(offset, paragraphStartCharacterPosition) - paragraphStartCharacterPosition;
		int offset2 = endPosition.Offset;
		int paragraphEndCharacterPosition = base.Paragraph.ParagraphEndCharacterPosition;
		int dcpEnd = Math.Min(offset2, paragraphEndCharacterPosition) - paragraphStartCharacterPosition;
		double paragraphTopSpace2 = ((offset < paragraphStartCharacterPosition) ? paragraphTopSpace : 0.0);
		bool handleEndOfPara = offset2 > paragraphEndCharacterPosition;
		Transform transform = null;
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			transform = new MatrixTransform(-1.0, 0.0, 0.0, 1.0, TextDpi.FromTextDpi(2 * _pageContext.PageRect.u + _pageContext.PageRect.du), 0.0);
			visibleRect = transform.TransformBounds(visibleRect);
		}
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull)
		{
			if (pTextDetails.u.full.cLines > 0)
			{
				geometry = (PTS.ToBoolean(pTextDetails.u.full.fLinesComposite) ? PathGeometryFromDcpRangeCompositeLines(dcpStart, dcpEnd, paragraphTopSpace2, handleEndOfPara, ref pTextDetails.u.full, visibleRect) : PathGeometryFromDcpRangeSimpleLines(dcpStart, dcpEnd, paragraphTopSpace2, handleEndOfPara, ref pTextDetails.u.full, visibleRect));
			}
			if (pTextDetails.u.full.cAttachedObjects > 0)
			{
				geometry2 = PathGeometryFromDcpRangeFloatersAndFigures(offset, offset2, ref pTextDetails.u.full);
			}
		}
		if (geometry != null && transform != null)
		{
			CaretElement.AddTransformToGeometry(geometry, transform);
		}
		if (geometry2 != null)
		{
			CaretElement.AddGeometry(ref geometry, geometry2);
		}
		return geometry;
	}

	internal bool IsAtCaretUnitBoundary(ITextPointer position)
	{
		bool result = false;
		int dcp = base.Paragraph.StructuralCache.TextContainer.Start.GetOffsetToPosition(position as TextPointer) - base.Paragraph.ParagraphStartCharacterPosition;
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull && pTextDetails.u.full.cLines > 0)
		{
			result = (PTS.ToBoolean(pTextDetails.u.full.fLinesComposite) ? IsAtCaretUnitBoundaryFromDcpCompositeLines(dcp, position, ref pTextDetails.u.full) : IsAtCaretUnitBoundaryFromDcpSimpleLines(dcp, position, ref pTextDetails.u.full));
		}
		return result;
	}

	internal ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		ITextPointer result = position;
		int dcp = base.Paragraph.StructuralCache.TextContainer.Start.GetOffsetToPosition(position as TextPointer) - base.Paragraph.ParagraphStartCharacterPosition;
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull && pTextDetails.u.full.cLines > 0)
		{
			result = (PTS.ToBoolean(pTextDetails.u.full.fLinesComposite) ? NextCaretUnitPositionFromDcpCompositeLines(dcp, position, direction, ref pTextDetails.u.full) : NextCaretUnitPositionFromDcpSimpleLines(dcp, position, direction, ref pTextDetails.u.full));
		}
		return result;
	}

	internal ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		ITextPointer result = position;
		Invariant.Assert(position is TextPointer);
		int dcp = base.Paragraph.StructuralCache.TextContainer.Start.GetOffsetToPosition(position as TextPointer) - base.Paragraph.ParagraphStartCharacterPosition;
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull)
		{
			if (pTextDetails.u.full.cLines > 0)
			{
				result = (PTS.ToBoolean(pTextDetails.u.full.fLinesComposite) ? BackspaceCaretUnitPositionFromDcpCompositeLines(dcp, position, ref pTextDetails.u.full) : BackspaceCaretUnitPositionFromDcpSimpleLines(dcp, position, ref pTextDetails.u.full));
			}
		}
		else
		{
			Invariant.Assert(pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdCached);
			Invariant.Assert(condition: false, "Should not get here. ParaCache is not currently used.");
		}
		return result;
	}

	internal ITextPointer GetTextPositionFromDistance(int dcpLine, double distance)
	{
		int num = TextDpi.ToTextDpi(distance);
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			num = _pageContext.PageRect.du - num;
		}
		int width = 0;
		bool firstLine = dcpLine == 0;
		int num2 = 0;
		nint pbrLineIn = IntPtr.Zero;
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull && pTextDetails.u.full.cLines > 0)
		{
			if (!PTS.ToBoolean(pTextDetails.u.full.fLinesComposite))
			{
				PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref pTextDetails.u.full, out var arrayLineDesc);
				for (int i = 0; i < arrayLineDesc.Length; i++)
				{
					PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
					if (dcpLine == fSLINEDESCRIPTIONSINGLE.dcpFirst)
					{
						width = fSLINEDESCRIPTIONSINGLE.dur;
						num -= fSLINEDESCRIPTIONSINGLE.urStart;
						num2 = fSLINEDESCRIPTIONSINGLE.dcpLim;
						pbrLineIn = fSLINEDESCRIPTIONSINGLE.pfsbreakreclineclient;
						break;
					}
				}
			}
			else
			{
				PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref pTextDetails.u.full, out var arrayLineDesc2);
				for (int j = 0; j < arrayLineDesc2.Length; j++)
				{
					PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc2[j];
					if (lineDesc.cElements == 0)
					{
						continue;
					}
					PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
					int k;
					for (k = 0; k < arrayLineElement.Length; k++)
					{
						PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[k];
						if (fSLINEELEMENT.dcpFirst == dcpLine)
						{
							width = fSLINEELEMENT.dur;
							num -= fSLINEELEMENT.urStart;
							num2 = fSLINEELEMENT.dcpLim;
							pbrLineIn = fSLINEELEMENT.pfsbreakreclineclient;
							break;
						}
					}
					if (k < arrayLineElement.Length)
					{
						firstLine = j == 0;
						break;
					}
				}
			}
		}
		Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, clearOnLeft: true, clearOnRight: true, TextParagraph.TextRunCache);
		Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
		if (IsOptimalParagraph)
		{
			formattingContext.LineFormatLengthTarget = num2 - dcpLine;
		}
		TextParagraph.FormatLineCore(line, pbrLineIn, formattingContext, dcpLine, width, firstLine, dcpLine);
		Invariant.Assert(line.SafeLength == num2 - dcpLine, "Line length is out of sync");
		CharacterHit textPositionFromDistance = line.GetTextPositionFromDistance(num);
		int num3 = textPositionFromDistance.FirstCharacterIndex + textPositionFromDistance.TrailingLength;
		int lastDcpAttachedObjectBeforeLine = TextParagraph.GetLastDcpAttachedObjectBeforeLine(dcpLine);
		if (num3 < lastDcpAttachedObjectBeforeLine)
		{
			num3 = lastDcpAttachedObjectBeforeLine;
		}
		StaticTextPointer staticTextPointerFromCP = TextContainerHelper.GetStaticTextPointerFromCP(base.Paragraph.StructuralCache.TextContainer, num3 + base.Paragraph.ParagraphStartCharacterPosition);
		LogicalDirection direction = ((textPositionFromDistance.TrailingLength <= 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
		line.Dispose();
		return staticTextPointerFromCP.CreateDynamicTextPointer(direction);
	}

	internal void GetGlyphRuns(List<GlyphRun> glyphRuns, ITextPointer start, ITextPointer end)
	{
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdFull)
		{
			int num = base.Paragraph.StructuralCache.TextContainer.Start.GetOffsetToPosition((TextPointer)start) - base.Paragraph.ParagraphStartCharacterPosition;
			int num2 = base.Paragraph.StructuralCache.TextContainer.Start.GetOffsetToPosition((TextPointer)end) - base.Paragraph.ParagraphStartCharacterPosition;
			Invariant.Assert(num >= pTextDetails.u.full.dcpFirst && num2 <= pTextDetails.u.full.dcpLim);
			if (pTextDetails.u.full.cLines > 0)
			{
				if (!PTS.ToBoolean(pTextDetails.u.full.fLinesComposite))
				{
					GetGlyphRunsFromSimpleLines(glyphRuns, num, num2, ref pTextDetails.u.full);
				}
				else
				{
					GetGlyphRunsFromCompositeLines(glyphRuns, num, num2, ref pTextDetails.u.full);
				}
			}
		}
		else
		{
			Invariant.Assert(pTextDetails.fsktd == PTS.FSKTEXTDETAILS.fsktdCached);
			Invariant.Assert(condition: false, "Should not get here. ParaCache is not currently used.");
		}
	}

	protected override void OnArrange()
	{
		base.OnArrange();
		if (!TextParagraph.HasFiguresFloatersOrInlineObjects())
		{
			return;
		}
		PTS.Validate(PTS.FsQueryTextDetails(base.PtsContext.Context, _paraHandle.Value, out var pTextDetails));
		if (pTextDetails.fsktd != PTS.FSKTEXTDETAILS.fsktdFull)
		{
			return;
		}
		if (pTextDetails.u.full.cLines > 0)
		{
			if (!PTS.ToBoolean(pTextDetails.u.full.fLinesComposite))
			{
				PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref pTextDetails.u.full, out var arrayLineDesc);
				for (int i = 0; i < arrayLineDesc.Length; i++)
				{
					PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
					List<InlineObject> list = TextParagraph.InlineObjectsFromRange(fSLINEDESCRIPTIONSINGLE.dcpFirst, fSLINEDESCRIPTIONSINGLE.dcpLim);
					if (list == null)
					{
						continue;
					}
					for (int j = 0; j < list.Count; j++)
					{
						UIElement uIElement = (UIElement)list[j].Element;
						if (uIElement.IsMeasureValid && !uIElement.IsArrangeValid)
						{
							uIElement.Arrange(new Rect(uIElement.DesiredSize));
						}
					}
				}
			}
			else
			{
				PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref pTextDetails.u.full, out var arrayLineDesc2);
				for (int k = 0; k < arrayLineDesc2.Length; k++)
				{
					PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc2[k];
					PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
					for (int l = 0; l < arrayLineElement.Length; l++)
					{
						PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[l];
						List<InlineObject> list2 = TextParagraph.InlineObjectsFromRange(fSLINEELEMENT.dcpFirst, fSLINEELEMENT.dcpLim);
						if (list2 == null)
						{
							continue;
						}
						for (int m = 0; m < list2.Count; m++)
						{
							UIElement uIElement2 = (UIElement)list2[m].Element;
							if (uIElement2.IsMeasureValid && !uIElement2.IsArrangeValid)
							{
								uIElement2.Arrange(new Rect(uIElement2.DesiredSize));
							}
						}
					}
				}
			}
		}
		if (pTextDetails.u.full.cAttachedObjects <= 0)
		{
			return;
		}
		PtsHelper.AttachedObjectListFromParagraph(base.PtsContext, _paraHandle.Value, pTextDetails.u.full.cAttachedObjects, out var arrayAttachedObjectDesc);
		for (int n = 0; n < arrayAttachedObjectDesc.Length; n++)
		{
			PTS.FSATTACHEDOBJECTDESCRIPTION fSATTACHEDOBJECTDESCRIPTION = arrayAttachedObjectDesc[n];
			BaseParaClient baseParaClient = base.PtsContext.HandleToObject(fSATTACHEDOBJECTDESCRIPTION.pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(baseParaClient);
			if (baseParaClient is FloaterParaClient)
			{
				PTS.Validate(PTS.FsQueryFloaterDetails(base.PtsContext.Context, fSATTACHEDOBJECTDESCRIPTION.pfspara, out var fsfloaterdetails));
				PTS.FSRECT rectTransform = fsfloaterdetails.fsrcFloater;
				if (base.ThisFlowDirection != base.PageFlowDirection)
				{
					PTS.FSRECT rectPage = _pageContext.PageRect;
					PTS.Validate(PTS.FsTransformRectangle(PTS.FlowDirectionToFswdir(base.ThisFlowDirection), ref rectPage, ref rectTransform, PTS.FlowDirectionToFswdir(base.PageFlowDirection), out rectTransform));
				}
				((FloaterParaClient)baseParaClient).ArrangeFloater(rectTransform, _rect, PTS.FlowDirectionToFswdir(base.ThisFlowDirection), _pageContext);
			}
			else if (baseParaClient is FigureParaClient)
			{
				PTS.Validate(PTS.FsQueryFigureObjectDetails(base.PtsContext.Context, fSATTACHEDOBJECTDESCRIPTION.pfspara, out var fsFigureDetails));
				PTS.FSRECT rectTransform2 = fsFigureDetails.fsrcFlowAround;
				if (base.ThisFlowDirection != base.PageFlowDirection)
				{
					PTS.FSRECT rectPage2 = _pageContext.PageRect;
					PTS.Validate(PTS.FsTransformRectangle(PTS.FlowDirectionToFswdir(base.ThisFlowDirection), ref rectPage2, ref rectTransform2, PTS.FlowDirectionToFswdir(base.PageFlowDirection), out rectTransform2));
				}
				((FigureParaClient)baseParaClient).ArrangeFigure(rectTransform2, _rect, PTS.FlowDirectionToFswdir(base.ThisFlowDirection), _pageContext);
			}
			else
			{
				Invariant.Assert(condition: false, "Attached object not figure or floater.");
			}
		}
	}

	private void SyncUpdateDeferredLineVisuals(VisualCollection lineVisuals, ref PTS.FSTEXTDETAILSFULL textDetails, bool ignoreUpdateInfo)
	{
		try
		{
			if (!PTS.ToBoolean(textDetails.fUpdateInfoForLinesPresent) || ignoreUpdateInfo || textDetails.cLines == 0)
			{
				lineVisuals.Clear();
			}
			else
			{
				if (_lineIndexFirstVisual == -1)
				{
					return;
				}
				PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
				int cLinesBeforeChange = textDetails.cLinesBeforeChange;
				int num = textDetails.cLinesChanged - textDetails.dcLinesChanged;
				int num2 = -1;
				if (textDetails.dvrShiftBeforeChange != 0)
				{
					int num3 = Math.Min(Math.Max(cLinesBeforeChange - _lineIndexFirstVisual, 0), lineVisuals.Count);
					for (int i = 0; i < num3; i++)
					{
						ContainerVisual obj = (ContainerVisual)lineVisuals[i];
						Vector offset = obj.Offset;
						offset.Y += TextDpi.FromTextDpi(textDetails.dvrShiftBeforeChange);
						obj.Offset = offset;
					}
				}
				if (cLinesBeforeChange < _lineIndexFirstVisual)
				{
					int num4 = Math.Min(Math.Max(cLinesBeforeChange - _lineIndexFirstVisual + num, 0), lineVisuals.Count);
					if (num4 > 0)
					{
						lineVisuals.RemoveRange(0, num4);
					}
					if (lineVisuals.Count == 0)
					{
						lineVisuals.Clear();
						_lineIndexFirstVisual = -1;
					}
					else
					{
						num2 = 0;
						_lineIndexFirstVisual = cLinesBeforeChange;
					}
				}
				else if (cLinesBeforeChange < _lineIndexFirstVisual + lineVisuals.Count)
				{
					int count = Math.Min(num, lineVisuals.Count - (cLinesBeforeChange - _lineIndexFirstVisual));
					lineVisuals.RemoveRange(cLinesBeforeChange - _lineIndexFirstVisual, count);
					num2 = cLinesBeforeChange - _lineIndexFirstVisual;
				}
				int num5 = -1;
				if (num2 != -1)
				{
					for (int j = textDetails.cLinesBeforeChange; j < textDetails.cLinesBeforeChange + textDetails.cLinesChanged; j++)
					{
						PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[j];
						ContainerVisual containerVisual = CreateLineVisual(ref arrayLineDesc[j], base.Paragraph.ParagraphStartCharacterPosition);
						lineVisuals.Insert(num2 + (j - textDetails.cLinesBeforeChange), containerVisual);
						containerVisual.Offset = new Vector(TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.urStart), TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.vrStart));
					}
					num5 = num2 + textDetails.cLinesChanged;
				}
				if (num5 != -1)
				{
					for (int k = num5; k < lineVisuals.Count; k++)
					{
						ContainerVisual obj2 = (ContainerVisual)lineVisuals[k];
						Vector offset2 = obj2.Offset;
						offset2.Y += TextDpi.FromTextDpi(textDetails.dvrShiftAfterChange);
						obj2.Offset = offset2;
					}
				}
			}
		}
		finally
		{
			if (lineVisuals.Count == 0)
			{
				_lineIndexFirstVisual = -1;
			}
		}
	}

	private ReadOnlyCollection<LineResult> LineResultsFromSimpleLines(ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return null;
		}
		PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		List<LineResult> list = new List<LineResult>(arrayLineDesc.Length);
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
			Rect rect = new Rect(TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.urBBox), TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.vrStart), TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.durBBox), TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.dvrAscent + fSLINEDESCRIPTIONSINGLE.dvrDescent));
			if (base.PageFlowDirection != base.ThisFlowDirection)
			{
				PTS.FSRECT rectPage = _pageContext.PageRect;
				PTS.FSRECT rectTransform = new PTS.FSRECT(rect);
				PTS.Validate(PTS.FsTransformRectangle(PTS.FlowDirectionToFswdir(base.ThisFlowDirection), ref rectPage, ref rectTransform, PTS.FlowDirectionToFswdir(base.PageFlowDirection), out rectTransform));
				rect = rectTransform.FromTextDpi();
			}
			list.Add(new TextParaLineResult(this, fSLINEDESCRIPTIONSINGLE.dcpFirst, fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst, rect, TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.dvrAscent)));
		}
		if (list.Count != 0)
		{
			TextParaLineResult textParaLineResult = (TextParaLineResult)list[list.Count - 1];
			if (HasEOP && textParaLineResult.DcpLast > base.Paragraph.Cch)
			{
				ErrorHandler.Assert(textParaLineResult.DcpLast - LineBase.SyntheticCharacterLength == base.Paragraph.Cch, ErrorHandler.ParagraphCharacterCountMismatch);
				textParaLineResult.DcpLast -= LineBase.SyntheticCharacterLength;
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return new ReadOnlyCollection<LineResult>(list);
	}

	private ReadOnlyCollection<LineResult> LineResultsFromCompositeLines(ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return null;
		}
		PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		List<LineResult> list = new List<LineResult>(arrayLineDesc.Length);
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc[i];
			if (lineDesc.cElements == 0)
			{
				continue;
			}
			PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
			for (int j = 0; j < arrayLineElement.Length; j++)
			{
				PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[j];
				Rect rect = new Rect(TextDpi.FromTextDpi(fSLINEELEMENT.urBBox), TextDpi.FromTextDpi(lineDesc.vrStart), TextDpi.FromTextDpi(fSLINEELEMENT.durBBox), TextDpi.FromTextDpi(fSLINEELEMENT.dvrAscent + fSLINEELEMENT.dvrDescent));
				if (base.ThisFlowDirection != base.PageFlowDirection)
				{
					PTS.FSRECT rectPage = _pageContext.PageRect;
					PTS.FSRECT rectTransform = new PTS.FSRECT(rect);
					PTS.Validate(PTS.FsTransformRectangle(PTS.FlowDirectionToFswdir(base.ThisFlowDirection), ref rectPage, ref rectTransform, PTS.FlowDirectionToFswdir(base.PageFlowDirection), out rectTransform));
					rect = rectTransform.FromTextDpi();
				}
				list.Add(new TextParaLineResult(this, fSLINEELEMENT.dcpFirst, fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, rect, TextDpi.FromTextDpi(fSLINEELEMENT.dvrAscent)));
			}
		}
		if (list.Count != 0)
		{
			TextParaLineResult textParaLineResult = (TextParaLineResult)list[list.Count - 1];
			if (HasEOP && textParaLineResult.DcpLast > base.Paragraph.Cch)
			{
				ErrorHandler.Assert(textParaLineResult.DcpLast - LineBase.SyntheticCharacterLength == base.Paragraph.Cch, ErrorHandler.ParagraphCharacterCountMismatch);
				textParaLineResult.DcpLast -= LineBase.SyntheticCharacterLength;
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return new ReadOnlyCollection<LineResult>(list);
	}

	private void RectFromDcpSimpleLines(int dcp, int originalDcp, LogicalDirection orientation, TextPointerContext context, ref PTS.FSTEXTDETAILSFULL textDetails, ref Rect rect, ref int vrBaseline)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return;
		}
		PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
			if ((fSLINEDESCRIPTIONSINGLE.dcpFirst > dcp || fSLINEDESCRIPTIONSINGLE.dcpLim <= dcp) && (fSLINEDESCRIPTIONSINGLE.dcpLim != dcp || i != arrayLineDesc.Length - 1))
			{
				continue;
			}
			Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
			Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnLeft), PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnRight), TextParagraph.TextRunCache);
			if (IsOptimalParagraph)
			{
				formattingContext.LineFormatLengthTarget = fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst;
			}
			TextParagraph.FormatLineCore(line, fSLINEDESCRIPTIONSINGLE.pfsbreakreclineclient, formattingContext, fSLINEDESCRIPTIONSINGLE.dcpFirst, fSLINEDESCRIPTIONSINGLE.dur, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fTreatedAsFirst), fSLINEDESCRIPTIONSINGLE.dcpFirst);
			Invariant.Assert(line.SafeLength == fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst, "Line length is out of sync");
			rect = line.GetBoundsFromTextPosition(dcp, out var flowDirection);
			rect.X += TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.urStart);
			rect.Y += TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.vrStart);
			if (base.ThisFlowDirection != flowDirection)
			{
				if (orientation == LogicalDirection.Forward)
				{
					rect.X = rect.Right;
				}
			}
			else if (orientation == LogicalDirection.Backward && originalDcp > 0 && (context == TextPointerContext.Text || context == TextPointerContext.EmbeddedElement))
			{
				rect.X = rect.Right;
			}
			rect.Width = 0.0;
			vrBaseline = line.Baseline + fSLINEDESCRIPTIONSINGLE.vrStart;
			line.Dispose();
			break;
		}
	}

	private void RectFromDcpCompositeLines(int dcp, int originalDcp, LogicalDirection orientation, TextPointerContext context, ref PTS.FSTEXTDETAILSFULL textDetails, ref Rect rect, ref int vrBaseline)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return;
		}
		PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc[i];
			if (lineDesc.cElements == 0)
			{
				continue;
			}
			PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
			for (int j = 0; j < arrayLineElement.Length; j++)
			{
				PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[j];
				if ((fSLINEELEMENT.dcpFirst > dcp || fSLINEELEMENT.dcpLim <= dcp) && (fSLINEELEMENT.dcpLim != dcp || j != arrayLineElement.Length - 1 || i != arrayLineDesc.Length - 1))
				{
					continue;
				}
				Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
				Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEELEMENT.fClearOnLeft), PTS.ToBoolean(fSLINEELEMENT.fClearOnRight), TextParagraph.TextRunCache);
				if (IsOptimalParagraph)
				{
					formattingContext.LineFormatLengthTarget = fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst;
				}
				TextParagraph.FormatLineCore(line, fSLINEELEMENT.pfsbreakreclineclient, formattingContext, fSLINEELEMENT.dcpFirst, fSLINEELEMENT.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), fSLINEELEMENT.dcpFirst);
				Invariant.Assert(line.SafeLength == fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, "Line length is out of sync");
				rect = line.GetBoundsFromTextPosition(dcp, out var flowDirection);
				rect.X += TextDpi.FromTextDpi(fSLINEELEMENT.urStart);
				rect.Y += TextDpi.FromTextDpi(lineDesc.vrStart);
				if (base.ThisFlowDirection != flowDirection)
				{
					if (orientation == LogicalDirection.Forward)
					{
						rect.X = rect.Right;
					}
				}
				else if (orientation == LogicalDirection.Backward && originalDcp > 0 && (context == TextPointerContext.Text || context == TextPointerContext.EmbeddedElement))
				{
					rect.X = rect.Right;
				}
				rect.Width = 0.0;
				vrBaseline = line.Baseline + lineDesc.vrStart;
				line.Dispose();
				break;
			}
		}
	}

	private Geometry PathGeometryFromDcpRangeSimpleLines(int dcpStart, int dcpEnd, double paragraphTopSpace, bool handleEndOfPara, ref PTS.FSTEXTDETAILSFULL textDetails, Rect visibleRect)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return null;
		}
		Geometry geometry = null;
		PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		int num = 0;
		int num2 = arrayLineDesc.Length;
		if (_lineIndexFirstVisual != -1)
		{
			num = _lineIndexFirstVisual;
			num2 = _visual.Children.Count;
		}
		for (int i = num; i < num + num2; i++)
		{
			PTS.FSLINEDESCRIPTIONSINGLE lineDesc = arrayLineDesc[i];
			if (handleEndOfPara)
			{
				if (dcpEnd < lineDesc.dcpFirst)
				{
					break;
				}
			}
			else if (dcpEnd <= lineDesc.dcpFirst)
			{
				break;
			}
			if (lineDesc.dcpLim <= dcpStart && (i != arrayLineDesc.Length - 1 || lineDesc.dcpLim != dcpStart))
			{
				continue;
			}
			int num3 = Math.Max(lineDesc.dcpFirst, dcpStart);
			int cchRange = Math.Max(Math.Min(lineDesc.dcpLim, dcpEnd) - num3, 1);
			double lineTopSpace = ((i == 0) ? paragraphTopSpace : 0.0);
			double lineRightSpace = (((!handleEndOfPara || i != arrayLineDesc.Length - 1) && (dcpEnd < lineDesc.dcpLim || !HasAnyLineBreakAtCp(lineDesc.dcpLim))) ? 0.0 : ((double)TextParagraph.Element.GetValue(TextElement.FontSizeProperty) * 0.5));
			IList<Rect> list = RectanglesFromDcpRangeOfSimpleLine(num3, cchRange, lineTopSpace, lineRightSpace, ref lineDesc, i, visibleRect);
			if (list != null)
			{
				int j = 0;
				for (int count = list.Count; j < count; j++)
				{
					RectangleGeometry addedGeometry = new RectangleGeometry(list[j]);
					CaretElement.AddGeometry(ref geometry, addedGeometry);
				}
			}
		}
		return geometry;
	}

	private Geometry PathGeometryFromDcpRangeCompositeLines(int dcpStart, int dcpEnd, double paragraphTopSpace, bool handleEndOfPara, ref PTS.FSTEXTDETAILSFULL textDetails, Rect visibleRect)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return null;
		}
		Geometry geometry = null;
		PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc[i];
			if (lineDesc.cElements == 0)
			{
				continue;
			}
			PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
			for (int j = 0; j < arrayLineElement.Length; j++)
			{
				PTS.FSLINEELEMENT elemDesc = arrayLineElement[j];
				if (handleEndOfPara)
				{
					if (dcpEnd < elemDesc.dcpFirst)
					{
						break;
					}
				}
				else if (dcpEnd <= elemDesc.dcpFirst)
				{
					break;
				}
				if (elemDesc.dcpLim <= dcpStart && (elemDesc.dcpLim != dcpStart || j != arrayLineElement.Length - 1 || i != arrayLineDesc.Length - 1))
				{
					continue;
				}
				int num = Math.Max(elemDesc.dcpFirst, dcpStart);
				int cchRange = Math.Max(Math.Min(elemDesc.dcpLim, dcpEnd) - num, 1);
				double lineTopSpace = ((i == 0) ? paragraphTopSpace : 0.0);
				double lineRightSpace = (((!handleEndOfPara || i != arrayLineDesc.Length - 1) && (dcpEnd < elemDesc.dcpLim || !HasAnyLineBreakAtCp(elemDesc.dcpLim))) ? 0.0 : ((double)TextParagraph.Element.GetValue(TextElement.FontSizeProperty) * 0.5));
				IList<Rect> list = RectanglesFromDcpRangeOfCompositeLineElement(num, cchRange, lineTopSpace, lineRightSpace, ref lineDesc, i, ref elemDesc, j, visibleRect);
				if (list != null)
				{
					int k = 0;
					for (int count = list.Count; k < count; k++)
					{
						RectangleGeometry addedGeometry = new RectangleGeometry(list[k]);
						CaretElement.AddGeometry(ref geometry, addedGeometry);
					}
				}
			}
		}
		return geometry;
	}

	private bool HasAnyLineBreakAtCp(int dcp)
	{
		return TextPointerBase.IsNextToAnyBreak(base.Paragraph.StructuralCache.TextContainer.CreatePointerAtOffset(base.Paragraph.ParagraphStartCharacterPosition + dcp, LogicalDirection.Forward), LogicalDirection.Backward);
	}

	private List<Rect> RectanglesFromDcpRangeOfSimpleLine(int dcpRangeStart, int cchRange, double lineTopSpace, double lineRightSpace, ref PTS.FSLINEDESCRIPTIONSINGLE lineDesc, int lineIndex, Rect visibleRect)
	{
		List<Rect> list = null;
		Invariant.Assert(lineDesc.dcpFirst <= dcpRangeStart && dcpRangeStart <= lineDesc.dcpLim && cchRange > 0);
		Rect rect = new PTS.FSRECT(lineDesc.urBBox, lineDesc.vrStart, lineDesc.durBBox, lineDesc.dvrAscent + lineDesc.dvrDescent).FromTextDpi();
		LineVisual lineVisual = FetchLineVisual(lineIndex);
		if (lineVisual != null)
		{
			rect.Width = Math.Max(lineVisual.WidthIncludingTrailingWhitespace, 0.0);
		}
		rect.Y -= lineTopSpace;
		rect.Height += lineTopSpace;
		rect.Width += lineRightSpace;
		Rect rect2 = rect;
		rect2.X = visibleRect.X;
		if (rect2.IntersectsWith(visibleRect))
		{
			if (dcpRangeStart == lineDesc.dcpFirst && lineDesc.dcpLim <= dcpRangeStart + cchRange)
			{
				list = new List<Rect>(1);
				list.Add(rect);
			}
			else
			{
				Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
				Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(lineDesc.fClearOnLeft), PTS.ToBoolean(lineDesc.fClearOnRight), TextParagraph.TextRunCache);
				if (IsOptimalParagraph)
				{
					formattingContext.LineFormatLengthTarget = lineDesc.dcpLim - lineDesc.dcpFirst;
				}
				TextParagraph.FormatLineCore(line, lineDesc.pfsbreakreclineclient, formattingContext, lineDesc.dcpFirst, lineDesc.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), lineDesc.dcpFirst);
				Invariant.Assert(line.SafeLength == lineDesc.dcpLim - lineDesc.dcpFirst, "Line length is out of sync");
				double num = TextDpi.FromTextDpi(lineDesc.urStart);
				double num2 = TextDpi.FromTextDpi(lineDesc.vrStart);
				list = line.GetRangeBounds(dcpRangeStart, cchRange, num, num2);
				if (!DoubleUtil.IsZero(lineTopSpace))
				{
					int i = 0;
					for (int count = list.Count; i < count; i++)
					{
						Rect value = list[i];
						value.Y -= lineTopSpace;
						value.Height += lineTopSpace;
						list[i] = value;
					}
				}
				if (!DoubleUtil.IsZero(lineRightSpace))
				{
					list.Add(new Rect(num + TextDpi.FromTextDpi(line.Start + line.Width), num2 - lineTopSpace, lineRightSpace, TextDpi.FromTextDpi(line.Height) + lineTopSpace));
				}
				line.Dispose();
			}
		}
		return list;
	}

	private List<Rect> RectanglesFromDcpRangeOfCompositeLineElement(int dcpRangeStart, int cchRange, double lineTopSpace, double lineRightSpace, ref PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc, int lineIndex, ref PTS.FSLINEELEMENT elemDesc, int elemIndex, Rect visibleRect)
	{
		List<Rect> list = null;
		Rect rect = new PTS.FSRECT(elemDesc.urBBox, lineDesc.vrStart, elemDesc.durBBox, lineDesc.dvrAscent + lineDesc.dvrDescent).FromTextDpi();
		LineVisual lineVisual = FetchLineVisualComposite(lineIndex, elemIndex);
		if (lineVisual != null)
		{
			rect.Width = Math.Max(lineVisual.WidthIncludingTrailingWhitespace, 0.0);
		}
		rect.Y -= lineTopSpace;
		rect.Height += lineTopSpace;
		rect.Width += lineRightSpace;
		Rect rect2 = rect;
		rect2.X = visibleRect.X;
		if (rect2.IntersectsWith(visibleRect))
		{
			if (dcpRangeStart == elemDesc.dcpFirst && elemDesc.dcpLim <= dcpRangeStart + cchRange)
			{
				list = new List<Rect>(1);
				list.Add(rect);
			}
			else
			{
				Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
				Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(elemDesc.fClearOnLeft), PTS.ToBoolean(elemDesc.fClearOnRight), TextParagraph.TextRunCache);
				if (IsOptimalParagraph)
				{
					formattingContext.LineFormatLengthTarget = elemDesc.dcpLim - elemDesc.dcpFirst;
				}
				TextParagraph.FormatLineCore(line, elemDesc.pfsbreakreclineclient, formattingContext, elemDesc.dcpFirst, elemDesc.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), elemDesc.dcpFirst);
				Invariant.Assert(line.SafeLength == elemDesc.dcpLim - elemDesc.dcpFirst, "Line length is out of sync");
				double num = TextDpi.FromTextDpi(elemDesc.urStart);
				double num2 = TextDpi.FromTextDpi(lineDesc.vrStart);
				list = line.GetRangeBounds(dcpRangeStart, cchRange, num, num2);
				if (!DoubleUtil.IsZero(lineTopSpace))
				{
					int i = 0;
					for (int count = list.Count; i < count; i++)
					{
						Rect value = list[i];
						value.Y -= lineTopSpace;
						value.Height += lineTopSpace;
						list[i] = value;
					}
				}
				if (!DoubleUtil.IsZero(lineRightSpace))
				{
					list.Add(new Rect(num + TextDpi.FromTextDpi(line.Start + line.Width), num2 - lineTopSpace, lineRightSpace, TextDpi.FromTextDpi(line.Height) + lineTopSpace));
				}
				line.Dispose();
			}
		}
		return list;
	}

	private LineVisual FetchLineVisual(int index)
	{
		LineVisual lineVisual = null;
		int childrenCount = VisualTreeHelper.GetChildrenCount(Visual);
		if (childrenCount != 0)
		{
			int num = index;
			if (_lineIndexFirstVisual != -1)
			{
				num -= _lineIndexFirstVisual;
			}
			if (0 <= num && num < childrenCount)
			{
				lineVisual = VisualTreeHelper.GetChild(Visual, num) as LineVisual;
				Invariant.Assert(lineVisual != null || VisualTreeHelper.GetChild(Visual, num) == null);
			}
		}
		return lineVisual;
	}

	private LineVisual FetchLineVisualComposite(int lineIndex, int elemIndex)
	{
		LineVisual lineVisual = null;
		Visual reference = Visual;
		if (VisualTreeHelper.GetChildrenCount(Visual) != 0)
		{
			int childIndex = lineIndex;
			if (VisualTreeHelper.GetChild(Visual, childIndex) is ParagraphElementVisual)
			{
				reference = Visual.InternalGetVisualChild(lineIndex);
				childIndex = elemIndex;
			}
			lineVisual = VisualTreeHelper.GetChild(reference, childIndex) as LineVisual;
			Invariant.Assert(lineVisual != null || VisualTreeHelper.GetChild(reference, childIndex) == null);
		}
		return lineVisual;
	}

	private Geometry PathGeometryFromDcpRangeFloatersAndFigures(int dcpStart, int dcpEnd, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		Geometry geometry = null;
		if (textDetails.cAttachedObjects > 0)
		{
			PtsHelper.AttachedObjectListFromParagraph(base.PtsContext, _paraHandle.Value, textDetails.cAttachedObjects, out var arrayAttachedObjectDesc);
			for (int i = 0; i < arrayAttachedObjectDesc.Length; i++)
			{
				PTS.FSATTACHEDOBJECTDESCRIPTION fSATTACHEDOBJECTDESCRIPTION = arrayAttachedObjectDesc[i];
				BaseParaClient baseParaClient = base.PtsContext.HandleToObject(fSATTACHEDOBJECTDESCRIPTION.pfsparaclient) as BaseParaClient;
				PTS.ValidateHandle(baseParaClient);
				BaseParagraph paragraph = baseParaClient.Paragraph;
				if (dcpEnd <= paragraph.ParagraphStartCharacterPosition)
				{
					break;
				}
				if (paragraph.ParagraphEndCharacterPosition > dcpStart)
				{
					RectangleGeometry addedGeometry = new RectangleGeometry(baseParaClient.Rect.FromTextDpi());
					CaretElement.AddGeometry(ref geometry, addedGeometry);
				}
			}
		}
		return geometry;
	}

	private bool IsAtCaretUnitBoundaryFromDcpSimpleLines(int dcp, ITextPointer position, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return false;
		}
		PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		bool result = false;
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
			if ((fSLINEDESCRIPTIONSINGLE.dcpFirst > dcp || fSLINEDESCRIPTIONSINGLE.dcpLim <= dcp) && (fSLINEDESCRIPTIONSINGLE.dcpLim != dcp || i != arrayLineDesc.Length - 1))
			{
				continue;
			}
			CharacterHit charHit = default(CharacterHit);
			if (dcp >= fSLINEDESCRIPTIONSINGLE.dcpLim - 1 && i == arrayLineDesc.Length - 1)
			{
				return true;
			}
			if (position.LogicalDirection == LogicalDirection.Backward)
			{
				if (fSLINEDESCRIPTIONSINGLE.dcpFirst == dcp)
				{
					if (i == 0)
					{
						return false;
					}
					i--;
					fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
					Invariant.Assert(dcp > 0);
					charHit = new CharacterHit(dcp - 1, 1);
				}
				else
				{
					Invariant.Assert(dcp > 0);
					charHit = new CharacterHit(dcp - 1, 1);
				}
			}
			else if (position.LogicalDirection == LogicalDirection.Forward)
			{
				charHit = new CharacterHit(dcp, 0);
			}
			Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
			Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnLeft), PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnRight), TextParagraph.TextRunCache);
			if (IsOptimalParagraph)
			{
				formattingContext.LineFormatLengthTarget = fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst;
			}
			TextParagraph.FormatLineCore(line, fSLINEDESCRIPTIONSINGLE.pfsbreakreclineclient, formattingContext, fSLINEDESCRIPTIONSINGLE.dcpFirst, fSLINEDESCRIPTIONSINGLE.dur, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fTreatedAsFirst), fSLINEDESCRIPTIONSINGLE.dcpFirst);
			Invariant.Assert(line.SafeLength == fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst, "Line length is out of sync");
			result = line.IsAtCaretCharacterHit(charHit);
			line.Dispose();
			break;
		}
		return result;
	}

	private bool IsAtCaretUnitBoundaryFromDcpCompositeLines(int dcp, ITextPointer position, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return false;
		}
		PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		bool result = false;
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc[i];
			if (lineDesc.cElements == 0)
			{
				continue;
			}
			PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
			for (int j = 0; j < arrayLineElement.Length; j++)
			{
				PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[j];
				if ((fSLINEELEMENT.dcpFirst > dcp || fSLINEELEMENT.dcpLim <= dcp) && (fSLINEELEMENT.dcpLim != dcp || j != arrayLineElement.Length - 1 || i != arrayLineDesc.Length - 1))
				{
					continue;
				}
				CharacterHit charHit = default(CharacterHit);
				if (dcp >= fSLINEELEMENT.dcpLim - 1 && j == arrayLineElement.Length - 1 && i == arrayLineDesc.Length - 1)
				{
					return true;
				}
				if (position.LogicalDirection == LogicalDirection.Backward)
				{
					if (dcp == fSLINEELEMENT.dcpFirst)
					{
						if (j > 0)
						{
							j--;
							fSLINEELEMENT = arrayLineElement[j];
							charHit = new CharacterHit(dcp - 1, 1);
						}
						else
						{
							if (i == 0)
							{
								return false;
							}
							i--;
							lineDesc = arrayLineDesc[i];
							if (lineDesc.cElements == 0)
							{
								return false;
							}
							PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out arrayLineElement);
							fSLINEELEMENT = arrayLineElement[^1];
							charHit = new CharacterHit(dcp - 1, 1);
						}
					}
					else
					{
						Invariant.Assert(dcp > 0);
						charHit = new CharacterHit(dcp - 1, 1);
					}
				}
				else if (position.LogicalDirection == LogicalDirection.Forward)
				{
					charHit = new CharacterHit(dcp, 0);
				}
				Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
				Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEELEMENT.fClearOnLeft), PTS.ToBoolean(fSLINEELEMENT.fClearOnRight), TextParagraph.TextRunCache);
				if (IsOptimalParagraph)
				{
					formattingContext.LineFormatLengthTarget = fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst;
				}
				TextParagraph.FormatLineCore(line, fSLINEELEMENT.pfsbreakreclineclient, formattingContext, fSLINEELEMENT.dcpFirst, fSLINEELEMENT.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), fSLINEELEMENT.dcpFirst);
				Invariant.Assert(line.SafeLength == fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, "Line length is out of sync");
				result = line.IsAtCaretCharacterHit(charHit);
				line.Dispose();
				return result;
			}
		}
		return result;
	}

	private ITextPointer NextCaretUnitPositionFromDcpSimpleLines(int dcp, ITextPointer position, LogicalDirection direction, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return position;
		}
		PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		ITextPointer result = position;
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
			if ((fSLINEDESCRIPTIONSINGLE.dcpFirst > dcp || fSLINEDESCRIPTIONSINGLE.dcpLim <= dcp) && (fSLINEDESCRIPTIONSINGLE.dcpLim != dcp || i != arrayLineDesc.Length - 1))
			{
				continue;
			}
			if (dcp == fSLINEDESCRIPTIONSINGLE.dcpFirst && direction == LogicalDirection.Backward)
			{
				if (i == 0)
				{
					return position;
				}
				i--;
				fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
			}
			else if (dcp >= fSLINEDESCRIPTIONSINGLE.dcpLim - 1 && direction == LogicalDirection.Forward && i == arrayLineDesc.Length - 1)
			{
				return position;
			}
			Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
			Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnLeft), PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnRight), TextParagraph.TextRunCache);
			if (IsOptimalParagraph)
			{
				formattingContext.LineFormatLengthTarget = fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst;
			}
			TextParagraph.FormatLineCore(line, fSLINEDESCRIPTIONSINGLE.pfsbreakreclineclient, formattingContext, fSLINEDESCRIPTIONSINGLE.dcpFirst, fSLINEDESCRIPTIONSINGLE.dur, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fTreatedAsFirst), fSLINEDESCRIPTIONSINGLE.dcpFirst);
			Invariant.Assert(line.SafeLength == fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst, "Line length is out of sync");
			CharacterHit index = new CharacterHit(dcp, 0);
			CharacterHit characterHit = ((direction != LogicalDirection.Forward) ? line.GetPreviousCaretCharacterHit(index) : line.GetNextCaretCharacterHit(index));
			result = GetTextPosition(direction: (characterHit.FirstCharacterIndex + characterHit.TrailingLength == fSLINEDESCRIPTIONSINGLE.dcpLim && direction == LogicalDirection.Forward) ? ((i != arrayLineDesc.Length - 1) ? LogicalDirection.Forward : LogicalDirection.Backward) : (((characterHit.FirstCharacterIndex + characterHit.TrailingLength != fSLINEDESCRIPTIONSINGLE.dcpFirst || direction != 0) ? (characterHit.TrailingLength <= 0) : (i == 0)) ? LogicalDirection.Forward : LogicalDirection.Backward), dcp: characterHit.FirstCharacterIndex + characterHit.TrailingLength);
			line.Dispose();
			break;
		}
		return result;
	}

	private ITextPointer NextCaretUnitPositionFromDcpCompositeLines(int dcp, ITextPointer position, LogicalDirection direction, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return position;
		}
		PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		ITextPointer result = position;
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc[i];
			if (lineDesc.cElements == 0)
			{
				continue;
			}
			PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
			for (int j = 0; j < arrayLineElement.Length; j++)
			{
				PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[j];
				if ((fSLINEELEMENT.dcpFirst > dcp || fSLINEELEMENT.dcpLim <= dcp) && (fSLINEELEMENT.dcpLim != dcp || j != arrayLineElement.Length - 1 || i != arrayLineDesc.Length - 1))
				{
					continue;
				}
				if (dcp == fSLINEELEMENT.dcpFirst && direction == LogicalDirection.Backward)
				{
					if (dcp == 0)
					{
						return position;
					}
					if (j > 0)
					{
						j--;
						fSLINEELEMENT = arrayLineElement[j];
					}
					else
					{
						i--;
						lineDesc = arrayLineDesc[i];
						if (lineDesc.cElements == 0)
						{
							return position;
						}
						PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out arrayLineElement);
						fSLINEELEMENT = arrayLineElement[^1];
					}
				}
				else if (dcp >= fSLINEELEMENT.dcpLim - 1 && direction == LogicalDirection.Forward)
				{
					if (dcp == fSLINEELEMENT.dcpLim)
					{
						return position;
					}
					if (dcp == fSLINEELEMENT.dcpLim - 1 && j == arrayLineElement.Length - 1 && i == arrayLineDesc.Length - 1)
					{
						return position;
					}
				}
				Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
				Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEELEMENT.fClearOnLeft), PTS.ToBoolean(fSLINEELEMENT.fClearOnRight), TextParagraph.TextRunCache);
				if (IsOptimalParagraph)
				{
					formattingContext.LineFormatLengthTarget = fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst;
				}
				TextParagraph.FormatLineCore(line, fSLINEELEMENT.pfsbreakreclineclient, formattingContext, fSLINEELEMENT.dcpFirst, fSLINEELEMENT.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), fSLINEELEMENT.dcpFirst);
				Invariant.Assert(line.SafeLength == fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, "Line length is out of sync");
				CharacterHit index = new CharacterHit(dcp, 0);
				CharacterHit characterHit = ((direction != LogicalDirection.Forward) ? line.GetPreviousCaretCharacterHit(index) : line.GetNextCaretCharacterHit(index));
				result = GetTextPosition(direction: (characterHit.FirstCharacterIndex + characterHit.TrailingLength == fSLINEELEMENT.dcpLim && direction == LogicalDirection.Forward) ? ((i != arrayLineDesc.Length - 1) ? LogicalDirection.Forward : LogicalDirection.Backward) : (((characterHit.FirstCharacterIndex + characterHit.TrailingLength != fSLINEELEMENT.dcpFirst || direction != 0) ? (characterHit.TrailingLength <= 0) : (i == 0)) ? LogicalDirection.Forward : LogicalDirection.Backward), dcp: characterHit.FirstCharacterIndex + characterHit.TrailingLength);
				line.Dispose();
				return result;
			}
		}
		return result;
	}

	private ITextPointer BackspaceCaretUnitPositionFromDcpSimpleLines(int dcp, ITextPointer position, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return position;
		}
		PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		ITextPointer result = position;
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
			if ((fSLINEDESCRIPTIONSINGLE.dcpFirst > dcp || fSLINEDESCRIPTIONSINGLE.dcpLim <= dcp) && (fSLINEDESCRIPTIONSINGLE.dcpLim != dcp || i != arrayLineDesc.Length - 1))
			{
				continue;
			}
			if (dcp == fSLINEDESCRIPTIONSINGLE.dcpFirst)
			{
				if (i == 0)
				{
					return position;
				}
				i--;
				fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
			}
			Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
			Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnLeft), PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnRight), TextParagraph.TextRunCache);
			if (IsOptimalParagraph)
			{
				formattingContext.LineFormatLengthTarget = fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst;
			}
			TextParagraph.FormatLineCore(line, fSLINEDESCRIPTIONSINGLE.pfsbreakreclineclient, formattingContext, fSLINEDESCRIPTIONSINGLE.dcpFirst, fSLINEDESCRIPTIONSINGLE.dur, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fTreatedAsFirst), fSLINEDESCRIPTIONSINGLE.dcpFirst);
			Invariant.Assert(line.SafeLength == fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst, "Line length is out of sync");
			CharacterHit index = new CharacterHit(dcp, 0);
			CharacterHit backspaceCaretCharacterHit = line.GetBackspaceCaretCharacterHit(index);
			result = GetTextPosition(direction: ((backspaceCaretCharacterHit.FirstCharacterIndex + backspaceCaretCharacterHit.TrailingLength != fSLINEDESCRIPTIONSINGLE.dcpFirst) ? (backspaceCaretCharacterHit.TrailingLength <= 0) : (i == 0)) ? LogicalDirection.Forward : LogicalDirection.Backward, dcp: backspaceCaretCharacterHit.FirstCharacterIndex + backspaceCaretCharacterHit.TrailingLength);
			line.Dispose();
			break;
		}
		return result;
	}

	private ITextPointer BackspaceCaretUnitPositionFromDcpCompositeLines(int dcp, ITextPointer position, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return position;
		}
		PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		ITextPointer result = position;
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc[i];
			if (lineDesc.cElements == 0)
			{
				continue;
			}
			PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
			for (int j = 0; j < arrayLineElement.Length; j++)
			{
				PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[j];
				if ((fSLINEELEMENT.dcpFirst > dcp || fSLINEELEMENT.dcpLim <= dcp) && (fSLINEELEMENT.dcpLim != dcp || j != arrayLineElement.Length - 1 || i != arrayLineDesc.Length - 1))
				{
					continue;
				}
				if (dcp == fSLINEELEMENT.dcpFirst)
				{
					if (dcp == 0)
					{
						return position;
					}
					if (j > 0)
					{
						j--;
						fSLINEELEMENT = arrayLineElement[j];
					}
					else
					{
						i--;
						lineDesc = arrayLineDesc[i];
						if (lineDesc.cElements == 0)
						{
							return position;
						}
						PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out arrayLineElement);
						fSLINEELEMENT = arrayLineElement[^1];
					}
				}
				Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
				Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEELEMENT.fClearOnLeft), PTS.ToBoolean(fSLINEELEMENT.fClearOnRight), TextParagraph.TextRunCache);
				if (IsOptimalParagraph)
				{
					formattingContext.LineFormatLengthTarget = fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst;
				}
				TextParagraph.FormatLineCore(line, fSLINEELEMENT.pfsbreakreclineclient, formattingContext, fSLINEELEMENT.dcpFirst, fSLINEELEMENT.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), fSLINEELEMENT.dcpFirst);
				Invariant.Assert(line.SafeLength == fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, "Line length is out of sync");
				CharacterHit index = new CharacterHit(dcp, 0);
				CharacterHit backspaceCaretCharacterHit = line.GetBackspaceCaretCharacterHit(index);
				result = GetTextPosition(direction: ((backspaceCaretCharacterHit.FirstCharacterIndex + backspaceCaretCharacterHit.TrailingLength != fSLINEELEMENT.dcpFirst) ? (backspaceCaretCharacterHit.TrailingLength <= 0) : (i == 0)) ? LogicalDirection.Forward : LogicalDirection.Backward, dcp: backspaceCaretCharacterHit.FirstCharacterIndex + backspaceCaretCharacterHit.TrailingLength);
				line.Dispose();
				return result;
			}
		}
		return result;
	}

	private void GetGlyphRunsFromSimpleLines(List<GlyphRun> glyphRuns, int dcpStart, int dcpEnd, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return;
		}
		PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
			if (dcpStart < fSLINEDESCRIPTIONSINGLE.dcpLim && dcpEnd > fSLINEDESCRIPTIONSINGLE.dcpFirst)
			{
				Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
				Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnLeft), PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnRight), TextParagraph.TextRunCache);
				if (IsOptimalParagraph)
				{
					formattingContext.LineFormatLengthTarget = fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst;
				}
				TextParagraph.FormatLineCore(line, fSLINEDESCRIPTIONSINGLE.pfsbreakreclineclient, formattingContext, fSLINEDESCRIPTIONSINGLE.dcpFirst, fSLINEDESCRIPTIONSINGLE.dur, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fTreatedAsFirst), fSLINEDESCRIPTIONSINGLE.dcpFirst);
				Invariant.Assert(line.SafeLength == fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst, "Line length is out of sync");
				line.GetGlyphRuns(glyphRuns, Math.Max(dcpStart, fSLINEDESCRIPTIONSINGLE.dcpFirst), Math.Min(dcpEnd, fSLINEDESCRIPTIONSINGLE.dcpLim));
				line.Dispose();
			}
			if (dcpEnd < fSLINEDESCRIPTIONSINGLE.dcpLim)
			{
				break;
			}
		}
	}

	private void GetGlyphRunsFromCompositeLines(List<GlyphRun> glyphRuns, int dcpStart, int dcpEnd, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return;
		}
		PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc[i];
			if (lineDesc.cElements == 0)
			{
				continue;
			}
			PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
			for (int j = 0; j < arrayLineElement.Length; j++)
			{
				PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[j];
				if (dcpStart < fSLINEELEMENT.dcpLim && dcpEnd > fSLINEELEMENT.dcpFirst)
				{
					Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
					Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEELEMENT.fClearOnLeft), PTS.ToBoolean(fSLINEELEMENT.fClearOnRight), TextParagraph.TextRunCache);
					if (IsOptimalParagraph)
					{
						formattingContext.LineFormatLengthTarget = fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst;
					}
					TextParagraph.FormatLineCore(line, fSLINEELEMENT.pfsbreakreclineclient, formattingContext, fSLINEELEMENT.dcpFirst, fSLINEELEMENT.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), fSLINEELEMENT.dcpFirst);
					Invariant.Assert(line.SafeLength == fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, "Line length is out of sync");
					line.GetGlyphRuns(glyphRuns, Math.Max(dcpStart, fSLINEELEMENT.dcpFirst), Math.Min(dcpEnd, fSLINEELEMENT.dcpLim));
					line.Dispose();
				}
				if (dcpEnd < fSLINEELEMENT.dcpLim)
				{
					break;
				}
			}
		}
	}

	private void RenderSimpleLines(ContainerVisual visual, ref PTS.FSTEXTDETAILSFULL textDetails, bool ignoreUpdateInfo)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		int paragraphStartCharacterPosition = base.Paragraph.ParagraphStartCharacterPosition;
		if (textDetails.cLines == 0)
		{
			return;
		}
		VisualCollection children = visual.Children;
		PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		if (!PTS.ToBoolean(textDetails.fUpdateInfoForLinesPresent) || ignoreUpdateInfo)
		{
			children.Clear();
			for (int i = 0; i < arrayLineDesc.Length; i++)
			{
				PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
				Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnLeft), PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnRight), TextParagraph.TextRunCache);
				Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, paragraphStartCharacterPosition);
				if (IsOptimalParagraph)
				{
					formattingContext.LineFormatLengthTarget = fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst;
				}
				TextParagraph.FormatLineCore(line, fSLINEDESCRIPTIONSINGLE.pfsbreakreclineclient, formattingContext, fSLINEDESCRIPTIONSINGLE.dcpFirst, fSLINEDESCRIPTIONSINGLE.dur, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fTreatedAsFirst), fSLINEDESCRIPTIONSINGLE.dcpFirst);
				Invariant.Assert(line.SafeLength == fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst, "Line length is out of sync");
				ContainerVisual containerVisual = line.CreateVisual();
				children.Insert(i, containerVisual);
				containerVisual.Offset = new Vector(TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.urStart), TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE.vrStart));
				line.Dispose();
			}
			return;
		}
		if (textDetails.dvrShiftBeforeChange != 0)
		{
			for (int j = 0; j < textDetails.cLinesBeforeChange; j++)
			{
				ContainerVisual obj = (ContainerVisual)children[j];
				Vector offset = obj.Offset;
				offset.Y += TextDpi.FromTextDpi(textDetails.dvrShiftBeforeChange);
				obj.Offset = offset;
			}
		}
		children.RemoveRange(textDetails.cLinesBeforeChange, textDetails.cLinesChanged - textDetails.dcLinesChanged);
		for (int k = textDetails.cLinesBeforeChange; k < textDetails.cLinesBeforeChange + textDetails.cLinesChanged; k++)
		{
			PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE2 = arrayLineDesc[k];
			Line.FormattingContext formattingContext2 = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE2.fClearOnLeft), PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE2.fClearOnRight), TextParagraph.TextRunCache);
			Line line2 = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, paragraphStartCharacterPosition);
			if (IsOptimalParagraph)
			{
				formattingContext2.LineFormatLengthTarget = fSLINEDESCRIPTIONSINGLE2.dcpLim - fSLINEDESCRIPTIONSINGLE2.dcpFirst;
			}
			TextParagraph.FormatLineCore(line2, fSLINEDESCRIPTIONSINGLE2.pfsbreakreclineclient, formattingContext2, fSLINEDESCRIPTIONSINGLE2.dcpFirst, fSLINEDESCRIPTIONSINGLE2.dur, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE2.fTreatedAsFirst), fSLINEDESCRIPTIONSINGLE2.dcpFirst);
			Invariant.Assert(line2.SafeLength == fSLINEDESCRIPTIONSINGLE2.dcpLim - fSLINEDESCRIPTIONSINGLE2.dcpFirst, "Line length is out of sync");
			ContainerVisual containerVisual2 = line2.CreateVisual();
			children.Insert(k, containerVisual2);
			containerVisual2.Offset = new Vector(TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE2.urStart), TextDpi.FromTextDpi(fSLINEDESCRIPTIONSINGLE2.vrStart));
			line2.Dispose();
		}
		for (int l = textDetails.cLinesBeforeChange + textDetails.cLinesChanged; l < arrayLineDesc.Length; l++)
		{
			ContainerVisual obj2 = (ContainerVisual)children[l];
			Vector offset2 = obj2.Offset;
			offset2.Y += TextDpi.FromTextDpi(textDetails.dvrShiftAfterChange);
			obj2.Offset = offset2;
		}
	}

	private bool IntersectsWithRectOnV(ref PTS.FSRECT rect)
	{
		if (_rect.v <= rect.v + rect.dv)
		{
			return _rect.v + _rect.dv >= rect.v;
		}
		return false;
	}

	private bool ContainedInRectOnV(ref PTS.FSRECT rect)
	{
		if (rect.v <= _rect.v)
		{
			return rect.v + rect.dv >= _rect.v + _rect.dv;
		}
		return false;
	}

	private ContainerVisual CreateLineVisual(ref PTS.FSLINEDESCRIPTIONSINGLE lineDesc, int cpTextParaStart)
	{
		Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(lineDesc.fClearOnLeft), PTS.ToBoolean(lineDesc.fClearOnRight), TextParagraph.TextRunCache);
		Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, cpTextParaStart);
		if (IsOptimalParagraph)
		{
			formattingContext.LineFormatLengthTarget = lineDesc.dcpLim - lineDesc.dcpFirst;
		}
		TextParagraph.FormatLineCore(line, lineDesc.pfsbreakreclineclient, formattingContext, lineDesc.dcpFirst, lineDesc.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), lineDesc.dcpFirst);
		Invariant.Assert(line.SafeLength == lineDesc.dcpLim - lineDesc.dcpFirst, "Line length is out of sync");
		ContainerVisual result = line.CreateVisual();
		line.Dispose();
		return result;
	}

	private void UpdateViewportSimpleLines(ContainerVisual visual, ref PTS.FSTEXTDETAILSFULL textDetails, ref PTS.FSRECT viewport)
	{
		VisualCollection children = visual.Children;
		try
		{
			if (!IntersectsWithRectOnV(ref viewport) || textDetails.cLines == 0)
			{
				children.Clear();
			}
			else
			{
				if (ContainedInRectOnV(ref viewport) && _lineIndexFirstVisual == 0 && children.Count == textDetails.cLines)
				{
					return;
				}
				int num = -1;
				int num2 = -1;
				int paragraphStartCharacterPosition = base.Paragraph.ParagraphStartCharacterPosition;
				PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
				if (ContainedInRectOnV(ref viewport))
				{
					num = 0;
					num2 = textDetails.cLines;
				}
				else
				{
					int i;
					for (i = 0; i < arrayLineDesc.Length; i++)
					{
						PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
						if (fSLINEDESCRIPTIONSINGLE.vrStart + fSLINEDESCRIPTIONSINGLE.dvrAscent + fSLINEDESCRIPTIONSINGLE.dvrDescent > viewport.v)
						{
							break;
						}
					}
					num = i;
					for (i = num; i < arrayLineDesc.Length && arrayLineDesc[i].vrStart <= viewport.v + viewport.dv; i++)
					{
					}
					num2 = i;
				}
				if (_lineIndexFirstVisual != -1 && (num > _lineIndexFirstVisual + children.Count || num2 < _lineIndexFirstVisual))
				{
					children.Clear();
					_lineIndexFirstVisual = -1;
				}
				if (_lineIndexFirstVisual == -1)
				{
					for (int j = num; j < num2; j++)
					{
						PTS.FSLINEDESCRIPTIONSINGLE lineDesc = arrayLineDesc[j];
						ContainerVisual containerVisual = CreateLineVisual(ref lineDesc, paragraphStartCharacterPosition);
						children.Add(containerVisual);
						containerVisual.Offset = new Vector(TextDpi.FromTextDpi(lineDesc.urStart), TextDpi.FromTextDpi(lineDesc.vrStart));
					}
					_lineIndexFirstVisual = num;
				}
				else if (num != _lineIndexFirstVisual || num2 - num != children.Count)
				{
					if (num < _lineIndexFirstVisual)
					{
						for (int k = num; k < _lineIndexFirstVisual; k++)
						{
							PTS.FSLINEDESCRIPTIONSINGLE lineDesc2 = arrayLineDesc[k];
							ContainerVisual containerVisual2 = CreateLineVisual(ref lineDesc2, paragraphStartCharacterPosition);
							children.Insert(k - num, containerVisual2);
							containerVisual2.Offset = new Vector(TextDpi.FromTextDpi(lineDesc2.urStart), TextDpi.FromTextDpi(lineDesc2.vrStart));
						}
					}
					else if (num != _lineIndexFirstVisual)
					{
						children.RemoveRange(0, num - _lineIndexFirstVisual);
					}
					_lineIndexFirstVisual = num;
				}
				if (num2 - num < children.Count)
				{
					int num3 = children.Count - (num2 - num);
					children.RemoveRange(children.Count - num3, num3);
				}
				else if (num2 - num > children.Count)
				{
					for (int l = _lineIndexFirstVisual + children.Count; l < num2; l++)
					{
						PTS.FSLINEDESCRIPTIONSINGLE lineDesc3 = arrayLineDesc[l];
						ContainerVisual containerVisual3 = CreateLineVisual(ref lineDesc3, paragraphStartCharacterPosition);
						children.Add(containerVisual3);
						containerVisual3.Offset = new Vector(TextDpi.FromTextDpi(lineDesc3.urStart), TextDpi.FromTextDpi(lineDesc3.vrStart));
					}
				}
			}
		}
		finally
		{
			if (children.Count == 0)
			{
				_lineIndexFirstVisual = -1;
			}
		}
	}

	private void RenderCompositeLines(ContainerVisual visual, ref PTS.FSTEXTDETAILSFULL textDetails, bool ignoreUpdateInfo)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		VisualCollection children = visual.Children;
		int paragraphStartCharacterPosition = base.Paragraph.ParagraphStartCharacterPosition;
		PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		if (!PTS.ToBoolean(textDetails.fUpdateInfoForLinesPresent) || ignoreUpdateInfo)
		{
			children.Clear();
			for (int i = 0; i < arrayLineDesc.Length; i++)
			{
				PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc[i];
				int num;
				VisualCollection visualCollection;
				if (lineDesc.cElements == 1)
				{
					num = i;
					visualCollection = children;
				}
				else
				{
					num = 0;
					ParagraphElementVisual paragraphElementVisual = new ParagraphElementVisual();
					children.Add(paragraphElementVisual);
					visualCollection = paragraphElementVisual.Children;
				}
				PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
				for (int j = 0; j < arrayLineElement.Length; j++)
				{
					PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[j];
					Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEELEMENT.fClearOnLeft), PTS.ToBoolean(fSLINEELEMENT.fClearOnRight), TextParagraph.TextRunCache);
					Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, paragraphStartCharacterPosition);
					if (IsOptimalParagraph)
					{
						formattingContext.LineFormatLengthTarget = fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst;
					}
					TextParagraph.FormatLineCore(line, fSLINEELEMENT.pfsbreakreclineclient, formattingContext, fSLINEELEMENT.dcpFirst, fSLINEELEMENT.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), fSLINEELEMENT.dcpFirst);
					Invariant.Assert(line.SafeLength == fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, "Line length is out of sync");
					ContainerVisual containerVisual = line.CreateVisual();
					visualCollection.Insert(num + j, containerVisual);
					containerVisual.Offset = new Vector(TextDpi.FromTextDpi(fSLINEELEMENT.urStart), TextDpi.FromTextDpi(lineDesc.vrStart));
					line.Dispose();
				}
			}
			return;
		}
		if (textDetails.dvrShiftBeforeChange != 0)
		{
			for (int k = 0; k < textDetails.cLinesBeforeChange; k++)
			{
				ContainerVisual obj = (ContainerVisual)children[k];
				Vector offset = obj.Offset;
				offset.Y += TextDpi.FromTextDpi(textDetails.dvrShiftBeforeChange);
				obj.Offset = offset;
			}
		}
		children.RemoveRange(textDetails.cLinesBeforeChange, textDetails.cLinesChanged - textDetails.dcLinesChanged);
		for (int l = textDetails.cLinesBeforeChange; l < textDetails.cLinesBeforeChange + textDetails.cLinesChanged; l++)
		{
			PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc2 = arrayLineDesc[l];
			int num2;
			VisualCollection visualCollection2;
			if (lineDesc2.cElements == 1)
			{
				num2 = l;
				visualCollection2 = children;
			}
			else
			{
				num2 = 0;
				ParagraphElementVisual paragraphElementVisual2 = new ParagraphElementVisual();
				children.Add(paragraphElementVisual2);
				visualCollection2 = paragraphElementVisual2.Children;
			}
			PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc2, out var arrayLineElement2);
			for (int m = 0; m < arrayLineElement2.Length; m++)
			{
				PTS.FSLINEELEMENT fSLINEELEMENT2 = arrayLineElement2[m];
				Line.FormattingContext formattingContext2 = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEELEMENT2.fClearOnLeft), PTS.ToBoolean(fSLINEELEMENT2.fClearOnRight), TextParagraph.TextRunCache);
				Line line2 = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, paragraphStartCharacterPosition);
				if (IsOptimalParagraph)
				{
					formattingContext2.LineFormatLengthTarget = fSLINEELEMENT2.dcpLim - fSLINEELEMENT2.dcpFirst;
				}
				TextParagraph.FormatLineCore(line2, fSLINEELEMENT2.pfsbreakreclineclient, formattingContext2, fSLINEELEMENT2.dcpFirst, fSLINEELEMENT2.dur, PTS.ToBoolean(lineDesc2.fTreatedAsFirst), fSLINEELEMENT2.dcpFirst);
				Invariant.Assert(line2.SafeLength == fSLINEELEMENT2.dcpLim - fSLINEELEMENT2.dcpFirst, "Line length is out of sync");
				ContainerVisual containerVisual2 = line2.CreateVisual();
				visualCollection2.Insert(num2 + m, containerVisual2);
				containerVisual2.Offset = new Vector(TextDpi.FromTextDpi(fSLINEELEMENT2.urStart), TextDpi.FromTextDpi(lineDesc2.vrStart));
				line2.Dispose();
			}
		}
		for (int n = textDetails.cLinesBeforeChange + textDetails.cLinesChanged; n < arrayLineDesc.Length; n++)
		{
			ContainerVisual obj2 = (ContainerVisual)children[n];
			Vector offset2 = obj2.Offset;
			offset2.Y += TextDpi.FromTextDpi(textDetails.dvrShiftAfterChange);
			obj2.Offset = offset2;
		}
	}

	private void ValidateVisualFloatersAndFigures(PTS.FSKUPDATE fskupdInherited, int cAttachedObjects)
	{
		if (cAttachedObjects <= 0)
		{
			return;
		}
		PtsHelper.AttachedObjectListFromParagraph(base.PtsContext, _paraHandle.Value, cAttachedObjects, out var arrayAttachedObjectDesc);
		for (int i = 0; i < arrayAttachedObjectDesc.Length; i++)
		{
			PTS.FSATTACHEDOBJECTDESCRIPTION fSATTACHEDOBJECTDESCRIPTION = arrayAttachedObjectDesc[i];
			BaseParaClient baseParaClient = base.PtsContext.HandleToObject(fSATTACHEDOBJECTDESCRIPTION.pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(baseParaClient);
			PTS.FSKUPDATE fSKUPDATE = fSATTACHEDOBJECTDESCRIPTION.fsupdinf.fskupd;
			if (fSKUPDATE == PTS.FSKUPDATE.fskupdInherited)
			{
				fSKUPDATE = fskupdInherited;
			}
			if (fSKUPDATE != PTS.FSKUPDATE.fskupdNoChange)
			{
				baseParaClient.ValidateVisual(fSKUPDATE);
			}
		}
	}

	private IInputElement InputHitTestSimpleLines(PTS.FSPOINT pt, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return null;
		}
		IInputElement result = null;
		int paragraphStartCharacterPosition = base.Paragraph.ParagraphStartCharacterPosition;
		PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONSINGLE fSLINEDESCRIPTIONSINGLE = arrayLineDesc[i];
			if (fSLINEDESCRIPTIONSINGLE.vrStart + fSLINEDESCRIPTIONSINGLE.dvrAscent + fSLINEDESCRIPTIONSINGLE.dvrDescent <= pt.v)
			{
				continue;
			}
			Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnLeft), PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fClearOnRight), TextParagraph.TextRunCache);
			Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, paragraphStartCharacterPosition);
			if (IsOptimalParagraph)
			{
				formattingContext.LineFormatLengthTarget = fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst;
			}
			using (line)
			{
				TextParagraph.FormatLineCore(line, fSLINEDESCRIPTIONSINGLE.pfsbreakreclineclient, formattingContext, fSLINEDESCRIPTIONSINGLE.dcpFirst, fSLINEDESCRIPTIONSINGLE.dur, PTS.ToBoolean(fSLINEDESCRIPTIONSINGLE.fTreatedAsFirst), fSLINEDESCRIPTIONSINGLE.dcpFirst);
				Invariant.Assert(line.SafeLength == fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst, "Line length is out of sync");
				if (fSLINEDESCRIPTIONSINGLE.urStart + line.CalculateUOffsetShift() <= pt.u && pt.u <= fSLINEDESCRIPTIONSINGLE.urStart + line.CalculateUOffsetShift() + fSLINEDESCRIPTIONSINGLE.dur)
				{
					int num = pt.u - fSLINEDESCRIPTIONSINGLE.urStart;
					Invariant.Assert(line.SafeLength == fSLINEDESCRIPTIONSINGLE.dcpLim - fSLINEDESCRIPTIONSINGLE.dcpFirst, "Line length is out of sync");
					if (line.Start <= num && num <= line.Start + line.Width)
					{
						result = line.InputHitTest(num);
					}
				}
			}
			break;
		}
		return result;
	}

	private bool IsDeferredVisualCreationSupported(ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		if (!base.Paragraph.StructuralCache.IsDeferredVisualCreationSupported)
		{
			return false;
		}
		if (PTS.ToBoolean(textDetails.fLinesComposite))
		{
			return false;
		}
		if (TextParagraph.HasFiguresFloatersOrInlineObjects())
		{
			return false;
		}
		return true;
	}

	private List<Rect> GetRectanglesInSimpleLines(ContentElement e, int start, int length, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		List<Rect> list = new List<Rect>();
		int num = start - base.Paragraph.ParagraphStartCharacterPosition;
		if (num < 0 || textDetails.cLines == 0)
		{
			return list;
		}
		PtsHelper.LineListSimpleFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		foreach (PTS.FSLINEDESCRIPTIONSINGLE lineDesc in arrayLineDesc)
		{
			List<Rect> rectanglesInSingleLine = GetRectanglesInSingleLine(lineDesc, e, num, length);
			Invariant.Assert(rectanglesInSingleLine != null);
			if (rectanglesInSingleLine.Count != 0)
			{
				list.AddRange(rectanglesInSingleLine);
			}
		}
		return list;
	}

	private List<Rect> GetRectanglesInSingleLine(PTS.FSLINEDESCRIPTIONSINGLE lineDesc, ContentElement e, int start, int length)
	{
		int num = start + length;
		List<Rect> result = new List<Rect>();
		if (start >= lineDesc.dcpLim)
		{
			return result;
		}
		if (num <= lineDesc.dcpFirst)
		{
			return result;
		}
		int num2 = ((start < lineDesc.dcpFirst) ? lineDesc.dcpFirst : start);
		int num3 = ((num < lineDesc.dcpLim) ? num : lineDesc.dcpLim);
		Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
		Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(lineDesc.fClearOnLeft), PTS.ToBoolean(lineDesc.fClearOnRight), TextParagraph.TextRunCache);
		if (IsOptimalParagraph)
		{
			formattingContext.LineFormatLengthTarget = lineDesc.dcpLim - lineDesc.dcpFirst;
		}
		TextParagraph.FormatLineCore(line, lineDesc.pfsbreakreclineclient, formattingContext, lineDesc.dcpFirst, lineDesc.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), lineDesc.dcpFirst);
		Invariant.Assert(line.SafeLength == lineDesc.dcpLim - lineDesc.dcpFirst, "Line length is out of sync");
		result = line.GetRangeBounds(num2, num3 - num2, TextDpi.FromTextDpi(lineDesc.urStart), TextDpi.FromTextDpi(lineDesc.vrStart));
		Invariant.Assert(result.Count > 0);
		line.Dispose();
		return result;
	}

	private IInputElement InputHitTestCompositeLines(PTS.FSPOINT pt, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		if (textDetails.cLines == 0)
		{
			return null;
		}
		IInputElement result = null;
		int paragraphStartCharacterPosition = base.Paragraph.ParagraphStartCharacterPosition;
		PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc[i];
			if (lineDesc.cElements == 0 || lineDesc.vrStart + lineDesc.dvrAscent + lineDesc.dvrDescent <= pt.v)
			{
				continue;
			}
			PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
			for (int j = 0; j < arrayLineElement.Length; j++)
			{
				PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[j];
				Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEELEMENT.fClearOnLeft), PTS.ToBoolean(fSLINEELEMENT.fClearOnRight), TextParagraph.TextRunCache);
				Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, paragraphStartCharacterPosition);
				if (IsOptimalParagraph)
				{
					formattingContext.LineFormatLengthTarget = fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst;
				}
				using (line)
				{
					TextParagraph.FormatLineCore(line, fSLINEELEMENT.pfsbreakreclineclient, formattingContext, fSLINEELEMENT.dcpFirst, fSLINEELEMENT.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), fSLINEELEMENT.dcpFirst);
					Invariant.Assert(line.SafeLength == fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, "Line length is out of sync");
					if (fSLINEELEMENT.urStart + line.CalculateUOffsetShift() <= pt.u && pt.u <= fSLINEELEMENT.urStart + line.CalculateUOffsetShift() + fSLINEELEMENT.dur)
					{
						int num = pt.u - fSLINEELEMENT.urStart;
						Invariant.Assert(line.SafeLength == fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, "Line length is out of sync");
						Invariant.Assert(line.SafeLength == fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, "Line length is out of sync");
						if (line.Start <= num && num <= line.Start + line.Width)
						{
							result = line.InputHitTest(num);
							break;
						}
					}
				}
			}
			break;
		}
		return result;
	}

	private List<Rect> GetRectanglesInCompositeLines(ContentElement e, int start, int length, ref PTS.FSTEXTDETAILSFULL textDetails)
	{
		ErrorHandler.Assert(!PTS.ToBoolean(textDetails.fDropCapPresent), ErrorHandler.NotSupportedDropCap);
		List<Rect> list = new List<Rect>();
		int num = start - base.Paragraph.ParagraphStartCharacterPosition;
		if (num < 0 || textDetails.cLines == 0)
		{
			return list;
		}
		PtsHelper.LineListCompositeFromTextPara(base.PtsContext, _paraHandle.Value, ref textDetails, out var arrayLineDesc);
		for (int i = 0; i < arrayLineDesc.Length; i++)
		{
			PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc = arrayLineDesc[i];
			if (lineDesc.cElements != 0)
			{
				List<Rect> rectanglesInCompositeLine = GetRectanglesInCompositeLine(lineDesc, e, num, length);
				Invariant.Assert(rectanglesInCompositeLine != null);
				if (rectanglesInCompositeLine.Count != 0)
				{
					list.AddRange(rectanglesInCompositeLine);
				}
			}
		}
		return list;
	}

	private List<Rect> GetRectanglesInCompositeLine(PTS.FSLINEDESCRIPTIONCOMPOSITE lineDesc, ContentElement e, int start, int length)
	{
		List<Rect> list = new List<Rect>();
		int num = start + length;
		PtsHelper.LineElementListFromCompositeLine(base.PtsContext, ref lineDesc, out var arrayLineElement);
		for (int i = 0; i < arrayLineElement.Length; i++)
		{
			PTS.FSLINEELEMENT fSLINEELEMENT = arrayLineElement[i];
			if (start < fSLINEELEMENT.dcpLim && num > fSLINEELEMENT.dcpFirst)
			{
				int num2 = ((start < fSLINEELEMENT.dcpFirst) ? fSLINEELEMENT.dcpFirst : start);
				int num3 = ((num < fSLINEELEMENT.dcpLim) ? num : fSLINEELEMENT.dcpLim);
				Line line = new Line(base.Paragraph.StructuralCache.TextFormatterHost, this, base.Paragraph.ParagraphStartCharacterPosition);
				Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: false, PTS.ToBoolean(fSLINEELEMENT.fClearOnLeft), PTS.ToBoolean(fSLINEELEMENT.fClearOnRight), TextParagraph.TextRunCache);
				if (IsOptimalParagraph)
				{
					formattingContext.LineFormatLengthTarget = fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst;
				}
				TextParagraph.FormatLineCore(line, fSLINEELEMENT.pfsbreakreclineclient, formattingContext, fSLINEELEMENT.dcpFirst, fSLINEELEMENT.dur, PTS.ToBoolean(lineDesc.fTreatedAsFirst), fSLINEELEMENT.dcpFirst);
				Invariant.Assert(line.SafeLength == fSLINEELEMENT.dcpLim - fSLINEELEMENT.dcpFirst, "Line length is out of sync");
				List<Rect> rangeBounds = line.GetRangeBounds(num2, num3 - num2, TextDpi.FromTextDpi(fSLINEELEMENT.urStart), TextDpi.FromTextDpi(lineDesc.vrStart));
				Invariant.Assert(rangeBounds.Count > 0);
				list.AddRange(rangeBounds);
				line.Dispose();
			}
		}
		return list;
	}
}
