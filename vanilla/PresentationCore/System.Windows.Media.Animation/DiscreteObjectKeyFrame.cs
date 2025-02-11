namespace System.Windows.Media.Animation;

/// <summary>Animates from the <see cref="T:System.Object" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.ObjectKeyFrame.Value" /> using discrete interpolation.  </summary>
public class DiscreteObjectKeyFrame : ObjectKeyFrame
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteObjectKeyFrame" /> class.</summary>
	public DiscreteObjectKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteObjectKeyFrame" /> class with the specified ending value. </summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	public DiscreteObjectKeyFrame(object value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteObjectKeyFrame" /> class with the specified ending value and key time.</summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">Key time for the key frame. The key time determines when the target value is reached which is also when the key frame ends.</param>
	public DiscreteObjectKeyFrame(object value, KeyTime keyTime)
		: base(value, keyTime)
	{
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.DiscreteObjectKeyFrame" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Animation.DiscreteObjectKeyFrame" />.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new DiscreteObjectKeyFrame();
	}

	/// <summary>Uses discrete interpolation to transition between the previous key frame value and the value of the current key frame. </summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override object InterpolateValueCore(object baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress < 1.0)
		{
			return baseValue;
		}
		return base.Value;
	}
}
