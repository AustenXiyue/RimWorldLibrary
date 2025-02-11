using MS.Internal;

namespace System.Windows.Media;

/// <summary>Specifies a <see cref="T:System.Windows.Media.Geometry" /> as the parameter to be used for hit testing a visual tree.</summary>
public class GeometryHitTestParameters : HitTestParameters
{
	private PathGeometry _hitGeometryInternal;

	private Geometry _hitGeometryCache;

	private Rect _origBounds;

	private Rect _bounds;

	private MatrixStack _matrixStack;

	/// <summary>Gets the <see cref="T:System.Windows.Media.Geometry" /> that defines the hit test geometry for this <see cref="T:System.Windows.Media.GeometryHitTestParameters" /> instance.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> that defines the hit test region.</returns>
	public Geometry HitGeometry
	{
		get
		{
			if (_hitGeometryCache == null)
			{
				_hitGeometryCache = (Geometry)_hitGeometryInternal.GetAsFrozen();
			}
			return _hitGeometryCache;
		}
	}

	internal PathGeometry InternalHitGeometry => _hitGeometryInternal;

	internal Rect Bounds => _bounds;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GeometryHitTestParameters" /> class, using the specified <see cref="T:System.Windows.Media.Geometry" />.</summary>
	/// <param name="geometry">The <see cref="T:System.Windows.Media.Geometry" /> value to use for the hit test geometry.</param>
	public GeometryHitTestParameters(Geometry geometry)
	{
		if (geometry == null)
		{
			throw new ArgumentNullException("geometry");
		}
		_hitGeometryInternal = geometry.GetAsPathGeometry();
		if (_hitGeometryInternal == geometry)
		{
			_hitGeometryInternal = _hitGeometryInternal.Clone();
		}
		Transform transform = _hitGeometryInternal.Transform;
		MatrixTransform matrixTransform = new MatrixTransform();
		_hitGeometryInternal.Transform = matrixTransform;
		_origBounds = _hitGeometryInternal.Bounds;
		if (transform != null && !transform.IsIdentity)
		{
			matrixTransform.Matrix = transform.Value;
		}
		_bounds = _hitGeometryInternal.Bounds;
		_matrixStack = new MatrixStack();
	}

	internal void PushMatrix(ref Matrix newMatrix)
	{
		MatrixTransform obj = (MatrixTransform)_hitGeometryInternal.Transform;
		Matrix matrix = obj.Value;
		_matrixStack.Push(ref matrix, combine: false);
		MatrixUtil.MultiplyMatrix(ref matrix, ref newMatrix);
		obj.Matrix = matrix;
		_bounds = Rect.Transform(_origBounds, matrix);
		ClearHitGeometryCache();
	}

	internal void PopMatrix()
	{
		Matrix matrix = _matrixStack.Peek();
		((MatrixTransform)_hitGeometryInternal.Transform).Matrix = matrix;
		_bounds = Rect.Transform(_origBounds, matrix);
		_matrixStack.Pop();
		ClearHitGeometryCache();
	}

	internal void EmergencyRestoreOriginalTransform()
	{
		Matrix matrix = ((MatrixTransform)_hitGeometryInternal.Transform).Matrix;
		while (!_matrixStack.IsEmpty)
		{
			matrix = _matrixStack.Peek();
			_matrixStack.Pop();
		}
		((MatrixTransform)_hitGeometryInternal.Transform).Matrix = matrix;
		ClearHitGeometryCache();
	}

	private void ClearHitGeometryCache()
	{
		_hitGeometryCache = null;
	}
}
