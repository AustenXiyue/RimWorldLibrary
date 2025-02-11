namespace System.Windows.Media.Animation;

/// <summary>Animates from the <see cref="T:System.Int16" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.Int16KeyFrame.Value" /> using discrete interpolation.  </summary>
public class DiscreteInt16KeyFrame : Int16KeyFrame
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteInt16KeyFrame" /> class.</summary>
	public DiscreteInt16KeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteInt16KeyFrame" /> class with the specified ending value. </summary>
	/// <param name="value">The ending value (also known as "target value") for the key frame.</param>
	public DiscreteInt16KeyFrame(short value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteInt16KeyFrame" /> class with the specified ending value and key time.</summary>
	/// <param name="value">The ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">The key time for the key frame. The key time determines when the target value is reached, which is also when the key frame ends.</param>
	public DiscreteInt16KeyFrame(short value, KeyTime keyTime)
		: base(value, keyTime)
	{
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.DiscreteInt16KeyFrame" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Animation.DiscreteInt16KeyFrame" />.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new DiscreteInt16KeyFrame();
	}

	/// <summary>Uses discrete interpolation to transition between the previous key frame value and the value of the current key frame. </summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value from 0.0 through 1.0 that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override short InterpolateValueCore(short baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress < 1.0)
		{
			return baseValue;
		}
		return base.Value;
	}
}
