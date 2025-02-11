using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal.Media3D;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Camera which specifies the view and projection transforms as <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> objects </summary>
public sealed class MatrixCamera : Camera
{
	/// <summary>Gets the <see cref="P:System.Windows.Media.Media3D.MatrixCamera.ViewMatrix" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.MatrixCamera.ViewMatrix" /> dependency property.</returns>
	public static readonly DependencyProperty ViewMatrixProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Media3D.MatrixCamera.ProjectionMatrix" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.MatrixCamera.ProjectionMatrix" /> dependency property.</returns>
	public static readonly DependencyProperty ProjectionMatrixProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Matrix3D s_ViewMatrix;

	internal static Matrix3D s_ProjectionMatrix;

	/// <summary> Gets or sets a <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> as the view transformation matrix.   </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Matrix3D" />that represents the position, look direction and up vector for the camera.</returns>
	public Matrix3D ViewMatrix
	{
		get
		{
			return (Matrix3D)GetValue(ViewMatrixProperty);
		}
		set
		{
			SetValueInternal(ViewMatrixProperty, value);
		}
	}

	/// <summary> Gets or sets a <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> as the projection transformation matrix.  </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> that specifies the projection transformation.</returns>
	public Matrix3D ProjectionMatrix
	{
		get
		{
			return (Matrix3D)GetValue(ProjectionMatrixProperty);
		}
		set
		{
			SetValueInternal(ProjectionMatrixProperty, value);
		}
	}

	/// <summary> Creates a new instance of <see cref="T:System.Windows.Media.Media3D.MatrixCamera" />. </summary>
	public MatrixCamera()
	{
	}

	/// <summary> Creates a new <see cref="T:System.Windows.Media.Media3D.MatrixCamera" /> from view and projection matrices. </summary>
	/// <param name="viewMatrix">Specifies the camera's view matrix.</param>
	/// <param name="projectionMatrix">Specifies the camera's projection matrix.</param>
	public MatrixCamera(Matrix3D viewMatrix, Matrix3D projectionMatrix)
	{
		ViewMatrix = viewMatrix;
		ProjectionMatrix = projectionMatrix;
	}

	internal override Matrix3D GetViewMatrix()
	{
		Matrix3D viewMatrix = ViewMatrix;
		Camera.PrependInverseTransform(base.Transform, ref viewMatrix);
		return viewMatrix;
	}

	internal override Matrix3D GetProjectionMatrix(double aspectRatio)
	{
		return ProjectionMatrix;
	}

	internal override RayHitTestParameters RayFromViewportPoint(Point p, Size viewSize, Rect3D boundingRect, out double distanceAdjustment)
	{
		Point normalizedPoint = M3DUtil.GetNormalizedPoint(p, viewSize);
		Matrix3D matrix3D = GetViewMatrix() * ProjectionMatrix;
		Matrix3D matrix3D2 = matrix3D;
		if (!matrix3D2.HasInverse)
		{
			throw new NotSupportedException(SR.HitTest_Singular);
		}
		matrix3D2.Invert();
		Point4D point4D = new Point4D(normalizedPoint.X, normalizedPoint.Y, 0.0, 1.0) * matrix3D2;
		Point3D origin = new Point3D(point4D.X / point4D.W, point4D.Y / point4D.W, point4D.Z / point4D.W);
		double x = matrix3D2.M31 - matrix3D2.M34 * origin.X;
		double y = matrix3D2.M32 - matrix3D2.M34 * origin.Y;
		double z = matrix3D2.M33 - matrix3D2.M34 * origin.Z;
		Vector3D vector3D = new Vector3D(x, y, z);
		vector3D.Normalize();
		if (point4D.W < 0.0)
		{
			vector3D = -vector3D;
		}
		RayHitTestParameters rayHitTestParameters = new RayHitTestParameters(origin, vector3D);
		Matrix3D matrix3D3 = default(Matrix3D);
		matrix3D3.TranslatePrepend(new Vector3D(0.0 - p.X, viewSize.Height - p.Y, 0.0));
		matrix3D3.ScalePrepend(new Vector3D(viewSize.Width / 2.0, (0.0 - viewSize.Height) / 2.0, 1.0));
		matrix3D3.TranslatePrepend(new Vector3D(1.0, 1.0, 0.0));
		rayHitTestParameters.HitTestProjectionMatrix = matrix3D * matrix3D3;
		distanceAdjustment = 0.0;
		return rayHitTestParameters;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.MatrixCamera" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new MatrixCamera Clone()
	{
		return (MatrixCamera)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.MatrixCamera" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new MatrixCamera CloneCurrentValue()
	{
		return (MatrixCamera)base.CloneCurrentValue();
	}

	private static void ViewMatrixPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MatrixCamera)d).PropertyChanged(ViewMatrixProperty);
	}

	private static void ProjectionMatrixPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MatrixCamera)d).PropertyChanged(ProjectionMatrixProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new MatrixCamera();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform3D transform = base.Transform;
			DUCE.ResourceHandle htransform = ((transform != null && transform != Transform3D.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.MILCMD_MATRIXCAMERA mILCMD_MATRIXCAMERA = default(DUCE.MILCMD_MATRIXCAMERA);
			mILCMD_MATRIXCAMERA.Type = MILCMD.MilCmdMatrixCamera;
			mILCMD_MATRIXCAMERA.Handle = _duceResource.GetHandle(channel);
			mILCMD_MATRIXCAMERA.htransform = htransform;
			mILCMD_MATRIXCAMERA.viewMatrix = CompositionResourceManager.Matrix3DToD3DMATRIX(ViewMatrix);
			mILCMD_MATRIXCAMERA.projectionMatrix = CompositionResourceManager.Matrix3DToD3DMATRIX(ProjectionMatrix);
			channel.SendCommand((byte*)(&mILCMD_MATRIXCAMERA), sizeof(DUCE.MILCMD_MATRIXCAMERA));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_MATRIXCAMERA))
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

	static MatrixCamera()
	{
		s_ViewMatrix = Matrix3D.Identity;
		s_ProjectionMatrix = Matrix3D.Identity;
		Type typeFromHandle = typeof(MatrixCamera);
		ViewMatrixProperty = Animatable.RegisterProperty("ViewMatrix", typeof(Matrix3D), typeFromHandle, Matrix3D.Identity, ViewMatrixPropertyChanged, null, isIndependentlyAnimated: false, null);
		ProjectionMatrixProperty = Animatable.RegisterProperty("ProjectionMatrix", typeof(Matrix3D), typeFromHandle, Matrix3D.Identity, ProjectionMatrixPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
