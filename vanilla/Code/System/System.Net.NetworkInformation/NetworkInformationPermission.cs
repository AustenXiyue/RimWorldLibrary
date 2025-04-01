using System.Security;
using System.Security.Permissions;

namespace System.Net.NetworkInformation;

/// <summary>Controls access to network information and traffic statistics for the local computer. This class cannot be inherited. </summary>
[Serializable]
public sealed class NetworkInformationPermission : CodeAccessPermission, IUnrestrictedPermission
{
	private NetworkInformationAccess access;

	private bool unrestricted;

	/// <summary>Gets the level of access to network information controlled by this permission. </summary>
	/// <returns>One of the <see cref="T:System.Net.NetworkInformation.NetworkInformationAccess" /> values.</returns>
	public NetworkInformationAccess Access => access;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkInformation.NetworkInformationPermission" /> class with the specified <see cref="T:System.Security.Permissions.PermissionState" />.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values.</param>
	public NetworkInformationPermission(PermissionState state)
	{
		if (state == PermissionState.Unrestricted)
		{
			access = NetworkInformationAccess.Read | NetworkInformationAccess.Ping;
			unrestricted = true;
		}
		else
		{
			access = NetworkInformationAccess.None;
		}
	}

	internal NetworkInformationPermission(bool unrestricted)
	{
		if (unrestricted)
		{
			access = NetworkInformationAccess.Read | NetworkInformationAccess.Ping;
			unrestricted = true;
		}
		else
		{
			access = NetworkInformationAccess.None;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkInformation.NetworkInformationPermission" /> class using the specified <see cref="T:System.Net.NetworkInformation.NetworkInformationAccess" /> value.</summary>
	/// <param name="access">One of the <see cref="T:System.Net.NetworkInformation.NetworkInformationAccess" /> values.</param>
	public NetworkInformationPermission(NetworkInformationAccess access)
	{
		this.access = access;
	}

	/// <summary>Adds the specified value to this permission.</summary>
	/// <param name="access">One of the <see cref="T:System.Net.NetworkInformation.NetworkInformationAccess" /> values.</param>
	public void AddPermission(NetworkInformationAccess access)
	{
		this.access |= access;
	}

	/// <summary>Returns a value indicating whether the current permission is unrestricted.</summary>
	/// <returns>true if the current permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return unrestricted;
	}

	/// <summary>Creates and returns an identical copy of this permission.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkInformation.NetworkInformationPermission" /> that is identical to the current permission</returns>
	public override IPermission Copy()
	{
		if (unrestricted)
		{
			return new NetworkInformationPermission(unrestricted: true);
		}
		return new NetworkInformationPermission(access);
	}

	/// <summary>Creates a permission that is the union of this permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A <see cref="T:System.Net.NetworkInformation.NetworkInformationPermission" />  permission to combine with the current permission. </param>
	public override IPermission Union(IPermission target)
	{
		if (target == null)
		{
			return Copy();
		}
		if (!(target is NetworkInformationPermission networkInformationPermission))
		{
			throw new ArgumentException(global::SR.GetString("Cannot cast target permission type."), "target");
		}
		if (unrestricted || networkInformationPermission.IsUnrestricted())
		{
			return new NetworkInformationPermission(unrestricted: true);
		}
		return new NetworkInformationPermission(access | networkInformationPermission.access);
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkInformation.NetworkInformationPermission" /> that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty or <paramref name="target" /> is null.</returns>
	/// <param name="target">An <see cref="T:System.Security.IPermission" /> to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not a <see cref="T:System.Net.NetworkInformation.NetworkInformationPermission" />.</exception>
	public override IPermission Intersect(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		if (!(target is NetworkInformationPermission networkInformationPermission))
		{
			throw new ArgumentException(global::SR.GetString("Cannot cast target permission type."), "target");
		}
		if (unrestricted && networkInformationPermission.IsUnrestricted())
		{
			return new NetworkInformationPermission(unrestricted: true);
		}
		return new NetworkInformationPermission(access & networkInformationPermission.access);
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">An <see cref="T:System.Security.IPermission" /> that is to be tested for the subset relationship. This permission must be of the same type as the current permission</param>
	public override bool IsSubsetOf(IPermission target)
	{
		if (target == null)
		{
			return access == NetworkInformationAccess.None;
		}
		if (!(target is NetworkInformationPermission networkInformationPermission))
		{
			throw new ArgumentException(global::SR.GetString("Cannot cast target permission type."), "target");
		}
		if (unrestricted && !networkInformationPermission.IsUnrestricted())
		{
			return false;
		}
		if ((access & networkInformationPermission.access) == access)
		{
			return true;
		}
		return false;
	}

	/// <summary>Sets the state of this permission using the specified XML encoding.</summary>
	/// <param name="securityElement">A <see cref="T:System.Security.SecurityElement" /> that contains the XML encoding to use to set the state of the current permission</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="securityElement" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="securityElement" /> is not a permission encoding.-or-<paramref name="securityElement" /> is not an encoding of a <see cref="T:System.Net.NetworkInformation.NetworkInformationPermission" />. -or-<paramref name="securityElement" /> has invalid <see cref="T:System.Net.NetworkInformation.NetworkInformationAccess" /> values.</exception>
	public override void FromXml(SecurityElement securityElement)
	{
		access = NetworkInformationAccess.None;
		if (securityElement == null)
		{
			throw new ArgumentNullException("securityElement");
		}
		if (!securityElement.Tag.Equals("IPermission"))
		{
			throw new ArgumentException(global::SR.GetString("Specified value does not contain 'IPermission' as its tag."), "securityElement");
		}
		if ((securityElement.Attribute("class") ?? throw new ArgumentException(global::SR.GetString("Specified value does not contain a 'class' attribute."), "securityElement")).IndexOf(GetType().FullName) < 0)
		{
			throw new ArgumentException(global::SR.GetString("The value class attribute is not valid."), "securityElement");
		}
		string text = securityElement.Attribute("Unrestricted");
		if (text != null && string.Compare(text, "true", StringComparison.OrdinalIgnoreCase) == 0)
		{
			access = NetworkInformationAccess.Read | NetworkInformationAccess.Ping;
			unrestricted = true;
		}
		else
		{
			if (securityElement.Children == null)
			{
				return;
			}
			foreach (SecurityElement child in securityElement.Children)
			{
				text = child.Attribute("Access");
				if (string.Compare(text, "Read", StringComparison.OrdinalIgnoreCase) == 0)
				{
					access |= NetworkInformationAccess.Read;
				}
				else if (string.Compare(text, "Ping", StringComparison.OrdinalIgnoreCase) == 0)
				{
					access |= NetworkInformationAccess.Ping;
				}
			}
		}
	}

	/// <summary>Creates an XML encoding of the state of this permission.</summary>
	/// <returns>A <see cref="T:System.Security.SecurityElement" /> that contains the XML encoding of the current permission.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = new SecurityElement("IPermission");
		securityElement.AddAttribute("class", GetType().FullName + ", " + GetType().Module.Assembly.FullName.Replace('"', '\''));
		securityElement.AddAttribute("version", "1");
		if (unrestricted)
		{
			securityElement.AddAttribute("Unrestricted", "true");
			return securityElement;
		}
		if ((access & NetworkInformationAccess.Read) > NetworkInformationAccess.None)
		{
			SecurityElement securityElement2 = new SecurityElement("NetworkInformationAccess");
			securityElement2.AddAttribute("Access", "Read");
			securityElement.AddChild(securityElement2);
		}
		if ((access & NetworkInformationAccess.Ping) > NetworkInformationAccess.None)
		{
			SecurityElement securityElement3 = new SecurityElement("NetworkInformationAccess");
			securityElement3.AddAttribute("Access", "Ping");
			securityElement.AddChild(securityElement3);
		}
		return securityElement;
	}
}
