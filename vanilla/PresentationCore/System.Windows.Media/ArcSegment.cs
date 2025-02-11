using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Media;

/// <summary> Represents an elliptical arc between two points. </summary>
public sealed class ArcSegment : PathSegment
{
	/// <summary> Identifies the <see cref="P:System.Windows.Media.ArcSegment.Point" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.ArcSegment.Point" /> dependency property identifier.</returns>
	public static readonly DependencyProperty PointProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.ArcSegment.Size" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.ArcSegment.Size" /> dependency property identifier.</returns>
	public static readonly DependencyProperty SizeProperty;

	/// <summary>Identifies the  <see cref="P:System.Windows.Media.ArcSegment.RotationAngle" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.ArcSegment.RotationAngle" /> dependency property identifier.</returns>
	public static readonly DependencyProperty RotationAngleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.ArcSegment.IsLargeArc" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.ArcSegment.IsLargeArc" /> dependency property identifier.</returns>
	public static readonly DependencyProperty IsLargeArcProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.ArcSegment.SweepDirection" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.ArcSegment.SweepDirection" /> dependency property identifier.</returns>
	public static readonly DependencyProperty SweepDirectionProperty;

	internal static Point s_Point;

	internal static Size s_Size;

	internal const double c_RotationAngle = 0.0;

	internal const bool c_IsLargeArc = false;

	internal const SweepDirection c_SweepDirection = SweepDirection.Counterclockwise;

	internal override int EffectiveValuesInitialSize => 6;

	/// <summary> Gets or sets the endpoint of the elliptical arc. </summary>
	/// <returns>The point to which the arc is drawn. The default value is (0,0). </returns>
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

	/// <summary> Gets or sets the x- and y-radius of the arc as a <see cref="T:System.Windows.Size" /> structure. </summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> structure that describes the x- and y-radius of the elliptical arc. The <see cref="T:System.Windows.Size" /> structure's <see cref="P:System.Windows.Size.Width" /> property specifies the arc's x-radius; its <see cref="P:System.Windows.Size.Height" /> property specifies the arc's y-radius. The default value is 0,0. </returns>
	public Size Size
	{
		get
		{
			return (Size)GetValue(SizeProperty);
		}
		set
		{
			SetValueInternal(SizeProperty, value);
		}
	}

