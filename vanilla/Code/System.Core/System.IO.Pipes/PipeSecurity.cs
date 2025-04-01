using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.IO.Pipes;

/// <summary>Represents the access control and audit security for a pipe.</summary>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class PipeSecurity : NativeObjectSecurity
{
	/// <summary>Gets the <see cref="T:System.Type" /> of the securable object that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <returns>The type of the securable object that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</returns>
	public override Type AccessRightType => typeof(PipeAccessRights);

	/// <summary>Gets the <see cref="T:System.Type" /> of the object that is associated with the access rules of the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <returns>The type of the object that is associated with the access rules of the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</returns>
	public override Type AccessRuleType => typeof(PipeAccessRule);

	/// <summary>Gets the <see cref="T:System.Type" /> object associated with the audit rules of the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <returns>The type of the object that is associated with the audit rules of the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</returns>
	public override Type AuditRuleType => typeof(PipeAuditRule);

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.PipeSecurity" /> class.</summary>
	public PipeSecurity()
		: base(isContainer: false, ResourceType.FileObject)
	{
	}

	internal PipeSecurity(SafeHandle handle, AccessControlSections includeSections)
		: base(isContainer: false, ResourceType.FileObject, handle, includeSections)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.AccessRule" /> class with the specified values.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.AccessRule" /> object that this method creates.</returns>
	/// <param name="identityReference">The identity that the access rule applies to. It must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</param>
	/// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators</param>
	/// <param name="isInherited">true if this rule is inherited from a parent container; otherwise false.</param>
	/// <param name="inheritanceFlags">One of the <see cref="T:System.Security.AccessControl.InheritanceFlags" /> values that specifies the inheritance properties of the access rule.</param>
	/// <param name="propagationFlags">One of the <see cref="T:System.Security.AccessControl.PropagationFlags" /> values that specifies whether inherited access rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <param name="type">Specifies the valid access control type.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="accessMask" />, <paramref name="inheritanceFlags" />, <paramref name="propagationFlags" />, or <paramref name="type" /> specifies an invalid value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identityReference" /> is null. -or-<paramref name="accessMask" /> is zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="identityReference" /> is neither of type <see cref="T:System.Security.Principal.SecurityIdentifier" /> nor of a type, such as <see cref="T:System.Security.Principal.NTAccount" />, that can be converted to type <see cref="T:System.Security.Principal.SecurityIdentifier" />.</exception>
	public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
	{
		return new PipeAccessRule(identityReference, (PipeAccessRights)accessMask, type);
	}

	/// <summary>Adds an access rule to the Discretionary Access Control List (DACL) that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <param name="rule">The access rule to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void AddAccessRule(PipeAccessRule rule)
	{
		AddAccessRule((AccessRule)rule);
	}

	/// <summary>Adds an audit rule to the System Access Control List (SACL)that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <param name="rule">The audit rule to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void AddAuditRule(PipeAuditRule rule)
	{
		AddAuditRule((AuditRule)rule);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.AccessControl.AuditRule" /> class with the specified values.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.AuditRule" /> object that this method creates.</returns>
	/// <param name="identityReference">The identity that the access rule applies to. It must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier" /> object.</param>
	/// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators</param>
	/// <param name="isInherited">true if this rule is inherited from a parent container; otherwise, false..</param>
	/// <param name="inheritanceFlags">One of the <see cref="T:System.Security.AccessControl.InheritanceFlags" /> values that specifies the inheritance properties of the access rule.</param>
	/// <param name="propagationFlags">One of the <see cref="T:System.Security.AccessControl.PropagationFlags" /> values that specifies whether inherited access rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags" /> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None" />.</param>
	/// <param name="flags">One of the <see cref="T:System.Security.AccessControl.AuditFlags" /> values that specifies the valid access control type.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="accessMask" />, <paramref name="inheritanceFlags" />, <paramref name="propagationFlags" />, or <paramref name="flags" /> properties specify an invalid value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="identityReference" /> property is null. -or-The <paramref name="accessMask" /> property is zero.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="identityReference" /> property is neither of type <see cref="T:System.Security.Principal.SecurityIdentifier" /> nor of a type, such as <see cref="T:System.Security.Principal.NTAccount" />, that can be converted to type <see cref="T:System.Security.Principal.SecurityIdentifier" />.</exception>
	public sealed override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
	{
		return new PipeAuditRule(identityReference, (PipeAccessRights)accessMask, flags);
	}

	/// <summary>Saves the specified sections of the security descriptor that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object to permanent storage.</summary>
	/// <param name="handle">The handle of the securable object that the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object is associated with.</param>
	[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
	protected internal void Persist(SafeHandle handle)
	{
		WriteLock();
		try
		{
			Persist(handle, base.AccessControlSectionsModified, null);
		}
		finally
		{
			WriteUnlock();
		}
	}

	/// <summary>Saves the specified sections of the security descriptor that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object to permanent storage.</summary>
	/// <param name="name">The name of the securable object that the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object is associated with.</param>
	[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
	protected internal void Persist(string name)
	{
		WriteLock();
		try
		{
			Persist(name, base.AccessControlSectionsModified, null);
		}
		finally
		{
			WriteUnlock();
		}
	}

	/// <summary>Removes an access rule from the Discretionary Access Control List (DACL) that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <returns>true if the operation is successful; otherwise, false.</returns>
	/// <param name="rule">The access rule to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public bool RemoveAccessRule(PipeAccessRule rule)
	{
		return RemoveAccessRule((AccessRule)rule);
	}

	/// <summary>Removes the specified access rule from the Discretionary Access Control List (DACL) that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <param name="rule">The access rule to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void RemoveAccessRuleSpecific(PipeAccessRule rule)
	{
		RemoveAccessRuleSpecific((AccessRule)rule);
	}

	/// <summary>Removes an audit rule from the System Access Control List (SACL) that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <returns>true if the audit rule was removed; otherwise, false</returns>
	/// <param name="rule">The audit rule to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public bool RemoveAuditRule(PipeAuditRule rule)
	{
		return RemoveAuditRule((AuditRule)rule);
	}

	/// <summary>Removes all audit rules that have the same security identifier as the specified audit rule from the System Access Control List (SACL) that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <param name="rule">The audit rule to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void RemoveAuditRuleAll(PipeAuditRule rule)
	{
		RemoveAuditRuleAll((AuditRule)rule);
	}

	/// <summary>Removes the specified audit rule from the System Access Control List (SACL) that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <param name="rule">The audit rule to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void RemoveAuditRuleSpecific(PipeAuditRule rule)
	{
		RemoveAuditRuleSpecific((AuditRule)rule);
	}

	/// <summary>Removes all access rules in the Discretionary Access Control List (DACL) that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object and then adds the specified access rule.</summary>
	/// <param name="rule">The access rule to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void ResetAccessRule(PipeAccessRule rule)
	{
		ResetAccessRule((AccessRule)rule);
	}

	/// <summary>Sets an access rule in the Discretionary Access Control List (DACL) that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <param name="rule">The rule to set.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void SetAccessRule(PipeAccessRule rule)
	{
		SetAccessRule((AccessRule)rule);
	}

	/// <summary>Sets an audit rule in the System Access Control List (SACL) that is associated with the current <see cref="T:System.IO.Pipes.PipeSecurity" /> object.</summary>
	/// <param name="rule">The rule to set.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="rule" /> parameter is null.</exception>
	public void SetAuditRule(PipeAuditRule rule)
	{
		SetAuditRule((AuditRule)rule);
	}
}
