using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Contains arguments for the <see cref="E:System.Windows.DragDrop.QueryContinueDrag" /> event.</summary>
public sealed class QueryContinueDragEventArgs : RoutedEventArgs
{
	private bool _escapePressed;

	private DragDropKeyStates _dragDropKeyStates;

	private DragAction _action;

	/// <summary>Gets a Boolean value indicating whether the ESC key has been pressed.</summary>
	/// <returns>A Boolean value indicating whether the ESC key has been pressed. true if the ESC was pressed; otherwise, false.</returns>
	public bool EscapePressed => _escapePressed;

	/// <summary>Gets a flag enumeration indicating the current state of the SHIFT, CTRL, and ALT keys, as well as the state of the mouse buttons.</summary>
	/// <returns>One or more members of the <see cref="T:System.Windows.DragDropKeyStates" /> flag enumeration.</returns>
	public DragDropKeyStates KeyStates => _dragDropKeyStates;

	/// <summary>Gets or sets the current status of the associated drag-and-drop operation.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.DragAction" /> enumeration indicating the current status of the associated drag-and-drop operation.</returns>
	public DragAction Action
	{
		get
		{
			return _action;
		}
		set
		{
			if (!DragDrop.IsValidDragAction(value))
			{
				throw new ArgumentException(SR.Format(SR.DragDrop_DragActionInvalid, "value"));
			}
			_action = value;
		}
	}

	internal QueryContinueDragEventArgs(bool escapePressed, DragDropKeyStates dragDropKeyStates)
	{
		DragDrop.IsValidDragDropKeyStates(dragDropKeyStates);
		_escapePressed = escapePressed;
		_dragDropKeyStates = dragDropKeyStates;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((QueryContinueDragEventHandler)genericHandler)(genericTarget, this);
	}
}
