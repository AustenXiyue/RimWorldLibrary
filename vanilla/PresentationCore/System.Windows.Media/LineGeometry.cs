using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Represents the geometry of a line. </summary>
public sealed class LineGeometry : Geometry
{
	private static byte[] s_lineTypes;

	private const uint c_segmentCount = 1u;

	private const uint c_pointCount = 2u;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.LineGeometry.StartPoint" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.LineGeometry.StartPoint" /> dependency property.</returns>
	public static readonly DependencyProperty StartPointProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.LineGeometry.EndPoint" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.LineGeometry.EndPoint" /> dependency property.</returns>
	public static readonly DependencyProperty EndPointProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Point s_StartPoint;

	internal static Point s_EndPoint;

	/// <summary>Gets the axis-aligned bounding box of this <see cref="T:System.Windows.Media.LineGeometry" />. </summary>
	/// <returns>The axis-aligned bounding box of this <see cref="T:System.Windows.Media.LineGeometry" />. </returns>
	public override Rect Bounds
	{
		get
		{
			ReadPreamble();
			Rect rect = new Rect(StartPoint, EndPoint);
			Transform transform = base.Transform;
			if (transform != null && !transform.IsIdentity)
			{
				transform.TransformRect(ref rect);
			}
			return rect;
		}
	}

	/// <summary>Gets or sets the start point of the line.  </summary>
	/// <returns>The start point of the line. The default is (0,0).</returns>
	public Point StartPoint
	{
		get
		{
			return (Point)GetValue(StartPointProperty);
		}
		set
		{
			SetValueInternal(StartPointProperty, value);
		}
	}

