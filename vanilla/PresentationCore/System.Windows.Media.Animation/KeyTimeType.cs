namespace System.Windows.Media.Animation;

/// <summary>Represents the different types that may represent a <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance.</summary>
public enum KeyTimeType : byte
{
	/// <summary>Specifies that the allotted total time for an animation sequence is divided evenly amongst each of the key frames. </summary>
	Uniform,
	/// <summary>Specifies that each <see cref="T:System.Windows.Media.Animation.KeyTime" /> value is expressed as a percentage of the total time allotted for a given animation sequence. </summary>
	Percent,
	/// <summary>Specifies that each <see cref="P:System.Windows.Media.Animation.ByteKeyFrame.KeyTime" /> is expressed as a <see cref="P:System.Windows.Media.Animation.KeyTime.TimeSpan" /> value relative to the <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> of an animation sequence. </summary>
	TimeSpan,
	/// <summary>Specifies that adjacent KeyFrames are each allotted a slice of time proportional to their length, respectively.  The overall goal is to produce a length value that keeps the pace of the animation sequence constant.</summary>
	Paced
}
