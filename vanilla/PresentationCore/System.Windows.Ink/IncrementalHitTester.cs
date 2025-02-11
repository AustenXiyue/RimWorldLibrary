using System.Collections.Generic;
using System.Windows.Input;
using MS.Internal.Ink;
using MS.Internal.PresentationCore;

namespace System.Windows.Ink;

/// <summary>Dynamically performs hit testing on a <see cref="T:System.Windows.Ink.Stroke" />.</summary>
public abstract class IncrementalHitTester
{
	private StrokeCollection _strokes;

	private List<StrokeInfo> _strokeInfos;

	private bool _fValid = true;

	/// <summary>Gets whether the <see cref="T:System.Windows.Ink.IncrementalHitTester" /> is hit testing.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Ink.IncrementalHitTester" /> is hit testing; otherwise, false. </returns>
	public bool IsValid => _fValid;

	internal List<StrokeInfo> StrokeInfos => _strokeInfos;

	/// <summary>Adds a <see cref="T:System.Windows.Point" /> to the <see cref="T:System.Windows.Ink.IncrementalHitTester" />.</summary>
	/// <param name="point">The <see cref="T:System.Windows.Point" /> to add to the <see cref="T:System.Windows.Ink.IncrementalHitTester" />.</param>
	public void AddPoint(Point point)
	{
		AddPoints(new Point[1] { point });
	}

	/// <summary>Adds points to the <see cref="T:System.Windows.Ink.IncrementalHitTester" />.</summary>
	/// <param name="points">An array of type <see cref="T:System.Windows.Point" /> to add to the <see cref="T:System.Windows.Ink.IncrementalHitTester" />.</param>
	public void AddPoints(IEnumerable<Point> points)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		if (IEnumerablePointHelper.GetCount(points) == 0)
		{
			throw new ArgumentException(SR.EmptyArrayNotAllowedAsArgument, "points");
		}
		if (!_fValid)
		{
			throw new InvalidOperationException(SR.EndHitTestingCalled);
		}
		AddPointsCore(points);
	}

	/// <summary>Adds the specified <see cref="T:System.Windows.Input.StylusPoint" /> objects to the <see cref="T:System.Windows.Ink.IncrementalHitTester" />.</summary>
	/// <param name="stylusPoints">A collection of <see cref="T:System.Windows.Input.StylusPoint" /> objects to add to the <see cref="T:System.Windows.Ink.IncrementalHitTester" />.</param>
	public void AddPoints(StylusPointCollection stylusPoints)
	{
		if (stylusPoints == null)
		{
			throw new ArgumentNullException("stylusPoints");
		}
		if (stylusPoints.Count == 0)
		{
			throw new ArgumentException(SR.EmptyArrayNotAllowedAsArgument, "stylusPoints");
		}
		if (!_fValid)
		{
			throw new InvalidOperationException(SR.EndHitTestingCalled);
		}
		Point[] array = new Point[stylusPoints.Count];
		for (int i = 0; i < stylusPoints.Count; i++)
		{
			array[i] = (Point)stylusPoints[i];
		}
		AddPointsCore(array);
	}

	/// <summary>Releases resources used by the <see cref="T:System.Windows.Ink.IncrementalHitTester" />. </summary>
	public void EndHitTesting()
	{
		if (_strokes != null)
		{
			_strokes.StrokesChangedInternal -= OnStrokesChanged;
			_strokes = null;
			int count = _strokeInfos.Count;
			for (int i = 0; i < count; i++)
			{
				_strokeInfos[i].Detach();
			}
			_strokeInfos = null;
		}
		_fValid = false;
	}

	internal IncrementalHitTester(StrokeCollection strokes)
	{
		_strokeInfos = new List<StrokeInfo>(strokes.Count);
		for (int i = 0; i < strokes.Count; i++)
		{
			Stroke stroke = strokes[i];
			_strokeInfos.Add(new StrokeInfo(stroke));
		}
		_strokes = strokes;
		_strokes.StrokesChangedInternal += OnStrokesChanged;
	}

	/// <summary>Adds points to the <see cref="T:System.Windows.Ink.IncrementalHitTester" />.</summary>
	/// <param name="points">The points to add</param>
	protected abstract void AddPointsCore(IEnumerable<Point> points);

	private void OnStrokesChanged(object sender, StrokeCollectionChangedEventArgs args)
	{
		StrokeCollection added = args.Added;
		StrokeCollection removed = args.Removed;
		if (added.Count > 0)
		{
			int num = _strokes.IndexOf(added[0]);
			for (int i = 0; i < added.Count; i++)
			{
				_strokeInfos.Insert(num, new StrokeInfo(added[i]));
				num++;
			}
		}
		if (removed.Count > 0)
		{
			StrokeCollection strokeCollection = new StrokeCollection(removed);
			int num2 = 0;
			while (num2 < _strokeInfos.Count && strokeCollection.Count > 0)
			{
				bool flag = false;
				for (int j = 0; j < strokeCollection.Count; j++)
				{
					if (strokeCollection[j] == _strokeInfos[num2].Stroke)
					{
						_strokeInfos.RemoveAt(num2);
						strokeCollection.RemoveAt(j);
						flag = true;
					}
				}
				if (!flag)
				{
					num2++;
				}
			}
		}
		if (_strokes.Count != _strokeInfos.Count)
		{
			RebuildStrokeInfoCache();
			return;
		}
		for (int k = 0; k < _strokeInfos.Count; k++)
		{
			if (_strokeInfos[k].Stroke != _strokes[k])
			{
				RebuildStrokeInfoCache();
				break;
			}
		}
	}

	private void RebuildStrokeInfoCache()
	{
		List<StrokeInfo> list = new List<StrokeInfo>(_strokes.Count);
		foreach (Stroke stroke in _strokes)
		{
			bool flag = false;
			for (int i = 0; i < _strokeInfos.Count; i++)
			{
				StrokeInfo strokeInfo = _strokeInfos[i];
				if (strokeInfo != null && stroke == strokeInfo.Stroke)
				{
					list.Add(strokeInfo);
					_strokeInfos[i] = null;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(new StrokeInfo(stroke));
			}
		}
		for (int j = 0; j < _strokeInfos.Count; j++)
		{
			_strokeInfos[j]?.Detach();
		}
		_strokeInfos = list;
	}
}
