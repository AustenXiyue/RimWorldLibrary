using System.Globalization;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Animates the value of a  <see cref="T:System.Int64" /> property between two target values using linear interpolation over a specified <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />. </summary>
public class Int64Animation : Int64AnimationBase
{
	private long[] _keyValues;

	private AnimationType _animationType;

	private bool _isAnimationFunctionValid;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Animation.Int64Animation.From" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Int64Animation.From" /> dependency property.</returns>
	public static readonly DependencyProperty FromProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Animation.Int64Animation.To" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Int64Animation.To" /> dependency property.</returns>
	public static readonly DependencyProperty ToProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Animation.Int64Animation.By" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Int64Animation.By" /> dependency property.</returns>
	public static readonly DependencyProperty ByProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Int64Animation.EasingFunction" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Int64Animation.EasingFunction" /> dependency property.</returns>
	public static readonly DependencyProperty EasingFunctionProperty;

	/// <summary> Gets or sets the animation's starting value.   </summary>
	/// <returns>The starting value of the animation. The default is null.</returns>
	public long? From
	{
		get
		{
			return (long?)GetValue(FromProperty);
		}
		set
		{
			SetValueInternal(FromProperty, value);
		}
	}

	/// <summary> Gets or sets the animation's ending value.   </summary>
	/// <returns>The ending value of the animation. The default is null.</returns>
	public long? To
	{
		get
		{
			return (long?)GetValue(ToProperty);
		}
		set
		{
			SetValueInternal(ToProperty, value);
		}
	}

