using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary> Allows the application of a 2-D brush, like a <see cref="T:System.Windows.Media.SolidColorBrush" /> or <see cref="T:System.Windows.Media.TileBrush" />, to a diffusely-lit 3-D model. </summary>
public sealed class DiffuseMaterial : Material
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.DiffuseMaterial.Color" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.DiffuseMaterial.Color" /> dependency property.</returns>
	public static readonly DependencyProperty ColorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.DiffuseMaterial.AmbientColor" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.DiffuseMaterial.AmbientColor" /> dependency property.</returns>
	public static readonly DependencyProperty AmbientColorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.DiffuseMaterial.Brush" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.DiffuseMaterial.Brush" /> dependency property.</returns>
	public static readonly DependencyProperty BrushProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Color s_Color;

	internal static Color s_AmbientColor;

	internal static Brush s_Brush;

	/// <summary>Gets or sets the color filter for the material's texture. </summary>
	/// <returns>The color filter for the <see cref="T:System.Windows.Media.Media3D.Material" />. The default value is #FFFFFF. Since all colors make up white, all colors are visible by default.</returns>
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

	/// <summary>Gets or sets a color that represents how the material reflects <see cref="T:System.Windows.Media.Media3D.AmbientLight" />.</summary>
	/// <returns>The color of the ambient light reflected by the 3D object. The default value is #FFFFFF.</returns>
	public Color AmbientColor
	{
		get
		{
			return (Color)GetValue(AmbientColorProperty);
		}
		set
		{
			SetValueInternal(AmbientColorProperty, value);
		}
	}

	/// <summary>
	///   <see cref="T:System.Windows.Media.Brush" /> to be applied as a <see cref="T:System.Windows.Media.Media3D.Material" /> to a 3-D model. </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Brush" /> to apply.</returns>
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

	/// <summary> Constructs a DiffuseMaterial. </summary>
	public DiffuseMaterial()
	{
	}

	/// <summary>Constructs a DiffuseMaterial with the specified Brush property. </summary>
	/// <param name="brush">The new material's brush.</param>
	public DiffuseMaterial(Brush brush)
	{
		Brush = brush;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.DiffuseMaterial" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DiffuseMaterial Clone()
	{
		return (DiffuseMaterial)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.DiffuseMaterial" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DiffuseMaterial CloneCurrentValue()
	{
		return (DiffuseMaterial)base.CloneCurrentValue();
	}

	private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DiffuseMaterial)d).PropertyChanged(ColorProperty);
	}

	private static void AmbientColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DiffuseMaterial)d).PropertyChanged(AmbientColorProperty);
	}

	private static void BrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		DiffuseMaterial diffuseMaterial = (DiffuseMaterial)d;
		Brush resource = (Brush)e.OldValue;
		Brush resource2 = (Brush)e.NewValue;
		if (diffuseMaterial.Dispatcher != null)
		{
			DUCE.IResource resource3 = diffuseMaterial;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					diffuseMaterial.ReleaseResource(resource, channel);
					diffuseMaterial.AddRefResource(resource2, channel);
				}
			}
		}
		diffuseMaterial.PropertyChanged(BrushProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new DiffuseMaterial();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle hbrush = ((DUCE.IResource)Brush)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.MILCMD_DIFFUSEMATERIAL mILCMD_DIFFUSEMATERIAL = default(DUCE.MILCMD_DIFFUSEMATERIAL);
			mILCMD_DIFFUSEMATERIAL.Type = MILCMD.MilCmdDiffuseMaterial;
			mILCMD_DIFFUSEMATERIAL.Handle = _duceResource.GetHandle(channel);
			mILCMD_DIFFUSEMATERIAL.color = CompositionResourceManager.ColorToMilColorF(Color);
			mILCMD_DIFFUSEMATERIAL.ambientColor = CompositionResourceManager.ColorToMilColorF(AmbientColor);
			mILCMD_DIFFUSEMATERIAL.hbrush = hbrush;
			channel.SendCommand((byte*)(&mILCMD_DIFFUSEMATERIAL), sizeof(DUCE.MILCMD_DIFFUSEMATERIAL));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_DIFFUSEMATERIAL))
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

	static DiffuseMaterial()
	{
		s_Color = Colors.White;
		s_AmbientColor = Colors.White;
		s_Brush = null;
		Type typeFromHandle = typeof(DiffuseMaterial);
		ColorProperty = Animatable.RegisterProperty("Color", typeof(Color), typeFromHandle, Colors.White, ColorPropertyChanged, null, isIndependentlyAnimated: false, null);
		AmbientColorProperty = Animatable.RegisterProperty("AmbientColor", typeof(Color), typeFromHandle, Colors.White, AmbientColorPropertyChanged, null, isIndependentlyAnimated: false, null);
		BrushProperty = Animatable.RegisterProperty("Brush", typeof(Brush), typeFromHandle, null, BrushPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
