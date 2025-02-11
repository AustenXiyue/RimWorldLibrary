using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents a complex shape that may be composed of arcs, curves, ellipses, lines, and rectangles. </summary>
[ContentProperty("Figures")]
public sealed class PathGeometry : Geometry
{
	internal class FigureList
	{
		internal PathFigureCollection _figures;

		internal PathFigureCollection Figures => _figures;

		internal FigureList()
		{
			_figures = new PathFigureCollection();
		}

		internal unsafe void AddFigureToList(bool isFilled, bool isClosed, MilPoint2F* pPoints, uint pointCount, byte* pSegTypes, uint segmentCount)
		{
			if (pointCount < 1 || segmentCount < 1)
			{
				return;
			}
			PathFigure pathFigure = new PathFigure();
			pathFigure.IsFilled = isFilled;
			pathFigure.StartPoint = new Point(pPoints->X, pPoints->Y);
			int num = 1;
			int num2 = 0;
			for (int i = 0; i < segmentCount; i += num2)
			{
				byte b = (byte)(pSegTypes[i] & 3);
				for (num2 = 1; i + num2 < segmentCount && pSegTypes[i] == pSegTypes[i + num2]; num2++)
				{
				}
				bool isStroked = (pSegTypes[i] & 4) == 0;
				bool isSmoothJoin = (pSegTypes[i] & 8) != 0;
				switch (b)
				{
				case 1:
					if (num + num2 > pointCount)
					{
						throw new InvalidOperationException(SR.PathGeometry_InternalReadBackError);
					}
					if (num2 > 1)
					{
						PointCollection pointCollection2 = new PointCollection();
						for (int k = 0; k < num2; k++)
						{
							pointCollection2.Add(new Point(pPoints[num + k].X, pPoints[num + k].Y));
						}
						pointCollection2.Freeze();
						PolyLineSegment polyLineSegment = new PolyLineSegment(pointCollection2, isStroked, isSmoothJoin);
						polyLineSegment.Freeze();
						pathFigure.Segments.Add(polyLineSegment);
					}
					else
					{
						pathFigure.Segments.Add(new LineSegment(new Point(pPoints[num].X, pPoints[num].Y), isStroked, isSmoothJoin));
					}
					num += num2;
					break;
				case 2:
				{
					int num3 = num2 * 3;
					if (num + num3 > pointCount)
					{
						throw new InvalidOperationException(SR.PathGeometry_InternalReadBackError);
					}
					if (num2 > 1)
					{
						PointCollection pointCollection = new PointCollection();
						for (int j = 0; j < num3; j++)
						{
							pointCollection.Add(new Point(pPoints[num + j].X, pPoints[num + j].Y));
						}
						pointCollection.Freeze();
						PolyBezierSegment polyBezierSegment = new PolyBezierSegment(pointCollection, isStroked, isSmoothJoin);
						polyBezierSegment.Freeze();
						pathFigure.Segments.Add(polyBezierSegment);
					}
					else
					{
						pathFigure.Segments.Add(new BezierSegment(new Point(pPoints[num].X, pPoints[num].Y), new Point(pPoints[num + 1].X, pPoints[num + 1].Y), new Point(pPoints[num + 2].X, pPoints[num + 2].Y), isStroked, isSmoothJoin));
					}
					num += num3;
					break;
				}
				default:
					throw new InvalidOperationException(SR.PathGeometry_InternalReadBackError);
				}
			}
			if (isClosed)
			{
				pathFigure.IsClosed = true;
			}
			pathFigure.Freeze();
			Figures.Add(pathFigure);
		}
	}

	internal unsafe delegate void AddFigureToListDelegate(bool isFilled, bool isClosed, MilPoint2F* pPoints, uint pointCount, byte* pTypes, uint typeCount);

	internal PathGeometryInternalFlags _flags;

	internal MilRectD _bounds;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.PathGeometry.FillRule" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.PathGeometry.FillRule" /> dependency property identifier.</returns>
	public static readonly DependencyProperty FillRuleProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.PathGeometry.Figures" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.PathGeometry.Figures" /> dependency property identifier.</returns>
	public static readonly DependencyProperty FiguresProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const FillRule c_FillRule = FillRule.EvenOdd;

	internal static PathFigureCollection s_Figures;

