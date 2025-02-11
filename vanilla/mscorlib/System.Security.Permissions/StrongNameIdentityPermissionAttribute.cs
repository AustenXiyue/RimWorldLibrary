using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.StrongNameIdentityPermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
[ComVisible(true)]
public sealed class StrongNameIdentityPermissionAttribute : CodeAccessSecurityAttribute
{
	private string name;

	private string key;

	private string version;

	/// <summary>Gets or sets the name of the strong name identity.</summary>
	/// <returns>A name to compare against the name specified by the security provider.</returns>
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Gets or sets the public key value of the strong name identity expressed as a hexadecimal string.</summary>
	/// <returns>The public key value of the strong name identity expressed as a hexadecimal string.</returns>
	public string PublicKey
	{
		get
		{
			return key;
		}
		set
		{
			key = value;
		}
	}

	/// <summary>Gets or sets the version of the strong name identity.</summary>
	/// <returns>The version number of the strong name identity.</returns>
	public string Version
	{
		get
		{
			return version;
		}
		set
		{
			version = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.StrongNameIdentityPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public StrongNameIdentityPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.StrongNameIdentityPermission" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.StrongNameIdentityPermission" /> that corresponds to this attribute.</returns>
	/// <exception cref="T:System.ArgumentException">The method failed because the key is null.</exception>
	public override IPermission CreatePermission()
	{
		if (base.Unrestricted)
		{
			return new StrongNameIdentityPermission(PermissionState.Unrestricted);
		}
		if (name == null && key == null && this.version == null)
		{
			return new StrongNameIdentityPermission(PermissionState.None);
		}
		if (key == null)
		{
			throw new ArgumentException(Locale.GetText("PublicKey is required"));
		}
		StrongNamePublicKeyBlob blob = StrongNamePublicKeyBlob.FromString(key);
		Version version = null;
		if (this.version != null)
		{
			version = new Version(this.version);
		}
		return new StrongNameIdentityPermission(blob, name, version);
	}
}
