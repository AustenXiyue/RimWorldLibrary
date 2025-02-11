using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media;

/// <summary>Translates (moves) an object in the 2-D x-y coordinate system. </summary>
public sealed class TranslateTransform : Transform
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.TranslateTransform.X" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TranslateTransform.X" /> dependency property.</returns>
	public static readonly DependencyProperty XProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TranslateTransform.Y" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TranslateTransform.Y" /> dependency property.</returns>
	public static readonly DependencyProperty YProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_X = 0.0;

	internal const double c_Y = 0.0;

	/// <summary>Gets a <see cref="T:System.Windows.Media.Matrix" /> representation of this <see cref="T:System.Windows.Media.TranslateTransform" />.</summary>
	/// <returns>A matrix that represents this <see cref="T:System.Windows.Media.TranslateTransform" />.</returns>
	public override Matrix Value
	{
		get
		{
			ReadPreamble();
			Matrix identity = Matrix.Identity;
			identity.Translate(X, Y);
			return identity;
		}
	}

	internal override bool IsIdentity
	{
		get
		{
			if (X == 0.0 && Y == 0.0)
			{
				return base.CanFreeze;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the distance to translate along the x-axis.  </summary>
	/// <returns>The distance to translate (move) an object along the x-axis. The default value is 0.</returns>
	public double X
	{
		get
		{
			return (double)GetValue(XProperty);
		}
		set
		{
			SetValueInternal(XProperty, value);
		}
	}

	/// <summary>Gets or sets the distance to translate (move) an object along the y-axis.  </summary>
	/// <returns>The distance to translate (move) an object along the y-axis. The default value is 0.</returns>
	public double Y
	{
		get
		{
			return (double)GetValue(YProperty);
		}
		set
		{
			SetValueInternal(YProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TranslateTransform" /> class. </summary>
	public TranslateTransform()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TranslateTransform" /> class and specifies the displacements in the direction of the x- and y- axes. </summary>
	/// <param name="offsetX">The displacement in the direction of the x-axis.</param>
	/// <param name="offsetY">The displacement in the direction of the y-axis.</param>
	public TranslateTransform(double offsetX, double offsetY)
	{
		X = offsetX;
		Y = offsetY;
	}

	internal override void TransformRect(ref Rect rect)
	{
		if (!rect.IsEmpty)
		{
			rect.Offset(X, Y);
		}
	}

	internal override void MultiplyValueByMatrix(ref Matrix result, ref Matrix matrixToMultiplyBy)
	{
		result = Matrix.Identity;
		result._offsetX = X;
		result._offsetY = Y;
		result._type = MatrixTypes.TRANSFORM_IS_TRANSLATION;
		MatrixUtil.MultiplyMatrix(ref result, ref matrixToMultiplyBy);
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.TranslateTransform" /> by making deep copies of its values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new TranslateTransform Clone()
	{
		return (TranslateTransform)base.Clone();
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.TranslateTransform" /> object by making deep copies of its values. This method does not copy resource references, data bindings, and animations, although it does copy their current values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new TranslateTransform CloneCurrentValue()
	{
		return (TranslateTransform)base.CloneCurrentValue();
	}

	private static void XPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TranslateTransform)d).PropertyChanged(XProperty);
	}

	private static void YPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TranslateTransform)d).PropertyChanged(YProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new TranslateTransform();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(XProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(YProperty, channel);
			DUCE.MILCMD_TRANSLATETRANSFORM mILCMD_TRANSLATETRANSFORM = default(DUCE.MILCMD_TRANSLATETRANSFORM);
			mILCMD_TRANSLATETRANSFORM.Type = MILCMD.MilCmdTranslateTransform;
			mILCMD_TRANSLATETRANSFORM.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_TRANSLATETRANSFORM.X = X;
			}
			mILCMD_TRANSLATETRANSFORM.hXAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_TRANSLATETRANSFORM.Y = Y;
			}
			mILCMD_TRANSLATETRANSFORM.hYAnimations = animationResourceHandle2;
			channel.SendCommand((byte*)(&mILCMD_TRANSLATETRANSFORM), sizeof(DUCE.MILCMD_TRANSLATETRANSFORM));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_TRANSLATETRANSFORM))
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

	static TranslateTransform()
	{
		Type typeFromHandle = typeof(TranslateTransform);
		XProperty = Animatable.RegisterProperty("X", typeof(double), typeFromHandle, 0.0, XPropertyChanged, null, isIndependentlyAnimated: true, null);
		YProperty = Animatable.RegisterProperty("Y", typeof(double), typeFromHandle, 0.0, YPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
