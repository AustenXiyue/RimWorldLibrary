using System.Runtime.InteropServices;

namespace System.Security.Policy;

/// <summary>Defines evidence that represents permission requests. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class PermissionRequestEvidence : EvidenceBase, IBuiltInEvidence
{
	private PermissionSet requested;

	private PermissionSet optional;

	private PermissionSet denied;

	/// <summary>Gets the permissions the code explicitly asks not to be granted.</summary>
	/// <returns>The permissions the code explicitly asks not to be granted.</returns>
	public PermissionSet DeniedPermissions => denied;

	/// <summary>Gets the permissions the code can use if they are granted, but are not required.</summary>
	/// <returns>The permissions the code can use if they are granted, but are not required.</returns>
	public PermissionSet OptionalPermissions => optional;

	/// <summary>Gets the minimum permissions the code requires to run.</summary>
	/// <returns>The minimum permissions the code requires to run.</returns>
	public PermissionSet RequestedPermissions => requested;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.PermissionRequestEvidence" /> class with the permission request of a code assembly.</summary>
	/// <param name="request">The minimum permissions the code requires to run. </param>
	/// <param name="optional">The permissions the code can use if they are granted, but that are not required. </param>
	/// <param name="denied">The permissions the code explicitly asks not to be granted. </param>
	public PermissionRequestEvidence(PermissionSet request, PermissionSet optional, PermissionSet denied)
	{
		if (request != null)
		{
			requested = new PermissionSet(request);
		}
		if (optional != null)
		{
			this.optional = new PermissionSet(optional);
		}
		if (denied != null)
		{
			this.denied = new PermissionSet(denied);
		}
	}

	/// <summary>Creates an equivalent copy of the current <see cref="T:System.Security.Policy.PermissionRequestEvidence" />.</summary>
	/// <returns>An equivalent copy of the current <see cref="T:System.Security.Policy.PermissionRequestEvidence" />.</returns>
	public PermissionRequestEvidence Copy()
	{
		return new PermissionRequestEvidence(requested, optional, denied);
	}

	/// <summary>Gets a string representation of the state of the <see cref="T:System.Security.Policy.PermissionRequestEvidence" />.</summary>
	/// <returns>A representation of the state of the <see cref="T:System.Security.Policy.PermissionRequestEvidence" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		SecurityElement securityElement = new SecurityElement("System.Security.Policy.PermissionRequestEvidence");
		securityElement.AddAttribute("version", "1");
		if (requested != null)
		{
			SecurityElement securityElement2 = new SecurityElement("Request");
			securityElement2.AddChild(requested.ToXml());
			securityElement.AddChild(securityElement2);
		}
		if (optional != null)
		{
			SecurityElement securityElement3 = new SecurityElement("Optional");
			securityElement3.AddChild(optional.ToXml());
			securityElement.AddChild(securityElement3);
		}
		if (denied != null)
		{
			SecurityElement securityElement4 = new SecurityElement("Denied");
			securityElement4.AddChild(denied.ToXml());
			securityElement.AddChild(securityElement4);
		}
		return securityElement.ToString();
	}

	int IBuiltInEvidence.GetRequiredSize(bool verbose)
	{
		int num = ((!verbose) ? 1 : 3);
		if (requested != null)
		{
			int num2 = requested.ToXml().ToString().Length + (verbose ? 5 : 0);
			num += num2;
		}
		if (optional != null)
		{
			int num3 = optional.ToXml().ToString().Length + (verbose ? 5 : 0);
			num += num3;
		}
		if (denied != null)
		{
			int num4 = denied.ToXml().ToString().Length + (verbose ? 5 : 0);
			num += num4;
		}
		return num;
	}

	[MonoTODO("IBuiltInEvidence")]
	int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
	{
		return 0;
	}

	[MonoTODO("IBuiltInEvidence")]
	int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
	{
		return 0;
	}
}
