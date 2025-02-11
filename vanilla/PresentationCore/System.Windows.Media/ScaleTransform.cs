using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media;

/// <summary>Scales an object in the 2-D x-y coordinate system. </summary>
public sealed class ScaleTransform : Transform
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.ScaleTransform.ScaleX" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.ScaleTransform.ScaleX" /> dependency property.</returns>
	public static readonly DependencyProperty ScaleXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.ScaleTransform.ScaleY" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.ScaleTransform.ScaleY" /> dependency property.</returns>
	public static readonly DependencyProperty ScaleYProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.ScaleTransform.CenterX" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.ScaleTransform.CenterX" /> dependency property.</returns>
	public static readonly DependencyProperty CenterXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.ScaleTransform.CenterY" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.ScaleTransform.CenterY" /> dependency property.</returns>
	public static readonly DependencyProperty CenterYProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_ScaleX = 1.0;

	internal const double c_ScaleY = 1.0;

	internal const double c_CenterX = 0.0;

	internal const double c_CenterY = 0.0;

	/// <summary>Gets the current scaling transformation as a <see cref="T:System.Windows.Media.Matrix" /> object. </summary>
	/// <returns>The current scaling transformation returned as a <see cref="T:System.Windows.Media.Matrix" /> object.</returns>
	public override Matrix Value
	{
		get
		{
			ReadPreamble();
			Matrix result = default(Matrix);
			result.ScaleAt(ScaleX, ScaleY, CenterX, CenterY);
			return result;
		}
	}

	internal override bool IsIdentity
	{
		get
		{
			if (ScaleX == 1.0 && ScaleY == 1.0)
			{
				return base.CanFreeze;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the x-axis scale factor.  </summary>
	/// <returns>The scale factor along the x-axis. The default is 1.</returns>
	public double ScaleX
	{
		get
		{
			return (double)GetValue(ScaleXProperty);
		}
		set
		{
			SetValueInternal(ScaleXProperty, value);
		}
	}

	/// <summary>Gets or sets the y-axis scale factor.  </summary>
	/// <returns>The scale factor along the y-axis. The default is 1.</returns>
	public double ScaleY
	{
		get
		{
			return (double)GetValue(ScaleYProperty);
		}
		set
		{
			SetValueInternal(ScaleYProperty, value);
		}
	}

	/// <summary>Gets or sets the x-coordinate of the center point of this <see cref="T:System.Windows.Media.ScaleTransform" />.  </summary>
	/// <returns>The x-coordinate of the center point of this <see cref="T:System.Windows.Media.ScaleTransform" />. The default is 0.</returns>
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

	/// <summary>Gets or sets the y-coordinate of the center point of this <see cref="T:System.Windows.Media.ScaleTransform" />.  </summary>
	/// <returns>The y-coordinate of the center point of this <see cref="T:System.Windows.Media.ScaleTransform" />. The default is 0.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.ScaleTransform" /> class. </summary>
	public ScaleTransform()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.ScaleTransform" /> class with the specified x- and y- scale factors. The scale operation is centered on (0,0). </summary>
	/// <param name="scaleX">The x-axis scale factor.</param>
	/// <param name="scaleY">The y-axis scale factor.</param>
	public ScaleTransform(double scaleX, double scaleY)
	{
		ScaleX = scaleX;
		ScaleY = scaleY;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.ScaleTransform" /> class that has the specified scale factors and center point.</summary>
	/// <param name="scaleX">The x-axis scale factor. For more information, see the <see cref="P:System.Windows.Media.ScaleTransform.ScaleX" /> property.</param>
	/// <param name="scaleY">The y-axis scale factor. For more information, see the <see cref="P:System.Windows.Media.ScaleTransform.ScaleY" /> property.</param>
	/// <param name="centerX">The x-coordinate of the center of this <see cref="T:System.Windows.Media.ScaleTransform" />. For more information, see the <see cref="P:System.Windows.Media.ScaleTransform.CenterX" /> property.</param>
	/// <param name="centerY">The y-coordinate of the center of this <see cref="T:System.Windows.Media.ScaleTransform" />. For more information, see the <see cref="P:System.Windows.Media.ScaleTransform.CenterY" /> property.</param>
	public ScaleTransform(double scaleX, double scaleY, double centerX, double centerY)
		: this(scaleX, scaleY)
	{
		CenterX = centerX;
		CenterY = centerY;
	}

	internal override void TransformRect(ref Rect rect)
	{
		if (rect.IsEmpty)
		{
			return;
		}
		double scaleX = ScaleX;
		double scaleY = ScaleY;
		double centerX = CenterX;
		double centerY = CenterY;
		int num;
		if (centerX == 0.0)
		{
			num = ((centerY != 0.0) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0062;
			}
		}
		else
		{
			num = 1;
		}
		rect.X -= centerX;
		rect.Y -= centerY;
		goto IL_0062;
		IL_0062:
		rect.Scale(scaleX, scaleY);
		if (num != 0)
		{
			rect.X += centerX;
			rect.Y += centerY;
		}
	}

	internal override void MultiplyValueByMatrix(ref Matrix result, ref Matrix matrixToMultiplyBy)
	{
		result = Matrix.Identity;
		result._m11 = ScaleX;
		result._m22 = ScaleY;
		double centerX = CenterX;
		double centerY = CenterY;
		result._type = MatrixTypes.TRANSFORM_IS_SCALING;
		if (centerX != 0.0 || centerY != 0.0)
		{
			result._offsetX = centerX - centerX * result._m11;
			result._offsetY = centerY - centerY * result._m22;
			result._type |= MatrixTypes.TRANSFORM_IS_TRANSLATION;
		}
		MatrixUtil.MultiplyMatrix(ref result, ref matrixToMultiplyBy);
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.ScaleTransform" /> by making deep copies of its values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new ScaleTransform Clone()
	{
		return (ScaleTransform)base.Clone();
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.ScaleTransform" /> object by making deep copies of its values. This method does not copy resource references, data bindings, or animations, although it does copy their current values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new ScaleTransform CloneCurrentValue()
	{
		return (ScaleTransform)base.CloneCurrentValue();
	}

	private static void ScaleXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ScaleTransform)d).PropertyChanged(ScaleXProperty);
	}

	private static void ScaleYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ScaleTransform)d).PropertyChanged(ScaleYProperty);
	}

	private static void CenterXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ScaleTransform)d).PropertyChanged(CenterXProperty);
	}

	private static void CenterYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ScaleTransform)d).PropertyChanged(CenterYProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new ScaleTransform();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(ScaleXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(ScaleYProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(CenterXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle4 = GetAnimationResourceHandle(CenterYProperty, channel);
			DUCE.MILCMD_SCALETRANSFORM mILCMD_SCALETRANSFORM = default(DUCE.MILCMD_SCALETRANSFORM);
			mILCMD_SCALETRANSFORM.Type = MILCMD.MilCmdScaleTransform;
			mILCMD_SCALETRANSFORM.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_SCALETRANSFORM.ScaleX = ScaleX;
			}
			mILCMD_SCALETRANSFORM.hScaleXAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_SCALETRANSFORM.ScaleY = ScaleY;
			}
			mILCMD_SCALETRANSFORM.hScaleYAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_SCALETRANSFORM.CenterX = CenterX;
			}
			mILCMD_SCALETRANSFORM.hCenterXAnimations = animationResourceHandle3;
			if (animationResourceHandle4.IsNull)
			{
				mILCMD_SCALETRANSFORM.CenterY = CenterY;
			}
			mILCMD_SCALETRANSFORM.hCenterYAnimations = animationResourceHandle4;
			channel.SendCommand((byte*)(&mILCMD_SCALETRANSFORM), sizeof(DUCE.MILCMD_SCALETRANSFORM));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_SCALETRANSFORM))
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

	static ScaleTransform()
	{
		Type typeFromHandle = typeof(ScaleTransform);
		ScaleXProperty = Animatable.RegisterProperty("ScaleX", typeof(double), typeFromHandle, 1.0, ScaleXPropertyChanged, null, isIndependentlyAnimated: true, null);
		ScaleYProperty = Animatable.RegisterProperty("ScaleY", typeof(double), typeFromHandle, 1.0, ScaleYPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterXProperty = Animatable.RegisterProperty("CenterX", typeof(double), typeFromHandle, 0.0, CenterXPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterYProperty = Animatable.RegisterProperty("CenterY", typeof(double), typeFromHandle, 0.0, CenterYPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
