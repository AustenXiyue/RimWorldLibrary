namespace System.Windows.Input;

/// <summary>Represents the method that will handle the <see cref="E:System.Windows.UIElement.KeyUp" /> and <see cref="E:System.Windows.UIElement.KeyDown" /> routed events, as well as related attached and Preview events.</summary>
/// <param name="sender">The object where the event handler is attached.</param>
/// <param name="e">The event data.</param>
public delegate void KeyEventHandler(object sender, KeyEventArgs e);
