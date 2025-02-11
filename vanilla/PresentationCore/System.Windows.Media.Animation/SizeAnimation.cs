using System.Globalization;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary> Animates the value of a  <see cref="T:System.Windows.Size" /> property between two target values using      linear interpolation over a specified <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />. </summary>
public class SizeAnimation : SizeAnimationBase
{
	private Size[] _keyValues;

	private AnimationType _animationType;

	private bool _isAnimationFunctionValid;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Animation.SizeAnimation.From" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.SizeAnimation.From" /> dependency property.</returns>
	public static readonly DependencyProperty FromProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Animation.SizeAnimation.To" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.SizeAnimation.To" /> dependency property.</returns>
	public static readonly DependencyProperty ToProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Animation.SizeAnimation.By" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.SizeAnimation.By" /> dependency property.</returns>
	public static readonly DependencyProperty ByProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.SizeAnimation.EasingFunction" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.SizeAnimation.EasingFunction" /> dependency property.</returns>
	public static readonly DependencyProperty EasingFunctionProperty;

	/// <summary> Gets or sets the animation's starting value.  </summary>
	/// <returns>The starting value of the animation. The default value is null.</returns>
	public Size? From
	{
		get
		{
			return (Size?)GetValue(FromProperty);
		}
		set
		{
			SetValueInternal(FromProperty, value);
		}
	}

	/// <summary> Gets or sets the animation's ending value.  </summary>
	/// <returns>The ending value of the animation. The default value is null.</returns>
	public Size? To
	{
		get
		{
			return (Size?)GetValue(ToProperty);
		}
		set
		{
			SetValueInternal(ToProperty, value);
		}
	}

