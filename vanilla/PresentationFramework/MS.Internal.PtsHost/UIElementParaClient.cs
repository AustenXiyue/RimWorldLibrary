using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class UIElementParaClient : FloaterBaseParaClient
{
	internal UIElementParaClient(FloaterBaseParagraph paragraph)
		: base(paragraph)
	{
	}

	protected override void OnArrange()
	{
		base.OnArrange();
		PTS.Validate(PTS.FsQueryFloaterDetails(base.PtsContext.Context, _paraHandle.Value, out var fsfloaterdetails));
		_rect = fsfloaterdetails.fsrcFloater;
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Paragraph.Element, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (base.ParentFlowDirection != base.PageFlowDirection)
		{
			mbpInfo.MirrorMargin();
			PTS.FSRECT rectPage = _pageContext.PageRect;
			PTS.Validate(PTS.FsTransformRectangle(PTS.FlowDirectionToFswdir(base.ParentFlowDirection), ref rectPage, ref _rect, PTS.FlowDirectionToFswdir(base.PageFlowDirection), out _rect));
		}
		_rect.u += mbpInfo.MarginLeft;
		_rect.du -= mbpInfo.MarginLeft + mbpInfo.MarginRight;
		_rect.du = Math.Max(TextDpi.ToTextDpi(TextDpi.MinWidth), _rect.du);
		_rect.dv = Math.Max(TextDpi.ToTextDpi(TextDpi.MinWidth), _rect.dv);
	}

	internal override List<Rect> GetRectangles(ContentElement e, int start, int length)
	{
		List<Rect> rectangles = new List<Rect>();
		if (base.Paragraph.Element == e)
		{
			GetRectanglesForParagraphElement(out rectangles);
		}
		return rectangles;
	}

	internal override void ValidateVisual(PTS.FSKUPDATE fskupdInherited)
	{
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Paragraph.Element, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		PtsHelper.UpdateMirroringTransform(base.PageFlowDirection, base.ThisFlowDirection, _visual, TextDpi.FromTextDpi(2 * _rect.u + _rect.du));
		UIElementIsland uIElementIsland = ((UIElementParagraph)base.Paragraph).UIElementIsland;
		if (uIElementIsland != null)
		{
			if (_visual.Children.Count != 1 || _visual.Children[0] != uIElementIsland)
			{
				if (VisualTreeHelper.GetParent(uIElementIsland) is Visual visual)
				{
					ContainerVisual obj = visual as ContainerVisual;
					Invariant.Assert(obj != null, "Parent should always derives from ContainerVisual.");
					obj.Children.Remove(uIElementIsland);
				}
				_visual.Children.Clear();
				_visual.Children.Add(uIElementIsland);
			}
			uIElementIsland.Offset = new PTS.FSVECTOR(_rect.u + mbpInfo.BPLeft, _rect.v + mbpInfo.BPTop).FromTextDpi();
		}
		else
		{
			_visual.Children.Clear();
		}
		Brush backgroundBrush = (Brush)base.Paragraph.Element.GetValue(TextElement.BackgroundProperty);
		_visual.DrawBackgroundAndBorder(backgroundBrush, mbpInfo.BorderBrush, mbpInfo.Border, _rect.FromTextDpi(), IsFirstChunk, IsLastChunk);
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		if (startPosition.CompareTo(((BlockUIContainer)base.Paragraph.Element).ContentEnd) < 0 && endPosition.CompareTo(((BlockUIContainer)base.Paragraph.Element).ContentStart) > 0)
		{
			return new RectangleGeometry(_rect.FromTextDpi());
		}
		return null;
	}

	internal override ParagraphResult CreateParagraphResult()
	{
		return new UIElementParagraphResult(this);
	}

	internal override IInputElement InputHitTest(PTS.FSPOINT pt)
	{
		if (_rect.Contains(pt))
		{
			return base.Paragraph.Element as IInputElement;
		}
		return null;
	}

	internal override TextContentRange GetTextContentRange()
	{
		return TextContainerHelper.GetTextContentRangeForTextElement((BlockUIContainer)base.Paragraph.Element);
	}
}
