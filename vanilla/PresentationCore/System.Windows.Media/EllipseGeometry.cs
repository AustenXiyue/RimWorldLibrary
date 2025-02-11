using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents the geometry of a circle or ellipse. </summary>
public sealed class EllipseGeometry : Geometry
{
	internal const double c_arcAsBezier = 0.5522847498307935;

	private const uint c_segmentCount = 4u;

	private const uint c_pointCount = 13u;

	private const byte c_smoothBezier = 42;

	private static readonly byte[] s_roundedPathTypes;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.EllipseGeometry.RadiusX" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.EllipseGeometry.RadiusX" /> dependency property identifier.</returns>
	public static readonly DependencyProperty RadiusXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.EllipseGeometry.RadiusY" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.EllipseGeometry.RadiusY" /> dependency property identifier.</returns>
	public static readonly DependencyProperty RadiusYProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.EllipseGeometry.Center" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.EllipseGeometry.Center" /> dependency property identifier.</returns>
	public static readonly DependencyProperty CenterProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_RadiusX = 0.0;

	internal const double c_RadiusY = 0.0;

	internal static Point s_Center;

	/// <summary>Gets a <see cref="T:System.Windows.Rect" /> that represents the bounding box of this <see cref="T:System.Windows.Media.EllipseGeometry" />. This method does not consider the extra area potentially added by a stroke. </summary>
	/// <returns>The bounding box of the <see cref="T:System.Windows.Media.EllipseGeometry" />. </returns>
	public override Rect Bounds
	{
		get
		{
			ReadPreamble();
			Transform transform = base.Transform;
			if (transform == null || transform.IsIdentity)
			{
				Point center = Center;
				double radiusX = RadiusX;
				double radiusY = RadiusY;
				return new Rect(center.X - Math.Abs(radiusX), center.Y - Math.Abs(radiusY), 2.0 * Math.Abs(radiusX), 2.0 * Math.Abs(radiusY));
			}
			Transform.GetTransformValue(transform, out var currentTransformValue);
			return GetBoundsHelper(null, Matrix.Identity, Center, RadiusX, RadiusY, currentTransformValue, Geometry.StandardFlatteningTolerance, ToleranceType.Absolute);
		}
	}

	/// <summary>Gets or sets the x-radius value of the <see cref="T:System.Windows.Media.EllipseGeometry" />.   </summary>
	/// <returns>The x-radius value of the <see cref="T:System.Windows.Media.EllipseGeometry" />. </returns>
	public double RadiusX
	{
		get
		{
			return (double)GetValue(RadiusXProperty);
		}
		set
		{
			SetValueInternal(RadiusXProperty, value);
		}
	}

	/// <summary>Gets or sets the y-radius value of the <see cref="T:System.Windows.Media.EllipseGeometry" />.  </summary>
	/// <returns>The y-radius value of the <see cref="T:System.Windows.Media.EllipseGeometry" />.</returns>
	public double RadiusY
	{
		get
		{
			return (double)GetValue(RadiusYProperty);
		}
		set
		{
			SetValueInternal(RadiusYProperty, value);
		}
	}

