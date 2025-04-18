namespace System;

/// <summary>Represents the SHIFT, ALT, and CTRL modifier keys on a keyboard.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[Flags]
public enum ConsoleModifiers
{
	/// <summary>The left or right ALT modifier key.</summary>
	Alt = 1,
	/// <summary>The left or right SHIFT modifier key.</summary>
	Shift = 2,
	/// <summary>The left or right CTRL modifier key.</summary>
	Control = 4
}
