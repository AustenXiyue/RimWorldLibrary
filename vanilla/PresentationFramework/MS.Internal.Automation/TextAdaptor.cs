using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Documents;

namespace MS.Internal.Automation;

internal class TextAdaptor : ITextProvider, IDisposable
{
	private AutomationPeer _textPeer;

	private ITextContainer _textContainer;

	ITextRangeProvider ITextProvider.DocumentRange => new TextRangeAdaptor(this, _textContainer.Start, _textContainer.End, _textPeer);

	SupportedTextSelection ITextProvider.SupportedTextSelection
	{
		get
		{
			if (_textContainer.TextSelection != null)
			{
				return SupportedTextSelection.Single;
			}
			return SupportedTextSelection.None;
		}
	}

	internal TextAdaptor(AutomationPeer textPeer, ITextContainer textContainer)
	{
		Invariant.Assert(textContainer != null, "Invalid ITextContainer");
		Invariant.Assert(textPeer is TextAutomationPeer || textPeer is ContentTextAutomationPeer, "Invalid AutomationPeer");
		_textPeer = textPeer;
		_textContainer = textContainer;
		_textContainer.Changed += OnTextContainerChanged;
		if (_textContainer.TextSelection != null)
		{
			_textContainer.TextSelection.Changed += OnTextSelectionChanged;
		}
	}

	public void Dispose()
	{
		if (_textContainer != null && _textContainer.TextSelection != null)
		{
			_textContainer.TextSelection.Changed -= OnTextSelectionChanged;
		}
		GC.SuppressFinalize(this);
	}

	internal Rect[] GetBoundingRectangles(ITextPointer start, ITextPointer end, bool clipToView, bool transformToScreen)
	{
		ITextView updatedTextView = GetUpdatedTextView();
		if (updatedTextView == null)
		{
			return Array.Empty<Rect>();
		}
		ReadOnlyCollection<TextSegment> textSegments = updatedTextView.TextSegments;
		if (textSegments.Count > 0)
		{
			if (!updatedTextView.Contains(start) && start.CompareTo(textSegments[0].Start) < 0)
			{
				start = textSegments[0].Start.CreatePointer();
			}
			if (!updatedTextView.Contains(end) && end.CompareTo(textSegments[textSegments.Count - 1].End) > 0)
			{
				end = textSegments[textSegments.Count - 1].End.CreatePointer();
			}
		}
		if (!updatedTextView.Contains(start) || !updatedTextView.Contains(end))
		{
			return Array.Empty<Rect>();
		}
		TextRangeAdaptor.MoveToInsertionPosition(start, LogicalDirection.Forward);
		TextRangeAdaptor.MoveToInsertionPosition(end, LogicalDirection.Backward);
		Rect rect = Rect.Empty;
		if (clipToView)
		{
			rect = GetVisibleRectangle(updatedTextView);
			if (rect.IsEmpty)
			{
				return Array.Empty<Rect>();
			}
		}
		List<Rect> list = new List<Rect>();
		ITextPointer textPointer = start.CreatePointer();
		while (textPointer.CompareTo(end) < 0)
		{
			TextSegment lineRange = updatedTextView.GetLineRange(textPointer);
			if (!lineRange.IsNull)
			{
				ITextPointer startPosition = ((lineRange.Start.CompareTo(start) <= 0) ? start : lineRange.Start);
				ITextPointer endPosition = ((lineRange.End.CompareTo(end) >= 0) ? end : lineRange.End);
				Rect empty = Rect.Empty;
				Geometry tightBoundingGeometryFromTextPositions = updatedTextView.GetTightBoundingGeometryFromTextPositions(startPosition, endPosition);
				if (tightBoundingGeometryFromTextPositions != null)
				{
					empty = tightBoundingGeometryFromTextPositions.Bounds;
					if (clipToView)
					{
						empty.Intersect(rect);
					}
					if (!empty.IsEmpty)
					{
						if (transformToScreen)
						{
							empty = new Rect(ClientToScreen(empty.TopLeft, updatedTextView.RenderScope), ClientToScreen(empty.BottomRight, updatedTextView.RenderScope));
						}
						list.Add(empty);
					}
				}
			}
			if (textPointer.MoveToLineBoundary(1) == 0)
			{
				textPointer = end;
			}
		}
		return list.ToArray();
	}

