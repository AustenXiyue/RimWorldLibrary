namespace System.Windows.Media.Animation;

/// <summary>Indicates the origin of a seek operation. The offset of the seek operation is relative to this origin. </summary>
public enum TimeSeekOrigin
{
	/// <summary>The offset is relative to the beginning of the activation period of the <see cref="T:System.Windows.Media.Animation.Timeline" />.   </summary>
	BeginTime,
	/// <summary>The offset is relative to the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> of the <see cref="T:System.Windows.Media.Animation.Timeline" />, the length of a single iteration. This value has no meaning if the <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> of the <see cref="T:System.Windows.Media.Animation.Timeline" /> is not resolved. </summary>
	Duration
}
