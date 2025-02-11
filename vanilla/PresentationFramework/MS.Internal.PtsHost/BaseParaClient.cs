using System;
using System.Collections.Generic;
using System.Windows;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal abstract class BaseParaClient : UnmanagedHandle
{
	protected readonly BaseParagraph _paragraph;

	protected MS.Internal.SecurityCriticalDataForSet<nint> _paraHandle;

	protected PTS.FSRECT _rect;

	protected int _dvrTopSpace;

	protected ParagraphVisual _visual;

	protected PageContext _pageContext;

	protected FlowDirection _flowDirectionParent;

	protected FlowDirection _flowDirection;

	internal virtual ParagraphVisual Visual
	{
		get
		{
			if (_visual == null)
			{
				_visual = new ParagraphVisual();
			}
			return _visual;
		}
	}

	internal virtual bool IsFirstChunk => true;

	internal virtual bool IsLastChunk => true;

	internal BaseParagraph Paragraph => _paragraph;

	internal PTS.FSRECT Rect => _rect;

	internal FlowDirection ThisFlowDirection => _flowDirection;

	internal FlowDirection ParentFlowDirection => _flowDirectionParent;

	internal FlowDirection PageFlowDirection => Paragraph.StructuralCache.PageFlowDirection;

	protected BaseParaClient(BaseParagraph paragraph)
		: base(paragraph.PtsContext)
	{
		_paraHandle = new MS.Internal.SecurityCriticalDataForSet<nint>(IntPtr.Zero);
		_paragraph = paragraph;
	}

	internal void Arrange(nint pfspara, PTS.FSRECT rcPara, int dvrTopSpace, uint fswdirParent)
	{
		_paraHandle.Value = pfspara;
		_rect = rcPara;
		_dvrTopSpace = dvrTopSpace;
		_pageContext = Paragraph.StructuralCache.CurrentArrangeContext.PageContext;
		_flowDirectionParent = PTS.FswdirToFlowDirection(fswdirParent);
		_flowDirection = (FlowDirection)Paragraph.Element.GetValue(FrameworkElement.FlowDirectionProperty);
		OnArrange();
	}

	internal virtual int GetFirstTextLineBaseline()
	{
		return _rect.v + _rect.dv;
	}

	internal void TransferDisplayInfo(BaseParaClient oldParaClient)
	{
		_visual = oldParaClient._visual;
		oldParaClient._visual = null;
	}

	internal virtual IInputElement InputHitTest(PTS.FSPOINT pt)
	{
		return null;
	}

	internal virtual List<Rect> GetRectangles(ContentElement e, int start, int length)
	{
		return new List<Rect>();
	}

	internal virtual void GetRectanglesForParagraphElement(out List<Rect> rectangles)
	{
		rectangles = new List<Rect>();
		Rect item = TextDpi.FromTextRect(_rect);
		rectangles.Add(item);
	}

	internal virtual void ValidateVisual(PTS.FSKUPDATE fskupdInherited)
	{
	}

	internal virtual void UpdateViewport(ref PTS.FSRECT viewport)
	{
	}

	internal abstract ParagraphResult CreateParagraphResult();

	internal abstract TextContentRange GetTextContentRange();

	protected virtual void OnArrange()
	{
		Paragraph.UpdateLastFormatPositions();
	}
}
