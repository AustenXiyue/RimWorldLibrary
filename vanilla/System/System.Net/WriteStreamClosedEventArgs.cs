using System.ComponentModel;

namespace System.Net;

/// <summary>Provides data for the <see cref="E:System.Net.WebClient.WriteStreamClosed" /> event.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class WriteStreamClosedEventArgs : EventArgs
{
	/// <summary>Gets the error value when a write stream is closed.</summary>
	/// <returns>Returns <see cref="T:System.Exception" />.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public Exception Error => null;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WriteStreamClosedEventArgs" /> class.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	public WriteStreamClosedEventArgs()
	{
	}
}
