using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Documents.TextElement" /> types to UI Automation.</summary>
public class TextElementAutomationPeer : ContentTextAutomationPeer
{
	internal bool? IsTextViewVisible => ((((TextElement)base.Owner)?.TextContainer?.TextView)?.RenderScope)?.IsVisible;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.TextElementAutomationPeer" /> class.</summary>
	/// <param name="owner">The <see cref="T:System.Windows.Documents.TextElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextElementAutomationPeer" />.</param>
	public TextElementAutomationPeer(TextElement owner)
		: base(owner)
	{
	}

	/// <summary>Gets the collection of child elements of the <see cref="T:System.Windows.Documents.TextElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.</summary>
	/// <returns>null.</returns>
	protected override List<AutomationPeer> GetChildrenCore()
	{
		TextElement textElement = (TextElement)base.Owner;
		return TextContainerHelper.GetAutomationPeersFromRange(textElement.ContentStart, textElement.ContentEnd, null);
	}

	/// <summary>Gets the <see cref="T:System.Windows.Rect" /> representing the bounding rectangle of the <see cref="T:System.Windows.Documents.TextElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetBoundingRectangle" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Rect" /> that contains the coordinates of the element, or <see cref="P:System.Windows.Rect.Empty" /> if the element is not a <see cref="T:System.Windows.Interop.HwndSource" /> and a <see cref="T:System.Windows.PresentationSource" />.</returns>
	protected override Rect GetBoundingRectangleCore()
	{
		TextElement textElement = (TextElement)base.Owner;
		ITextView textView = textElement.TextContainer.TextView;
		if (textView == null || !textView.IsValid)
		{
			return Rect.Empty;
		}
		Geometry tightBoundingGeometryFromTextPositions = textView.GetTightBoundingGeometryFromTextPositions(textElement.ContentStart, textElement.ContentEnd);
		if (tightBoundingGeometryFromTextPositions != null)
		{
			PresentationSource presentationSource = PresentationSource.CriticalFromVisual(textView.RenderScope);
			if (presentationSource == null)
			{
				return Rect.Empty;
			}
			if (!(presentationSource is HwndSource hwndSource))
			{
				return Rect.Empty;
			}
			return PointUtil.ClientToScreen(PointUtil.RootToClient(PointUtil.ElementToRoot(tightBoundingGeometryFromTextPositions.Bounds, textView.RenderScope, presentationSource), presentationSource), hwndSource);
		}
		return Rect.Empty;
	}

	/// <summary>Gets a <see cref="T:System.Windows.Point" /> that represents the clickable space that is on the <see cref="T:System.Windows.Documents.TextElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextElementAutomationPeer" />. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClickablePoint" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Point" /> on the element that allows a click. The point values are (<see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" />) if the element is not a <see cref="T:System.Windows.Interop.HwndSource" /> and a <see cref="T:System.Windows.PresentationSource" />.</returns>
	protected override Point GetClickablePointCore()
	{
		Point result = default(Point);
		TextElement textElement = (TextElement)base.Owner;
		ITextView textView = textElement.TextContainer.TextView;
		if (textView == null || !textView.IsValid || (!textView.Contains(textElement.ContentStart) && !textView.Contains(textElement.ContentEnd)))
		{
			return result;
		}
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(textView.RenderScope);
		if (presentationSource == null)
		{
			return result;
		}
		if (!(presentationSource is HwndSource hwndSource))
		{
			return result;
		}
		TextPointer textPointer = textElement.ContentStart.GetNextInsertionPosition(LogicalDirection.Forward);
		if (textPointer == null || textPointer.CompareTo(textElement.ContentEnd) > 0)
		{
			textPointer = textElement.ContentEnd;
		}
		Rect rect = PointUtil.ClientToScreen(PointUtil.RootToClient(PointUtil.ElementToRoot(CalculateVisibleRect(textView, textElement, textElement.ContentStart, textPointer), textView.RenderScope, presentationSource), presentationSource), hwndSource);
		return new Point(rect.Left + rect.Width * 0.5, rect.Top + rect.Height * 0.5);
	}

	/// <summary>Gets a value that indicates whether <see cref="T:System.Windows.Documents.TextElement" /> that is associated with this <see cref="T:System.Windows.Automation.Peers.TextElementAutomationPeer" /> is off of the screen. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.IsOffscreen" />.</summary>
	/// <returns>true if the element is not on the screen; otherwise, false.</returns>
	protected override bool IsOffscreenCore()
	{
		switch (AutomationProperties.GetIsOffscreenBehavior(base.Owner))
		{
		case IsOffscreenBehavior.Onscreen:
			return false;
		case IsOffscreenBehavior.Offscreen:
			return true;
		default:
		{
			TextElement textElement = (TextElement)base.Owner;
			ITextView textView = textElement.TextContainer.TextView;
			if (textView == null || !textView.IsValid || (!textView.Contains(textElement.ContentStart) && !textView.Contains(textElement.ContentEnd)))
			{
				return true;
			}
			if (CalculateVisibleRect(textView, textElement, textElement.ContentStart, textElement.ContentEnd) == Rect.Empty)
			{
				return true;
			}
			return false;
		}
		}
	}

	private Rect CalculateVisibleRect(ITextView textView, TextElement textElement, TextPointer startPointer, TextPointer endPointer)
	{
		Rect rect = textView.GetTightBoundingGeometryFromTextPositions(startPointer, endPointer)?.Bounds ?? Rect.Empty;
		Visual visual = textView.RenderScope;
		while (visual != null && rect != Rect.Empty)
		{
			if (VisualTreeHelper.GetClip(visual) != null)
			{
				GeneralTransform inverse = textView.RenderScope.TransformToAncestor(visual).Inverse;
				if (inverse == null)
				{
					return Rect.Empty;
				}
				Rect bounds = VisualTreeHelper.GetClip(visual).Bounds;
				bounds = inverse.TransformBounds(bounds);
				rect.Intersect(bounds);
			}
			visual = VisualTreeHelper.GetParent(visual) as Visual;
		}
		return rect;
	}

	internal override List<AutomationPeer> GetAutomationPeersFromRange(ITextPointer start, ITextPointer end)
	{
		GetChildren();
		TextElement textElement = (TextElement)base.Owner;
		return TextContainerHelper.GetAutomationPeersFromRange(start, end, textElement.ContentStart);
	}
}
