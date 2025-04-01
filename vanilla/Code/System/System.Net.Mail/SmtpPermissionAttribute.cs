using System.Security;
using System.Security.Permissions;

namespace System.Net.Mail;

/// <summary>Controls access to Simple Mail Transport Protocol (SMTP) servers.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class SmtpPermissionAttribute : CodeAccessSecurityAttribute
{
	private string access;

	/// <summary>Gets or sets the level of access to SMTP servers controlled by the attribute.</summary>
	/// <returns>A <see cref="T:System.String" /> value. Valid values are "Connect" and "None".</returns>
	public string Access
	{
		get
		{
			return access;
		}
		set
		{
			access = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.SmtpPermissionAttribute" /> class. </summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values that specifies the permission behavior.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="action" /> is not a valid <see cref="T:System.Security.Permissions.SecurityAction" />.</exception>
	public SmtpPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	private SmtpAccess GetSmtpAccess()
	{
		if (access == null)
		{
			return SmtpAccess.None;
		}
		switch (access.ToLowerInvariant())
		{
		case "connecttounrestrictedport":
			return SmtpAccess.ConnectToUnrestrictedPort;
		case "connect":
			return SmtpAccess.Connect;
		case "none":
			return SmtpAccess.None;
		default:
		{
			string text = global::Locale.GetText("Invalid Access='{0}' value.", access);
			throw new ArgumentException("Access", text);
		}
		}
	}

	/// <summary>Creates a permission object that can be stored with the <see cref="T:System.Security.Permissions.SecurityAction" /> in an assembly's metadata.</summary>
	/// <returns>An <see cref="T:System.Net.Mail.SmtpPermission" /> instance.</returns>
	public override IPermission CreatePermission()
	{
		if (base.Unrestricted)
		{
			return new SmtpPermission(unrestricted: true);
		}
		return new SmtpPermission(GetSmtpAccess());
	}
}
