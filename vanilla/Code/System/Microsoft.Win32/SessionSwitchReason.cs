namespace Microsoft.Win32;

/// <summary>Defines identifiers used to represent the type of a session switch event.</summary>
public enum SessionSwitchReason
{
	/// <summary>A session has been connected from the console.</summary>
	ConsoleConnect = 1,
	/// <summary>A session has been disconnected from the console.</summary>
	ConsoleDisconnect,
	/// <summary>A session has been connected from a remote connection.</summary>
	RemoteConnect,
	/// <summary>A session has been disconnected from a remote connection.</summary>
	RemoteDisconnect,
	/// <summary>A user has logged on to a session.</summary>
	SessionLogon,
	/// <summary>A user has logged off from a session.</summary>
	SessionLogoff,
	/// <summary>A session has been locked.</summary>
	SessionLock,
	/// <summary>A session has been unlocked.</summary>
	SessionUnlock,
	/// <summary>A session has changed its status to or from remote controlled mode.</summary>
	SessionRemoteControl
}
