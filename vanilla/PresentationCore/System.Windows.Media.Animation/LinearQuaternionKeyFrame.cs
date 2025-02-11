using System.Windows.Media.Media3D;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Animates from the <see cref="T:System.Windows.Media.Media3D.Quaternion" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.QuaternionKeyFrame.Value" /> using linear interpolation.</summary>
public class LinearQuaternionKeyFrame : QuaternionKeyFrame
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.LinearQuaternionKeyFrame.UseShortestPath" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.LinearQuaternionKeyFrame.UseShortestPath" />  dependency property.</returns>
	public static readonly DependencyProperty UseShortestPathProperty = DependencyProperty.Register("UseShortestPath", typeof(bool), typeof(LinearQuaternionKeyFrame), new PropertyMetadata(BooleanBoxes.TrueBox));

	/// <summary>Gets or sets a Boolean value that indicates whether the animation uses spherical linear interpolation to calculate the shortest arc between positions. This is a dependency property.</summary>
	/// <returns>Boolean value that indicates whether the animation uses spherical linear interpolation to calculate the shortest arc between positions.</returns>
	public bool UseShortestPath
	{
		get
		{
			return (bool)GetValue(UseShortestPathProperty);
		}
		set
		{
			SetValue(UseShortestPathProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.LinearQuaternionKeyFrame" /> class.</summary>
	public LinearQuaternionKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.LinearQuaternionKeyFrame" /> class with the specified ending value. </summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	public LinearQuaternionKeyFrame(Quaternion value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.LinearQuaternionKeyFrame" /> class with the specified ending value and key time.</summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">Key time for the key frame. The key time determines when the target value is reached which is also when the key frame ends.</param>
	public LinearQuaternionKeyFrame(Quaternion value, KeyTime keyTime)
		: base(value, keyTime)
	{
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.LinearQuaternionKeyFrame" />.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new LinearQuaternionKeyFrame();
	}

	/// <summary>Interpolates, in a linear fashion, between the previous key frame value and the value of the current key frame, using the supplied progress increment. </summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value between 0.0 and 1.0, inclusive, that specifies the percentage of time that has elapsed for this key frame.</param>
	protected override Quaternion InterpolateValueCore(Quaternion baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress == 0.0)
		{
			return baseValue;
		}
		if (keyFrameProgress == 1.0)
		{
			return base.Value;
		}
		return AnimatedTypeHelpers.InterpolateQuaternion(baseValue, base.Value, keyFrameProgress, UseShortestPath);
	}
}
