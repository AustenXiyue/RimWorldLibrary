namespace System.Windows.Media.Animation;

/// <summary>Indicates how a <see cref="T:System.Windows.Media.Animation.ParallelTimeline" /> will behave when one or more of its <see cref="T:System.Windows.Media.Animation.Timeline" /> children slips.</summary>
public enum SlipBehavior
{
	/// <summary>Indicates that a <see cref="T:System.Windows.Media.Animation.ParallelTimeline" /> will not slip with the child <see cref="T:System.Windows.Media.Animation.Timeline" />, but will expand to fit all slipping <see cref="T:System.Windows.Media.Animation.Timeline" /> children. NOTE: This is only effective when the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> of the <see cref="T:System.Windows.Media.Animation.ParallelTimeline" /> is not explicitly specified.</summary>
	Grow,
	/// <summary>Indicates that a <see cref="T:System.Windows.Media.Animation.ParallelTimeline" /> will slip along with its first child <see cref="T:System.Windows.Media.Animation.Timeline" /> that can slip whenever that child is delayed or accelerated..</summary>
	Slip
}
