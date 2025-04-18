using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.IsolatedStoragePermission" /> to be applied to code using declarative security.</summary>
[Serializable]
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public abstract class IsolatedStoragePermissionAttribute : CodeAccessSecurityAttribute
{
	private IsolatedStorageContainment usage_allowed;

	private long user_quota;

	/// <summary>Gets or sets the level of isolated storage that should be declared.</summary>
	/// <returns>One of the <see cref="T:System.Security.Permissions.IsolatedStorageContainment" /> values.</returns>
	public IsolatedStorageContainment UsageAllowed
	{
		get
		{
			return usage_allowed;
		}
		set
		{
			usage_allowed = value;
		}
	}

	/// <summary>Gets or sets the maximum user storage quota size.</summary>
	/// <returns>The maximum user storage quota size in bytes.</returns>
	public long UserQuota
	{
		get
		{
			return user_quota;
		}
		set
		{
			user_quota = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.IsolatedStoragePermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	protected IsolatedStoragePermissionAttribute(SecurityAction action)
		: base(action)
	{
	}
}
