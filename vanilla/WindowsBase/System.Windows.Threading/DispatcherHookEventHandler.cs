namespace System.Windows.Threading;

/// <summary>Represents the method that will handle <see cref="T:System.Windows.Threading.DispatcherHooks" /> related events. </summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
public delegate void DispatcherHookEventHandler(object sender, DispatcherHookEventArgs e);
