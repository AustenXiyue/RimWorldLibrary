using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost;

namespace MS.Internal.Documents;

internal class DocumentPageTextView : TextViewBase
{
	private readonly UIElement _owner;

	private readonly ITextContainer _textContainer;

	private DocumentPage _page;

	private ITextView _pageTextView;

	internal override UIElement RenderScope => _owner;

	internal override ITextContainer TextContainer => _textContainer;

	internal override bool IsValid
	{
		get
		{
			if (!_owner.IsMeasureValid || !_owner.IsArrangeValid || _page == null)
			{
				return false;
			}
			if (IsPageMissing)
			{
				return true;
			}
			if (_pageTextView != null)
			{
				return _pageTextView.IsValid;
			}
			return false;
		}
	}

	internal override bool RendersOwnSelection
	{
		get
		{
			if (_pageTextView != null)
			{
				return _pageTextView.RendersOwnSelection;
			}
			return false;
		}
	}

	internal override ReadOnlyCollection<TextSegment> TextSegments
	{
		get
		{
			if (!IsValid || IsPageMissing)
			{
				return new ReadOnlyCollection<TextSegment>(new List<TextSegment>());
			}
			return _pageTextView.TextSegments;
		}
	}

	internal DocumentPageView DocumentPageView => _owner as DocumentPageView;

	private bool IsPageMissing => _page == DocumentPage.Missing;

	internal DocumentPageTextView(DocumentPageView owner, ITextContainer textContainer)
	{
		Invariant.Assert(owner != null && textContainer != null);
		_owner = owner;
		_page = owner.DocumentPageInternal;
		_textContainer = textContainer;
		if (_page is IServiceProvider)
		{
			_pageTextView = ((IServiceProvider)_page).GetService(typeof(ITextView)) as ITextView;
		}
		if (_pageTextView != null)
		{
			_pageTextView.Updated += HandlePageTextViewUpdated;
		}
	}

	internal DocumentPageTextView(FlowDocumentView owner, ITextContainer textContainer)
	{
		Invariant.Assert(owner != null && textContainer != null);
		_owner = owner;
		_page = owner.DocumentPage;
		_textContainer = textContainer;
		if (_page is IServiceProvider)
		{
			_pageTextView = ((IServiceProvider)_page).GetService(typeof(ITextView)) as ITextView;
		}
		if (_pageTextView != null)
		{
			_pageTextView.Updated += HandlePageTextViewUpdated;
		}
	}

