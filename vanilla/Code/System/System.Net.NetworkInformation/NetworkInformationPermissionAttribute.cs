using System.Security;
using System.Security.Permissions;

namespace System.Net.NetworkInformation;

/// <summary>Allows security actions for <see cref="T:System.Net.NetworkInformation.NetworkInformationPermission" /> to be applied to code using declarative security.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class NetworkInformationPermissionAttribute : CodeAccessSecurityAttribute
{
	private const string strAccess = "Access";

	private string access;

	/// <summary>Gets or sets the network information access level.</summary>
	/// <returns>A string that specifies the access level.</returns>
	public string Access
	{
		get
		{
			return access;
		}
		set
		{
			access = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkInformation.NetworkInformationPermissionAttribute" /> class.</summary>
	/// <param name="action">A <see cref="T:System.Security.Permissions.SecurityAction" /> value that specifies the permission behavior.</param>
	public NetworkInformationPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new <see cref="T:System.Net.NetworkInformation.NetworkInformationPermission" /> object.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkInformation.NetworkInformationPermission" /> that corresponds to this attribute.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override IPermission CreatePermission()
	{
		NetworkInformationPermission networkInformationPermission = null;
		if (base.Unrestricted)
		{
			networkInformationPermission = new NetworkInformationPermission(PermissionState.Unrestricted);
		}
		else
		{
			networkInformationPermission = new NetworkInformationPermission(PermissionState.None);
			if (access != null)
			{
				if (string.Compare(access, "Read", StringComparison.OrdinalIgnoreCase) == 0)
				{
					networkInformationPermission.AddPermission(NetworkInformationAccess.Read);
				}
				else if (string.Compare(access, "Ping", StringComparison.OrdinalIgnoreCase) == 0)
				{
					networkInformationPermission.AddPermission(NetworkInformationAccess.Ping);
				}
				else
				{
					if (string.Compare(access, "None", StringComparison.OrdinalIgnoreCase) != 0)
					{
						throw new ArgumentException(global::SR.GetString("The parameter value '{0}={1}' is invalid.", "Access", access));
					}
					networkInformationPermission.AddPermission(NetworkInformationAccess.None);
				}
			}
		}
		return networkInformationPermission;
	}
}
