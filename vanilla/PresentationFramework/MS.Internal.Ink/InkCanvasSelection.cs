using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using MS.Internal.Controls;

namespace MS.Internal.Ink;

internal sealed class InkCanvasSelection
{
	private InkCanvas _inkCanvas;

	private StrokeCollection _selectedStrokes;

	private Rect _cachedStrokesBounds;

	private bool _areStrokesChanged;

	private List<UIElement> _selectedElements;

	private EventHandler _layoutUpdatedHandler;

	private InkCanvasSelectionHitResult? _activeSelectionHitResult;

	internal StrokeCollection SelectedStrokes
	{
		get
		{
			if (_selectedStrokes == null)
			{
				_selectedStrokes = new StrokeCollection();
				_areStrokesChanged = true;
			}
			return _selectedStrokes;
		}
	}

	internal ReadOnlyCollection<UIElement> SelectedElements
	{
		get
		{
			if (_selectedElements == null)
			{
				_selectedElements = new List<UIElement>();
			}
			return new ReadOnlyCollection<UIElement>(_selectedElements);
		}
	}

	internal bool HasSelection
	{
		get
		{
			if (SelectedStrokes.Count == 0)
			{
				return SelectedElements.Count != 0;
			}
			return true;
		}
	}

	internal Rect SelectionBounds => Rect.Union(GetStrokesBounds(), GetElementsUnionBounds());

	private IEnumerable<Rect> SelectedElementsBoundsEnumerator
	{
		get
		{
			EnusreElementsBounds();
			InkCanvasInnerCanvas innerCanvas = _inkCanvas.InnerCanvas;
			foreach (UIElement selectedElement in SelectedElements)
			{
				GeneralTransform generalTransform = selectedElement.TransformToAncestor(innerCanvas);
				Size renderSize = selectedElement.RenderSize;
				Rect rect = new Rect(0.0, 0.0, renderSize.Width, renderSize.Height);
				yield return generalTransform.TransformBounds(rect);
			}
		}
	}

	internal InkCanvasSelection(InkCanvas inkCanvas)
	{
		if (inkCanvas == null)
		{
			throw new ArgumentNullException("inkCanvas");
		}
		_inkCanvas = inkCanvas;
		_inkCanvas.FeedbackAdorner.UpdateBounds(Rect.Empty);
	}

