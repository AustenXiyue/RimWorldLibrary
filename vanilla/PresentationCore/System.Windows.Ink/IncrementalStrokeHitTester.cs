using System.Collections.Generic;
using MS.Internal.Ink;

namespace System.Windows.Ink;

/// <summary>Dynamically hit tests a stroke with an eraser path.</summary>
public class IncrementalStrokeHitTester : IncrementalHitTester
{
	private ErasingStroke _erasingStroke;

	/// <summary>Occurs when the <see cref="T:System.Windows.Ink.IncrementalStrokeHitTester" /> intersects an ink <see cref="T:System.Windows.Ink.Stroke" />.</summary>
	public event StrokeHitEventHandler StrokeHit;

	internal IncrementalStrokeHitTester(StrokeCollection strokes, StylusShape eraserShape)
		: base(strokes)
	{
		_erasingStroke = new ErasingStroke(eraserShape);
	}

	/// <param name="points">The points.</param>
	protected override void AddPointsCore(IEnumerable<Point> points)
	{
		_erasingStroke.MoveTo(points);
		Rect bounds = _erasingStroke.Bounds;
		if (bounds.IsEmpty)
		{
			return;
		}
		List<StrokeHitEventArgs> list = null;
		if (this.StrokeHit != null)
		{
			List<StrokeIntersection> list2 = new List<StrokeIntersection>();
			for (int i = 0; i < base.StrokeInfos.Count; i++)
			{
				StrokeInfo strokeInfo = base.StrokeInfos[i];
				if (bounds.IntersectsWith(strokeInfo.StrokeBounds) && _erasingStroke.EraseTest(StrokeNodeIterator.GetIterator(strokeInfo.Stroke, strokeInfo.Stroke.DrawingAttributes), list2))
				{
					if (list == null)
					{
						list = new List<StrokeHitEventArgs>();
					}
					list.Add(new StrokeHitEventArgs(strokeInfo.Stroke, list2.ToArray()));
					list2.Clear();
				}
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				StrokeHitEventArgs eventArgs = list[j];
				OnStrokeHit(eventArgs);
			}
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Ink.IncrementalStrokeHitTester.StrokeHit" /> event.</summary>
	/// <param name="eventArgs">Event data.</param>
	protected void OnStrokeHit(StrokeHitEventArgs eventArgs)
	{
		if (this.StrokeHit != null)
		{
			this.StrokeHit(this, eventArgs);
		}
	}
}
