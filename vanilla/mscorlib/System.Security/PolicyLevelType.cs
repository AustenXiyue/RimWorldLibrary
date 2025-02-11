using System.Runtime.InteropServices;

namespace System.Security;

/// <summary>Specifies the type of a managed code policy level.</summary>
[Serializable]
[ComVisible(true)]
public enum PolicyLevelType
{
	/// <summary>Security policy for all managed code that is run by the user.</summary>
	User,
	/// <summary>Security policy for all managed code that is run on the computer.</summary>
	Machine,
	/// <summary>Security policy for all managed code in an enterprise.</summary>
	Enterprise,
	/// <summary>Security policy for all managed code in an application.</summary>
	AppDomain
}
