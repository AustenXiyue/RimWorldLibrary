namespace System.Windows.Input;

/// <summary>Represents the method that will handle the routed events related to the <see cref="T:System.Windows.Input.TextComposition" /> and <see cref="T:System.Windows.Input.TextCompositionManager" /> classes, for example <see cref="E:System.Windows.UIElement.TextInput" />.</summary>
/// <param name="sender">The object where the event handler is attached.</param>
/// <param name="e">The event data.</param>
public delegate void TextCompositionEventHandler(object sender, TextCompositionEventArgs e);
