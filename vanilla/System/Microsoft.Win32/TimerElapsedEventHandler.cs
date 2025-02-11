namespace Microsoft.Win32;

/// <summary>Represents the method that will handle the <see cref="E:Microsoft.Win32.SystemEvents.TimerElapsed" /> event.</summary>
/// <param name="sender">The source of the event. When this event is raised by the <see cref="T:Microsoft.Win32.SystemEvents" /> class, this object is always null. </param>
/// <param name="e">A <see cref="T:Microsoft.Win32.TimerElapsedEventArgs" /> that contains the event data. </param>
public delegate void TimerElapsedEventHandler(object sender, TimerElapsedEventArgs e);
