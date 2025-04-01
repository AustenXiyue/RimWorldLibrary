using System.Runtime.InteropServices;

namespace System.Security.Principal;

/// <summary>Defines the privileges of the user account associated with the access token. </summary>
[Serializable]
[Flags]
[ComVisible(true)]
public enum TokenAccessLevels
{
	/// <summary>The user can attach a primary token to a process.</summary>
	AssignPrimary = 1,
	/// <summary>The user can duplicate the token.</summary>
	Duplicate = 2,
	/// <summary>The user can impersonate a client.</summary>
	Impersonate = 4,
	/// <summary>The user can query the token.</summary>
	Query = 8,
	/// <summary>The user can query the source of the token.</summary>
	QuerySource = 0x10,
	/// <summary>The user can enable or disable privileges in the token.</summary>
	AdjustPrivileges = 0x20,
	/// <summary>The user can change the attributes of the groups in the token.</summary>
	AdjustGroups = 0x40,
	/// <summary>The user can change the default owner, primary group, or discretionary access control list (DACL) of the token.</summary>
	AdjustDefault = 0x80,
	/// <summary>The user can adjust the session identifier of the token.</summary>
	AdjustSessionId = 0x100,
	/// <summary>The user has standard read rights and the <see cref="F:System.Security.Principal.TokenAccessLevels.Query" /> privilege for the token.</summary>
	Read = 0x20008,
	/// <summary>The user has standard write rights and the <see cref="F:System.Security.Principal.TokenAccessLevels.AdjustPrivileges,F:System.Security.Principal.TokenAccessLevels.AdjustGroups" />, and <see cref="F:System.Security.Principal.TokenAccessLevels.AdjustDefault" /> privileges for the token.</summary>
	Write = 0x200E0,
	/// <summary>The user has all possible access to the token.</summary>
	AllAccess = 0xF01FF,
	/// <summary>The maximum value that can be assigned for the <see cref="T:System.Security.Principal.TokenAccessLevels" /> enumeration.</summary>
	MaximumAllowed = 0x2000000
}
