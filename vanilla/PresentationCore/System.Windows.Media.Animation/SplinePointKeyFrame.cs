using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Animates from the <see cref="T:System.Windows.Point" /> value of the previous key frame to its own <see cref="P:System.Windows.Media.Animation.PointKeyFrame.Value" /> using splined interpolation.  </summary>
public class SplinePointKeyFrame : PointKeyFrame
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.SplinePointKeyFrame.KeySpline" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.SplinePointKeyFrame.KeySpline" /> dependency property.</returns>
	public static readonly DependencyProperty KeySplineProperty = DependencyProperty.Register("KeySpline", typeof(KeySpline), typeof(SplinePointKeyFrame), new PropertyMetadata(new KeySpline()));

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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SplinePointKeyFrame" /> class. </summary>
	public SplinePointKeyFrame()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SplinePointKeyFrame" /> class with the specified ending value.  </summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	public SplinePointKeyFrame(Point value)
		: this()
	{
		base.Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SplinePointKeyFrame" /> class with the specified ending value and key time. </summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">Key time for the key frame. The key time determines when the target value is reached, which is also when the key frame ends.</param>
	public SplinePointKeyFrame(Point value, KeyTime keyTime)
		: this()
	{
		base.Value = value;
		base.KeyTime = keyTime;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SplinePointKeyFrame" /> class with the specified ending value, key time, and <see cref="T:System.Windows.Media.Animation.KeySpline" />. </summary>
	/// <param name="value">Ending value (also known as "target value") for the key frame.</param>
	/// <param name="keyTime">Key time for the key frame. The key time determines when the target value is reached, which is also when the key frame ends.</param>
	/// <param name="keySpline">
	///   <see cref="T:System.Windows.Media.Animation.KeySpline" /> for the key frame. The <see cref="T:System.Windows.Media.Animation.KeySpline" /> represents a Bezier curve that defines animation progress of the key frame.</param>
	public SplinePointKeyFrame(Point value, KeyTime keyTime, KeySpline keySpline)
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

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.SplinePointKeyFrame" />.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new SplinePointKeyFrame();
	}

	/// <summary>Uses splined interpolation to transition between the previous key frame value and the value of the current key frame. </summary>
	/// <returns>The output value of this key frame given the specified base value and progress.</returns>
	/// <param name="baseValue">The value to animate from.</param>
	/// <param name="keyFrameProgress">A value from 0.0 through 1.0 that specifies the percentage of time that has elapsed for this key frame.</param>
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
		double splineProgress = KeySpline.GetSplineProgress(keyFrameProgress);
		return AnimatedTypeHelpers.InterpolatePoint(baseValue, base.Value, splineProgress);
	}
}
