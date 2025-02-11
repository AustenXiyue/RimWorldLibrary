using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.Media3D;
using MS.Utility;

namespace System.Windows.Media.Media3D;

/// <summary>Triangle primitive for building a 3-D shape. </summary>
public sealed class MeshGeometry3D : Geometry3D
{
	private Rect3D _cachedBounds = Rect3D.Empty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.MeshGeometry3D.Positions" /> dependency property. </summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Media3D.MeshGeometry3D.Positions" /> dependency property.</returns>
	public static readonly DependencyProperty PositionsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.MeshGeometry3D.Normals" /> dependency property.</summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Media3D.MeshGeometry3D.Normals" /> dependency property.</returns>
	public static readonly DependencyProperty NormalsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.MeshGeometry3D.TextureCoordinates" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.MeshGeometry3D.TextureCoordinates" /> dependency property.</returns>
	public static readonly DependencyProperty TextureCoordinatesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.MeshGeometry3D.TriangleIndices" /> dependency property. </summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Media3D.MeshGeometry3D.TriangleIndices" /> dependency property.</returns>
	public static readonly DependencyProperty TriangleIndicesProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Point3DCollection s_Positions;

	internal static Vector3DCollection s_Normals;

	internal static PointCollection s_TextureCoordinates;

	internal static Int32Collection s_TriangleIndices;

	/// <summary> Gets the bounding <see cref="T:System.Windows.Media.Media3D.Rect3D" /> for this <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" />.</summary>
	/// <returns>Bounding <see cref="T:System.Windows.Media.Media3D.Rect3D" /> for the <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" />.</returns>
	public override Rect3D Bounds
	{
		get
		{
			ReadPreamble();
			if (_cachedBounds.IsEmpty)
			{
				UpdateCachedBounds();
			}
			return _cachedBounds;
		}
	}

	/// <summary>Gets or sets a collection of vertex positions for a <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" />.  </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Point3DCollection" /> that contains the vertex positions of the MeshGeometry3D.</returns>
	public Point3DCollection Positions
	{
		get
		{
			return (Point3DCollection)GetValue(PositionsProperty);
		}
		set
		{
			SetValueInternal(PositionsProperty, value);
		}
	}

	/// <summary>Gets or sets a collection of normal vectors for the <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" />.  </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Vector3DCollection" /> that contains the normal vectors for the MeshGeometry3D.</returns>
	public Vector3DCollection Normals
	{
		get
		{
			return (Vector3DCollection)GetValue(NormalsProperty);
		}
		set
		{
			SetValueInternal(NormalsProperty, value);
		}
	}

	/// <summary>Gets or sets a collection of texture coordinates for the <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" />.  </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.PointCollection" /> that contains the texture coordinates for the MeshGeometry3D.</returns>
	public PointCollection TextureCoordinates
	{
		get
		{
			return (PointCollection)GetValue(TextureCoordinatesProperty);
		}
		set
		{
			SetValueInternal(TextureCoordinatesProperty, value);
		}
	}

