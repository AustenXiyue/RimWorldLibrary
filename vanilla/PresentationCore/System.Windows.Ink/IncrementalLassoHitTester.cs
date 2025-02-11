using System.Collections.Generic;
using System.Windows.Input;
using MS.Internal;
using MS.Internal.Ink;

namespace System.Windows.Ink;

/// <summary>Dynamically hit tests a <see cref="T:System.Windows.Ink.Stroke" /> with a lasso.</summary>
public class IncrementalLassoHitTester : IncrementalHitTester
{
	private Lasso _lasso;

	private int _percentIntersect;

	/// <summary>Occurs when the lasso path selects or unselects an ink <see cref="T:System.Windows.Ink.Stroke" />. </summary>
	public event LassoSelectionChangedEventHandler SelectionChanged;

	internal IncrementalLassoHitTester(StrokeCollection strokes, int percentageWithinLasso)
		: base(strokes)
	{
		_lasso = new SingleLoopLasso();
		_percentIntersect = percentageWithinLasso;
	}

	/// <summary>Adds points to the <see cref="T:System.Windows.Ink.IncrementalHitTester" />.</summary>
	/// <param name="points">The points to add</param>
	protected override void AddPointsCore(IEnumerable<Point> points)
	{
		int i = ((_lasso.PointCount != 0) ? (_lasso.PointCount - 1) : 0);
		_lasso.AddPoints(points);
		if (_lasso.IsEmpty || (i == _lasso.PointCount - 1 && !_lasso.IsIncrementalLassoDirty) || this.SelectionChanged == null)
		{
			return;
		}
		StrokeCollection strokeCollection = null;
		StrokeCollection strokeCollection2 = null;
		Lasso lasso = new Lasso();
		if (!_lasso.IsIncrementalLassoDirty)
		{
			if (0 < i)
			{
				lasso.AddPoint(_lasso[0]);
			}
			for (; i < _lasso.PointCount; i++)
			{
				lasso.AddPoint(_lasso[i]);
			}
		}
		foreach (StrokeInfo strokeInfo in base.StrokeInfos)
		{
			Lasso lasso2;
			if (strokeInfo.IsDirty || _lasso.IsIncrementalLassoDirty)
			{
				lasso2 = _lasso;
				strokeInfo.IsDirty = false;
			}
			else
			{
				lasso2 = lasso;
			}
			double num = 0.0;
			if (lasso2.Bounds.IntersectsWith(strokeInfo.StrokeBounds))
			{
				StylusPointCollection stylusPoints = strokeInfo.StylusPoints;
				for (int j = 0; j < stylusPoints.Count; j++)
				{
					if (lasso2.Contains((Point)stylusPoints[j]))
					{
						double pointWeight = strokeInfo.GetPointWeight(j);
						num = ((lasso2 != _lasso && !_lasso.Contains((Point)stylusPoints[j])) ? (num - pointWeight) : (num + pointWeight));
					}
				}
			}
			if (num == 0.0 && lasso2 != _lasso)
			{
				continue;
			}
			strokeInfo.HitWeight = ((lasso2 == _lasso) ? num : (strokeInfo.HitWeight + num));
			bool flag = DoubleUtil.GreaterThanOrClose(strokeInfo.HitWeight, strokeInfo.TotalWeight * (double)_percentIntersect / 100.0 - 0.0001);
			if (strokeInfo.IsHit == flag)
			{
				continue;
			}
			strokeInfo.IsHit = flag;
			if (flag)
			{
				if (strokeCollection == null)
				{
					strokeCollection = new StrokeCollection();
				}
				strokeCollection.Add(strokeInfo.Stroke);
			}
			else
			{
				if (strokeCollection2 == null)
				{
					strokeCollection2 = new StrokeCollection();
				}
				strokeCollection2.Add(strokeInfo.Stroke);
			}
		}
		_lasso.IsIncrementalLassoDirty = false;
		if (strokeCollection != null || strokeCollection2 != null)
		{
			OnSelectionChanged(new LassoSelectionChangedEventArgs(strokeCollection, strokeCollection2));
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Ink.IncrementalLassoHitTester.SelectionChanged" /> event. </summary>
	/// <param name="eventArgs">Event data.</param>
	protected void OnSelectionChanged(LassoSelectionChangedEventArgs eventArgs)
	{
		if (this.SelectionChanged != null)
		{
			this.SelectionChanged(this, eventArgs);
		}
	}
}
