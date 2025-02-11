namespace System.Windows.Threading;

/// <summary>Represents the method that will handle the <see cref="E:System.Windows.Threading.Dispatcher.UnhandledException" /> event.</summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
public delegate void DispatcherUnhandledExceptionEventHandler(object sender, DispatcherUnhandledExceptionEventArgs e);
