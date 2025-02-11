namespace System.Windows.Threading;

/// <summary>Represents the method that will handle the <see cref="E:System.Windows.Threading.Dispatcher.UnhandledExceptionFilter" /> event.</summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
public delegate void DispatcherUnhandledExceptionFilterEventHandler(object sender, DispatcherUnhandledExceptionFilterEventArgs e);
