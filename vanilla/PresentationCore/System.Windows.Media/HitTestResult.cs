namespace System.Windows.Media;

/// <summary>Provides the base class for several derived classes that represents the return value from a hit test.</summary>
public abstract class HitTestResult
{
	private DependencyObject _visualHit;

	/// <summary>Gets the visual object that was hit.</summary>
	/// <returns>A <see cref="T:System.Windows.DependencyObject" /> value that represents the visual object that was hit.</returns>
	public DependencyObject VisualHit => _visualHit;

	internal HitTestResult(DependencyObject visualHit)
	{
		_visualHit = visualHit;
	}
}
