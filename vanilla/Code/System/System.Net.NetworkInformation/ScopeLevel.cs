namespace System.Net.NetworkInformation;

/// <summary>The scope level for an IPv6 address.</summary>
public enum ScopeLevel
{
	/// <summary>The scope level is not specified.</summary>
	None = 0,
	/// <summary>The scope is interface-level.</summary>
	Interface = 1,
	/// <summary>The scope is link-level.</summary>
	Link = 2,
	/// <summary>The scope is subnet-level.</summary>
	Subnet = 3,
	/// <summary>The scope is admin-level.</summary>
	Admin = 4,
	/// <summary>The scope is site-level.</summary>
	Site = 5,
	/// <summary>The scope is organization-level.</summary>
	Organization = 8,
	/// <summary>The scope is global.</summary>
	Global = 14
}
