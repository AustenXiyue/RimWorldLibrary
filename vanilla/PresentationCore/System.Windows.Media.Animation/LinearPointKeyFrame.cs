using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Animates from the <see cref="T:System.Windows.Point" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.PointKeyFrame.Value" /> using linear interpolation.  </summary>
public class LinearPointKeyFrame : PointKeyFrame
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.LinearPointKeyFrame" /> class.</summary>
	public LinearPointKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.LinearPointKeyFrame" /> class with the specified ending value. </summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	public LinearPointKeyFrame(Point value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.LinearPointKeyFrame" /> class with the specified ending value and key time.</summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">Key time for the key frame. The key time determines when the target value is reached which is also when the key frame ends.</param>
	public LinearPointKeyFrame(Point value, KeyTime keyTime)
		: base(value, keyTime)
	{
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.LinearPointKeyFrame" />.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new LinearPointKeyFrame();
	}

	/// <summary>Interpolates, in a linear fashion, between the previous key frame value and the value of the current key frame, using the supplied progress increment. </summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override Point InterpolateValueCore(Point baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress == 0.0)
		{
			return baseValue;
		}
		if (keyFrameProgress == 1.0)
		{
			return base.Value;
		}
		return AnimatedTypeHelpers.InterpolatePoint(baseValue, base.Value, keyFrameProgress);
	}
}
