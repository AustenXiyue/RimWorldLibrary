using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.Documents;

internal abstract class TextViewBase : ITextView
{
	internal abstract UIElement RenderScope { get; }

	internal abstract ITextContainer TextContainer { get; }

	internal abstract bool IsValid { get; }

	internal virtual bool RendersOwnSelection => false;

	internal abstract ReadOnlyCollection<TextSegment> TextSegments { get; }

	UIElement ITextView.RenderScope => RenderScope;

	ITextContainer ITextView.TextContainer => TextContainer;

	bool ITextView.IsValid => IsValid;

	bool ITextView.RendersOwnSelection => RendersOwnSelection;

	ReadOnlyCollection<TextSegment> ITextView.TextSegments => TextSegments;

	public event BringPositionIntoViewCompletedEventHandler BringPositionIntoViewCompleted;

	public event BringPointIntoViewCompletedEventHandler BringPointIntoViewCompleted;

	public event BringLineIntoViewCompletedEventHandler BringLineIntoViewCompleted;

	public event BringPageIntoViewCompletedEventHandler BringPageIntoViewCompleted;

	public event EventHandler Updated;

	internal abstract ITextPointer GetTextPositionFromPoint(Point point, bool snapToText);

	internal virtual Rect GetRectangleFromTextPosition(ITextPointer position)
	{
		Transform transform;
		Rect rect = GetRawRectangleFromTextPosition(position, out transform);
		Invariant.Assert(transform != null);
		if (rect != Rect.Empty)
		{
			rect = transform.TransformBounds(rect);
		}
		return rect;
	}

	internal abstract Rect GetRawRectangleFromTextPosition(ITextPointer position, out Transform transform);

