namespace System.Windows.Input;

/// <summary>Represents the method that will handle mouse related routed events that do not specifically involve mouse buttons or the mouse wheel; for example, <see cref="E:System.Windows.UIElement.MouseMove" />.</summary>
/// <param name="sender">The object where the event handler is attached.</param>
/// <param name="e">The event data.</param>
public delegate void MouseEventHandler(object sender, MouseEventArgs e);
