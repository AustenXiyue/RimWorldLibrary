using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Allows security actions for <see cref="T:System.Security.Permissions.UrlIdentityPermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
[ComVisible(true)]
public sealed class UrlIdentityPermissionAttribute : CodeAccessSecurityAttribute
{
	private string url;

	/// <summary>Gets or sets the full URL of the calling code.</summary>
	/// <returns>The URL to match with the URL specified by the host.</returns>
	public string Url
	{
		get
		{
			return url;
		}
		set
		{
			url = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.UrlIdentityPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public UrlIdentityPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates and returns a new <see cref="T:System.Security.Permissions.UrlIdentityPermission" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.UrlIdentityPermission" /> that corresponds to this attribute.</returns>
	public override IPermission CreatePermission()
	{
		if (base.Unrestricted)
		{
			return new UrlIdentityPermission(PermissionState.Unrestricted);
		}
		if (url == null)
		{
			return new UrlIdentityPermission(PermissionState.None);
		}
		return new UrlIdentityPermission(url);
	}
}
