using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace System.Net;

/// <summary>Controls rights to make or accept connections on a transport address.</summary>
[Serializable]
public sealed class SocketPermission : CodeAccessPermission, IUnrestrictedPermission
{
	private ArrayList m_acceptList = new ArrayList();

	private ArrayList m_connectList = new ArrayList();

	private bool m_noRestriction;

	/// <summary>Defines a constant that represents all ports.</summary>
	public const int AllPorts = -1;

	/// <summary>Gets a list of <see cref="T:System.Net.EndpointPermission" /> instances that identifies the endpoints that can be accepted under this permission instance.</summary>
	/// <returns>An instance that implements the <see cref="T:System.Collections.IEnumerator" /> interface that contains <see cref="T:System.Net.EndpointPermission" /> instances.</returns>
	public IEnumerator AcceptList => m_acceptList.GetEnumerator();

	/// <summary>Gets a list of <see cref="T:System.Net.EndpointPermission" /> instances that identifies the endpoints that can be connected to under this permission instance.</summary>
	/// <returns>An instance that implements the <see cref="T:System.Collections.IEnumerator" /> interface that contains <see cref="T:System.Net.EndpointPermission" /> instances.</returns>
	public IEnumerator ConnectList => m_connectList.GetEnumerator();

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.SocketPermission" /> class that allows unrestricted access to the <see cref="T:System.Net.Sockets.Socket" /> or disallows access to the <see cref="T:System.Net.Sockets.Socket" />.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	public SocketPermission(PermissionState state)
	{
		m_noRestriction = state == PermissionState.Unrestricted;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.SocketPermission" /> class for the given transport address with the specified permission.</summary>
	/// <param name="access">One of the <see cref="T:System.Net.NetworkAccess" /> values. </param>
	/// <param name="transport">One of the <see cref="T:System.Net.TransportType" /> values. </param>
	/// <param name="hostName">The host name for the transport address. </param>
	/// <param name="portNumber">The port number for the transport address. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostName" /> is null. </exception>
	public SocketPermission(NetworkAccess access, TransportType transport, string hostName, int portNumber)
	{
		m_noRestriction = false;
		AddPermission(access, transport, hostName, portNumber);
	}

	/// <summary>Adds a permission to the set of permissions for a transport address.</summary>
	/// <param name="access">One of the <see cref="T:System.Net.NetworkAccess" /> values. </param>
	/// <param name="transport">One of the <see cref="T:System.Net.TransportType" /> values. </param>
	/// <param name="hostName">The host name for the transport address. </param>
	/// <param name="portNumber">The port number for the transport address. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="hostName" /> is null. </exception>
	public void AddPermission(NetworkAccess access, TransportType transport, string hostName, int portNumber)
	{
		if (!m_noRestriction)
		{
			EndpointPermission value = new EndpointPermission(hostName, portNumber, transport);
			if (access == NetworkAccess.Accept)
			{
				m_acceptList.Add(value);
			}
			else
			{
				m_connectList.Add(value);
			}
		}
	}

	/// <summary>Creates a copy of a <see cref="T:System.Net.SocketPermission" /> instance.</summary>
	/// <returns>A new instance of the <see cref="T:System.Net.SocketPermission" /> class that is a copy of the current instance.</returns>
	public override IPermission Copy()
	{
		return new SocketPermission(m_noRestriction ? PermissionState.Unrestricted : PermissionState.None)
		{
			m_connectList = (ArrayList)m_connectList.Clone(),
			m_acceptList = (ArrayList)m_acceptList.Clone()
		};
	}

	/// <summary>Returns the logical intersection between two <see cref="T:System.Net.SocketPermission" /> instances.</summary>
	/// <returns>The <see cref="T:System.Net.SocketPermission" /> instance that represents the intersection of two <see cref="T:System.Net.SocketPermission" /> instances. If the intersection is empty, the method returns null. If the <paramref name="target" /> parameter is a null reference, the method returns null.</returns>
	/// <param name="target">The <see cref="T:System.Net.SocketPermission" /> instance to intersect with the current instance. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not a <see cref="T:System.Net.SocketPermission" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">
	///   <see cref="T:System.Net.DnsPermission" /> is not granted to the method caller. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override IPermission Intersect(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		if (!(target is SocketPermission socketPermission))
		{
			throw new ArgumentException("Argument not of type SocketPermission");
		}
		if (m_noRestriction)
		{
			if (!IntersectEmpty(socketPermission))
			{
				return socketPermission.Copy();
			}
			return null;
		}
		if (socketPermission.m_noRestriction)
		{
			if (!IntersectEmpty(this))
			{
				return Copy();
			}
			return null;
		}
		SocketPermission socketPermission2 = new SocketPermission(PermissionState.None);
		Intersect(m_connectList, socketPermission.m_connectList, socketPermission2.m_connectList);
		Intersect(m_acceptList, socketPermission.m_acceptList, socketPermission2.m_acceptList);
		if (!IntersectEmpty(socketPermission2))
		{
			return socketPermission2;
		}
		return null;
	}

