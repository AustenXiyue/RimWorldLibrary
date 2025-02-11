using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace MS.Internal.Documents;

internal class MultiPageTextView : TextViewBase
{
	private class BringIntoViewRequest
	{
		internal readonly object UserState;

		internal BringIntoViewRequest(object userState)
		{
			UserState = userState;
		}
	}

	private class BringPositionIntoViewRequest : BringIntoViewRequest
	{
		internal readonly ITextPointer Position;

		internal bool Succeeded;

		internal BringPositionIntoViewRequest(ITextPointer position, object userState)
			: base(userState)
		{
			Position = position;
			Succeeded = false;
		}
	}

	private class BringPointIntoViewRequest : BringIntoViewRequest
	{
		internal readonly Point Point;

		internal ITextPointer Position;

		internal BringPointIntoViewRequest(Point point, object userState)
			: base(userState)
		{
			Point = point;
			Position = null;
		}
	}

	private class BringLineIntoViewRequest : BringIntoViewRequest
	{
		internal readonly ITextPointer Position;

		internal readonly double SuggestedX;

		internal readonly int Count;

		internal ITextPointer NewPosition;

		internal double NewSuggestedX;

		internal int NewCount;

		internal int NewPageNumber;

		internal BringLineIntoViewRequest(ITextPointer position, double suggestedX, int count, object userState)
			: base(userState)
		{
			Position = position;
			SuggestedX = suggestedX;
			Count = count;
			NewPosition = position;
			NewSuggestedX = suggestedX;
			NewCount = count;
		}
	}

	private class BringPageIntoViewRequest : BringIntoViewRequest
	{
		internal readonly ITextPointer Position;

		internal readonly Point SuggestedOffset;

		internal readonly int Count;

		internal ITextPointer NewPosition;

		internal Point NewSuggestedOffset;

		internal int NewCount;

		internal int NewPageNumber;

		internal BringPageIntoViewRequest(ITextPointer position, Point suggestedOffset, int count, object userState)
			: base(userState)
		{
			Position = position;
			SuggestedOffset = suggestedOffset;
			Count = count;
			NewPosition = position;
			NewSuggestedOffset = suggestedOffset;
			NewCount = count;
		}
	}

	private readonly DocumentViewerBase _viewer;

	private readonly UIElement _renderScope;

	private readonly ITextContainer _textContainer;

	private List<DocumentPageTextView> _pageTextViews;

	private BringIntoViewRequest _pendingRequest;

	internal override UIElement RenderScope => _renderScope;

	internal override ITextContainer TextContainer => _textContainer;

