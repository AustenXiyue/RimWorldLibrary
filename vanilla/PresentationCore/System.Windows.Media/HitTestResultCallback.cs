namespace System.Windows.Media;

/// <summary>Represents a callback that is used to customize hit testing. WPF invokes the <see cref="T:System.Windows.Media.HitTestResultCallback" /> to report hit test intersections to the user.</summary>
/// <returns>A <see cref="T:System.Windows.Media.HitTestFilterBehavior" /> that represents the action resulting from the hit test.</returns>
/// <param name="result">The <see cref="T:System.Windows.Media.HitTestResult" /> value that represents a visual object that is returned from a hit test.</param>
public delegate HitTestResultBehavior HitTestResultCallback(HitTestResult result);
