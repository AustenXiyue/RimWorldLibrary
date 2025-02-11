namespace System.Windows;

/// <summary>Specifies the reason for which the user's session is ending. Used by the <see cref="P:System.Windows.SessionEndingCancelEventArgs.ReasonSessionEnding" /> property.</summary>
public enum ReasonSessionEnding : byte
{
	/// <summary>The session is ending because the user is logging off.</summary>
	Logoff,
	/// <summary>The session is ending because the user is shutting down Windows.</summary>
	Shutdown
}
