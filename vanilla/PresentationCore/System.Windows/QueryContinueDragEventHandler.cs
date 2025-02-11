namespace System.Windows;

/// <summary>Represents a method that will handle the routed events that enables a drag-and-drop operation to be canceled by the drag source, for example <see cref="E:System.Windows.UIElement.QueryContinueDrag" />.</summary>
/// <param name="sender">The object where the event handler is attached.</param>
/// <param name="e">The event data.</param>
public delegate void QueryContinueDragEventHandler(object sender, QueryContinueDragEventArgs e);
