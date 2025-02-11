namespace System.Windows;

/// <summary>Represents the callback that is invoked when the effective property value of a dependency property changes.</summary>
/// <param name="d">The <see cref="T:System.Windows.DependencyObject" /> on which the property has changed value.</param>
/// <param name="e">Event data that is issued by any event that tracks changes to the effective value of this property.</param>
public delegate void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e);
