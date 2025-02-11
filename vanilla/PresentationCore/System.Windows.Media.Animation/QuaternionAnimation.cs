using System.Globalization;
using System.Windows.Media.Media3D;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Animates the value of a <see cref="T:System.Windows.Media.Media3D.Quaternion" /> property between two target values using    linear interpolation over a specified <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />.</summary>
public class QuaternionAnimation : QuaternionAnimationBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.QuaternionAnimation.UseShortestPath" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.Animation.QuaternionAnimation.UseShortestPath" /> dependency property identifier.</returns>
	public static readonly DependencyProperty UseShortestPathProperty;

	private Quaternion[] _keyValues;

	private AnimationType _animationType;

	private bool _isAnimationFunctionValid;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.QuaternionAnimation.From" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.Animation.QuaternionAnimation.From" /> dependency property identifier.</returns>
	public static readonly DependencyProperty FromProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.QuaternionAnimation.To" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.Animation.QuaternionAnimation.To" /> dependency property identifier.</returns>
	public static readonly DependencyProperty ToProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.QuaternionAnimation.By" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.Animation.QuaternionAnimation.By" /> dependency property identifier.</returns>
	public static readonly DependencyProperty ByProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.QuaternionAnimation.EasingFunction" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.QuaternionAnimation.EasingFunction" /> dependency property.</returns>
	public static readonly DependencyProperty EasingFunctionProperty;

	/// <summary>Gets or sets a Boolean value that indicates whether the animation uses spherical linear interpolation to calculate the shortest arc between positions.</summary>
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

	/// <summary>Gets or sets the animation's starting value.  </summary>
	/// <returns>The starting value of the animation. The default value is null.</returns>
	public Quaternion? From
	{
		get
		{
			return (Quaternion?)GetValue(FromProperty);
		}
		set
		{
			SetValueInternal(FromProperty, value);
		}
	}

	/// <summary>  Gets or sets the animation's ending value.  </summary>
	/// <returns>The ending value of the animation. The default value is null.</returns>
	public Quaternion? To
	{
		get
		{
			return (Quaternion?)GetValue(ToProperty);
		}
		set
		{
			SetValueInternal(ToProperty, value);
		}
	}

	/// <summary>Gets or sets the total amount by which the animation changes its starting value.  </summary>
	/// <returns>The total amount by which the animation changes its starting value. The default value is null.</returns>
	public Quaternion? By
	{
		get
		{
			return (Quaternion?)GetValue(ByProperty);
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

	/// <summary>Gets or sets a value that specifies whether the animation's value accumulates when it repeats.  </summary>
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

	static QuaternionAnimation()
	{
		UseShortestPathProperty = DependencyProperty.Register("UseShortestPath", typeof(bool), typeof(QuaternionAnimation), new PropertyMetadata(BooleanBoxes.TrueBox));
		Type typeFromHandle = typeof(Quaternion?);
		Type typeFromHandle2 = typeof(QuaternionAnimation);
		PropertyChangedCallback propertyChangedCallback = AnimationFunction_Changed;
		ValidateValueCallback validateValueCallback = ValidateFromToOrByValue;
		FromProperty = DependencyProperty.Register("From", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		ToProperty = DependencyProperty.Register("To", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		ByProperty = DependencyProperty.Register("By", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeFromHandle2);
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Animation.QuaternionAnimation" />.</summary>
	public QuaternionAnimation()
	{
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Animation.QuaternionAnimation" /> using the specified <see cref="T:System.Windows.Media.Media3D.Quaternion" /> and <see cref="T:System.Windows.Duration" />.</summary>
	/// <param name="toValue">Quaternion to which to animate.</param>
	/// <param name="duration">Duration of the QuaternionAnimation.</param>
	public QuaternionAnimation(Quaternion toValue, Duration duration)
		: this()
	{
		To = toValue;
		base.Duration = duration;
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Animation.QuaternionAnimation" /> using the specified <see cref="T:System.Windows.Media.Media3D.Quaternion" />, <see cref="T:System.Windows.Duration" />, and <see cref="T:System.Windows.Media.Animation.FillBehavior" />.</summary>
	/// <param name="toValue">Quaternion to which to animate.</param>
	/// <param name="duration">Duration of the QuaternionAnimation.</param>
	/// <param name="fillBehavior">Behavior of the timeline outside its active period.</param>
	public QuaternionAnimation(Quaternion toValue, Duration duration, FillBehavior fillBehavior)
		: this()
	{
		To = toValue;
		base.Duration = duration;
		base.FillBehavior = fillBehavior;
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Animation.QuaternionAnimation" /> using the specified <see cref="T:System.Windows.Media.Media3D.Quaternion" /> to another specified <see cref="T:System.Windows.Media.Media3D.Quaternion" /> over the specified <see cref="T:System.Windows.Duration" />.</summary>
	/// <param name="fromValue">Quaternion that from which to animate.</param>
	/// <param name="toValue">Quaternion to which to animate.</param>
	/// <param name="duration">Duration of the QuaternionAnimation.</param>
	public QuaternionAnimation(Quaternion fromValue, Quaternion toValue, Duration duration)
		: this()
	{
		From = fromValue;
		To = toValue;
		base.Duration = duration;
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.Animation.QuaternionAnimation" /> using the specified <see cref="T:System.Windows.Media.Media3D.Quaternion" /> to another specified <see cref="T:System.Windows.Media.Media3D.Quaternion" /> over the specified <see cref="T:System.Windows.Duration" />, with the specified behavior at the end of the timeline.</summary>
	/// <param name="fromValue">Quaternion from which to animate.</param>
	/// <param name="toValue">Quaternion to which to animate.</param>
	/// <param name="duration">Duration of the QuaternionAnimation.</param>
	/// <param name="fillBehavior">Behavior of the timeline outside its active period.</param>
	public QuaternionAnimation(Quaternion fromValue, Quaternion toValue, Duration duration, FillBehavior fillBehavior)
		: this()
	{
		From = fromValue;
		To = toValue;
		base.Duration = duration;
		base.FillBehavior = fillBehavior;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Quaternion" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new QuaternionAnimation Clone()
	{
		return (QuaternionAnimation)base.Clone();
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.Animation.QuaternionAnimation" />.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new QuaternionAnimation();
	}

	/// <summary>Calculates a value that represents the current value of the property being animated, as determined by the <see cref="T:System.Windows.Media.Animation.QuaternionAnimation" />.</summary>
	/// <returns>The calculated value of the property, as determined by the current animation.</returns>
	/// <param name="defaultOriginValue">The suggested origin value, used if the animation does not have its own explicitly set start value.</param>
	/// <param name="defaultDestinationValue">The suggested destination value, used if the animation does not have its own explicitly set end value.</param>
	/// <param name="animationClock">An <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that generates the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> used by the animation.</param>
	protected override Quaternion GetCurrentValueCore(Quaternion defaultOriginValue, Quaternion defaultDestinationValue, AnimationClock animationClock)
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
		Quaternion quaternion = Quaternion.Identity;
		Quaternion quaternion2 = Quaternion.Identity;
		Quaternion value = Quaternion.Identity;
		Quaternion value2 = Quaternion.Identity;
		bool flag = false;
		bool flag2 = false;
		switch (_animationType)
		{
		case AnimationType.Automatic:
			quaternion = defaultOriginValue;
			quaternion2 = defaultDestinationValue;
			flag = true;
			flag2 = true;
			break;
		case AnimationType.From:
			quaternion = _keyValues[0];
			quaternion2 = defaultDestinationValue;
			flag2 = true;
			break;
		case AnimationType.To:
			quaternion = defaultOriginValue;
			quaternion2 = _keyValues[0];
			flag = true;
			break;
		case AnimationType.By:
			quaternion2 = _keyValues[0];
			value2 = defaultOriginValue;
			flag = true;
			break;
		case AnimationType.FromTo:
			quaternion = _keyValues[0];
			quaternion2 = _keyValues[1];
			if (IsAdditive)
			{
				value2 = defaultOriginValue;
				flag = true;
			}
			break;
		case AnimationType.FromBy:
			quaternion = _keyValues[0];
			quaternion2 = AnimatedTypeHelpers.AddQuaternion(_keyValues[0], _keyValues[1]);
			if (IsAdditive)
			{
				value2 = defaultOriginValue;
				flag = true;
			}
			break;
		}
		if (flag && !AnimatedTypeHelpers.IsValidAnimationValueQuaternion(defaultOriginValue))
		{
			throw new InvalidOperationException(SR.Format(SR.Animation_Invalid_DefaultValue, GetType(), "origin", defaultOriginValue.ToString(CultureInfo.InvariantCulture)));
		}
		if (flag2 && !AnimatedTypeHelpers.IsValidAnimationValueQuaternion(defaultDestinationValue))
		{
			throw new InvalidOperationException(SR.Format(SR.Animation_Invalid_DefaultValue, GetType(), "destination", defaultDestinationValue.ToString(CultureInfo.InvariantCulture)));
		}
		if (IsCumulative)
		{
			double num2 = (animationClock.CurrentIteration - 1).Value;
			if (num2 > 0.0)
			{
				value = AnimatedTypeHelpers.ScaleQuaternion(AnimatedTypeHelpers.SubtractQuaternion(quaternion2, quaternion), num2);
			}
		}
		return AnimatedTypeHelpers.AddQuaternion(value2, AnimatedTypeHelpers.AddQuaternion(value, AnimatedTypeHelpers.InterpolateQuaternion(quaternion, quaternion2, num, UseShortestPath)));
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
				_keyValues = new Quaternion[2];
				_keyValues[0] = From.Value;
				_keyValues[1] = To.Value;
			}
			else if (By.HasValue)
			{
				_animationType = AnimationType.FromBy;
				_keyValues = new Quaternion[2];
				_keyValues[0] = From.Value;
				_keyValues[1] = By.Value;
			}
			else
			{
				_animationType = AnimationType.From;
				_keyValues = new Quaternion[1];
				_keyValues[0] = From.Value;
			}
		}
		else if (To.HasValue)
		{
			_animationType = AnimationType.To;
			_keyValues = new Quaternion[1];
			_keyValues[0] = To.Value;
		}
		else if (By.HasValue)
		{
			_animationType = AnimationType.By;
			_keyValues = new Quaternion[1];
			_keyValues[0] = By.Value;
		}
		_isAnimationFunctionValid = true;
	}

	private static void AnimationFunction_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		QuaternionAnimation obj = (QuaternionAnimation)d;
		obj._isAnimationFunctionValid = false;
		obj.PropertyChanged(e.Property);
	}

	private static bool ValidateFromToOrByValue(object value)
	{
		Quaternion? quaternion = (Quaternion?)value;
		if (quaternion.HasValue)
		{
			return AnimatedTypeHelpers.IsValidAnimationValueQuaternion(quaternion.Value);
		}
		return true;
	}
}
