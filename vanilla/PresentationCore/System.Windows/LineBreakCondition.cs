namespace System.Windows;

/// <summary>Describes the breaking condition around an inline object.</summary>
public enum LineBreakCondition
{
	/// <summary>Break if not prohibited by another object.</summary>
	BreakDesired,
	/// <summary>Break if allowed by another object.</summary>
	BreakPossible,
	/// <summary>Break always prohibited unless the other object is set to <see cref="F:System.Windows.LineBreakCondition.BreakAlways" />.</summary>
	BreakRestrained,
	/// <summary>Break is always allowed.</summary>
	BreakAlways
}
