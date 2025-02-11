using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography;

/// <summary>Represents a cryptographic object identifier. This class cannot be inherited.</summary>
public sealed class Oid
{
	private string m_value;

	private string m_friendlyName;

	private OidGroup m_group;

	/// <summary>Gets or sets the dotted number of the identifier.</summary>
	/// <returns>The dotted number of the identifier.</returns>
	public string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	/// <summary>Gets or sets the friendly name of the identifier.</summary>
	/// <returns>The friendly name of the identifier.</returns>
	public string FriendlyName
	{
		get
		{
			if (m_friendlyName == null && m_value != null)
			{
				m_friendlyName = X509Utils.FindOidInfoWithFallback(1u, m_value, m_group);
			}
			return m_friendlyName;
		}
		set
		{
			m_friendlyName = value;
			if (m_friendlyName != null)
			{
				string text = X509Utils.FindOidInfoWithFallback(2u, m_friendlyName, m_group);
				if (text != null)
				{
					m_value = text;
				}
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Oid" /> class.</summary>
	public Oid()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Oid" /> class using a string value of an <see cref="T:System.Security.Cryptography.Oid" /> object.</summary>
	/// <param name="oid">An object identifier.</param>
	public Oid(string oid)
		: this(oid, OidGroup.All, lookupFriendlyName: true)
	{
	}

	internal Oid(string oid, OidGroup group, bool lookupFriendlyName)
	{
		if (lookupFriendlyName)
		{
			string text = X509Utils.FindOidInfoWithFallback(2u, oid, group);
			if (text == null)
			{
				text = oid;
			}
			Value = text;
		}
		else
		{
			Value = oid;
		}
		m_group = group;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Oid" /> class using the specified value and friendly name.</summary>
	/// <param name="value">The dotted number of the identifier.</param>
	/// <param name="friendlyName">The friendly name of the identifier.</param>
	public Oid(string value, string friendlyName)
	{
		m_value = value;
		m_friendlyName = friendlyName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Oid" /> class using the specified <see cref="T:System.Security.Cryptography.Oid" /> object.</summary>
	/// <param name="oid">The object identifier information to use to create the new object identifier.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="oid " />is null.</exception>
	public Oid(Oid oid)
	{
		if (oid == null)
		{
			throw new ArgumentNullException("oid");
		}
		m_value = oid.m_value;
		m_friendlyName = oid.m_friendlyName;
		m_group = oid.m_group;
	}

	private Oid(string value, string friendlyName, OidGroup group)
	{
		m_value = value;
		m_friendlyName = friendlyName;
		m_group = group;
	}

	/// <summary>Creates an <see cref="T:System.Security.Cryptography.Oid" /> object from an OID friendly name by searching the specified group.</summary>
	/// <returns>An object that represents the specified OID.</returns>
	/// <param name="friendlyName">The friendly name of the identifier.</param>
	/// <param name="group">The group to search in.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="friendlyName " /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The OID was not found.</exception>
	public static Oid FromFriendlyName(string friendlyName, OidGroup group)
	{
		if (friendlyName == null)
		{
			throw new ArgumentNullException("friendlyName");
		}
		return new Oid(X509Utils.FindOidInfo(2u, friendlyName, group) ?? throw new CryptographicException(global::SR.GetString("The OID value is invalid.")), friendlyName, group);
	}

	/// <summary>Creates an <see cref="T:System.Security.Cryptography.Oid" /> object by using the specified OID value and group.</summary>
	/// <returns>A new instance of an <see cref="T:System.Security.Cryptography.Oid" /> object.</returns>
	/// <param name="oidValue">The OID value.</param>
	/// <param name="group">The group to search in.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="oidValue" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The friendly name for the OID value was not found.</exception>
	public static Oid FromOidValue(string oidValue, OidGroup group)
	{
		if (oidValue == null)
		{
			throw new ArgumentNullException("oidValue");
		}
		string text = X509Utils.FindOidInfo(1u, oidValue, group);
		if (text == null)
		{
			throw new CryptographicException(global::SR.GetString("The OID value is invalid."));
		}
		return new Oid(oidValue, text, group);
	}
}