	internal override bool IsValid
	{
		get
		{
			bool result = false;
			if (_pageTextViews != null)
			{
				result = true;
				for (int i = 0; i < _pageTextViews.Count; i++)
				{
					if (!_pageTextViews[i].IsValid)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}
	}

	internal override bool RendersOwnSelection
	{
		get
		{
			if (_pageTextViews != null && _pageTextViews.Count > 0)
			{
				return _pageTextViews[0].RendersOwnSelection;
			}
			return false;
		}
	}

	internal override ReadOnlyCollection<TextSegment> TextSegments
	{
		get
		{
			List<TextSegment> list = new List<TextSegment>();
			if (IsValid)
			{
				for (int i = 0; i < _pageTextViews.Count; i++)
				{
					list.AddRange(_pageTextViews[i].TextSegments);
				}
			}
			return new ReadOnlyCollection<TextSegment>(list);
		}
	}

	internal MultiPageTextView(DocumentViewerBase viewer, UIElement renderScope, ITextContainer textContainer)
	{
		_viewer = viewer;
		_renderScope = renderScope;
		_textContainer = textContainer;
		_pageTextViews = new List<DocumentPageTextView>();
		OnPagesUpdatedCore();
	}

	protected override void OnUpdated(EventArgs e)
	{
		base.OnUpdated(e);
		if (IsValid)
		{
			OnUpdatedWorker(null);
		}
		else
		{
			_renderScope.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(OnUpdatedWorker), EventArgs.Empty);
		}
	}

	internal override ITextPointer GetTextPositionFromPoint(Point point, bool snapToText)
	{
		ITextPointer result = null;
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		DocumentPageTextView textViewFromPoint = GetTextViewFromPoint(point, snap: false);
		if (textViewFromPoint != null)
		{
			point = TransformToDescendant(textViewFromPoint.RenderScope, point);
			result = textViewFromPoint.GetTextPositionFromPoint(point, snapToText);
		}
		return result;
	}

	internal override Rect GetRawRectangleFromTextPosition(ITextPointer position, out Transform transform)
	{
		Rect result = Rect.Empty;
		transform = Transform.Identity;
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		DocumentPageTextView textViewFromPosition = GetTextViewFromPosition(position);
		if (textViewFromPosition != null)
		{
			result = textViewFromPosition.GetRawRectangleFromTextPosition(position, out var transform2);
			Transform transformToAncestor = GetTransformToAncestor(textViewFromPosition.RenderScope);
			transform = GetAggregateTransform(transform2, transformToAncestor);
		}
		return result;
	}

	internal override Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		Geometry geometry = null;
		int i = 0;
		for (int count = _pageTextViews.Count; i < count; i++)
		{
			ReadOnlyCollection<TextSegment> textSegments = _pageTextViews[i].TextSegments;
			for (int j = 0; j < textSegments.Count; j++)
			{
				TextSegment textSegment = textSegments[j];
				ITextPointer textPointer = ((startPosition.CompareTo(textSegment.Start) > 0) ? startPosition : textSegment.Start);
				ITextPointer textPointer2 = ((endPosition.CompareTo(textSegment.End) < 0) ? endPosition : textSegment.End);
				if (textPointer.CompareTo(textPointer2) < 0)
				{
					Geometry tightBoundingGeometryFromTextPositions = _pageTextViews[i].GetTightBoundingGeometryFromTextPositions(textPointer, textPointer2);
					if (tightBoundingGeometryFromTextPositions != null)
					{
						Transform affineTransform = _pageTextViews[i].RenderScope.TransformToAncestor(_renderScope).AffineTransform;
						CaretElement.AddTransformToGeometry(tightBoundingGeometryFromTextPositions, affineTransform);
						CaretElement.AddGeometry(ref geometry, tightBoundingGeometryFromTextPositions);
					}
				}
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
		int pageNumber;
		return GetPositionAtNextLineCore(position, suggestedX, count, out newSuggestedX, out linesMoved, out pageNumber);
	}

	internal override ITextPointer GetPositionAtNextPage(ITextPointer position, Point suggestedOffset, int count, out Point newSuggestedOffset, out int pagesMoved)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		int pageNumber;
		return GetPositionAtNextPageCore(position, suggestedOffset, count, out newSuggestedOffset, out pagesMoved, out pageNumber);
	}

	internal override bool IsAtCaretUnitBoundary(ITextPointer position)
	{
		bool result = false;
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		DocumentPageTextView textViewFromPosition = GetTextViewFromPosition(position);
		if (textViewFromPosition != null)
		{
			result = textViewFromPosition.IsAtCaretUnitBoundary(position);
		}
		return result;
	}

	internal override ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		ITextPointer result = null;
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		DocumentPageTextView textViewFromPosition = GetTextViewFromPosition(position);
		if (textViewFromPosition != null)
		{
			result = textViewFromPosition.GetNextCaretUnitPosition(position, direction);
		}
		return result;
	}

