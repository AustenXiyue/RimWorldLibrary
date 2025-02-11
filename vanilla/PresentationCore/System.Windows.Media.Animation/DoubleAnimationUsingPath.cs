namespace System.Windows.Media.Animation;

/// <summary>Animates the value of a <see cref="T:System.Double" /> property between two or more target values using a <see cref="T:System.Windows.Media.PathGeometry" /> to specify those values. This animation can be used to move a visual object along a path. </summary>
public class DoubleAnimationUsingPath : DoubleAnimationBase
{
	private bool _isValid;

	private double _accumulatingValue;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.DoubleAnimationUsingPath.PathGeometry" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.DoubleAnimationUsingPath.PathGeometry" /> dependency property.</returns>
	public static readonly DependencyProperty PathGeometryProperty = DependencyProperty.Register("PathGeometry", typeof(PathGeometry), typeof(DoubleAnimationUsingPath), new PropertyMetadata((object)null));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.DoubleAnimationUsingPath.Source" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.DoubleAnimationUsingPath.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(PathAnimationSource), typeof(DoubleAnimationUsingPath), new PropertyMetadata(PathAnimationSource.X));

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

	/// <summary>Gets or sets the aspect of this animation's <see cref="P:System.Windows.Media.Animation.DoubleAnimationUsingPath.PathGeometry" /> that determines its output value.  </summary>
	/// <returns>The aspect of this animation's <see cref="P:System.Windows.Media.Animation.DoubleAnimationUsingPath.PathGeometry" /> that determines its output value. The default value is <see cref="F:System.Windows.Media.Animation.PathAnimationSource.X" />.</returns>
	public PathAnimationSource Source
	{
		get
		{
			return (PathAnimationSource)GetValue(SourceProperty);
		}
		set
		{
			SetValue(SourceProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the target property's current value should be added to this animation's starting value.  </summary>
	/// <returns>true if the target property's current value should be added to this animation's starting value; otherwise, false. The default value is false.</returns>
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

	/// <summary>  Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.DoubleAnimationUsingPath" /> class. </summary>
	public DoubleAnimationUsingPath()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.DoubleAnimationUsingPath" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DoubleAnimationUsingPath Clone()
	{
		return (DoubleAnimationUsingPath)base.Clone();
	}

	/// <summary>Implementation of <see cref="M:System.Windows.Freezable.CreateInstanceCore" />.              </summary>
	/// <returns>The new <see cref="T:System.Windows.Freezable" /> created.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new DoubleAnimationUsingPath();
	}

	/// <summary>Called when this <see cref="T:System.Windows.Media.Animation.DoubleAnimationUsingPath" /> is modified.</summary>
	protected override void OnChanged()
	{
		_isValid = false;
		base.OnChanged();
	}

	/// <summary>Calculates a value that represents the current value of the property being animated, as determined by the <see cref="T:System.Windows.Media.Animation.DoubleAnimationUsingPath" />.   </summary>
	/// <returns>The calculated value of the property, as determined by the current animation.</returns>
	/// <param name="defaultOriginValue">The suggested origin value, used if the animation does not have its own explicitly set start value.</param>
	/// <param name="defaultDestinationValue">The suggested destination value, used if the animation does not have its own explicitly set end value.</param>
	/// <param name="animationClock">An <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that generates the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> used by the animation.</param>
	protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
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
		double num = 0.0;
		pathGeometry.GetPointAtFractionLength(animationClock.CurrentProgress.Value, out var point, out var tangent);
		switch (Source)
		{
		case PathAnimationSource.Angle:
			num = CalculateAngleFromTangentVector(tangent.X, tangent.Y);
			break;
		case PathAnimationSource.X:
			num = point.X;
			break;
		case PathAnimationSource.Y:
			num = point.Y;
			break;
		}
		double num2 = (animationClock.CurrentIteration - 1).Value;
		if (IsCumulative && num2 > 0.0)
		{
			num += _accumulatingValue * num2;
		}
		if (IsAdditive)
		{
			return defaultOriginValue + num;
		}
		return num;
	}

	private void Validate()
	{
		if (IsCumulative)
		{
			PathGeometry pathGeometry = PathGeometry;
			pathGeometry.GetPointAtFractionLength(0.0, out var point, out var tangent);
			pathGeometry.GetPointAtFractionLength(1.0, out var point2, out var tangent2);
			switch (Source)
			{
			case PathAnimationSource.Angle:
				_accumulatingValue = CalculateAngleFromTangentVector(tangent2.X, tangent2.Y) - CalculateAngleFromTangentVector(tangent.X, tangent.Y);
				break;
			case PathAnimationSource.X:
				_accumulatingValue = point2.X - point.X;
				break;
			case PathAnimationSource.Y:
				_accumulatingValue = point2.Y - point.Y;
				break;
			}
		}
		_isValid = true;
	}

	internal static double CalculateAngleFromTangentVector(double x, double y)
	{
		double num = Math.Acos(x) * (180.0 / Math.PI);
		if (y < 0.0)
		{
			num = 360.0 - num;
		}
		return num;
	}
}
