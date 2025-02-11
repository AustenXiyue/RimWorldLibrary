using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Light object that projects its effect along a direction specified by a <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
public sealed class DirectionalLight : Light
{
	/// <summary> Identifies the <see cref="P:System.Windows.Media.Media3D.DirectionalLight.Direction" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.DirectionalLight.Direction" /> dependency property.</returns>
	public static readonly DependencyProperty DirectionProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Vector3D s_Direction;

	/// <summary> Represents the vector along which the light's effect will be seen on models in a 3-D scene.   </summary>
	/// <returns>Vector3D along which the light projects, and which must have a non-zero magnitude. The default value is (0,0,-1).</returns>
	public Vector3D Direction
	{
		get
		{
			return (Vector3D)GetValue(DirectionProperty);
		}
		set
		{
			SetValueInternal(DirectionProperty, value);
		}
	}

	/// <summary>Creates an instance of a light that projects its effect in a specified direction. </summary>
	public DirectionalLight()
	{
	}

	/// <summary>Creates an instance of a light that projects its effect along a specified Vector3D with a specified color.</summary>
	/// <param name="diffuseColor">Diffuse color of the new light.</param>
	/// <param name="direction">Direction of the new light.</param>
	public DirectionalLight(Color diffuseColor, Vector3D direction)
	{
		base.Color = diffuseColor;
		Direction = direction;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.DirectionalLight" /> object, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DirectionalLight Clone()
	{
		return (DirectionalLight)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.DirectionalLight" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DirectionalLight CloneCurrentValue()
	{
		return (DirectionalLight)base.CloneCurrentValue();
	}

	private static void DirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DirectionalLight)d).PropertyChanged(DirectionProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new DirectionalLight();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform3D transform = base.Transform;
			DUCE.ResourceHandle htransform = ((transform != null && transform != Transform3D.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(Light.ColorProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(DirectionProperty, channel);
			DUCE.MILCMD_DIRECTIONALLIGHT mILCMD_DIRECTIONALLIGHT = default(DUCE.MILCMD_DIRECTIONALLIGHT);
			mILCMD_DIRECTIONALLIGHT.Type = MILCMD.MilCmdDirectionalLight;
			mILCMD_DIRECTIONALLIGHT.Handle = _duceResource.GetHandle(channel);
			mILCMD_DIRECTIONALLIGHT.htransform = htransform;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_DIRECTIONALLIGHT.color = CompositionResourceManager.ColorToMilColorF(base.Color);
			}
			mILCMD_DIRECTIONALLIGHT.hColorAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_DIRECTIONALLIGHT.direction = CompositionResourceManager.Vector3DToMilPoint3F(Direction);
			}
			mILCMD_DIRECTIONALLIGHT.hDirectionAnimations = animationResourceHandle2;
			channel.SendCommand((byte*)(&mILCMD_DIRECTIONALLIGHT), sizeof(DUCE.MILCMD_DIRECTIONALLIGHT));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_DIRECTIONALLIGHT))
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

	static DirectionalLight()
	{
		s_Direction = new Vector3D(0.0, 0.0, -1.0);
		Type typeFromHandle = typeof(DirectionalLight);
		DirectionProperty = Animatable.RegisterProperty("Direction", typeof(Vector3D), typeFromHandle, new Vector3D(0.0, 0.0, -1.0), DirectionPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
