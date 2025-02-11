using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Translates an object in the three-dimensional x-y-z plane. </summary>
public sealed class TranslateTransform3D : AffineTransform3D
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.TranslateTransform3D.OffsetX" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.TranslateTransform3D.OffsetX" /> dependency property.</returns>
	public static readonly DependencyProperty OffsetXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.TranslateTransform3D.OffsetY" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.TranslateTransform3D.OffsetY" /> dependency property.</returns>
	public static readonly DependencyProperty OffsetYProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.TranslateTransform3D.OffsetZ" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.TranslateTransform3D.OffsetZ" /> dependency property.</returns>
	public static readonly DependencyProperty OffsetZProperty;

	private double _cachedOffsetXValue;

	private double _cachedOffsetYValue;

	private double _cachedOffsetZValue;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_OffsetX = 0.0;

	internal const double c_OffsetY = 0.0;

	internal const double c_OffsetZ = 0.0;

	/// <summary>Gets a <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> that represents the value of the translation.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> that represents the value of the translation.</returns>
	public override Matrix3D Value
	{
		get
		{
			ReadPreamble();
			Matrix3D matrix = default(Matrix3D);
			Append(ref matrix);
			return matrix;
		}
	}

	/// <summary>Gets or sets the X-axis value of the translation's offset.  </summary>
	/// <returns>Double that represents the X-axis value of the translation's offset.</returns>
	public double OffsetX
	{
		get
		{
			ReadPreamble();
			return _cachedOffsetXValue;
		}
		set
		{
			SetValueInternal(OffsetXProperty, value);
		}
	}

	/// <summary>Gets or sets the Y-axis value of the translation's offset.  </summary>
	/// <returns>Double that represents the Y-axis value of the translation's offset.</returns>
	public double OffsetY
	{
		get
		{
			ReadPreamble();
			return _cachedOffsetYValue;
		}
		set
		{
			SetValueInternal(OffsetYProperty, value);
		}
	}

	/// <summary>Gets or sets the Z-axis value of the translation's offset.  </summary>
	/// <returns>Double that represents the Z-axis value of the translation's offset.</returns>
	public double OffsetZ
	{
		get
		{
			ReadPreamble();
			return _cachedOffsetZValue;
		}
		set
		{
			SetValueInternal(OffsetZProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.TranslateTransform3D" /> class. </summary>
	public TranslateTransform3D()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.TranslateTransform3D" /> class, using the specified offset <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
	/// <param name="offset">
	///   <see cref="T:System.Windows.Media.Media3D.Vector3D" /> by which to offset the model.</param>
	public TranslateTransform3D(Vector3D offset)
	{
		OffsetX = offset.X;
		OffsetY = offset.Y;
		OffsetZ = offset.Z;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.TranslateTransform3D" /> class using the specified offset.</summary>
	/// <param name="offsetX">Double that specifies the X value of the Vector3D that specifies the translation offset.</param>
	/// <param name="offsetY">Double that specifies the Y value of the Vector3D that specifies the translation offset.</param>
	/// <param name="offsetZ">Double that specifies the Z value of the Vector3D that specifies the translation offset.</param>
	public TranslateTransform3D(double offsetX, double offsetY, double offsetZ)
	{
		OffsetX = offsetX;
		OffsetY = offsetY;
		OffsetZ = offsetZ;
	}

	internal override void Append(ref Matrix3D matrix)
	{
		matrix.Translate(new Vector3D(_cachedOffsetXValue, _cachedOffsetYValue, _cachedOffsetZValue));
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.TranslateTransform3D" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TranslateTransform3D Clone()
	{
		return (TranslateTransform3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.TranslateTransform3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TranslateTransform3D CloneCurrentValue()
	{
		return (TranslateTransform3D)base.CloneCurrentValue();
	}

	private static void OffsetXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TranslateTransform3D obj = (TranslateTransform3D)d;
		obj._cachedOffsetXValue = (double)e.NewValue;
		obj.PropertyChanged(OffsetXProperty);
	}

	private static void OffsetYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TranslateTransform3D obj = (TranslateTransform3D)d;
		obj._cachedOffsetYValue = (double)e.NewValue;
		obj.PropertyChanged(OffsetYProperty);
	}

	private static void OffsetZPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TranslateTransform3D obj = (TranslateTransform3D)d;
		obj._cachedOffsetZValue = (double)e.NewValue;
		obj.PropertyChanged(OffsetZProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new TranslateTransform3D();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(OffsetXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(OffsetYProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(OffsetZProperty, channel);
			DUCE.MILCMD_TRANSLATETRANSFORM3D mILCMD_TRANSLATETRANSFORM3D = default(DUCE.MILCMD_TRANSLATETRANSFORM3D);
			mILCMD_TRANSLATETRANSFORM3D.Type = MILCMD.MilCmdTranslateTransform3D;
			mILCMD_TRANSLATETRANSFORM3D.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_TRANSLATETRANSFORM3D.offsetX = OffsetX;
			}
			mILCMD_TRANSLATETRANSFORM3D.hOffsetXAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_TRANSLATETRANSFORM3D.offsetY = OffsetY;
			}
			mILCMD_TRANSLATETRANSFORM3D.hOffsetYAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_TRANSLATETRANSFORM3D.offsetZ = OffsetZ;
			}
			mILCMD_TRANSLATETRANSFORM3D.hOffsetZAnimations = animationResourceHandle3;
			channel.SendCommand((byte*)(&mILCMD_TRANSLATETRANSFORM3D), sizeof(DUCE.MILCMD_TRANSLATETRANSFORM3D));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_TRANSLATETRANSFORM3D))
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

	static TranslateTransform3D()
	{
		Type typeFromHandle = typeof(TranslateTransform3D);
		OffsetXProperty = Animatable.RegisterProperty("OffsetX", typeof(double), typeFromHandle, 0.0, OffsetXPropertyChanged, null, isIndependentlyAnimated: true, null);
		OffsetYProperty = Animatable.RegisterProperty("OffsetY", typeof(double), typeFromHandle, 0.0, OffsetYPropertyChanged, null, isIndependentlyAnimated: true, null);
		OffsetZProperty = Animatable.RegisterProperty("OffsetZ", typeof(double), typeFromHandle, 0.0, OffsetZPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
