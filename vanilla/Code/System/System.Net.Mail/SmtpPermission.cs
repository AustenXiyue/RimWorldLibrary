using System.Security;
using System.Security.Permissions;

namespace System.Net.Mail;

/// <summary>Controls access to Simple Mail Transport Protocol (SMTP) servers.</summary>
[Serializable]
public sealed class SmtpPermission : CodeAccessPermission, IUnrestrictedPermission
{
	private const int version = 1;

	private bool unrestricted;

	private SmtpAccess access;

	/// <summary>Gets the level of access to SMTP servers controlled by the permission.</summary>
	/// <returns>One of the <see cref="T:System.Net.Mail.SmtpAccess" /> values. </returns>
	public SmtpAccess Access => access;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.SmtpPermission" /> class with the specified state.</summary>
	/// <param name="unrestricted">true if the new permission is unrestricted; otherwise, false.</param>
	public SmtpPermission(bool unrestricted)
	{
		this.unrestricted = unrestricted;
		access = (unrestricted ? SmtpAccess.ConnectToUnrestrictedPort : SmtpAccess.None);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.SmtpPermission" /> class using the specified permission state value.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	public SmtpPermission(PermissionState state)
	{
		unrestricted = state == PermissionState.Unrestricted;
		access = (unrestricted ? SmtpAccess.ConnectToUnrestrictedPort : SmtpAccess.None);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.SmtpPermission" /> class using the specified access level.</summary>
	/// <param name="access">One of the <see cref="T:System.Net.Mail.SmtpAccess" /> values.</param>
	public SmtpPermission(SmtpAccess access)
	{
		this.access = access;
	}

	/// <summary>Adds the specified access level value to the permission. </summary>
	/// <param name="access">One of the <see cref="T:System.Net.Mail.SmtpAccess" /> values.</param>
	public void AddPermission(SmtpAccess access)
	{
		if (!unrestricted && access > this.access)
		{
			this.access = access;
		}
	}

	/// <summary>Creates and returns an identical copy of the current permission. </summary>
	/// <returns>An <see cref="T:System.Net.Mail.SmtpPermission" /> that is identical to the current permission.</returns>
	public override IPermission Copy()
	{
		if (unrestricted)
		{
			return new SmtpPermission(unrestricted: true);
		}
		return new SmtpPermission(access);
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>An <see cref="T:System.Net.Mail.SmtpPermission" /> that represents the intersection of the current permission and the specified permission. Returns null if the intersection is empty or <paramref name="target" /> is null.</returns>
	/// <param name="target">An <see cref="T:System.Security.IPermission" /> to intersect with the current permission. It must be of the same type as the current permission.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not an <see cref="T:System.Net.Mail.SmtpPermission" />.</exception>
	public override IPermission Intersect(IPermission target)
	{
		SmtpPermission smtpPermission = Cast(target);
		if (smtpPermission == null)
		{
			return null;
		}
		if (unrestricted && smtpPermission.unrestricted)
		{
			return new SmtpPermission(unrestricted: true);
		}
		if (access > smtpPermission.access)
		{
			return new SmtpPermission(smtpPermission.access);
		}
		return new SmtpPermission(access);
	}

	/// <summary>Returns a value indicating whether the current permission is a subset of the specified permission. </summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">An <see cref="T:System.Security.IPermission" /> that is to be tested for the subset relationship. This permission must be of the same type as the current permission.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not an <see cref="T:System.Net.Mail.SmtpPermission" />.</exception>
	public override bool IsSubsetOf(IPermission target)
	{
		SmtpPermission smtpPermission = Cast(target);
		if (smtpPermission == null)
		{
			return IsEmpty();
		}
		if (unrestricted)
		{
			return smtpPermission.unrestricted;
		}
		return access <= smtpPermission.access;
	}

	/// <summary>Returns a value indicating whether the current permission is unrestricted.</summary>
	/// <returns>true if the current permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return unrestricted;
	}

	/// <summary>Creates an XML encoding of the state of the permission. </summary>
	/// <returns>A <see cref="T:System.Security.SecurityElement" /> that contains an XML encoding of the current permission.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = PermissionHelper.Element(typeof(SmtpPermission), 1);
		if (unrestricted)
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		else
		{
			switch (access)
			{
			case SmtpAccess.ConnectToUnrestrictedPort:
				securityElement.AddAttribute("Access", "ConnectToUnrestrictedPort");
				break;
			case SmtpAccess.Connect:
				securityElement.AddAttribute("Access", "Connect");
				break;
			}
		}
		return securityElement;
	}

	/// <summary>Sets the state of the permission using the specified XML encoding.</summary>
	/// <param name="securityElement">The XML encoding to use to set the state of the current permission.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="securityElement" /> does not describe an <see cref="T:System.Net.Mail.SmtpPermission" /> object.-or-<paramref name="securityElement" /> does not contain the required state information to reconstruct the permission.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="securityElement" /> is null.</exception>
	public override void FromXml(SecurityElement securityElement)
	{
		PermissionHelper.CheckSecurityElement(securityElement, "securityElement", 1, 1);
		if (securityElement.Tag != "IPermission")
		{
			throw new ArgumentException("securityElement");
		}
		if (PermissionHelper.IsUnrestricted(securityElement))
		{
			access = SmtpAccess.Connect;
		}
		else
		{
			access = SmtpAccess.None;
		}
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission. </summary>
	/// <returns>A new <see cref="T:System.Net.Mail.SmtpPermission" /> permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">An <see cref="T:System.Security.IPermission" /> to combine with the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not an <see cref="T:System.Net.Mail.SmtpPermission" />.</exception>
	public override IPermission Union(IPermission target)
	{
		SmtpPermission smtpPermission = Cast(target);
		if (smtpPermission == null)
		{
			return Copy();
		}
		if (unrestricted || smtpPermission.unrestricted)
		{
			return new SmtpPermission(unrestricted: true);
		}
		if (access > smtpPermission.access)
		{
			return new SmtpPermission(access);
		}
		return new SmtpPermission(smtpPermission.access);
	}

	private bool IsEmpty()
	{
		if (!unrestricted)
		{
			return access == SmtpAccess.None;
		}
		return false;
	}

	private SmtpPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		SmtpPermission obj = target as SmtpPermission;
		if (obj == null)
		{
			PermissionHelper.ThrowInvalidPermission(target, typeof(SmtpPermission));
		}
		return obj;
	}
}
