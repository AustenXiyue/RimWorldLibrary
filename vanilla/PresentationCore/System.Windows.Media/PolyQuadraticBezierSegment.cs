using System.Collections.Generic;
using System.Windows.Media.Animation;
using MS.Internal;

namespace System.Windows.Media;

/// <summary> Represents a set of quadratic Bezier segments. </summary>
public sealed class PolyQuadraticBezierSegment : PathSegment
{
	/// <summary> Identifies the <see cref="P:System.Windows.Media.PolyQuadraticBezierSegment.Points" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.PolyQuadraticBezierSegment.Points" /> dependency property.</returns>
	public static readonly DependencyProperty PointsProperty;

	internal static PointCollection s_Points;

	/// <summary> Gets or sets the <see cref="T:System.Windows.Media.PointCollection" /> that defines this <see cref="T:System.Windows.Media.PolyQuadraticBezierSegment" /> object.  </summary>
	/// <returns>A collection that defines the shape of this <see cref="T:System.Windows.Media.PolyQuadraticBezierSegment" /> object. The default value is an empty collection.</returns>
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
		if (Points == null)
		{
			return "";
		}
		return "Q" + Points.ConvertToString(format, provider);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PolyQuadraticBezierSegment" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PolyQuadraticBezierSegment Clone()
	{
		return (PolyQuadraticBezierSegment)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PolyQuadraticBezierSegment" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PolyQuadraticBezierSegment CloneCurrentValue()
	{
		return (PolyQuadraticBezierSegment)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new PolyQuadraticBezierSegment();
	}

	static PolyQuadraticBezierSegment()
	{
		s_Points = PointCollection.Empty;
		Type typeFromHandle = typeof(PolyQuadraticBezierSegment);
		PointsProperty = Animatable.RegisterProperty("Points", typeof(PointCollection), typeFromHandle, new FreezableDefaultValueFactory(PointCollection.Empty), null, null, isIndependentlyAnimated: false, null);
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.PolyQuadraticBezierSegment" /> class. </summary>
	public PolyQuadraticBezierSegment()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PolyQuadraticBezierSegment" /> class with the specified collection of <see cref="T:System.Windows.Point" /> objects and a value specifying whether the segments are stroked.</summary>
	/// <param name="points">The collection of points that specify the geometry of the Bezier curve segments.</param>
	/// <param name="isStroked">true to stroke the segments; otherwise, false.</param>
	public PolyQuadraticBezierSegment(IEnumerable<Point> points, bool isStroked)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		Points = new PointCollection(points);
		base.IsStroked = isStroked;
	}

	internal PolyQuadraticBezierSegment(IEnumerable<Point> points, bool isStroked, bool isSmoothJoin)
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
		if (points == null || points.Count < 2)
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
			figure.Segments.Add(new PolyQuadraticBezierSegment(pointCollection, base.IsStroked, base.IsSmoothJoin));
		}
		current = points.Internal_GetItem(points.Count - 1);
	}

	internal override bool IsEmpty()
	{
		if (Points != null)
		{
			return Points.Count < 2;
		}
		return true;
	}

	internal override bool IsCurved()
	{
		return !IsEmpty();
	}

	internal override void SerializeData(StreamGeometryContext ctx)
	{
		ctx.PolyQuadraticBezierTo(Points, base.IsStroked, base.IsSmoothJoin);
	}
}
