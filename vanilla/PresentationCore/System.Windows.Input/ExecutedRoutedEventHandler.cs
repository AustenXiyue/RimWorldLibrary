namespace System.Windows.Input;

/// <summary>Represents the method that will handle the <see cref="E:System.Windows.Input.CommandBinding.Executed" /> and <see cref="E:System.Windows.Input.CommandBinding.PreviewExecuted" /> routed events, as well as related attached events.</summary>
/// <param name="sender">The object where the event handler is attached.</param>
/// <param name="e">The event data.</param>
public delegate void ExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs e);
