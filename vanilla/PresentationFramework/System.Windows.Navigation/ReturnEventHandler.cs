namespace System.Windows.Navigation;

/// <summary>Represents the method that will handle the <see cref="E:System.Windows.Navigation.PageFunction`1.Return" /> event of the <see cref="T:System.Windows.Navigation.PageFunction`1" /> class.</summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
/// <typeparam name="T">The type specifier for the event.</typeparam>
public delegate void ReturnEventHandler<T>(object sender, ReturnEventArgs<T> e);
