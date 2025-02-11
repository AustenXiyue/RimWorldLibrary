namespace System.Windows.Input;

/// <summary>Represents the method that will handle <see cref="E:System.Windows.Input.InputManager.PreNotifyInput" /> and <see cref="E:System.Windows.Input.InputManager.PostNotifyInput" /> events. </summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
public delegate void NotifyInputEventHandler(object sender, NotifyInputEventArgs e);
