namespace System.Windows.Media;

/// <summary>Specifies the return behavior of a hit test in a hit test filter callback method.</summary>
public enum HitTestFilterBehavior
{
	/// <summary>Hit test against the current <see cref="T:System.Windows.Media.Visual" />, but not its descendants.</summary>
	ContinueSkipChildren = 2,
	/// <summary>Do not hit test against the current <see cref="T:System.Windows.Media.Visual" /> or its descendants.</summary>
	ContinueSkipSelfAndChildren = 0,
	/// <summary>Do not hit test against the current <see cref="T:System.Windows.Media.Visual" />, but hit test against its descendants.</summary>
	ContinueSkipSelf = 4,
	/// <summary>Hit test against the current <see cref="T:System.Windows.Media.Visual" /> and its descendants.</summary>
	Continue = 6,
	/// <summary>Stop hit testing at the current <see cref="T:System.Windows.Media.Visual" />.</summary>
	Stop = 8
}
