namespace System.Security.Cryptography;

/// <summary>Specifies the protection level for the key in user interface (UI) prompting scenarios.</summary>
[Flags]
public enum CngUIProtectionLevels
{
	/// <summary>No UI prompt is displayed when the key is accessed.</summary>
	None = 0,
	/// <summary>A UI prompt is displayed the first time the key is accessed in a process.</summary>
	ProtectKey = 1,
	/// <summary>A UI prompt is displayed every time the key is accessed.</summary>
	ForceHighProtection = 2
}
