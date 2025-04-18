using System.Runtime.InteropServices;
using System.Security.Principal;
using Unity;

namespace System.Security.AccessControl;

/// <summary>Represents the access control and audit security for a file or directory.</summary>
public abstract class FileSystemSecurity : NativeObjectSecurity
{
	/// <summary>Gets the enumeration that the <see cref="T:System.Security.AccessControl.FileSystemSecurity" /> class uses to represent access rights.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Security.AccessControl.FileSystemRights" /> enumeration.</returns>
	public override Type AccessRightType => typeof(FileSystemRights);

	/// <summary>Gets the enumeration that the <see cref="T:System.Security.AccessControl.FileSystemSecurity" /> class uses to represent access rules.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> class.</returns>
	public override Type AccessRuleType => typeof(FileSystemAccessRule);

	/// <summary>Gets the type that the <see cref="T:System.Security.AccessControl.FileSystemSecurity" /> class uses to represent audit rules.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the <see cref="T:System.Security.AccessControl.FileSystemAuditRule" /> class.</returns>
	public override Type AuditRuleType => typeof(FileSystemAuditRule);

	internal FileSystemSecurity(bool isContainer)
		: base(isContainer, ResourceType.FileObject)
	{
	}

	internal FileSystemSecurity(bool isContainer, string name, AccessControlSections includeSections)
		: base(isContainer, ResourceType.FileObject, name, includeSections)
	{
	}

	internal FileSystemSecurity(bool isContainer, SafeHandle handle, AccessControlSections includeSections)
		: base(isContainer, ResourceType.FileObject, handle, includeSections)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> class that represents a new access control rule for the specified user, with the specified access rights, access control, and flags.</summary>
	/// <returns>A new <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> object that represents a new access control rule for the specified user, with the specified access rights, access control, and flags.</returns>
	/// <param name="identityReference">An <see cref="T:System.Security.Principal.IdentityReference" /> object that represents a user account.</param>
	/// <param name="accessMask">An integer that specifies an access type.</param>
	/// <param name="isInherited">true if the access rule is inherited; otherwise, false.    </param>
	/// <param name="inheritanceFlags">One of the <see cref="T:System.Security.AccessControl.InheritanceFlags" /> values that specifies how to propagate access masks to child objects.</param>
	/// <param name="propagationFlags">One of the <see cref="T:System.Security.AccessControl.PropagationFlags" /> values that specifies how to propagate Access Control Entries (ACEs) to child objects.</param>
	/// <param name="type">One of the <see cref="T:System.Security.AccessControl.AccessControlType" /> values that specifies whether access is allowed or denied.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="accessMask" />, <paramref name="inheritanceFlags" />, <paramref name="propagationFlags" />, or <paramref name="type" /> parameters specify an invalid value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="identityReference" /> parameter is null. -or-The <paramref name="accessMask" /> parameter is zero.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="identityReference" /> parameter is neither of type <see cref="T:System.Security.Principal.SecurityIdentifier" />, nor of a type such as <see cref="T:System.Security.Principal.NTAccount" /> that can be converted to type <see cref="T:System.Security.Principal.SecurityIdentifier" />.</exception>
	public sealed override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
	{
		return new FileSystemAccessRule(identityReference, (FileSystemRights)accessMask, isInherited, inheritanceFlags, propagationFlags, type);
	}

	/// <summary>Adds the specified access control list (ACL) permission to the current file or directory.</summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> object that represents an access control list (ACL) permission to add to a file or directory. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void AddAccessRule(FileSystemAccessRule rule)
	{
		AddAccessRule((AccessRule)rule);
	}

	/// <summary>Removes all matching allow or deny access control list (ACL) permissions from the current file or directory.</summary>
	/// <returns>true if the access rule was removed; otherwise, false.</returns>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> object that represents an access control list (ACL) permission to remove from a file or directory.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public bool RemoveAccessRule(FileSystemAccessRule rule)
	{
		return RemoveAccessRule((AccessRule)rule);
	}

	/// <summary>Removes all access control list (ACL) permissions for the specified user from the current file or directory.</summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> object that specifies a user whose access control list (ACL) permissions should be removed from a file or directory.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void RemoveAccessRuleAll(FileSystemAccessRule rule)
	{
		RemoveAccessRuleAll((AccessRule)rule);
	}