	internal ITextView GetUpdatedTextView()
	{
		ITextView textView = _textContainer.TextView;
		if (textView != null && !textView.IsValid)
		{
			if (!textView.Validate())
			{
				textView = null;
			}
			if (textView != null && !textView.IsValid)
			{
				textView = null;
			}
		}
		return textView;
	}

	internal void Select(ITextPointer start, ITextPointer end)
	{
		if (_textContainer.TextSelection != null)
		{
			_textContainer.TextSelection.Select(start, end);
		}
	}

	internal void ScrollIntoView(ITextPointer start, ITextPointer end, bool alignToTop)
	{
		Rect rect = Rect.Empty;
		Rect[] boundingRectangles = GetBoundingRectangles(start, end, clipToView: false, transformToScreen: false);
		foreach (Rect rect2 in boundingRectangles)
		{
			rect.Union(rect2);
		}
		ITextView updatedTextView = GetUpdatedTextView();
		if (updatedTextView != null && !rect.IsEmpty)
		{
			Rect visibleRectangle = GetVisibleRectangle(updatedTextView);
			Rect rect3 = Rect.Intersect(rect, visibleRectangle);
			if (rect3 == rect)
			{
				return;
			}
			UIElement renderScope = updatedTextView.RenderScope;
			for (Visual visual = renderScope; visual != null; visual = VisualTreeHelper.GetParent(visual) as Visual)
			{
				if (visual is IScrollInfo scrollInfo)
				{
					if (visual != renderScope)
					{
						rect = renderScope.TransformToAncestor(visual).TransformBounds(rect);
					}
					if (scrollInfo.CanHorizontallyScroll)
					{
						scrollInfo.SetHorizontalOffset(alignToTop ? rect.Left : (rect.Right - scrollInfo.ViewportWidth));
					}
					if (scrollInfo.CanVerticallyScroll)
					{
						scrollInfo.SetVerticalOffset(alignToTop ? rect.Top : (rect.Bottom - scrollInfo.ViewportHeight));
					}
					break;
				}
			}
			if (renderScope is FrameworkElement frameworkElement)
			{
				frameworkElement.BringIntoView(rect3);
			}
		}
		else
		{
			ITextPointer obj = (alignToTop ? start.CreatePointer() : end.CreatePointer());
			obj.MoveToElementEdge(alignToTop ? ElementEdge.AfterStart : ElementEdge.AfterEnd);
			if (obj.GetAdjacentElement(LogicalDirection.Backward) is FrameworkContentElement frameworkContentElement)
			{
				frameworkContentElement.BringIntoView();
			}
		}
	}

	internal ITextRangeProvider TextRangeFromTextPointers(ITextPointer rangeStart, ITextPointer rangeEnd)
	{
		if (rangeStart == null && rangeEnd == null)
		{
			return null;
		}
		rangeStart = rangeStart ?? _textContainer.Start;
		rangeEnd = rangeEnd ?? _textContainer.End;
		if (rangeStart.TextContainer != _textContainer || rangeEnd.TextContainer != _textContainer)
		{
			return null;
		}
		if (rangeStart.CompareTo(rangeEnd) > 0)
		{
			rangeStart = rangeEnd;
			rangeEnd = rangeStart;
		}
		return TextRangeProviderWrapper.WrapArgument(new TextRangeAdaptor(this, rangeStart, rangeEnd, _textPeer), _textPeer);
	}

	private void OnTextContainerChanged(object sender, TextContainerChangedEventArgs e)
	{
		_textPeer.RaiseAutomationEvent(AutomationEvents.TextPatternOnTextChanged);
	}