	/// <summary> Gets or sets the total amount by which the animation changes its starting value.  </summary>
	/// <returns>The total amount by which the animation changes its starting value.     The default value is null.</returns>
	public Size? By
	{
		get
		{
			return (Size?)GetValue(ByProperty);
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

	static SizeAnimation()
	{
		Type typeFromHandle = typeof(Size?);
		Type typeFromHandle2 = typeof(SizeAnimation);
		PropertyChangedCallback propertyChangedCallback = AnimationFunction_Changed;
		ValidateValueCallback validateValueCallback = ValidateFromToOrByValue;
		FromProperty = DependencyProperty.Register("From", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		ToProperty = DependencyProperty.Register("To", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		ByProperty = DependencyProperty.Register("By", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeFromHandle2);
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SizeAnimation" /> class. </summary>
	public SizeAnimation()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SizeAnimation" /> class that animates to the specified value over the specified duration. The starting value for the animation is the base value of the property being animated or the output from another animation. </summary>
	/// <param name="toValue">The destination value of the animation. </param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	public SizeAnimation(Size toValue, Duration duration)
		: this()
	{
		To = toValue;
		base.Duration = duration;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SizeAnimation" /> class that animates to the specified value over the specified duration and has the specified fill behavior. The starting value for the animation is the base value of the property being animated or the output from another animation. </summary>
	/// <param name="toValue">The destination value of the animation. </param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	/// <param name="fillBehavior">Specifies how the animation behaves when it is not active.</param>
	public SizeAnimation(Size toValue, Duration duration, FillBehavior fillBehavior)
		: this()
	{
		To = toValue;
		base.Duration = duration;
		base.FillBehavior = fillBehavior;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SizeAnimation" /> class that animates from the specified starting value to the specified destination value over the specified duration. </summary>
	/// <param name="fromValue">The starting value of the animation.</param>
	/// <param name="toValue">The destination value of the animation. </param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	public SizeAnimation(Size fromValue, Size toValue, Duration duration)
		: this()
	{
		From = fromValue;
		To = toValue;
		base.Duration = duration;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SizeAnimation" /> class that animates from the specified starting value to the specified destination value over the specified duration and has the specified fill behavior. </summary>
	/// <param name="fromValue">The starting value of the animation.</param>
	/// <param name="toValue">The destination value of the animation. </param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	/// <param name="fillBehavior">Specifies how the animation behaves when it is not active.</param>
	public SizeAnimation(Size fromValue, Size toValue, Duration duration, FillBehavior fillBehavior)
		: this()
	{
		From = fromValue;
		To = toValue;
		base.Duration = duration;
		base.FillBehavior = fillBehavior;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.SizeAnimation" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new SizeAnimation Clone()
	{
		return (SizeAnimation)base.Clone();
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.Animation.SizeAnimation" />.           </summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new SizeAnimation();
	}

	/// <summary>Calculates a value that represents the current value of the property being animated, as determined by the <see cref="T:System.Windows.Media.Animation.SizeAnimation" />.  </summary>
	/// <returns>The calculated value of the property, as determined by the current animation.</returns>
	/// <param name="defaultOriginValue">The suggested origin value, used if the animation does not have its own explicitly set start value.</param>
	/// <param name="defaultDestinationValue">The suggested destination value, used if the animation does not have its own explicitly set end value.</param>
	/// <param name="animationClock">An <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that generates the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> used by the animation.</param>
	protected override Size GetCurrentValueCore(Size defaultOriginValue, Size defaultDestinationValue, AnimationClock animationClock)
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
		Size size = default(Size);
		Size size2 = default(Size);
		Size value = default(Size);
		Size value2 = default(Size);
		bool flag = false;
		bool flag2 = false;
		switch (_animationType)
		{
		case AnimationType.Automatic:
			size = defaultOriginValue;
			size2 = defaultDestinationValue;
			flag = true;
			flag2 = true;
			break;
		case AnimationType.From:
			size = _keyValues[0];
			size2 = defaultDestinationValue;
			flag2 = true;
			break;
		case AnimationType.To:
			size = defaultOriginValue;
			size2 = _keyValues[0];
			flag = true;
			break;
		case AnimationType.By:
			size2 = _keyValues[0];
			value2 = defaultOriginValue;
			flag = true;
			break;
		case AnimationType.FromTo:
			size = _keyValues[0];
			size2 = _keyValues[1];
			if (IsAdditive)
			{
				value2 = defaultOriginValue;
				flag = true;
			}
			break;
		case AnimationType.FromBy:
			size = _keyValues[0];
			size2 = AnimatedTypeHelpers.AddSize(_keyValues[0], _keyValues[1]);
			if (IsAdditive)
			{
				value2 = defaultOriginValue;
				flag = true;
			}
			break;
		}
		if (flag && !AnimatedTypeHelpers.IsValidAnimationValueSize(defaultOriginValue))
		{
			throw new InvalidOperationException(SR.Format(SR.Animation_Invalid_DefaultValue, GetType(), "origin", defaultOriginValue.ToString(CultureInfo.InvariantCulture)));
		}
		if (flag2 && !AnimatedTypeHelpers.IsValidAnimationValueSize(defaultDestinationValue))
		{
			throw new InvalidOperationException(SR.Format(SR.Animation_Invalid_DefaultValue, GetType(), "destination", defaultDestinationValue.ToString(CultureInfo.InvariantCulture)));
		}
		if (IsCumulative)
		{
			double num2 = (animationClock.CurrentIteration - 1).Value;
			if (num2 > 0.0)
			{
				value = AnimatedTypeHelpers.ScaleSize(AnimatedTypeHelpers.SubtractSize(size2, size), num2);
			}
		}
		return AnimatedTypeHelpers.AddSize(value2, AnimatedTypeHelpers.AddSize(value, AnimatedTypeHelpers.InterpolateSize(size, size2, num)));
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
				_keyValues = new Size[2];
				_keyValues[0] = From.Value;
				_keyValues[1] = To.Value;
			}
			else if (By.HasValue)
			{
				_animationType = AnimationType.FromBy;
				_keyValues = new Size[2];
				_keyValues[0] = From.Value;
				_keyValues[1] = By.Value;
			}
			else
			{
				_animationType = AnimationType.From;
				_keyValues = new Size[1];
				_keyValues[0] = From.Value;
			}
		}
		else if (To.HasValue)
		{
			_animationType = AnimationType.To;
			_keyValues = new Size[1];
			_keyValues[0] = To.Value;
		}
		else if (By.HasValue)
		{
			_animationType = AnimationType.By;
			_keyValues = new Size[1];
			_keyValues[0] = By.Value;
		}
		_isAnimationFunctionValid = true;
	}

	private static void AnimationFunction_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		SizeAnimation obj = (SizeAnimation)d;
		obj._isAnimationFunctionValid = false;
		obj.PropertyChanged(e.Property);
	}

	private static bool ValidateFromToOrByValue(object value)
	{
		Size? size = (Size?)value;
		if (size.HasValue)
		{
			return AnimatedTypeHelpers.IsValidAnimationValueSize(size.Value);
		}
		return true;
	}
}