	/// <summary>Gets or sets the amount (in degrees) by which the ellipse is rotated about the x-axis.  </summary>
	/// <returns>The amount (in degrees) by which the ellipse is rotated about the x-axis. The default value is 0.</returns>
	public double RotationAngle
	{
		get
		{
			return (double)GetValue(RotationAngleProperty);
		}
		set
		{
			SetValueInternal(RotationAngleProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the arc should be greater than 180 degrees. </summary>
	/// <returns>true if the arc should be greater than 180 degrees; otherwise, false. The default value is false.</returns>
	public bool IsLargeArc
	{
		get
		{
			return (bool)GetValue(IsLargeArcProperty);
		}
		set
		{
			SetValueInternal(IsLargeArcProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary> Gets or sets a value that specifies whether the arc is drawn in the <see cref="F:System.Windows.Media.SweepDirection.Clockwise" /> or <see cref="F:System.Windows.Media.SweepDirection.Counterclockwise" /> direction.  </summary>
	/// <returns>A value that specifies the direction in which the arc is drawn. The default value is <see cref="F:System.Windows.Media.SweepDirection.Counterclockwise" />.</returns>
	public SweepDirection SweepDirection
	{
		get
		{
			return (SweepDirection)GetValue(SweepDirectionProperty);
		}
		set
		{
			SetValueInternal(SweepDirectionProperty, value);
		}
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.ArcSegment" /> class. </summary>
	public ArcSegment()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.ArcSegment" /> class. </summary>
	/// <param name="point">The destination point of the arc; the start point of the arc is defined as the current point of the <see cref="T:System.Windows.Media.PathFigure" /> to which the <see cref="T:System.Windows.Media.ArcSegment" /> is added.</param>
	/// <param name="size">The x- and y-radius of the arc. The x-radius is specified by the <see cref="T:System.Windows.Size" /> structure's <see cref="P:System.Windows.Size.Width" /> property, and the y-radius is specified by the <see cref="T:System.Windows.Size" /> structure's <see cref="P:System.Windows.Size.Height" /> property.</param>
	/// <param name="rotationAngle">The x-axis rotation of the ellipse.</param>
	/// <param name="isLargeArc">Whether the arc should be greater than 180 degrees.</param>
	/// <param name="sweepDirection">Set to <see cref="F:System.Windows.Media.SweepDirection.Clockwise" /> to draw the arc in a positive angle direction; set to <see cref="F:System.Windows.Media.SweepDirection.Counterclockwise" /> to draw the arc in a negative angle direction.</param>
	/// <param name="isStroked">Set to true to stroke the arc when a <see cref="T:System.Windows.Media.Pen" /> is used to render the segment; otherwise, false.</param>
	public ArcSegment(Point point, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked)
	{
		Size = size;
		RotationAngle = rotationAngle;
		IsLargeArc = isLargeArc;
		SweepDirection = sweepDirection;
		Point = point;
		base.IsStroked = isStroked;
	}

	internal unsafe override void AddToFigure(Matrix matrix, PathFigure figure, ref Point current)
	{
		Point point = Point;
		if (matrix.IsIdentity)
		{
			figure.Segments.Add(this);
			return;
		}
		Point* ptr = stackalloc Point[12];
		Size size = Size;
		double rotationAngle = RotationAngle;
		MilMatrix3x2D milMatrix3x2D = CompositionResourceManager.MatrixToMilMatrix3x2D(ref matrix);
		MilCoreApi.MilUtility_ArcToBezier(current, size, rotationAngle, IsLargeArc, SweepDirection, point, &milMatrix3x2D, ptr, out var cPieces);
		Invariant.Assert(cPieces <= 4);
		cPieces = Math.Min(cPieces, 4);
		bool isStroked = base.IsStroked;
		bool isSmoothJoin = base.IsSmoothJoin;
		if (cPieces > 0)
		{
			for (int i = 0; i < cPieces; i++)
			{
				figure.Segments.Add(new BezierSegment(ptr[3 * i], ptr[3 * i + 1], ptr[3 * i + 2], isStroked, i < cPieces - 1 || isSmoothJoin));
			}
		}
		else if (cPieces == 0)
		{
			figure.Segments.Add(new LineSegment(*ptr, isStroked, isSmoothJoin));
		}
		current = point;
	}

	internal override void SerializeData(StreamGeometryContext ctx)
	{
		ctx.ArcTo(Point, Size, RotationAngle, IsLargeArc, SweepDirection, base.IsStroked, base.IsSmoothJoin);
	}

	internal override bool IsCurved()
	{
		return true;
	}

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
		return string.Format(provider, "A{1:" + format + "}{0}{2:" + format + "}{0}{3}{0}{4}{0}{5:" + format + "}", numericListSeparator, Size, RotationAngle, IsLargeArc ? "1" : "0", (SweepDirection == SweepDirection.Clockwise) ? "1" : "0", Point);
	}

	private static object CoerceSize(DependencyObject d, object value)
	{
		if (((Size)value).IsEmpty)
		{
			return new Size(0.0, 0.0);
		}
		return value;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.ArcSegment" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ArcSegment Clone()
	{
		return (ArcSegment)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.ArcSegment" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ArcSegment CloneCurrentValue()
	{
		return (ArcSegment)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new ArcSegment();
	}

	static ArcSegment()
	{
		Type typeFromHandle = typeof(ArcSegment);
		PointProperty = Animatable.RegisterProperty("Point", typeof(Point), typeFromHandle, default(Point), null, null, isIndependentlyAnimated: false, null);
		SizeProperty = Animatable.RegisterProperty("Size", typeof(Size), typeFromHandle, default(Size), null, null, isIndependentlyAnimated: false, CoerceSize);
		RotationAngleProperty = Animatable.RegisterProperty("RotationAngle", typeof(double), typeFromHandle, 0.0, null, null, isIndependentlyAnimated: false, null);
		IsLargeArcProperty = Animatable.RegisterProperty("IsLargeArc", typeof(bool), typeFromHandle, false, null, null, isIndependentlyAnimated: false, null);
		SweepDirectionProperty = Animatable.RegisterProperty("SweepDirection", typeof(SweepDirection), typeFromHandle, SweepDirection.Counterclockwise, null, ValidateEnums.IsSweepDirectionValid, isIndependentlyAnimated: false, null);
	}
}
