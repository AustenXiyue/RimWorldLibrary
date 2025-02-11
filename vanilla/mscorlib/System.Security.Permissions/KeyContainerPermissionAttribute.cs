using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.KeyContainerPermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class KeyContainerPermissionAttribute : CodeAccessSecurityAttribute
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
			_flags = value;
		}
	}

	/// <summary>Gets or sets the name of the key container.</summary>
	/// <returns>The name of the key container.</returns>
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
	/// <returns>One of the AT_ values defined in the Wincrypt.h header file. </returns>
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
	/// <returns>The name of the key store. The default is "*".</returns>
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
	/// <returns>One of the PROV_ values defined in the Wincrypt.h header file. </returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.KeyContainerPermissionAttribute" /> class with the specified security action.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public KeyContainerPermissionAttribute(SecurityAction action)
		: base(action)
	{
		_spec = -1;
		_type = -1;
	}

	/// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.KeyContainerPermission" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.KeyContainerPermission" /> that corresponds to the attribute.</returns>
	public override IPermission CreatePermission()
	{
		if (base.Unrestricted)
		{
			return new KeyContainerPermission(PermissionState.Unrestricted);
		}
		if (EmptyEntry())
		{
			return new KeyContainerPermission(_flags);
		}
		return new KeyContainerPermission(accessList: new KeyContainerPermissionAccessEntry[1]
		{
			new KeyContainerPermissionAccessEntry(_store, _providerName, _type, _containerName, _spec, _flags)
		}, flags: _flags);
	}

	private bool EmptyEntry()
	{
		if (_containerName != null)
		{
			return false;
		}
		if (_spec != 0)
		{
			return false;
		}
		if (_store != null)
		{
			return false;
		}
		if (_providerName != null)
		{
			return false;
		}
		if (_type != 0)
		{
			return false;
		}
		return true;
	}
}
