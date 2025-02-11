using System.ComponentModel;
using System.Windows.Ink;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.InkCanvas.StrokeErasing" /> event. </summary>
public class InkCanvasStrokeErasingEventArgs : CancelEventArgs
{
	private Stroke _stroke;

	/// <summary>Gets the stroke that is about to be erased.</summary>
	/// <returns>The stroke that is about to be erased.</returns>
	public Stroke Stroke => _stroke;

	internal InkCanvasStrokeErasingEventArgs(Stroke stroke)
	{
		if (stroke == null)
		{
			throw new ArgumentNullException("stroke");
		}
		_stroke = stroke;
	}
}
