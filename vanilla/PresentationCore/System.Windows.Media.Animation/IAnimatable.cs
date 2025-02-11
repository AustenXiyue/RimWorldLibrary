namespace System.Windows.Media.Animation;

/// <summary>This type supports the WPF infrastructure and is not intended to be used directly from your code. To make a class animatable, it should derive from <see cref="T:System.Windows.UIElement" />, <see cref="T:System.Windows.ContentElement" />, or <see cref="T:System.Windows.Media.Animation.Animatable" />. </summary>
public interface IAnimatable
{
	/// <summary>Gets a value that indicates whether this instance has any animated properties. </summary>
	/// <returns>true if a <see cref="T:System.Windows.Media.Animation.Clock" /> is associated with at least one of the current object's properties; otherwise false. </returns>
	bool HasAnimatedProperties { get; }

	/// <summary>Applies the effect of a given <see cref="T:System.Windows.Media.Animation.AnimationClock" /> to a given dependency property.</summary>
	/// <param name="dp">The <see cref="T:System.Windows.DependencyProperty" /> to animate.</param>
	/// <param name="clock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that animates the property.</param>
	void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock);

	/// <summary>Applies the effect of a given <see cref="T:System.Windows.Media.Animation.AnimationClock" /> to a given dependency property. The effect of the new <see cref="T:System.Windows.Media.Animation.AnimationClock" /> on any current animations is determined by the value of the <paramref name="handoffBehavior" /> parameter.</summary>
	/// <param name="dp">The <see cref="T:System.Windows.DependencyProperty" /> to animate.</param>
	/// <param name="clock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that animates the property.</param>
	/// <param name="handoffBehavior">Determines how the new <see cref="T:System.Windows.Media.Animation.AnimationClock" /> will transition from or affect any current animations on the property.</param>
	void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock, HandoffBehavior handoffBehavior);

	/// <summary>Initiates an animation sequence for the <see cref="T:System.Windows.DependencyProperty" /> object, based on the specified <see cref="T:System.Windows.Media.Animation.AnimationTimeline" />. </summary>
	/// <param name="dp">The object to animate.</param>
	/// <param name="animation">The timeline with the necessary functionality to animate the property.</param>
	void BeginAnimation(DependencyProperty dp, AnimationTimeline animation);

	/// <summary>Initiates an animation sequence for the <see cref="T:System.Windows.DependencyProperty" />.object, based on both the specified <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> and <see cref="T:System.Windows.Media.Animation.HandoffBehavior" />. </summary>
	/// <param name="dp">The object to animate.</param>
	/// <param name="animation">The timeline with the necessary functionality to tailor the new animation.</param>
	/// <param name="handoffBehavior">The object specifying the manner in which to interact with all relevant animation sequences.</param>
	void BeginAnimation(DependencyProperty dp, AnimationTimeline animation, HandoffBehavior handoffBehavior);

	/// <summary>Retrieves the base value of the specified <see cref="T:System.Windows.DependencyProperty" /> object. </summary>
	/// <returns>The object representing the base value of <paramref name="Dp" />.</returns>
	/// <param name="dp">The object for which the base value is being requested.</param>
	object GetAnimationBaseValue(DependencyProperty dp);
}
