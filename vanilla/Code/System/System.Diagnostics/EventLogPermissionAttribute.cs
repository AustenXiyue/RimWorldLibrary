using System.Security;
using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Allows declaritive permission checks for event logging. </summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Event, AllowMultiple = true, Inherited = false)]
public class EventLogPermissionAttribute : CodeAccessSecurityAttribute
{
	private string machineName;

	private EventLogPermissionAccess permissionAccess;

	/// <summary>Gets or sets the name of the computer on which events might be read.</summary>
	/// <returns>The name of the computer on which events might be read. The default is ".".</returns>
	/// <exception cref="T:System.ArgumentException">The computer name is invalid. </exception>
	/// <filterpriority>2</filterpriority>
	public string MachineName
	{
		get
		{
			return machineName;
		}
		set
		{
			ResourcePermissionBase.ValidateMachineName(value);
			machineName = value;
		}
	}

	/// <summary>Gets or sets the access levels used in the permissions request.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Diagnostics.EventLogPermissionAccess" /> values. The default is <see cref="F:System.Diagnostics.EventLogPermissionAccess.Write" />.</returns>
	/// <filterpriority>2</filterpriority>
	public EventLogPermissionAccess PermissionAccess
	{
		get
		{
			return permissionAccess;
		}
		set
		{
			permissionAccess = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLogPermissionAttribute" /> class.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public EventLogPermissionAttribute(SecurityAction action)
		: base(action)
	{
		machineName = ".";
		permissionAccess = EventLogPermissionAccess.Write;
	}

	/// <summary>Creates the permission based on the <see cref="P:System.Diagnostics.EventLogPermissionAttribute.MachineName" /> property and the requested access levels that are set through the <see cref="P:System.Diagnostics.EventLogPermissionAttribute.PermissionAccess" /> property on the attribute.</summary>
	/// <returns>An <see cref="T:System.Security.IPermission" /> that represents the created permission.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override IPermission CreatePermission()
	{
		if (base.Unrestricted)
		{
			return new EventLogPermission(PermissionState.Unrestricted);
		}
		return new EventLogPermission(permissionAccess, machineName);
	}
}
