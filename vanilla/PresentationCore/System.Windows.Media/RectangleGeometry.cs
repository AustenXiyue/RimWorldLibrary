using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media;

/// <summary> Describes a two-dimensional rectangle. </summary>
public sealed class RectangleGeometry : Geometry
{
	private static uint c_roundedSegmentCount;

	private static uint c_roundedPointCount;

	private static byte smoothBezier;

	private static byte smoothLine;

	private static byte[] s_roundedPathTypes;

	private const uint c_squaredSegmentCount = 4u;

	private const uint c_squaredPointCount = 5u;

	private static readonly byte[] s_squaredPathTypes;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.RectangleGeometry.RadiusX" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RectangleGeometry.RadiusX" /> dependency property.</returns>
	public static readonly DependencyProperty RadiusXProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.RectangleGeometry.RadiusY" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RectangleGeometry.RadiusY" /> dependency property.</returns>
	public static readonly DependencyProperty RadiusYProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.RectangleGeometry.Rect" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RectangleGeometry.Rect" /> dependency property.</returns>
	public static readonly DependencyProperty RectProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_RadiusX = 0.0;

	internal const double c_RadiusY = 0.0;

	internal static Rect s_Rect;

	/// <summary> Gets a <see cref="T:System.Windows.Rect" /> that specifies the bounding box of a <see cref="T:System.Windows.Media.RectangleGeometry" />. This method does not take any pens into account. </summary>
	/// <returns>The bounding box of the <see cref="T:System.Windows.Media.RectangleGeometry" />. </returns>
	public override Rect Bounds
	{
		get
		{
			ReadPreamble();
			Rect rect = Rect;
			Transform transform = base.Transform;
			Rect rect2;
			if (rect.IsEmpty)
			{
				rect2 = Rect.Empty;
			}
			else if (transform == null || transform.IsIdentity)
			{
				rect2 = rect;
			}
			else
			{
				double radiusX = RadiusX;
				double radiusY = RadiusY;
				if (radiusX == 0.0 && radiusY == 0.0)
				{
					rect2 = rect;
					transform.TransformRect(ref rect2);
				}
				else
				{
					Transform.GetTransformValue(transform, out var currentTransformValue);
					rect2 = GetBoundsHelper(null, Matrix.Identity, rect, radiusX, radiusY, currentTransformValue, Geometry.StandardFlatteningTolerance, ToleranceType.Absolute);
				}
			}
			return rect2;
		}
	}

	/// <summary>Gets or sets the x-radius of the ellipse use to round the corners of the rectangle.   </summary>
	/// <returns>A value greater than or equal to zero and less than or equal to half the rectangle's width that describes the x-radius of the ellipse use to round the corners of the rectangle. Values greater than half the rectangle's width are treated as though equal to half the rectangle's width. Negative values are treated as positive values. The default is 0.0. </returns>
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

	/// <summary>Gets or sets the y-radius of the ellipse use to round the corners of the rectangle.   </summary>
	/// <returns>A value greater than or equal to zero and less than or equal to half the rectangle's width that describes the y-radius of the ellipse use to round the corners of the rectangle. Values greater than half the rectangle's width are treated as though equal to half the rectangle's width. Negative values are treated as positive values. The default is 0.0. </returns>
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

