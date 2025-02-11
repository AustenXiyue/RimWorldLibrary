using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary> Specifies a rotation transformation. </summary>
public sealed class RotateTransform3D : AffineTransform3D
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.RotateTransform3D.CenterX" /> dependency property.</summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.Media3D.RotateTransform3D.CenterX" /> dependency property.</returns>
	public static readonly DependencyProperty CenterXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.RotateTransform3D.CenterY" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.RotateTransform3D.CenterY" /> dependency property.</returns>
	public static readonly DependencyProperty CenterYProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.RotateTransform3D.CenterZ" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.RotateTransform3D.CenterZ" /> dependency property.</returns>
	public static readonly DependencyProperty CenterZProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.RotateTransform3D.Rotation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.RotateTransform3D.Rotation" /> dependency property.</returns>
	public static readonly DependencyProperty RotationProperty;

	private double _cachedCenterXValue;

	private double _cachedCenterYValue;

	private double _cachedCenterZValue;

	private Rotation3D _cachedRotationValue = Rotation3D.Identity;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_CenterX = 0.0;

	internal const double c_CenterY = 0.0;

	internal const double c_CenterZ = 0.0;

	internal static Rotation3D s_Rotation;

	/// <summary>Retrieves a <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> that represents the rotation.</summary>
	/// <returns>Matrix3D that represents the rotation.</returns>
	public override Matrix3D Value
	{
		get
		{
			ReadPreamble();
			Rotation3D cachedRotationValue = _cachedRotationValue;
			if (cachedRotationValue == null)
			{
				return Matrix3D.Identity;
			}
			Quaternion quaternion = cachedRotationValue.InternalQuaternion;
			Point3D center = new Point3D(_cachedCenterXValue, _cachedCenterYValue, _cachedCenterZValue);
			return Matrix3D.CreateRotationMatrix(ref quaternion, ref center);
		}
	}

	/// <summary>Gets or sets the X coordinate of the <see cref="T:System.Windows.Media.Media3D.Point3D" /> about which to rotate.  </summary>
	/// <returns>Double that represents the X coordinate of the <see cref="T:System.Windows.Media.Media3D.Point3D" /> about which to rotate.</returns>
	public double CenterX
	{
		get
		{
			ReadPreamble();
			return _cachedCenterXValue;
		}
		set
		{
			SetValueInternal(CenterXProperty, value);
		}
	}

	/// <summary>Gets or sets the Y coordinate of the <see cref="T:System.Windows.Media.Media3D.Point3D" /> about which to rotate.  </summary>
	/// <returns>Double that represents the Y coordinate of the <see cref="T:System.Windows.Media.Media3D.Point3D" /> about which to rotate.</returns>
	public double CenterY
	{
		get
		{
			ReadPreamble();
			return _cachedCenterYValue;
		}
		set
		{
			SetValueInternal(CenterYProperty, value);
		}
	}

	/// <summary>Gets or sets the Z coordinate of the <see cref="T:System.Windows.Media.Media3D.Point3D" /> about which to rotate.</summary>
	/// <returns>Double that represents the Z coordinate of the <see cref="T:System.Windows.Media.Media3D.Point3D" /> about which to rotate.</returns>
	public double CenterZ
	{
		get
		{
			ReadPreamble();
			return _cachedCenterZValue;
		}
		set
		{
			SetValueInternal(CenterZProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Media3D.Rotation3D" /> that specifies the rotation.  </summary>
	/// <returns>Rotation3D that specifies an angle of rotation about an axis.</returns>
	public Rotation3D Rotation
	{
		get
		{
			ReadPreamble();
			return _cachedRotationValue;
		}
		set
		{
			SetValueInternal(RotationProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.RotateTransform3D" /> class. </summary>
	public RotateTransform3D()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.RotateTransform3D" /> class with the specified rotation. </summary>
	/// <param name="rotation">Rotation3D that specifies the rotation.</param>
	public RotateTransform3D(Rotation3D rotation)
	{
		Rotation = rotation;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.RotateTransform3D" /> class with the specified center and rotation. </summary>
	/// <param name="rotation">Rotation3D that specifies the rotation.</param>
	/// <param name="center">Center of the transformation's rotation.</param>
	public RotateTransform3D(Rotation3D rotation, Point3D center)
	{
		Rotation = rotation;
		CenterX = center.X;
		CenterY = center.Y;
		CenterZ = center.Z;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.RotateTransform3D" /> class uusing the specified rotation and center coordinates.</summary>
	/// <param name="rotation">Rotation3D that specifies the rotation.</param>
	/// <param name="centerX">Double that specifies the X value about which to rotate.</param>
	/// <param name="centerY">Double that specifies the Y value about which to rotate.</param>
	/// <param name="centerZ">Double that specifies the Z value about which to rotate.</param>
	public RotateTransform3D(Rotation3D rotation, double centerX, double centerY, double centerZ)
	{
		Rotation = rotation;
		CenterX = centerX;
		CenterY = centerY;
		CenterZ = centerZ;
	}

	internal override void Append(ref Matrix3D matrix)
	{
		matrix *= Value;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.RotateTransform3D" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new RotateTransform3D Clone()
	{
		return (RotateTransform3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.RotateTransform3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new RotateTransform3D CloneCurrentValue()
	{
		return (RotateTransform3D)base.CloneCurrentValue();
	}

	private static void CenterXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RotateTransform3D obj = (RotateTransform3D)d;
		obj._cachedCenterXValue = (double)e.NewValue;
		obj.PropertyChanged(CenterXProperty);
	}

	private static void CenterYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RotateTransform3D obj = (RotateTransform3D)d;
		obj._cachedCenterYValue = (double)e.NewValue;
		obj.PropertyChanged(CenterYProperty);
	}

	private static void CenterZPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RotateTransform3D obj = (RotateTransform3D)d;
		obj._cachedCenterZValue = (double)e.NewValue;
		obj.PropertyChanged(CenterZProperty);
	}

	private static void RotationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		RotateTransform3D rotateTransform3D = (RotateTransform3D)d;
		rotateTransform3D._cachedRotationValue = (Rotation3D)e.NewValue;
		Rotation3D resource = (Rotation3D)e.OldValue;
		Rotation3D resource2 = (Rotation3D)e.NewValue;
		if (rotateTransform3D.Dispatcher != null)
		{
			DUCE.IResource resource3 = rotateTransform3D;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					rotateTransform3D.ReleaseResource(resource, channel);
					rotateTransform3D.AddRefResource(resource2, channel);
				}
			}
		}
		rotateTransform3D.PropertyChanged(RotationProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new RotateTransform3D();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle hrotation = ((DUCE.IResource)Rotation)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(CenterXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(CenterYProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(CenterZProperty, channel);
			DUCE.MILCMD_ROTATETRANSFORM3D mILCMD_ROTATETRANSFORM3D = default(DUCE.MILCMD_ROTATETRANSFORM3D);
			mILCMD_ROTATETRANSFORM3D.Type = MILCMD.MilCmdRotateTransform3D;
			mILCMD_ROTATETRANSFORM3D.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_ROTATETRANSFORM3D.centerX = CenterX;
			}
			mILCMD_ROTATETRANSFORM3D.hCenterXAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_ROTATETRANSFORM3D.centerY = CenterY;
			}
			mILCMD_ROTATETRANSFORM3D.hCenterYAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_ROTATETRANSFORM3D.centerZ = CenterZ;
			}
			mILCMD_ROTATETRANSFORM3D.hCenterZAnimations = animationResourceHandle3;
			mILCMD_ROTATETRANSFORM3D.hrotation = hrotation;
			channel.SendCommand((byte*)(&mILCMD_ROTATETRANSFORM3D), sizeof(DUCE.MILCMD_ROTATETRANSFORM3D));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_ROTATETRANSFORM3D))
		{
			((DUCE.IResource)Rotation)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)Rotation)?.ReleaseOnChannel(channel);
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

	static RotateTransform3D()
	{
		s_Rotation = Rotation3D.Identity;
		Type typeFromHandle = typeof(RotateTransform3D);
		CenterXProperty = Animatable.RegisterProperty("CenterX", typeof(double), typeFromHandle, 0.0, CenterXPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterYProperty = Animatable.RegisterProperty("CenterY", typeof(double), typeFromHandle, 0.0, CenterYPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterZProperty = Animatable.RegisterProperty("CenterZ", typeof(double), typeFromHandle, 0.0, CenterZPropertyChanged, null, isIndependentlyAnimated: true, null);
		RotationProperty = Animatable.RegisterProperty("Rotation", typeof(Rotation3D), typeFromHandle, Rotation3D.Identity, RotationPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
