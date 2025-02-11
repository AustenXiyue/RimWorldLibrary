using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Represents a 3-D rotation of a specified angle about a specified axis.</summary>
public sealed class AxisAngleRotation3D : Rotation3D
{
	private Quaternion _cachedQuaternionValue = c_dirtyQuaternion;

	internal static readonly Quaternion c_dirtyQuaternion;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.AxisAngleRotation3D.Axis" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.AxisAngleRotation3D.Axis" /> dependency property.</returns>
	public static readonly DependencyProperty AxisProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.AxisAngleRotation3D.Angle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.AxisAngleRotation3D.Angle" /> dependency property.</returns>
	public static readonly DependencyProperty AngleProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Vector3D s_Axis;

	internal const double c_Angle = 0.0;

	internal override Quaternion InternalQuaternion
	{
		get
		{
			if (_cachedQuaternionValue == c_dirtyQuaternion)
			{
				Vector3D axis = Axis;
				if (axis.LengthSquared > 1.1754943508222875E-38)
				{
					_cachedQuaternionValue = new Quaternion(axis, Angle);
				}
				else
				{
					_cachedQuaternionValue = Quaternion.Identity;
				}
			}
			return _cachedQuaternionValue;
		}
	}

	/// <summary>Gets or sets the axis of a 3-D rotation. </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that specifies the axis of rotation.</returns>
	public Vector3D Axis
	{
		get
		{
			return (Vector3D)GetValue(AxisProperty);
		}
		set
		{
			SetValueInternal(AxisProperty, value);
		}
	}

	/// <summary>Gets or sets the angle of a 3-D rotation, in degrees. </summary>
	/// <returns>Double that specifies the angle of a 3-D rotation, in degrees.</returns>
	public double Angle
	{
		get
		{
			return (double)GetValue(AngleProperty);
		}
		set
		{
			SetValueInternal(AngleProperty, value);
		}
	}

	/// <summary>Creates an instance of a 3-D rotation.</summary>
	public AxisAngleRotation3D()
	{
	}

	/// <summary>Creates an instance of a 3-D rotation using the specified axis and angle.</summary>
	/// <param name="axis">
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> that specifies the axis around which to rotate.</param>
	/// <param name="angle">Double that specifies the angle of rotation, in degrees.</param>
	public AxisAngleRotation3D(Vector3D axis, double angle)
	{
		Axis = axis;
		Angle = angle;
	}

	internal void AxisPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		_cachedQuaternionValue = c_dirtyQuaternion;
	}

	internal void AnglePropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		_cachedQuaternionValue = c_dirtyQuaternion;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.AxisAngleRotation3D" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new AxisAngleRotation3D Clone()
	{
		return (AxisAngleRotation3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.AxisAngleRotation3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new AxisAngleRotation3D CloneCurrentValue()
	{
		return (AxisAngleRotation3D)base.CloneCurrentValue();
	}

	private static void AxisPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		AxisAngleRotation3D obj = (AxisAngleRotation3D)d;
		obj.AxisPropertyChangedHook(e);
		obj.PropertyChanged(AxisProperty);
	}

	private static void AnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		AxisAngleRotation3D obj = (AxisAngleRotation3D)d;
		obj.AnglePropertyChangedHook(e);
		obj.PropertyChanged(AngleProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new AxisAngleRotation3D();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(AxisProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(AngleProperty, channel);
			DUCE.MILCMD_AXISANGLEROTATION3D mILCMD_AXISANGLEROTATION3D = default(DUCE.MILCMD_AXISANGLEROTATION3D);
			mILCMD_AXISANGLEROTATION3D.Type = MILCMD.MilCmdAxisAngleRotation3D;
			mILCMD_AXISANGLEROTATION3D.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_AXISANGLEROTATION3D.axis = CompositionResourceManager.Vector3DToMilPoint3F(Axis);
			}
			mILCMD_AXISANGLEROTATION3D.hAxisAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_AXISANGLEROTATION3D.angle = Angle;
			}
			mILCMD_AXISANGLEROTATION3D.hAngleAnimations = animationResourceHandle2;
			channel.SendCommand((byte*)(&mILCMD_AXISANGLEROTATION3D), sizeof(DUCE.MILCMD_AXISANGLEROTATION3D));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_AXISANGLEROTATION3D))
		{
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
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

	static AxisAngleRotation3D()
	{
		c_dirtyQuaternion = new Quaternion(Math.E, Math.PI, 8.539734222673566, 55.0);
		s_Axis = new Vector3D(0.0, 1.0, 0.0);
		Type typeFromHandle = typeof(AxisAngleRotation3D);
		AxisProperty = Animatable.RegisterProperty("Axis", typeof(Vector3D), typeFromHandle, new Vector3D(0.0, 1.0, 0.0), AxisPropertyChanged, null, isIndependentlyAnimated: true, null);
		AngleProperty = Animatable.RegisterProperty("Angle", typeof(double), typeFromHandle, 0.0, AnglePropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
