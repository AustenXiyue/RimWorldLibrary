namespace System.ComponentModel.Design;

/// <summary>Defines identifiers that indicate information about the context in which a request for Help information originated.</summary>
public enum HelpContextType
{
	/// <summary>A general context.</summary>
	Ambient,
	/// <summary>A window.</summary>
	Window,
	/// <summary>A selection.</summary>
	Selection,
	/// <summary>A tool window selection.</summary>
	ToolWindowSelection
}
