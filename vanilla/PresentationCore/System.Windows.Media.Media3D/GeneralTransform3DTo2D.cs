using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Provides 3-D to 2-D transformation support.</summary>
public class GeneralTransform3DTo2D : Freezable
{
	private Matrix3D _projectionTransform;

	private GeneralTransform _transformBetween2D;

	internal GeneralTransform3DTo2D()
	{
		_transformBetween2D = null;
	}

	internal GeneralTransform3DTo2D(Matrix3D projectionTransform, GeneralTransform transformBetween2D)
	{
		_projectionTransform = projectionTransform;
		_transformBetween2D = (GeneralTransform)transformBetween2D.GetAsFrozen();
	}

	/// <summary>Transforms the specified 3-D point and returns the result.</summary>
	/// <returns>true if <paramref name="inPoint" /> was transformed; otherwise, false.</returns>
	/// <param name="inPoint">The 3-D point to transform.</param>
	/// <param name="result">The result of transforming <paramref name="inPoint" />.</param>
	public bool TryTransform(Point3D inPoint, out Point result)
	{
		bool result2 = false;
		result = default(Point);
		Point3D point3D = _projectionTransform.Transform(inPoint);
		if (_transformBetween2D != null)
		{
			result = _transformBetween2D.Transform(new Point(point3D.X, point3D.Y));
			result2 = true;
		}
		return result2;
	}

	/// <summary>Transforms the specified 3-D point and returns the result.</summary>
	/// <returns>The result of transforming <paramref name="point" />.</returns>
	/// <param name="point">The 3-D point to transform.</param>
	/// <exception cref="T:System.InvalidOperationException">The transform did not succeed.</exception>
	public Point Transform(Point3D point)
	{
		if (!TryTransform(point, out var result))
		{
			throw new InvalidOperationException(SR.Format(SR.GeneralTransform_TransformFailed, null));
		}
		return result;
	}

	/// <summary>Transforms the specified 3-D bounding box and returns an axis-aligned bounding box that contains all of the points in the original 3-D bounding box.</summary>
	/// <returns>An axis-aligned bounding box that contains all of the points in the specified 3-D bounding box.</returns>
	/// <param name="rect3D">The 3-D bounding box to transform.</param>
	public Rect TransformBounds(Rect3D rect3D)
	{
		if (_transformBetween2D != null)
		{
			return _transformBetween2D.TransformBounds(MILUtilities.ProjectBounds(ref _projectionTransform, ref rect3D));
		}
		return Rect.Empty;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new GeneralTransform3DTo2D();
	}

	/// <summary>Makes the instance a clone (deep copy) of the specified <see cref="T:System.Windows.Freezable" /> using base (non-animated) property values.</summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		GeneralTransform3DTo2D transform = (GeneralTransform3DTo2D)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(transform);
	}

	/// <summary>Makes the instance a modifiable clone (deep copy) of the specified <see cref="T:System.Windows.Freezable" /> using current property values.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Freezable" /> to be cloned.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		GeneralTransform3DTo2D transform = (GeneralTransform3DTo2D)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(transform);
	}

	/// <summary>Makes the instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" /> using base (non-animated) property values.</summary>
	/// <param name="sourceFreezable">The instance to copy.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		GeneralTransform3DTo2D transform = (GeneralTransform3DTo2D)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(transform);
	}

	/// <summary>Makes the current instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" />. If the object has animated dependency properties, their current animated values are copied.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Freezable" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		GeneralTransform3DTo2D transform = (GeneralTransform3DTo2D)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(transform);
	}

	private void CopyCommon(GeneralTransform3DTo2D transform)
	{
		_projectionTransform = transform._projectionTransform;
		_transformBetween2D = transform._transformBetween2D;
	}
}
