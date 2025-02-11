namespace System.Windows;

/// <summary>Represents the method that will handle events raised when a <see cref="T:System.Windows.DependencyProperty" /> is changed on a particular <see cref="T:System.Windows.DependencyObject" /> implementation.</summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
public delegate void DependencyPropertyChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e);
