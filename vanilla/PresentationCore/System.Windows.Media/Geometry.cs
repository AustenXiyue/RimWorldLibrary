using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Converters;
using MS.Internal;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

/// <summary>Classes that derive from this abstract base class define geometric shapes. <see cref="T:System.Windows.Media.Geometry" /> objects can be used for clipping, hit-testing, and rendering 2-D graphic data. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
[TypeConverter(typeof(GeometryConverter))]
[ValueSerializer(typeof(GeometryValueSerializer))]
public abstract class Geometry : Animatable, DUCE.IResource, IFormattable
{
	internal struct PathGeometryData
	{
		internal FillRule FillRule;

		internal MilMatrix3x2D Matrix;

		internal byte[] SerializedData;

		internal unsafe uint Size
		{
			get
			{
				if (SerializedData == null || SerializedData.Length == 0)
				{
					return 0u;
				}
				fixed (byte* serializedData = SerializedData)
				{
					MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)serializedData;
					uint num = ((ptr != null) ? ptr->Size : 0);
					Invariant.Assert(num <= (uint)SerializedData.Length);
					return num;
				}
			}
		}

		internal unsafe bool IsEmpty()
		{
			if (SerializedData == null || SerializedData.Length == 0)
			{
				return true;
			}
			fixed (byte* serializedData = SerializedData)
			{
				MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)serializedData;
				return ptr->FigureCount == 0;
			}
		}
	}

	private const double c_tolerance = 0.25;

	private static Geometry s_empty;

	private static PathGeometryData s_emptyPathGeometryData;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Geometry.Transform" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Geometry.Transform" /> dependency property.</returns>
	public static readonly DependencyProperty TransformProperty;

	internal static Transform s_Transform;

	/// <summary>Gets an empty object. </summary>
	/// <returns>The empty geometry object.</returns>
	public static Geometry Empty => s_empty;

	/// <summary>Gets a <see cref="T:System.Windows.Rect" /> that specifies the axis-aligned bounding box of the <see cref="T:System.Windows.Media.Geometry" />. </summary>
	/// <returns>The axis-aligned bounding box of the <see cref="T:System.Windows.Media.Geometry" />.</returns>
	public virtual Rect Bounds => PathGeometry.GetPathBounds(GetPathGeometryData(), null, Matrix.Identity, StandardFlatteningTolerance, ToleranceType.Absolute, skipHollows: false);

	/// <summary>Gets the standard tolerance used for polygonal approximation. </summary>
	/// <returns>The standard tolerance, 0.25.</returns>
	public static double StandardFlatteningTolerance => 0.25;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Transform" /> object applied to a <see cref="T:System.Windows.Media.Geometry" />.  </summary>
	/// <returns>The transformation applied to the <see cref="T:System.Windows.Media.Geometry" />. Note that this value may be a single <see cref="T:System.Windows.Media.Transform" /> or a <see cref="T:System.Windows.Media.TransformCollection" /> cast as a <see cref="T:System.Windows.Media.Transform" />.</returns>
	public Transform Transform
	{
		get
		{
			return (Transform)GetValue(TransformProperty);
		}
		set
		{
			SetValueInternal(TransformProperty, value);
		}
	}

	internal Geometry()
	{
	}

	/// <summary>Returns an axis-aligned rectangle that is exactly large enough to contain the geometry after it has been outlined with the specified <see cref="T:System.Windows.Media.Pen" />, given the specified tolerance factor.</summary>
	/// <returns>An axis aligned rectangle that is exactly large enough to contain the outlined geometry.</returns>
	/// <param name="pen">An object that describes the area of the geometry's stroke.</param>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public virtual Rect GetRenderBounds(Pen pen, double tolerance, ToleranceType type)
	{
		ReadPreamble();
		Matrix identity = Matrix.Identity;
		return GetBoundsInternal(pen, identity, tolerance, type);
	}

	/// <summary>Returns an axis-aligned rectangle that is exactly large enough to contain the geometry after it has been outlined with the specified <see cref="T:System.Windows.Media.Pen" />. </summary>
	/// <returns>An axis aligned rectangle that is exactly large enough to contain the outlined geometry.</returns>
	/// <param name="pen">An object that describes the area of the geometry's stroke.</param>
	public Rect GetRenderBounds(Pen pen)
	{
		ReadPreamble();
		Matrix identity = Matrix.Identity;
		return GetBoundsInternal(pen, identity, StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	internal virtual bool AreClose(Geometry geometry)
	{
		return false;
	}

	internal virtual Rect GetBoundsInternal(Pen pen, Matrix matrix, double tolerance, ToleranceType type)
	{
		if (IsObviouslyEmpty())
		{
			return Rect.Empty;
		}
		return PathGeometry.GetPathBounds(GetPathGeometryData(), pen, matrix, tolerance, type, skipHollows: true);
	}

	internal Rect GetBoundsInternal(Pen pen, Matrix matrix)
	{
		return GetBoundsInternal(pen, matrix, StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	internal unsafe static Rect GetBoundsHelper(Pen pen, Matrix* pWorldMatrix, Point* pPoints, byte* pTypes, uint pointCount, uint segmentCount, Matrix* pGeometryMatrix, double tolerance, ToleranceType type, bool fSkipHollows)
	{
		double[] dashArray = null;
		bool flag = Pen.ContributesToBounds(pen);
		MIL_PEN_DATA mIL_PEN_DATA = default(MIL_PEN_DATA);
		if (flag)
		{
			pen.GetBasicPenData(&mIL_PEN_DATA, out dashArray);
		}
		MilMatrix3x2D milMatrix3x2D = default(MilMatrix3x2D);
		if (pGeometryMatrix != null)
		{
			milMatrix3x2D = CompositionResourceManager.MatrixToMilMatrix3x2D(ref *pGeometryMatrix);
		}
		MilMatrix3x2D milMatrix3x2D2 = CompositionResourceManager.MatrixToMilMatrix3x2D(ref *pWorldMatrix);
		Rect empty = default(Rect);
		fixed (double* ptr = dashArray)
		{
			int num = MilCoreApi.MilUtility_PolygonBounds(&milMatrix3x2D2, flag ? (&mIL_PEN_DATA) : null, (dashArray == null) ? null : ptr, pPoints, pTypes, pointCount, segmentCount, (pGeometryMatrix == null) ? null : (&milMatrix3x2D), tolerance, type == ToleranceType.Relative, fSkipHollows, &empty);
			if (num == -2003304438)
			{
				empty = Rect.Empty;
			}
			else
			{
				HRESULT.Check(num);
			}
		}
		return empty;
	}

	internal virtual void TransformPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
	}

	internal Geometry GetTransformedCopy(Transform transform)
	{
		Geometry geometry = Clone();
		Transform transform2 = Transform;
		if (transform != null && !transform.IsIdentity)
		{
			if (transform2 == null || transform2.IsIdentity)
			{
				geometry.Transform = transform;
			}
			else
			{
				geometry.Transform = new MatrixTransform(transform2.Value * transform.Value);
			}
		}
		return geometry;
	}

	/// <summary>Gets a value that indicates whether the value of the <see cref="P:System.Windows.Media.Geometry.Transform" /> property should be serialized.</summary>
	/// <returns>true if the value of the geometry's <see cref="P:System.Windows.Media.Geometry.Transform" /> property should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeTransform()
	{
		Transform transform = Transform;
		if (transform != null)
		{
			return !transform.IsIdentity;
		}
		return false;
	}

	/// <summary>Gets the area, within the specified tolerance, of the filled region of the <see cref="T:System.Windows.Media.Geometry" /> object. </summary>
	/// <returns>The area of the filled region of the geometry.</returns>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public unsafe virtual double GetArea(double tolerance, ToleranceType type)
	{
		ReadPreamble();
		if (IsObviouslyEmpty())
		{
			return 0.0;
		}
		PathGeometryData pathGeometryData = GetPathGeometryData();
		if (pathGeometryData.IsEmpty())
		{
			return 0.0;
		}
		double result = default(double);
		fixed (byte* serializedData = pathGeometryData.SerializedData)
		{
			int num = MilCoreApi.MilUtility_GeometryGetArea(pathGeometryData.FillRule, serializedData, pathGeometryData.Size, &pathGeometryData.Matrix, tolerance, type == ToleranceType.Relative, &result);
			if (num == -2003304438)
			{
				result = 0.0;
			}
			else
			{
				HRESULT.Check(num);
			}
		}
		return result;
	}

	/// <summary>Gets the area of the filled region of the <see cref="T:System.Windows.Media.Geometry" /> object. </summary>
	/// <returns>The area of the filled region of the geometry.</returns>
	public double GetArea()
	{
		return GetArea(StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	/// <summary>Determines whether the object is empty. </summary>
	/// <returns>true if the geometry is empty; otherwise, false.</returns>
	public abstract bool IsEmpty();

	/// <summary>Determines whether the object might have curved segments. </summary>
	/// <returns>true if the geometry object might have curved segments; otherwise, false.</returns>
	public abstract bool MayHaveCurves();

	/// <summary>Indicates whether the geometry contains the specified <see cref="T:System.Windows.Point" />, given the specified margin of error.</summary>
	/// <returns>true if the geometry contains <paramref name="hitPoint" />, given the specified margin of error; otherwise, false.</returns>
	/// <param name="hitPoint">The point to test for containment.</param>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public bool FillContains(Point hitPoint, double tolerance, ToleranceType type)
	{
		return ContainsInternal(null, hitPoint, tolerance, type);
	}

	/// <summary>Indicates whether the geometry contains the specified <see cref="T:System.Windows.Point" />.</summary>
	/// <returns>true if the geometry contains <paramref name="hitPoint" />; otherwise, false.</returns>
	/// <param name="hitPoint">The point to test for containment.</param>
	public bool FillContains(Point hitPoint)
	{
		return ContainsInternal(null, hitPoint, StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Point" /> is contained in the stroke produced by applying the specified <see cref="T:System.Windows.Media.Pen" /> to the geometry, given the specified margin of error.</summary>
	/// <returns>true if the stroke created by applying the specified <see cref="T:System.Windows.Media.Pen" /> to the geometry contains the specified point, given the specified tolerance factor; otherwise, false.</returns>
	/// <param name="pen">An object that defines the stroke of a geometry.</param>
	/// <param name="hitPoint">The point to test for containment.</param>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public bool StrokeContains(Pen pen, Point hitPoint, double tolerance, ToleranceType type)
	{
		if (pen == null)
		{
			return false;
		}
		return ContainsInternal(pen, hitPoint, tolerance, type);
	}

	internal unsafe virtual bool ContainsInternal(Pen pen, Point hitPoint, double tolerance, ToleranceType type)
	{
		if (IsObviouslyEmpty())
		{
			return false;
		}
		PathGeometryData pathGeometryData = GetPathGeometryData();
		if (pathGeometryData.IsEmpty())
		{
			return false;
		}
		bool pDoesContain = false;
		double[] dashArray = null;
		MIL_PEN_DATA mIL_PEN_DATA = default(MIL_PEN_DATA);
		pen?.GetBasicPenData(&mIL_PEN_DATA, out dashArray);
		fixed (byte* serializedData = pathGeometryData.SerializedData)
		{
			fixed (double* pDashArray = dashArray)
			{
				int num = MilCoreApi.MilUtility_PathGeometryHitTest(&pathGeometryData.Matrix, (pen == null) ? null : (&mIL_PEN_DATA), pDashArray, pathGeometryData.FillRule, serializedData, pathGeometryData.Size, tolerance, type == ToleranceType.Relative, &hitPoint, out pDoesContain);
				if (num == -2003304438)
				{
					pDoesContain = false;
				}
				else
				{
					HRESULT.Check(num);
				}
			}
		}
		return pDoesContain;
	}

	internal unsafe bool ContainsInternal(Pen pen, Point hitPoint, double tolerance, ToleranceType type, Point* pPoints, uint pointCount, byte* pTypes, uint typeCount)
	{
		bool pDoesContain = false;
		MilMatrix3x2D milMatrix3x2D = CompositionResourceManager.TransformToMilMatrix3x2D(Transform);
		double[] dashArray = null;
		MIL_PEN_DATA mIL_PEN_DATA = default(MIL_PEN_DATA);
		pen?.GetBasicPenData(&mIL_PEN_DATA, out dashArray);
		fixed (double* pDashArray = dashArray)
		{
			int num = MilCoreApi.MilUtility_PolygonHitTest(&milMatrix3x2D, (pen == null) ? null : (&mIL_PEN_DATA), pDashArray, pPoints, pTypes, pointCount, typeCount, tolerance, type == ToleranceType.Relative, &hitPoint, out pDoesContain);
			if (num == -2003304438)
			{
				pDoesContain = false;
			}
			else
			{
				HRESULT.Check(num);
			}
		}
		return pDoesContain;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Point" /> is contained in the stroke produced by applying the specified <see cref="T:System.Windows.Media.Pen" /> to the geometry. </summary>
	/// <returns>true if <paramref name="hitPoint" /> is contained in the stroke produced by applying the specified <see cref="T:System.Windows.Media.Pen" /> to the geometry; otherwise, false.</returns>
	/// <param name="pen">An object that determines the area of the geometry's stroke.</param>
	/// <param name="hitPoint">The point to test for containment.</param>
	public bool StrokeContains(Pen pen, Point hitPoint)
	{
		return StrokeContains(pen, hitPoint, StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	/// <summary>Indicates whether the current geometry contains the specified <see cref="T:System.Windows.Media.Geometry" />, given the specified margin of error.</summary>
	/// <returns>true if the current geometry contains <paramref name="geometry" />, given the specified margin of error; otherwise, false.</returns>
	/// <param name="geometry">The geometry to test for containment.</param>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometries. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public bool FillContains(Geometry geometry, double tolerance, ToleranceType type)
	{
		return FillContainsWithDetail(geometry, tolerance, type) == IntersectionDetail.FullyContains;
	}

	/// <summary>Indicates whether the current geometry completely contains the specified <see cref="T:System.Windows.Media.Geometry" />.</summary>
	/// <returns>true if the current geometry completely contains <paramref name="geometry" />; otherwise, false.</returns>
	/// <param name="geometry">The geometry to test for containment.</param>
	public bool FillContains(Geometry geometry)
	{
		return FillContains(geometry, StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	/// <summary>Returns a value that describes the intersection between the current geometry and the specified geometry, given the specified margin of error.</summary>
	/// <returns>One of the enumeration values.</returns>
	/// <param name="geometry">The geometry to test for containment.</param>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometries. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public virtual IntersectionDetail FillContainsWithDetail(Geometry geometry, double tolerance, ToleranceType type)
	{
		ReadPreamble();
		if (IsObviouslyEmpty() || geometry == null || geometry.IsObviouslyEmpty())
		{
			return IntersectionDetail.Empty;
		}
		return PathGeometry.HitTestWithPathGeometry(this, geometry, tolerance, type);
	}

	/// <summary>Returns a value that describes the intersection between the current geometry and the specified geometry.</summary>
	/// <returns>One of the enumeration values.</returns>
	/// <param name="geometry">The geometry to test for containment.</param>
	public IntersectionDetail FillContainsWithDetail(Geometry geometry)
	{
		return FillContainsWithDetail(geometry, StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	/// <summary>Gets a value that describes the intersection between the specified <see cref="T:System.Windows.Media.Geometry" /> and the stroke created by applying the specified <see cref="T:System.Windows.Media.Pen" /> to the current geometry, given the specified margin of error.</summary>
	/// <returns>One of the enumeration values.</returns>
	/// <param name="pen">An object that determines the area of the current geometry's stroke.</param>
	/// <param name="geometry">The geometry to test for containment.</param>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometries. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public IntersectionDetail StrokeContainsWithDetail(Pen pen, Geometry geometry, double tolerance, ToleranceType type)
	{
		if (IsObviouslyEmpty() || geometry == null || geometry.IsObviouslyEmpty() || pen == null)
		{
			return IntersectionDetail.Empty;
		}
		return PathGeometry.HitTestWithPathGeometry(GetWidenedPathGeometry(pen), geometry, tolerance, type);
	}

	/// <summary>Returns a value that describes the intersection between the specified <see cref="T:System.Windows.Media.Geometry" /> and the stroke created by applying the specified <see cref="T:System.Windows.Media.Pen" /> to the current geometry.</summary>
	/// <returns>One of the enumeration values.</returns>
	/// <param name="pen">An object that determines the area of the current geometry's stroke.</param>
	/// <param name="geometry">The geometry to test for containment.</param>
	public IntersectionDetail StrokeContainsWithDetail(Pen pen, Geometry geometry)
	{
		return StrokeContainsWithDetail(pen, geometry, StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.PathGeometry" />, within the specified tolerance, that is a polygonal approximation of the <see cref="T:System.Windows.Media.Geometry" /> object. </summary>
	/// <returns>The polygonal approximation of the <see cref="T:System.Windows.Media.Geometry" />.</returns>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public unsafe virtual PathGeometry GetFlattenedPathGeometry(double tolerance, ToleranceType type)
	{
		ReadPreamble();
		if (IsObviouslyEmpty())
		{
			return new PathGeometry();
		}
		PathGeometryData pathGeometryData = GetPathGeometryData();
		if (pathGeometryData.IsEmpty())
		{
			return new PathGeometry();
		}
		PathGeometry pathGeometry = null;
		fixed (byte* serializedData = pathGeometryData.SerializedData)
		{
			FillRule resultFillRule = FillRule.Nonzero;
			PathGeometry.FigureList figureList = new PathGeometry.FigureList();
			int num = UnsafeNativeMethods.MilCoreApi.MilUtility_PathGeometryFlatten(&pathGeometryData.Matrix, pathGeometryData.FillRule, serializedData, pathGeometryData.Size, tolerance, type == ToleranceType.Relative, figureList.AddFigureToList, out resultFillRule);
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
		return pathGeometry;
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.PathGeometry" /> that is a polygonal approximation of the <see cref="T:System.Windows.Media.Geometry" /> object. </summary>
	/// <returns>The polygonal approximation of the <see cref="T:System.Windows.Media.Geometry" />.</returns>
	public PathGeometry GetFlattenedPathGeometry()
	{
		return GetFlattenedPathGeometry(StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.PathGeometry" /> that is the shape defined by the stroke on the <see cref="T:System.Windows.Media.Geometry" /> produced by the specified <see cref="T:System.Windows.Media.Pen" />, given the specified tolerance factor.</summary>
	/// <returns>The geometry, widened by <paramref name="pen" />.</returns>
	/// <param name="pen">The object used to define the area of the geometry's stroke.</param>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public unsafe virtual PathGeometry GetWidenedPathGeometry(Pen pen, double tolerance, ToleranceType type)
	{
		ReadPreamble();
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (IsObviouslyEmpty())
		{
			return new PathGeometry();
		}
		PathGeometryData pathGeometryData = GetPathGeometryData();
		if (pathGeometryData.IsEmpty())
		{
			return new PathGeometry();
		}
		PathGeometry result = null;
		double[] dashArray = null;
		MIL_PEN_DATA mIL_PEN_DATA = default(MIL_PEN_DATA);
		pen.GetBasicPenData(&mIL_PEN_DATA, out dashArray);
		fixed (byte* serializedData = pathGeometryData.SerializedData)
		{
			FillRule widenedFillRule = FillRule.Nonzero;
			PathGeometry.FigureList figureList = new PathGeometry.FigureList();
			GCHandle gCHandle = default(GCHandle);
			if (dashArray != null)
			{
				gCHandle = GCHandle.Alloc(dashArray, GCHandleType.Pinned);
			}
			try
			{
				int num = UnsafeNativeMethods.MilCoreApi.MilUtility_PathGeometryWiden(&mIL_PEN_DATA, (dashArray == null) ? ((double*)null) : ((double*)gCHandle.AddrOfPinnedObject()), &pathGeometryData.Matrix, pathGeometryData.FillRule, serializedData, pathGeometryData.Size, tolerance, type == ToleranceType.Relative, figureList.AddFigureToList, out widenedFillRule);
				if (num == -2003304438)
				{
					result = new PathGeometry();
				}
				else
				{
					HRESULT.Check(num);
					result = new PathGeometry(figureList.Figures, widenedFillRule, null);
				}
			}
			finally
			{
				if (gCHandle.IsAllocated)
				{
					gCHandle.Free();
				}
			}
		}
		return result;
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.PathGeometry" /> that is the shape defined by the stroke on the <see cref="T:System.Windows.Media.Geometry" /> produced by the specified <see cref="T:System.Windows.Media.Pen" />. </summary>
	/// <returns>The outlined geometry.</returns>
	/// <param name="pen">An object that describes the area of the geometry's stroke.</param>
	public PathGeometry GetWidenedPathGeometry(Pen pen)
	{
		return GetWidenedPathGeometry(pen, StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	/// <summary>Combines the two geometries using the specified <see cref="T:System.Windows.Media.GeometryCombineMode" /> and tolerance factor, and applies the specified transform to the resulting geometry.</summary>
	/// <returns>The combined geometry.</returns>
	/// <param name="geometry1">The first geometry to combine.</param>
	/// <param name="geometry2">The second geometry to combine.</param>
	/// <param name="mode">One of the enumeration values that specifies how the geometries are combined.</param>
	/// <param name="transform">A transformation to apply to the combined geometry, or null.</param>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometries. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public static PathGeometry Combine(Geometry geometry1, Geometry geometry2, GeometryCombineMode mode, Transform transform, double tolerance, ToleranceType type)
	{
		return PathGeometry.InternalCombine(geometry1, geometry2, mode, transform, tolerance, type);
	}

	/// <summary>Combines the two geometries using the specified <see cref="T:System.Windows.Media.GeometryCombineMode" /> and applies the specified transform to the resulting geometry.</summary>
	/// <returns>The combined geometry.</returns>
	/// <param name="geometry1">The first geometry to combine.</param>
	/// <param name="geometry2">The second geometry to combine.</param>
	/// <param name="mode">One of the enumeration values that specifies how the geometries are combined.</param>
	/// <param name="transform">A transformation to apply to the combined geometry, or null.</param>
	public static PathGeometry Combine(Geometry geometry1, Geometry geometry2, GeometryCombineMode mode, Transform transform)
	{
		return PathGeometry.InternalCombine(geometry1, geometry2, mode, transform, StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.PathGeometry" />, within the specified tolerance, that is a simplified outline of the filled region of the <see cref="T:System.Windows.Media.Geometry" />. </summary>
	/// <returns>A simplified outline of the filled region of the <see cref="T:System.Windows.Media.Geometry" />.</returns>
	/// <param name="tolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution. If <paramref name="tolerance" /> is less than .000001, .000001 is used instead.</param>
	/// <param name="type">One of the <see cref="T:System.Windows.Media.ToleranceType" /> values that specifies whether the tolerance factor is an absolute value or relative to the area of the geometry.</param>
	public unsafe virtual PathGeometry GetOutlinedPathGeometry(double tolerance, ToleranceType type)
	{
		ReadPreamble();
		if (IsObviouslyEmpty())
		{
			return new PathGeometry();
		}
		PathGeometryData pathGeometryData = GetPathGeometryData();
		if (pathGeometryData.IsEmpty())
		{
			return new PathGeometry();
		}
		PathGeometry pathGeometry = null;
		fixed (byte* serializedData = pathGeometryData.SerializedData)
		{
			Invariant.Assert(serializedData != null);
			FillRule outlinedFillRule = FillRule.Nonzero;
			PathGeometry.FigureList figureList = new PathGeometry.FigureList();
			int num = UnsafeNativeMethods.MilCoreApi.MilUtility_PathGeometryOutline(&pathGeometryData.Matrix, pathGeometryData.FillRule, serializedData, pathGeometryData.Size, tolerance, type == ToleranceType.Relative, figureList.AddFigureToList, out outlinedFillRule);
			if (num == -2003304438)
			{
				pathGeometry = new PathGeometry();
			}
			else
			{
				HRESULT.Check(num);
				pathGeometry = new PathGeometry(figureList.Figures, outlinedFillRule, null);
			}
		}
		return pathGeometry;
	}

	/// <summary>Gets a <see cref="T:System.Windows.Media.PathGeometry" /> that is a simplified outline of the filled region of the <see cref="T:System.Windows.Media.Geometry" />. </summary>
	/// <returns>A simplified outline of the filled region of the <see cref="T:System.Windows.Media.Geometry" />.</returns>
	public PathGeometry GetOutlinedPathGeometry()
	{
		return GetOutlinedPathGeometry(StandardFlatteningTolerance, ToleranceType.Absolute);
	}

	internal abstract PathGeometry GetAsPathGeometry();

	internal abstract PathGeometryData GetPathGeometryData();

	internal PathFigureCollection GetPathFigureCollection()
	{
		return GetTransformedFigureCollection(null);
	}

	internal Matrix GetCombinedMatrix(Transform transform)
	{
		Matrix result = Matrix.Identity;
		Transform transform2 = Transform;
		if (transform2 != null && !transform2.IsIdentity)
		{
			result = transform2.Value;
			if (transform != null && !transform.IsIdentity)
			{
				result *= transform.Value;
			}
		}
		else if (transform != null && !transform.IsIdentity)
		{
			result = transform.Value;
		}
		return result;
	}

	internal abstract PathFigureCollection GetTransformedFigureCollection(Transform transform);

	internal virtual bool IsObviouslyEmpty()
	{
		return IsEmpty();
	}

	internal virtual bool CanSerializeToString()
	{
		return false;
	}

	internal static PathGeometryData GetEmptyPathGeometryData()
	{
		return s_emptyPathGeometryData;
	}

	private unsafe static PathGeometryData MakeEmptyPathGeometryData()
	{
		PathGeometryData result = default(PathGeometryData);
		result.FillRule = FillRule.EvenOdd;
		result.Matrix = CompositionResourceManager.MatrixToMilMatrix3x2D(Matrix.Identity);
		int num = sizeof(MIL_PATHGEOMETRY);
		result.SerializedData = new byte[num];
		fixed (byte* serializedData = result.SerializedData)
		{
			MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)serializedData;
			ptr->FigureCount = 0u;
			ptr->Size = (uint)num;
		}
		return result;
	}

	private static Geometry MakeEmptyGeometry()
	{
		StreamGeometry streamGeometry = new StreamGeometry();
		streamGeometry.Freeze();
		return streamGeometry;
	}

	/// <summary>Creates a modifiable clone of the <see cref="T:System.Windows.Media.Geometry" />, making deep copies of the object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Geometry Clone()
	{
		return (Geometry)base.Clone();
	}

	/// <summary>Creates a modifiable clone of the <see cref="T:System.Windows.Media.Geometry" /> object, making deep copies of the object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Geometry CloneCurrentValue()
	{
		return (Geometry)base.CloneCurrentValue();
	}

	private static void TransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Geometry geometry = (Geometry)d;
		geometry.TransformPropertyChangedHook(e);
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Transform resource = (Transform)e.OldValue;
		Transform resource2 = (Transform)e.NewValue;
		if (geometry.Dispatcher != null)
		{
			DUCE.IResource resource3 = geometry;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					geometry.ReleaseResource(resource, channel);
					geometry.AddRefResource(resource2, channel);
				}
			}
		}
		geometry.PropertyChanged(TransformProperty);
	}

	internal abstract DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return AddRefOnChannelCore(channel);
		}
	}

	internal abstract void ReleaseOnChannelCore(DUCE.Channel channel);

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			ReleaseOnChannelCore(channel);
		}
	}

	internal abstract DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return GetHandleCore(channel);
		}
	}

	internal abstract int GetChannelCountCore();

	int DUCE.IResource.GetChannelCount()
	{
		return GetChannelCountCore();
	}

	internal abstract DUCE.Channel GetChannelCore(int index);

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return GetChannelCore(index);
	}

	/// <summary>Creates a string representation of the object based on the current culture. </summary>
	/// <returns>A string representation of the object.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of the object using the specified culture-specific formatting information. </summary>
	/// <returns>A string representation of the object.</returns>
	/// <param name="provider">Culture-specific formatting information, or null to use the current culture.</param>
	public string ToString(IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(null, provider);
	}

	/// <summary>Formats the value of the current instance using the specified format.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="provider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(format, provider);
	}

	internal virtual string ConvertToString(string format, IFormatProvider provider)
	{
		return base.ToString();
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Geometry" /> instance from the specified string using the current culture.</summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Geometry" /> instance created from the specified string.</returns>
	/// <param name="source">A string that describes the geometry to be created.</param>
	public static Geometry Parse(string source)
	{
		IFormatProvider invariantEnglishUS = System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS;
		return Parsers.ParseGeometry(source, invariantEnglishUS);
	}

	static Geometry()
	{
		s_empty = MakeEmptyGeometry();
		s_emptyPathGeometryData = MakeEmptyPathGeometryData();
		s_Transform = Transform.Identity;
		Type typeFromHandle = typeof(Geometry);
		TransformProperty = Animatable.RegisterProperty("Transform", typeof(Transform), typeFromHandle, Transform.Identity, TransformPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