	/// <summary>Gets or sets the end point of a line.  </summary>
	/// <returns>The end point of the line. The default is (0,0). </returns>
	public Point EndPoint
	{
		get
		{
			return (Point)GetValue(EndPointProperty);
		}
		set
		{
			SetValueInternal(EndPointProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LineGeometry" /> class that has no length. </summary>
	public LineGeometry()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LineGeometry" /> class that has the specified start and end points.  </summary>
	/// <param name="startPoint">The start point of the line. </param>
	/// <param name="endPoint">The end point of the line. </param>
	public LineGeometry(Point startPoint, Point endPoint)
	{
		StartPoint = startPoint;
		EndPoint = endPoint;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LineGeometry" /> class. </summary>
	/// <param name="startPoint">The start point. </param>
	/// <param name="endPoint">The end point. </param>
	/// <param name="transform">The transformation to apply to the line.</param>
	public LineGeometry(Point startPoint, Point endPoint, Transform transform)
		: this(startPoint, endPoint)
	{
		base.Transform = transform;
	}

	internal override Rect GetBoundsInternal(Pen pen, Matrix worldMatrix, double tolerance, ToleranceType type)
	{
		Transform.GetTransformValue(base.Transform, out var currentTransformValue);
		return GetBoundsHelper(pen, worldMatrix, StartPoint, EndPoint, currentTransformValue, tolerance, type);
	}

	internal unsafe static Rect GetBoundsHelper(Pen pen, Matrix worldMatrix, Point pt1, Point pt2, Matrix geometryMatrix, double tolerance, ToleranceType type)
	{
		if (pen == null && worldMatrix.IsIdentity && geometryMatrix.IsIdentity)
		{
			return new Rect(pt1, pt2);
		}
		Point* ptr = stackalloc Point[2];
		*ptr = pt1;
		ptr[1] = pt2;
		fixed (byte* pTypes = s_lineTypes)
		{
			return Geometry.GetBoundsHelper(pen, &worldMatrix, ptr, pTypes, 2u, 1u, &geometryMatrix, tolerance, type, fSkipHollows: false);
		}
	}

	internal unsafe override bool ContainsInternal(Pen pen, Point hitPoint, double tolerance, ToleranceType type)
	{
		Point* ptr = stackalloc Point[2];
		*ptr = StartPoint;
		ptr[1] = EndPoint;
		fixed (byte* typeList = GetTypeList())
		{
			return ContainsInternal(pen, hitPoint, tolerance, type, ptr, GetPointCount(), typeList, GetSegmentCount());
		}
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.Media.LineGeometry" /> object is empty. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.LineGeometry" /> is empty; otherwise, false.</returns>
	public override bool IsEmpty()
	{
		return false;
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.Media.LineGeometry" /> object can have curved segments. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.LineGeometry" /> object can have curved segments; otherwise, false.</returns>
	public override bool MayHaveCurves()
	{
		return false;
	}

	/// <summary>Gets the area of the filled region of this <see cref="T:System.Windows.Media.LineGeometry" /> object.  </summary>
	/// <returns>The area of the filled region of this <see cref="T:System.Windows.Media.LineGeometry" /> object, which is always 0 because a line contains no area.</returns>
	/// <param name="tolerance">The computational tolerance of error.</param>
	/// <param name="type">The specified type for interpreting the error tolerance.</param>
	public override double GetArea(double tolerance, ToleranceType type)
	{
		return 0.0;
	}

	private byte[] GetTypeList()
	{
		return s_lineTypes;
	}

	private uint GetPointCount()
	{
		return 2u;
	}

	private uint GetSegmentCount()
	{
		return 1u;
	}

	internal override PathGeometry GetAsPathGeometry()
	{
		PathStreamGeometryContext pathStreamGeometryContext = new PathStreamGeometryContext(FillRule.EvenOdd, base.Transform);
		PathGeometry.ParsePathGeometryData(GetPathGeometryData(), pathStreamGeometryContext);
		return pathStreamGeometryContext.GetPathGeometry();
	}

	internal override PathFigureCollection GetTransformedFigureCollection(Transform transform)
	{
		Point startPoint = StartPoint;
		Point endPoint = EndPoint;
		Transform transform2 = base.Transform;
		if (transform2 != null && !transform2.IsIdentity)
		{
			Matrix value = transform2.Value;
			startPoint *= value;
			endPoint *= value;
		}
		if (transform != null && !transform.IsIdentity)
		{
			Matrix value2 = transform.Value;
			startPoint *= value2;
			endPoint *= value2;
		}
		PathFigureCollection pathFigureCollection = new PathFigureCollection();
		pathFigureCollection.Add(new PathFigure(startPoint, new PathSegment[1]
		{
			new LineSegment(endPoint, isStroked: true)
		}, closed: false));
		return pathFigureCollection;
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
		ByteStreamGeometryContext byteStreamGeometryContext = new ByteStreamGeometryContext();
		byteStreamGeometryContext.BeginFigure(StartPoint, isFilled: true, isClosed: false);
		byteStreamGeometryContext.LineTo(EndPoint, isStroked: true, isSmoothJoin: false);
		byteStreamGeometryContext.Close();
		result.SerializedData = byteStreamGeometryContext.GetData();
		return result;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.LineGeometry" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new LineGeometry Clone()
	{
		return (LineGeometry)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.LineGeometry" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new LineGeometry CloneCurrentValue()
	{
		return (LineGeometry)base.CloneCurrentValue();
	}

	private static void StartPointPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((LineGeometry)d).PropertyChanged(StartPointProperty);
	}

	private static void EndPointPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((LineGeometry)d).PropertyChanged(EndPointProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new LineGeometry();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform transform = base.Transform;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(StartPointProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(EndPointProperty, channel);
			DUCE.MILCMD_LINEGEOMETRY mILCMD_LINEGEOMETRY = default(DUCE.MILCMD_LINEGEOMETRY);
			mILCMD_LINEGEOMETRY.Type = MILCMD.MilCmdLineGeometry;
			mILCMD_LINEGEOMETRY.Handle = _duceResource.GetHandle(channel);
			mILCMD_LINEGEOMETRY.hTransform = hTransform;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_LINEGEOMETRY.StartPoint = StartPoint;
			}
			mILCMD_LINEGEOMETRY.hStartPointAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_LINEGEOMETRY.EndPoint = EndPoint;
			}
			mILCMD_LINEGEOMETRY.hEndPointAnimations = animationResourceHandle2;
			channel.SendCommand((byte*)(&mILCMD_LINEGEOMETRY), sizeof(DUCE.MILCMD_LINEGEOMETRY));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_LINEGEOMETRY))
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

	static LineGeometry()
	{
		s_lineTypes = new byte[1] { 1 };
		s_StartPoint = default(Point);
		s_EndPoint = default(Point);
		Type typeFromHandle = typeof(LineGeometry);
		StartPointProperty = Animatable.RegisterProperty("StartPoint", typeof(Point), typeFromHandle, default(Point), StartPointPropertyChanged, null, isIndependentlyAnimated: true, null);
		EndPointProperty = Animatable.RegisterProperty("EndPoint", typeof(Point), typeFromHandle, default(Point), EndPointPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
