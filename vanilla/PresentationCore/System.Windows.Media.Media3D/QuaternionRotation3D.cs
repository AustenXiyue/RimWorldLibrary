using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Represents a rotation transformation defined as a quaternion.</summary>
public sealed class QuaternionRotation3D : Rotation3D
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.QuaternionRotation3D.Quaternion" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.QuaternionRotation3D.Quaternion" /> dependency property.</returns>
	public static readonly DependencyProperty QuaternionProperty;

	private Quaternion _cachedQuaternionValue = Quaternion.Identity;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Quaternion s_Quaternion;

	internal override Quaternion InternalQuaternion => _cachedQuaternionValue;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Quaternion" /> that defines the destination rotation.  </summary>
	/// <returns>Quaternion that defines the destination rotation.</returns>
	public Quaternion Quaternion
	{
		get
		{
			ReadPreamble();
			return _cachedQuaternionValue;
		}
		set
		{
			SetValueInternal(QuaternionProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.QuaternionRotation3D" /> class.</summary>
	public QuaternionRotation3D()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.QuaternionRotation3D" /> class using the specified <see cref="T:System.Windows.Media.Media3D.Quaternion" />.</summary>
	/// <param name="quaternion">Quaternion that specifies the rotation to which to interpolate.</param>
	public QuaternionRotation3D(Quaternion quaternion)
	{
		Quaternion = quaternion;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.QuaternionRotation3D" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new QuaternionRotation3D Clone()
	{
		return (QuaternionRotation3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.QuaternionRotation3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new QuaternionRotation3D CloneCurrentValue()
	{
		return (QuaternionRotation3D)base.CloneCurrentValue();
	}

	private static void QuaternionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		QuaternionRotation3D obj = (QuaternionRotation3D)d;
		obj._cachedQuaternionValue = (Quaternion)e.NewValue;
		obj.PropertyChanged(QuaternionProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new QuaternionRotation3D();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(QuaternionProperty, channel);
			DUCE.MILCMD_QUATERNIONROTATION3D mILCMD_QUATERNIONROTATION3D = default(DUCE.MILCMD_QUATERNIONROTATION3D);
			mILCMD_QUATERNIONROTATION3D.Type = MILCMD.MilCmdQuaternionRotation3D;
			mILCMD_QUATERNIONROTATION3D.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_QUATERNIONROTATION3D.quaternion = CompositionResourceManager.QuaternionToMilQuaternionF(Quaternion);
			}
			mILCMD_QUATERNIONROTATION3D.hQuaternionAnimations = animationResourceHandle;
			channel.SendCommand((byte*)(&mILCMD_QUATERNIONROTATION3D), sizeof(DUCE.MILCMD_QUATERNIONROTATION3D));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_QUATERNIONROTATION3D))
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

	static QuaternionRotation3D()
	{
		s_Quaternion = Quaternion.Identity;
		Type typeFromHandle = typeof(QuaternionRotation3D);
		QuaternionProperty = Animatable.RegisterProperty("Quaternion", typeof(Quaternion), typeFromHandle, Quaternion.Identity, QuaternionPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
