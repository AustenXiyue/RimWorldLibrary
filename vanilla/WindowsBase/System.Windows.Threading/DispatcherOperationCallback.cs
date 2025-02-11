namespace System.Windows.Threading;

/// <summary>Represents a delegate to use for dispatcher operations.</summary>
/// <returns>The object returned by the callback.</returns>
/// <param name="arg">An argument passed to the callback.</param>
public delegate object DispatcherOperationCallback(object arg);
