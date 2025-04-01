namespace System.Diagnostics;

/// <summary>Represents the method that will handle the <see cref="E:System.Diagnostics.EventLog.EntryWritten" /> event of an <see cref="T:System.Diagnostics.EventLog" />.</summary>
/// <param name="sender">The source of the event. </param>
/// <param name="e">An <see cref="T:System.Diagnostics.EntryWrittenEventArgs" /> that contains the event data. </param>
/// <filterpriority>2</filterpriority>
public delegate void EntryWrittenEventHandler(object sender, EntryWrittenEventArgs e);
