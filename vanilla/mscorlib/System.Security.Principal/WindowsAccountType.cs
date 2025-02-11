using System.Runtime.InteropServices;

namespace System.Security.Principal;

/// <summary>Specifies the type of Windows account used.</summary>
[Serializable]
[ComVisible(true)]
public enum WindowsAccountType
{
	/// <summary>A standard user account.</summary>
	Normal,
	/// <summary>A Windows guest account.</summary>
	Guest,
	/// <summary>A Windows system account.</summary>
	System,
	/// <summary>An anonymous account.</summary>
	Anonymous
}
