namespace System.Windows.Media.Animation;

/// <summary>Animates the value of a <see cref="T:System.Windows.Media.Matrix" /> property by using a <see cref="T:System.Windows.Media.PathGeometry" /> to generate the animated values. This animation can be used to move a visual object along a path. </summary>
public class MatrixAnimationUsingPath : MatrixAnimationBase
{
	private bool _isValid;

	private Vector _accumulatingOffset;

	private double _accumulatingAngle;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.MatrixAnimationUsingPath.DoesRotateWithTangent" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.MatrixAnimationUsingPath.DoesRotateWithTangent" /> dependency property.</returns>
	public static readonly DependencyProperty DoesRotateWithTangentProperty = DependencyProperty.Register("DoesRotateWithTangent", typeof(bool), typeof(MatrixAnimationUsingPath), new PropertyMetadata(false));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.MatrixAnimationUsingPath.IsAngleCumulative" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.MatrixAnimationUsingPath.IsAngleCumulative" /> dependency property.</returns>
	public static readonly DependencyProperty IsAngleCumulativeProperty = DependencyProperty.Register("IsAngleCumulative", typeof(bool), typeof(MatrixAnimationUsingPath), new PropertyMetadata(false));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.MatrixAnimationUsingPath.IsOffsetCumulative" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.MatrixAnimationUsingPath.IsOffsetCumulative" /> dependency property.</returns>
	public static readonly DependencyProperty IsOffsetCumulativeProperty = DependencyProperty.Register("IsOffsetCumulative", typeof(bool), typeof(MatrixAnimationUsingPath), new PropertyMetadata(false));

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.MatrixAnimationUsingPath.PathGeometry" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.MatrixAnimationUsingPath.PathGeometry" /> dependency property.</returns>
	public static readonly DependencyProperty PathGeometryProperty = DependencyProperty.Register("PathGeometry", typeof(PathGeometry), typeof(MatrixAnimationUsingPath), new PropertyMetadata((object)null));

	/// <summary>Gets or sets a value indicating whether the object rotates along the tangent of the path.  </summary>
	/// <returns>true if the object will rotate along the tangent of the path; otherwise, false. The default is false.</returns>
	public bool DoesRotateWithTangent
	{
		get
		{
			return (bool)GetValue(DoesRotateWithTangentProperty);
		}
		set
		{
			SetValue(DoesRotateWithTangentProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the target property's current value should be added to this animation's starting value.  </summary>
	/// <returns>true if the target property's current value should be added to this animation's starting value; otherwise, false. The default is false. </returns>
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

	/// <summary>Gets or sets a value that specifies whether the rotation angle of the animated matrix should accumulate over repetitions.  </summary>
	/// <returns>true if the animation's rotation angle should accumulate over repetitions; otherwise, false. The default is false.</returns>
	public bool IsAngleCumulative
	{
		get
		{
			return (bool)GetValue(IsAngleCumulativeProperty);
		}
		set
		{
			SetValue(IsAngleCumulativeProperty, value);
		}
	}

	/// <summary>Gets or sets a value indicating whether the offset produced by the animated matrix will accumulate over repetitions.  </summary>
	/// <returns>true if the object will accumulate over repeats of the animation; otherwise, false. The default is false.</returns>
	public bool IsOffsetCumulative
	{
		get
		{
			return (bool)GetValue(IsOffsetCumulativeProperty);
		}
		set
		{
			SetValue(IsOffsetCumulativeProperty, value);
		}
	}

	/// <summary>Gets or sets the geometry used to generate this animation's output values.  </summary>
	/// <returns>The geometry used to generate this animation's output values. The default is null.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.MatrixAnimationUsingPath" /> class.</summary>
	public MatrixAnimationUsingPath()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.MatrixAnimationUsingPath" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new MatrixAnimationUsingPath Clone()
	{
		return (MatrixAnimationUsingPath)base.Clone();
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.Animation.MatrixAnimationUsingPath" />.          </summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new MatrixAnimationUsingPath();
	}

	/// <summary>Called when this <see cref="T:System.Windows.Media.Animation.MatrixAnimationUsingPath" /> is modified.</summary>
	protected override void OnChanged()
	{
		_isValid = false;
		base.OnChanged();
	}

	/// <summary>Calculates a value that represents the current value of the property being animated, as determined by the <see cref="T:System.Windows.Media.Animation.MatrixAnimationUsingPath" />.</summary>
	/// <returns>The calculated value of the property, as determined by the current animation.</returns>
	/// <param name="defaultOriginValue">The suggested origin value, used if the animation does not have its own explicitly set start value.</param>
	/// <param name="defaultDestinationValue">The suggested destination value, used if the animation does not have its own explicitly set end value.</param>
	/// <param name="animationClock">An <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that generates the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> used by the animation.</param>
	protected override Matrix GetCurrentValueCore(Matrix defaultOriginValue, Matrix defaultDestinationValue, AnimationClock animationClock)
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
		pathGeometry.GetPointAtFractionLength(animationClock.CurrentProgress.Value, out var point, out var tangent);
		double num = 0.0;
		if (DoesRotateWithTangent)
		{
			num = DoubleAnimationUsingPath.CalculateAngleFromTangentVector(tangent.X, tangent.Y);
		}
		Matrix matrix = default(Matrix);
		double num2 = (animationClock.CurrentIteration - 1).Value;
		if (num2 > 0.0)
		{
			if (IsOffsetCumulative)
			{
				point += _accumulatingOffset * num2;
			}
			if (DoesRotateWithTangent && IsAngleCumulative)
			{
				num += _accumulatingAngle * num2;
			}
		}
		matrix.Rotate(num);
		matrix.Translate(point.X, point.Y);
		if (IsAdditive)
		{
			return Matrix.Multiply(matrix, defaultOriginValue);
		}
		return matrix;
	}

	private void Validate()
	{
		if (IsOffsetCumulative || IsAngleCumulative)
		{
			PathGeometry pathGeometry = PathGeometry;
			pathGeometry.GetPointAtFractionLength(0.0, out var point, out var tangent);
			pathGeometry.GetPointAtFractionLength(1.0, out var point2, out var tangent2);
			_accumulatingAngle = DoubleAnimationUsingPath.CalculateAngleFromTangentVector(tangent2.X, tangent2.Y) - DoubleAnimationUsingPath.CalculateAngleFromTangentVector(tangent.X, tangent.Y);
			_accumulatingOffset.X = point2.X - point.X;
			_accumulatingOffset.Y = point2.Y - point.Y;
		}
		_isValid = true;
	}
}