	/// <summary>Removes a single matching allow or deny access control list (ACL) permission from the current file or directory.</summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> object that specifies a user whose access control list (ACL) permissions should be removed from a file or directory.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void RemoveAccessRuleSpecific(FileSystemAccessRule rule)
	{
		RemoveAccessRuleSpecific((AccessRule)rule);
	}

	/// <summary>Adds the specified access control list (ACL) permission to the current file or directory and removes all matching ACL permissions.</summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> object that represents an access control list (ACL) permission to add to a file or directory.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void ResetAccessRule(FileSystemAccessRule rule)
	{
		ResetAccessRule((AccessRule)rule);
	}

	/// <summary>Sets the specified access control list (ACL) permission for the current file or directory. </summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAccessRule" /> object that represents an access control list (ACL) permission to set for a file or directory.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void SetAccessRule(FileSystemAccessRule rule)
	{
		SetAccessRule((AccessRule)rule);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.FileSystemAuditRule" /> class representing the specified audit rule for the specified user.</summary>
	/// <returns>A new <see cref="T:System.Security.AccessControl.FileSystemAuditRule" /> object representing the specified audit rule for the specified user.</returns>
	/// <param name="identityReference">An <see cref="T:System.Security.Principal.IdentityReference" /> object that represents a user account.</param>
	/// <param name="accessMask">An integer that specifies an access type.</param>
	/// <param name="isInherited">true if the access rule is inherited; otherwise, false.    </param>
	/// <param name="inheritanceFlags">One of the <see cref="T:System.Security.AccessControl.InheritanceFlags" /> values that specifies how to propagate access masks to child objects.</param>
	/// <param name="propagationFlags">One of the <see cref="T:System.Security.AccessControl.PropagationFlags" /> values that specifies how to propagate Access Control Entries (ACEs) to child objects.</param>
	/// <param name="flags">One of the <see cref="T:System.Security.AccessControl.AuditFlags" /> values that specifies the type of auditing to perform.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="accessMask" />, <paramref name="inheritanceFlags" />, <paramref name="propagationFlags" />, or <paramref name="flags" /> properties specify an invalid value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="identityReference" /> property is null. -or-The <paramref name="accessMask" /> property is zero.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="identityReference" /> property is neither of type <see cref="T:System.Security.Principal.SecurityIdentifier" />, nor of a type such as <see cref="T:System.Security.Principal.NTAccount" /> that can be converted to type <see cref="T:System.Security.Principal.SecurityIdentifier" />.</exception>
	public sealed override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
	{
		return new FileSystemAuditRule(identityReference, (FileSystemRights)accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
	}

	/// <summary>Adds the specified audit rule to the current file or directory.</summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAuditRule" />  object that represents an audit rule to add to a file or directory.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void AddAuditRule(FileSystemAuditRule rule)
	{
		AddAuditRule((AuditRule)rule);
	}

	/// <summary>Removes all matching allow or deny audit rules from the current file or directory.</summary>
	/// <returns>true if the audit rule was removed; otherwise, false</returns>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAuditRule" />  object that represents an audit rule to remove from a file or directory.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public bool RemoveAuditRule(FileSystemAuditRule rule)
	{
		return RemoveAuditRule((AuditRule)rule);
	}

	/// <summary>Removes all audit rules for the specified user from the current file or directory.</summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAuditRule" /> object that specifies a user whose audit rules should be removed from a file or directory.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void RemoveAuditRuleAll(FileSystemAuditRule rule)
	{
		RemoveAuditRuleAll((AuditRule)rule);
	}

	/// <summary>Removes a single matching allow or deny audit rule from the current file or directory.</summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAuditRule" />  object that represents an audit rule to remove from a file or directory.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void RemoveAuditRuleSpecific(FileSystemAuditRule rule)
	{
		RemoveAuditRuleSpecific((AuditRule)rule);
	}

	/// <summary>Sets the specified audit rule for the current file or directory.</summary>
	/// <param name="rule">A <see cref="T:System.Security.AccessControl.FileSystemAuditRule" /> object that represents an audit rule to set for a file or directory.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void SetAuditRule(FileSystemAuditRule rule)
	{
		SetAuditRule((AuditRule)rule);
	}

	internal FileSystemSecurity()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
