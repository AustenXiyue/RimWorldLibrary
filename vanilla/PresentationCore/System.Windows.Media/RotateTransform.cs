using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Rotates an object clockwise about a specified point in a 2-D x-y coordinate system. </summary>
public sealed class RotateTransform : Transform
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.RotateTransform.Angle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RotateTransform.Angle" /> dependency property.</returns>
	public static readonly DependencyProperty AngleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.RotateTransform.CenterX" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RotateTransform.CenterX" /> dependency property.</returns>
	public static readonly DependencyProperty CenterXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.RotateTransform.CenterY" /> dependency property.</summary>
	public static readonly DependencyProperty CenterYProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_Angle = 0.0;

	internal const double c_CenterX = 0.0;

	internal const double c_CenterY = 0.0;

	/// <summary>Gets the current rotation transformation as a <see cref="T:System.Windows.Media.Matrix" /> object. </summary>
	/// <returns>The current rotation transformation as a <see cref="T:System.Windows.Media.Matrix" />.</returns>
	public override Matrix Value
	{
		get
		{
			ReadPreamble();
			Matrix result = default(Matrix);
			result.RotateAt(Angle, CenterX, CenterY);
			return result;
		}
	}

	internal override bool IsIdentity
	{
		get
		{
			if (Angle == 0.0)
			{
				return base.CanFreeze;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the angle, in degrees, of clockwise rotation.  </summary>
	/// <returns>The angle, in degrees, of clockwise rotation. The default is 0.</returns>
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

	/// <summary>Gets or sets the x-coordinate of the rotation center point.  </summary>
	/// <returns>The x-coordinate of the center of rotation. The default is 0.</returns>
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

	/// <summary>Gets or sets the y-coordinate of the rotation center point.  </summary>
	/// <returns>The y-coordinate of the center of rotation. The default is 0.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.RotateTransform" /> class. </summary>
	public RotateTransform()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.RotateTransform" /> class that has the specified angle, in degrees, of clockwise rotation. The rotation is centered on the origin, (0,0).  </summary>
	/// <param name="angle">The clockwise rotation angle, in degrees.</param>
	public RotateTransform(double angle)
	{
		Angle = angle;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.RotateTransform" /> class that has the specified angle and center point.</summary>
	/// <param name="angle">The clockwise rotation angle, in degrees. For more information, see the <see cref="P:System.Windows.Media.RotateTransform.Angle" /> property.</param>
	/// <param name="centerX">The x-coordinate of the center point for the <see cref="T:System.Windows.Media.RotateTransform" />. For more information, see the <see cref="P:System.Windows.Media.RotateTransform.CenterX" /> property.</param>
	/// <param name="centerY">The y-coordinate of the center point for the <see cref="T:System.Windows.Media.RotateTransform" />. For more information, see the <see cref="P:System.Windows.Media.RotateTransform.CenterY" /> property.</param>
	public RotateTransform(double angle, double centerX, double centerY)
		: this(angle)
	{
		CenterX = centerX;
		CenterY = centerY;
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.RotateTransform" /> by making deep copies of its values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new RotateTransform Clone()
	{
		return (RotateTransform)base.Clone();
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.RotateTransform" /> object by making deep copies of its values. This method does not copy resource references, data bindings, or animations, although it does copy their current values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new RotateTransform CloneCurrentValue()
	{
		return (RotateTransform)base.CloneCurrentValue();
	}

	private static void AnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RotateTransform)d).PropertyChanged(AngleProperty);
	}

	private static void CenterXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RotateTransform)d).PropertyChanged(CenterXProperty);
	}

	private static void CenterYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RotateTransform)d).PropertyChanged(CenterYProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new RotateTransform();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(AngleProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(CenterXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(CenterYProperty, channel);
			DUCE.MILCMD_ROTATETRANSFORM mILCMD_ROTATETRANSFORM = default(DUCE.MILCMD_ROTATETRANSFORM);
			mILCMD_ROTATETRANSFORM.Type = MILCMD.MilCmdRotateTransform;
			mILCMD_ROTATETRANSFORM.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_ROTATETRANSFORM.Angle = Angle;
			}
			mILCMD_ROTATETRANSFORM.hAngleAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_ROTATETRANSFORM.CenterX = CenterX;
			}
			mILCMD_ROTATETRANSFORM.hCenterXAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_ROTATETRANSFORM.CenterY = CenterY;
			}
			mILCMD_ROTATETRANSFORM.hCenterYAnimations = animationResourceHandle3;
			channel.SendCommand((byte*)(&mILCMD_ROTATETRANSFORM), sizeof(DUCE.MILCMD_ROTATETRANSFORM));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_ROTATETRANSFORM))
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

	static RotateTransform()
	{
		Type typeFromHandle = typeof(RotateTransform);
		AngleProperty = Animatable.RegisterProperty("Angle", typeof(double), typeFromHandle, 0.0, AnglePropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterXProperty = Animatable.RegisterProperty("CenterX", typeof(double), typeFromHandle, 0.0, CenterXPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterYProperty = Animatable.RegisterProperty("CenterY", typeof(double), typeFromHandle, 0.0, CenterYPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
