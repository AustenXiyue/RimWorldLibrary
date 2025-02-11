namespace System.Windows.Media.Animation;

/// <summary>Animates from the <see cref="T:System.Windows.Vector" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.VectorKeyFrame.Value" /> using discrete interpolation.  </summary>
public class DiscreteVectorKeyFrame : VectorKeyFrame
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteVectorKeyFrame" /> class.</summary>
	public DiscreteVectorKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteVectorKeyFrame" /> class with the specified ending value. </summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	public DiscreteVectorKeyFrame(Vector value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteVectorKeyFrame" /> class with the specified ending value and key time.</summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">Key time for the key frame. The key time determines when the target value is reached which is also when the key frame ends.</param>
	public DiscreteVectorKeyFrame(Vector value, KeyTime keyTime)
		: base(value, keyTime)
	{
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.DiscreteVectorKeyFrame" />.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new DiscreteVectorKeyFrame();
	}

	/// <summary>Uses discrete interpolation to transition between the previous key frame value and the value of the current key frame.</summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override Vector InterpolateValueCore(Vector baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress < 1.0)
		{
			return baseValue;
		}
		return base.Value;
	}
}
