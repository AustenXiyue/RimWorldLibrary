using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Represents a 2-D skew. </summary>
public sealed class SkewTransform : Transform
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.SkewTransform.AngleX" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.SkewTransform.AngleX" /> dependency property.</returns>
	public static readonly DependencyProperty AngleXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.SkewTransform.AngleY" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.SkewTransform.AngleY" /> dependency property.</returns>
	public static readonly DependencyProperty AngleYProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.SkewTransform.CenterX" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.SkewTransform.CenterX" /> dependency property.</returns>
	public static readonly DependencyProperty CenterXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.SkewTransform.CenterY" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.SkewTransform.CenterY" /> dependency property.</returns>
	public static readonly DependencyProperty CenterYProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_AngleX = 0.0;

	internal const double c_AngleY = 0.0;

	internal const double c_CenterX = 0.0;

	internal const double c_CenterY = 0.0;

	/// <summary>Gets the current transformation value as a <see cref="T:System.Windows.Media.Matrix" />. </summary>
	/// <returns>The current skewing transformation value as a <see cref="T:System.Windows.Media.Matrix" />.</returns>
	public override Matrix Value
	{
		get
		{
			ReadPreamble();
			Matrix result = default(Matrix);
			double angleX = AngleX;
			double angleY = AngleY;
			double centerX = CenterX;
			double centerY = CenterY;
			int num;
			if (centerX == 0.0)
			{
				num = ((centerY != 0.0) ? 1 : 0);
				if (num == 0)
				{
					goto IL_0059;
				}
			}
			else
			{
				num = 1;
			}
			result.Translate(0.0 - centerX, 0.0 - centerY);
			goto IL_0059;
			IL_0059:
			result.Skew(angleX, angleY);
			if (num != 0)
			{
				result.Translate(centerX, centerY);
			}
			return result;
		}
	}

	internal override bool IsIdentity
	{
		get
		{
			if (AngleX == 0.0 && AngleY == 0.0)
			{
				return base.CanFreeze;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the x-axis skew angle, which is measured in degrees counterclockwise from the y-axis.  </summary>
	/// <returns>The skew angle, which is measured in degrees counterclockwise from the y-axis. The default is 0.</returns>
	public double AngleX
	{
		get
		{
			return (double)GetValue(AngleXProperty);
		}
		set
		{
			SetValueInternal(AngleXProperty, value);
		}
	}

	/// <summary>Gets or sets the y-axis skew angle, which is measured in degrees counterclockwise from the x-axis.  </summary>
	/// <returns>The skew angle, which is measured in degrees counterclockwise from the x-axis. The default is 0.</returns>
	public double AngleY
	{
		get
		{
			return (double)GetValue(AngleYProperty);
		}
		set
		{
			SetValueInternal(AngleYProperty, value);
		}
	}

	/// <summary>Gets or sets the x-coordinate of the transform center.  </summary>
	/// <returns>The x-coordinate of the transform center. The default is 0.</returns>
	public double CenterX
	{
		get
		{
			return (double)GetValue(CenterXProperty);
		}
		set
		{
			SetValueInternal(CenterXProperty, value);
		}
	}

	/// <summary>Gets or sets the y-coordinate of the transform center.  </summary>
	/// <returns>The y-coordinate of the transform center. The default is 0.</returns>
	public double CenterY
	{
		get
		{
			return (double)GetValue(CenterYProperty);
		}
		set
		{
			SetValueInternal(CenterYProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.SkewTransform" /> class.</summary>
	public SkewTransform()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.SkewTransform" /> class that has the specified x- and y-axes angles and is centered on the origin.</summary>
	/// <param name="angleX">The x-axis skew angle, which is measured in degrees counterclockwise from the y-axis. For more information, see the <see cref="P:System.Windows.Media.SkewTransform.AngleX" /> property.</param>
	/// <param name="angleY">The y-axis skew angle, which is measured in degrees counterclockwise from the x-axis. For more information, see the <see cref="P:System.Windows.Media.SkewTransform.AngleY" /> property.</param>
	public SkewTransform(double angleX, double angleY)
	{
		AngleX = angleX;
		AngleY = angleY;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.SkewTransform" /> class that has the specified x- and y-axes angles and center. </summary>
	/// <param name="angleX">The x-axis skew angle, which is measured in degrees counterclockwise from the y-axis. For more information, see the <see cref="P:System.Windows.Media.SkewTransform.AngleX" /> property.</param>
	/// <param name="angleY">The y-axis skew angle, which is measured in degrees counterclockwise from the x-axis. For more information, see the <see cref="P:System.Windows.Media.SkewTransform.AngleY" /> property.</param>
	/// <param name="centerX">The x-coordinate of the transform center. For more information, see the <see cref="P:System.Windows.Media.SkewTransform.CenterX" /> property.</param>
	/// <param name="centerY">The y-coordinate of the transform center. For more information, see the <see cref="P:System.Windows.Media.SkewTransform.CenterY" /> property.</param>
	public SkewTransform(double angleX, double angleY, double centerX, double centerY)
		: this(angleX, angleY)
	{
		CenterX = centerX;
		CenterY = centerY;
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.SkewTransform" /> by making deep copies of its values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new SkewTransform Clone()
	{
		return (SkewTransform)base.Clone();
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.SkewTransform" /> object by making deep copies of its values. This method does not copy resource references, data bindings, or animations, although it does copy their current values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new SkewTransform CloneCurrentValue()
	{
		return (SkewTransform)base.CloneCurrentValue();
	}

	private static void AngleXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SkewTransform)d).PropertyChanged(AngleXProperty);
	}

	private static void AngleYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SkewTransform)d).PropertyChanged(AngleYProperty);
	}

	private static void CenterXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SkewTransform)d).PropertyChanged(CenterXProperty);
	}

	private static void CenterYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SkewTransform)d).PropertyChanged(CenterYProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new SkewTransform();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(AngleXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(AngleYProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(CenterXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle4 = GetAnimationResourceHandle(CenterYProperty, channel);
			DUCE.MILCMD_SKEWTRANSFORM mILCMD_SKEWTRANSFORM = default(DUCE.MILCMD_SKEWTRANSFORM);
			mILCMD_SKEWTRANSFORM.Type = MILCMD.MilCmdSkewTransform;
			mILCMD_SKEWTRANSFORM.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_SKEWTRANSFORM.AngleX = AngleX;
			}
			mILCMD_SKEWTRANSFORM.hAngleXAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_SKEWTRANSFORM.AngleY = AngleY;
			}
			mILCMD_SKEWTRANSFORM.hAngleYAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_SKEWTRANSFORM.CenterX = CenterX;
			}
			mILCMD_SKEWTRANSFORM.hCenterXAnimations = animationResourceHandle3;
			if (animationResourceHandle4.IsNull)
			{
				mILCMD_SKEWTRANSFORM.CenterY = CenterY;
			}
			mILCMD_SKEWTRANSFORM.hCenterYAnimations = animationResourceHandle4;
			channel.SendCommand((byte*)(&mILCMD_SKEWTRANSFORM), sizeof(DUCE.MILCMD_SKEWTRANSFORM));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_SKEWTRANSFORM))
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

	static SkewTransform()
	{
		Type typeFromHandle = typeof(SkewTransform);
		AngleXProperty = Animatable.RegisterProperty("AngleX", typeof(double), typeFromHandle, 0.0, AngleXPropertyChanged, null, isIndependentlyAnimated: true, null);
		AngleYProperty = Animatable.RegisterProperty("AngleY", typeof(double), typeFromHandle, 0.0, AngleYPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterXProperty = Animatable.RegisterProperty("CenterX", typeof(double), typeFromHandle, 0.0, CenterXPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterYProperty = Animatable.RegisterProperty("CenterY", typeof(double), typeFromHandle, 0.0, CenterYPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
