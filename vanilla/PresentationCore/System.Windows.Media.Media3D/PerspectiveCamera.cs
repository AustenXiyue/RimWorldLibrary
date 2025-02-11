using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal.Media3D;

namespace System.Windows.Media.Media3D;

/// <summary> Represents a perspective projection camera. </summary>
public sealed class PerspectiveCamera : ProjectionCamera
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.PerspectiveCamera.FieldOfView" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.PerspectiveCamera.FieldOfView" /> dependency property identifier.</returns>
	public static readonly DependencyProperty FieldOfViewProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_FieldOfView = 45.0;

	/// <summary> Gets or sets a value that represents the camera's horizontal field of view. </summary>
	/// <returns>The camera's horizontal field of view, in degrees. The default value is 45.</returns>
	public double FieldOfView
	{
		get
		{
			return (double)GetValue(FieldOfViewProperty);
		}
		set
		{
			SetValueInternal(FieldOfViewProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.PerspectiveCamera" /> class.</summary>
	public PerspectiveCamera()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.PerspectiveCamera" /> class using the specified position, direction, and field of view.</summary>
	/// <param name="position">Point3D that specifies the camera's position.</param>
	/// <param name="lookDirection">Vector3D that specifies the direction of the camera's projection.</param>
	/// <param name="upDirection">Vector3D that specifies the upward direction according to the perspective of the onlooker.</param>
	/// <param name="fieldOfView">Width of the camera's angle of projection, specified in degrees.</param>
	public PerspectiveCamera(Point3D position, Vector3D lookDirection, Vector3D upDirection, double fieldOfView)
	{
		base.Position = position;
		base.LookDirection = lookDirection;
		base.UpDirection = upDirection;
		FieldOfView = fieldOfView;
	}

	internal Matrix3D GetProjectionMatrix(double aspectRatio, double zn, double zf)
	{
		double num = Math.Tan(M3DUtil.DegreesToRadians(FieldOfView) / 2.0);
		double m = aspectRatio / num;
		double m2 = 1.0 / num;
		double num2 = ((zf != double.PositiveInfinity) ? (zf / (zn - zf)) : (-1.0));
		double offsetZ = zn * num2;
		return new Matrix3D(m2, 0.0, 0.0, 0.0, 0.0, m, 0.0, 0.0, 0.0, 0.0, num2, -1.0, 0.0, 0.0, offsetZ, 0.0);
	}

	internal override Matrix3D GetProjectionMatrix(double aspectRatio)
	{
		return GetProjectionMatrix(aspectRatio, base.NearPlaneDistance, base.FarPlaneDistance);
	}

	internal override RayHitTestParameters RayFromViewportPoint(Point p, Size viewSize, Rect3D boundingRect, out double distanceAdjustment)
	{
		Point3D position = base.Position;
		Vector3D lookDirection = base.LookDirection;
		Vector3D upDirection = base.UpDirection;
		Transform3D transform = base.Transform;
		double nearPlaneDistance = base.NearPlaneDistance;
		double farPlaneDistance = base.FarPlaneDistance;
		double num = M3DUtil.DegreesToRadians(FieldOfView);
		Point normalizedPoint = M3DUtil.GetNormalizedPoint(p, viewSize);
		double aspectRatio = M3DUtil.GetAspectRatio(viewSize);
		double num2 = Math.Tan(num / 2.0);
		double num3 = aspectRatio / num2;
		double num4 = 1.0 / num2;
		Vector3D vector = new Vector3D(normalizedPoint.X / num4, normalizedPoint.Y / num3, -1.0);
		Matrix3D viewMatrix = ProjectionCamera.CreateViewMatrix(null, ref position, ref lookDirection, ref upDirection);
		Matrix3D matrix3D = viewMatrix;
		matrix3D.Invert();
		matrix3D.MultiplyVector(ref vector);
		Point3D point = position + nearPlaneDistance * vector;
		vector.Normalize();
		if (transform != null && transform != Transform3D.Identity)
		{
			Matrix3D value = transform.Value;
			value.MultiplyPoint(ref point);
			value.MultiplyVector(ref vector);
			Camera.PrependInverseTransform(value, ref viewMatrix);
		}
		RayHitTestParameters rayHitTestParameters = new RayHitTestParameters(point, vector);
		Matrix3D projectionMatrix = GetProjectionMatrix(aspectRatio, nearPlaneDistance, farPlaneDistance);
		Matrix3D matrix3D2 = default(Matrix3D);
		matrix3D2.TranslatePrepend(new Vector3D(0.0 - p.X, viewSize.Height - p.Y, 0.0));
		matrix3D2.ScalePrepend(new Vector3D(viewSize.Width / 2.0, (0.0 - viewSize.Height) / 2.0, 1.0));
		matrix3D2.TranslatePrepend(new Vector3D(1.0, 1.0, 0.0));
		rayHitTestParameters.HitTestProjectionMatrix = viewMatrix * projectionMatrix * matrix3D2;
		distanceAdjustment = 0.0;
		return rayHitTestParameters;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.PerspectiveCamera" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PerspectiveCamera Clone()
	{
		return (PerspectiveCamera)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.PerspectiveCamera" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PerspectiveCamera CloneCurrentValue()
	{
		return (PerspectiveCamera)base.CloneCurrentValue();
	}

	private static void FieldOfViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((PerspectiveCamera)d).PropertyChanged(FieldOfViewProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new PerspectiveCamera();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform3D transform = base.Transform;
			DUCE.ResourceHandle htransform = ((transform != null && transform != Transform3D.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(ProjectionCamera.NearPlaneDistanceProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(ProjectionCamera.FarPlaneDistanceProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(ProjectionCamera.PositionProperty, channel);
			DUCE.ResourceHandle animationResourceHandle4 = GetAnimationResourceHandle(ProjectionCamera.LookDirectionProperty, channel);
			DUCE.ResourceHandle animationResourceHandle5 = GetAnimationResourceHandle(ProjectionCamera.UpDirectionProperty, channel);
			DUCE.ResourceHandle animationResourceHandle6 = GetAnimationResourceHandle(FieldOfViewProperty, channel);
			DUCE.MILCMD_PERSPECTIVECAMERA mILCMD_PERSPECTIVECAMERA = default(DUCE.MILCMD_PERSPECTIVECAMERA);
			mILCMD_PERSPECTIVECAMERA.Type = MILCMD.MilCmdPerspectiveCamera;
			mILCMD_PERSPECTIVECAMERA.Handle = _duceResource.GetHandle(channel);
			mILCMD_PERSPECTIVECAMERA.htransform = htransform;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_PERSPECTIVECAMERA.nearPlaneDistance = base.NearPlaneDistance;
			}
			mILCMD_PERSPECTIVECAMERA.hNearPlaneDistanceAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_PERSPECTIVECAMERA.farPlaneDistance = base.FarPlaneDistance;
			}
			mILCMD_PERSPECTIVECAMERA.hFarPlaneDistanceAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_PERSPECTIVECAMERA.position = CompositionResourceManager.Point3DToMilPoint3F(base.Position);
			}
			mILCMD_PERSPECTIVECAMERA.hPositionAnimations = animationResourceHandle3;
			if (animationResourceHandle4.IsNull)
			{
				mILCMD_PERSPECTIVECAMERA.lookDirection = CompositionResourceManager.Vector3DToMilPoint3F(base.LookDirection);
			}
			mILCMD_PERSPECTIVECAMERA.hLookDirectionAnimations = animationResourceHandle4;
			if (animationResourceHandle5.IsNull)
			{
				mILCMD_PERSPECTIVECAMERA.upDirection = CompositionResourceManager.Vector3DToMilPoint3F(base.UpDirection);
			}
			mILCMD_PERSPECTIVECAMERA.hUpDirectionAnimations = animationResourceHandle5;
			if (animationResourceHandle6.IsNull)
			{
				mILCMD_PERSPECTIVECAMERA.fieldOfView = FieldOfView;
			}
			mILCMD_PERSPECTIVECAMERA.hFieldOfViewAnimations = animationResourceHandle6;
			channel.SendCommand((byte*)(&mILCMD_PERSPECTIVECAMERA), sizeof(DUCE.MILCMD_PERSPECTIVECAMERA));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_PERSPECTIVECAMERA))
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

	static PerspectiveCamera()
	{
		Type typeFromHandle = typeof(PerspectiveCamera);
		FieldOfViewProperty = Animatable.RegisterProperty("FieldOfView", typeof(double), typeFromHandle, 45.0, FieldOfViewPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
