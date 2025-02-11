using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal.Controls;

namespace MS.Internal.Ink;

internal sealed class LassoSelectionBehavior : StylusEditingBehavior
{
	private struct ElementCornerPoints
	{
		internal Point UpperLeft;

		internal Point UpperRight;

		internal Point LowerRight;

		internal Point LowerLeft;

		internal bool Set;
	}

	private Point _startPoint;

	private bool _disableLasso;

	private LassoHelper _lassoHelper;

	private IncrementalLassoHitTester _incrementalLassoHitTester;

	private double _xDiff;

	private double _yDiff;

	private const double _maxThreshold = 50.0;

	private const double _minThreshold = 15.0;

	private const int _percentIntersectForInk = 80;

	private const int _percentIntersectForElements = 60;

	internal LassoSelectionBehavior(EditingCoordinator editingCoordinator, InkCanvas inkCanvas)
		: base(editingCoordinator, inkCanvas)
	{
	}

	protected override void OnSwitchToMode(InkCanvasEditingMode mode)
	{
		switch (mode)
		{
		case InkCanvasEditingMode.Ink:
		case InkCanvasEditingMode.GestureOnly:
		case InkCanvasEditingMode.InkAndGesture:
			Commit(commit: false);
			base.EditingCoordinator.ChangeStylusEditingMode(this, mode);
			break;
		case InkCanvasEditingMode.EraseByPoint:
		case InkCanvasEditingMode.EraseByStroke:
			Commit(commit: false);
			base.EditingCoordinator.ChangeStylusEditingMode(this, mode);
			break;
		case InkCanvasEditingMode.None:
			Commit(commit: false);
			base.EditingCoordinator.ChangeStylusEditingMode(this, mode);
			break;
		case InkCanvasEditingMode.Select:
			break;
		}
	}

	protected override void StylusInputBegin(StylusPointCollection stylusPoints, bool userInitiated)
	{
		_disableLasso = false;
		bool flag = false;
		List<Point> list = new List<Point>();
		for (int i = 0; i < stylusPoints.Count; i++)
		{
			Point point = (Point)stylusPoints[i];
			if (i == 0)
			{
				_startPoint = point;
				list.Add(point);
			}
			else if (!flag)
			{
				if (DoubleUtil.GreaterThan((point - _startPoint).LengthSquared, 49.0))
				{
					list.Add(point);
					flag = true;
				}
			}
			else
			{
				list.Add(point);
			}
		}
		if (flag)
		{
			StartLasso(list);
		}
	}

	protected override void StylusInputContinue(StylusPointCollection stylusPoints, bool userInitiated)
	{
		if (_lassoHelper != null)
		{
			List<Point> list = new List<Point>();
			for (int i = 0; i < stylusPoints.Count; i++)
			{
				list.Add((Point)stylusPoints[i]);
			}
			Point[] array = _lassoHelper.AddPoints(list);
			if (array.Length != 0)
			{
				_incrementalLassoHitTester.AddPoints(array);
			}
		}
		else
		{
			if (_disableLasso)
			{
				return;
			}
			bool flag = false;
			List<Point> list2 = new List<Point>();
			for (int j = 0; j < stylusPoints.Count; j++)
			{
				Point point = (Point)stylusPoints[j];
				if (!flag)
				{
					if (DoubleUtil.GreaterThan((point - _startPoint).LengthSquared, 49.0))
					{
						list2.Add(point);
						flag = true;
					}
				}
				else
				{
					list2.Add(point);
				}
			}
			if (flag)
			{
				StartLasso(list2);
			}
		}
	}

	protected override void StylusInputEnd(bool commit)
	{
		StrokeCollection strokeCollection = new StrokeCollection();
		List<UIElement> list = new List<UIElement>();
		if (_lassoHelper != null)
		{
			strokeCollection = base.InkCanvas.EndDynamicSelection(_lassoHelper.Visual);
			list = HitTestForElements();
			_incrementalLassoHitTester.SelectionChanged -= OnSelectionChanged;
			_incrementalLassoHitTester.EndHitTesting();
			_incrementalLassoHitTester = null;
			_lassoHelper = null;
		}
		else
		{
			TapSelectObject(_startPoint, out var tappedStroke, out var tappedElement);
			if (tappedStroke != null)
			{
				strokeCollection = new StrokeCollection();
				strokeCollection.Add(tappedStroke);
			}
			else if (tappedElement != null)
			{
				list.Add(tappedElement);
			}
		}
		SelfDeactivate();
		if (commit)
		{
			base.InkCanvas.ChangeInkCanvasSelection(strokeCollection, list.ToArray());
		}
	}

