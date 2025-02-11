using System.Windows.Media.Animation;
using MS.Internal;

namespace System.Windows.Media;

/// <summary>Represents a cubic Bezier curve drawn between two points. </summary>
public sealed class BezierSegment : PathSegment
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.BezierSegment.Point1" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.BezierSegment.Point1" /> dependency property identifier.</returns>
	public static readonly DependencyProperty Point1Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.BezierSegment.Point2" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.BezierSegment.Point2" /> dependency property identifier.</returns>
	public static readonly DependencyProperty Point2Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.BezierSegment.Point3" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.BezierSegment.Point3" /> dependency property identifier.</returns>
	public static readonly DependencyProperty Point3Property;

	internal static Point s_Point1;

	internal static Point s_Point2;

	internal static Point s_Point3;

	/// <summary>Gets or sets the first control point of the curve.  </summary>
	/// <returns>The first control point of the curve.</returns>
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

	/// <summary>Gets or sets the second control point of the curve.  </summary>
	/// <returns>The second control point of the curve.</returns>
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

	/// <summary>Gets or sets the end point of the curve.  </summary>
	/// <returns>The end point of the curve.</returns>
	public Point Point3
	{
		get
		{
			return (Point)GetValue(Point3Property);
		}
		set
		{
			SetValueInternal(Point3Property, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 3;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.BezierSegment" /> class. </summary>
	public BezierSegment()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.BezierSegment" /> class with the specified control points, end point, and stroke option. </summary>
	/// <param name="point1">The first control point, which determines the beginning portion of the curve.</param>
	/// <param name="point2">The second control point, which determines the ending portion of the curve.</param>
	/// <param name="point3">The point to which the curve is drawn.</param>
	/// <param name="isStroked">true to stroke the curve when a <see cref="T:System.Windows.Media.Pen" /> is used to render the segment; otherwise, false.</param>
	public BezierSegment(Point point1, Point point2, Point point3, bool isStroked)
	{
		Point1 = point1;
		Point2 = point2;
		Point3 = point3;
		base.IsStroked = isStroked;
	}

	internal BezierSegment(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
	{
		Point1 = point1;
		Point2 = point2;
		Point3 = point3;
		base.IsStroked = isStroked;
		base.IsSmoothJoin = isSmoothJoin;
	}

	internal override void AddToFigure(Matrix matrix, PathFigure figure, ref Point current)
	{
		current = Point3;
		if (matrix.IsIdentity)
		{
			figure.Segments.Add(this);
			return;
		}
		Point point = Point1;
		point *= matrix;
		Point point2 = Point2;
		point2 *= matrix;
		Point point3 = current;
		point3 *= matrix;
		figure.Segments.Add(new BezierSegment(point, point2, point3, base.IsStroked, base.IsSmoothJoin));
	}

	internal override void SerializeData(StreamGeometryContext ctx)
	{
		ctx.BezierTo(Point1, Point2, Point3, base.IsStroked, base.IsSmoothJoin);
	}

	internal override bool IsCurved()
	{
		return true;
	}

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
		return string.Format(provider, "C{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}", numericListSeparator, Point1, Point2, Point3);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.BezierSegment" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BezierSegment Clone()
	{
		return (BezierSegment)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.BezierSegment" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BezierSegment CloneCurrentValue()
	{
		return (BezierSegment)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BezierSegment();
	}

	static BezierSegment()
	{
		Type typeFromHandle = typeof(BezierSegment);
		Point1Property = Animatable.RegisterProperty("Point1", typeof(Point), typeFromHandle, default(Point), null, null, isIndependentlyAnimated: false, null);
		Point2Property = Animatable.RegisterProperty("Point2", typeof(Point), typeFromHandle, default(Point), null, null, isIndependentlyAnimated: false, null);
		Point3Property = Animatable.RegisterProperty("Point3", typeof(Point), typeFromHandle, default(Point), null, null, isIndependentlyAnimated: false, null);
	}
}