	/// <summary>Gets or sets the dimensions of the rectangle.   </summary>
	/// <returns>The position and size of the rectangle. The default is <see cref="P:System.Windows.Rect.Empty" />.</returns>
	public Rect Rect
	{
		get
		{
			return (Rect)GetValue(RectProperty);
		}
		set
		{
			SetValueInternal(RectProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.RectangleGeometry" /> class, and creates a rectangle with zero area. </summary>
	public RectangleGeometry()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.RectangleGeometry" /> class and specifies its dimensions. </summary>
	/// <param name="rect">A <see cref="T:System.Windows.Rect" /> structure with the rectangle's dimensions.</param>
	public RectangleGeometry(Rect rect)
	{
		Rect = rect;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.RectangleGeometry" /> class. </summary>
	/// <param name="rect">A <see cref="T:System.Windows.Rect" /> structure with the rectangle's dimensions.</param>
	/// <param name="radiusX">The radius of the rounded corner where it connects with the upper and lower edges of the rectangle.</param>
	/// <param name="radiusY">The radius of the rounded corner where it connects with the left and right edges of the rectangle.</param>
	public RectangleGeometry(Rect rect, double radiusX, double radiusY)
		: this(rect)
	{
		RadiusX = radiusX;
		RadiusY = radiusY;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.RectangleGeometry" /> class. </summary>
	/// <param name="rect">A <see cref="T:System.Windows.Rect" /> structure with the rectangle's dimensions.</param>
	/// <param name="radiusX">The radius of the rounded corner where it connects with the upper and lower edges of the rectangle.</param>
	/// <param name="radiusY">The radius of the rounded corner where it connects with the left and right edges of the rectangle.</param>
	/// <param name="transform">The transformation to apply to the geometry.</param>
	public RectangleGeometry(Rect rect, double radiusX, double radiusY, Transform transform)
		: this(rect, radiusX, radiusY)
	{
		base.Transform = transform;
	}

	internal override bool AreClose(Geometry geometry)
	{
		if (geometry is RectangleGeometry rectangleGeometry)
		{
			Rect rect = Rect;
			Rect rect2 = rectangleGeometry.Rect;
			if (DoubleUtil.AreClose(rect.X, rect2.X) && DoubleUtil.AreClose(rect.Y, rect2.Y) && DoubleUtil.AreClose(rect.Width, rect2.Width) && DoubleUtil.AreClose(rect.Height, rect2.Height) && DoubleUtil.AreClose(RadiusX, rectangleGeometry.RadiusX) && DoubleUtil.AreClose(RadiusY, rectangleGeometry.RadiusY) && Transform == rectangleGeometry.Transform)
			{
				return IsFrozen == rectangleGeometry.IsFrozen;
			}
			return false;
		}
		return base.AreClose(geometry);
	}

	internal override Rect GetBoundsInternal(Pen pen, Matrix worldMatrix, double tolerance, ToleranceType type)
	{
		Transform.GetTransformValue(base.Transform, out var currentTransformValue);
		return GetBoundsHelper(pen, worldMatrix, Rect, RadiusX, RadiusY, currentTransformValue, tolerance, type);
	}

	internal unsafe static Rect GetBoundsHelper(Pen pen, Matrix worldMatrix, Rect rect, double radiusX, double radiusY, Matrix geometryMatrix, double tolerance, ToleranceType type)
	{
		Rect result;
		if (rect.IsEmpty)
		{
			result = Rect.Empty;
		}
		else if ((pen == null || pen.DoesNotContainGaps) && geometryMatrix.IsIdentity && worldMatrix.IsIdentity)
		{
			double num = 0.0;
			result = rect;
			if (Pen.ContributesToBounds(pen))
			{
				num = Math.Abs(pen.Thickness);
				result.X -= 0.5 * num;
				result.Y -= 0.5 * num;
				result.Width += num;
				result.Height += num;
			}
		}
		else
		{
			GetCounts(rect, radiusX, radiusY, out var pointCount, out var segmentCount);
			Invariant.Assert(pointCount != 0);
			Point* ptr = stackalloc Point[(int)pointCount];
			GetPointList(ptr, pointCount, rect, radiusX, radiusY);
			fixed (byte* typeList = GetTypeList(rect, radiusX, radiusY))
			{
				result = Geometry.GetBoundsHelper(pen, &worldMatrix, ptr, typeList, pointCount, segmentCount, &geometryMatrix, tolerance, type, fSkipHollows: false);
			}
		}
		return result;
	}

	internal unsafe override bool ContainsInternal(Pen pen, Point hitPoint, double tolerance, ToleranceType type)
	{
		if (IsEmpty())
		{
			return false;
		}
		double radiusX = RadiusX;
		double radiusY = RadiusY;
		Rect rect = Rect;
		uint pointCount = GetPointCount(rect, radiusX, radiusY);
		uint segmentCount = GetSegmentCount(rect, radiusX, radiusY);
		Point* ptr = stackalloc Point[(int)pointCount];
		GetPointList(ptr, pointCount, rect, radiusX, radiusY);
		fixed (byte* typeList = GetTypeList(rect, radiusX, radiusY))
		{
			return ContainsInternal(pen, hitPoint, tolerance, type, ptr, pointCount, typeList, segmentCount);
		}
	}

	/// <summary> Gets the area of the filled region of this <see cref="T:System.Windows.Media.RectangleGeometry" /> object. </summary>
	/// <returns>The area of the filled region of this <see cref="T:System.Windows.Media.RectangleGeometry" /> object.</returns>
	/// <param name="tolerance">The computational tolerance of error.</param>
	/// <param name="type">Specifies how the error tolerance will be interpreted.</param>
	public override double GetArea(double tolerance, ToleranceType type)
	{
		ReadPreamble();
		if (IsEmpty())
		{
			return 0.0;
		}
		double radiusX = RadiusX;
		double radiusY = RadiusY;
		Rect rect = Rect;
		double num = Math.Abs(rect.Width * rect.Height);
		num -= Math.Abs(radiusX * radiusY) * 0.8584073464102069;
		Transform transform = base.Transform;
		if (!transform.IsIdentity)
		{
			num *= Math.Abs(transform.Value.Determinant);
		}
		return num;
	}

	internal override PathFigureCollection GetTransformedFigureCollection(Transform transform)
	{
		if (IsEmpty())
		{
			return null;
		}
		Matrix combinedMatrix = GetCombinedMatrix(transform);
		double radiusX = RadiusX;
		double radiusY = RadiusY;
		Rect rect = Rect;
		if (IsRounded(radiusX, radiusY))
		{
			Point[] pointList = GetPointList(rect, radiusX, radiusY);
			if (!combinedMatrix.IsIdentity)
			{
				for (int i = 0; i < pointList.Length; i++)
				{
					pointList[i] *= combinedMatrix;
				}
			}
			PathFigureCollection pathFigureCollection = new PathFigureCollection();
			pathFigureCollection.Add(new PathFigure(pointList[0], new PathSegment[7]
			{
				new BezierSegment(pointList[1], pointList[2], pointList[3], isStroked: true, isSmoothJoin: true),
				new LineSegment(pointList[4], isStroked: true, isSmoothJoin: true),
				new BezierSegment(pointList[5], pointList[6], pointList[7], isStroked: true, isSmoothJoin: true),
				new LineSegment(pointList[8], isStroked: true, isSmoothJoin: true),
				new BezierSegment(pointList[9], pointList[10], pointList[11], isStroked: true, isSmoothJoin: true),
				new LineSegment(pointList[12], isStroked: true, isSmoothJoin: true),
				new BezierSegment(pointList[13], pointList[14], pointList[15], isStroked: true, isSmoothJoin: true)
			}, closed: true));
			return pathFigureCollection;
		}
		PathFigureCollection pathFigureCollection2 = new PathFigureCollection();
		pathFigureCollection2.Add(new PathFigure(rect.TopLeft * combinedMatrix, new PathSegment[1]
		{
			new PolyLineSegment(new Point[3]
			{
				rect.TopRight * combinedMatrix,
				rect.BottomRight * combinedMatrix,
				rect.BottomLeft * combinedMatrix
			}, isStroked: true)
		}, closed: true));
		return pathFigureCollection2;
	}

	internal static bool IsRounded(double radiusX, double radiusY)
	{
		if (radiusX != 0.0)
		{
			return radiusY != 0.0;
		}
		return false;
	}

	internal bool IsRounded()
	{
		if (RadiusX != 0.0)
		{
			return RadiusY != 0.0;
		}
		return false;
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
		double radiusX = RadiusX;
		double radiusY = RadiusY;
		Rect rect = Rect;
		ByteStreamGeometryContext byteStreamGeometryContext = new ByteStreamGeometryContext();
		if (IsRounded(radiusX, radiusY))
		{
			Point[] pointList = GetPointList(rect, radiusX, radiusY);
			byteStreamGeometryContext.BeginFigure(pointList[0], isFilled: true, isClosed: true);
			byteStreamGeometryContext.BezierTo(pointList[1], pointList[2], pointList[3], isStroked: true, isSmoothJoin: false);
			byteStreamGeometryContext.LineTo(pointList[4], isStroked: true, isSmoothJoin: false);
			byteStreamGeometryContext.BezierTo(pointList[5], pointList[6], pointList[7], isStroked: true, isSmoothJoin: false);
			byteStreamGeometryContext.LineTo(pointList[8], isStroked: true, isSmoothJoin: false);
			byteStreamGeometryContext.BezierTo(pointList[9], pointList[10], pointList[11], isStroked: true, isSmoothJoin: false);
			byteStreamGeometryContext.LineTo(pointList[12], isStroked: true, isSmoothJoin: false);
			byteStreamGeometryContext.BezierTo(pointList[13], pointList[14], pointList[15], isStroked: true, isSmoothJoin: false);
		}
		else
		{
			byteStreamGeometryContext.BeginFigure(rect.TopLeft, isFilled: true, isClosed: true);
			byteStreamGeometryContext.LineTo(rect.TopRight, isStroked: true, isSmoothJoin: false);
			byteStreamGeometryContext.LineTo(rect.BottomRight, isStroked: true, isSmoothJoin: false);
			byteStreamGeometryContext.LineTo(rect.BottomLeft, isStroked: true, isSmoothJoin: false);
		}
		byteStreamGeometryContext.Close();
		result.SerializedData = byteStreamGeometryContext.GetData();
		return result;
	}

	private unsafe Point[] GetPointList(Rect rect, double radiusX, double radiusY)
	{
		uint pointCount = GetPointCount(rect, radiusX, radiusY);
		Point[] array = new Point[pointCount];
		fixed (Point* points = array)
		{
			GetPointList(points, pointCount, rect, radiusX, radiusY);
		}
		return array;
	}

	private unsafe static void GetPointList(Point* points, uint pointsCount, Rect rect, double radiusX, double radiusY)
	{
		if (IsRounded(radiusX, radiusY))
		{
			Invariant.Assert(pointsCount >= c_roundedPointCount);
			radiusX = Math.Min(rect.Width * 0.5, Math.Abs(radiusX));
			radiusY = Math.Min(rect.Height * 0.5, Math.Abs(radiusY));
			double num = 0.44771525016920655 * radiusX;
			double num2 = 0.44771525016920655 * radiusY;
			Point* num3 = points + 1;
			Point* num4 = points + 15;
			double num5 = (points[14].X = rect.X);
			double num7 = (num4->X = num5);
			double x2 = (points->X = num7);
			num3->X = x2;
			Point* num9 = points + 2;
			x2 = (points[13].X = rect.X + num);
			num9->X = x2;
			Point* num11 = points + 3;
			x2 = (points[12].X = rect.X + radiusX);
			num11->X = x2;
			Point* num13 = points + 4;
			x2 = (points[11].X = rect.Right - radiusX);
			num13->X = x2;
			Point* num15 = points + 5;
			x2 = (points[10].X = rect.Right - num);
			num15->X = x2;
			Point* num17 = points + 6;
			Point* num18 = points + 7;
			Point* num19 = points + 8;
			num5 = (points[9].X = rect.Right);
			num7 = (num19->X = num5);
			x2 = (num18->X = num7);
			num17->X = x2;
			Point* num22 = points + 2;
			Point* num23 = points + 3;
			Point* num24 = points + 4;
			num5 = (points[5].Y = rect.Y);
			num7 = (num24->Y = num5);
			x2 = (num23->Y = num7);
			num22->Y = x2;
			Point* num27 = points + 1;
			x2 = (points[6].Y = rect.Y + num2);
			num27->Y = x2;
			x2 = (points[7].Y = rect.Y + radiusY);
			points->Y = x2;
			Point* num30 = points + 15;
			x2 = (points[8].Y = rect.Bottom - radiusY);
			num30->Y = x2;
			Point* num32 = points + 14;
			x2 = (points[9].Y = rect.Bottom - num2);
			num32->Y = x2;
			Point* num34 = points + 13;
			Point* num35 = points + 12;
			Point* num36 = points + 11;
			num5 = (points[10].Y = rect.Bottom);
			num7 = (num36->Y = num5);
			x2 = (num35->Y = num7);
			num34->Y = x2;
			points[16] = *points;
		}
		else
		{
			Invariant.Assert(pointsCount >= 5);
			Point* num39 = points + 3;
			double num7 = (points[4].X = rect.X);
			double x2 = (num39->X = num7);
			points->X = x2;
			Point* num41 = points + 1;
			x2 = (points[2].X = rect.Right);
			num41->X = x2;
			Point* num42 = points + 1;
			num7 = (points[4].Y = rect.Y);
			x2 = (num42->Y = num7);
			points->Y = x2;
			Point* num44 = points + 2;
			x2 = (points[3].Y = rect.Bottom);
			num44->Y = x2;
		}
	}

	private static byte[] GetTypeList(Rect rect, double radiusX, double radiusY)
	{
		if (rect.IsEmpty)
		{
			return null;
		}
		if (IsRounded(radiusX, radiusY))
		{
			return s_roundedPathTypes;
		}
		return s_squaredPathTypes;
	}

	private uint GetPointCount(Rect rect, double radiusX, double radiusY)
	{
		if (rect.IsEmpty)
		{
			return 0u;
		}
		if (IsRounded(radiusX, radiusY))
		{
			return c_roundedPointCount;
		}
		return 5u;
	}

	private uint GetSegmentCount(Rect rect, double radiusX, double radiusY)
	{
		if (rect.IsEmpty)
		{
			return 0u;
		}
		if (IsRounded(radiusX, radiusY))
		{
			return c_roundedSegmentCount;
		}
		return 4u;
	}

	private static void GetCounts(Rect rect, double radiusX, double radiusY, out uint pointCount, out uint segmentCount)
	{
		if (rect.IsEmpty)
		{
			pointCount = 0u;
			segmentCount = 0u;
		}
		else if (IsRounded(radiusX, radiusY))
		{
			pointCount = c_roundedPointCount;
			segmentCount = c_roundedSegmentCount;
		}
		else
		{
			pointCount = 5u;
			segmentCount = 4u;
		}
	}

	/// <summary> Determines whether this <see cref="T:System.Windows.Media.RectangleGeometry" /> object is empty. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.RectangleGeometry" /> is empty; otherwise, false.</returns>
	public override bool IsEmpty()
	{
		return Rect.IsEmpty;
	}

	/// <summary> Determines whether this <see cref="T:System.Windows.Media.RectangleGeometry" /> object may have curved segments. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.RectangleGeometry" /> object may have curved segments; otherwise, false.</returns>
	public override bool MayHaveCurves()
	{
		return IsRounded();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.RectangleGeometry" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new RectangleGeometry Clone()
	{
		return (RectangleGeometry)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.RectangleGeometry" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new RectangleGeometry CloneCurrentValue()
	{
		return (RectangleGeometry)base.CloneCurrentValue();
	}

	private static void RadiusXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RectangleGeometry)d).PropertyChanged(RadiusXProperty);
	}

	private static void RadiusYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RectangleGeometry)d).PropertyChanged(RadiusYProperty);
	}

	private static void RectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RectangleGeometry)d).PropertyChanged(RectProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new RectangleGeometry();
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
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(RectProperty, channel);
			DUCE.MILCMD_RECTANGLEGEOMETRY mILCMD_RECTANGLEGEOMETRY = default(DUCE.MILCMD_RECTANGLEGEOMETRY);
			mILCMD_RECTANGLEGEOMETRY.Type = MILCMD.MilCmdRectangleGeometry;
			mILCMD_RECTANGLEGEOMETRY.Handle = _duceResource.GetHandle(channel);
			mILCMD_RECTANGLEGEOMETRY.hTransform = hTransform;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_RECTANGLEGEOMETRY.RadiusX = RadiusX;
			}
			mILCMD_RECTANGLEGEOMETRY.hRadiusXAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_RECTANGLEGEOMETRY.RadiusY = RadiusY;
			}
			mILCMD_RECTANGLEGEOMETRY.hRadiusYAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_RECTANGLEGEOMETRY.Rect = Rect;
			}
			mILCMD_RECTANGLEGEOMETRY.hRectAnimations = animationResourceHandle3;
			channel.SendCommand((byte*)(&mILCMD_RECTANGLEGEOMETRY), sizeof(DUCE.MILCMD_RECTANGLEGEOMETRY));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_RECTANGLEGEOMETRY))
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

	static RectangleGeometry()
	{
		c_roundedSegmentCount = 8u;
		c_roundedPointCount = 17u;
		smoothBezier = 42;
		smoothLine = 9;
		s_roundedPathTypes = new byte[8] { 58, smoothLine, smoothBezier, smoothLine, smoothBezier, smoothLine, smoothBezier, smoothLine };
		s_squaredPathTypes = new byte[4] { 17, 1, 1, 1 };
		s_Rect = Rect.Empty;
		Type typeFromHandle = typeof(RectangleGeometry);
		RadiusXProperty = Animatable.RegisterProperty("RadiusX", typeof(double), typeFromHandle, 0.0, RadiusXPropertyChanged, null, isIndependentlyAnimated: true, null);
		RadiusYProperty = Animatable.RegisterProperty("RadiusY", typeof(double), typeFromHandle, 0.0, RadiusYPropertyChanged, null, isIndependentlyAnimated: true, null);
		RectProperty = Animatable.RegisterProperty("Rect", typeof(Rect), typeFromHandle, Rect.Empty, RectPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
