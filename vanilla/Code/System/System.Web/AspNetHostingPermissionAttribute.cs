using System.Security;
using System.Security.Permissions;

namespace System.Web;

/// <summary>Allows security actions for <see cref="T:System.Web.AspNetHostingPermission" /> to be applied to code using declarative security. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
public sealed class AspNetHostingPermissionAttribute : CodeAccessSecurityAttribute
{
	private AspNetHostingPermissionLevel _level;

	/// <summary>Gets or sets the current hosting permission level.</summary>
	/// <returns>One of the <see cref="T:System.Web.AspNetHostingPermissionLevel" /> enumeration values.</returns>
	public AspNetHostingPermissionLevel Level
	{
		get
		{
			return _level;
		}
		set
		{
			if (value < AspNetHostingPermissionLevel.None || value > AspNetHostingPermissionLevel.Unrestricted)
			{
				throw new ArgumentException(string.Format(global::Locale.GetText("Invalid enum {0}."), value), "Level");
			}
			_level = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Web.AspNetHostingPermissionAttribute" /> class.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> enumeration values. </param>
	public AspNetHostingPermissionAttribute(SecurityAction action)
		: base(action)
	{
		_level = AspNetHostingPermissionLevel.None;
	}

	/// <summary>Creates a new <see cref="T:System.Web.AspNetHostingPermission" /> with the permission level previously set by the <see cref="P:System.Web.AspNetHostingPermissionAttribute.Level" /> property.</summary>
	/// <returns>An <see cref="T:System.Security.IPermission" /> that is the new <see cref="T:System.Web.AspNetHostingPermission" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override IPermission CreatePermission()
	{
		if (base.Unrestricted)
		{
			return new AspNetHostingPermission(PermissionState.Unrestricted);
		}
		return new AspNetHostingPermission(_level);
	}
}
