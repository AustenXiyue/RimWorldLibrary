using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Applies a <see cref="T:System.Windows.Media.Brush" /> to a 3-D model so that it participates in lighting calculations as if the <see cref="T:System.Windows.Media.Media3D.Material" /> were emitting light equal to the color of the <see cref="T:System.Windows.Media.Brush" />. </summary>
public sealed class EmissiveMaterial : Material
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.EmissiveMaterial.Color" /> dependency property.</summary>
	public static readonly DependencyProperty ColorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.EmissiveMaterial.Brush" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.EmissiveMaterial.Brush" /> dependency property.</returns>
	public static readonly DependencyProperty BrushProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Color s_Color;

	internal static Brush s_Brush;

	/// <summary>Gets or sets the color filter for the material's texture. </summary>
	/// <returns>The color filter for the <see cref="T:System.Windows.Media.Media3D.Material" />.</returns>
	public Color Color
	{
		get
		{
			return (Color)GetValue(ColorProperty);
		}
		set
		{
			SetValueInternal(ColorProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> applied by the <see cref="T:System.Windows.Media.Media3D.EmissiveMaterial" />. </summary>
	/// <returns>The brush applied by the <see cref="T:System.Windows.Media.Media3D.EmissiveMaterial" />. The default value is null.</returns>
	public Brush Brush
	{
		get
		{
			return (Brush)GetValue(BrushProperty);
		}
		set
		{
			SetValueInternal(BrushProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.EmissiveMaterial" /> class.</summary>
	public EmissiveMaterial()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.EmissiveMaterial" /> class with the specified brush. </summary>
	/// <param name="brush">The new material's brush.</param>
	public EmissiveMaterial(Brush brush)
	{
		Brush = brush;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.EmissiveMaterial" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new EmissiveMaterial Clone()
	{
		return (EmissiveMaterial)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.EmissiveMaterial" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new EmissiveMaterial CloneCurrentValue()
	{
		return (EmissiveMaterial)base.CloneCurrentValue();
	}

	private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((EmissiveMaterial)d).PropertyChanged(ColorProperty);
	}

	private static void BrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		EmissiveMaterial emissiveMaterial = (EmissiveMaterial)d;
		Brush resource = (Brush)e.OldValue;
		Brush resource2 = (Brush)e.NewValue;
		if (emissiveMaterial.Dispatcher != null)
		{
			DUCE.IResource resource3 = emissiveMaterial;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					emissiveMaterial.ReleaseResource(resource, channel);
					emissiveMaterial.AddRefResource(resource2, channel);
				}
			}
		}
		emissiveMaterial.PropertyChanged(BrushProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new EmissiveMaterial();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle hbrush = ((DUCE.IResource)Brush)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.MILCMD_EMISSIVEMATERIAL mILCMD_EMISSIVEMATERIAL = default(DUCE.MILCMD_EMISSIVEMATERIAL);
			mILCMD_EMISSIVEMATERIAL.Type = MILCMD.MilCmdEmissiveMaterial;
			mILCMD_EMISSIVEMATERIAL.Handle = _duceResource.GetHandle(channel);
			mILCMD_EMISSIVEMATERIAL.color = CompositionResourceManager.ColorToMilColorF(Color);
			mILCMD_EMISSIVEMATERIAL.hbrush = hbrush;
			channel.SendCommand((byte*)(&mILCMD_EMISSIVEMATERIAL), sizeof(DUCE.MILCMD_EMISSIVEMATERIAL));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_EMISSIVEMATERIAL))
		{
			((DUCE.IResource)Brush)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)Brush)?.ReleaseOnChannel(channel);
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

	static EmissiveMaterial()
	{
		s_Color = Colors.White;
		s_Brush = null;
		Type typeFromHandle = typeof(EmissiveMaterial);
		ColorProperty = Animatable.RegisterProperty("Color", typeof(Color), typeFromHandle, Colors.White, ColorPropertyChanged, null, isIndependentlyAnimated: false, null);
		BrushProperty = Animatable.RegisterProperty("Brush", typeof(Brush), typeFromHandle, null, BrushPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