	internal override ITextPointer GetTextPositionFromPoint(Point point, bool snapToText)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (IsPageMissing)
		{
			return null;
		}
		point = TransformToDescendant(point);
		return _pageTextView.GetTextPositionFromPoint(point, snapToText);
	}

	internal override Rect GetRawRectangleFromTextPosition(ITextPointer position, out Transform transform)
	{
		transform = Transform.Identity;
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (IsPageMissing)
		{
			return Rect.Empty;
		}
		Transform transform2;
		Rect rawRectangleFromTextPosition = _pageTextView.GetRawRectangleFromTextPosition(position, out transform2);
		Invariant.Assert(transform2 != null);
		transform = GetAggregateTransform(secondTransform: GetTransformToAncestor(), firstTransform: transform2);
		return rawRectangleFromTextPosition;
	}

	internal override Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		Geometry geometry = null;
		if (!IsPageMissing)
		{
			geometry = _pageTextView.GetTightBoundingGeometryFromTextPositions(startPosition, endPosition);
			if (geometry != null)
			{
				Transform affineTransform = GetTransformToAncestor().AffineTransform;
				CaretElement.AddTransformToGeometry(geometry, affineTransform);
			}
		}
		return geometry;
	}

	internal override ITextPointer GetPositionAtNextLine(ITextPointer position, double suggestedX, int count, out double newSuggestedX, out int linesMoved)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (IsPageMissing)
		{
			newSuggestedX = suggestedX;
			linesMoved = 0;
			return position;
		}
		suggestedX = TransformToDescendant(new Point(suggestedX, 0.0)).X;
		ITextPointer positionAtNextLine = _pageTextView.GetPositionAtNextLine(position, suggestedX, count, out newSuggestedX, out linesMoved);
		newSuggestedX = TransformToAncestor(new Point(newSuggestedX, 0.0)).X;
		return positionAtNextLine;
	}

	internal override ITextPointer GetPositionAtNextPage(ITextPointer position, Point suggestedOffset, int count, out Point newSuggestedOffset, out int pagesMoved)
	{
		ITextPointer textPointer = position;
		newSuggestedOffset = suggestedOffset;
		Point point = suggestedOffset;
		pagesMoved = 0;
		if (count == 0)
		{
			return textPointer;
		}
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (IsPageMissing)
		{
			return position;
		}
		point.Y = GetYOffsetAtNextPage(point.Y, count, out pagesMoved);
		if (pagesMoved != 0)
		{
			point = TransformToDescendant(point);
			textPointer = _pageTextView.GetTextPositionFromPoint(point, snapToText: true);
			Invariant.Assert(textPointer != null);
			Rect rectangleFromTextPosition = _pageTextView.GetRectangleFromTextPosition(position);
			newSuggestedOffset = TransformToAncestor(new Point(rectangleFromTextPosition.X, rectangleFromTextPosition.Y));
		}
		return textPointer;
	}

	internal override bool IsAtCaretUnitBoundary(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (IsPageMissing)
		{
			return false;
		}
		return _pageTextView.IsAtCaretUnitBoundary(position);
	}

	internal override ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (IsPageMissing)
		{
			return null;
		}
		return _pageTextView.GetNextCaretUnitPosition(position, direction);
	}

	internal override ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (IsPageMissing)
		{
			return null;
		}
		return _pageTextView.GetBackspaceCaretUnitPosition(position);
	}

	internal override TextSegment GetLineRange(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (IsPageMissing)
		{
			return TextSegment.Null;
		}
		return _pageTextView.GetLineRange(position);
	}

	internal override ReadOnlyCollection<GlyphRun> GetGlyphRuns(ITextPointer start, ITextPointer end)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (IsPageMissing)
		{
			return new ReadOnlyCollection<GlyphRun>(new List<GlyphRun>());
		}
		return _pageTextView.GetGlyphRuns(start, end);
	}

	internal override bool Contains(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (IsPageMissing)
		{
			return false;
		}
		return _pageTextView.Contains(position);
	}

	internal void OnPageConnected()
	{
		OnPageDisconnected();
		if (_owner is DocumentPageView)
		{
			_page = ((DocumentPageView)_owner).DocumentPageInternal;
		}
		else if (_owner is FlowDocumentView)
		{
			_page = ((FlowDocumentView)_owner).DocumentPage;
		}
		if (_page is IServiceProvider)
		{
			_pageTextView = ((IServiceProvider)_page).GetService(typeof(ITextView)) as ITextView;
		}
		if (_pageTextView != null)
		{
			_pageTextView.Updated += HandlePageTextViewUpdated;
		}
		if (IsValid)
		{
			OnUpdated(EventArgs.Empty);
		}
	}

	internal void OnPageDisconnected()
	{
		if (_pageTextView != null)
		{
			_pageTextView.Updated -= HandlePageTextViewUpdated;
		}
		_pageTextView = null;
		_page = null;
	}

	internal void OnTransformChanged()
	{
		if (IsValid)
		{
			OnUpdated(EventArgs.Empty);
		}
	}

	internal override bool Validate()
	{
		if (!_owner.IsMeasureValid || !_owner.IsArrangeValid)
		{
			_owner.UpdateLayout();
		}
		if (_pageTextView != null)
		{
			return _pageTextView.Validate();
		}
		return false;
	}

	internal override bool Validate(ITextPointer position)
	{
		if (!(_owner is FlowDocumentView { Document: not null } flowDocumentView))
		{
			return base.Validate(position);
		}
		if (Validate())
		{
			BackgroundFormatInfo backgroundFormatInfo = flowDocumentView.Document.StructuralCache.BackgroundFormatInfo;
			FlowDocumentFormatter bottomlessFormatter = flowDocumentView.Document.BottomlessFormatter;
			int num = -1;
			while (IsValid && !Contains(position))
			{
				backgroundFormatInfo.BackgroundFormat(bottomlessFormatter, ignoreThrottle: true);
				_owner.UpdateLayout();
				if (backgroundFormatInfo.CPInterrupted <= num)
				{
					break;
				}
				num = backgroundFormatInfo.CPInterrupted;
			}
		}
		return IsValid && Contains(position);
	}

	internal override void ThrottleBackgroundTasksForUserInput()
	{
		if (_owner is FlowDocumentView { Document: not null } flowDocumentView)
		{
			flowDocumentView.Document.StructuralCache.ThrottleBackgroundFormatting();
		}
	}

	private void HandlePageTextViewUpdated(object sender, EventArgs e)
	{
		Invariant.Assert(_pageTextView != null);
		if (sender == _pageTextView)
		{
			OnUpdated(EventArgs.Empty);
		}
	}

	private Transform GetTransformToAncestor()
	{
		Invariant.Assert(IsValid && !IsPageMissing);
		Transform transform = _page.Visual.TransformToAncestor(_owner) as Transform;
		if (transform == null)
		{
			transform = Transform.Identity;
		}
		return transform;
	}

	private Point TransformToAncestor(Point point)
	{
		Invariant.Assert(IsValid && !IsPageMissing);
		GeneralTransform generalTransform = _page.Visual.TransformToAncestor(_owner);
		if (generalTransform != null)
		{
			point = generalTransform.Transform(point);
		}
		return point;
	}

	private Point TransformToDescendant(Point point)
	{
		Invariant.Assert(IsValid && !IsPageMissing);
		GeneralTransform generalTransform = _page.Visual.TransformToAncestor(_owner);
		if (generalTransform != null)
		{
			generalTransform = generalTransform.Inverse;
			if (generalTransform != null)
			{
				point = generalTransform.Transform(point);
			}
		}
		return point;
	}

	private double GetYOffsetAtNextPage(double offset, int count, out int pagesMoved)
	{
		double num = offset;
		pagesMoved = 0;
		if (_owner is IScrollInfo && ((IScrollInfo)_owner).ScrollOwner != null)
		{
			IScrollInfo obj = (IScrollInfo)_owner;
			double viewportHeight = obj.ViewportHeight;
			double extentHeight = obj.ExtentHeight;
			if (count > 0)
			{
				while (pagesMoved < count && DoubleUtil.LessThanOrClose(offset + viewportHeight, extentHeight))
				{
					num += viewportHeight;
					pagesMoved++;
				}
			}
			else
			{
				while (Math.Abs(pagesMoved) < Math.Abs(count) && DoubleUtil.GreaterThanOrClose(offset - viewportHeight, 0.0))
				{
					num -= viewportHeight;
					pagesMoved--;
				}
			}
		}
		return num;
	}
}
