using System.Collections.Generic;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.Media3D;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Renders the 2-D children within the specified 3-D viewport bounds.</summary>
[ContentProperty("Visual")]
public sealed class Viewport2DVisual3D : Visual3D
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Viewport2DVisual3D.Visual" /> dependency property.</summary>
	public static readonly DependencyProperty VisualProperty = VisualBrush.VisualProperty.AddOwner(typeof(Viewport2DVisual3D), new PropertyMetadata(null, OnVisualChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Viewport2DVisual3D.Geometry" /> dependency property.</summary>
	public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register("Geometry", typeof(Geometry3D), typeof(Viewport2DVisual3D), new PropertyMetadata(null, OnGeometryChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Viewport2DVisual3D.Material" /> dependency property.</summary>
	public static readonly DependencyProperty MaterialProperty = DependencyProperty.Register("Material", typeof(Material), typeof(Viewport2DVisual3D), new PropertyMetadata(null, OnMaterialPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Viewport2DVisual3D.IsVisualHostMaterial" /> attached property.</summary>
	public static readonly DependencyProperty IsVisualHostMaterialProperty = DependencyProperty.RegisterAttached("IsVisualHostMaterial", typeof(bool), typeof(Viewport2DVisual3D), new PropertyMetadata(BooleanBoxes.FalseBox));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Viewport2DVisual3D.CacheMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.Viewport2DVisual3D.CacheMode" /> dependency property.</returns>
	public static readonly DependencyProperty CacheModeProperty = DependencyProperty.Register("CacheMode", typeof(CacheMode), typeof(Viewport2DVisual3D), new PropertyMetadata(null, OnCacheModeChanged));

	private static readonly DependencyProperty CachingHintProperty = RenderOptions.CachingHintProperty.AddOwner(typeof(Viewport2DVisual3D), new UIPropertyMetadata(OnCachingHintChanged));

	private static readonly DependencyProperty CacheInvalidationThresholdMinimumProperty = RenderOptions.CacheInvalidationThresholdMinimumProperty.AddOwner(typeof(Viewport2DVisual3D), new UIPropertyMetadata(OnCacheInvalidationThresholdMinimumChanged));

	private static readonly DependencyProperty CacheInvalidationThresholdMaximumProperty = RenderOptions.CacheInvalidationThresholdMaximumProperty.AddOwner(typeof(Viewport2DVisual3D), new UIPropertyMetadata(OnCacheInvalidationThresholdMaximumChanged));

	private VisualBrush _visualBrush;

	private BitmapCacheBrush _bitmapCacheBrush;

	private Point3DCollection _positionsCache;

	private PointCollection _textureCoordinatesCache;

	private Int32Collection _triangleIndicesCache;

	/// <summary>Gets or sets the 2-D visual to be placed on the 3-D object.</summary>
	/// <returns>The visual to be placed on the 3-D object.</returns>
	public Visual Visual
	{
		get
		{
			return (Visual)GetValue(VisualProperty);
		}
		set
		{
			SetValue(VisualProperty, value);
		}
	}

	private VisualBrush InternalVisualBrush
	{
		get
		{
			return _visualBrush;
		}
		set
		{
			_visualBrush = value;
		}
	}

	private BitmapCacheBrush InternalBitmapCacheBrush
	{
		get
		{
			return _bitmapCacheBrush;
		}
		set
		{
			_bitmapCacheBrush = value;
		}
	}

	/// <summary>Gets or sets the 3-D geometry for this <see cref="T:System.Windows.Media.Media3D.Viewport2DVisual3D" />.</summary>
	/// <returns>The 3-D geometry for this <see cref="T:System.Windows.Media.Media3D.Viewport2DVisual3D" />.</returns>
	public Geometry3D Geometry
	{
		get
		{
			return (Geometry3D)GetValue(GeometryProperty);
		}
		set
		{
			SetValue(GeometryProperty, value);
		}
	}

	internal Point3DCollection InternalPositionsCache
	{
		get
		{
			if (_positionsCache == null && Geometry is MeshGeometry3D meshGeometry3D)
			{
				_positionsCache = meshGeometry3D.Positions;
				if (_positionsCache != null)
				{
					_positionsCache = (Point3DCollection)_positionsCache.GetCurrentValueAsFrozen();
				}
			}
			return _positionsCache;
		}
		set
		{
			_positionsCache = value;
		}
	}

	internal PointCollection InternalTextureCoordinatesCache
	{
		get
		{
			if (_textureCoordinatesCache == null && Geometry is MeshGeometry3D meshGeometry3D)
			{
				_textureCoordinatesCache = meshGeometry3D.TextureCoordinates;
				if (_textureCoordinatesCache != null)
				{
					_textureCoordinatesCache = (PointCollection)_textureCoordinatesCache.GetCurrentValueAsFrozen();
				}
			}
			return _textureCoordinatesCache;
		}
		set
		{
			_textureCoordinatesCache = value;
		}
	}

	internal Int32Collection InternalTriangleIndicesCache
	{
		get
		{
			if (_triangleIndicesCache == null && Geometry is MeshGeometry3D meshGeometry3D)
			{
				_triangleIndicesCache = meshGeometry3D.TriangleIndices;
				if (_triangleIndicesCache != null)
				{
					_triangleIndicesCache = (Int32Collection)_triangleIndicesCache.GetCurrentValueAsFrozen();
				}
			}
			return _triangleIndicesCache;
		}
		set
		{
			_triangleIndicesCache = value;
		}
	}

	/// <summary>Gets or sets the material that describes the appearance of the 3-D object.</summary>
	/// <returns>The material for the 3-D object.</returns>
	public Material Material
	{
		get
		{
			return (Material)GetValue(MaterialProperty);
		}
		set
		{
			SetValue(MaterialProperty, value);
		}
	}

	/// <summary>Gets or sets a cached representation of the <see cref="T:System.Windows.Media.Media3D.Viewport2DVisual3D" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.CacheMode" /> that holds a cached representation of the <see cref="T:System.Windows.Media.Media3D.Viewport2DVisual3D" />.</returns>
	public CacheMode CacheMode
	{
		get
		{
			return (CacheMode)GetValue(CacheModeProperty);
		}
		set
		{
			SetValue(CacheModeProperty, value);
		}
	}

	protected override int Visual3DChildrenCount => 0;

	internal override int InternalVisual2DOr3DChildrenCount => (Visual != null) ? 1 : 0;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Viewport2DVisual3D" /> class.</summary>
	public Viewport2DVisual3D()
	{
		_visualBrush = CreateVisualBrush();
		_bitmapCacheBrush = CreateBitmapCacheBrush();
		base.Visual3DModel = new GeometryModel3D
		{
			CanBeInheritanceContext = false
		};
	}

	internal static bool Get3DPointFor2DCoordinate(Point point, out Point3D point3D, Point3DCollection positions, PointCollection textureCoords, Int32Collection triIndices)
	{
		point3D = default(Point3D);
		Point3D[] array = new Point3D[3];
		Point[] array2 = new Point[3];
		if (positions != null && textureCoords != null)
		{
			if (triIndices == null || triIndices.Count == 0)
			{
				int count = textureCoords.Count;
				int count2 = positions.Count;
				count2 -= count2 % 3;
				for (int i = 0; i < count2; i += 3)
				{
					for (int j = 0; j < 3; j++)
					{
						array[j] = positions[i + j];
						if (i + j < count)
						{
							array2[j] = textureCoords[i + j];
						}
						else
						{
							array2[j] = new Point(0.0, 0.0);
						}
					}
					if (M3DUtil.IsPointInTriangle(point, array2, array, out point3D))
					{
						return true;
					}
				}
			}
			else
			{
				int count3 = positions.Count;
				int count4 = textureCoords.Count;
				int k = 2;
				for (int count5 = triIndices.Count; k < count5; k += 3)
				{
					bool flag = true;
					for (int l = 0; l < 3; l++)
					{
						int num = triIndices[k - 2 + l];
						if (num < 0 || num >= count3)
						{
							return false;
						}
						if (num < 0 || num >= count4)
						{
							flag = false;
							break;
						}
						array[l] = positions[num];
						array2[l] = textureCoords[num];
					}
					if (flag && M3DUtil.IsPointInTriangle(point, array2, array, out point3D))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	internal static Point TextureCoordsToVisualCoords(Point uv, Visual visual)
	{
		return TextureCoordsToVisualCoords(uv, visual.CalculateSubgraphRenderBoundsOuterSpace());
	}

	internal static Point TextureCoordsToVisualCoords(Point uv, Rect descBounds)
	{
		return new Point(uv.X * descBounds.Width + descBounds.Left, uv.Y * descBounds.Height + descBounds.Top);
	}

	internal static bool GetIntersectionInfo(RayHitTestResult rayHitResult, out Point outputPoint)
	{
		bool result = false;
		outputPoint = default(Point);
		if (rayHitResult is RayMeshGeometry3DHitTestResult { MeshHit: var meshHit, VertexWeight1: var vertexWeight, VertexWeight2: var vertexWeight2, VertexWeight3: var vertexWeight3, VertexIndex1: var vertexIndex, VertexIndex2: var vertexIndex2, VertexIndex3: var vertexIndex3 })
		{
			PointCollection textureCoordinates = meshHit.TextureCoordinates;
			if (textureCoordinates != null && vertexIndex < textureCoordinates.Count && vertexIndex2 < textureCoordinates.Count && vertexIndex3 < textureCoordinates.Count)
			{
				Point point = textureCoordinates[vertexIndex];
				Point point2 = textureCoordinates[vertexIndex2];
				Point point3 = textureCoordinates[vertexIndex3];
				outputPoint = new Point(point.X * vertexWeight + point2.X * vertexWeight2 + point3.X * vertexWeight3, point.Y * vertexWeight + point2.Y * vertexWeight2 + point3.Y * vertexWeight3);
				result = true;
			}
		}
		return result;
	}

	internal static Point VisualCoordsToTextureCoords(Point pt, Visual visual)
	{
		return VisualCoordsToTextureCoords(pt, visual.CalculateSubgraphRenderBoundsOuterSpace());
	}

	internal static Point VisualCoordsToTextureCoords(Point pt, Rect descBounds)
	{
		return new Point((pt.X - descBounds.Left) / (descBounds.Right - descBounds.Left), (pt.Y - descBounds.Top) / (descBounds.Bottom - descBounds.Top));
	}

	private void GenerateMaterial()
	{
		Material material = Material;
		if (material != null)
		{
			material = material.CloneCurrentValue();
		}
		((GeometryModel3D)base.Visual3DModel).Material = material;
		if (material != null)
		{
			SwapInCyclicBrush(material);
		}
	}

	internal static void OnVisualChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		Viewport2DVisual3D viewport2DVisual3D = (Viewport2DVisual3D)sender;
		Visual visual = (Visual)e.OldValue;
		Visual visual2 = (Visual)e.NewValue;
		if (visual != visual2)
		{
			if (viewport2DVisual3D.CacheMode is BitmapCache)
			{
				viewport2DVisual3D.InternalBitmapCacheBrush.Target = visual2;
				return;
			}
			viewport2DVisual3D.RemoveVisualChild(visual);
			viewport2DVisual3D.AddVisualChild(visual2);
			viewport2DVisual3D.InternalVisualBrush.Visual = visual2;
		}
	}

	private void AddVisualChild(Visual child)
	{
		if (child != null)
		{
			if (child._parent != null)
			{
				throw new ArgumentException(SR.Visual_HasParent);
			}
			child._parent = this;
			OnVisualChildrenChanged(child, null);
			child.FireOnVisualParentChanged(null);
		}
	}

	private void RemoveVisualChild(Visual child)
	{
		if (child != null && child._parent != null)
		{
			if (child._parent != this)
			{
				throw new ArgumentException(SR.Visual_NotChild);
			}
			child._parent = null;
			child.FireOnVisualParentChanged(this);
			OnVisualChildrenChanged(null, child);
		}
	}

	private VisualBrush CreateVisualBrush()
	{
		VisualBrush obj = new VisualBrush
		{
			CanBeInheritanceContext = false,
			ViewportUnits = BrushMappingMode.Absolute,
			TileMode = TileMode.None
		};
		RenderOptions.SetCachingHint(obj, (CachingHint)GetValue(CachingHintProperty));
		RenderOptions.SetCacheInvalidationThresholdMinimum(obj, (double)GetValue(CacheInvalidationThresholdMinimumProperty));
		RenderOptions.SetCacheInvalidationThresholdMaximum(obj, (double)GetValue(CacheInvalidationThresholdMaximumProperty));
		return obj;
	}

	private BitmapCacheBrush CreateBitmapCacheBrush()
	{
		return new BitmapCacheBrush
		{
			CanBeInheritanceContext = false,
			AutoWrapTarget = true,
			BitmapCache = (CacheMode as BitmapCache)
		};
	}

	private void SwapInCyclicBrush(Material material)
	{
		int num = 0;
		Stack<Material> stack = new Stack<Material>();
		stack.Push(material);
		Brush brush = ((CacheMode is BitmapCache) ? ((Brush)InternalBitmapCacheBrush) : ((Brush)InternalVisualBrush));
		while (stack.Count > 0)
		{
			Material material2 = stack.Pop();
			if (material2 is DiffuseMaterial)
			{
				DiffuseMaterial diffuseMaterial = (DiffuseMaterial)material2;
				if ((bool)diffuseMaterial.GetValue(IsVisualHostMaterialProperty))
				{
					diffuseMaterial.Brush = brush;
					num++;
				}
			}
			else if (material2 is EmissiveMaterial)
			{
				EmissiveMaterial emissiveMaterial = (EmissiveMaterial)material2;
				if ((bool)emissiveMaterial.GetValue(IsVisualHostMaterialProperty))
				{
					emissiveMaterial.Brush = brush;
					num++;
				}
			}
			else if (material2 is SpecularMaterial)
			{
				SpecularMaterial specularMaterial = (SpecularMaterial)material2;
				if ((bool)specularMaterial.GetValue(IsVisualHostMaterialProperty))
				{
					specularMaterial.Brush = brush;
					num++;
				}
			}
			else if (material2 is MaterialGroup)
			{
				MaterialGroup obj = (MaterialGroup)material2;
				if ((bool)obj.GetValue(IsVisualHostMaterialProperty))
				{
					throw new ArgumentException(SR.Viewport2DVisual3D_MaterialGroupIsInteractiveMaterial, "material");
				}
				MaterialCollection children = obj.Children;
				if (children != null)
				{
					int i = 0;
					for (int count = children.Count; i < count; i++)
					{
						Material item = children[i];
						stack.Push(item);
					}
				}
			}
			else
			{
				Invariant.Assert(condition: true, "Unexpected Material type encountered.  V2DV3D handles DiffuseMaterial, EmissiveMaterial, SpecularMaterial, and MaterialGroup.");
			}
		}
		if (num > 1)
		{
			throw new ArgumentException(SR.Viewport2DVisual3D_MultipleInteractiveMaterials, "material");
		}
	}

	internal static void OnGeometryChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		Viewport2DVisual3D viewport2DVisual3D = (Viewport2DVisual3D)sender;
		viewport2DVisual3D.InvalidateAllCachedValues();
		if (!e.IsASubPropertyChange)
		{
			((GeometryModel3D)viewport2DVisual3D.Visual3DModel).Geometry = viewport2DVisual3D.Geometry;
		}
	}

	private void InvalidateAllCachedValues()
	{
		InternalPositionsCache = null;
		InternalTextureCoordinatesCache = null;
		InternalTriangleIndicesCache = null;
	}

	internal static void OnMaterialPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		((Viewport2DVisual3D)sender).GenerateMaterial();
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Media.Media3D.Viewport2DVisual3D.IsVisualHostMaterial" /> attached property to a specified element.</summary>
	/// <param name="element">The element to which the attached property is written.</param>
	/// <param name="value">The required <see cref="P:System.Windows.Media.Media3D.Viewport2DVisual3D.IsVisualHostMaterial" /> value.</param>
	public static void SetIsVisualHostMaterial(Material element, bool value)
	{
		element.SetValue(IsVisualHostMaterialProperty, BooleanBoxes.Box(value));
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Media.Media3D.Viewport2DVisual3D.IsVisualHostMaterial" /> attached property for a specified <see cref="T:System.Windows.UIElement" />.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Viewport2DVisual3D.IsVisualHostMaterial" /> property value for the element.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	public static bool GetIsVisualHostMaterial(Material element)
	{
		return (bool)element.GetValue(IsVisualHostMaterialProperty);
	}

	internal static void OnCacheModeChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		Viewport2DVisual3D viewport2DVisual3D = (Viewport2DVisual3D)sender;
		BitmapCache bitmapCache = ((CacheMode)e.OldValue) as BitmapCache;
		BitmapCache bitmapCache2 = ((CacheMode)e.NewValue) as BitmapCache;
		if (bitmapCache != bitmapCache2)
		{
			viewport2DVisual3D.InternalBitmapCacheBrush.BitmapCache = bitmapCache2;
			if (bitmapCache == null)
			{
				viewport2DVisual3D.RemoveVisualChild(viewport2DVisual3D.Visual);
				viewport2DVisual3D.AddVisualChild(viewport2DVisual3D.InternalBitmapCacheBrush.InternalTarget);
				viewport2DVisual3D.InternalVisualBrush.Visual = null;
				viewport2DVisual3D.InternalBitmapCacheBrush.Target = viewport2DVisual3D.Visual;
			}
			if (bitmapCache2 == null)
			{
				viewport2DVisual3D.InternalBitmapCacheBrush.Target = null;
				viewport2DVisual3D.InternalVisualBrush.Visual = viewport2DVisual3D.Visual;
				viewport2DVisual3D.RemoveVisualChild(viewport2DVisual3D.InternalBitmapCacheBrush.InternalTarget);
				viewport2DVisual3D.AddVisualChild(viewport2DVisual3D.Visual);
			}
			if (bitmapCache == null || bitmapCache2 == null)
			{
				viewport2DVisual3D.GenerateMaterial();
			}
		}
	}

	protected override Visual3D GetVisual3DChild(int index)
	{
		throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
	}

	internal override DependencyObject InternalGet2DOr3DVisualChild(int index)
	{
		Visual visual = Visual;
		if (index != 0 || visual == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return visual;
	}

	private static void OnCachingHintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RenderOptions.SetCachingHint(((Viewport2DVisual3D)d)._visualBrush, (CachingHint)e.NewValue);
	}

	private static void OnCacheInvalidationThresholdMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RenderOptions.SetCacheInvalidationThresholdMinimum(((Viewport2DVisual3D)d)._visualBrush, (double)e.NewValue);
	}

	private static void OnCacheInvalidationThresholdMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RenderOptions.SetCacheInvalidationThresholdMaximum(((Viewport2DVisual3D)d)._visualBrush, (double)e.NewValue);
	}
}