	/// <summary>Gets or sets a collection of triangle indices for the <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" />.  </summary>
	/// <returns>Collection that contains the triangle indices of the MeshGeometry3D.</returns>
	public Int32Collection TriangleIndices
	{
		get
		{
			return (Int32Collection)GetValue(TriangleIndicesProperty);
		}
		set
		{
			SetValueInternal(TriangleIndicesProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 4;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" /> class.</summary>
	public MeshGeometry3D()
	{
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		if ((e.IsAValueChange || e.IsASubPropertyChange) && e.Property == PositionsProperty)
		{
			SetCachedBoundsDirty();
		}
		base.OnPropertyChanged(e);
	}

	internal Rect GetTextureCoordinateBounds()
	{
		PointCollection textureCoordinates = TextureCoordinates;
		int num = textureCoordinates?.Count ?? 0;
		if (num > 0)
		{
			Point point = textureCoordinates[0];
			Point point2 = textureCoordinates[0];
			for (int i = 1; i < num; i++)
			{
				Point point3 = textureCoordinates.Internal_GetItem(i);
				double x = point3.X;
				if (point.X > x)
				{
					point.X = x;
				}
				else if (point2.X < x)
				{
					point2.X = x;
				}
				double y = point3.Y;
				if (point.Y > y)
				{
					point.Y = y;
				}
				else if (point2.Y < y)
				{
					point2.Y = y;
				}
			}
			return new Rect(point, point2);
		}
		return Rect.Empty;
	}

	internal override void RayHitTestCore(RayHitTestParameters rayParams, FaceType hitTestableFaces)
	{
		Point3DCollection positions = Positions;
		if (positions == null)
		{
			return;
		}
		rayParams.GetLocalLine(out var origin, out var direction);
		Int32Collection triangleIndices = TriangleIndices;
		FaceType type = ((!rayParams.IsRay) ? (FaceType.Front | FaceType.Back) : hitTestableFaces);
		if (triangleIndices == null || triangleIndices.Count == 0)
		{
			FrugalStructList<Point3D> collection = positions._collection;
			for (int num = collection.Count - collection.Count % 3 - 1; num >= 2; num -= 3)
			{
				int num2 = num - 2;
				int num3 = num - 1;
				int num4 = num;
				Point3D v = collection[num2];
				Point3D v2 = collection[num3];
				Point3D v3 = collection[num4];
				if (LineUtil.ComputeLineTriangleIntersection(type, ref origin, ref direction, ref v, ref v2, ref v3, out var hitCoord, out var dist))
				{
					if (rayParams.IsRay)
					{
						ValidateRayHit(rayParams, ref origin, ref direction, dist, num2, num3, num4, ref hitCoord);
					}
					else
					{
						ValidateLineHit(rayParams, hitTestableFaces, num2, num3, num4, ref v, ref v2, ref v3, ref hitCoord);
					}
				}
			}
			return;
		}
		FrugalStructList<Point3D> collection2 = positions._collection;
		FrugalStructList<int> collection3 = triangleIndices._collection;
		int count = collection3.Count;
		int count2 = collection2.Count;
		for (int i = 2; i < count; i += 3)
		{
			int num5 = collection3[i - 2];
			int num6 = collection3[i - 1];
			int num7 = collection3[i];
			if (0 > num5 || num5 >= count2 || 0 > num6 || num6 >= count2 || 0 > num7 || num7 >= count2)
			{
				break;
			}
			Point3D v4 = collection2[num5];
			Point3D v5 = collection2[num6];
			Point3D v6 = collection2[num7];
			if (LineUtil.ComputeLineTriangleIntersection(type, ref origin, ref direction, ref v4, ref v5, ref v6, out var hitCoord2, out var dist2))
			{
				if (rayParams.IsRay)
				{
					ValidateRayHit(rayParams, ref origin, ref direction, dist2, num5, num6, num7, ref hitCoord2);
				}
				else
				{
					ValidateLineHit(rayParams, hitTestableFaces, num5, num6, num7, ref v4, ref v5, ref v6, ref hitCoord2);
				}
			}
		}
	}

	private void ValidateRayHit(RayHitTestParameters rayParams, ref Point3D origin, ref Vector3D direction, double hitTime, int i0, int i1, int i2, ref Point barycentric)
	{
		if (!(hitTime > 0.0))
		{
			return;
		}
		Matrix3D matrix3D = (rayParams.HasWorldTransformMatrix ? rayParams.WorldTransformMatrix : Matrix3D.Identity);
		Point3D point = origin + hitTime * direction;
		Point3D point2 = point;
		matrix3D.MultiplyPoint(ref point2);
		if (rayParams.HasHitTestProjectionMatrix)
		{
			Matrix3D hitTestProjectionMatrix = rayParams.HitTestProjectionMatrix;
			double num = point2.X * hitTestProjectionMatrix.M13 + point2.Y * hitTestProjectionMatrix.M23 + point2.Z * hitTestProjectionMatrix.M33 + hitTestProjectionMatrix.OffsetZ;
			double num2 = point2.X * hitTestProjectionMatrix.M14 + point2.Y * hitTestProjectionMatrix.M24 + point2.Z * hitTestProjectionMatrix.M34 + hitTestProjectionMatrix.M44;
			if (!(num / num2 <= 1.0))
			{
				return;
			}
		}
		double length = (point2 - rayParams.Origin).Length;
		if (rayParams.HasModelTransformMatrix)
		{
			rayParams.ModelTransformMatrix.MultiplyPoint(ref point);
		}
		rayParams.ReportResult(this, point, length, i0, i1, i2, barycentric);
	}

	private void ValidateLineHit(RayHitTestParameters rayParams, FaceType facesToHit, int i0, int i1, int i2, ref Point3D v0, ref Point3D v1, ref Point3D v2, ref Point barycentric)
	{
		Matrix3D matrix3D = (rayParams.HasWorldTransformMatrix ? rayParams.WorldTransformMatrix : Matrix3D.Identity);
		Point3D point = M3DUtil.Interpolate(ref v0, ref v1, ref v2, ref barycentric);
		Point3D point2 = point;
		matrix3D.MultiplyPoint(ref point2);
		Vector3D vector = point2 - rayParams.Origin;
		if (!(Vector3D.DotProduct(rayParams.Direction, vector) > 0.0))
		{
			return;
		}
		if (rayParams.HasHitTestProjectionMatrix)
		{
			Matrix3D hitTestProjectionMatrix = rayParams.HitTestProjectionMatrix;
			double num = point2.X * hitTestProjectionMatrix.M13 + point2.Y * hitTestProjectionMatrix.M23 + point2.Z * hitTestProjectionMatrix.M33 + hitTestProjectionMatrix.OffsetZ;
			double num2 = point2.X * hitTestProjectionMatrix.M14 + point2.Y * hitTestProjectionMatrix.M24 + point2.Z * hitTestProjectionMatrix.M34 + hitTestProjectionMatrix.M44;
			if (!(num / num2 <= 1.0))
			{
				return;
			}
		}
		Point3D point3 = v0;
		Point3D point4 = v1;
		Point3D point5 = v2;
		matrix3D.MultiplyPoint(ref point3);
		matrix3D.MultiplyPoint(ref point4);
		matrix3D.MultiplyPoint(ref point5);
		double num3 = 0.0 - Vector3D.DotProduct(Vector3D.CrossProduct(point4 - point3, point5 - point3), vector);
		double determinant = matrix3D.Determinant;
		bool flag = num3 > 0.0 == determinant >= 0.0;
		if (((facesToHit & FaceType.Front) == FaceType.Front && flag) || ((facesToHit & FaceType.Back) == FaceType.Back && !flag))
		{
			double length = vector.Length;
			if (rayParams.HasModelTransformMatrix)
			{
				rayParams.ModelTransformMatrix.MultiplyPoint(ref point);
			}
			rayParams.ReportResult(this, point, length, i0, i1, i2, barycentric);
		}
	}

	private void UpdateCachedBounds()
	{
		_cachedBounds = M3DUtil.ComputeAxisAlignedBoundingBox(Positions);
	}

	private void SetCachedBoundsDirty()
	{
		_cachedBounds = Rect3D.Empty;
	}

	[Conditional("DEBUG")]
	private void Debug_VerifyCachedBounds()
	{
		Rect3D rect3D = M3DUtil.ComputeAxisAlignedBoundingBox(Positions);
		if (_cachedBounds.X < rect3D.X || _cachedBounds.X > rect3D.X || _cachedBounds.Y < rect3D.Y || _cachedBounds.Y > rect3D.Y || _cachedBounds.Z < rect3D.Z || _cachedBounds.Z > rect3D.Z || _cachedBounds.SizeX < rect3D.SizeX || _cachedBounds.SizeX > rect3D.SizeX || _cachedBounds.SizeY < rect3D.SizeY || _cachedBounds.SizeY > rect3D.SizeY || _cachedBounds.SizeZ < rect3D.SizeZ || _cachedBounds.SizeZ > rect3D.SizeZ)
		{
			_ = _cachedBounds == Rect3D.Empty;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new MeshGeometry3D Clone()
	{
		return (MeshGeometry3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.MeshGeometry3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new MeshGeometry3D CloneCurrentValue()
	{
		return (MeshGeometry3D)base.CloneCurrentValue();
	}

	private static void PositionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MeshGeometry3D)d).PropertyChanged(PositionsProperty);
	}

	private static void NormalsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MeshGeometry3D)d).PropertyChanged(NormalsProperty);
	}

	private static void TextureCoordinatesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MeshGeometry3D)d).PropertyChanged(TextureCoordinatesProperty);
	}

	private static void TriangleIndicesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MeshGeometry3D)d).PropertyChanged(TriangleIndicesProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new MeshGeometry3D();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Point3DCollection positions = Positions;
			Vector3DCollection normals = Normals;
			PointCollection textureCoordinates = TextureCoordinates;
			Int32Collection triangleIndices = TriangleIndices;
			int num = positions?.Count ?? 0;
			int num2 = normals?.Count ?? 0;
			int num3 = textureCoordinates?.Count ?? 0;
			int num4 = triangleIndices?.Count ?? 0;
			DUCE.MILCMD_MESHGEOMETRY3D mILCMD_MESHGEOMETRY3D = default(DUCE.MILCMD_MESHGEOMETRY3D);
			mILCMD_MESHGEOMETRY3D.Type = MILCMD.MilCmdMeshGeometry3D;
			mILCMD_MESHGEOMETRY3D.Handle = _duceResource.GetHandle(channel);
			mILCMD_MESHGEOMETRY3D.PositionsSize = (uint)(sizeof(MilPoint3F) * num);
			mILCMD_MESHGEOMETRY3D.NormalsSize = (uint)(sizeof(MilPoint3F) * num2);
			mILCMD_MESHGEOMETRY3D.TextureCoordinatesSize = (uint)(sizeof(Point) * num3);
			mILCMD_MESHGEOMETRY3D.TriangleIndicesSize = (uint)(4 * num4);
			channel.BeginCommand((byte*)(&mILCMD_MESHGEOMETRY3D), sizeof(DUCE.MILCMD_MESHGEOMETRY3D), (int)(mILCMD_MESHGEOMETRY3D.PositionsSize + mILCMD_MESHGEOMETRY3D.NormalsSize + mILCMD_MESHGEOMETRY3D.TextureCoordinatesSize + mILCMD_MESHGEOMETRY3D.TriangleIndicesSize));
			for (int i = 0; i < num; i++)
			{
				MilPoint3F milPoint3F = CompositionResourceManager.Point3DToMilPoint3F(positions.Internal_GetItem(i));
				channel.AppendCommandData((byte*)(&milPoint3F), sizeof(MilPoint3F));
			}
			for (int j = 0; j < num2; j++)
			{
				MilPoint3F milPoint3F2 = CompositionResourceManager.Vector3DToMilPoint3F(normals.Internal_GetItem(j));
				channel.AppendCommandData((byte*)(&milPoint3F2), sizeof(MilPoint3F));
			}
			for (int k = 0; k < num3; k++)
			{
				Point point = textureCoordinates.Internal_GetItem(k);
				channel.AppendCommandData((byte*)(&point), sizeof(Point));
			}
			for (int l = 0; l < num4; l++)
			{
				int num5 = triangleIndices.Internal_GetItem(l);
				channel.AppendCommandData((byte*)(&num5), 4);
			}
			channel.EndCommand();
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_MESHGEOMETRY3D))
		{
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
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

	static MeshGeometry3D()
	{
		s_Positions = Point3DCollection.Empty;
		s_Normals = Vector3DCollection.Empty;
		s_TextureCoordinates = PointCollection.Empty;
		s_TriangleIndices = Int32Collection.Empty;
		Type typeFromHandle = typeof(MeshGeometry3D);
		PositionsProperty = Animatable.RegisterProperty("Positions", typeof(Point3DCollection), typeFromHandle, new FreezableDefaultValueFactory(Point3DCollection.Empty), PositionsPropertyChanged, null, isIndependentlyAnimated: false, null);
		NormalsProperty = Animatable.RegisterProperty("Normals", typeof(Vector3DCollection), typeFromHandle, new FreezableDefaultValueFactory(Vector3DCollection.Empty), NormalsPropertyChanged, null, isIndependentlyAnimated: false, null);
		TextureCoordinatesProperty = Animatable.RegisterProperty("TextureCoordinates", typeof(PointCollection), typeFromHandle, new FreezableDefaultValueFactory(PointCollection.Empty), TextureCoordinatesPropertyChanged, null, isIndependentlyAnimated: false, null);
		TriangleIndicesProperty = Animatable.RegisterProperty("TriangleIndices", typeof(Int32Collection), typeFromHandle, new FreezableDefaultValueFactory(Int32Collection.Empty), TriangleIndicesPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
