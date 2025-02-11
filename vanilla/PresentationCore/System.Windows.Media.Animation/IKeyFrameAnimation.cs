using System.Collections;

namespace System.Windows.Media.Animation;

/// <summary>An <see cref="T:System.Windows.Media.Animation.IKeyFrameAnimation" /> interface implementation provides untyped access to key frame collection members. </summary>
public interface IKeyFrameAnimation
{
	/// <summary>Gets or sets an ordered collection <see cref="P:System.Windows.Media.Animation.IKeyFrameAnimation.KeyFrames" /> associated with this animation sequence. </summary>
	/// <returns>An <see cref="T:System.Collections.IList" /> of <see cref="P:System.Windows.Media.Animation.IKeyFrameAnimation.KeyFrames" />.</returns>
	IList KeyFrames { get; set; }
}