	internal override ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		ITextPointer result = null;
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		DocumentPageTextView textViewFromPosition = GetTextViewFromPosition(position);
		if (textViewFromPosition != null)
		{
			result = textViewFromPosition.GetBackspaceCaretUnitPosition(position);
		}
		return result;
	}

	internal override TextSegment GetLineRange(ITextPointer position)
	{
		TextSegment result = TextSegment.Null;
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		DocumentPageTextView textViewFromPosition = GetTextViewFromPosition(position);
		if (textViewFromPosition != null)
		{
			result = textViewFromPosition.GetLineRange(position);
		}
		return result;
	}

	internal override bool Contains(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		return GetTextViewFromPosition(position) != null;
	}

	internal override void BringPositionIntoViewAsync(ITextPointer position, object userState)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (_pendingRequest != null)
		{
			OnBringPositionIntoViewCompleted(new BringPositionIntoViewCompletedEventArgs(position, succeeded: false, null, cancelled: false, userState));
		}
		BringPositionIntoViewRequest bringPositionIntoViewRequest = (BringPositionIntoViewRequest)(_pendingRequest = new BringPositionIntoViewRequest(position, userState));
		if (GetTextViewFromPosition(position) != null)
		{
			bringPositionIntoViewRequest.Succeeded = true;
			OnBringPositionIntoViewCompleted(bringPositionIntoViewRequest);
		}
		else if (position is ContentPosition)
		{
			if (_viewer.Document.DocumentPaginator is DynamicDocumentPaginator dynamicDocumentPaginator)
			{
				int pageNumber = dynamicDocumentPaginator.GetPageNumber((ContentPosition)position) + 1;
				if (_viewer.CanGoToPage(pageNumber))
				{
					_viewer.GoToPage(pageNumber);
				}
				else
				{
					OnBringPositionIntoViewCompleted(bringPositionIntoViewRequest);
				}
			}
			else
			{
				OnBringPositionIntoViewCompleted(bringPositionIntoViewRequest);
			}
		}
		else
		{
			OnBringPositionIntoViewCompleted(bringPositionIntoViewRequest);
		}
	}

	internal override void BringPointIntoViewAsync(Point point, object userState)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (_pendingRequest != null)
		{
			OnBringPointIntoViewCompleted(new BringPointIntoViewCompletedEventArgs(point, null, succeeded: false, null, cancelled: false, userState));
			return;
		}
		BringPointIntoViewRequest bringPointIntoViewRequest = (BringPointIntoViewRequest)(_pendingRequest = new BringPointIntoViewRequest(point, userState));
		DocumentPageTextView textViewFromPoint = GetTextViewFromPoint(point, snap: false);
		if (textViewFromPoint != null)
		{
			point = TransformToDescendant(textViewFromPoint.RenderScope, point);
			ITextPointer textPositionFromPoint = textViewFromPoint.GetTextPositionFromPoint(point, snapToText: true);
			bringPointIntoViewRequest.Position = textPositionFromPoint;
			OnBringPointIntoViewCompleted(bringPointIntoViewRequest);
			return;
		}
		_renderScope.TransformToAncestor(_viewer).TryTransform(point, out point);
		bool flag = false;
		if (_viewer is FlowDocumentPageViewer)
		{
			flag = ((FlowDocumentPageViewer)_viewer).BringPointIntoView(point);
		}
		else if (_viewer is DocumentViewer)
		{
			flag = ((DocumentViewer)_viewer).BringPointIntoView(point);
		}
		else if (DoubleUtil.LessThan(point.X, 0.0))
		{
			if (_viewer.CanGoToPreviousPage)
			{
				_viewer.PreviousPage();
				flag = true;
			}
		}
		else if (DoubleUtil.GreaterThan(point.X, _viewer.RenderSize.Width))
		{
			if (_viewer.CanGoToNextPage)
			{
				_viewer.NextPage();
				flag = true;
			}
		}
		else if (DoubleUtil.LessThan(point.Y, 0.0))
		{
			if (_viewer.CanGoToPreviousPage)
			{
				_viewer.PreviousPage();
				flag = true;
			}
		}
		else if (DoubleUtil.GreaterThan(point.Y, _viewer.RenderSize.Height) && _viewer.CanGoToNextPage)
		{
			_viewer.NextPage();
			flag = true;
		}
		if (!flag)
		{
			OnBringPointIntoViewCompleted(bringPointIntoViewRequest);
		}
	}

	internal override void BringLineIntoViewAsync(ITextPointer position, double suggestedX, int count, object userState)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (_pendingRequest != null)
		{
			OnBringLineIntoViewCompleted(new BringLineIntoViewCompletedEventArgs(position, suggestedX, count, position, suggestedX, 0, succeeded: false, null, cancelled: false, userState));
			return;
		}
		_pendingRequest = new BringLineIntoViewRequest(position, suggestedX, count, userState);
		BringLineIntoViewCore((BringLineIntoViewRequest)_pendingRequest);
	}

	internal override void BringPageIntoViewAsync(ITextPointer position, Point suggestedOffset, int count, object userState)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (_pendingRequest != null)
		{
			OnBringPageIntoViewCompleted(new BringPageIntoViewCompletedEventArgs(position, suggestedOffset, count, position, suggestedOffset, 0, succeeded: false, null, cancelled: false, userState));
			return;
		}
		_pendingRequest = new BringPageIntoViewRequest(position, suggestedOffset, count, userState);
		BringPageIntoViewCore((BringPageIntoViewRequest)_pendingRequest);
	}

	internal override void CancelAsync(object userState)
	{
		if (_pendingRequest != null)
		{
			if (_pendingRequest is BringLineIntoViewRequest)
			{
				BringLineIntoViewRequest bringLineIntoViewRequest = (BringLineIntoViewRequest)_pendingRequest;
				OnBringLineIntoViewCompleted(new BringLineIntoViewCompletedEventArgs(bringLineIntoViewRequest.Position, bringLineIntoViewRequest.SuggestedX, bringLineIntoViewRequest.Count, bringLineIntoViewRequest.NewPosition, bringLineIntoViewRequest.NewSuggestedX, bringLineIntoViewRequest.Count - bringLineIntoViewRequest.NewCount, succeeded: false, null, cancelled: true, bringLineIntoViewRequest.UserState));
			}
			else if (_pendingRequest is BringPageIntoViewRequest)
			{
				BringPageIntoViewRequest bringPageIntoViewRequest = (BringPageIntoViewRequest)_pendingRequest;
				OnBringPageIntoViewCompleted(new BringPageIntoViewCompletedEventArgs(bringPageIntoViewRequest.Position, bringPageIntoViewRequest.SuggestedOffset, bringPageIntoViewRequest.Count, bringPageIntoViewRequest.NewPosition, bringPageIntoViewRequest.NewSuggestedOffset, bringPageIntoViewRequest.Count - bringPageIntoViewRequest.NewCount, succeeded: false, null, cancelled: true, bringPageIntoViewRequest.UserState));
			}
			else if (_pendingRequest is BringPointIntoViewRequest)
			{
				BringPointIntoViewRequest bringPointIntoViewRequest = (BringPointIntoViewRequest)_pendingRequest;
				OnBringPointIntoViewCompleted(new BringPointIntoViewCompletedEventArgs(bringPointIntoViewRequest.Point, bringPointIntoViewRequest.Position, succeeded: false, null, cancelled: true, bringPointIntoViewRequest.UserState));
			}
			else if (_pendingRequest is BringPositionIntoViewRequest)
			{
				BringPositionIntoViewRequest bringPositionIntoViewRequest = (BringPositionIntoViewRequest)_pendingRequest;
				OnBringPositionIntoViewCompleted(new BringPositionIntoViewCompletedEventArgs(bringPositionIntoViewRequest.Position, succeeded: false, null, cancelled: true, bringPositionIntoViewRequest.UserState));
			}
			_pendingRequest = null;
		}
	}

	internal void OnPagesUpdated()
	{
		OnPagesUpdatedCore();
		if (IsValid)
		{
			OnUpdated(EventArgs.Empty);
		}
	}

	internal void OnPageLayoutChanged()
	{
		if (IsValid)
		{
			OnUpdated(EventArgs.Empty);
		}
	}

	internal ITextView GetPageTextViewFromPosition(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		return GetTextViewFromPosition(position);
	}

	private void OnPagesUpdatedCore()
	{
		for (int i = 0; i < _pageTextViews.Count; i++)
		{
			_pageTextViews[i].Updated -= HandlePageTextViewUpdated;
		}
		_pageTextViews.Clear();
		ReadOnlyCollection<DocumentPageView> pageViews = _viewer.PageViews;
		if (pageViews == null)
		{
			return;
		}
		for (int i = 0; i < pageViews.Count; i++)
		{
			if (((IServiceProvider)pageViews[i]).GetService(typeof(ITextView)) is DocumentPageTextView documentPageTextView)
			{
				_pageTextViews.Add(documentPageTextView);
				documentPageTextView.Updated += HandlePageTextViewUpdated;
			}
		}
	}

	private void HandlePageTextViewUpdated(object sender, EventArgs e)
	{
		OnUpdated(EventArgs.Empty);
	}

	private void BringLineIntoViewCore(BringLineIntoViewRequest request)
	{
		double newSuggestedX;
		int linesMoved;
		int pageNumber;
		ITextPointer positionAtNextLineCore = GetPositionAtNextLineCore(request.NewPosition, request.NewSuggestedX, request.NewCount, out newSuggestedX, out linesMoved, out pageNumber);
		Invariant.Assert(Math.Abs(request.NewCount) >= Math.Abs(linesMoved));
		request.NewPosition = positionAtNextLineCore;
		request.NewSuggestedX = newSuggestedX;
		request.NewCount -= linesMoved;
		request.NewPageNumber = pageNumber;
		if (request.NewCount == 0)
		{
			OnBringLineIntoViewCompleted(request);
		}
		else if (positionAtNextLineCore is DocumentSequenceTextPointer || positionAtNextLineCore is FixedTextPointer)
		{
			if (_viewer.CanGoToPage(pageNumber + 1))
			{
				_viewer.GoToPage(pageNumber + 1);
			}
			else
			{
				OnBringLineIntoViewCompleted(request);
			}
		}
		else if (request.NewCount > 0)
		{
			if (_viewer.CanGoToNextPage)
			{
				_viewer.NextPage();
			}
			else
			{
				OnBringLineIntoViewCompleted(request);
			}
		}
		else if (_viewer.CanGoToPreviousPage)
		{
			_viewer.PreviousPage();
		}
		else
		{
			OnBringLineIntoViewCompleted(request);
		}
	}

	private void BringPageIntoViewCore(BringPageIntoViewRequest request)
	{
		Point newSuggestedOffset;
		int pagesMoved;
		int pageNumber;
		ITextPointer positionAtNextPageCore = GetPositionAtNextPageCore(request.NewPosition, request.NewSuggestedOffset, request.NewCount, out newSuggestedOffset, out pagesMoved, out pageNumber);
		Invariant.Assert(Math.Abs(request.NewCount) >= Math.Abs(pagesMoved));
		request.NewPosition = positionAtNextPageCore;
		request.NewSuggestedOffset = newSuggestedOffset;
		request.NewCount -= pagesMoved;
		if (request.NewCount == 0 || pageNumber == -1)
		{
			OnBringPageIntoViewCompleted(request);
			return;
		}
		pageNumber += ((request.NewCount > 0) ? 1 : (-1));
		if (_viewer.CanGoToPage(pageNumber + 1))
		{
			request.NewPageNumber = pageNumber;
			_viewer.GoToPage(pageNumber + 1);
		}
		else
		{
			OnBringPageIntoViewCompleted(request);
		}
	}

	private ITextPointer GetPositionAtNextLineCore(ITextPointer position, double suggestedX, int count, out double newSuggestedX, out int linesMoved, out int pageNumber)
	{
		DocumentPageTextView textViewFromPosition = GetTextViewFromPosition(position);
		ITextPointer result;
		if (textViewFromPosition != null)
		{
			int num = count;
			suggestedX = TransformToDescendant(textViewFromPosition.RenderScope, new Point(suggestedX, 0.0)).X;
			result = textViewFromPosition.GetPositionAtNextLine(position, suggestedX, count, out newSuggestedX, out linesMoved);
			pageNumber = ((DocumentPageView)textViewFromPosition.RenderScope).PageNumber;
			newSuggestedX = TransformToAncestor(textViewFromPosition.RenderScope, new Point(newSuggestedX, 0.0)).X;
			while (num != linesMoved)
			{
				int linesMoved2 = 0;
				count = num - linesMoved;
				pageNumber += ((count > 0) ? 1 : (-1));
				textViewFromPosition = GetTextViewFromPageNumber(pageNumber);
				if (textViewFromPosition == null)
				{
					break;
				}
				_ = textViewFromPosition.TextSegments;
				int num2 = count;
				if (count > 0)
				{
					position = textViewFromPosition.GetTextPositionFromPoint(new Point(suggestedX, 0.0), snapToText: true);
					if (position != null)
					{
						count--;
						linesMoved++;
					}
				}
				else
				{
					position = textViewFromPosition.GetTextPositionFromPoint(new Point(suggestedX, textViewFromPosition.RenderScope.RenderSize.Height), snapToText: true);
					if (position != null)
					{
						count++;
						linesMoved--;
					}
				}
				if (position != null)
				{
					if (count == 0)
					{
						result = GetPositionAtPageBoundary(num2 > 0, textViewFromPosition, position, suggestedX);
						newSuggestedX = suggestedX;
					}
					else
					{
						result = textViewFromPosition.GetPositionAtNextLine(position, suggestedX, count, out newSuggestedX, out linesMoved2);
						linesMoved += linesMoved2;
					}
					newSuggestedX = TransformToAncestor(textViewFromPosition.RenderScope, new Point(newSuggestedX, 0.0)).X;
				}
			}
		}
		else
		{
			result = position;
			linesMoved = 0;
			newSuggestedX = suggestedX;
			pageNumber = -1;
		}
		return result;
	}

	private ITextPointer GetPositionAtNextPageCore(ITextPointer position, Point suggestedOffset, int count, out Point newSuggestedOffset, out int pagesMoved, out int pageNumber)
	{
		ITextPointer textPointer = position;
		pagesMoved = 0;
		newSuggestedOffset = suggestedOffset;
		pageNumber = -1;
		DocumentPageTextView textViewFromPosition = GetTextViewFromPosition(position);
		if (textViewFromPosition != null)
		{
			int pageNumber2 = ((DocumentPageView)textViewFromPosition.RenderScope).PageNumber;
			DocumentPageTextView textViewForNextPage = GetTextViewForNextPage(pageNumber2, count, out pageNumber);
			pagesMoved = pageNumber - pageNumber2;
			Invariant.Assert(Math.Abs(pagesMoved) <= Math.Abs(count));
			if (pageNumber != pageNumber2 && textViewForNextPage != null)
			{
				Point point = TransformToDescendant(textViewFromPosition.RenderScope, suggestedOffset);
				textPointer = textViewForNextPage.GetTextPositionFromPoint(point, snapToText: true);
				if (textPointer != null)
				{
					Rect rectangleFromTextPosition = textViewForNextPage.GetRectangleFromTextPosition(textPointer);
					point = TransformToAncestor(textViewFromPosition.RenderScope, new Point(rectangleFromTextPosition.X, rectangleFromTextPosition.Y));
					newSuggestedOffset = point;
				}
				else
				{
					textPointer = position;
					pagesMoved = 0;
					pageNumber = pageNumber2;
				}
			}
			else
			{
				pagesMoved = 0;
				pageNumber = pageNumber2;
			}
		}
		return textPointer;
	}

	private ITextPointer GetPositionAtPageBoundary(bool pageTop, ITextView pageTextView, ITextPointer position, double suggestedX)
	{
		double newSuggestedX;
		int linesMoved;
		ITextPointer positionAtNextLine;
		if (pageTop)
		{
			positionAtNextLine = pageTextView.GetPositionAtNextLine(position, suggestedX, 1, out newSuggestedX, out linesMoved);
			if (linesMoved == 1)
			{
				return pageTextView.GetPositionAtNextLine(positionAtNextLine, newSuggestedX, -1, out newSuggestedX, out linesMoved);
			}
			return position;
		}
		positionAtNextLine = pageTextView.GetPositionAtNextLine(position, suggestedX, -1, out newSuggestedX, out linesMoved);
		if (linesMoved == -1)
		{
			return pageTextView.GetPositionAtNextLine(positionAtNextLine, newSuggestedX, 1, out newSuggestedX, out linesMoved);
		}
		return position;
	}

	private DocumentPageTextView GetTextViewFromPoint(Point point, bool snap)
	{
		DocumentPageTextView documentPageTextView = null;
		for (int i = 0; i < _pageTextViews.Count; i++)
		{
			if (TransformToAncestor(_pageTextViews[i].RenderScope, new Rect(_pageTextViews[i].RenderScope.RenderSize)).Contains(point))
			{
				documentPageTextView = _pageTextViews[i];
				break;
			}
		}
		if (documentPageTextView == null && snap)
		{
			double[] array = new double[_pageTextViews.Count];
			for (int i = 0; i < _pageTextViews.Count; i++)
			{
				Rect rect = TransformToAncestor(_pageTextViews[i].RenderScope, new Rect(_pageTextViews[i].RenderScope.RenderSize));
				double x = ((!(point.X >= rect.Left) || !(point.X <= rect.Right)) ? Math.Min(Math.Abs(point.X - rect.Left), Math.Abs(point.X - rect.Right)) : 0.0);
				double x2 = ((!(point.Y >= rect.Top) || !(point.Y <= rect.Bottom)) ? Math.Min(Math.Abs(point.Y - rect.Top), Math.Abs(point.Y - rect.Bottom)) : 0.0);
				array[i] = Math.Sqrt(Math.Pow(x, 2.0) + Math.Pow(x2, 2.0));
			}
			double num = double.MaxValue;
			for (int i = 0; i < array.Length; i++)
			{
				if (num > array[i])
				{
					num = array[i];
					documentPageTextView = _pageTextViews[i];
				}
			}
		}
		return documentPageTextView;
	}

	private DocumentPageTextView GetTextViewFromPosition(ITextPointer position)
	{
		DocumentPageTextView result = null;
		for (int i = 0; i < _pageTextViews.Count; i++)
		{
			if (_pageTextViews[i].Contains(position))
			{
				result = _pageTextViews[i];
				break;
			}
		}
		return result;
	}

	private DocumentPageTextView GetTextViewFromPageNumber(int pageNumber)
	{
		DocumentPageTextView result = null;
		for (int i = 0; i < _pageTextViews.Count; i++)
		{
			if (_pageTextViews[i].DocumentPageView.PageNumber == pageNumber)
			{
				result = _pageTextViews[i];
				break;
			}
		}
		return result;
	}

	private DocumentPageTextView GetTextViewForNextPage(int pageNumber, int count, out int newPageNumber)
	{
		Invariant.Assert(count != 0);
		newPageNumber = pageNumber + count;
		int num = newPageNumber;
		DocumentPageTextView documentPageTextView = null;
		int num2 = Math.Abs(count);
		for (int i = 0; i < _pageTextViews.Count; i++)
		{
			if (_pageTextViews[i].DocumentPageView.PageNumber == newPageNumber)
			{
				documentPageTextView = _pageTextViews[i];
				num = newPageNumber;
				break;
			}
			int pageNumber2 = _pageTextViews[i].DocumentPageView.PageNumber;
			if (count > 0 && pageNumber2 > pageNumber)
			{
				int num3 = pageNumber2 - pageNumber;
				if (num3 < num2)
				{
					num2 = num3;
					documentPageTextView = _pageTextViews[i];
					num = pageNumber2;
				}
			}
			else if (count < 0 && pageNumber2 < pageNumber)
			{
				int num4 = Math.Abs(pageNumber2 - pageNumber);
				if (num4 < num2)
				{
					num2 = num4;
					documentPageTextView = _pageTextViews[i];
					num = pageNumber2;
				}
			}
		}
		if (documentPageTextView != null)
		{
			newPageNumber = num;
		}
		else
		{
			newPageNumber = pageNumber;
			documentPageTextView = GetTextViewFromPageNumber(pageNumber);
		}
		Invariant.Assert(newPageNumber >= 0);
		return documentPageTextView;
	}

	private Transform GetTransformToAncestor(Visual innerScope)
	{
		Transform transform = innerScope.TransformToAncestor(_renderScope) as Transform;
		if (transform == null)
		{
			transform = Transform.Identity;
		}
		return transform;
	}

	private Rect TransformToAncestor(Visual innerScope, Rect rect)
	{
		if (rect != Rect.Empty)
		{
			GeneralTransform generalTransform = innerScope.TransformToAncestor(_renderScope);
			if (generalTransform != null)
			{
				rect = generalTransform.TransformBounds(rect);
			}
		}
		return rect;
	}

	private Point TransformToAncestor(Visual innerScope, Point point)
	{
		GeneralTransform generalTransform = innerScope.TransformToAncestor(_renderScope);
		if (generalTransform != null)
		{
			point = generalTransform.Transform(point);
		}
		return point;
	}

	private Point TransformToDescendant(Visual innerScope, Point point)
	{
		GeneralTransform generalTransform = innerScope.TransformToAncestor(_renderScope);
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

	private void OnBringPositionIntoViewCompleted(BringPositionIntoViewRequest request)
	{
		_pendingRequest = null;
		OnBringPositionIntoViewCompleted(new BringPositionIntoViewCompletedEventArgs(request.Position, request.Succeeded, null, cancelled: false, request.UserState));
	}

	private void OnBringPointIntoViewCompleted(BringPointIntoViewRequest request)
	{
		_pendingRequest = null;
		OnBringPointIntoViewCompleted(new BringPointIntoViewCompletedEventArgs(request.Point, request.Position, request.Position != null, null, cancelled: false, request.UserState));
	}

	private void OnBringLineIntoViewCompleted(BringLineIntoViewRequest request)
	{
		_pendingRequest = null;
		OnBringLineIntoViewCompleted(new BringLineIntoViewCompletedEventArgs(request.Position, request.SuggestedX, request.Count, request.NewPosition, request.NewSuggestedX, request.Count - request.NewCount, request.NewCount == 0, null, cancelled: false, request.UserState));
	}

	private void OnBringPageIntoViewCompleted(BringPageIntoViewRequest request)
	{
		_pendingRequest = null;
		OnBringPageIntoViewCompleted(new BringPageIntoViewCompletedEventArgs(request.Position, request.SuggestedOffset, request.Count, request.NewPosition, request.NewSuggestedOffset, request.Count - request.NewCount, request.NewCount == 0, null, cancelled: false, request.UserState));
	}

	private object OnUpdatedWorker(object o)
	{
		if (IsValid && _pendingRequest != null)
		{
			if (_pendingRequest is BringLineIntoViewRequest)
			{
				BringLineIntoViewRequest bringLineIntoViewRequest = (BringLineIntoViewRequest)_pendingRequest;
				ITextView textViewFromPageNumber = GetTextViewFromPageNumber(bringLineIntoViewRequest.NewPageNumber);
				if (textViewFromPageNumber != null)
				{
					_ = TransformToDescendant(textViewFromPageNumber.RenderScope, new Point(bringLineIntoViewRequest.NewSuggestedX, 0.0)).X;
					ITextPointer textPositionFromPoint;
					if (bringLineIntoViewRequest.Count > 0)
					{
						textPositionFromPoint = textViewFromPageNumber.GetTextPositionFromPoint(new Point(-1.0, -1.0), snapToText: true);
						if (textPositionFromPoint != null)
						{
							bringLineIntoViewRequest.NewCount--;
						}
					}
					else
					{
						textPositionFromPoint = textViewFromPageNumber.GetTextPositionFromPoint((Point)textViewFromPageNumber.RenderScope.RenderSize, snapToText: true);
						if (textPositionFromPoint != null)
						{
							bringLineIntoViewRequest.NewCount++;
						}
					}
					if (textPositionFromPoint == null)
					{
						if (bringLineIntoViewRequest.NewPosition == null)
						{
							bringLineIntoViewRequest.NewPosition = bringLineIntoViewRequest.Position;
							bringLineIntoViewRequest.NewCount = bringLineIntoViewRequest.Count;
						}
						OnBringLineIntoViewCompleted(bringLineIntoViewRequest);
					}
					else if (bringLineIntoViewRequest.NewCount != 0)
					{
						bringLineIntoViewRequest.NewPosition = textPositionFromPoint;
						BringLineIntoViewCore(bringLineIntoViewRequest);
					}
					else
					{
						bringLineIntoViewRequest.NewPosition = GetPositionAtPageBoundary(bringLineIntoViewRequest.Count > 0, textViewFromPageNumber, textPositionFromPoint, bringLineIntoViewRequest.NewSuggestedX);
						OnBringLineIntoViewCompleted(bringLineIntoViewRequest);
					}
				}
				else if (IsPageNumberOutOfRange(bringLineIntoViewRequest.NewPageNumber))
				{
					OnBringLineIntoViewCompleted(bringLineIntoViewRequest);
				}
			}
			else if (_pendingRequest is BringPageIntoViewRequest)
			{
				BringPageIntoViewRequest bringPageIntoViewRequest = (BringPageIntoViewRequest)_pendingRequest;
				ITextView textViewFromPageNumber = GetTextViewFromPageNumber(bringPageIntoViewRequest.NewPageNumber);
				if (textViewFromPageNumber != null)
				{
					Point point = TransformToDescendant(textViewFromPageNumber.RenderScope, bringPageIntoViewRequest.NewSuggestedOffset);
					Point point2 = point;
					Invariant.Assert(bringPageIntoViewRequest.NewCount != 0);
					ITextPointer textPositionFromPoint = textViewFromPageNumber.GetTextPositionFromPoint(point2, snapToText: true);
					if (textPositionFromPoint != null)
					{
						bringPageIntoViewRequest.NewCount = ((bringPageIntoViewRequest.Count > 0) ? (bringPageIntoViewRequest.NewCount - 1) : (bringPageIntoViewRequest.NewCount + 1));
					}
					if (textPositionFromPoint == null)
					{
						if (bringPageIntoViewRequest.NewPosition == null)
						{
							bringPageIntoViewRequest.NewPosition = bringPageIntoViewRequest.Position;
							bringPageIntoViewRequest.NewCount = bringPageIntoViewRequest.Count;
						}
						OnBringPageIntoViewCompleted(bringPageIntoViewRequest);
					}
					else if (bringPageIntoViewRequest.NewCount != 0)
					{
						bringPageIntoViewRequest.NewPosition = textPositionFromPoint;
						BringPageIntoViewCore(bringPageIntoViewRequest);
					}
					else
					{
						bringPageIntoViewRequest.NewPosition = textPositionFromPoint;
						OnBringPageIntoViewCompleted(bringPageIntoViewRequest);
					}
				}
				else if (IsPageNumberOutOfRange(bringPageIntoViewRequest.NewPageNumber))
				{
					OnBringPageIntoViewCompleted(bringPageIntoViewRequest);
				}
			}
			else if (_pendingRequest is BringPointIntoViewRequest)
			{
				BringPointIntoViewRequest bringPointIntoViewRequest = (BringPointIntoViewRequest)_pendingRequest;
				ITextView textViewFromPageNumber = GetTextViewFromPoint(bringPointIntoViewRequest.Point, snap: true);
				if (textViewFromPageNumber != null)
				{
					Point point = TransformToDescendant(textViewFromPageNumber.RenderScope, bringPointIntoViewRequest.Point);
					bringPointIntoViewRequest.Position = textViewFromPageNumber.GetTextPositionFromPoint(point, snapToText: true);
				}
				OnBringPointIntoViewCompleted(bringPointIntoViewRequest);
			}
			else if (_pendingRequest is BringPositionIntoViewRequest)
			{
				BringPositionIntoViewRequest bringPositionIntoViewRequest = (BringPositionIntoViewRequest)_pendingRequest;
				bringPositionIntoViewRequest.Succeeded = bringPositionIntoViewRequest.Position.HasValidLayout;
				OnBringPositionIntoViewCompleted(bringPositionIntoViewRequest);
			}
		}
		return null;
	}

	private bool IsPageNumberOutOfRange(int pageNumber)
	{
		if (pageNumber < 0)
		{
			return true;
		}
		IDocumentPaginatorSource document = _viewer.Document;
		if (document == null)
		{
			return true;
		}
		DocumentPaginator documentPaginator = document.DocumentPaginator;
		if (documentPaginator == null)
		{
			return true;
		}
		if (documentPaginator.IsPageCountValid && pageNumber >= documentPaginator.PageCount)
		{
			return true;
		}
		return false;
	}
}
