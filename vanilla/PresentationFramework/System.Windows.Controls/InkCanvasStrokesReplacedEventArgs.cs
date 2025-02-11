using System.Windows.Ink;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.InkCanvas.StrokesReplaced" /> event. </summary>
public class InkCanvasStrokesReplacedEventArgs : EventArgs
{
	private StrokeCollection _newStrokes;

	private StrokeCollection _previousStrokes;

	/// <summary>Gets the new strokes of the <see cref="T:System.Windows.Controls.InkCanvas" />.</summary>
	/// <returns>The new strokes of the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	public StrokeCollection NewStrokes => _newStrokes;

	/// <summary>Gets the previous strokes of the <see cref="T:System.Windows.Controls.InkCanvas" />.</summary>
	/// <returns>The previous strokes of the <see cref="T:System.Windows.Controls.InkCanvas" />.</returns>
	public StrokeCollection PreviousStrokes => _previousStrokes;

	internal InkCanvasStrokesReplacedEventArgs(StrokeCollection newStrokes, StrokeCollection previousStrokes)
	{
		if (newStrokes == null)
		{
			throw new ArgumentNullException("newStrokes");
		}
		if (previousStrokes == null)
		{
			throw new ArgumentNullException("previousStrokes");
		}
		_newStrokes = newStrokes;
		_previousStrokes = previousStrokes;
	}
}
