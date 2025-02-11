using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Animates from the <see cref="T:System.Double" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.DoubleKeyFrame.Value" /> using linear interpolation.  </summary>
public class LinearDoubleKeyFrame : DoubleKeyFrame
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.LinearDoubleKeyFrame" /> class.</summary>
	public LinearDoubleKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.LinearDoubleKeyFrame" /> class with the specified ending value. </summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	public LinearDoubleKeyFrame(double value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.LinearDoubleKeyFrame" /> class with the specified ending value and key time.</summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">Key time for the key frame. The key time determines when the target value is reached which is also when the key frame ends.</param>
	public LinearDoubleKeyFrame(double value, KeyTime keyTime)
		: base(value, keyTime)
	{
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.LinearDoubleKeyFrame" />.</summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Animation.LinearDoubleKeyFrame" /> instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new LinearDoubleKeyFrame();
	}

	/// <summary>Interpolates, in a linear fashion, between the previous key frame value and the value of the current key frame, using the supplied progress increment. </summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override double InterpolateValueCore(double baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress == 0.0)
		{
			return baseValue;
		}
		if (keyFrameProgress == 1.0)
		{
			return base.Value;
		}
		return AnimatedTypeHelpers.InterpolateDouble(baseValue, base.Value, keyFrameProgress);
	}
}