	private void OnTextSelectionChanged(object sender, EventArgs e)
	{
		_textPeer.RaiseAutomationEvent(AutomationEvents.TextPatternOnTextSelectionChanged);
	}

	private Rect GetVisibleRectangle(ITextView textView)
	{
		Rect rect = new Rect(textView.RenderScope.RenderSize);
		Visual visual = VisualTreeHelper.GetParent(textView.RenderScope) as Visual;
		while (visual != null && rect != Rect.Empty)
		{
			if (VisualTreeHelper.GetClip(visual) != null)
			{
				GeneralTransform inverse = textView.RenderScope.TransformToAncestor(visual).Inverse;
				if (inverse != null)
				{
					Rect bounds = VisualTreeHelper.GetClip(visual).Bounds;
					bounds = inverse.TransformBounds(bounds);
					rect.Intersect(bounds);
				}
				else
				{
					rect = Rect.Empty;
				}
			}
			visual = VisualTreeHelper.GetParent(visual) as Visual;
		}
		return rect;
	}

	private Point ClientToScreen(Point point, Visual visual)
	{
		if (AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures)
		{
			return ObsoleteClientToScreen(point, visual);
		}
		try
		{
			point = visual.PointToScreen(point);
		}
		catch (InvalidOperationException)
		{
		}
		return point;
	}

	private Point ObsoleteClientToScreen(Point point, Visual visual)
	{
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(visual);
		if (presentationSource != null)
		{
			GeneralTransform generalTransform = visual.TransformToAncestor(presentationSource.RootVisual);
			if (generalTransform != null)
			{
				point = generalTransform.Transform(point);
			}
		}
		return PointUtil.ClientToScreen(point, presentationSource);
	}

	private Point ScreenToClient(Point point, Visual visual)
	{
		if (AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures)
		{
			return ObsoleteScreenToClient(point, visual);
		}
		try
		{
			point = visual.PointFromScreen(point);
		}
		catch (InvalidOperationException)
		{
		}
		return point;
	}

	private Point ObsoleteScreenToClient(Point point, Visual visual)
	{
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(visual);
		point = PointUtil.ScreenToClient(point, presentationSource);
		if (presentationSource != null)
		{
			GeneralTransform generalTransform = visual.TransformToAncestor(presentationSource.RootVisual);
			if (generalTransform != null)
			{
				generalTransform = generalTransform.Inverse;
				if (generalTransform != null)
				{
					point = generalTransform.Transform(point);
				}
			}
		}
		return point;
	}

	ITextRangeProvider[] ITextProvider.GetSelection()
	{
		ITextRange textSelection = _textContainer.TextSelection;
		if (textSelection == null)
		{
			throw new InvalidOperationException(SR.TextProvider_TextSelectionNotSupported);
		}
		return new ITextRangeProvider[1]
		{
			new TextRangeAdaptor(this, textSelection.Start, textSelection.End, _textPeer)
		};
	}

	ITextRangeProvider[] ITextProvider.GetVisibleRanges()
	{
		ITextRangeProvider[] array = null;
		ITextView updatedTextView = GetUpdatedTextView();
		if (updatedTextView != null)
		{
			List<TextSegment> list = new List<TextSegment>();
			if (updatedTextView is MultiPageTextView)
			{
				list.AddRange(updatedTextView.TextSegments);
			}
			else
			{
				Rect visibleRectangle = GetVisibleRectangle(updatedTextView);
				if (!visibleRectangle.IsEmpty)
				{
					ITextPointer textPositionFromPoint = updatedTextView.GetTextPositionFromPoint(visibleRectangle.TopLeft, snapToText: true);
					ITextPointer textPositionFromPoint2 = updatedTextView.GetTextPositionFromPoint(visibleRectangle.BottomRight, snapToText: true);
					list.Add(new TextSegment(textPositionFromPoint, textPositionFromPoint2, preserveLogicalDirection: true));
				}
			}
			if (list.Count > 0)
			{
				array = new ITextRangeProvider[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					array[i] = new TextRangeAdaptor(this, list[i].Start, list[i].End, _textPeer);
				}
			}
		}
		if (array == null)
		{
			array = new ITextRangeProvider[1]
			{
				new TextRangeAdaptor(this, _textContainer.Start, _textContainer.Start, _textPeer)
			};
		}
		return array;
	}

