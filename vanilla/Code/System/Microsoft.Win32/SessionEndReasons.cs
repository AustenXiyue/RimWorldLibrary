namespace Microsoft.Win32;

/// <summary>Defines identifiers that represent how the current logon session is ending.</summary>
public enum SessionEndReasons
{
	/// <summary>The user is logging off and ending the current user session. The operating system continues to run.</summary>
	Logoff = 1,
	/// <summary>The operating system is shutting down.</summary>
	SystemShutdown
}
