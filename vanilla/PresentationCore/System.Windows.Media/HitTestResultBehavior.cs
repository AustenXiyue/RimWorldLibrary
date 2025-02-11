namespace System.Windows.Media;

/// <summary>Determines whether to continue the enumeration of any remaining visual objects during a hit test.</summary>
public enum HitTestResultBehavior
{
	/// <summary>Stop any further hit testing and return from the callback. </summary>
	Stop,
	/// <summary>Continue hit testing against the next visual in the visual tree hierarchy.</summary>
	Continue
}
