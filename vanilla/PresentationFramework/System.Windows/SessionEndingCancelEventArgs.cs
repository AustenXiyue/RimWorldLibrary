using System.ComponentModel;

namespace System.Windows;

/// <summary>Contains the event arguments for the <see cref="E:System.Windows.Application.SessionEnding" /> event.</summary>
public class SessionEndingCancelEventArgs : CancelEventArgs
{
	private ReasonSessionEnding _reasonSessionEnding;

	/// <summary>Gets a value that indicates why the session is ending.</summary>
	/// <returns>A <see cref="T:System.Windows.ReasonSessionEnding" /> value that indicates why the session ended.</returns>
	public ReasonSessionEnding ReasonSessionEnding => _reasonSessionEnding;

	internal SessionEndingCancelEventArgs(ReasonSessionEnding reasonSessionEnding)
	{
		_reasonSessionEnding = reasonSessionEnding;
	}
}
