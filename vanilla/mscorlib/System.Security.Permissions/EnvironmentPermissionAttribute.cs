using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.EnvironmentPermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
[ComVisible(true)]
public sealed class EnvironmentPermissionAttribute : CodeAccessSecurityAttribute
{
	private string read;

	private string write;

	/// <summary>Sets full access for the environment variables specified by the string value.</summary>
	/// <returns>A list of environment variables for full access.</returns>
	/// <exception cref="T:System.NotSupportedException">The get method is not supported for this property.</exception>
	public string All
	{
		get
		{
			throw new NotSupportedException("All");
		}
		set
		{
			read = value;
			write = value;
		}
	}

	/// <summary>Gets or sets read access for the environment variables specified by the string value.</summary>
	/// <returns>A list of environment variables for read access.</returns>
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

	/// <summary>Gets or sets write access for the environment variables specified by the string value.</summary>
	/// <returns>A list of environment variables for write access.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.EnvironmentPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="action" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.SecurityAction" />. </exception>
	public EnvironmentPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.EnvironmentPermission" />.</summary>
	/// <returns>An <see cref="T:System.Security.Permissions.EnvironmentPermission" /> that corresponds to this attribute.</returns>
	public override IPermission CreatePermission()
	{
		EnvironmentPermission environmentPermission = null;
		if (base.Unrestricted)
		{
			environmentPermission = new EnvironmentPermission(PermissionState.Unrestricted);
		}
		else
		{
			environmentPermission = new EnvironmentPermission(PermissionState.None);
			if (read != null)
			{
				environmentPermission.AddPathList(EnvironmentPermissionAccess.Read, read);
			}
			if (write != null)
			{
				environmentPermission.AddPathList(EnvironmentPermissionAccess.Write, write);
			}
		}
		return environmentPermission;
	}
}
