using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Allows a 2-D brush, like a <see cref="T:System.Windows.Media.SolidColorBrush" /> or <see cref="T:System.Windows.Media.TileBrush" />, to be applied to a specularly-lit 3-D model.  </summary>
public sealed class SpecularMaterial : Material
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.SpecularMaterial.Color" /> dependency property.</summary>
	public static readonly DependencyProperty ColorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.SpecularMaterial.Brush" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.SpecularMaterial.Brush" /> dependency property.</returns>
	public static readonly DependencyProperty BrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.SpecularMaterial.SpecularPower" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.SpecularMaterial.SpecularPower" /> dependency property.</returns>
	public static readonly DependencyProperty SpecularPowerProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Color s_Color;

	internal static Brush s_Brush;

	internal const double c_SpecularPower = 40.0;

	/// <summary>Gets or sets a value that filters the color properties of the material applied to the model.  </summary>
	/// <returns>The color with which to filter the material.</returns>
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

	/// <summary> Gets or sets the 2-D brush to apply to a specularly-lit 3-D model.  </summary>
	/// <returns>The brush to apply.</returns>
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

	/// <summary>Gets or sets a value that specifies the degree to which a material applied to a 3-D model reflects the lighting model as shine.  </summary>
	/// <returns>Relative contribution, for a material applied as a 2-D brush to a 3-D model, of the specular component of the lighting model.</returns>
	public double SpecularPower
	{
		get
		{
			return (double)GetValue(SpecularPowerProperty);
		}
		set
		{
			SetValueInternal(SpecularPowerProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.SpecularMaterial" /> class. </summary>
	public SpecularMaterial()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.SpecularMaterial" /> class with the specified brush and specular exponent.</summary>
	/// <param name="brush">The brush applied by the new <see cref="T:System.Windows.Media.Media3D.SpecularMaterial" />.</param>
	/// <param name="specularPower">The specular exponent.</param>
	public SpecularMaterial(Brush brush, double specularPower)
	{
		Brush = brush;
		SpecularPower = specularPower;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.SpecularMaterial" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new SpecularMaterial Clone()
	{
		return (SpecularMaterial)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.SpecularMaterial" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new SpecularMaterial CloneCurrentValue()
	{
		return (SpecularMaterial)base.CloneCurrentValue();
	}

	private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SpecularMaterial)d).PropertyChanged(ColorProperty);
	}

	private static void BrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		SpecularMaterial specularMaterial = (SpecularMaterial)d;
		Brush resource = (Brush)e.OldValue;
		Brush resource2 = (Brush)e.NewValue;
		if (specularMaterial.Dispatcher != null)
		{
			DUCE.IResource resource3 = specularMaterial;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					specularMaterial.ReleaseResource(resource, channel);
					specularMaterial.AddRefResource(resource2, channel);
				}
			}
		}
		specularMaterial.PropertyChanged(BrushProperty);
	}

	private static void SpecularPowerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SpecularMaterial)d).PropertyChanged(SpecularPowerProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new SpecularMaterial();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle hbrush = ((DUCE.IResource)Brush)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.MILCMD_SPECULARMATERIAL mILCMD_SPECULARMATERIAL = default(DUCE.MILCMD_SPECULARMATERIAL);
			mILCMD_SPECULARMATERIAL.Type = MILCMD.MilCmdSpecularMaterial;
			mILCMD_SPECULARMATERIAL.Handle = _duceResource.GetHandle(channel);
			mILCMD_SPECULARMATERIAL.color = CompositionResourceManager.ColorToMilColorF(Color);
			mILCMD_SPECULARMATERIAL.hbrush = hbrush;
			mILCMD_SPECULARMATERIAL.specularPower = SpecularPower;
			channel.SendCommand((byte*)(&mILCMD_SPECULARMATERIAL), sizeof(DUCE.MILCMD_SPECULARMATERIAL));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_SPECULARMATERIAL))
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

	static SpecularMaterial()
	{
		s_Color = Colors.White;
		s_Brush = null;
		Type typeFromHandle = typeof(SpecularMaterial);
		ColorProperty = Animatable.RegisterProperty("Color", typeof(Color), typeFromHandle, Colors.White, ColorPropertyChanged, null, isIndependentlyAnimated: false, null);
		BrushProperty = Animatable.RegisterProperty("Brush", typeof(Brush), typeFromHandle, null, BrushPropertyChanged, null, isIndependentlyAnimated: false, null);
		SpecularPowerProperty = Animatable.RegisterProperty("SpecularPower", typeof(double), typeFromHandle, 40.0, SpecularPowerPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
