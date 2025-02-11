namespace System.Windows.Media.Animation;

/// <summary>Describes the potential states of a timeline's <see cref="T:System.Windows.Media.Animation.Clock" /> object. </summary>
public enum ClockState
{
	/// <summary>The current <see cref="T:System.Windows.Media.Animation.Clock" /> time changes in direct relation to that of its parent. If the timeline is an animation, it is actively affecting targeted properties, so their value may change from tick (a sampling point in time) to tick. If the timeline has children, they may be <see cref="F:System.Windows.Media.Animation.ClockState.Active" />, <see cref="F:System.Windows.Media.Animation.ClockState.Filling" />, or <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />. </summary>
	Active,
	/// <summary>The <see cref="T:System.Windows.Media.Animation.Clock" /> timing continues, but does not change in relation to that of its parent. If the timeline is an animation, it is actively affecting targeted properties, but its values don't change from tick to tick. If the timeline has children, they may be <see cref="F:System.Windows.Media.Animation.ClockState.Active" />, <see cref="F:System.Windows.Media.Animation.ClockState.Filling" />, or <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />.</summary>
	Filling,
	/// <summary>The <see cref="T:System.Windows.Media.Animation.Clock" /> timing is halted, making the clock's current time and progress values undefined. If this timeline is an animation, it no longer affects targeted properties. If this timeline has children, they are also <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />.</summary>
	Stopped
}
