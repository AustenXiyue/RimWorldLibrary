namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.DataProtectionPermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class DataProtectionPermissionAttribute : CodeAccessSecurityAttribute
{
	private DataProtectionPermissionFlags _flags;

	/// <summary>Gets or sets the data protection permissions.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Security.Permissions.DataProtectionPermissionFlags" /> values. The default is <see cref="F:System.Security.Permissions.DataProtectionPermissionFlags.NoFlags" />.</returns>
	public DataProtectionPermissionFlags Flags
	{
		get
		{
			return _flags;
		}
		set
		{
			if ((value & DataProtectionPermissionFlags.AllFlags) != value)
			{
				throw new ArgumentException(string.Format(global::Locale.GetText("Invalid flags {0}"), value), "DataProtectionPermissionFlags");
			}
			_flags = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether data can be encrypted using the <see cref="T:System.Security.Cryptography.ProtectedData" /> class.</summary>
	/// <returns>true if data can be encrypted; otherwise, false.  </returns>
	public bool ProtectData
	{
		get
		{
			return (_flags & DataProtectionPermissionFlags.ProtectData) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= DataProtectionPermissionFlags.ProtectData;
			}
			else
			{
				_flags &= ~DataProtectionPermissionFlags.ProtectData;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether data can be unencrypted using the <see cref="T:System.Security.Cryptography.ProtectedData" /> class.</summary>
	/// <returns>true if data can be unencrypted; otherwise, false.  </returns>
	public bool UnprotectData
	{
		get
		{
			return (_flags & DataProtectionPermissionFlags.UnprotectData) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= DataProtectionPermissionFlags.UnprotectData;
			}
			else
			{
				_flags &= ~DataProtectionPermissionFlags.UnprotectData;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether memory can be encrypted using the <see cref="T:System.Security.Cryptography.ProtectedMemory" /> class.</summary>
	/// <returns>true if memory can be encrypted; otherwise, false.  </returns>
	public bool ProtectMemory
	{
		get
		{
			return (_flags & DataProtectionPermissionFlags.ProtectMemory) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= DataProtectionPermissionFlags.ProtectMemory;
			}
			else
			{
				_flags &= ~DataProtectionPermissionFlags.ProtectMemory;
			}
		}
	}

	/// <summary>Gets or sets a value indicating whether memory can be unencrypted using the <see cref="T:System.Security.Cryptography.ProtectedMemory" /> class.</summary>
	/// <returns>true if memory can be unencrypted; otherwise, false.  </returns>
	public bool UnprotectMemory
	{
		get
		{
			return (_flags & DataProtectionPermissionFlags.UnprotectMemory) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= DataProtectionPermissionFlags.UnprotectMemory;
			}
			else
			{
				_flags &= ~DataProtectionPermissionFlags.UnprotectMemory;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.DataProtectionPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public DataProtectionPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.DataProtectionPermission" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.DataProtectionPermission" /> that corresponds to the attribute.</returns>
	public override IPermission CreatePermission()
	{
		DataProtectionPermission dataProtectionPermission = null;
		if (base.Unrestricted)
		{
			return new DataProtectionPermission(PermissionState.Unrestricted);
		}
		return new DataProtectionPermission(_flags);
	}
}
