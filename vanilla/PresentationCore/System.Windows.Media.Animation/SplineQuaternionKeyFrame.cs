using System.Windows.Media.Media3D;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Animates from the <see cref="T:System.Windows.Media.Media3D.Quaternion" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.QuaternionKeyFrame.Value" /> using splined interpolation.  </summary>
public class SplineQuaternionKeyFrame : QuaternionKeyFrame
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.SplineQuaternionKeyFrame.UseShortestPath" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.SplineQuaternionKeyFrame.UseShortestPath" /> dependency property.</returns>
	public static readonly DependencyProperty UseShortestPathProperty = DependencyProperty.Register("UseShortestPath", typeof(bool), typeof(SplineQuaternionKeyFrame), new PropertyMetadata(BooleanBoxes.TrueBox));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.SplineQuaternionKeyFrame.KeySpline" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.SplineQuaternionKeyFrame.KeySpline" /> dependency property.</returns>
	public static readonly DependencyProperty KeySplineProperty = DependencyProperty.Register("KeySpline", typeof(KeySpline), typeof(SplineQuaternionKeyFrame), new PropertyMetadata(new KeySpline()));

	/// <summary>Gets or sets a value that indicates whether the animation uses spherical linear interpolation to calculate the shortest arc between positions.  </summary>
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

	/// <summary>Gets or sets the two control points that define animation progress for this key frame.  </summary>
	/// <returns>The two control points that specify the cubic Bezier curve that defines the progress of the key frame.</returns>
	public KeySpline KeySpline
	{
		get
		{
			return (KeySpline)GetValue(KeySplineProperty);
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			SetValue(KeySplineProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SplineQuaternionKeyFrame" /> class. </summary>
	public SplineQuaternionKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SplineQuaternionKeyFrame" /> class with the specified ending value.  </summary>
	/// <param name="value">The ending value (also known as "target value") for the key frame.</param>
	public SplineQuaternionKeyFrame(Quaternion value)
		: this()
	{
		base.Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SplineQuaternionKeyFrame" /> class with the specified ending value and key time. </summary>
	/// <param name="value">The ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">The key time for the key frame. The key time determines when the target value is reached, which is also when the key frame ends.</param>
	public SplineQuaternionKeyFrame(Quaternion value, KeyTime keyTime)
		: this()
	{
		base.Value = value;
		base.KeyTime = keyTime;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SplineQuaternionKeyFrame" /> class with the specified ending value, key time, and <see cref="T:System.Windows.Media.Animation.KeySpline" />. </summary>
	/// <param name="value">The ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">The key time for the key frame. The key time determines when the target value is reached, which is also when the key frame ends.</param>
	/// <param name="keySpline">A <see cref="T:System.Windows.Media.Animation.KeySpline" /> for the key frame. The <see cref="T:System.Windows.Media.Animation.KeySpline" /> represents a Bezier curve that defines animation progress of the key frame.</param>
	public SplineQuaternionKeyFrame(Quaternion value, KeyTime keyTime, KeySpline keySpline)
		: this()
	{
		if (keySpline == null)
		{
			throw new ArgumentNullException("keySpline");
		}
		base.Value = value;
		base.KeyTime = keyTime;
		KeySpline = keySpline;
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.SplineQuaternionKeyFrame" />.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new SplineQuaternionKeyFrame();
	}

	/// <summary>Interpolates, in a splined fashion, between the previous key frame value and the value of the current key frame, using the supplied progress increment.</summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value from 0.0 through 1.0 that specifies the percentage of time that has elapsed for this key frame.</param>
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
		double splineProgress = KeySpline.GetSplineProgress(keyFrameProgress);
		return AnimatedTypeHelpers.InterpolateQuaternion(baseValue, base.Value, splineProgress, UseShortestPath);
	}
}
