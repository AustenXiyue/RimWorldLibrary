namespace System.Windows.Input;

/// <summary>Represents the method that will handle the <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> event.</summary>
/// <param name="sender">The command target that is invoking the handler.</param>
/// <param name="e">The event data.</param>
public delegate void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e);