	/// <summary> Gets or sets the total amount by which the animation changes its starting value.   </summary>
	/// <returns>The total amount by which the animation changes its starting value.     The default is null.</returns>
	public long? By
	{
		get
		{
			return (long?)GetValue(ByProperty);
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
	/// <returns>true if the target property's current value should be added to this animation's starting value; otherwise, false. The default is false.</returns>
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

	/// <summary> Gets or sets a value that specifies whether the animation's value accumulates when it repeats.   </summary>
	/// <returns>true if the animation accumulates its values when its <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" /> property causes it to repeat its simple duration; otherwise, false. The default is false.</returns>
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

	static Int64Animation()
	{
		Type typeFromHandle = typeof(long?);
		Type typeFromHandle2 = typeof(Int64Animation);
		PropertyChangedCallback propertyChangedCallback = AnimationFunction_Changed;
		ValidateValueCallback validateValueCallback = ValidateFromToOrByValue;
		FromProperty = DependencyProperty.Register("From", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		ToProperty = DependencyProperty.Register("To", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		ByProperty = DependencyProperty.Register("By", typeFromHandle, typeFromHandle2, new PropertyMetadata(null, propertyChangedCallback), validateValueCallback);
		EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeFromHandle2);
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Int64Animation" /> class. </summary>
	public Int64Animation()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Int64Animation" /> class that animates to the specified value over the specified duration. The starting value for the animation is the base value of the property being animated or the output from another animation. </summary>
	/// <param name="toValue">The destination value of the animation. </param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	public Int64Animation(long toValue, Duration duration)
		: this()
	{
		To = toValue;
		base.Duration = duration;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Int64Animation" /> class that animates to the specified value over the specified duration and has the specified fill behavior. The starting value for the animation is the base value of the property being animated or the output from another animation. </summary>
	/// <param name="toValue">The destination value of the animation. </param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	/// <param name="fillBehavior">Specifies how the animation behaves when it is not active.</param>
	public Int64Animation(long toValue, Duration duration, FillBehavior fillBehavior)
		: this()
	{
		To = toValue;
		base.Duration = duration;
		base.FillBehavior = fillBehavior;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Int64Animation" /> class that animates from the specified starting value to the specified destination value over the specified duration. </summary>
	/// <param name="fromValue">The starting value of the animation.</param>
	/// <param name="toValue">The destination value of the animation. </param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	public Int64Animation(long fromValue, long toValue, Duration duration)
		: this()
	{
		From = fromValue;
		To = toValue;
		base.Duration = duration;
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Int64Animation" /> class that animates from the specified starting value to the specified destination value over the specified duration and has the specified fill behavior. </summary>
	/// <param name="fromValue">The starting value of the animation.</param>
	/// <param name="toValue">The destination value of the animation. </param>
	/// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> property for more information.</param>
	/// <param name="fillBehavior">Specifies how the animation behaves when it is not active.</param>
	public Int64Animation(long fromValue, long toValue, Duration duration, FillBehavior fillBehavior)
		: this()
	{
		From = fromValue;
		To = toValue;
		base.Duration = duration;
		base.FillBehavior = fillBehavior;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.Int64Animation" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Int64Animation Clone()
	{
		return (Int64Animation)base.Clone();
	}

	/// <summary>Implementation of <see cref="M:System.Windows.Freezable.CreateInstanceCore" />.              </summary>
	/// <returns>The new <see cref="T:System.Windows.Freezable" />.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new Int64Animation();
	}

	/// <summary> Calculates the value this animation believes should be the current value for the property. </summary>
	/// <returns>The value this animation believes should be the current value for the property.</returns>
	/// <param name="defaultOriginValue">This value is the suggested origin value provided to the animation to be used if the animation does not have its own concept of a start value. If this animation is the first in a composition chain this value will be the snapshot value if one is available or the base property value if it is not; otherwise this value will be the value returned by the previous animation in the chain with an animationClock that is not Stopped.</param>
	/// <param name="defaultDestinationValue">This value is the suggested destination value provided to the animation to be used if the animation does not have its own concept of an end value. This value will be the base value if the animation is in the first composition layer of animations on a property; otherwise this value will be the output value from the previous composition layer of animations for the property.</param>
	/// <param name="animationClock">This is the animationClock which can generate the CurrentTime or CurrentProgress value to be used by the animation to generate its output value.</param>
	protected override long GetCurrentValueCore(long defaultOriginValue, long defaultDestinationValue, AnimationClock animationClock)
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
		long num2 = 0L;
		long num3 = 0L;
		long value = 0L;
		long value2 = 0L;
		bool flag = false;
		bool flag2 = false;
		switch (_animationType)
		{
		case AnimationType.Automatic:
			num2 = defaultOriginValue;
			num3 = defaultDestinationValue;
			flag = true;
			flag2 = true;
			break;
		case AnimationType.From:
			num2 = _keyValues[0];
			num3 = defaultDestinationValue;
			flag2 = true;
			break;
		case AnimationType.To:
			num2 = defaultOriginValue;
			num3 = _keyValues[0];
			flag = true;
			break;
		case AnimationType.By:
			num3 = _keyValues[0];
			value2 = defaultOriginValue;
			flag = true;
			break;
		case AnimationType.FromTo:
			num2 = _keyValues[0];
			num3 = _keyValues[1];
			if (IsAdditive)
			{
				value2 = defaultOriginValue;
				flag = true;
			}
			break;
		case AnimationType.FromBy:
			num2 = _keyValues[0];
			num3 = AnimatedTypeHelpers.AddInt64(_keyValues[0], _keyValues[1]);
			if (IsAdditive)
			{
				value2 = defaultOriginValue;
				flag = true;
			}
			break;
		}
		if (flag && !AnimatedTypeHelpers.IsValidAnimationValueInt64(defaultOriginValue))
		{
			throw new InvalidOperationException(SR.Format(SR.Animation_Invalid_DefaultValue, GetType(), "origin", defaultOriginValue.ToString(CultureInfo.InvariantCulture)));
		}
		if (flag2 && !AnimatedTypeHelpers.IsValidAnimationValueInt64(defaultDestinationValue))
		{
			throw new InvalidOperationException(SR.Format(SR.Animation_Invalid_DefaultValue, GetType(), "destination", defaultDestinationValue.ToString(CultureInfo.InvariantCulture)));
		}
		if (IsCumulative)
		{
			double num4 = (animationClock.CurrentIteration - 1).Value;
			if (num4 > 0.0)
			{
				value = AnimatedTypeHelpers.ScaleInt64(AnimatedTypeHelpers.SubtractInt64(num3, num2), num4);
			}
		}
		return AnimatedTypeHelpers.AddInt64(value2, AnimatedTypeHelpers.AddInt64(value, AnimatedTypeHelpers.InterpolateInt64(num2, num3, num)));
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
				_keyValues = new long[2];
				_keyValues[0] = From.Value;
				_keyValues[1] = To.Value;
			}
			else if (By.HasValue)
			{
				_animationType = AnimationType.FromBy;
				_keyValues = new long[2];
				_keyValues[0] = From.Value;
				_keyValues[1] = By.Value;
			}
			else
			{
				_animationType = AnimationType.From;
				_keyValues = new long[1];
				_keyValues[0] = From.Value;
			}
		}
		else if (To.HasValue)
		{
			_animationType = AnimationType.To;
			_keyValues = new long[1];
			_keyValues[0] = To.Value;
		}
		else if (By.HasValue)
		{
			_animationType = AnimationType.By;
			_keyValues = new long[1];
			_keyValues[0] = By.Value;
		}
		_isAnimationFunctionValid = true;
	}

	private static void AnimationFunction_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Int64Animation obj = (Int64Animation)d;
		obj._isAnimationFunctionValid = false;
		obj.PropertyChanged(e.Property);
	}

	private static bool ValidateFromToOrByValue(object value)
	{
		long? num = (long?)value;
		if (num.HasValue)
		{
			return AnimatedTypeHelpers.IsValidAnimationValueInt64(num.Value);
		}
		return true;
	}
}
