using System.Globalization;
using System.Windows.Media.Media3D;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Animates the value of a Vector3D property using linear interpolation between two values. </summary>
public class Vector3DAnimation : Vector3DAnimationBase
{
	private Vector3D[] _keyValues;

	private AnimationType _animationType;

	private bool _isAnimationFunctionValid;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Animation.Vector3DAnimation.From" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Vector3DAnimation.From" /> dependency property.</returns>
	public static readonly DependencyProperty FromProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Animation.Vector3DAnimation.To" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Vector3DAnimation.To" /> dependency property.</returns>
	public static readonly DependencyProperty ToProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Animation.Vector3DAnimation.By" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Vector3DAnimation.By" /> dependency property.</returns>
	public static readonly DependencyProperty ByProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Vector3DAnimation.EasingFunction" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Vector3DAnimation.EasingFunction" /> dependency property.</returns>
	public static readonly DependencyProperty EasingFunctionProperty;

	/// <summary> Gets or sets the animation's starting value.  </summary>
	/// <returns>The starting value of the animation. The default value is null.</returns>
	public Vector3D? From
	{
		get
		{
			return (Vector3D?)GetValue(FromProperty);
		}
		set
		{
			SetValueInternal(FromProperty, value);
		}
	}

	/// <summary> Gets or sets the animation's ending value.  </summary>
	/// <returns>The ending value of the animation. The default value is null.</returns>
	public Vector3D? To
	{
		get
		{
			return (Vector3D?)GetValue(ToProperty);
		}
		set
		{
			SetValueInternal(ToProperty, value);
		}
	}

	/// <summary> Gets or sets the total amount by which the animation changes its starting value.  </summary>
	/// <returns>The total amount by which the animation changes its starting value.     The default value is null.</returns>
	public Vector3D? By
	{
		get
		{
			return (Vector3D?)GetValue(ByProperty);
		}
		set
		{
			SetValueInternal(ByProperty, value);
		}
	}

	/// <summary>Gets or sets the easing function applied to this animation.</summary>
	/// <returns>The easing function applied to this animation.</returns>
	public IEasingFunction EasingFunction
	{
		get
		{
			return (IEasingFunction)GetValue(EasingFunctionProperty);
		}
		set
		{
			SetValueInternal(EasingFunctionProperty, value);
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
			SetValueInternal(AnimationTimeline.IsAdditiveProperty, BooleanBoxes.Box(value));
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
			SetValueInternal(AnimationTimeline.IsCumulativeProperty, BooleanBoxes.Box(value));
		}
	}