	internal abstract Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition);

	internal abstract ITextPointer GetPositionAtNextLine(ITextPointer position, double suggestedX, int count, out double newSuggestedX, out int linesMoved);

	internal virtual ITextPointer GetPositionAtNextPage(ITextPointer position, Point suggestedOffset, int count, out Point newSuggestedOffset, out int pagesMoved)
	{
		newSuggestedOffset = suggestedOffset;
		pagesMoved = 0;
		return position;
	}

	internal abstract bool IsAtCaretUnitBoundary(ITextPointer position);

	internal abstract ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction);

	internal abstract ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position);

	internal abstract TextSegment GetLineRange(ITextPointer position);

	internal virtual ReadOnlyCollection<GlyphRun> GetGlyphRuns(ITextPointer start, ITextPointer end)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		return new ReadOnlyCollection<GlyphRun>(new List<GlyphRun>());
	}

	internal abstract bool Contains(ITextPointer position);

	internal static void BringRectIntoViewMinimally(ITextView textView, Rect rect)
	{
		if (textView.RenderScope is IScrollInfo scrollInfo)
		{
			Rect rect2 = new Rect(scrollInfo.HorizontalOffset, scrollInfo.VerticalOffset, scrollInfo.ViewportWidth, scrollInfo.ViewportHeight);
			rect.X += rect2.X;
			rect.Y += rect2.Y;
			double num = ScrollContentPresenter.ComputeScrollOffsetWithMinimalScroll(rect2.Left, rect2.Right, rect.Left, rect.Right);
			double num2 = ScrollContentPresenter.ComputeScrollOffsetWithMinimalScroll(rect2.Top, rect2.Bottom, rect.Top, rect.Bottom);
			scrollInfo.SetHorizontalOffset(num);
			scrollInfo.SetVerticalOffset(num2);
			if (FrameworkElement.GetFrameworkParent(textView.RenderScope) is FrameworkElement frameworkElement)
			{
				if (scrollInfo.ViewportWidth > 0.0)
				{
					rect.X -= num;
				}
				if (scrollInfo.ViewportHeight > 0.0)
				{
					rect.Y -= num2;
				}
				frameworkElement.BringIntoView(rect);
			}
		}
		else
		{
			((FrameworkElement)textView.RenderScope).BringIntoView(rect);
		}
	}

	internal virtual void BringPositionIntoViewAsync(ITextPointer position, object userState)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		OnBringPositionIntoViewCompleted(new BringPositionIntoViewCompletedEventArgs(position, Contains(position), null, cancelled: false, userState));
	}

	internal virtual void BringPointIntoViewAsync(Point point, object userState)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ITextPointer textPositionFromPoint = GetTextPositionFromPoint(point, snapToText: true);
		OnBringPointIntoViewCompleted(new BringPointIntoViewCompletedEventArgs(point, textPositionFromPoint, textPositionFromPoint != null, null, cancelled: false, userState));
	}

	internal virtual void BringLineIntoViewAsync(ITextPointer position, double suggestedX, int count, object userState)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		double newSuggestedX;
		int linesMoved;
		ITextPointer positionAtNextLine = GetPositionAtNextLine(position, suggestedX, count, out newSuggestedX, out linesMoved);
		OnBringLineIntoViewCompleted(new BringLineIntoViewCompletedEventArgs(position, suggestedX, count, positionAtNextLine, newSuggestedX, linesMoved, linesMoved == count, null, cancelled: false, userState));
	}

	internal virtual void BringPageIntoViewAsync(ITextPointer position, Point suggestedOffset, int count, object userState)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		Point newSuggestedOffset;
		int pagesMoved;
		ITextPointer positionAtNextPage = GetPositionAtNextPage(position, suggestedOffset, count, out newSuggestedOffset, out pagesMoved);
		OnBringPageIntoViewCompleted(new BringPageIntoViewCompletedEventArgs(position, suggestedOffset, count, positionAtNextPage, newSuggestedOffset, pagesMoved, pagesMoved == count, null, cancelled: false, userState));
	}

	internal virtual void CancelAsync(object userState)
	{
	}

	internal virtual bool Validate()
	{
		return IsValid;
	}

	internal virtual bool Validate(Point point)
	{
		return Validate();
	}

	internal virtual bool Validate(ITextPointer position)
	{
		Validate();
		if (IsValid)
		{
			return Contains(position);
		}
		return false;
	}

	internal virtual void ThrottleBackgroundTasksForUserInput()
	{
	}

	protected virtual void OnBringPositionIntoViewCompleted(BringPositionIntoViewCompletedEventArgs e)
	{
		if (this.BringPositionIntoViewCompleted != null)
		{
			this.BringPositionIntoViewCompleted(this, e);
		}
	}

	protected virtual void OnBringPointIntoViewCompleted(BringPointIntoViewCompletedEventArgs e)
	{
		if (this.BringPointIntoViewCompleted != null)
		{
			this.BringPointIntoViewCompleted(this, e);
		}
	}

	protected virtual void OnBringLineIntoViewCompleted(BringLineIntoViewCompletedEventArgs e)
	{
		if (this.BringLineIntoViewCompleted != null)
		{
			this.BringLineIntoViewCompleted(this, e);
		}
	}

	protected virtual void OnBringPageIntoViewCompleted(BringPageIntoViewCompletedEventArgs e)
	{
		if (this.BringPageIntoViewCompleted != null)
		{
			this.BringPageIntoViewCompleted(this, e);
		}
	}

	protected virtual void OnUpdated(EventArgs e)
	{
		if (this.Updated != null)
		{
			this.Updated(this, e);
		}
	}

	protected virtual Transform GetAggregateTransform(Transform firstTransform, Transform secondTransform)
	{
		Invariant.Assert(firstTransform != null);
		Invariant.Assert(secondTransform != null);
		if (firstTransform.IsIdentity)
		{
			return secondTransform;
		}
		if (secondTransform.IsIdentity)
		{
			return firstTransform;
		}
		return new MatrixTransform(firstTransform.Value * secondTransform.Value);
	}

	ITextPointer ITextView.GetTextPositionFromPoint(Point point, bool snapToText)
	{
		return GetTextPositionFromPoint(point, snapToText);
	}

	Rect ITextView.GetRectangleFromTextPosition(ITextPointer position)
	{
		return GetRectangleFromTextPosition(position);
	}

	Rect ITextView.GetRawRectangleFromTextPosition(ITextPointer position, out Transform transform)
	{
		return GetRawRectangleFromTextPosition(position, out transform);
	}

	Geometry ITextView.GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		return GetTightBoundingGeometryFromTextPositions(startPosition, endPosition);
	}

	ITextPointer ITextView.GetPositionAtNextLine(ITextPointer position, double suggestedX, int count, out double newSuggestedX, out int linesMoved)
	{
		return GetPositionAtNextLine(position, suggestedX, count, out newSuggestedX, out linesMoved);
	}

	ITextPointer ITextView.GetPositionAtNextPage(ITextPointer position, Point suggestedOffset, int count, out Point newSuggestedOffset, out int pagesMoved)
	{
		return GetPositionAtNextPage(position, suggestedOffset, count, out newSuggestedOffset, out pagesMoved);
	}

	bool ITextView.IsAtCaretUnitBoundary(ITextPointer position)
	{
		return IsAtCaretUnitBoundary(position);
	}

	ITextPointer ITextView.GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		return GetNextCaretUnitPosition(position, direction);
	}

	ITextPointer ITextView.GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		return GetBackspaceCaretUnitPosition(position);
	}

	TextSegment ITextView.GetLineRange(ITextPointer position)
	{
		return GetLineRange(position);
	}

	ReadOnlyCollection<GlyphRun> ITextView.GetGlyphRuns(ITextPointer start, ITextPointer end)
	{
		return GetGlyphRuns(start, end);
	}

	bool ITextView.Contains(ITextPointer position)
	{
		return Contains(position);
	}

	void ITextView.BringPositionIntoViewAsync(ITextPointer position, object userState)
	{
		BringPositionIntoViewAsync(position, userState);
	}

	void ITextView.BringPointIntoViewAsync(Point point, object userState)
	{
		BringPointIntoViewAsync(point, userState);
	}

	void ITextView.BringLineIntoViewAsync(ITextPointer position, double suggestedX, int count, object userState)
	{
		BringLineIntoViewAsync(position, suggestedX, count, userState);
	}

	void ITextView.BringPageIntoViewAsync(ITextPointer position, Point suggestedOffset, int count, object userState)
	{
		BringPageIntoViewAsync(position, suggestedOffset, count, userState);
	}

	void ITextView.CancelAsync(object userState)
	{
		CancelAsync(userState);
	}

	bool ITextView.Validate()
	{
		return Validate();
	}

	bool ITextView.Validate(Point point)
	{
		return Validate(point);
	}

	bool ITextView.Validate(ITextPointer position)
	{
		return Validate(position);
	}

	void ITextView.ThrottleBackgroundTasksForUserInput()
	{
		ThrottleBackgroundTasksForUserInput();
	}
}