	private bool IntersectEmpty(SocketPermission permission)
	{
		if (!permission.m_noRestriction && permission.m_connectList.Count == 0)
		{
			return permission.m_acceptList.Count == 0;
		}
		return false;
	}

	private void Intersect(ArrayList list1, ArrayList list2, ArrayList result)
	{
		foreach (EndpointPermission item in list1)
		{
			foreach (EndpointPermission item2 in list2)
			{
				EndpointPermission endpointPermission2 = item.Intersect(item2);
				if (endpointPermission2 == null)
				{
					continue;
				}
				bool flag = false;
				for (int i = 0; i < result.Count; i++)
				{
					EndpointPermission perm2 = (EndpointPermission)result[i];
					EndpointPermission endpointPermission3 = endpointPermission2.Intersect(perm2);
					if (endpointPermission3 != null)
					{
						result[i] = endpointPermission3;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					result.Add(endpointPermission2);
				}
			}
		}
	}

	/// <summary>Determines if the current permission is a subset of the specified permission.</summary>
	/// <returns>If <paramref name="target" /> is null, this method returns true if the current instance defines no permissions; otherwise, false. If <paramref name="target" /> is not null, this method returns true if the current instance defines a subset of <paramref name="target" /> permissions; otherwise, false.</returns>
	/// <param name="target">A <see cref="T:System.Net.SocketPermission" /> that is to be tested for the subset relationship. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not a <see cref="T:System.Net.Sockets.SocketException" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">
	///   <see cref="T:System.Net.DnsPermission" /> is not granted to the method caller. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Net.DnsPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override bool IsSubsetOf(IPermission target)
	{
		if (target == null)
		{
			if (!m_noRestriction && m_connectList.Count == 0)
			{
				return m_acceptList.Count == 0;
			}
			return false;
		}
		if (!(target is SocketPermission socketPermission))
		{
			throw new ArgumentException("Parameter target must be of type SocketPermission");
		}
		if (socketPermission.m_noRestriction)
		{
			return true;
		}
		if (m_noRestriction)
		{
			return false;
		}
		if (m_acceptList.Count == 0 && m_connectList.Count == 0)
		{
			return true;
		}
		if (socketPermission.m_acceptList.Count == 0 && socketPermission.m_connectList.Count == 0)
		{
			return false;
		}
		if (IsSubsetOf(m_connectList, socketPermission.m_connectList))
		{
			return IsSubsetOf(m_acceptList, socketPermission.m_acceptList);
		}
		return false;
	}

	private bool IsSubsetOf(ArrayList list1, ArrayList list2)
	{
		foreach (EndpointPermission item in list1)
		{
			bool flag = false;
			foreach (EndpointPermission item2 in list2)
			{
				if (item.IsSubsetOf(item2))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Checks the overall permission state of the object.</summary>
	/// <returns>true if the <see cref="T:System.Net.SocketPermission" /> instance is created with the Unrestricted value from <see cref="T:System.Security.Permissions.PermissionState" />; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return m_noRestriction;
	}

	/// <summary>Creates an XML encoding of a <see cref="T:System.Net.SocketPermission" /> instance and its current state.</summary>
	/// <returns>A <see cref="T:System.Security.SecurityElement" /> instance that contains an XML-encoded representation of the <see cref="T:System.Net.SocketPermission" /> instance, including state information.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = new SecurityElement("IPermission");
		securityElement.AddAttribute("class", GetType().AssemblyQualifiedName);
		securityElement.AddAttribute("version", "1");
		if (m_noRestriction)
		{
			securityElement.AddAttribute("Unrestricted", "true");
			return securityElement;
		}
		if (m_connectList.Count > 0)
		{
			ToXml(securityElement, "ConnectAccess", m_connectList.GetEnumerator());
		}
		if (m_acceptList.Count > 0)
		{
			ToXml(securityElement, "AcceptAccess", m_acceptList.GetEnumerator());
		}
		return securityElement;
	}

	private void ToXml(SecurityElement root, string childName, IEnumerator enumerator)
	{
		SecurityElement securityElement = new SecurityElement(childName);
		while (enumerator.MoveNext())
		{
			EndpointPermission endpointPermission = enumerator.Current as EndpointPermission;
			SecurityElement securityElement2 = new SecurityElement("ENDPOINT");
			securityElement2.AddAttribute("host", endpointPermission.Hostname);
			securityElement2.AddAttribute("transport", endpointPermission.Transport.ToString());
			securityElement2.AddAttribute("port", (endpointPermission.Port == -1) ? "All" : endpointPermission.Port.ToString());
			securityElement.AddChild(securityElement2);
		}
		root.AddChild(securityElement);
	}

	/// <summary>Reconstructs a <see cref="T:System.Net.SocketPermission" /> instance for an XML encoding.</summary>
	/// <param name="securityElement">The XML encoding used to reconstruct the <see cref="T:System.Net.SocketPermission" /> instance. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="securityElement" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="securityElement" /> is not a permission element for this type. </exception>
	public override void FromXml(SecurityElement securityElement)
	{
		if (securityElement == null)
		{
			throw new ArgumentNullException("securityElement");
		}
		if (securityElement.Tag != "IPermission")
		{
			throw new ArgumentException("securityElement");
		}
		string text = securityElement.Attribute("Unrestricted");
		if (text != null)
		{
			m_noRestriction = string.Compare(text, "true", ignoreCase: true) == 0;
			if (m_noRestriction)
			{
				return;
			}
		}
		m_noRestriction = false;
		m_connectList = new ArrayList();
		m_acceptList = new ArrayList();
		foreach (SecurityElement child in securityElement.Children)
		{
			if (child.Tag == "ConnectAccess")
			{
				FromXml(child.Children, NetworkAccess.Connect);
			}
			else if (child.Tag == "AcceptAccess")
			{
				FromXml(child.Children, NetworkAccess.Accept);
			}
		}
	}

	private void FromXml(ArrayList endpoints, NetworkAccess access)
	{
		foreach (SecurityElement endpoint in endpoints)
		{
			if (!(endpoint.Tag != "ENDPOINT"))
			{
				string hostName = endpoint.Attribute("host");
				TransportType transport = (TransportType)Enum.Parse(typeof(TransportType), endpoint.Attribute("transport"), ignoreCase: true);
				string text = endpoint.Attribute("port");
				int num = 0;
				num = ((!(text == "All")) ? int.Parse(text) : (-1));
				AddPermission(access, transport, hostName, num);
			}
		}
	}

	/// <summary>Returns the logical union between two <see cref="T:System.Net.SocketPermission" /> instances.</summary>
	/// <returns>The <see cref="T:System.Net.SocketPermission" /> instance that represents the union of two <see cref="T:System.Net.SocketPermission" /> instances. If <paramref name="target" /> parameter is null, it returns a copy of the current instance.</returns>
	/// <param name="target">The <see cref="T:System.Net.SocketPermission" /> instance to combine with the current instance. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not a <see cref="T:System.Net.SocketPermission" />. </exception>
	public override IPermission Union(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		if (!(target is SocketPermission socketPermission))
		{
			throw new ArgumentException("Argument not of type SocketPermission");
		}
		if (m_noRestriction || socketPermission.m_noRestriction)
		{
			return new SocketPermission(PermissionState.Unrestricted);
		}
		SocketPermission socketPermission2 = (SocketPermission)socketPermission.Copy();
		socketPermission2.m_acceptList.InsertRange(socketPermission2.m_acceptList.Count, m_acceptList);
		socketPermission2.m_connectList.InsertRange(socketPermission2.m_connectList.Count, m_connectList);
		return socketPermission2;
	}
}