	ITextRangeProvider ITextProvider.RangeFromChild(IRawElementProviderSimple childElementProvider)
	{
		if (childElementProvider == null)
		{
			throw new ArgumentNullException("childElementProvider");
		}
		DependencyObject dependencyObject = ((!(_textPeer is TextAutomationPeer)) ? ((ContentTextAutomationPeer)_textPeer).ElementFromProvider(childElementProvider) : ((TextAutomationPeer)_textPeer).ElementFromProvider(childElementProvider));
		TextRangeAdaptor textRangeAdaptor = null;
		if (dependencyObject != null)
		{
			ITextPointer textPointer = null;
			ITextPointer textPointer2 = null;
			if (dependencyObject is TextElement)
			{
				textPointer = ((TextElement)dependencyObject).ElementStart;
				textPointer2 = ((TextElement)dependencyObject).ElementEnd;
			}
			else
			{
				DependencyObject parent = LogicalTreeHelper.GetParent(dependencyObject);
				if (parent is InlineUIContainer || parent is BlockUIContainer)
				{
					textPointer = ((TextElement)parent).ContentStart;
					textPointer2 = ((TextElement)parent).ContentEnd;
				}
				else
				{
					for (ITextPointer textPointer3 = _textContainer.Start.CreatePointer(); textPointer3.CompareTo(_textContainer.End) < 0; textPointer3.MoveToNextContextPosition(LogicalDirection.Forward))
					{
						switch (textPointer3.GetPointerContext(LogicalDirection.Forward))
						{
						case TextPointerContext.ElementStart:
							if (dependencyObject != textPointer3.GetAdjacentElement(LogicalDirection.Forward))
							{
								continue;
							}
							textPointer = textPointer3.CreatePointer(LogicalDirection.Forward);
							textPointer3.MoveToElementEdge(ElementEdge.AfterEnd);
							textPointer2 = textPointer3.CreatePointer(LogicalDirection.Backward);
							break;
						case TextPointerContext.EmbeddedElement:
							if (dependencyObject != textPointer3.GetAdjacentElement(LogicalDirection.Forward))
							{
								continue;
							}
							textPointer = textPointer3.CreatePointer(LogicalDirection.Forward);
							textPointer3.MoveToNextContextPosition(LogicalDirection.Forward);
							textPointer2 = textPointer3.CreatePointer(LogicalDirection.Backward);
							break;
						default:
							continue;
						}
						break;
					}
				}
			}
			if (textPointer != null && textPointer2 != null)
			{
				textRangeAdaptor = new TextRangeAdaptor(this, textPointer, textPointer2, _textPeer);
			}
		}
		if (textRangeAdaptor == null)
		{
			throw new InvalidOperationException(SR.TextProvider_InvalidChildElement);
		}
		return textRangeAdaptor;
	}

	ITextRangeProvider ITextProvider.RangeFromPoint(Point location)
	{
		TextRangeAdaptor textRangeAdaptor = null;
		ITextView updatedTextView = GetUpdatedTextView();
		if (updatedTextView != null)
		{
			location = ScreenToClient(location, updatedTextView.RenderScope);
			ITextPointer textPositionFromPoint = updatedTextView.GetTextPositionFromPoint(location, snapToText: true);
			if (textPositionFromPoint != null)
			{
				textRangeAdaptor = new TextRangeAdaptor(this, textPositionFromPoint, textPositionFromPoint, _textPeer);
			}
		}
		if (textRangeAdaptor == null)
		{
			throw new ArgumentException(SR.TextProvider_InvalidPoint);
		}
		return textRangeAdaptor;
	}
}
