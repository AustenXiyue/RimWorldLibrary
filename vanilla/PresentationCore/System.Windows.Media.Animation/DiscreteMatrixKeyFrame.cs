namespace System.Windows.Media.Animation;

/// <summary>Animates from the <see cref="T:System.Windows.Media.Matrix" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.MatrixKeyFrame.Value" /> using discrete interpolation.  </summary>
public class DiscreteMatrixKeyFrame : MatrixKeyFrame
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteMatrixKeyFrame" /> class.</summary>
	public DiscreteMatrixKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteMatrixKeyFrame" /> class with the specified ending value. </summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	public DiscreteMatrixKeyFrame(Matrix value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteMatrixKeyFrame" /> class with the specified ending value and key time.</summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">Key time for the key frame. The key time determines when the target value is reached which is also when the key frame ends.</param>
	public DiscreteMatrixKeyFrame(Matrix value, KeyTime keyTime)
		: base(value, keyTime)
	{
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.DiscreteMatrixKeyFrame" />.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new DiscreteMatrixKeyFrame();
	}

	/// <summary>Uses discrete interpolation to transition between the previous key frame value and the value of the current key frame.</summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override Matrix InterpolateValueCore(Matrix baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress < 1.0)
		{
			return baseValue;
		}
		return base.Value;
	}
}
