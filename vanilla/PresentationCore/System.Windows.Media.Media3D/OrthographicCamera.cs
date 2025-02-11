using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal.Media3D;

namespace System.Windows.Media.Media3D;

/// <summary> Represents an orthographic projection camera. </summary>
public sealed class OrthographicCamera : ProjectionCamera
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.OrthographicCamera.Width" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.OrthographicCamera.Width" /> dependency property.</returns>
	public static readonly DependencyProperty WidthProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_Width = 2.0;

	/// <summary> Gets or sets the width of the camera's viewing box, in world units. </summary>
	/// <returns>Width of the camera's viewing box, in world units.</returns>
	public double Width
	{
		get
		{
			return (double)GetValue(WidthProperty);
		}
		set
		{
			SetValueInternal(WidthProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.OrthographicCamera" /> class.</summary>
	public OrthographicCamera()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.OrthographicCamera" /> class with the specified position, projection direction, upward direction, and width.</summary>
	/// <param name="position">A <see cref="T:System.Windows.Media.Media3D.Point3D" /> that specifies the camera position.</param>
	/// <param name="lookDirection">A <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that specifies the direction of the camera's projection.</param>
	/// <param name="upDirection">A <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that specifies the upward direction according to the perspective of the onlooker.</param>
	/// <param name="width">The width of the camera's viewing box, in world units.</param>
	public OrthographicCamera(Point3D position, Vector3D lookDirection, Vector3D upDirection, double width)
	{
		base.Position = position;
		base.LookDirection = lookDirection;
		base.UpDirection = upDirection;
		Width = width;
	}

	internal Matrix3D GetProjectionMatrix(double aspectRatio, double zn, double zf)
	{
		double width = Width;
		double num = width / aspectRatio;
		double num2 = 1.0 / (zn - zf);
		double offsetZ = zn * num2;
		return new Matrix3D(2.0 / width, 0.0, 0.0, 0.0, 0.0, 2.0 / num, 0.0, 0.0, 0.0, 0.0, num2, 0.0, 0.0, 0.0, offsetZ, 1.0);
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
		double num = base.NearPlaneDistance;
		double farPlaneDistance = base.FarPlaneDistance;
		double width = Width;
		Point normalizedPoint = M3DUtil.GetNormalizedPoint(p, viewSize);
		double aspectRatio = M3DUtil.GetAspectRatio(viewSize);
		double num2 = width;
		double num3 = num2 / aspectRatio;
		Vector3D vector = new Vector3D(0.0, 0.0, -1.0);
		Matrix3D matrix = ProjectionCamera.CreateViewMatrix(base.Transform, ref position, ref lookDirection, ref upDirection);
		Matrix3D matrix3D = matrix;
		matrix3D.Invert();
		Rect3D rect3D = M3DUtil.ComputeTransformedAxisAlignedBoundingBox(ref boundingRect, ref matrix);
		double num4 = 0.0 - AddEpsilon(rect3D.Z + rect3D.SizeZ);
		if (num4 > num)
		{
			distanceAdjustment = num4 - num;
			num = num4;
		}
		else
		{
			distanceAdjustment = 0.0;
		}
		Point3D point = new Point3D(normalizedPoint.X * (num2 / 2.0), normalizedPoint.Y * (num3 / 2.0), 0.0 - num);
		matrix3D.MultiplyPoint(ref point);
		matrix3D.MultiplyVector(ref vector);
		RayHitTestParameters rayHitTestParameters = new RayHitTestParameters(point, vector);
		Matrix3D projectionMatrix = GetProjectionMatrix(aspectRatio, num, farPlaneDistance);
		Matrix3D matrix3D2 = default(Matrix3D);
		matrix3D2.TranslatePrepend(new Vector3D(0.0 - p.X, viewSize.Height - p.Y, 0.0));
		matrix3D2.ScalePrepend(new Vector3D(viewSize.Width / 2.0, (0.0 - viewSize.Height) / 2.0, 1.0));
		matrix3D2.TranslatePrepend(new Vector3D(1.0, 1.0, 0.0));
		rayHitTestParameters.HitTestProjectionMatrix = matrix * projectionMatrix * matrix3D2;
		return rayHitTestParameters;
	}

	private double AddEpsilon(double x)
	{
		return x + 0.1 * Math.Abs(x) + 1.0;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.OrthographicCamera" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new OrthographicCamera Clone()
	{
		return (OrthographicCamera)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.OrthographicCamera" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new OrthographicCamera CloneCurrentValue()
	{
		return (OrthographicCamera)base.CloneCurrentValue();
	}

	private static void WidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((OrthographicCamera)d).PropertyChanged(WidthProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new OrthographicCamera();
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
			DUCE.ResourceHandle animationResourceHandle6 = GetAnimationResourceHandle(WidthProperty, channel);
			DUCE.MILCMD_ORTHOGRAPHICCAMERA mILCMD_ORTHOGRAPHICCAMERA = default(DUCE.MILCMD_ORTHOGRAPHICCAMERA);
			mILCMD_ORTHOGRAPHICCAMERA.Type = MILCMD.MilCmdOrthographicCamera;
			mILCMD_ORTHOGRAPHICCAMERA.Handle = _duceResource.GetHandle(channel);
			mILCMD_ORTHOGRAPHICCAMERA.htransform = htransform;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_ORTHOGRAPHICCAMERA.nearPlaneDistance = base.NearPlaneDistance;
			}
			mILCMD_ORTHOGRAPHICCAMERA.hNearPlaneDistanceAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_ORTHOGRAPHICCAMERA.farPlaneDistance = base.FarPlaneDistance;
			}
			mILCMD_ORTHOGRAPHICCAMERA.hFarPlaneDistanceAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_ORTHOGRAPHICCAMERA.position = CompositionResourceManager.Point3DToMilPoint3F(base.Position);
			}
			mILCMD_ORTHOGRAPHICCAMERA.hPositionAnimations = animationResourceHandle3;
			if (animationResourceHandle4.IsNull)
			{
				mILCMD_ORTHOGRAPHICCAMERA.lookDirection = CompositionResourceManager.Vector3DToMilPoint3F(base.LookDirection);
			}
			mILCMD_ORTHOGRAPHICCAMERA.hLookDirectionAnimations = animationResourceHandle4;
			if (animationResourceHandle5.IsNull)
			{
				mILCMD_ORTHOGRAPHICCAMERA.upDirection = CompositionResourceManager.Vector3DToMilPoint3F(base.UpDirection);
			}
			mILCMD_ORTHOGRAPHICCAMERA.hUpDirectionAnimations = animationResourceHandle5;
			if (animationResourceHandle6.IsNull)
			{
				mILCMD_ORTHOGRAPHICCAMERA.width = Width;
			}
			mILCMD_ORTHOGRAPHICCAMERA.hWidthAnimations = animationResourceHandle6;
			channel.SendCommand((byte*)(&mILCMD_ORTHOGRAPHICCAMERA), sizeof(DUCE.MILCMD_ORTHOGRAPHICCAMERA));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_ORTHOGRAPHICCAMERA))
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

	static OrthographicCamera()
	{
		Type typeFromHandle = typeof(OrthographicCamera);
		WidthProperty = Animatable.RegisterProperty("Width", typeof(double), typeFromHandle, 2.0, WidthPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
