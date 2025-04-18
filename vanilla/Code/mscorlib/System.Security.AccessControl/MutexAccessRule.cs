using System.Security.Principal;

namespace System.Security.AccessControl;

/// <summary>Represents a set of access rights allowed or denied for a user or group. This class cannot be inherited.</summary>
public sealed class MutexAccessRule : AccessRule
{
	/// <summary>Gets the rights allowed or denied by the access rule.</summary>
	/// <returns>A bitwise combination of <see cref="T:System.Security.AccessControl.MutexRights" /> values indicating the rights allowed or denied by the access rule.</returns>
	public MutexRights MutexRights => (MutexRights)base.AccessMask;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.MutexAccessRule" /> class, specifying the user or group the rule applies to, the access rights, and whether the specified access rights are allowed or denied.</summary>
	/// <param name="identity">The user or group the rule applies to. Must be of type <see cref="T:System.Security.Principal.SecurityIdentifier" /> or a type such as <see cref="T:System.Security.Principal.NTAccount" /> that can be converted to type <see cref="T:System.Security.Principal.SecurityIdentifier" />.</param>
	/// <param name="eventRights">A bitwise combination of <see cref="T:System.Security.AccessControl.MutexRights" /> values specifying the rights allowed or denied.</param>
	/// <param name="type">One of the <see cref="T:System.Security.AccessControl.AccessControlType" /> values specifying whether the rights are allowed or denied.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="eventRights" /> specifies an invalid value.-or-<paramref name="type" /> specifies an invalid value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identity" /> is null. -or-<paramref name="eventRights" /> is zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="identity" /> is neither of type <see cref="T:System.Security.Principal.SecurityIdentifier" /> nor of a type such as <see cref="T:System.Security.Principal.NTAccount" /> that can be converted to type <see cref="T:System.Security.Principal.SecurityIdentifier" />.</exception>
	public MutexAccessRule(IdentityReference identity, MutexRights eventRights, AccessControlType type)
		: base(identity, (int)eventRights, isInherited: false, InheritanceFlags.None, PropagationFlags.None, type)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.MutexAccessRule" /> class, specifying the name of the user or group the rule applies to, the access rights, and whether the specified access rights are allowed or denied.</summary>
	/// <param name="identity">The name of the user or group the rule applies to.</param>
	/// <param name="eventRights">A bitwise combination of <see cref="T:System.Security.AccessControl.MutexRights" /> values specifying the rights allowed or denied.</param>
	/// <param name="type">One of the <see cref="T:System.Security.AccessControl.AccessControlType" /> values specifying whether the rights are allowed or denied.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="eventRights" /> specifies an invalid value.-or-<paramref name="type" /> specifies an invalid value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="eventRights" /> is zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="identity" /> is null.-or-<paramref name="identity" /> is a zero-length string.-or-<paramref name="identity" /> is longer than 512 characters.</exception>
	public MutexAccessRule(string identity, MutexRights eventRights, AccessControlType type)
		: this(new NTAccount(identity), eventRights, type)
	{
	}
}
