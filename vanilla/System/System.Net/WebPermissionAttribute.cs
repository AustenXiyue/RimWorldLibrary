using System.Security;
using System.Security.Permissions;

namespace System.Net;

/// <summary>Specifies permission to access Internet resources. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class WebPermissionAttribute : CodeAccessSecurityAttribute
{
	private object m_accept;

	private object m_connect;

	/// <summary>Gets or sets the URI connection string controlled by the current <see cref="T:System.Net.WebPermissionAttribute" />.</summary>
	/// <returns>A string containing the URI connection controlled by the current <see cref="T:System.Net.WebPermissionAttribute" />.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.WebPermissionAttribute.Connect" /> is not null when you attempt to set the value. If you wish to specify more than one Connect URI, use an additional attribute declaration statement. </exception>
	public string Connect
	{
		get
		{
			return m_connect as string;
		}
		set
		{
			if (m_connect != null)
			{
				throw new ArgumentException(global::SR.GetString("The permission '{0}={1}' cannot be added. Add a separate Attribute statement.", "Connect", value), "value");
			}
			m_connect = value;
		}
	}

	/// <summary>Gets or sets the URI string accepted by the current <see cref="T:System.Net.WebPermissionAttribute" />.</summary>
	/// <returns>A string containing the URI accepted by the current <see cref="T:System.Net.WebPermissionAttribute" />.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.WebPermissionAttribute.Accept" /> is not null when you attempt to set the value. If you wish to specify more than one Accept URI, use an additional attribute declaration statement. </exception>
	public string Accept
	{
		get
		{
			return m_accept as string;
		}
		set
		{
			if (m_accept != null)
			{
				throw new ArgumentException(global::SR.GetString("The permission '{0}={1}' cannot be added. Add a separate Attribute statement.", "Accept", value), "value");
			}
			m_accept = value;
		}
	}

	/// <summary>Gets or sets a regular expression pattern that describes the URI connection controlled by the current <see cref="T:System.Net.WebPermissionAttribute" />.</summary>
	/// <returns>A string containing a regular expression pattern that describes the URI connection controlled by this <see cref="T:System.Net.WebPermissionAttribute" />.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.WebPermissionAttribute.ConnectPattern" /> is not null when you attempt to set the value. If you wish to specify more than one connect URI, use an additional attribute declaration statement. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string ConnectPattern
	{
		get
		{
			if (!(m_connect is DelayedRegex))
			{
				if (!(m_connect is bool) || !(bool)m_connect)
				{
					return null;
				}
				return ".*";
			}
			return m_connect.ToString();
		}
		set
		{
			if (m_connect != null)
			{
				throw new ArgumentException(global::SR.GetString("The permission '{0}={1}' cannot be added. Add a separate Attribute statement.", "ConnectPatern", value), "value");
			}
			if (value == ".*")
			{
				m_connect = true;
			}
			else
			{
				m_connect = new DelayedRegex(value);
			}
		}
	}

	/// <summary>Gets or sets a regular expression pattern that describes the URI accepted by the current <see cref="T:System.Net.WebPermissionAttribute" />.</summary>
	/// <returns>A string containing a regular expression pattern that describes the URI accepted by the current <see cref="T:System.Net.WebPermissionAttribute" />. This string must be escaped according to the rules for encoding a <see cref="T:System.Text.RegularExpressions.Regex" /> constructor string.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Net.WebPermissionAttribute.AcceptPattern" /> is not null when you attempt to set the value. If you wish to specify more than one Accept URI, use an additional attribute declaration statement. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string AcceptPattern
	{
		get
		{
			if (!(m_accept is DelayedRegex))
			{
				if (!(m_accept is bool) || !(bool)m_accept)
				{
					return null;
				}
				return ".*";
			}
			return m_accept.ToString();
		}
		set
		{
			if (m_accept != null)
			{
				throw new ArgumentException(global::SR.GetString("The permission '{0}={1}' cannot be added. Add a separate Attribute statement.", "AcceptPattern", value), "value");
			}
			if (value == ".*")
			{
				m_accept = true;
			}
			else
			{
				m_accept = new DelayedRegex(value);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.WebPermissionAttribute" /> class with a value that specifies the security actions that can be performed on this class.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="action" /> is not a valid <see cref="T:System.Security.Permissions.SecurityAction" /> value. </exception>
	public WebPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new instance of the <see cref="T:System.Net.WebPermission" /> class.</summary>
	/// <returns>A <see cref="T:System.Net.WebPermission" /> corresponding to the security declaration.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override IPermission CreatePermission()
	{
		WebPermission webPermission = null;
		if (base.Unrestricted)
		{
			webPermission = new WebPermission(PermissionState.Unrestricted);
		}
		else
		{
			NetworkAccess networkAccess = (NetworkAccess)0;
			if (m_connect is bool)
			{
				if ((bool)m_connect)
				{
					networkAccess |= NetworkAccess.Connect;
				}
				m_connect = null;
			}
			if (m_accept is bool)
			{
				if ((bool)m_accept)
				{
					networkAccess |= NetworkAccess.Accept;
				}
				m_accept = null;
			}
			webPermission = new WebPermission(networkAccess);
			if (m_accept != null)
			{
				if (m_accept is DelayedRegex)
				{
					webPermission.AddAsPattern(NetworkAccess.Accept, (DelayedRegex)m_accept);
				}
				else
				{
					webPermission.AddPermission(NetworkAccess.Accept, (string)m_accept);
				}
			}
			if (m_connect != null)
			{
				if (m_connect is DelayedRegex)
				{
					webPermission.AddAsPattern(NetworkAccess.Connect, (DelayedRegex)m_connect);
				}
				else
				{
					webPermission.AddPermission(NetworkAccess.Connect, (string)m_connect);
				}
			}
		}
		return webPermission;
	}
}
