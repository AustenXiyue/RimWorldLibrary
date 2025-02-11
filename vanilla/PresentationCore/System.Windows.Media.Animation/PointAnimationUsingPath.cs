namespace System.Windows.Media.Animation;

/// <summary>Animates the value of a <see cref="T:System.Windows.Point" /> property between two or more target values using a <see cref="T:System.Windows.Media.PathGeometry" /> to specify those values. This animation can be used to move a visual object along a path.</summary>
public class PointAnimationUsingPath : PointAnimationBase
{
	private bool _isValid;

	private Vector _accumulatingVector;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.PointAnimationUsingPath.PathGeometry" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.PointAnimationUsingPath.PathGeometry" /> dependency property.</returns>
	public static readonly DependencyProperty PathGeometryProperty = DependencyProperty.Register("PathGeometry", typeof(PathGeometry), typeof(PointAnimationUsingPath), new PropertyMetadata((object)null));

	/// <summary>Specifies the geometry used to generate this animation's output values.  </summary>
	/// <returns>The path used to generate this animation's output values. The default value is null.</returns>
	public PathGeometry PathGeometry
	{
		get
		{
			return (PathGeometry)GetValue(PathGeometryProperty);
		}
		set
		{
			SetValue(PathGeometryProperty, value);
		}
	}

	/// <summary> Gets a value that specifies whether the animation's output value is added to the base value of the property being animated.  </summary>
	/// <returns>true if the animation adds its output value to the base value of the property being animated instead of replacing it; otherwise, false. The default value is false.</returns>
	public bool IsAdditive
	{
		get
		{
			return (bool)GetValue(AnimationTimeline.IsAdditiveProperty);
		}
		set
		{
			SetValue(AnimationTimeline.IsAdditiveProperty, value);
		}
	}

	/// <summary> Gets or sets a value that specifies whether the animation's value accumulates when it repeats.  </summary>
	/// <returns>true if the animation accumulates its values when its <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" /> property causes it to repeat its simple duration. otherwise, false. The default value is false.</returns>
	public bool IsCumulative
	{
		get
		{
			return (bool)GetValue(AnimationTimeline.IsCumulativeProperty);
		}
		set
		{
			SetValue(AnimationTimeline.IsCumulativeProperty, value);
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.Animation.PointAnimationUsingPath" /> class.</summary>
	public PointAnimationUsingPath()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.PointAnimationUsingPath" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PointAnimationUsingPath Clone()
	{
		return (PointAnimationUsingPath)base.Clone();
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.Animation.PointAnimationUsingPath" />.          </summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new PointAnimationUsingPath();
	}

	/// <summary>Called when this <see cref="T:System.Windows.Media.Animation.PointAnimationUsingPath" /> is modified.</summary>
	protected override void OnChanged()
	{
		_isValid = false;
		base.OnChanged();
	}

	/// <summary>Calculates a value that represents the current value of the property being animated, as determined by the <see cref="T:System.Windows.Media.Animation.PointAnimationUsingPath" />.  </summary>
	/// <returns>The calculated value of the property, as determined by the current animation.</returns>
	/// <param name="defaultOriginValue">The suggested origin value, used if the animation does not have its own explicitly set start value.</param>
	/// <param name="defaultDestinationValue">The suggested destination value, used if the animation does not have its own explicitly set end value.</param>
	/// <param name="animationClock">An <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that generates the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> used by the animation.</param>
	protected override Point GetCurrentValueCore(Point defaultOriginValue, Point defaultDestinationValue, AnimationClock animationClock)
	{
		PathGeometry pathGeometry = PathGeometry;
		if (pathGeometry == null)
		{
			return defaultDestinationValue;
		}
		if (!_isValid)
		{
			Validate();
		}
		pathGeometry.GetPointAtFractionLength(animationClock.CurrentProgress.Value, out var point, out var _);
		double num = (animationClock.CurrentIteration - 1).Value;
		if (IsCumulative && num > 0.0)
		{
			point += _accumulatingVector * num;
		}
		if (IsAdditive)
		{
			return defaultOriginValue + (Vector)point;
		}
		return point;
	}

	private void Validate()
	{
		if (IsCumulative)
		{
			PathGeometry pathGeometry = PathGeometry;
			pathGeometry.GetPointAtFractionLength(0.0, out var point, out var _);
			pathGeometry.GetPointAtFractionLength(1.0, out var point2, out var _);
			_accumulatingVector.X = point2.X - point.X;
			_accumulatingVector.Y = point2.Y - point.Y;
		}
		_isValid = true;
	}
}
