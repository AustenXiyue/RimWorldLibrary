namespace System.Windows.Media.Animation;

/// <summary>Abstract class that, when implemented, defines an animation segment with its own target value and interpolation method for a <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />. </summary>
public abstract class Int32KeyFrame : Freezable, IKeyFrame
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.KeyTime" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.KeyTime" /> dependency property.</returns>
	public static readonly DependencyProperty KeyTimeProperty = DependencyProperty.Register("KeyTime", typeof(KeyTime), typeof(Int32KeyFrame), new PropertyMetadata(KeyTime.Uniform));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.Value" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.Value" /> dependency property.</returns>
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(Int32KeyFrame), new PropertyMetadata());

	/// <summary>Gets or sets the time at which the key frame's target <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.Value" /> should be reached.   </summary>
	/// <returns>The time at which the key frame's current value should be equal to its <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.Value" /> property. The default value is <see cref="P:System.Windows.Media.Animation.KeyTime.Uniform" />.</returns>
	public KeyTime KeyTime
	{
		get
		{
			return (KeyTime)GetValue(KeyTimeProperty);
		}
		set
		{
			SetValueInternal(KeyTimeProperty, value);
		}
	}

	/// <summary>See <see cref="P:System.Windows.Media.Animation.IKeyFrame.Value" />.</summary>
	/// <returns> </returns>
	object IKeyFrame.Value
	{
		get
		{
			return Value;
		}
		set
		{
			Value = (int)value;
		}
	}

	/// <summary>Gets or sets the key frame's target value.  </summary>
	/// <returns>The key frame's target value, which is the value of this key frame at its specified <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.KeyTime" />. The default value is 0.</returns>
	public int Value
	{
		get
		{
			return (int)GetValue(ValueProperty);
		}
		set
		{
			SetValueInternal(ValueProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Int32KeyFrame" /> class.</summary>
	protected Int32KeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Int32KeyFrame" /> class that has the specified target <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.Value" />.  </summary>
	/// <param name="value">The <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.Value" /> of the new <see cref="T:System.Windows.Media.Animation.Int32KeyFrame" /> instance.</param>
	protected Int32KeyFrame(int value)
		: this()
	{
		Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Int32KeyFrame" /> class that has the specified target <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.Value" /> and <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.KeyTime" />.  </summary>
	/// <param name="value">The <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.Value" /> of the new <see cref="T:System.Windows.Media.Animation.Int32KeyFrame" /> instance.</param>
	/// <param name="keyTime">The <see cref="P:System.Windows.Media.Animation.Int32KeyFrame.KeyTime" /> of the new <see cref="T:System.Windows.Media.Animation.Int32KeyFrame" /> instance.</param>
	protected Int32KeyFrame(int value, KeyTime keyTime)
		: this()
	{
		Value = value;
		KeyTime = keyTime;
	}

	/// <summary>Returns the interpolated value of a specific key frame at the progress increment provided.</summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Occurs if <paramref name="keyFrameProgress" /> is not between 0.0 and 1.0, inclusive.</exception>
	public int InterpolateValue(int baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress < 0.0 || keyFrameProgress > 1.0)
		{
			throw new ArgumentOutOfRangeException("keyFrameProgress");
		}
		return InterpolateValueCore(baseValue, keyFrameProgress);
	}

	/// <summary>Calculates the value of a key frame at the progress increment provided.</summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from; typically the value of the previous key frame.</param>
	/// <param name="keyFrameProgress">A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.</param>
	protected abstract int InterpolateValueCore(int baseValue, double keyFrameProgress);
}
