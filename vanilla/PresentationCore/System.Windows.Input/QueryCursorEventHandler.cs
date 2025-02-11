namespace System.Windows.Input;

/// <summary>Represents the method that will handle the <see cref="E:System.Windows.UIElement.QueryCursor" /> and <see cref="E:System.Windows.ContentElement.QueryCursor" /> events, as well as the <see cref="E:System.Windows.Input.Mouse.QueryCursor" /> attached event.</summary>
/// <param name="sender">The object where the event handler is attached.</param>
/// <param name="e">The event data.</param>
public delegate void QueryCursorEventHandler(object sender, QueryCursorEventArgs e);
