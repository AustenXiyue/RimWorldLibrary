using System.Security;
using System.Security.Permissions;

namespace System.Net;

/// <summary>Specifies security actions to control <see cref="T:System.Net.Sockets.Socket" /> connections. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class SocketPermissionAttribute : CodeAccessSecurityAttribute
{
	private string m_access;

	private string m_host;

	private string m_port;

	private string m_transport;

	/// <summary>Gets or sets the network access method that is allowed by this <see cref="T:System.Net.SocketPermissionAttribute" />.</summary>
	/// <returns>A string that contains the network access method that is allowed by this instance of <see cref="T:System.Net.SocketPermissionAttribute" />. Valid values are "Accept" and "Connect." </returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Net.SocketPermissionAttribute.Access" /> property is not null when you attempt to set the value. To specify more than one Access method, use an additional attribute declaration statement. </exception>
	public string Access
	{
		get
		{
			return m_access;
		}
		set
		{
			if (m_access != null)
			{
				AlreadySet("Access");
			}
			m_access = value;
		}
	}

	/// <summary>Gets or sets the DNS host name or IP address that is specified by this <see cref="T:System.Net.SocketPermissionAttribute" />.</summary>
	/// <returns>A string that contains the DNS host name or IP address that is associated with this instance of <see cref="T:System.Net.SocketPermissionAttribute" />.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.SocketPermissionAttribute.Host" /> is not null when you attempt to set the value. To specify more than one host, use an additional attribute declaration statement. </exception>
	public string Host
	{
		get
		{
			return m_host;
		}
		set
		{
			if (m_host != null)
			{
				AlreadySet("Host");
			}
			m_host = value;
		}
	}

	/// <summary>Gets or sets the port number that is associated with this <see cref="T:System.Net.SocketPermissionAttribute" />.</summary>
	/// <returns>A string that contains the port number that is associated with this instance of <see cref="T:System.Net.SocketPermissionAttribute" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Net.SocketPermissionAttribute.Port" /> property is null when you attempt to set the value. To specify more than one port, use an additional attribute declaration statement. </exception>
	public string Port
	{
		get
		{
			return m_port;
		}
		set
		{
			if (m_port != null)
			{
				AlreadySet("Port");
			}
			m_port = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Net.TransportType" /> that is specified by this <see cref="T:System.Net.SocketPermissionAttribute" />.</summary>
	/// <returns>A string that contains the <see cref="T:System.Net.TransportType" /> that is associated with this <see cref="T:System.Net.SocketPermissionAttribute" />.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.SocketPermissionAttribute.Transport" /> is not null when you attempt to set the value. To specify more than one transport type, use an additional attribute declaration statement. </exception>
	public string Transport
	{
		get
		{
			return m_transport;
		}
		set
		{
			if (m_transport != null)
			{
				AlreadySet("Transport");
			}
			m_transport = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.SocketPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" /> value.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="action" /> is not a valid <see cref="T:System.Security.Permissions.SecurityAction" /> value. </exception>
	public SocketPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new instance of the <see cref="T:System.Net.SocketPermission" /> class.</summary>
	/// <returns>An instance of the <see cref="T:System.Net.SocketPermission" /> class that corresponds to the security declaration.</returns>
	/// <exception cref="T:System.ArgumentException">One or more of the current instance's <see cref="P:System.Net.SocketPermissionAttribute.Access" />, <see cref="P:System.Net.SocketPermissionAttribute.Host" />, <see cref="P:System.Net.SocketPermissionAttribute.Transport" />, or <see cref="P:System.Net.SocketPermissionAttribute.Port" /> properties is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override IPermission CreatePermission()
	{
		if (base.Unrestricted)
		{
			return new SocketPermission(PermissionState.Unrestricted);
		}
		string text = string.Empty;
		if (m_access == null)
		{
			text += "Access, ";
		}
		if (m_host == null)
		{
			text += "Host, ";
		}
		if (m_port == null)
		{
			text += "Port, ";
		}
		if (m_transport == null)
		{
			text += "Transport, ";
		}
		if (text.Length > 0)
		{
			string text2 = global::Locale.GetText("The value(s) for {0} must be specified.");
			text = text.Substring(0, text.Length - 2);
			throw new ArgumentException(string.Format(text2, text));
		}
		int num = -1;
		NetworkAccess access;
		if (string.Compare(m_access, "Connect", ignoreCase: true) == 0)
		{
			access = NetworkAccess.Connect;
		}
		else
		{
			if (string.Compare(m_access, "Accept", ignoreCase: true) != 0)
			{
				throw new ArgumentException(string.Format(global::Locale.GetText("The parameter value for 'Access', '{1}, is invalid."), m_access));
			}
			access = NetworkAccess.Accept;
		}
		if (string.Compare(m_port, "All", ignoreCase: true) != 0)
		{
			try
			{
				num = int.Parse(m_port);
			}
			catch
			{
				throw new ArgumentException(string.Format(global::Locale.GetText("The parameter value for 'Port', '{1}, is invalid."), m_port));
			}
			new IPEndPoint(1L, num);
		}
		TransportType transport;
		try
		{
			transport = (TransportType)Enum.Parse(typeof(TransportType), m_transport, ignoreCase: true);
		}
		catch
		{
			throw new ArgumentException(string.Format(global::Locale.GetText("The parameter value for 'Transport', '{1}, is invalid."), m_transport));
		}
		SocketPermission socketPermission = new SocketPermission(PermissionState.None);
		socketPermission.AddPermission(access, transport, m_host, num);
		return socketPermission;
	}

	internal void AlreadySet(string property)
	{
		throw new ArgumentException(string.Format(global::Locale.GetText("The parameter '{0}' can be set only once."), property), property);
	}
}
