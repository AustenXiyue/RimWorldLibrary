using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal class ContainerParaClient : BaseParaClient
{
	private bool _isFirstChunk;

	private bool _isLastChunk;

	internal override bool IsFirstChunk => _isFirstChunk;

	internal override bool IsLastChunk => _isLastChunk;

	internal ContainerParaClient(ContainerParagraph paragraph)
		: base(paragraph)
	{
	}

	protected override void OnArrange()
	{
		base.OnArrange();
		PTS.Validate(PTS.FsQuerySubtrackDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubTrackDetails));
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Paragraph.Element, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (base.ParentFlowDirection != base.PageFlowDirection)
		{
			mbpInfo.MirrorMargin();
		}
		_rect.u += mbpInfo.MarginLeft;
		_rect.du -= mbpInfo.MarginLeft + mbpInfo.MarginRight;
		_rect.du = Math.Max(TextDpi.ToTextDpi(TextDpi.MinWidth), _rect.du);
		_rect.dv = Math.Max(TextDpi.ToTextDpi(TextDpi.MinWidth), _rect.dv);
		uint fswdirTrack = PTS.FlowDirectionToFswdir(_flowDirection);
		if (pSubTrackDetails.cParas != 0)
		{
			PtsHelper.ParaListFromSubtrack(base.PtsContext, _paraHandle.Value, ref pSubTrackDetails, out var arrayParaDesc);
			PtsHelper.ArrangeParaList(base.PtsContext, pSubTrackDetails.fsrc, arrayParaDesc, fswdirTrack);
		}
	}

	internal override IInputElement InputHitTest(PTS.FSPOINT pt)
	{
		IInputElement inputElement = null;
		PTS.Validate(PTS.FsQuerySubtrackDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubTrackDetails));
		if (pSubTrackDetails.cParas != 0)
		{
			PtsHelper.ParaListFromSubtrack(base.PtsContext, _paraHandle.Value, ref pSubTrackDetails, out var arrayParaDesc);
			inputElement = PtsHelper.InputHitTestParaList(base.PtsContext, pt, ref pSubTrackDetails.fsrc, arrayParaDesc);
		}
		if (inputElement == null && _rect.Contains(pt))
		{
			inputElement = base.Paragraph.Element as IInputElement;
		}
		return inputElement;
	}

	internal override List<Rect> GetRectangles(ContentElement e, int start, int length)
	{
		List<Rect> rectangles = new List<Rect>();
		if (base.Paragraph.Element as ContentElement == e)
		{
			GetRectanglesForParagraphElement(out rectangles);
		}
		else
		{
			PTS.Validate(PTS.FsQuerySubtrackDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubTrackDetails));
			if (pSubTrackDetails.cParas != 0)
			{
				PtsHelper.ParaListFromSubtrack(base.PtsContext, _paraHandle.Value, ref pSubTrackDetails, out var arrayParaDesc);
				rectangles = PtsHelper.GetRectanglesInParaList(base.PtsContext, e, start, length, arrayParaDesc);
			}
			else
			{
				rectangles = new List<Rect>();
			}
		}
		Invariant.Assert(rectangles != null);
		return rectangles;
	}

	internal override void ValidateVisual(PTS.FSKUPDATE fskupdInherited)
	{
		PTS.Validate(PTS.FsQuerySubtrackDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubTrackDetails));
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Paragraph.Element, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			mbpInfo.MirrorBP();
		}
		Brush backgroundBrush = (Brush)base.Paragraph.Element.GetValue(TextElement.BackgroundProperty);
		_visual.DrawBackgroundAndBorder(backgroundBrush, mbpInfo.BorderBrush, mbpInfo.Border, _rect.FromTextDpi(), IsFirstChunk, IsLastChunk);
		if (pSubTrackDetails.cParas != 0)
		{
			PtsHelper.ParaListFromSubtrack(base.PtsContext, _paraHandle.Value, ref pSubTrackDetails, out var arrayParaDesc);
			PtsHelper.UpdateParaListVisuals(base.PtsContext, _visual.Children, fskupdInherited, arrayParaDesc);
		}
		else
		{
			_visual.Children.Clear();
		}
	}

	internal override void UpdateViewport(ref PTS.FSRECT viewport)
	{
		PTS.Validate(PTS.FsQuerySubtrackDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubTrackDetails));
		if (pSubTrackDetails.cParas != 0)
		{
			PtsHelper.ParaListFromSubtrack(base.PtsContext, _paraHandle.Value, ref pSubTrackDetails, out var arrayParaDesc);
			PtsHelper.UpdateViewportParaList(base.PtsContext, arrayParaDesc, ref viewport);
		}
	}

	internal override ParagraphResult CreateParagraphResult()
	{
		return new ContainerParagraphResult(this);
	}

	internal override TextContentRange GetTextContentRange()
	{
		TextElement textElement = base.Paragraph.Element as TextElement;
		Invariant.Assert(textElement != null, "Expecting TextElement as owner of ContainerParagraph.");
		PTS.Validate(PTS.FsQuerySubtrackDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubTrackDetails));
		TextContentRange textContentRange;
		if (pSubTrackDetails.cParas == 0 || (_isFirstChunk && _isLastChunk))
		{
			textContentRange = TextContainerHelper.GetTextContentRangeForTextElement(textElement);
		}
		else
		{
			PtsHelper.ParaListFromSubtrack(base.PtsContext, _paraHandle.Value, ref pSubTrackDetails, out var arrayParaDesc);
			textContentRange = new TextContentRange();
			for (int i = 0; i < arrayParaDesc.Length; i++)
			{
				BaseParaClient baseParaClient = base.Paragraph.StructuralCache.PtsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
				PTS.ValidateHandle(baseParaClient);
				textContentRange.Merge(baseParaClient.GetTextContentRange());
			}
			if (_isFirstChunk)
			{
				textContentRange.Merge(TextContainerHelper.GetTextContentRangeForTextElementEdge(textElement, ElementEdge.BeforeStart));
			}
			if (_isLastChunk)
			{
				textContentRange.Merge(TextContainerHelper.GetTextContentRangeForTextElementEdge(textElement, ElementEdge.AfterEnd));
			}
		}
		Invariant.Assert(textContentRange != null);
		return textContentRange;
	}

	internal ReadOnlyCollection<ParagraphResult> GetChildrenParagraphResults(out bool hasTextContent)
	{
		PTS.Validate(PTS.FsQuerySubtrackDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubTrackDetails));
		hasTextContent = false;
		if (pSubTrackDetails.cParas == 0)
		{
			return new ReadOnlyCollection<ParagraphResult>(new List<ParagraphResult>(0));
		}
		PtsHelper.ParaListFromSubtrack(base.PtsContext, _paraHandle.Value, ref pSubTrackDetails, out var arrayParaDesc);
		List<ParagraphResult> list = new List<ParagraphResult>(arrayParaDesc.Length);
		for (int i = 0; i < arrayParaDesc.Length; i++)
		{
			BaseParaClient obj = base.PtsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(obj);
			ParagraphResult paragraphResult = obj.CreateParagraphResult();
			if (paragraphResult.HasTextContent)
			{
				hasTextContent = true;
			}
			list.Add(paragraphResult);
		}
		return new ReadOnlyCollection<ParagraphResult>(list);
	}

	internal void SetChunkInfo(bool isFirstChunk, bool isLastChunk)
	{
		_isFirstChunk = isFirstChunk;
		_isLastChunk = isLastChunk;
	}

	internal override int GetFirstTextLineBaseline()
	{
		PTS.Validate(PTS.FsQuerySubtrackDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubTrackDetails));
		if (pSubTrackDetails.cParas == 0)
		{
			return _rect.v;
		}
		PtsHelper.ParaListFromSubtrack(base.PtsContext, _paraHandle.Value, ref pSubTrackDetails, out var arrayParaDesc);
		BaseParaClient obj = base.PtsContext.HandleToObject(arrayParaDesc[0].pfsparaclient) as BaseParaClient;
		PTS.ValidateHandle(obj);
		return obj.GetFirstTextLineBaseline();
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition, Rect visibleRect)
	{
		bool hasTextContent;
		ReadOnlyCollection<ParagraphResult> childrenParagraphResults = GetChildrenParagraphResults(out hasTextContent);
		Invariant.Assert(childrenParagraphResults != null, "Paragraph collection is null.");
		if (childrenParagraphResults.Count > 0)
		{
			return TextDocumentView.GetTightBoundingGeometryFromTextPositionsHelper(childrenParagraphResults, startPosition, endPosition, TextDpi.FromTextDpi(_dvrTopSpace), visibleRect);
		}
		return null;
	}
}