	/// <summary> Gets a <see cref="T:System.Windows.Rect" /> that specifies the bounding box of this <see cref="T:System.Windows.Media.PathGeometry" /> object.   Note: This method does not take any pens into account.    </summary>
	/// <returns>The bounding box of this <see cref="T:System.Windows.Media.PathGeometry" />.</returns>
	public override Rect Bounds
	{
		get
		{
			ReadPreamble();
			if (IsEmpty())
			{
				return Rect.Empty;
			}
			if ((_flags & PathGeometryInternalFlags.BoundsValid) == 0)
			{
				_bounds = GetPathBoundsAsRB(GetPathGeometryData(), null, Matrix.Identity, Geometry.StandardFlatteningTolerance, ToleranceType.Absolute, skipHollows: false);
				_flags |= PathGeometryInternalFlags.BoundsValid;
			}
			return _bounds.AsRect;
		}
	}

	/// <summary> Gets or sets a value that determines how the intersecting areas contained in this <see cref="T:System.Windows.Media.PathGeometry" /> are combined.  </summary>
	/// <returns>Indicates how the intersecting areas of this <see cref="T:System.Windows.Media.PathGeometry" /> are combined.  The default value is EvenOdd.</returns>
	public FillRule FillRule
	{
		get
		{
			return (FillRule)GetValue(FillRuleProperty);
		}
		set
		{
			SetValueInternal(FillRuleProperty, FillRuleBoxes.Box(value));
		}
	}

