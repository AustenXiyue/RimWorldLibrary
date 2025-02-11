using System.ComponentModel;

namespace System.Net;

/// <summary>Represents the method that will handle the <see cref="E:System.Net.WebClient.WriteStreamClosed" /> event of a <see cref="T:System.Net.WebClient" />.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void WriteStreamClosedEventHandler(object sender, WriteStreamClosedEventArgs e);
