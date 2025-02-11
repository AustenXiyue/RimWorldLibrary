namespace System.Windows.Media;

/// <summary>Represents the callback method that specifies parts of the visual tree to omit from hit test processing</summary>
/// <returns>A <see cref="T:System.Windows.Media.HitTestFilterBehavior" /> that represents the action resulting from the hit test.</returns>
/// <param name="potentialHitTestTarget">The visual to hit test. </param>
public delegate HitTestFilterBehavior HitTestFilterCallback(DependencyObject potentialHitTestTarget);