	internal void StartFeedbackAdorner(Rect feedbackRect, InkCanvasSelectionHitResult activeSelectionHitResult)
	{
		_activeSelectionHitResult = activeSelectionHitResult;
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_inkCanvas.InnerCanvas);
		InkCanvasFeedbackAdorner feedbackAdorner = _inkCanvas.FeedbackAdorner;
		adornerLayer.Add(feedbackAdorner);
		feedbackAdorner.UpdateBounds(feedbackRect);
	}

	internal void UpdateFeedbackAdorner(Rect feedbackRect)
	{
		_inkCanvas.FeedbackAdorner.UpdateBounds(feedbackRect);
	}

	internal void EndFeedbackAdorner(Rect finalRectangle)
	{
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_inkCanvas.InnerCanvas);
		InkCanvasFeedbackAdorner feedbackAdorner = _inkCanvas.FeedbackAdorner;
		feedbackAdorner.UpdateBounds(Rect.Empty);
		adornerLayer.Remove(feedbackAdorner);
		CommitChanges(finalRectangle, raiseEvent: true);
		_activeSelectionHitResult = null;
	}

	internal void Select(StrokeCollection strokes, IList<UIElement> elements, bool raiseSelectionChanged)
	{
		int num = 0;
		SelectionIsDifferentThanCurrent(strokes, out var strokesAreDifferent, elements, out var elementsAreDifferent);
		if (!(strokesAreDifferent || elementsAreDifferent))
		{
			return;
		}
		if (strokesAreDifferent && SelectedStrokes.Count != 0)
		{
			QuitListeningToStrokeChanges();
			num = SelectedStrokes.Count;
			for (int i = 0; i < num; i++)
			{
				SelectedStrokes[i].IsSelected = false;
			}
		}
		_selectedStrokes = strokes;
		_areStrokesChanged = true;
		_selectedElements = new List<UIElement>(elements);
		if (_inkCanvas.ActiveEditingMode == InkCanvasEditingMode.Select)
		{
			num = strokes.Count;
			for (int j = 0; j < num; j++)
			{
				strokes[j].IsSelected = true;
			}
		}
		UpdateCanvasLayoutUpdatedHandler();
		UpdateSelectionAdorner();
		ListenToStrokeChanges();
		if (raiseSelectionChanged)
		{
			_inkCanvas.RaiseSelectionChanged(EventArgs.Empty);
		}
	}

	internal void CommitChanges(Rect finalRectangle, bool raiseEvent)
	{
		Rect selectionBounds = SelectionBounds;
		if (selectionBounds.IsEmpty)
		{
			return;
		}
		try
		{
			QuitListeningToStrokeChanges();
			if (raiseEvent)
			{
				if (!DoubleUtil.AreClose(finalRectangle.Height, selectionBounds.Height) || !DoubleUtil.AreClose(finalRectangle.Width, selectionBounds.Width))
				{
					CommitResizeChange(finalRectangle);
				}
				else if (!DoubleUtil.AreClose(finalRectangle.Top, selectionBounds.Top) || !DoubleUtil.AreClose(finalRectangle.Left, selectionBounds.Left))
				{
					CommitMoveChange(finalRectangle);
				}
			}
			else
			{
				MoveSelection(selectionBounds, finalRectangle);
			}
		}
		finally
		{
			ListenToStrokeChanges();
		}
	}

	internal void RemoveElement(UIElement removedElement)
	{
		if (_selectedElements != null && _selectedElements.Count != 0 && _selectedElements.Remove(removedElement) && _selectedElements.Count == 0)
		{
			UpdateCanvasLayoutUpdatedHandler();
			UpdateSelectionAdorner();
		}
	}

	internal void UpdateElementBounds(UIElement element, Matrix transform)
	{
		UpdateElementBounds(element, element, transform);
	}

	internal void UpdateElementBounds(UIElement originalElement, UIElement updatedElement, Matrix transform)
	{
		if (originalElement.DependencyObjectType.Id != updatedElement.DependencyObjectType.Id)
		{
			return;
		}
		GeneralTransform generalTransform = originalElement.TransformToAncestor(_inkCanvas.InnerCanvas);
		FrameworkElement frameworkElement = originalElement as FrameworkElement;
		Thickness thickness = default(Thickness);
		Size size;
		if (frameworkElement == null)
		{
			size = originalElement.RenderSize;
		}
		else
		{
			size = new Size(frameworkElement.ActualWidth, frameworkElement.ActualHeight);
			thickness = frameworkElement.Margin;
		}
		Rect rect = new Rect(0.0, 0.0, size.Width, size.Height);
		rect = generalTransform.TransformBounds(rect);
		Rect rect2 = Rect.Transform(rect, transform);
		if (!DoubleUtil.AreClose(rect.Width, rect2.Width))
		{
			if (frameworkElement == null)
			{
				Size renderSize = originalElement.RenderSize;
				renderSize.Width = rect2.Width;
				updatedElement.RenderSize = renderSize;
			}
			else
			{
				((FrameworkElement)updatedElement).Width = rect2.Width;
			}
		}
		if (!DoubleUtil.AreClose(rect.Height, rect2.Height))
		{
			if (frameworkElement == null)
			{
				Size renderSize2 = originalElement.RenderSize;
				renderSize2.Height = rect2.Height;
				updatedElement.RenderSize = renderSize2;
			}
			else
			{
				((FrameworkElement)updatedElement).Height = rect2.Height;
			}
		}
		double left = InkCanvas.GetLeft(originalElement);
		double top = InkCanvas.GetTop(originalElement);
		double right = InkCanvas.GetRight(originalElement);
		double bottom = InkCanvas.GetBottom(originalElement);
		Point point = default(Point);
		if (!double.IsNaN(left))
		{
			point.X = left;
		}
		else if (!double.IsNaN(right))
		{
			point.X = right;
		}
		if (!double.IsNaN(top))
		{
			point.Y = top;
		}
		else if (!double.IsNaN(bottom))
		{
			point.Y = bottom;
		}
		Point point2 = point * transform;
		if (!double.IsNaN(left))
		{
			InkCanvas.SetLeft(updatedElement, point2.X - thickness.Left);
		}
		else if (!double.IsNaN(right))
		{
			InkCanvas.SetRight(updatedElement, right - (point2.X - point.X));
		}
		else
		{
			InkCanvas.SetLeft(updatedElement, point2.X - thickness.Left);
		}
		if (!double.IsNaN(top))
		{
			InkCanvas.SetTop(updatedElement, point2.Y - thickness.Top);
		}
		else if (!double.IsNaN(bottom))
		{
			InkCanvas.SetBottom(updatedElement, bottom - (point2.Y - point.Y));
		}
		else
		{
			InkCanvas.SetTop(updatedElement, point2.Y - thickness.Top);
		}
	}

	internal void TransformStrokes(StrokeCollection strokes, Matrix matrix)
	{
		strokes.Transform(matrix, applyToStylusTip: false);
	}

	internal InkCanvasSelectionHitResult HitTestSelection(Point pointOnInkCanvas)
	{
		if (_activeSelectionHitResult.HasValue)
		{
			return _activeSelectionHitResult.Value;
		}
		if (!HasSelection)
		{
			return InkCanvasSelectionHitResult.None;
		}
		Point point = _inkCanvas.TransformToDescendant(_inkCanvas.SelectionAdorner).Transform(pointOnInkCanvas);
		InkCanvasSelectionHitResult inkCanvasSelectionHitResult = _inkCanvas.SelectionAdorner.SelectionHandleHitTest(point);
		if (inkCanvasSelectionHitResult == InkCanvasSelectionHitResult.Selection && SelectedElements.Count == 1 && SelectedStrokes.Count == 0)
		{
			Point pointOnInnerCanvas = _inkCanvas.TransformToDescendant(_inkCanvas.InnerCanvas).Transform(pointOnInkCanvas);
			if (HasHitSingleSelectedElement(pointOnInnerCanvas))
			{
				inkCanvasSelectionHitResult = InkCanvasSelectionHitResult.None;
			}
		}
		return inkCanvasSelectionHitResult;
	}

	internal void SelectionIsDifferentThanCurrent(StrokeCollection strokes, out bool strokesAreDifferent, IList<UIElement> elements, out bool elementsAreDifferent)
	{
		strokesAreDifferent = false;
		elementsAreDifferent = false;
		if (SelectedStrokes.Count == 0)
		{
			if (strokes.Count > 0)
			{
				strokesAreDifferent = true;
			}
		}
		else if (!StrokesAreEqual(SelectedStrokes, strokes))
		{
			strokesAreDifferent = true;
		}
		if (SelectedElements.Count == 0)
		{
			if (elements.Count > 0)
			{
				elementsAreDifferent = true;
			}
		}
		else if (!FrameworkElementArraysAreEqual(elements, SelectedElements))
		{
			elementsAreDifferent = true;
		}
	}

	private bool HasHitSingleSelectedElement(Point pointOnInnerCanvas)
	{
		bool result = false;
		if (SelectedElements.Count == 1)
		{
			IEnumerator<Rect> enumerator = SelectedElementsBoundsEnumerator.GetEnumerator();
			if (enumerator.MoveNext())
			{
				result = enumerator.Current.Contains(pointOnInnerCanvas);
			}
		}
		return result;
	}

	private void QuitListeningToStrokeChanges()
	{
		if (_inkCanvas.Strokes != null)
		{
			_inkCanvas.Strokes.StrokesChanged -= OnStrokeCollectionChanged;
		}
		foreach (Stroke selectedStroke in SelectedStrokes)
		{
			selectedStroke.Invalidated -= OnStrokeInvalidated;
		}
	}

	private void ListenToStrokeChanges()
	{
		if (_inkCanvas.Strokes != null)
		{
			_inkCanvas.Strokes.StrokesChanged += OnStrokeCollectionChanged;
		}
		foreach (Stroke selectedStroke in SelectedStrokes)
		{
			selectedStroke.Invalidated += OnStrokeInvalidated;
		}
	}

	private void CommitMoveChange(Rect finalRectangle)
	{
		Rect selectionBounds = SelectionBounds;
		InkCanvasSelectionEditingEventArgs inkCanvasSelectionEditingEventArgs = new InkCanvasSelectionEditingEventArgs(selectionBounds, finalRectangle);
		_inkCanvas.RaiseSelectionMoving(inkCanvasSelectionEditingEventArgs);
		if (!inkCanvasSelectionEditingEventArgs.Cancel)
		{
			if (finalRectangle != inkCanvasSelectionEditingEventArgs.NewRectangle)
			{
				finalRectangle = inkCanvasSelectionEditingEventArgs.NewRectangle;
			}
			MoveSelection(selectionBounds, finalRectangle);
			_inkCanvas.RaiseSelectionMoved(EventArgs.Empty);
		}
	}

	private void CommitResizeChange(Rect finalRectangle)
	{
		Rect selectionBounds = SelectionBounds;
		InkCanvasSelectionEditingEventArgs inkCanvasSelectionEditingEventArgs = new InkCanvasSelectionEditingEventArgs(selectionBounds, finalRectangle);
		_inkCanvas.RaiseSelectionResizing(inkCanvasSelectionEditingEventArgs);
		if (!inkCanvasSelectionEditingEventArgs.Cancel)
		{
			if (finalRectangle != inkCanvasSelectionEditingEventArgs.NewRectangle)
			{
				finalRectangle = inkCanvasSelectionEditingEventArgs.NewRectangle;
			}
			MoveSelection(selectionBounds, finalRectangle);
			_inkCanvas.RaiseSelectionResized(EventArgs.Empty);
		}
	}

	private void MoveSelection(Rect previousRect, Rect newRect)
	{
		Matrix matrix = MapRectToRect(newRect, previousRect);
		int count = SelectedElements.Count;
		IList<UIElement> selectedElements = SelectedElements;
		for (int i = 0; i < count; i++)
		{
			UpdateElementBounds(selectedElements[i], matrix);
		}
		if (SelectedStrokes.Count > 0)
		{
			TransformStrokes(SelectedStrokes, matrix);
			_areStrokesChanged = true;
		}
		if (SelectedElements.Count == 0)
		{
			UpdateSelectionAdorner();
		}
		_inkCanvas.BringIntoView(newRect);
	}

	private void OnCanvasLayoutUpdated(object sender, EventArgs e)
	{
		UpdateSelectionAdorner();
	}

	private void OnStrokeInvalidated(object sender, EventArgs e)
	{
		OnStrokeCollectionChanged(sender, new StrokeCollectionChangedEventArgs(new StrokeCollection(), new StrokeCollection()));
	}

	private void OnStrokeCollectionChanged(object target, StrokeCollectionChangedEventArgs e)
	{
		if (e.Added.Count != 0 && e.Removed.Count == 0)
		{
			return;
		}
		foreach (Stroke item in e.Removed)
		{
			if (SelectedStrokes.Contains(item))
			{
				item.Invalidated -= OnStrokeInvalidated;
				item.IsSelected = false;
				SelectedStrokes.Remove(item);
			}
		}
		_areStrokesChanged = true;
		UpdateSelectionAdorner();
	}

	private Rect GetStrokesBounds()
	{
		if (_areStrokesChanged)
		{
			_cachedStrokesBounds = ((SelectedStrokes.Count != 0) ? SelectedStrokes.GetBounds() : Rect.Empty);
			_areStrokesChanged = false;
		}
		return _cachedStrokesBounds;
	}

	private List<Rect> GetElementsBounds()
	{
		List<Rect> list = new List<Rect>();
		if (SelectedElements.Count != 0)
		{
			foreach (Rect item in SelectedElementsBoundsEnumerator)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private Rect GetElementsUnionBounds()
	{
		if (SelectedElements.Count == 0)
		{
			return Rect.Empty;
		}
		Rect empty = Rect.Empty;
		foreach (Rect item in SelectedElementsBoundsEnumerator)
		{
			empty.Union(item);
		}
		return empty;
	}

	private void UpdateSelectionAdorner()
	{
		if (_inkCanvas.ActiveEditingMode != 0)
		{
			_inkCanvas.SelectionAdorner.UpdateSelectionWireFrame(GetStrokesBounds(), GetElementsBounds());
		}
	}

	private void EnusreElementsBounds()
	{
		InkCanvasInnerCanvas innerCanvas = _inkCanvas.InnerCanvas;
		if (!innerCanvas.IsMeasureValid || !innerCanvas.IsArrangeValid)
		{
			innerCanvas.UpdateLayout();
		}
	}

	private static Matrix MapRectToRect(Rect target, Rect source)
	{
		if (source.IsEmpty)
		{
			throw new ArgumentOutOfRangeException("source", SR.InvalidDiameter);
		}
		double num = target.Width / source.Width;
		double offsetX = target.Left - num * source.Left;
		double num2 = target.Height / source.Height;
		double offsetY = target.Top - num2 * source.Top;
		return new Matrix(num, 0.0, 0.0, num2, offsetX, offsetY);
	}

	private void UpdateCanvasLayoutUpdatedHandler()
	{
		if (SelectedElements.Count != 0)
		{
			if (_layoutUpdatedHandler == null)
			{
				_layoutUpdatedHandler = OnCanvasLayoutUpdated;
				_inkCanvas.InnerCanvas.LayoutUpdated += _layoutUpdatedHandler;
			}
		}
		else if (_layoutUpdatedHandler != null)
		{
			_inkCanvas.InnerCanvas.LayoutUpdated -= _layoutUpdatedHandler;
			_layoutUpdatedHandler = null;
		}
	}

	private static bool StrokesAreEqual(StrokeCollection strokes1, StrokeCollection strokes2)
	{
		if (strokes1 == null && strokes2 == null)
		{
			return true;
		}
		if (strokes1 == null || strokes2 == null)
		{
			return false;
		}
		if (strokes1.Count != strokes2.Count)
		{
			return false;
		}
		foreach (Stroke item in strokes1)
		{
			if (!strokes2.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	private static bool FrameworkElementArraysAreEqual(IList<UIElement> elements1, IList<UIElement> elements2)
	{
		if (elements1 == null && elements2 == null)
		{
			return true;
		}
		if (elements1 == null || elements2 == null)
		{
			return false;
		}
		if (elements1.Count != elements2.Count)
		{
			return false;
		}
		foreach (UIElement item in elements1)
		{
			if (!elements2.Contains(item))
			{
				return false;
			}
		}
		return true;
	}
}
