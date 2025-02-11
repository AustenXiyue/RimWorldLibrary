using System.Windows.Media.Media3D;

namespace System.Windows.Media.Animation;

/// <summary>Animates from the <see cref="T:System.Windows.Media.Media3D.Vector3D" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.Vector3DKeyFrame.Value" /> using discrete interpolation.  </summary>
public class DiscreteVector3DKeyFrame : Vector3DKeyFrame
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteVector3DKeyFrame" /> class.</summary>
	public DiscreteVector3DKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteVector3DKeyFrame" /> class with the specified ending value. </summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	public DiscreteVector3DKeyFrame(Vector3D value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DiscreteVector3DKeyFrame" /> class with the specified ending value and key time.</summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">Key time for the key frame. The key time determines when the target value is reached, which is also when the key frame ends.</param>
	public DiscreteVector3DKeyFrame(Vector3D value, KeyTime keyTime)
		: base(value, keyTime)
	{
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.DiscreteVector3DKeyFrame" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Animation.DiscreteVector3DKeyFrame" />.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new DiscreteVector3DKeyFrame();
	}

	/// <summary>Uses discrete interpolation to transition between the previous key frame value and the value of the current key frame. </summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value from 0.0 through 1.0 that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override Vector3D InterpolateValueCore(Vector3D baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress < 1.0)
		{
			return baseValue;
		}
		return base.Value;
	}
}