	/// <summary>Gets or sets the center point of the <see cref="T:System.Windows.Media.EllipseGeometry" />.  </summary>
	/// <returns>The center point of the <see cref="T:System.Windows.Media.EllipseGeometry" />.</returns>
	public Point Center
	{
		get
		{
			return (Point)GetValue(CenterProperty);
		}
		set
		{
			SetValueInternal(CenterProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 3;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.EllipseGeometry" /> class. </summary>
	public EllipseGeometry()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.EllipseGeometry" /> class that has a horizontal diameter equal to the width of the passed <see cref="T:System.Windows.Rect" />, a vertical diameter equal to the length of the passed <see cref="T:System.Windows.Rect" />, and a center point location equal to the center of the passed <see cref="T:System.Windows.Rect" />. </summary>
	/// <param name="rect">The rectangle that describes the ellipse dimensions.</param>
	public EllipseGeometry(Rect rect)
	{
		if (rect.IsEmpty)
		{
			throw new ArgumentException(SR.Format(SR.Rect_Empty, "rect"));
		}
		RadiusX = (rect.Right - rect.X) * 0.5;
		RadiusY = (rect.Bottom - rect.Y) * 0.5;
		Center = new Point(rect.X + RadiusX, rect.Y + RadiusY);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.EllipseGeometry" /> class as an ellipse that has a specified center location, x radius, and y radius. </summary>
	/// <param name="center">The location of the center of the ellipse.</param>
	/// <param name="radiusX">The horizontal radius of the ellipse.</param>
	/// <param name="radiusY">The vertical radius of the ellipse.</param>
	public EllipseGeometry(Point center, double radiusX, double radiusY)
	{
		Center = center;
		RadiusX = radiusX;
		RadiusY = radiusY;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.EllipseGeometry" /> class that has the specified position, size, and transformation.  </summary>
	/// <param name="center">The location of the center of the ellipse.</param>
	/// <param name="radiusX">The horizontal radius of the ellipse.</param>
	/// <param name="radiusY">The vertical radius of the ellipse.</param>
	/// <param name="transform">The transformation to apply to the ellipse.</param>
	public EllipseGeometry(Point center, double radiusX, double radiusY, Transform transform)
		: this(center, radiusX, radiusY)
	{
		base.Transform = transform;
	}

	internal override Rect GetBoundsInternal(Pen pen, Matrix matrix, double tolerance, ToleranceType type)
	{
		Transform.GetTransformValue(base.Transform, out var currentTransformValue);
		return GetBoundsHelper(pen, matrix, Center, RadiusX, RadiusY, currentTransformValue, tolerance, type);
	}

	internal unsafe static Rect GetBoundsHelper(Pen pen, Matrix worldMatrix, Point center, double radiusX, double radiusY, Matrix geometryMatrix, double tolerance, ToleranceType type)
	{
		Rect result;
		if ((pen == null || pen.DoesNotContainGaps) && worldMatrix.IsIdentity && geometryMatrix.IsIdentity)
		{
			double num = 0.0;
			if (Pen.ContributesToBounds(pen))
			{
				num = Math.Abs(pen.Thickness);
			}
			result = new Rect(center.X - Math.Abs(radiusX) - 0.5 * num, center.Y - Math.Abs(radiusY) - 0.5 * num, 2.0 * Math.Abs(radiusX) + num, 2.0 * Math.Abs(radiusY) + num);
		}
		else
		{
			Point* ptr = stackalloc Point[13];
			GetPointList(ptr, 13u, center, radiusX, radiusY);
			fixed (byte* pTypes = s_roundedPathTypes)
			{
				result = Geometry.GetBoundsHelper(pen, &worldMatrix, ptr, pTypes, 13u, 4u, &geometryMatrix, tolerance, type, fSkipHollows: false);
			}
		}
		return result;
	}

	internal unsafe override bool ContainsInternal(Pen pen, Point hitPoint, double tolerance, ToleranceType type)
	{
		Point* ptr = stackalloc Point[(int)GetPointCount()];
		GetPointList(ptr, GetPointCount(), Center, RadiusX, RadiusY);
		fixed (byte* typeList = GetTypeList())
		{
			return ContainsInternal(pen, hitPoint, tolerance, type, ptr, GetPointCount(), typeList, GetSegmentCount());
		}
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.Media.EllipseGeometry" /> object is empty. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.EllipseGeometry" /> is empty; otherwise, false.</returns>
	public override bool IsEmpty()
	{
		return false;
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.Media.EllipseGeometry" /> object can have curved segments. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.EllipseGeometry" /> object can have curved segments; otherwise, false.</returns>
	public override bool MayHaveCurves()
	{
		return true;
	}

	/// <summary>Gets the area of this <see cref="T:System.Windows.Media.EllipseGeometry" />. </summary>
	/// <returns>The area of the filled region of this ellipse.</returns>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the enumeration values, <see cref="F:System.Windows.Media.ToleranceType.Absolute" /> or <see cref="F:System.Windows.Media.ToleranceType.Relative" />, that specifies whether the tolerance factor is an absolute value or relative to the area of this geometry.</param>
	public override double GetArea(double tolerance, ToleranceType type)
	{
		ReadPreamble();
		double num = Math.Abs(RadiusX * RadiusY) * Math.PI;
		Transform transform = base.Transform;
		if (transform != null && !transform.IsIdentity)
		{
			num *= Math.Abs(transform.Value.Determinant);
		}
		return num;
	}

	internal override PathFigureCollection GetTransformedFigureCollection(Transform transform)
	{
		Point[] pointList = GetPointList();
		Matrix combinedMatrix = GetCombinedMatrix(transform);
		if (!combinedMatrix.IsIdentity)
		{
			for (int i = 0; i < pointList.Length; i++)
			{
				pointList[i] *= combinedMatrix;
			}
		}
		PathFigureCollection pathFigureCollection = new PathFigureCollection();
		pathFigureCollection.Add(new PathFigure(pointList[0], new PathSegment[4]
		{
			new BezierSegment(pointList[1], pointList[2], pointList[3], isStroked: true, isSmoothJoin: true),
			new BezierSegment(pointList[4], pointList[5], pointList[6], isStroked: true, isSmoothJoin: true),
			new BezierSegment(pointList[7], pointList[8], pointList[9], isStroked: true, isSmoothJoin: true),
			new BezierSegment(pointList[10], pointList[11], pointList[12], isStroked: true, isSmoothJoin: true)
		}, closed: true));
		return pathFigureCollection;
	}

	internal override PathGeometry GetAsPathGeometry()
	{
		PathStreamGeometryContext pathStreamGeometryContext = new PathStreamGeometryContext(FillRule.EvenOdd, base.Transform);
		PathGeometry.ParsePathGeometryData(GetPathGeometryData(), pathStreamGeometryContext);
		return pathStreamGeometryContext.GetPathGeometry();
	}

	internal override PathGeometryData GetPathGeometryData()
	{
		if (IsObviouslyEmpty())
		{
			return Geometry.GetEmptyPathGeometryData();
		}
		PathGeometryData result = default(PathGeometryData);
		result.FillRule = FillRule.EvenOdd;
		result.Matrix = CompositionResourceManager.TransformToMilMatrix3x2D(base.Transform);
		Point[] pointList = GetPointList();
		ByteStreamGeometryContext byteStreamGeometryContext = new ByteStreamGeometryContext();
		byteStreamGeometryContext.BeginFigure(pointList[0], isFilled: true, isClosed: true);
		for (int i = 0; i < 12; i += 3)
		{
			byteStreamGeometryContext.BezierTo(pointList[i + 1], pointList[i + 2], pointList[i + 3], isStroked: true, isSmoothJoin: true);
		}
		byteStreamGeometryContext.Close();
		result.SerializedData = byteStreamGeometryContext.GetData();
		return result;
	}

	private unsafe Point[] GetPointList()
	{
		Point[] array = new Point[GetPointCount()];
		fixed (Point* points = array)
		{
			GetPointList(points, GetPointCount(), Center, RadiusX, RadiusY);
		}
		return array;
	}

	private unsafe static void GetPointList(Point* points, uint pointsCount, Point center, double radiusX, double radiusY)
	{
		Invariant.Assert(pointsCount >= 13);
		radiusX = Math.Abs(radiusX);
		radiusY = Math.Abs(radiusY);
		double num = radiusX * 0.5522847498307935;
		Point* num2 = points + 1;
		Point* num3 = points + 11;
		double num5 = (points[12].X = center.X + radiusX);
		double num7 = (num3->X = num5);
		double x = (num2->X = num7);
		points->X = x;
		Point* num9 = points + 2;
		x = (points[10].X = center.X + num);
		num9->X = x;
		Point* num11 = points + 3;
		x = (points[9].X = center.X);
		num11->X = x;
		Point* num12 = points + 4;
		x = (points[8].X = center.X - num);
		num12->X = x;
		Point* num14 = points + 5;
		Point* num15 = points + 6;
		num7 = (points[7].X = center.X - radiusX);
		x = (num15->X = num7);
		num14->X = x;
		num = radiusY * 0.5522847498307935;
		Point* num18 = points + 2;
		Point* num19 = points + 3;
		num7 = (points[4].Y = center.Y + radiusY);
		x = (num19->Y = num7);
		num18->Y = x;
		Point* num22 = points + 1;
		x = (points[5].Y = center.Y + num);
		num22->Y = x;
		Point* num24 = points + 6;
		num7 = (points[12].Y = center.Y);
		x = (num24->Y = num7);
		points->Y = x;
		Point* num26 = points + 7;
		x = (points[11].Y = center.Y - num);
		num26->Y = x;
		Point* num28 = points + 8;
		Point* num29 = points + 9;
		num7 = (points[10].Y = center.Y - radiusY);
		x = (num29->Y = num7);
		num28->Y = x;
	}

	private byte[] GetTypeList()
	{
		return s_roundedPathTypes;
	}

	private uint GetPointCount()
	{
		return 13u;
	}

	private uint GetSegmentCount()
	{
		return 4u;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.EllipseGeometry" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new EllipseGeometry Clone()
	{
		return (EllipseGeometry)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.EllipseGeometry" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new EllipseGeometry CloneCurrentValue()
	{
		return (EllipseGeometry)base.CloneCurrentValue();
	}

	private static void RadiusXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((EllipseGeometry)d).PropertyChanged(RadiusXProperty);
	}

	private static void RadiusYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((EllipseGeometry)d).PropertyChanged(RadiusYProperty);
	}

	private static void CenterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((EllipseGeometry)d).PropertyChanged(CenterProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new EllipseGeometry();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform transform = base.Transform;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(RadiusXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(RadiusYProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(CenterProperty, channel);
			DUCE.MILCMD_ELLIPSEGEOMETRY mILCMD_ELLIPSEGEOMETRY = default(DUCE.MILCMD_ELLIPSEGEOMETRY);
			mILCMD_ELLIPSEGEOMETRY.Type = MILCMD.MilCmdEllipseGeometry;
			mILCMD_ELLIPSEGEOMETRY.Handle = _duceResource.GetHandle(channel);
			mILCMD_ELLIPSEGEOMETRY.hTransform = hTransform;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_ELLIPSEGEOMETRY.RadiusX = RadiusX;
			}
			mILCMD_ELLIPSEGEOMETRY.hRadiusXAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_ELLIPSEGEOMETRY.RadiusY = RadiusY;
			}
			mILCMD_ELLIPSEGEOMETRY.hRadiusYAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_ELLIPSEGEOMETRY.Center = Center;
			}
			mILCMD_ELLIPSEGEOMETRY.hCenterAnimations = animationResourceHandle3;
			channel.SendCommand((byte*)(&mILCMD_ELLIPSEGEOMETRY), sizeof(DUCE.MILCMD_ELLIPSEGEOMETRY));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_ELLIPSEGEOMETRY))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)base.Transform)?.ReleaseOnChannel(channel);
			ReleaseOnChannelAnimations(channel);
		}
	}

	internal override DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel)
	{
		return _duceResource.GetHandle(channel);
	}

	internal override int GetChannelCountCore()
	{
		return _duceResource.GetChannelCount();
	}

	internal override DUCE.Channel GetChannelCore(int index)
	{
		return _duceResource.GetChannel(index);
	}

	static EllipseGeometry()
	{
		s_roundedPathTypes = new byte[4] { 58, 42, 42, 42 };
		s_Center = default(Point);
		Type typeFromHandle = typeof(EllipseGeometry);
		RadiusXProperty = Animatable.RegisterProperty("RadiusX", typeof(double), typeFromHandle, 0.0, RadiusXPropertyChanged, null, isIndependentlyAnimated: true, null);
		RadiusYProperty = Animatable.RegisterProperty("RadiusY", typeof(double), typeFromHandle, 0.0, RadiusYPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterProperty = Animatable.RegisterProperty("Center", typeof(Point), typeFromHandle, default(Point), CenterPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
