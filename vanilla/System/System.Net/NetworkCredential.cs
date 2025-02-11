using System.Security;

namespace System.Net;

/// <summary>Provides credentials for password-based authentication schemes such as basic, digest, NTLM, and Kerberos authentication.</summary>
public class NetworkCredential : ICredentials, ICredentialsByHost
{
	private string m_domain;

	private string m_userName;

	private SecureString m_password;

	/// <summary>Gets or sets the user name associated with the credentials.</summary>
	/// <returns>The user name associated with the credentials.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string UserName
	{
		get
		{
			return InternalGetUserName();
		}
		set
		{
			if (value == null)
			{
				m_userName = string.Empty;
			}
			else
			{
				m_userName = value;
			}
		}
	}

	/// <summary>Gets or sets the password for the user name associated with the credentials.</summary>
	/// <returns>The password associated with the credentials. If this <see cref="T:System.Net.NetworkCredential" /> instance was initialized with the <paramref name="password" /> parameter set to null, then the <see cref="P:System.Net.NetworkCredential.Password" /> property will return an empty string.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string Password
	{
		get
		{
			return InternalGetPassword();
		}
		set
		{
			m_password = UnsafeNclNativeMethods.SecureStringHelper.CreateSecureString(value);
		}
	}

	/// <summary>Gets or sets the password as a <see cref="T:System.Security.SecureString" /> instance.</summary>
	/// <returns>The password for the user name associated with the credentials.</returns>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Security.SecureString" /> class is not supported on this platform.</exception>
	public SecureString SecurePassword
	{
		get
		{
			return InternalGetSecurePassword().Copy();
		}
		set
		{
			if (value == null)
			{
				m_password = new SecureString();
			}
			else
			{
				m_password = value.Copy();
			}
		}
	}

	/// <summary>Gets or sets the domain or computer name that verifies the credentials.</summary>
	/// <returns>The name of the domain associated with the credentials.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string Domain
	{
		get
		{
			return InternalGetDomain();
		}
		set
		{
			if (value == null)
			{
				m_domain = string.Empty;
			}
			else
			{
				m_domain = value;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkCredential" /> class.</summary>
	public NetworkCredential()
		: this(string.Empty, string.Empty, string.Empty)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkCredential" /> class with the specified user name and password.</summary>
	/// <param name="userName">The user name associated with the credentials. </param>
	/// <param name="password">The password for the user name associated with the credentials. </param>
	public NetworkCredential(string userName, string password)
		: this(userName, password, string.Empty)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkCredential" /> class with the specified user name and password.</summary>
	/// <param name="userName">The user name associated with the credentials.</param>
	/// <param name="password">The password for the user name associated with the credentials.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Security.SecureString" /> class is not supported on this platform.</exception>
	public NetworkCredential(string userName, SecureString password)
		: this(userName, password, string.Empty)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkCredential" /> class with the specified user name, password, and domain.</summary>
	/// <param name="userName">The user name associated with the credentials. </param>
	/// <param name="password">The password for the user name associated with the credentials. </param>
	/// <param name="domain">The domain associated with these credentials. </param>
	public NetworkCredential(string userName, string password, string domain)
	{
		UserName = userName;
		Password = password;
		Domain = domain;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.NetworkCredential" /> class with the specified user name, password, and domain.</summary>
	/// <param name="userName">The user name associated with the credentials.</param>
	/// <param name="password">The password for the user name associated with the credentials.</param>
	/// <param name="domain">The domain associated with these credentials.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Security.SecureString" /> class is not supported on this platform.</exception>
	public NetworkCredential(string userName, SecureString password, string domain)
	{
		UserName = userName;
		SecurePassword = password;
		Domain = domain;
	}

	internal string InternalGetUserName()
	{
		return m_userName;
	}

	internal string InternalGetPassword()
	{
		return UnsafeNclNativeMethods.SecureStringHelper.CreateString(m_password);
	}

	internal SecureString InternalGetSecurePassword()
	{
		return m_password;
	}

	internal string InternalGetDomain()
	{
		return m_domain;
	}

	internal string InternalGetDomainUserName()
	{
		string text = InternalGetDomain();
		if (text.Length != 0)
		{
			text += "\\";
		}
		return text + InternalGetUserName();
	}

	/// <summary>Returns an instance of the <see cref="T:System.Net.NetworkCredential" /> class for the specified Uniform Resource Identifier (URI) and authentication type.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkCredential" /> object.</returns>
	/// <param name="uri">The URI that the client provides authentication for. </param>
	/// <param name="authType">The type of authentication requested, as defined in the <see cref="P:System.Net.IAuthenticationModule.AuthenticationType" /> property. </param>
	public NetworkCredential GetCredential(Uri uri, string authType)
	{
		return this;
	}

	/// <summary>Returns an instance of the <see cref="T:System.Net.NetworkCredential" /> class for the specified host, port, and authentication type.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkCredential" /> for the specified host, port, and authentication protocol, or null if there are no credentials available for the specified host, port, and authentication protocol.</returns>
	/// <param name="host">The host computer that authenticates the client.</param>
	/// <param name="port">The port on the <paramref name="host" /> that the client communicates with.</param>
	/// <param name="authenticationType">The type of authentication requested, as defined in the <see cref="P:System.Net.IAuthenticationModule.AuthenticationType" /> property. </param>
	public NetworkCredential GetCredential(string host, int port, string authenticationType)
	{
		return this;
	}
}
