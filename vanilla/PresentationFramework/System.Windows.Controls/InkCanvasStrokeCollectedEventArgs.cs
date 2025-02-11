using System.Windows.Ink;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.InkCanvas.StrokeCollected" /> event. </summary>
public class InkCanvasStrokeCollectedEventArgs : RoutedEventArgs
{
	private Stroke _stroke;

	/// <summary>Gets the stroke that was added to the <see cref="T:System.Windows.Controls.InkCanvas" />.</summary>
	/// <returns>The stroke that was added to the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	public Stroke Stroke => _stroke;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.InkCanvasStrokeCollectedEventArgs" /> class.</summary>
	/// <param name="stroke">The collected <see cref="T:System.Windows.Ink.Stroke" /> object.</param>
	public InkCanvasStrokeCollectedEventArgs(Stroke stroke)
		: base(InkCanvas.StrokeCollectedEvent)
	{
		if (stroke == null)
		{
			throw new ArgumentNullException("stroke");
		}
		_stroke = stroke;
	}

	/// <summary>Provides a way to invoke event handlers in a type-specific way.</summary>
	/// <param name="genericHandler">The event handler.</param>
	/// <param name="genericTarget">The event target.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((InkCanvasStrokeCollectedEventHandler)genericHandler)(genericTarget, this);
	}
}
