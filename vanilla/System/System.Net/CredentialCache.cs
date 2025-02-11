using System.Collections;

namespace System.Net;

/// <summary>Provides storage for multiple credentials.</summary>
public class CredentialCache : ICredentials, ICredentialsByHost, IEnumerable
{
	private class CredentialEnumerator : IEnumerator
	{
		private CredentialCache m_cache;

		private ICredentials[] m_array;

		private int m_index = -1;

		private int m_version;

		object IEnumerator.Current
		{
			get
			{
				if (m_index < 0 || m_index >= m_array.Length)
				{
					throw new InvalidOperationException(global::SR.GetString("Enumeration has either not started or has already finished."));
				}
				if (m_version != m_cache.m_version)
				{
					throw new InvalidOperationException(global::SR.GetString("Collection was modified; enumeration operation may not execute."));
				}
				return m_array[m_index];
			}
		}

		internal CredentialEnumerator(CredentialCache cache, Hashtable table, Hashtable hostTable, int version)
		{
			m_cache = cache;
			m_array = new ICredentials[table.Count + hostTable.Count];
			table.Values.CopyTo(m_array, 0);
			hostTable.Values.CopyTo(m_array, table.Count);
			m_version = version;
		}

		bool IEnumerator.MoveNext()
		{
			if (m_version != m_cache.m_version)
			{
				throw new InvalidOperationException(global::SR.GetString("Collection was modified; enumeration operation may not execute."));
			}
			if (++m_index < m_array.Length)
			{
				return true;
			}
			m_index = m_array.Length;
			return false;
		}

		void IEnumerator.Reset()
		{
			m_index = -1;
		}
	}

	private Hashtable cache = new Hashtable();

	private Hashtable cacheForHosts = new Hashtable();

	internal int m_version;

	private int m_NumbDefaultCredInCache;

	internal bool IsDefaultInCache => m_NumbDefaultCredInCache != 0;

	/// <summary>Gets the system credentials of the application.</summary>
	/// <returns>An <see cref="T:System.Net.ICredentials" /> that represents the system credentials of the application.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="USERNAME" />
	/// </PermissionSet>
	public static ICredentials DefaultCredentials => SystemNetworkCredential.defaultCredential;

	/// <summary>Gets the network credentials of the current security context.</summary>
	/// <returns>An <see cref="T:System.Net.NetworkCredential" /> that represents the network credentials of the current user or application.</returns>
	public static NetworkCredential DefaultNetworkCredentials => SystemNetworkCredential.defaultCredential;

	/// <summary>Creates a new instance of the <see cref="T:System.Net.CredentialCache" /> class.</summary>
	public CredentialCache()
	{
	}

	/// <summary>Adds a <see cref="T:System.Net.NetworkCredential" /> instance to the credential cache for use with protocols other than SMTP and associates it with a Uniform Resource Identifier (URI) prefix and authentication protocol. </summary>
	/// <param name="uriPrefix">A <see cref="T:System.Uri" /> that specifies the URI prefix of the resources that the credential grants access to. </param>
	/// <param name="authType">The authentication scheme used by the resource named in <paramref name="uriPrefix" />. </param>
	/// <param name="cred">The <see cref="T:System.Net.NetworkCredential" /> to add to the credential cache. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="uriPrefix" /> is null. -or- <paramref name="authType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The same credentials are added more than once. </exception>
	public void Add(Uri uriPrefix, string authType, NetworkCredential cred)
	{
		if (uriPrefix == null)
		{
			throw new ArgumentNullException("uriPrefix");
		}
		if (authType == null)
		{
			throw new ArgumentNullException("authType");
		}
		if (cred is SystemNetworkCredential)
		{
			throw new ArgumentException(global::SR.GetString("Default credentials cannot be supplied for the {0} authentication scheme.", authType), "authType");
		}
		m_version++;
		CredentialKey key = new CredentialKey(uriPrefix, authType);
		cache.Add(key, cred);
		if (cred is SystemNetworkCredential)
		{
			m_NumbDefaultCredInCache++;
		}
	}

