namespace System.Windows.Controls;

/// <summary> How the undo stack caused or is affected by a text change. </summary>
public enum UndoAction
{
	/// <summary> This change will not affect the undo stack at all </summary>
	None,
	/// <summary> This change will merge into the previous undo unit </summary>
	Merge,
	/// <summary> This change is the result of a call to Undo() </summary>
	Undo,
	/// <summary> This change is the result of a call to Redo() </summary>
	Redo,
	/// <summary> This change will clear the undo stack </summary>
	Clear,
	/// <summary> This change will create a new undo unit </summary>
	Create
}
