using System.Windows.Media.Animation;
using MS.Internal;

namespace System.Windows.Media;

/// <summary>Creates a quadratic Bezier curve between two points in a <see cref="T:System.Windows.Media.PathFigure" />. </summary>
public sealed class QuadraticBezierSegment : PathSegment
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.QuadraticBezierSegment.Point1" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.QuadraticBezierSegment.Point1" /> dependency property.</returns>
	public static readonly DependencyProperty Point1Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.QuadraticBezierSegment.Point2" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.QuadraticBezierSegment.Point2" /> dependency property.</returns>
	public static readonly DependencyProperty Point2Property;

	internal static Point s_Point1;

	internal static Point s_Point2;

	/// <summary>Gets or sets the control <see cref="T:System.Windows.Point" /> of the curve.  </summary>
	/// <returns>The control point of this <see cref="T:System.Windows.Media.QuadraticBezierSegment" />.</returns>
	public Point Point1
	{
		get
		{
			return (Point)GetValue(Point1Property);
		}
		set
		{
			SetValueInternal(Point1Property, value);
		}
	}

	/// <summary>Gets or sets the end <see cref="T:System.Windows.Point" /> of this <see cref="T:System.Windows.Media.QuadraticBezierSegment" />.  </summary>
	/// <returns>The end point of this <see cref="T:System.Windows.Media.QuadraticBezierSegment" />.</returns>
	public Point Point2
	{
		get
		{
			return (Point)GetValue(Point2Property);
		}
		set
		{
			SetValueInternal(Point2Property, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.QuadraticBezierSegment" /> class. </summary>
	public QuadraticBezierSegment()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.QuadraticBezierSegment" /> class with the specified control point, end point, and Boolean indicating whether to stroke this <see cref="T:System.Windows.Media.QuadraticBezierSegment" />.  </summary>
	/// <param name="point1">The control point of this <see cref="T:System.Windows.Media.QuadraticBezierSegment" />.</param>
	/// <param name="point2">The end point of this <see cref="T:System.Windows.Media.QuadraticBezierSegment" />.</param>
	/// <param name="isStroked">true if this <see cref="T:System.Windows.Media.QuadraticBezierSegment" /> is to be stroked; otherwise, false.</param>
	public QuadraticBezierSegment(Point point1, Point point2, bool isStroked)
	{
		Point1 = point1;
		Point2 = point2;
		base.IsStroked = isStroked;
	}

	internal QuadraticBezierSegment(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
	{
		Point1 = point1;
		Point2 = point2;
		base.IsStroked = isStroked;
		base.IsSmoothJoin = isSmoothJoin;
	}

	internal override void AddToFigure(Matrix matrix, PathFigure figure, ref Point current)
	{
		current = Point2;
		if (matrix.IsIdentity)
		{
			figure.Segments.Add(this);
			return;
		}
		Point point = Point1;
		point *= matrix;
		Point point2 = current;
		point2 *= matrix;
		figure.Segments.Add(new QuadraticBezierSegment(point, point2, base.IsStroked, base.IsSmoothJoin));
	}

	internal override void SerializeData(StreamGeometryContext ctx)
	{
		ctx.QuadraticBezierTo(Point1, Point2, base.IsStroked, base.IsSmoothJoin);
	}

	internal override bool IsCurved()
	{
		return true;
	}

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
		return string.Format(provider, "Q{1:" + format + "}{0}{2:" + format + "}", numericListSeparator, Point1, Point2);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.QuadraticBezierSegment" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new QuadraticBezierSegment Clone()
	{
		return (QuadraticBezierSegment)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.QuadraticBezierSegment" /> object, making deep copies of this object's current values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new QuadraticBezierSegment CloneCurrentValue()
	{
		return (QuadraticBezierSegment)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new QuadraticBezierSegment();
	}

	static QuadraticBezierSegment()
	{
		Type typeFromHandle = typeof(QuadraticBezierSegment);
		Point1Property = Animatable.RegisterProperty("Point1", typeof(Point), typeFromHandle, default(Point), null, null, isIndependentlyAnimated: false, null);
		Point2Property = Animatable.RegisterProperty("Point2", typeof(Point), typeFromHandle, default(Point), null, null, isIndependentlyAnimated: false, null);
	}
}
