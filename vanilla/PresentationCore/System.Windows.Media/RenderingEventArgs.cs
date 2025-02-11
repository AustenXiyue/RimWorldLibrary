namespace System.Windows.Media;

/// <summary>Required arguments for the <see cref="E:System.Windows.Media.CompositionTarget.Rendering" /> event.</summary>
public class RenderingEventArgs : EventArgs
{
	private TimeSpan _renderingTime;

	/// <summary>Gets the estimated target time at which the next frame of an animation will be rendered.</summary>
	/// <returns>The estimated target time at which the next frame of an animation will be rendered.</returns>
	public TimeSpan RenderingTime => _renderingTime;

	internal RenderingEventArgs(TimeSpan renderingTime)
	{
		_renderingTime = renderingTime;
	}
}
