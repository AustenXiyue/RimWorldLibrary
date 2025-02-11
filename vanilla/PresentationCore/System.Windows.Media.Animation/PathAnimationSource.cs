namespace System.Windows.Media.Animation;

/// <summary>Specifies the output property value of the path that is used to drive the animation. </summary>
public enum PathAnimationSource : byte
{
	/// <summary>Specifies the x-coordinate offset during the progression along an animation sequence path. </summary>
	X,
	/// <summary>Specifies the y-coordinate offset during the progression along an animation sequence path. </summary>
	Y,
	/// <summary>Specifies the tangent angle of rotation during the progression along an animation sequence path.</summary>
	Angle
}
