namespace System.Windows;

/// <summary>Specifies how and if a drag-and-drop operation should continue.</summary>
public enum DragAction
{
	/// <summary>The operation will continue.</summary>
	Continue,
	/// <summary>The operation will stop with a drop.</summary>
	Drop,
	/// <summary>The operation is canceled with no drop message.</summary>
	Cancel
}
