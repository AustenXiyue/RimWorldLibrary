using System.Windows.Media.Animation;

namespace System.Windows.Media;

/// <summary>Creates a line between two points in a <see cref="T:System.Windows.Media.PathFigure" />.  </summary>
public sealed class LineSegment : PathSegment
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.LineSegment.Point" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.LineSegment.Point" /> dependency property.</returns>
	public static readonly DependencyProperty PointProperty;

	internal static Point s_Point;

	/// <summary>Gets or sets the end point of the line segment.  </summary>
	/// <returns>The end point of the line segment.</returns>
	public Point Point
	{
		get
		{
			return (Point)GetValue(PointProperty);
		}
		set
		{
			SetValueInternal(PointProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LineSegment" /> class. </summary>
	public LineSegment()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LineSegment" /> class that has the specified end <see cref="T:System.Windows.Point" /> and Boolean that determines whether this <see cref="T:System.Windows.Media.LineSegment" /> is stroked. </summary>
	/// <param name="point">The end point of this <see cref="T:System.Windows.Media.LineSegment" />.</param>
	/// <param name="isStroked">true to stroke this <see cref="T:System.Windows.Media.LineSegment" />; otherwise, false.</param>
	public LineSegment(Point point, bool isStroked)
	{
		Point = point;
		base.IsStroked = isStroked;
	}

	internal LineSegment(Point point, bool isStroked, bool isSmoothJoin)
	{
		Point = point;
		base.IsStroked = isStroked;
		base.IsSmoothJoin = isSmoothJoin;
	}

	internal override void AddToFigure(Matrix matrix, PathFigure figure, ref Point current)
	{
		current = Point;
		if (matrix.IsIdentity)
		{
			figure.Segments.Add(this);
			return;
		}
		Point point = current;
		point *= matrix;
		figure.Segments.Add(new LineSegment(point, base.IsStroked, base.IsSmoothJoin));
	}

	internal override void SerializeData(StreamGeometryContext ctx)
	{
		ctx.LineTo(Point, base.IsStroked, base.IsSmoothJoin);
	}

	internal override bool IsCurved()
	{
		return false;
	}

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		return "L" + ((IFormattable)Point).ToString(format, provider);
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.LineSegment" /> by making deep copies of its values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new LineSegment Clone()
	{
		return (LineSegment)base.Clone();
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.LineSegment" /> object by making deep copies of its values. This method does not copy resource references, data bindings, or animations, although it does copy their current values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new LineSegment CloneCurrentValue()
	{
		return (LineSegment)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new LineSegment();
	}

	static LineSegment()
	{
		Type typeFromHandle = typeof(LineSegment);
		PointProperty = Animatable.RegisterProperty("Point", typeof(Point), typeFromHandle, default(Point), null, null, isIndependentlyAnimated: false, null);
	}
}