	protected override void OnCommitWithoutStylusInput(bool commit)
	{
		SelfDeactivate();
	}

	protected override Cursor GetCurrentCursor()
	{
		return Cursors.Cross;
	}

	private void OnSelectionChanged(object sender, LassoSelectionChangedEventArgs e)
	{
		base.InkCanvas.UpdateDynamicSelection(e.SelectedStrokes, e.DeselectedStrokes);
	}

	private List<UIElement> HitTestForElements()
	{
		List<UIElement> list = new List<UIElement>();
		if (base.InkCanvas.Children.Count == 0)
		{
			return list;
		}
		for (int i = 0; i < base.InkCanvas.Children.Count; i++)
		{
			UIElement uiElement = base.InkCanvas.Children[i];
			HitTestElement(base.InkCanvas.InnerCanvas, uiElement, list);
		}
		return list;
	}

	private void HitTestElement(InkCanvasInnerCanvas parent, UIElement uiElement, List<UIElement> elementsToSelect)
	{
		ElementCornerPoints transformedElementCornerPoints = GetTransformedElementCornerPoints(parent, uiElement);
		if (transformedElementCornerPoints.Set)
		{
			Point[] points = GeneratePointGrid(transformedElementCornerPoints);
			if (_lassoHelper.ArePointsInLasso(points, 60))
			{
				elementsToSelect.Add(uiElement);
			}
		}
	}

	private static ElementCornerPoints GetTransformedElementCornerPoints(InkCanvasInnerCanvas canvas, UIElement childElement)
	{
		ElementCornerPoints result = default(ElementCornerPoints);
		result.Set = false;
		if (childElement.Visibility != 0)
		{
			return result;
		}
		GeneralTransform generalTransform = childElement.TransformToAncestor(canvas);
		generalTransform.TryTransform(new Point(0.0, 0.0), out result.UpperLeft);
		generalTransform.TryTransform(new Point(childElement.RenderSize.Width, 0.0), out result.UpperRight);
		generalTransform.TryTransform(new Point(0.0, childElement.RenderSize.Height), out result.LowerLeft);
		generalTransform.TryTransform(new Point(childElement.RenderSize.Width, childElement.RenderSize.Height), out result.LowerRight);
		result.Set = true;
		return result;
	}

	private Point[] GeneratePointGrid(ElementCornerPoints elementPoints)
	{
		if (!elementPoints.Set)
		{
			return new Point[0];
		}
		ArrayList arrayList = new ArrayList();
		UpdatePointDistances(elementPoints);
		arrayList.Add(elementPoints.UpperLeft);
		arrayList.Add(elementPoints.UpperRight);
		FillInPoints(arrayList, elementPoints.UpperLeft, elementPoints.UpperRight);
		arrayList.Add(elementPoints.LowerLeft);
		arrayList.Add(elementPoints.LowerRight);
		FillInPoints(arrayList, elementPoints.LowerLeft, elementPoints.LowerRight);
		FillInGrid(arrayList, elementPoints.UpperLeft, elementPoints.UpperRight, elementPoints.LowerRight, elementPoints.LowerLeft);
		Point[] array = new Point[arrayList.Count];
		arrayList.CopyTo(array);
		return array;
	}

	private void FillInPoints(ArrayList pointArray, Point point1, Point point2)
	{
		if (!PointsAreCloseEnough(point1, point2))
		{
			Point point3 = GeneratePointBetweenPoints(point1, point2);
			pointArray.Add(point3);
			if (!PointsAreCloseEnough(point1, point3))
			{
				FillInPoints(pointArray, point1, point3);
			}
			if (!PointsAreCloseEnough(point3, point2))
			{
				FillInPoints(pointArray, point3, point2);
			}
		}
	}

