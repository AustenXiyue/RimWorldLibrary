namespace System.Windows.Media.Animation;

/// <summary>Abstract class that, when implemented, defines an animation segment with its own target value and interpolation method for a <see cref="T:System.Windows.Media.Animation.CharAnimationUsingKeyFrames" />. </summary>
public abstract class CharKeyFrame : Freezable, IKeyFrame
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.CharKeyFrame.KeyTime" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.CharKeyFrame.KeyTime" /> dependency property.</returns>
	public static readonly DependencyProperty KeyTimeProperty = DependencyProperty.Register("KeyTime", typeof(KeyTime), typeof(CharKeyFrame), new PropertyMetadata(KeyTime.Uniform));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.CharKeyFrame.Value" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.CharKeyFrame.Value" /> dependency property.</returns>
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(char), typeof(CharKeyFrame), new PropertyMetadata());

	/// <summary> Gets or sets the time at which the key frame's target <see cref="P:System.Windows.Media.Animation.CharKeyFrame.Value" /> should be reached.   </summary>
	/// <returns>The time at which the key frame's current value should be equal to its <see cref="P:System.Windows.Media.Animation.CharKeyFrame.Value" /> property. The default value is <see cref="P:System.Windows.Media.Animation.KeyTime.Uniform" />.</returns>
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

	/// <summary>Gets or sets the value associated with a <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance. </summary>
	/// <returns>The current value for this property. </returns>
	object IKeyFrame.Value
	{
		get
		{
			return Value;
		}
		set
		{
			Value = (char)value;
		}
	}

	/// <summary> Gets or sets the key frame's target value.   </summary>
	/// <returns>The key frame's target value, which is the value of this key frame at its specified <see cref="P:System.Windows.Media.Animation.CharKeyFrame.KeyTime" />. The default value is 0.</returns>
	public char Value
	{
		get
		{
			return (char)GetValue(ValueProperty);
		}
		set
		{
			SetValueInternal(ValueProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.CharKeyFrame" /> class.</summary>
	protected CharKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.CharKeyFrame" /> class that has the specified target <see cref="P:System.Windows.Media.Animation.CharKeyFrame.Value" />.  </summary>
	/// <param name="value">The <see cref="P:System.Windows.Media.Animation.CharKeyFrame.Value" /> of the new <see cref="T:System.Windows.Media.Animation.CharKeyFrame" /> instance.</param>
	protected CharKeyFrame(char value)
		: this()
	{
		Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.CharKeyFrame" /> class that has the specified target <see cref="P:System.Windows.Media.Animation.CharKeyFrame.Value" /> and <see cref="P:System.Windows.Media.Animation.CharKeyFrame.KeyTime" />.  </summary>
	/// <param name="value">The <see cref="P:System.Windows.Media.Animation.CharKeyFrame.Value" /> of the new <see cref="T:System.Windows.Media.Animation.CharKeyFrame" /> instance.</param>
	/// <param name="keyTime">The <see cref="P:System.Windows.Media.Animation.CharKeyFrame.KeyTime" /> of the new <see cref="T:System.Windows.Media.Animation.CharKeyFrame" /> instance.</param>
	protected CharKeyFrame(char value, KeyTime keyTime)
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
	public char InterpolateValue(char baseValue, double keyFrameProgress)
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
	protected abstract char InterpolateValueCore(char baseValue, double keyFrameProgress);
}
