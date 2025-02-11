using System.Windows.Media.Media3D;

namespace System.Windows.Media.Animation;

/// <summary> Animates from the <see cref="T:System.Windows.Media.Media3D.Rotation3D" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.Rotation3DKeyFrame.Value" /> using discrete interpolation.  </summary>
public class DiscreteRotation3DKeyFrame : Rotation3DKeyFrame
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteRotation3DKeyFrame" /> class. </summary>
	public DiscreteRotation3DKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteRotation3DKeyFrame" /> class with the specified ending value. </summary>
	/// <param name="value">The ending value (also known as "target value") for the key frame.</param>
	public DiscreteRotation3DKeyFrame(Rotation3D value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteRotation3DKeyFrame" /> class with the specified ending value and key time. </summary>
	/// <param name="value">The ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">The key time for the key frame. The key time determines when the target value is reached, which is also when the key frame ends.</param>
	public DiscreteRotation3DKeyFrame(Rotation3D value, KeyTime keyTime)
		: base(value, keyTime)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteRotation3DKeyFrame" /> class.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new DiscreteRotation3DKeyFrame();
	}

	/// <summary>Computes a discrete interpolation between the previous key frame value and the value of the current key frame, using the supplied progress increment. </summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value from 0.0 through 1.0 that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override Rotation3D InterpolateValueCore(Rotation3D baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress < 1.0)
		{
			return baseValue;
		}
		return base.Value;
	}
}