	private void FillInGrid(ArrayList pointArray, Point upperLeft, Point upperRight, Point lowerRight, Point lowerLeft)
	{
		if (!PointsAreCloseEnough(upperLeft, lowerLeft))
		{
			Point point = GeneratePointBetweenPoints(upperLeft, lowerLeft);
			Point point2 = GeneratePointBetweenPoints(upperRight, lowerRight);
			pointArray.Add(point);
			pointArray.Add(point2);
			FillInPoints(pointArray, point, point2);
			if (!PointsAreCloseEnough(upperLeft, point))
			{
				FillInGrid(pointArray, upperLeft, upperRight, point2, point);
			}
			if (!PointsAreCloseEnough(point, lowerLeft))
			{
				FillInGrid(pointArray, point, point2, lowerRight, lowerLeft);
			}
		}
	}

	private static Point GeneratePointBetweenPoints(Point point1, Point point2)
	{
		double num = ((point1.X > point2.X) ? point1.X : point2.X);
		double num2 = ((point1.X < point2.X) ? point1.X : point2.X);
		double num3 = ((point1.Y > point2.Y) ? point1.Y : point2.Y);
		double num4 = ((point1.Y < point2.Y) ? point1.Y : point2.Y);
		return new Point(num2 + (num - num2) * 0.5, num4 + (num3 - num4) * 0.5);
	}

	private bool PointsAreCloseEnough(Point point1, Point point2)
	{
		double num = point1.X - point2.X;
		double num2 = point1.Y - point2.Y;
		if (num < _xDiff && num > 0.0 - _xDiff && num2 < _yDiff && num2 > 0.0 - _yDiff)
		{
			return true;
		}
		return false;
	}

	private void UpdatePointDistances(ElementCornerPoints elementPoints)
	{
		double num = elementPoints.UpperLeft.X - elementPoints.UpperRight.X;
		if (num < 0.0)
		{
			num = 0.0 - num;
		}
		double num2 = elementPoints.UpperLeft.Y - elementPoints.LowerLeft.Y;
		if (num2 < 0.0)
		{
			num2 = 0.0 - num2;
		}
		_xDiff = num * 0.25;
		if (_xDiff > 50.0)
		{
			_xDiff = 50.0;
		}
		else if (_xDiff < 15.0)
		{
			_xDiff = 15.0;
		}
		_yDiff = num2 * 0.25;
		if (_yDiff > 50.0)
		{
			_yDiff = 50.0;
		}
		else if (_yDiff < 15.0)
		{
			_yDiff = 15.0;
		}
	}

	private void StartLasso(List<Point> points)
	{
		if (base.InkCanvas.ClearSelectionRaiseSelectionChanging() && base.EditingCoordinator.ActiveEditingMode == InkCanvasEditingMode.Select)
		{
			_incrementalLassoHitTester = base.InkCanvas.Strokes.GetIncrementalLassoHitTester(80);
			_incrementalLassoHitTester.SelectionChanged += OnSelectionChanged;
			_lassoHelper = new LassoHelper();
			base.InkCanvas.BeginDynamicSelection(_lassoHelper.Visual);
			Point[] array = _lassoHelper.AddPoints(points);
			if (array.Length != 0)
			{
				_incrementalLassoHitTester.AddPoints(array);
			}
		}
		else
		{
			_disableLasso = true;
		}
	}

	private void TapSelectObject(Point point, out Stroke tappedStroke, out UIElement tappedElement)
	{
		tappedStroke = null;
		tappedElement = null;
		StrokeCollection strokeCollection = base.InkCanvas.Strokes.HitTest(point, 5.0);
		if (strokeCollection.Count > 0)
		{
			tappedStroke = strokeCollection[strokeCollection.Count - 1];
			return;
		}
		Point point2 = base.InkCanvas.TransformToVisual(base.InkCanvas.InnerCanvas).Transform(point);
		tappedElement = base.InkCanvas.InnerCanvas.HitTestOnElements(point2);
	}
}
