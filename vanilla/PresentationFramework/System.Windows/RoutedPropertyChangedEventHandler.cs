namespace System.Windows;

/// <summary>Represents methods that will handle various routed events that track property value changes.</summary>
/// <param name="sender">The object where the event handler is attached.</param>
/// <param name="e">The event data. Specific event definitions will constrain <see cref="T:System.Windows.RoutedPropertyChangedEventArgs`1" />  to a type, with the type parameter of the constraint matching the type parameter constraint of a delegate implementation.</param>
/// <typeparam name="T">The type of the property value where changes in value are reported.</typeparam>
public delegate void RoutedPropertyChangedEventHandler<T>(object sender, RoutedPropertyChangedEventArgs<T> e);
