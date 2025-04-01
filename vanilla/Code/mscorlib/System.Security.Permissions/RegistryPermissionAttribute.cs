using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.RegistryPermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
[ComVisible(true)]
public sealed class RegistryPermissionAttribute : CodeAccessSecurityAttribute
{
	private string create;

	private string read;

	private string write;

	private string changeAccessControl;

	private string viewAccessControl;

	/// <summary>Gets or sets full access for the specified registry keys.</summary>
	/// <returns>A semicolon-separated list of registry key paths, for full access. </returns>
	/// <exception cref="T:System.NotSupportedException">The get accessor is called; it is only provided for C# compiler compatibility.</exception>
	[Obsolete("use newer properties")]
	public string All
	{
		get
		{
			throw new NotSupportedException("All");
		}
		set
		{
			create = value;
			read = value;
			write = value;
		}
	}

	/// <summary>Gets or sets create-level access for the specified registry keys. </summary>
	/// <returns>A semicolon-separated list of registry key paths, for create-level access. </returns>
	public string Create
	{
		get
		{
			return create;
		}
		set
		{
			create = value;
		}
	}

	/// <summary>Gets or sets read access for the specified registry keys.</summary>
	/// <returns>A semicolon-separated list of registry key paths, for read access. </returns>
	public string Read
	{
		get
		{
			return read;
		}
		set
		{
			read = value;
		}
	}

	/// <summary>Gets or sets write access for the specified registry keys.</summary>
	/// <returns>A semicolon-separated list of registry key paths, for write access. </returns>
	public string Write
	{
		get
		{
			return write;
		}
		set
		{
			write = value;
		}
	}

	/// <summary>Gets or sets change access control for the specified registry keys.</summary>
	/// <returns>A semicolon-separated list of registry key paths, for change access control. .</returns>
	public string ChangeAccessControl
	{
		get
		{
			return changeAccessControl;
		}
		set
		{
			changeAccessControl = value;
		}
	}

	/// <summary>Gets or sets view access control for the specified registry keys.</summary>
	/// <returns>A semicolon-separated list of registry key paths, for view access control.</returns>
	public string ViewAccessControl
	{
		get
		{
			return viewAccessControl;
		}
		set
		{
			viewAccessControl = value;
		}
	}

	/// <summary>Gets or sets a specified set of registry keys that can be viewed and modified.</summary>
	/// <returns>A semicolon-separated list of registry key paths, for create, read, and write access. </returns>
	/// <exception cref="T:System.NotSupportedException">The get accessor is called; it is only provided for C# compiler compatibility. </exception>
	public string ViewAndModify
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			create = value;
			read = value;
			write = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.RegistryPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="action" /> parameter is not a valid <see cref="T:System.Security.Permissions.SecurityAction" />. </exception>
	public RegistryPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.RegistryPermission" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.RegistryPermission" /> that corresponds to this attribute.</returns>
	public override IPermission CreatePermission()
	{
		RegistryPermission registryPermission = null;
		if (base.Unrestricted)
		{
			registryPermission = new RegistryPermission(PermissionState.Unrestricted);
		}
		else
		{
			registryPermission = new RegistryPermission(PermissionState.None);
			if (create != null)
			{
				registryPermission.AddPathList(RegistryPermissionAccess.Create, create);
			}
			if (read != null)
			{
				registryPermission.AddPathList(RegistryPermissionAccess.Read, read);
			}
			if (write != null)
			{
				registryPermission.AddPathList(RegistryPermissionAccess.Write, write);
			}
		}
		return registryPermission;
	}
}