	/// <summary> Gets or sets the collection of <see cref="T:System.Windows.Media.PathFigure" /> objects that describe the path's contents.  </summary>
	/// <returns>A collection of <see cref="T:System.Windows.Media.PathFigure" /> objects that describe the path's contents. Each individual <see cref="T:System.Windows.Media.PathFigure" /> describes a shape.</returns>
	public PathFigureCollection Figures
	{
		get
		{
			return (PathFigureCollection)GetValue(FiguresProperty);
		}
		set
		{
			SetValueInternal(FiguresProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.PathGeometry" /> class. </summary>
	public PathGeometry()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PathGeometry" /> class with the specified <see cref="P:System.Windows.Media.PathGeometry.Figures" />. </summary>
	/// <param name="figures">The <see cref="P:System.Windows.Media.PathGeometry.Figures" /> of the <see cref="T:System.Windows.Media.PathGeometry" /> which describes the contents of the <see cref="T:System.Windows.Shapes.Path" />. </param>
	public PathGeometry(IEnumerable<PathFigure> figures)
	{
		if (figures != null)
		{
			foreach (PathFigure figure in figures)
			{
				Figures.Add(figure);
			}
			SetDirty();
			return;
		}
		throw new ArgumentNullException("figures");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.PathGeometry" /> class with the specified <see cref="P:System.Windows.Media.PathGeometry.Figures" />, <see cref="P:System.Windows.Media.PathGeometry.FillRule" />, and <see cref="P:System.Windows.Media.Geometry.Transform" />.</summary>
	/// <param name="figures">The <see cref="P:System.Windows.Media.PathGeometry.Figures" /> of the <see cref="T:System.Windows.Media.PathGeometry" /> which describes the contents of the <see cref="T:System.Windows.Shapes.Path" />.</param>
	/// <param name="fillRule">The <see cref="P:System.Windows.Media.PathGeometry.FillRule" /> of the <see cref="T:System.Windows.Media.PathGeometry" />.</param>
	/// <param name="transform">The <see cref="P:System.Windows.Media.Geometry.Transform" /> which specifies the transform applied.</param>
	public PathGeometry(IEnumerable<PathFigure> figures, FillRule fillRule, Transform transform)
	{
		base.Transform = transform;
		if (!ValidateEnums.IsFillRuleValid(fillRule))
		{
			return;
		}
		FillRule = fillRule;
		if (figures != null)
		{
			foreach (PathFigure figure in figures)
			{
				Figures.Add(figure);
			}
			SetDirty();
			return;
		}
		throw new ArgumentNullException("figures");
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.PathGeometry" /> version of the specified <see cref="T:System.Windows.Media.Geometry" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.PathGeometry" /> created from the current values of the specified <see cref="T:System.Windows.Media.Geometry" />.</returns>
	/// <param name="geometry">The geometry from which to create a <see cref="T:System.Windows.Media.PathGeometry" />.</param>
	public static PathGeometry CreateFromGeometry(Geometry geometry)
	{
		return geometry?.GetAsPathGeometry();
	}

	internal unsafe static void ParsePathGeometryData(PathGeometryData pathData, CapacityStreamGeometryContext ctx)
	{
		if (pathData.IsEmpty())
		{
			return;
		}
		int num = 0;
		fixed (byte* serializedData = pathData.SerializedData)
		{
			Invariant.Assert(pathData.SerializedData.Length >= num + sizeof(MIL_PATHGEOMETRY));
			MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)serializedData;
			num += sizeof(MIL_PATHGEOMETRY);
			if (ptr->FigureCount == 0)
			{
				return;
			}
			ctx.SetFigureCount((int)ptr->FigureCount);
			for (int i = 0; i < ptr->FigureCount; i++)
			{
				MIL_PATHFIGURE* ptr2 = (MIL_PATHFIGURE*)(serializedData + num);
				num += sizeof(MIL_PATHFIGURE);
				ctx.BeginFigure(ptr2->StartPoint, (ptr2->Flags & MilPathFigureFlags.IsFillable) != 0, (ptr2->Flags & MilPathFigureFlags.IsClosed) != 0);
				if (ptr2->Count == 0)
				{
					continue;
				}
				ctx.SetSegmentCount((int)ptr2->Count);
				for (int j = 0; j < ptr2->Count; j++)
				{
					MIL_SEGMENT* ptr3 = (MIL_SEGMENT*)(serializedData + num);
					switch (ptr3->Type)
					{
					case MIL_SEGMENT_TYPE.MilSegmentLine:
					{
						MIL_SEGMENT_LINE* ptr7 = (MIL_SEGMENT_LINE*)(serializedData + num);
						ctx.LineTo(ptr7->Point, (ptr7->Flags & MILCoreSegFlags.SegIsAGap) == 0, (ptr7->Flags & MILCoreSegFlags.SegSmoothJoin) != 0);
						num += sizeof(MIL_SEGMENT_LINE);
						break;
					}
					case MIL_SEGMENT_TYPE.MilSegmentBezier:
					{
						MIL_SEGMENT_BEZIER* ptr6 = (MIL_SEGMENT_BEZIER*)(serializedData + num);
						ctx.BezierTo(ptr6->Point1, ptr6->Point2, ptr6->Point3, (ptr6->Flags & MILCoreSegFlags.SegIsAGap) == 0, (ptr6->Flags & MILCoreSegFlags.SegSmoothJoin) != 0);
						num += sizeof(MIL_SEGMENT_BEZIER);
						break;
					}
					case MIL_SEGMENT_TYPE.MilSegmentQuadraticBezier:
					{
						MIL_SEGMENT_QUADRATICBEZIER* ptr8 = (MIL_SEGMENT_QUADRATICBEZIER*)(serializedData + num);
						ctx.QuadraticBezierTo(ptr8->Point1, ptr8->Point2, (ptr8->Flags & MILCoreSegFlags.SegIsAGap) == 0, (ptr8->Flags & MILCoreSegFlags.SegSmoothJoin) != 0);
						num += sizeof(MIL_SEGMENT_QUADRATICBEZIER);
						break;
					}
					case MIL_SEGMENT_TYPE.MilSegmentArc:
					{
						MIL_SEGMENT_ARC* ptr9 = (MIL_SEGMENT_ARC*)(serializedData + num);
						ctx.ArcTo(ptr9->Point, ptr9->Size, ptr9->XRotation, ptr9->LargeArc != 0, (ptr9->Sweep != 0) ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, (ptr9->Flags & MILCoreSegFlags.SegIsAGap) == 0, (ptr9->Flags & MILCoreSegFlags.SegSmoothJoin) != 0);
						num += sizeof(MIL_SEGMENT_ARC);
						break;
					}
					case MIL_SEGMENT_TYPE.MilSegmentPolyLine:
					case MIL_SEGMENT_TYPE.MilSegmentPolyBezier:
					case MIL_SEGMENT_TYPE.MilSegmentPolyQuadraticBezier:
					{
						MIL_SEGMENT_POLY* ptr4 = (MIL_SEGMENT_POLY*)(serializedData + num);
						if (ptr4->Count != 0)
						{
							List<Point> list = new List<Point>((int)ptr4->Count);
							Point* ptr5 = (Point*)(serializedData + num + sizeof(MIL_SEGMENT_POLY));
							for (uint num2 = 0u; num2 < ptr4->Count; num2++)
							{
								list.Add(*ptr5);
								ptr5++;
							}
							switch (ptr3->Type)
							{
							case MIL_SEGMENT_TYPE.MilSegmentPolyLine:
								ctx.PolyLineTo(list, (ptr4->Flags & MILCoreSegFlags.SegIsAGap) == 0, (ptr4->Flags & MILCoreSegFlags.SegSmoothJoin) != 0);
								break;
							case MIL_SEGMENT_TYPE.MilSegmentPolyBezier:
								ctx.PolyBezierTo(list, (ptr4->Flags & MILCoreSegFlags.SegIsAGap) == 0, (ptr4->Flags & MILCoreSegFlags.SegSmoothJoin) != 0);
								break;
							case MIL_SEGMENT_TYPE.MilSegmentPolyQuadraticBezier:
								ctx.PolyQuadraticBezierTo(list, (ptr4->Flags & MILCoreSegFlags.SegIsAGap) == 0, (ptr4->Flags & MILCoreSegFlags.SegSmoothJoin) != 0);
								break;
							}
						}
						num += sizeof(MIL_SEGMENT_POLY) + (int)ptr4->Count * sizeof(Point);
						break;
					}
					}
				}
			}
		}
	}

	protected override void OnChanged()
	{
		SetDirty();
		base.OnChanged();
	}

	internal override PathFigureCollection GetTransformedFigureCollection(Transform transform)
	{
		Matrix combinedMatrix = GetCombinedMatrix(transform);
		PathFigureCollection pathFigureCollection;
		if (combinedMatrix.IsIdentity)
		{
			pathFigureCollection = Figures;
			if (pathFigureCollection == null)
			{
				pathFigureCollection = new PathFigureCollection();
			}
		}
		else
		{
			pathFigureCollection = new PathFigureCollection();
			PathFigureCollection figures = Figures;
			int num = figures?.Count ?? 0;
			for (int i = 0; i < num; i++)
			{
				PathFigure pathFigure = figures.Internal_GetItem(i);
				pathFigureCollection.Add(pathFigure.GetTransformedCopy(combinedMatrix));
			}
		}
		return pathFigureCollection;
	}

	/// <summary> Converts the specified <see cref="T:System.Windows.Media.Geometry" /> into a collection of <see cref="T:System.Windows.Media.PathFigure" /> objects and adds it to the path.   Note: If the specified <see cref="T:System.Windows.Media.Geometry" /> is animated, the conversion from <see cref="T:System.Windows.Media.Geometry" /> to <see cref="T:System.Windows.Media.PathFigure" /> may result in some loss of information. </summary>
	/// <param name="geometry">The geometry to add to the path.</param>
	public void AddGeometry(Geometry geometry)
	{
		if (geometry == null)
		{
			throw new ArgumentNullException("geometry");
		}
		if (!geometry.IsEmpty())
		{
			PathFigureCollection pathFigureCollection = geometry.GetPathFigureCollection();
			PathFigureCollection pathFigureCollection2 = Figures;
			if (pathFigureCollection2 == null)
			{
				PathFigureCollection pathFigureCollection4 = (Figures = new PathFigureCollection());
				pathFigureCollection2 = pathFigureCollection4;
			}
			for (int i = 0; i < pathFigureCollection.Count; i++)
			{
				pathFigureCollection2.Add(pathFigureCollection.Internal_GetItem(i));
			}
		}
	}

	/// <summary> Gets the <see cref="T:System.Windows.Point" /> and a tangent vector on this <see cref="T:System.Windows.Media.PathGeometry" /> at the specified fraction of its length. </summary>
	/// <param name="progress">The fraction of the length of this <see cref="T:System.Windows.Media.PathGeometry" />.</param>
	/// <param name="point">When this method returns, contains the location on this <see cref="T:System.Windows.Media.PathGeometry" /> at the specified fraction of its length. This parameter is passed uninitialized.</param>
	/// <param name="tangent">When this method returns, contains the tangent vector. This parameter is passed uninitialized. </param>
	public unsafe void GetPointAtFractionLength(double progress, out Point point, out Point tangent)
	{
		if (IsEmpty())
		{
			point = default(Point);
			tangent = default(Point);
			return;
		}
		PathGeometryData pathGeometryData = GetPathGeometryData();
		fixed (byte* serializedData = pathGeometryData.SerializedData)
		{
			HRESULT.Check(MilCoreApi.MilUtility_GetPointAtLengthFraction(&pathGeometryData.Matrix, pathGeometryData.FillRule, serializedData, pathGeometryData.Size, progress, out point, out tangent));
		}
	}

	internal unsafe static PathGeometry InternalCombine(Geometry geometry1, Geometry geometry2, GeometryCombineMode mode, Transform transform, double tolerance, ToleranceType type)
	{
		PathGeometry pathGeometry = null;
		MilMatrix3x2D milMatrix3x2D = CompositionResourceManager.TransformToMilMatrix3x2D(transform);
		PathGeometryData pathGeometryData = geometry1.GetPathGeometryData();
		PathGeometryData pathGeometryData2 = geometry2.GetPathGeometryData();
		fixed (byte* serializedData = pathGeometryData.SerializedData)
		{
			fixed (byte* serializedData2 = pathGeometryData2.SerializedData)
			{
				FillRule resultFillRule = FillRule.Nonzero;
				FigureList figureList = new FigureList();
				int num = UnsafeNativeMethods.MilCoreApi.MilUtility_PathGeometryCombine(&milMatrix3x2D, &pathGeometryData.Matrix, pathGeometryData.FillRule, serializedData, pathGeometryData.Size, &pathGeometryData2.Matrix, pathGeometryData2.FillRule, serializedData2, pathGeometryData2.Size, tolerance, type == ToleranceType.Relative, figureList.AddFigureToList, mode, out resultFillRule);
				if (num == -2003304438)
				{
					pathGeometry = new PathGeometry();
				}
				else
				{
					HRESULT.Check(num);
					pathGeometry = new PathGeometry(figureList.Figures, resultFillRule, null);
				}
			}
		}
		return pathGeometry;
	}

	/// <summary> Removes all <see cref="T:System.Windows.Media.PathFigure" /> objects from this <see cref="T:System.Windows.Media.PathGeometry" />. </summary>
	public void Clear()
	{
		Figures?.Clear();
	}

	internal static Rect GetPathBounds(PathGeometryData pathData, Pen pen, Matrix worldMatrix, double tolerance, ToleranceType type, bool skipHollows)
	{
		if (pathData.IsEmpty())
		{
			return Rect.Empty;
		}
		return GetPathBoundsAsRB(pathData, pen, worldMatrix, tolerance, type, skipHollows).AsRect;
	}

	internal unsafe static MilRectD GetPathBoundsAsRB(PathGeometryData pathData, Pen pen, Matrix worldMatrix, double tolerance, ToleranceType type, bool skipHollows)
	{
		double[] dashArray = null;
		MIL_PEN_DATA mIL_PEN_DATA = default(MIL_PEN_DATA);
		pen?.GetBasicPenData(&mIL_PEN_DATA, out dashArray);
		MilMatrix3x2D milMatrix3x2D = CompositionResourceManager.MatrixToMilMatrix3x2D(ref worldMatrix);
		fixed (byte* serializedData = pathData.SerializedData)
		{
			MilRectD naN = default(MilRectD);
			fixed (double* pDashArray = dashArray)
			{
				int num = UnsafeNativeMethods.MilCoreApi.MilUtility_PathGeometryBounds((pen == null) ? null : (&mIL_PEN_DATA), pDashArray, &milMatrix3x2D, pathData.FillRule, serializedData, pathData.Size, &pathData.Matrix, tolerance, type == ToleranceType.Relative, skipHollows, &naN);
				if (num == -2003304438)
				{
					naN = MilRectD.NaN;
				}
				else
				{
					HRESULT.Check(num);
				}
			}
			return naN;
		}
	}

	internal unsafe static IntersectionDetail HitTestWithPathGeometry(Geometry geometry1, Geometry geometry2, double tolerance, ToleranceType type)
	{
		IntersectionDetail result = IntersectionDetail.NotCalculated;
		PathGeometryData pathGeometryData = geometry1.GetPathGeometryData();
		PathGeometryData pathGeometryData2 = geometry2.GetPathGeometryData();
		fixed (byte* serializedData = pathGeometryData.SerializedData)
		{
			fixed (byte* serializedData2 = pathGeometryData2.SerializedData)
			{
				int num = MilCoreApi.MilUtility_PathGeometryHitTestPathGeometry(&pathGeometryData.Matrix, pathGeometryData.FillRule, serializedData, pathGeometryData.Size, &pathGeometryData2.Matrix, pathGeometryData2.FillRule, serializedData2, pathGeometryData2.Size, tolerance, type == ToleranceType.Relative, &result);
				if (num == -2003304438)
				{
					result = IntersectionDetail.Empty;
				}
				else
				{
					HRESULT.Check(num);
				}
			}
		}
		return result;
	}

	/// <summary> Determines whether this <see cref="T:System.Windows.Media.PathGeometry" /> object is empty. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.PathGeometry" /> is empty; otherwise, false.</returns>
	public override bool IsEmpty()
	{
		PathFigureCollection figures = Figures;
		if (figures != null)
		{
			return figures.Count <= 0;
		}
		return true;
	}

	/// <summary> Determines whether this <see cref="T:System.Windows.Media.PathGeometry" /> object may have curved segments. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.PathGeometry" /> object may have curved segments; otherwise, false.</returns>
	public override bool MayHaveCurves()
	{
		PathFigureCollection figures = Figures;
		int num = figures?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			if (figures.Internal_GetItem(i).MayHaveCurves())
			{
				return true;
			}
		}
		return false;
	}

	internal override PathGeometry GetAsPathGeometry()
	{
		return CloneCurrentValue();
	}

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		PathFigureCollection figures = Figures;
		FillRule fillRule = FillRule;
		string text = string.Empty;
		if (figures != null)
		{
			text = figures.ConvertToString(format, provider);
		}
		if (fillRule != 0)
		{
			return "F1" + text;
		}
		return text;
	}

	internal void SetDirty()
	{
		_flags = PathGeometryInternalFlags.Dirty;
	}

	internal override PathGeometryData GetPathGeometryData()
	{
		PathGeometryData result = default(PathGeometryData);
		result.FillRule = FillRule;
		result.Matrix = CompositionResourceManager.TransformToMilMatrix3x2D(base.Transform);
		if (IsObviouslyEmpty())
		{
			return Geometry.GetEmptyPathGeometryData();
		}
		ByteStreamGeometryContext byteStreamGeometryContext = new ByteStreamGeometryContext();
		PathFigureCollection figures = Figures;
		int num = figures?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			figures.Internal_GetItem(i).SerializeData(byteStreamGeometryContext);
		}
		byteStreamGeometryContext.Close();
		result.SerializedData = byteStreamGeometryContext.GetData();
		return result;
	}

	private unsafe void ManualUpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			Transform transform = base.Transform;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.MILCMD_PATHGEOMETRY mILCMD_PATHGEOMETRY = default(DUCE.MILCMD_PATHGEOMETRY);
			mILCMD_PATHGEOMETRY.Type = MILCMD.MilCmdPathGeometry;
			mILCMD_PATHGEOMETRY.Handle = _duceResource.GetHandle(channel);
			mILCMD_PATHGEOMETRY.hTransform = hTransform;
			mILCMD_PATHGEOMETRY.FillRule = FillRule;
			PathGeometryData pathGeometryData = GetPathGeometryData();
			mILCMD_PATHGEOMETRY.FiguresSize = pathGeometryData.Size;
			channel.BeginCommand((byte*)(&mILCMD_PATHGEOMETRY), sizeof(DUCE.MILCMD_PATHGEOMETRY), checked((int)mILCMD_PATHGEOMETRY.FiguresSize));
			fixed (byte* serializedData = pathGeometryData.SerializedData)
			{
				channel.AppendCommandData(serializedData, checked((int)mILCMD_PATHGEOMETRY.FiguresSize));
			}
			channel.EndCommand();
		}
	}

	internal override void TransformPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if ((_flags & PathGeometryInternalFlags.BoundsValid) != 0)
		{
			SetDirty();
		}
	}

	internal void FiguresPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		SetDirty();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PathGeometry" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PathGeometry Clone()
	{
		return (PathGeometry)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.PathGeometry" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PathGeometry CloneCurrentValue()
	{
		return (PathGeometry)base.CloneCurrentValue();
	}

	private static void FillRulePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((PathGeometry)d).PropertyChanged(FillRuleProperty);
	}

	private static void FiguresPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		PathGeometry obj = (PathGeometry)d;
		obj.FiguresPropertyChangedHook(e);
		obj.PropertyChanged(FiguresProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new PathGeometry();
	}

	internal override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		ManualUpdateResource(channel, skipOnChannelCheck);
		base.UpdateResource(channel, skipOnChannelCheck);
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_PATHGEOMETRY))
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

	static PathGeometry()
	{
		s_Figures = PathFigureCollection.Empty;
		Type typeFromHandle = typeof(PathGeometry);
		FillRuleProperty = Animatable.RegisterProperty("FillRule", typeof(FillRule), typeFromHandle, FillRule.EvenOdd, FillRulePropertyChanged, ValidateEnums.IsFillRuleValid, isIndependentlyAnimated: false, null);
		FiguresProperty = Animatable.RegisterProperty("Figures", typeof(PathFigureCollection), typeFromHandle, new FreezableDefaultValueFactory(PathFigureCollection.Empty), FiguresPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