	static Vector3DAnimation()
	{
		Type typeFromHandle = typeof(Vector3D?);
		Type typeFromHandle2 = typeof(Vector3DAnimation);
		PropertyChangedCallback propertyChangedCallback = AnimationFunction_Changed;
		ValidateValueCallback validateValueCallback = ValidateFromToOrByValue;
		FromProperty = DependencyProperty.Register("From", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		ToProperty = DependencyProperty.Register("To", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		ByProperty = DependencyProperty.Register("By", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeFromHandle2);
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Vector3DAnimation" /> class. </summary>
	public Vector3DAnimation()
	{
	}

	/// <summary>Initializes a new instance of the Vector3DAnimation class with the specified destination value and <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />.</summary>
	/// <param name="toValue">The ending value of this animation.</param>
	/// <param name="duration">The new animation's <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />.</param>
	public Vector3DAnimation(Vector3D toValue, Duration duration)
		: this()
	{
		To = toValue;
		base.Duration = duration;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Vector3DAnimation" /> class that animates to the specified value over the specified duration and has the specified fill behavior. The starting value for the animation is the base value of the property being animated or the output from another animation.</summary>
	/// <param name="toValue">The destination value of the animation.</param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	/// <param name="fillBehavior">Specifies how the animation behaves when it is not active.</param>
	public Vector3DAnimation(Vector3D toValue, Duration duration, FillBehavior fillBehavior)
		: this()
	{
		To = toValue;
		base.Duration = duration;
		base.FillBehavior = fillBehavior;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Vector3DAnimation" /> class that animates from the specified starting value to the specified destination value over the specified duration.</summary>
	/// <param name="fromValue">The starting value of the animation.</param>
	/// <param name="toValue">The destination value of the animation.</param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	public Vector3DAnimation(Vector3D fromValue, Vector3D toValue, Duration duration)
		: this()
	{
		From = fromValue;
		To = toValue;
		base.Duration = duration;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Vector3DAnimation" /> class that animates from the specified starting value to the specified destination value over the specified duration and has the specified fill behavior.</summary>
	/// <param name="fromValue">The starting value of the animation.</param>
	/// <param name="toValue">The destination value of the animation.</param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	/// <param name="fillBehavior">Specifies how the animation behaves when it is not active.</param>
	public Vector3DAnimation(Vector3D fromValue, Vector3D toValue, Duration duration, FillBehavior fillBehavior)
		: this()
	{
		From = fromValue;
		To = toValue;
		base.Duration = duration;
		base.FillBehavior = fillBehavior;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.Vector3DAnimation" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Vector3DAnimation Clone()
	{
		return (Vector3DAnimation)base.Clone();
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.Animation.Vector3DAnimation" />. </summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new Vector3DAnimation();
	}

	/// <summary>Calculates a value that represents the current value of the property being animated, as determined by the <see cref="T:System.Windows.Media.Animation.Vector3DAnimation" />. </summary>
	/// <returns>The calculated value of the property, as determined by the current animation.</returns>
	/// <param name="defaultOriginValue">The suggested origin value, used if the animation does not have its own explicitly set start value.</param>
	/// <param name="defaultDestinationValue">The suggested destination value, used if the animation does not have its own explicitly set end value.</param>
	/// <param name="animationClock">An <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that generates the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> used by the animation.</param>
	protected override Vector3D GetCurrentValueCore(Vector3D defaultOriginValue, Vector3D defaultDestinationValue, AnimationClock animationClock)
	{
		if (!_isAnimationFunctionValid)
		{
			ValidateAnimationFunction();
		}
		double num = animationClock.CurrentProgress.Value;
		IEasingFunction easingFunction = EasingFunction;
		if (easingFunction != null)
		{
			num = easingFunction.Ease(num);
		}
		Vector3D vector3D = default(Vector3D);
		Vector3D vector3D2 = default(Vector3D);
		Vector3D value = default(Vector3D);
		Vector3D value2 = default(Vector3D);
		bool flag = false;
		bool flag2 = false;
		switch (_animationType)
		{
		case AnimationType.Automatic:
			vector3D = defaultOriginValue;
			vector3D2 = defaultDestinationValue;
			flag = true;
			flag2 = true;
			break;
		case AnimationType.From:
			vector3D = _keyValues[0];
			vector3D2 = defaultDestinationValue;
			flag2 = true;
			break;
		case AnimationType.To:
			vector3D = defaultOriginValue;
			vector3D2 = _keyValues[0];
			flag = true;
			break;
		case AnimationType.By:
			vector3D2 = _keyValues[0];
			value2 = defaultOriginValue;
			flag = true;
			break;
		case AnimationType.FromTo:
			vector3D = _keyValues[0];
			vector3D2 = _keyValues[1];
			if (IsAdditive)
			{
				value2 = defaultOriginValue;
				flag = true;
			}
			break;
		case AnimationType.FromBy:
			vector3D = _keyValues[0];
			vector3D2 = AnimatedTypeHelpers.AddVector3D(_keyValues[0], _keyValues[1]);
			if (IsAdditive)
			{
				value2 = defaultOriginValue;
				flag = true;
			}
			break;
		}
		if (flag && !AnimatedTypeHelpers.IsValidAnimationValueVector3D(defaultOriginValue))
		{
			throw new InvalidOperationException(SR.Format(SR.Animation_Invalid_DefaultValue, GetType(), "origin", defaultOriginValue.ToString(CultureInfo.InvariantCulture)));
		}
		if (flag2 && !AnimatedTypeHelpers.IsValidAnimationValueVector3D(defaultDestinationValue))
		{
			throw new InvalidOperationException(SR.Format(SR.Animation_Invalid_DefaultValue, GetType(), "destination", defaultDestinationValue.ToString(CultureInfo.InvariantCulture)));
		}
		if (IsCumulative)
		{
			double num2 = (animationClock.CurrentIteration - 1).Value;
			if (num2 > 0.0)
			{
				value = AnimatedTypeHelpers.ScaleVector3D(AnimatedTypeHelpers.SubtractVector3D(vector3D2, vector3D), num2);
			}
		}
		return AnimatedTypeHelpers.AddVector3D(value2, AnimatedTypeHelpers.AddVector3D(value, AnimatedTypeHelpers.InterpolateVector3D(vector3D, vector3D2, num)));
	}

	private void ValidateAnimationFunction()
	{
		_animationType = AnimationType.Automatic;
		_keyValues = null;
		if (From.HasValue)
		{
			if (To.HasValue)
			{
				_animationType = AnimationType.FromTo;
				_keyValues = new Vector3D[2];
				_keyValues[0] = From.Value;
				_keyValues[1] = To.Value;
			}
			else if (By.HasValue)
			{
				_animationType = AnimationType.FromBy;
				_keyValues = new Vector3D[2];
				_keyValues[0] = From.Value;
				_keyValues[1] = By.Value;
			}
			else
			{
				_animationType = AnimationType.From;
				_keyValues = new Vector3D[1];
				_keyValues[0] = From.Value;
			}
		}
		else if (To.HasValue)
		{
			_animationType = AnimationType.To;
			_keyValues = new Vector3D[1];
			_keyValues[0] = To.Value;
		}
		else if (By.HasValue)
		{
			_animationType = AnimationType.By;
			_keyValues = new Vector3D[1];
			_keyValues[0] = By.Value;
		}
		_isAnimationFunctionValid = true;
	}

	private static void AnimationFunction_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Vector3DAnimation obj = (Vector3DAnimation)d;
		obj._isAnimationFunctionValid = false;
		obj.PropertyChanged(e.Property);
	}

	private static bool ValidateFromToOrByValue(object value)
	{
		Vector3D? vector3D = (Vector3D?)value;
		if (vector3D.HasValue)
		{
			return AnimatedTypeHelpers.IsValidAnimationValueVector3D(vector3D.Value);
		}
		return true;
	}
}
