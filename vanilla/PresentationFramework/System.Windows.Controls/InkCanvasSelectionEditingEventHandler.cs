namespace System.Windows.Controls;

/// <summary>Represents the method that handles two events raised when changes occur on an <see cref="T:System.Windows.Controls.InkCanvas" />: the <see cref="E:System.Windows.Controls.InkCanvas.SelectionMoving" /> event, or the <see cref="E:System.Windows.Controls.InkCanvas.SelectionResizing" /> event. </summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">The event data.</param>
public delegate void InkCanvasSelectionEditingEventHandler(object sender, InkCanvasSelectionEditingEventArgs e);