	/// <summary>Adds a <see cref="T:System.Net.NetworkCredential" /> instance for use with SMTP to the credential cache and associates it with a host computer, port, and authentication protocol. Credentials added using this method are valid for SMTP only. This method does not work for HTTP or FTP requests.</summary>
	/// <param name="host">A <see cref="T:System.String" /> that identifies the host computer.</param>
	/// <param name="port">A <see cref="T:System.Int32" /> that specifies the port to connect to on <paramref name="host" />.</param>
	/// <param name="authenticationType">A <see cref="T:System.String" /> that identifies the authentication scheme used when connecting to <paramref name="host" /> using <paramref name="cred" />. See Remarks.</param>
	/// <param name="credential">The <see cref="T:System.Net.NetworkCredential" /> to add to the credential cache. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="host" /> is null. -or-<paramref name="authType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="authType" /> not an accepted value. See Remarks. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is less than zero.</exception>
	public void Add(string host, int port, string authenticationType, NetworkCredential credential)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (authenticationType == null)
		{
			throw new ArgumentNullException("authenticationType");
		}
		if (host.Length == 0)
		{
			throw new ArgumentException(global::SR.GetString("The parameter '{0}' cannot be an empty string.", "host"));
		}
		if (port < 0)
		{
			throw new ArgumentOutOfRangeException("port");
		}
		if (credential is SystemNetworkCredential)
		{
			throw new ArgumentException(global::SR.GetString("Default credentials cannot be supplied for the {0} authentication scheme.", authenticationType), "authenticationType");
		}
		m_version++;
		CredentialHostKey key = new CredentialHostKey(host, port, authenticationType);
		cacheForHosts.Add(key, credential);
		if (credential is SystemNetworkCredential)
		{
			m_NumbDefaultCredInCache++;
		}
	}

	/// <summary>Deletes a <see cref="T:System.Net.NetworkCredential" /> instance from the cache if it is associated with the specified Uniform Resource Identifier (URI) prefix and authentication protocol.</summary>
	/// <param name="uriPrefix">A <see cref="T:System.Uri" /> that specifies the URI prefix of the resources that the credential is used for. </param>
	/// <param name="authType">The authentication scheme used by the host named in <paramref name="uriPrefix" />. </param>
	public void Remove(Uri uriPrefix, string authType)
	{
		if (!(uriPrefix == null) && authType != null)
		{
			m_version++;
			CredentialKey key = new CredentialKey(uriPrefix, authType);
			if (cache[key] is SystemNetworkCredential)
			{
				m_NumbDefaultCredInCache--;
			}
			cache.Remove(key);
		}
	}

	/// <summary>Deletes a <see cref="T:System.Net.NetworkCredential" /> instance from the cache if it is associated with the specified host, port, and authentication protocol.</summary>
	/// <param name="host">A <see cref="T:System.String" /> that identifies the host computer.</param>
	/// <param name="port">A <see cref="T:System.Int32" /> that specifies the port to connect to on <paramref name="host" />.</param>
	/// <param name="authenticationType">A <see cref="T:System.String" /> that identifies the authentication scheme used when connecting to <paramref name="host" />. See Remarks.</param>
	public void Remove(string host, int port, string authenticationType)
	{
		if (host != null && authenticationType != null && port >= 0)
		{
			m_version++;
			CredentialHostKey key = new CredentialHostKey(host, port, authenticationType);
			if (cacheForHosts[key] is SystemNetworkCredential)
			{
				m_NumbDefaultCredInCache--;
			}
			cacheForHosts.Remove(key);
		}
	}

	/// <summary>Returns the <see cref="T:System.Net.NetworkCredential" /> instance associated with the specified Uniform Resource Identifier (URI) and authentication type.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkCredential" /> or, if there is no matching credential in the cache, null.</returns>
	/// <param name="uriPrefix">A <see cref="T:System.Uri" /> that specifies the URI prefix of the resources that the credential grants access to. </param>
	/// <param name="authType">The authentication scheme used by the resource named in <paramref name="uriPrefix" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="uriPrefix" /> or <paramref name="authType" /> is null. </exception>
	public NetworkCredential GetCredential(Uri uriPrefix, string authType)
	{
		if (uriPrefix == null)
		{
			throw new ArgumentNullException("uriPrefix");
		}
		if (authType == null)
		{
			throw new ArgumentNullException("authType");
		}
		int num = -1;
		NetworkCredential result = null;
		IDictionaryEnumerator enumerator = cache.GetEnumerator();
		while (enumerator.MoveNext())
		{
			CredentialKey credentialKey = (CredentialKey)enumerator.Key;
			if (credentialKey.Match(uriPrefix, authType))
			{
				int uriPrefixLength = credentialKey.UriPrefixLength;
				if (uriPrefixLength > num)
				{
					num = uriPrefixLength;
					result = (NetworkCredential)enumerator.Value;
				}
			}
		}
		return result;
	}

	/// <summary>Returns the <see cref="T:System.Net.NetworkCredential" /> instance associated with the specified host, port, and authentication protocol.</summary>
	/// <returns>A <see cref="T:System.Net.NetworkCredential" /> or, if there is no matching credential in the cache, null.</returns>
	/// <param name="host">A <see cref="T:System.String" /> that identifies the host computer.</param>
	/// <param name="port">A <see cref="T:System.Int32" /> that specifies the port to connect to on <paramref name="host" />.</param>
	/// <param name="authenticationType">A <see cref="T:System.String" /> that identifies the authentication scheme used when connecting to <paramref name="host" />. See Remarks.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="host" /> is null. -or- <paramref name="authType" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="authType" /> not an accepted value. See Remarks. -or-<paramref name="host" /> is equal to the empty string ("").</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is less than zero.</exception>
	public NetworkCredential GetCredential(string host, int port, string authenticationType)
	{
		if (host == null)
		{
			throw new ArgumentNullException("host");
		}
		if (authenticationType == null)
		{
			throw new ArgumentNullException("authenticationType");
		}
		if (host.Length == 0)
		{
			throw new ArgumentException(global::SR.GetString("The parameter '{0}' cannot be an empty string.", "host"));
		}
		if (port < 0)
		{
			throw new ArgumentOutOfRangeException("port");
		}
		NetworkCredential result = null;
		IDictionaryEnumerator enumerator = cacheForHosts.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (((CredentialHostKey)enumerator.Key).Match(host, port, authenticationType))
			{
				result = (NetworkCredential)enumerator.Value;
			}
		}
		return result;
	}

	/// <summary>Returns an enumerator that can iterate through the <see cref="T:System.Net.CredentialCache" /> instance.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Net.CredentialCache" />.</returns>
	public IEnumerator GetEnumerator()
	{
		return new CredentialEnumerator(this, cache, cacheForHosts, m_version);
	}
}
