using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Provides 2-D to 3-D transformation support.</summary>
public class GeneralTransform2DTo3D : Freezable
{
	private GeneralTransform _transform2D;

	private GeneralTransform3D _transform3D;

	private Point3DCollection _positions;

	private PointCollection _textureCoords;

	private Int32Collection _triIndices;

	private Rect _childBounds;

	internal GeneralTransform2DTo3D()
	{
	}

	internal GeneralTransform2DTo3D(GeneralTransform transform2D, Viewport2DVisual3D containingVisual3D, GeneralTransform3D transform3D)
	{
		Visual visual = containingVisual3D.Visual;
		_transform3D = (GeneralTransform3D)transform3D.GetCurrentValueAsFrozen();
		GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
		generalTransformGroup.Children.Add((GeneralTransform)transform2D.GetCurrentValueAsFrozen());
		generalTransformGroup.Children.Add((GeneralTransform)visual.TransformToOuterSpace().GetCurrentValueAsFrozen());
		generalTransformGroup.Freeze();
		_transform2D = generalTransformGroup;
		_positions = containingVisual3D.InternalPositionsCache;
		_textureCoords = containingVisual3D.InternalTextureCoordinatesCache;
		_triIndices = containingVisual3D.InternalTriangleIndicesCache;
		_childBounds = visual.CalculateSubgraphRenderBoundsOuterSpace();
	}

	/// <summary>Attempts to transform the specified point and returns a value that indicates whether the transformation was successful.</summary>
	/// <returns>true if <paramref name="inPoint" /> was transformed; otherwise, false.</returns>
	/// <param name="inPoint">The point to transform.</param>
	/// <param name="result">The result of transforming <paramref name="inPoint" />.</param>
	public bool TryTransform(Point inPoint, out Point3D result)
	{
		result = default(Point3D);
		if (!_transform2D.TryTransform(inPoint, out var result2))
		{
			return false;
		}
		if (!Viewport2DVisual3D.Get3DPointFor2DCoordinate(Viewport2DVisual3D.VisualCoordsToTextureCoords(result2, _childBounds), out var point3D, _positions, _textureCoords, _triIndices))
		{
			return false;
		}
		if (!_transform3D.TryTransform(point3D, out result))
		{
			return false;
		}
		return true;
	}

	/// <summary>Transforms the specified point and returns the result.</summary>
	/// <returns>The result of transforming <paramref name="point" />.</returns>
	/// <param name="point">The point to transform.</param>
	/// <exception cref="T:System.InvalidOperationException">The transform did not succeed.</exception>
	public Point3D Transform(Point point)
	{
		if (!TryTransform(point, out var result))
		{
			throw new InvalidOperationException(SR.Format(SR.GeneralTransform_TransformFailed, null));
		}
		return result;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new GeneralTransform2DTo3D();
	}

	/// <summary>Makes the instance a clone (deep copy) of the specified <see cref="T:System.Windows.Freezable" /> using base (non-animated) property values.</summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		GeneralTransform2DTo3D transform = (GeneralTransform2DTo3D)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(transform);
	}

	/// <summary>Makes the instance a modifiable clone (deep copy) of the specified <see cref="T:System.Windows.Freezable" /> using current property values.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Freezable" /> to be cloned.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		GeneralTransform2DTo3D transform = (GeneralTransform2DTo3D)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(transform);
	}

	/// <summary>Makes the instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" /> using base (non-animated) property values.</summary>
	/// <param name="sourceFreezable">The instance to copy.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		GeneralTransform2DTo3D transform = (GeneralTransform2DTo3D)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(transform);
	}

	/// <summary>Makes the current instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" />. If the object has animated dependency properties, their current animated values are copied.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Freezable" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		GeneralTransform2DTo3D transform = (GeneralTransform2DTo3D)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(transform);
	}

	private void CopyCommon(GeneralTransform2DTo3D transform)
	{
		_transform2D = transform._transform2D;
		_transform3D = transform._transform3D;
		_positions = transform._positions;
		_textureCoords = transform._textureCoords;
		_triIndices = transform._triIndices;
		_childBounds = transform._childBounds;
	}
}
