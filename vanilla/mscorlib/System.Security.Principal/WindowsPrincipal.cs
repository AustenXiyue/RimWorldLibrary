using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Permissions;
using Unity;

namespace System.Security.Principal;

/// <summary>Enables code to check the Windows group membership of a Windows user.</summary>
[Serializable]
[ComVisible(true)]
public class WindowsPrincipal : ClaimsPrincipal
{
	private WindowsIdentity _identity;

	private string[] m_roles;

	/// <summary>Gets the identity of the current principal.</summary>
	/// <returns>The <see cref="T:System.Security.Principal.WindowsIdentity" /> object of the current principal.</returns>
	public override IIdentity Identity => _identity;

	private IntPtr Token => _identity.Token;

	/// <summary>Gets all Windows device claims from this principal.</summary>
	/// <returns>A collection of all Windows device claims from this principal.</returns>
	public virtual IEnumerable<Claim> DeviceClaims
	{
		get
		{
			//IL_0007: Expected O, but got I4
			ThrowStub.ThrowNotSupportedException();
			return (IEnumerable<Claim>)0;
		}
	}

	/// <summary>Gets all Windows user claims from this principal.</summary>
	/// <returns>A collection of all Windows user claims from this principal.</returns>
	public virtual IEnumerable<Claim> UserClaims
	{
		get
		{
			//IL_0007: Expected O, but got I4
			ThrowStub.ThrowNotSupportedException();
			return (IEnumerable<Claim>)0;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.WindowsPrincipal" /> class by using the specified <see cref="T:System.Security.Principal.WindowsIdentity" /> object.</summary>
	/// <param name="ntIdentity">The object from which to construct the new instance of <see cref="T:System.Security.Principal.WindowsPrincipal" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ntIdentity" /> is null. </exception>
	public WindowsPrincipal(WindowsIdentity ntIdentity)
	{
		if (ntIdentity == null)
		{
			throw new ArgumentNullException("ntIdentity");
		}
		_identity = ntIdentity;
	}

	/// <summary>Determines whether the current principal belongs to the Windows user group with the specified relative identifier (RID).</summary>
	/// <returns>true if the current principal is a member of the specified Windows user group, that is, in a particular role; otherwise, false.</returns>
	/// <param name="rid">The RID of the Windows user group in which to check for the principal’s membership status. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public virtual bool IsInRole(int rid)
	{
		if (Environment.IsUnix)
		{
			return IsMemberOfGroupId(Token, (IntPtr)rid);
		}
		string text = null;
		switch (rid)
		{
		case 544:
			text = "BUILTIN\\Administrators";
			break;
		case 545:
			text = "BUILTIN\\Users";
			break;
		case 546:
			text = "BUILTIN\\Guests";
			break;
		case 547:
			text = "BUILTIN\\Power Users";
			break;
		case 548:
			text = "BUILTIN\\Account Operators";
			break;
		case 549:
			text = "BUILTIN\\System Operators";
			break;
		case 550:
			text = "BUILTIN\\Print Operators";
			break;
		case 551:
			text = "BUILTIN\\Backup Operators";
			break;
		case 552:
			text = "BUILTIN\\Replicator";
			break;
		default:
			return false;
		}
		return IsInRole(text);
	}

	/// <summary>Determines whether the current principal belongs to the Windows user group with the specified name.</summary>
	/// <returns>true if the current principal is a member of the specified Windows user group; otherwise, false.</returns>
	/// <param name="role">The name of the Windows user group for which to check membership. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
	public override bool IsInRole(string role)
	{
		if (role == null)
		{
			return false;
		}
		if (Environment.IsUnix)
		{
			return IsMemberOfGroupName(Token, role);
		}
		if (m_roles == null)
		{
			m_roles = WindowsIdentity._GetRoles(Token);
		}
		role = role.ToUpperInvariant();
		string[] roles = m_roles;
		foreach (string text in roles)
		{
			if (text != null && role == text.ToUpperInvariant())
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Determines whether the current principal belongs to the Windows user group with the specified <see cref="T:System.Security.Principal.WindowsBuiltInRole" />.</summary>
	/// <returns>true if the current principal is a member of the specified Windows user group; otherwise, false.</returns>
	/// <param name="role">One of the <see cref="T:System.Security.Principal.WindowsBuiltInRole" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="role" /> is not a valid <see cref="T:System.Security.Principal.WindowsBuiltInRole" /> value.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public virtual bool IsInRole(WindowsBuiltInRole role)
	{
		if (Environment.IsUnix)
		{
			string text = null;
			if (role == WindowsBuiltInRole.Administrator)
			{
				text = "root";
				return IsInRole(text);
			}
			return false;
		}
		return IsInRole((int)role);
	}

	/// <summary>Determines whether the current principal belongs to the Windows user group with the specified security identifier (SID).</summary>
	/// <returns>true if the current principal is a member of the specified Windows user group; otherwise, false.</returns>
	/// <param name="sid">A <see cref="T:System.Security.Principal.SecurityIdentifier" />  that uniquely identifies a Windows user group.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sid" /> is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">Windows returned a Win32 error.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	[MonoTODO("not implemented")]
	[ComVisible(false)]
	public virtual bool IsInRole(SecurityIdentifier sid)
	{
		throw new NotImplementedException();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool IsMemberOfGroupId(IntPtr user, IntPtr group);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool IsMemberOfGroupName(IntPtr user, string group);
}
