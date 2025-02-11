using System;
using System.Windows;
using System.Windows.Media.Media3D;
using MS.Internal.PresentationCore;

namespace MS.Internal.Media3D;

internal class GeneralTransform3DTo2DTo3D : GeneralTransform3D
{
	private GeneralTransform3DTo2D _transform3DTo2D;

	private GeneralTransform2DTo3D _transform2DTo3D;

	public override GeneralTransform3D Inverse => null;

	internal override Transform3D AffineTransform
	{
		[FriendAccessAllowed]
		get
		{
			return null;
		}
	}

	internal GeneralTransform3DTo2DTo3D()
	{
	}

	internal GeneralTransform3DTo2DTo3D(GeneralTransform3DTo2D transform3DTo2D, GeneralTransform2DTo3D transform2DTo3D)
	{
		_transform3DTo2D = (GeneralTransform3DTo2D)transform3DTo2D.GetAsFrozen();
		_transform2DTo3D = (GeneralTransform2DTo3D)transform2DTo3D.GetAsFrozen();
	}

	public override bool TryTransform(Point3D inPoint, out Point3D result)
	{
		Point result2 = default(Point);
		result = default(Point3D);
		if (_transform3DTo2D == null || !_transform3DTo2D.TryTransform(inPoint, out result2))
		{
			return false;
		}
		if (_transform2DTo3D == null || !_transform2DTo3D.TryTransform(result2, out result))
		{
			return false;
		}
		return true;
	}

	public override Rect3D TransformBounds(Rect3D rect)
	{
		throw new NotImplementedException();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GeneralTransform3DTo2DTo3D();
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		GeneralTransform3DTo2DTo3D transform = (GeneralTransform3DTo2DTo3D)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(transform);
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		GeneralTransform3DTo2DTo3D transform = (GeneralTransform3DTo2DTo3D)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(transform);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		GeneralTransform3DTo2DTo3D transform = (GeneralTransform3DTo2DTo3D)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(transform);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		GeneralTransform3DTo2DTo3D transform = (GeneralTransform3DTo2DTo3D)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(transform);
	}

	private void CopyCommon(GeneralTransform3DTo2DTo3D transform)
	{
		_transform3DTo2D = transform._transform3DTo2D;
		_transform2DTo3D = transform._transform2DTo3D;
	}
}
