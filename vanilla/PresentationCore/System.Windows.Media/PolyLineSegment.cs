using System.Collections.Generic;
using System.Windows.Media.Animation;
using MS.Internal;

namespace System.Windows.Media;

/// <summary> Represents a set of line segments defined by a <see cref="T:System.Windows.Media.PointCollection" /> with each <see cref="T:System.Windows.Point" /> specifying the end point of a line segment. </summary>
public sealed class PolyLineSegment : PathSegment
{
	/// <summary> Identifies the <see cref="P:System.Windows.Media.PolyLineSegment.Points" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.PolyLineSegment.Points" /> dependency property.</returns>
	public static readonly DependencyProperty PointsProperty;

	internal static PointCollection s_Points;

	/// <summary> Gets or sets the collection of <see cref="T:System.Windows.Point" /> structures that defines this <see cref="T:System.Windows.Media.PolyLineSegment" /> object.  </summary>
	/// <returns>The shape of this <see cref="T:System.Windows.Media.PolyLineSegment" /> object.</returns>
	public PointCollection Points
	{
		get
		{
			return (PointCollection)GetValue(PointsProperty);
		}
		set
		{
			SetValueInternal(PointsProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		if (IsEmpty())
		{
			return "";
		}
		return "L" + Points.ConvertToString(format, provider);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PolyLineSegment" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PolyLineSegment Clone()
	{
		return (PolyLineSegment)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PolyLineSegment" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PolyLineSegment CloneCurrentValue()
	{
		return (PolyLineSegment)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new PolyLineSegment();
	}

	static PolyLineSegment()
	{
		s_Points = PointCollection.Empty;
		Type typeFromHandle = typeof(PolyLineSegment);
		PointsProperty = Animatable.RegisterProperty("Points", typeof(PointCollection), typeFromHandle, new FreezableDefaultValueFactory(PointCollection.Empty), null, null, isIndependentlyAnimated: false, null);
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.PolyLineSegment" /> class. </summary>
	public PolyLineSegment()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PolyLineSegment" /> class with the specified list of points that determine the line segments and a value indicating whether the segments are stroked.</summary>
	/// <param name="points">A collection of points that determine the line segments of the <see cref="T:System.Windows.Media.PolyLineSegment" />.</param>
	/// <param name="isStroked">true to make the segment stroked; otherwise, false..</param>
	public PolyLineSegment(IEnumerable<Point> points, bool isStroked)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		Points = new PointCollection(points);
		base.IsStroked = isStroked;
	}

	internal PolyLineSegment(IEnumerable<Point> points, bool isStroked, bool isSmoothJoin)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		Points = new PointCollection(points);
		base.IsStroked = isStroked;
		base.IsSmoothJoin = isSmoothJoin;
	}

	internal override void AddToFigure(Matrix matrix, PathFigure figure, ref Point current)
	{
		PointCollection points = Points;
		if (points == null || points.Count < 1)
		{
			return;
		}
		if (matrix.IsIdentity)
		{
			figure.Segments.Add(this);
		}
		else
		{
			PointCollection pointCollection = new PointCollection();
			Point point = default(Point);
			int count = points.Count;
			for (int i = 0; i < count; i++)
			{
				point = points.Internal_GetItem(i);
				point *= matrix;
				pointCollection.Add(point);
			}
			figure.Segments.Add(new PolyLineSegment(pointCollection, base.IsStroked, base.IsSmoothJoin));
		}
		current = points.Internal_GetItem(points.Count - 1);
	}

	internal override bool IsEmpty()
	{
		if (Points != null)
		{
			return Points.Count < 1;
		}
		return true;
	}

	internal override bool IsCurved()
	{
		return false;
	}

	internal override void SerializeData(StreamGeometryContext ctx)
	{
		ctx.PolyLineTo(Points, base.IsStroked, base.IsSmoothJoin);
	}
}
