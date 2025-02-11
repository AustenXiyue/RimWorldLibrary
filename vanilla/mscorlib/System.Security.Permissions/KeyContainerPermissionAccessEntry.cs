using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace System.Security.Permissions;

/// <summary>Specifies access rights for specific key containers. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class KeyContainerPermissionAccessEntry
{
	private KeyContainerPermissionFlags _flags;

	private string _containerName;

	private int _spec;

	private string _store;

	private string _providerName;

	private int _type;

	/// <summary>Gets or sets the key container permissions.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Security.Permissions.KeyContainerPermissionFlags" /> values. The default is <see cref="F:System.Security.Permissions.KeyContainerPermissionFlags.NoFlags" />.</returns>
	public KeyContainerPermissionFlags Flags
	{
		get
		{
			return _flags;
		}
		set
		{
			if ((value & KeyContainerPermissionFlags.AllFlags) != 0)
			{
				throw new ArgumentException(string.Format(Locale.GetText("Invalid enum {0}"), value), "KeyContainerPermissionFlags");
			}
			_flags = value;
		}
	}

	/// <summary>Gets or sets the key container name.</summary>
	/// <returns>The name of the key container.</returns>
	/// <exception cref="T:System.ArgumentException">The resulting entry would have unrestricted access. </exception>
	public string KeyContainerName
	{
		get
		{
			return _containerName;
		}
		set
		{
			_containerName = value;
		}
	}

	/// <summary>Gets or sets the key specification.</summary>
	/// <returns>One of the AT_ values defined in the Wincrypt.h header file.</returns>
	/// <exception cref="T:System.ArgumentException">The resulting entry would have unrestricted access. </exception>
	public int KeySpec
	{
		get
		{
			return _spec;
		}
		set
		{
			_spec = value;
		}
	}

	/// <summary>Gets or sets the name of the key store.</summary>
	/// <returns>The name of the key store.</returns>
	/// <exception cref="T:System.ArgumentException">The resulting entry would have unrestricted access. </exception>
	public string KeyStore
	{
		get
		{
			return _store;
		}
		set
		{
			_store = value;
		}
	}

	/// <summary>Gets or sets the provider name.</summary>
	/// <returns>The name of the provider.</returns>
	/// <exception cref="T:System.ArgumentException">The resulting entry would have unrestricted access. </exception>
	public string ProviderName
	{
		get
		{
			return _providerName;
		}
		set
		{
			_providerName = value;
		}
	}

	/// <summary>Gets or sets the provider type.</summary>
	/// <returns>One of the PROV_ values defined in the Wincrypt.h header file.</returns>
	/// <exception cref="T:System.ArgumentException">The resulting entry would have unrestricted access. </exception>
	public int ProviderType
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> class, using the specified cryptographic service provider (CSP) parameters and access permissions.</summary>
	/// <param name="parameters">A <see cref="T:System.Security.Cryptography.CspParameters" /> object that contains the cryptographic service provider (CSP) parameters. </param>
	/// <param name="flags">A bitwise combination of the <see cref="T:System.Security.Permissions.KeyContainerPermissionFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The resulting entry would have unrestricted access. </exception>
	public KeyContainerPermissionAccessEntry(CspParameters parameters, KeyContainerPermissionFlags flags)
	{
		if (parameters == null)
		{
			throw new ArgumentNullException("parameters");
		}
		ProviderName = parameters.ProviderName;
		ProviderType = parameters.ProviderType;
		KeyContainerName = parameters.KeyContainerName;
		KeySpec = parameters.KeyNumber;
		Flags = flags;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> class, using the specified key container name and access permissions.</summary>
	/// <param name="keyContainerName">The name of the key container. </param>
	/// <param name="flags">A bitwise combination of the <see cref="T:System.Security.Permissions.KeyContainerPermissionFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The resulting entry would have unrestricted access. </exception>
	public KeyContainerPermissionAccessEntry(string keyContainerName, KeyContainerPermissionFlags flags)
	{
		KeyContainerName = keyContainerName;
		Flags = flags;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> class with the specified property values.</summary>
	/// <param name="keyStore">The name of the key store. </param>
	/// <param name="providerName">The name of the provider. </param>
	/// <param name="providerType">The type code for the provider. See the <see cref="P:System.Security.Permissions.KeyContainerPermissionAccessEntry.ProviderType" /> property for values. </param>
	/// <param name="keyContainerName">The name of the key container. </param>
	/// <param name="keySpec">The key specification. See the <see cref="P:System.Security.Permissions.KeyContainerPermissionAccessEntry.KeySpec" /> property for values. </param>
	/// <param name="flags">A bitwise combination of the <see cref="T:System.Security.Permissions.KeyContainerPermissionFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The resulting entry would have unrestricted access. </exception>
	public KeyContainerPermissionAccessEntry(string keyStore, string providerName, int providerType, string keyContainerName, int keySpec, KeyContainerPermissionFlags flags)
	{
		KeyStore = keyStore;
		ProviderName = providerName;
		ProviderType = providerType;
		KeyContainerName = keyContainerName;
		KeySpec = keySpec;
		Flags = flags;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> object is equal to the current instance.</summary>
	/// <returns>true if the specified <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> is equal to the current <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> object; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> object to compare with the currentinstance. </param>
	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		if (!(o is KeyContainerPermissionAccessEntry keyContainerPermissionAccessEntry))
		{
			return false;
		}
		if (_flags != keyContainerPermissionAccessEntry._flags)
		{
			return false;
		}
		if (_containerName != keyContainerPermissionAccessEntry._containerName)
		{
			return false;
		}
		if (_store != keyContainerPermissionAccessEntry._store)
		{
			return false;
		}
		if (_providerName != keyContainerPermissionAccessEntry._providerName)
		{
			return false;
		}
		if (_type != keyContainerPermissionAccessEntry._type)
		{
			return false;
		}
		return true;
	}

	/// <summary>Gets a hash code for the current instance that is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> object.</returns>
	public override int GetHashCode()
	{
		int num = _type ^ _spec ^ (int)_flags;
		if (_containerName != null)
		{
			num ^= _containerName.GetHashCode();
		}
		if (_store != null)
		{
			num ^= _store.GetHashCode();
		}
		if (_providerName != null)
		{
			num ^= _providerName.GetHashCode();
		}
		return num;
	}
}
