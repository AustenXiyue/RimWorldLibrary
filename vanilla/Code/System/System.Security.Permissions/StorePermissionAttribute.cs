namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.StorePermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class StorePermissionAttribute : CodeAccessSecurityAttribute
{
	private StorePermissionFlags _flags;

	/// <summary>Gets or sets the store permissions.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Security.Permissions.StorePermissionFlags" /> values. The default is <see cref="F:System.Security.Permissions.StorePermissionFlags.NoFlags" />.</returns>
	public StorePermissionFlags Flags
	{
		get
		{
			return _flags;
		}
		set
		{
			if ((value & StorePermissionFlags.AllFlags) != value)
			{
				throw new ArgumentException(string.Format(global::Locale.GetText("Invalid flags {0}"), value), "StorePermissionFlags");
			}
			_flags = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the code is permitted to add to a store.</summary>
	/// <returns>true if the ability to add to a store is allowed; otherwise, false.</returns>
	public bool AddToStore
	{
		get
		{
			return (_flags & StorePermissionFlags.AddToStore) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= StorePermissionFlags.AddToStore;
			}
			else
			{
				_flags &= ~StorePermissionFlags.AddToStore;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the code is permitted to create a store.</summary>
	/// <returns>true if the ability to create a store is allowed; otherwise, false.</returns>
	public bool CreateStore
	{
		get
		{
			return (_flags & StorePermissionFlags.CreateStore) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= StorePermissionFlags.CreateStore;
			}
			else
			{
				_flags &= ~StorePermissionFlags.CreateStore;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the code is permitted to delete a store.</summary>
	/// <returns>true if the ability to delete a store is allowed; otherwise, false.</returns>
	public bool DeleteStore
	{
		get
		{
			return (_flags & StorePermissionFlags.DeleteStore) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= StorePermissionFlags.DeleteStore;
			}
			else
			{
				_flags &= ~StorePermissionFlags.DeleteStore;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the code is permitted to enumerate the certificates in a store.</summary>
	/// <returns>true if the ability to enumerate certificates is allowed; otherwise, false.</returns>
	public bool EnumerateCertificates
	{
		get
		{
			return (_flags & StorePermissionFlags.EnumerateCertificates) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= StorePermissionFlags.EnumerateCertificates;
			}
			else
			{
				_flags &= ~StorePermissionFlags.EnumerateCertificates;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the code is permitted to enumerate stores.</summary>
	/// <returns>true if the ability to enumerate stores is allowed; otherwise, false.</returns>
	public bool EnumerateStores
	{
		get
		{
			return (_flags & StorePermissionFlags.EnumerateStores) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= StorePermissionFlags.EnumerateStores;
			}
			else
			{
				_flags &= ~StorePermissionFlags.EnumerateStores;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the code is permitted to open a store.</summary>
	/// <returns>true if the ability to open a store is allowed; otherwise, false.</returns>
	public bool OpenStore
	{
		get
		{
			return (_flags & StorePermissionFlags.OpenStore) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= StorePermissionFlags.OpenStore;
			}
			else
			{
				_flags &= ~StorePermissionFlags.OpenStore;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether the code is permitted to remove a certificate from a store.</summary>
	/// <returns>true if the ability to remove a certificate from a store is allowed; otherwise, false.</returns>
	public bool RemoveFromStore
	{
		get
		{
			return (_flags & StorePermissionFlags.RemoveFromStore) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= StorePermissionFlags.RemoveFromStore;
			}
			else
			{
				_flags &= ~StorePermissionFlags.RemoveFromStore;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.StorePermissionAttribute" /> class with the specified security action.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public StorePermissionAttribute(SecurityAction action)
		: base(action)
	{
		_flags = StorePermissionFlags.NoFlags;
	}

	/// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.StorePermission" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.StorePermission" /> that corresponds to the attribute.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override IPermission CreatePermission()
	{
		StorePermission storePermission = null;
		if (base.Unrestricted)
		{
			return new StorePermission(PermissionState.Unrestricted);
		}
		return new StorePermission(_flags);
	}
}
