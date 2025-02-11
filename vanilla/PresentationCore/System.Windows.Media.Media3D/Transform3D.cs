using System.Windows.Media.Composition;
using MS.Internal.Media3D;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Provides a parent class for all three-dimensional transformations, including translation, rotation, and scale transformations. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class Transform3D : GeneralTransform3D, DUCE.IResource
{
	private static Transform3D s_identity;

	/// <summary>Gets the inverse transformation of this object, if possible.</summary>
	/// <returns>An inverse of this instance, if possible; otherwise, null.</returns>
	public override GeneralTransform3D Inverse
	{
		get
		{
			ReadPreamble();
			Matrix3D value = Value;
			if (!value.HasInverse)
			{
				return null;
			}
			value.Invert();
			return new MatrixTransform3D(value);
		}
	}

	internal override Transform3D AffineTransform
	{
		[FriendAccessAllowed]
		get
		{
			return this;
		}
	}

	/// <summary>Gets the identity transformation. </summary>
	/// <returns>Identity transformation.</returns>
	public static Transform3D Identity
	{
		get
		{
			if (s_identity == null)
			{
				MatrixTransform3D matrixTransform3D = new MatrixTransform3D();
				matrixTransform3D.Freeze();
				s_identity = matrixTransform3D;
			}
			return s_identity;
		}
	}

	/// <summary>Gets a value that specifies whether the matrix is affine. </summary>
	/// <returns>true if the matrix is affine; otherwise, false.</returns>
	public abstract bool IsAffine { get; }

	/// <summary>Gets the <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> that represents the value of the current transformation. </summary>
	/// <returns>Matrix3D that represents the value of the current transformation.</returns>
	public abstract Matrix3D Value { get; }

	internal Transform3D()
	{
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Point3D" />. </summary>
	/// <returns>Transformed Point3D.</returns>
	/// <param name="point">Point3D to transform.</param>
	public new Point3D Transform(Point3D point)
	{
		return base.Transform(point);
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
	/// <returns>Transformed Vector3D.</returns>
	/// <param name="vector">Vector3D to transform.</param>
	public Vector3D Transform(Vector3D vector)
	{
		return Value.Transform(vector);
	}

	/// <summary>Transforms the specified <see cref="T:System.Windows.Media.Media3D.Point4D" />. </summary>
	/// <returns>Transformed Point4D.</returns>
	/// <param name="point">Point4D to transform.</param>
	public Point4D Transform(Point4D point)
	{
		return Value.Transform(point);
	}

	/// <summary> Transforms the specified array of <see cref="T:System.Windows.Media.Media3D.Point3D" /> objects. </summary>
	/// <param name="points">Array of Point3D objects to transform.</param>
	public void Transform(Point3D[] points)
	{
		Value.Transform(points);
	}

	/// <summary> Transforms the specified array of <see cref="T:System.Windows.Media.Media3D.Vector3D" /> objects. </summary>
	/// <param name="vectors">Array of Vector3D objects to transform.</param>
	public void Transform(Vector3D[] vectors)
	{
		Value.Transform(vectors);
	}

	/// <summary> Transforms the specified array of <see cref="T:System.Windows.Media.Media3D.Point4D" /> objects. </summary>
	/// <param name="points">Array of Point4D objects to transform.</param>
	public void Transform(Point4D[] points)
	{
		Value.Transform(points);
	}

	/// <summary>Attempts to transform the specified 3-D point and returns a value that indicates whether the transformation was successful.</summary>
	/// <returns>true if <paramref name="inPoint" /> was transformed; otherwise, false.</returns>
	/// <param name="inPoint">The 3-D point to transform.</param>
	/// <param name="result">The result of transforming <paramref name="inPoint" />.</param>
	public override bool TryTransform(Point3D inPoint, out Point3D result)
	{
		result = Value.Transform(inPoint);
		return true;
	}

	/// <summary>Transforms the specified 3-D bounding box and returns an axis-aligned 3-D bounding box that is exactly large enough to contain it.</summary>
	/// <returns>The smallest axis-aligned 3-D bounding box possible that contains the transformed <paramref name="rect" />.</returns>
	/// <param name="rect">The 3-D bounding box to transform.</param>
	public override Rect3D TransformBounds(Rect3D rect)
	{
		return M3DUtil.ComputeTransformedAxisAlignedBoundingBox(ref rect, this);
	}

	internal abstract void Append(ref Matrix3D matrix);

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Transform3D" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new Transform3D Clone()
	{
		return (Transform3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Transform3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new Transform3D CloneCurrentValue()
	{
		return (Transform3D)base.CloneCurrentValue();
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
}
