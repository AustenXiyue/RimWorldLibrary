using System.ComponentModel;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.InkCanvas.SelectionMoving" /> and <see cref="E:System.Windows.Controls.InkCanvas.SelectionResizing" /> events. </summary>
public class InkCanvasSelectionEditingEventArgs : CancelEventArgs
{
	private Rect _oldRectangle;

	private Rect _newRectangle;

	/// <summary>Gets the bounds of the selection before the user moved or resized it.</summary>
	/// <returns>The bounds of the selection before the user moved or resized it.</returns>
	public Rect OldRectangle => _oldRectangle;

	/// <summary>Gets or sets the bounds of the selection after it is moved or resized.</summary>
	/// <returns>The bounds of the selection after it is moved or resized.</returns>
	public Rect NewRectangle
	{
		get
		{
			return _newRectangle;
		}
		set
		{
			_newRectangle = value;
		}
	}

	internal InkCanvasSelectionEditingEventArgs(Rect oldRectangle, Rect newRectangle)
	{
		_oldRectangle = oldRectangle;
		_newRectangle = newRectangle;
	}
}
