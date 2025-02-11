namespace System.Windows.Media.Animation;

/// <summary>Specifies how a <see cref="T:System.Windows.Media.Animation.Timeline" /> behaves when it is outside its active period but its parent is inside its active or hold period. </summary>
public enum FillBehavior
{
	/// <summary>After it reaches the end of its active period, the timeline holds its progress until the end of its parent's active and hold periods. </summary>
	HoldEnd,
	/// <summary>The timeline stops if it is outside its active period while its parent is inside its active period.</summary>
	Stop
}
