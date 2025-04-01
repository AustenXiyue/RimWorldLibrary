using System.Security;
using System.Security.Permissions;

namespace System.Web;

/// <summary>Controls access permissions in ASP.NET hosted environments. This class cannot be inherited.</summary>
[Serializable]
public sealed class AspNetHostingPermission : CodeAccessPermission, IUnrestrictedPermission
{
	private const int version = 1;

	private AspNetHostingPermissionLevel _level;

	/// <summary>Gets or sets the current hosting permission level for an ASP.NET application.</summary>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Web.AspNetHostingPermission" /> class with the specified permission level.</summary>
	/// <param name="level">An <see cref="T:System.Web.AspNetHostingPermissionLevel" /> enumeration value. </param>
	public AspNetHostingPermission(AspNetHostingPermissionLevel level)
	{
		Level = level;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Web.AspNetHostingPermission" /> class with the specified <see cref="T:System.Security.Permissions.PermissionState" /> enumeration value.</summary>
	/// <param name="state">A <see cref="T:System.Security.Permissions.PermissionState" /> enumeration value. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="state" /> is not set to one of the <see cref="T:System.Security.Permissions.PermissionState" /> enumeration values.</exception>
	public AspNetHostingPermission(PermissionState state)
	{
		if (PermissionHelper.CheckPermissionState(state, allowUnrestricted: true) == PermissionState.Unrestricted)
		{
			_level = AspNetHostingPermissionLevel.Unrestricted;
		}
		else
		{
			_level = AspNetHostingPermissionLevel.None;
		}
	}

	/// <summary>Returns a value indicating whether unrestricted access to the resource that is protected by the current permission is allowed.</summary>
	/// <returns>true if unrestricted use of the resource protected by the permission is allowed; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return _level == AspNetHostingPermissionLevel.Unrestricted;
	}

	/// <summary>When implemented by a derived class, creates and returns an identical copy of the current permission object.</summary>
	/// <returns>A copy of the current permission object.</returns>
	public override IPermission Copy()
	{
		return new AspNetHostingPermission(_level);
	}

	/// <summary>Reconstructs a permission object with a specified state from an XML encoding.</summary>
	/// <param name="securityElement">The <see cref="T:System.Security.SecurityElement" /> containing the XML encoding to use to reconstruct the permission object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="securityElement" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.SecurityElement.Tag" /> property of <paramref name="securityElement" /> is not equal to "IPermission". - or- The class <see cref="M:System.Security.SecurityElement.Attribute(System.String)" /> of <paramref name="securityElement" /> is null or an empty string (""). </exception>
	public override void FromXml(SecurityElement securityElement)
	{
		PermissionHelper.CheckSecurityElement(securityElement, "securityElement", 1, 1);
		if (securityElement.Tag != "IPermission")
		{
			throw new ArgumentException(string.Format(global::Locale.GetText("Invalid tag '{0}' for permission."), securityElement.Tag), "securityElement");
		}
		if (securityElement.Attribute("version") == null)
		{
			throw new ArgumentException(global::Locale.GetText("Missing version attribute."), "securityElement");
		}
		if (PermissionHelper.IsUnrestricted(securityElement))
		{
			_level = AspNetHostingPermissionLevel.Unrestricted;
			return;
		}
		string text = securityElement.Attribute("Level");
		if (text != null)
		{
			_level = (AspNetHostingPermissionLevel)Enum.Parse(typeof(AspNetHostingPermissionLevel), text);
		}
		else
		{
			_level = AspNetHostingPermissionLevel.None;
		}
	}

	/// <summary>Creates an XML encoding of the permission object and its current state.</summary>
	/// <returns>A <see cref="T:System.Security.SecurityElement" /> containing the XML encoding of the permission object, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = PermissionHelper.Element(typeof(AspNetHostingPermission), 1);
		if (IsUnrestricted())
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		securityElement.AddAttribute("Level", _level.ToString());
		return securityElement;
	}

	/// <summary>When implemented by a derived class, creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>An <see cref="T:System.Security.IPermission" /> that represents the intersection of the current permission and the specified permission; otherwise, null if the intersection is empty.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not an <see cref="T:System.Web.AspNetHostingPermission" />. </exception>
	public override IPermission Intersect(IPermission target)
	{
		AspNetHostingPermission aspNetHostingPermission = Cast(target);
		if (aspNetHostingPermission == null)
		{
			return null;
		}
		return new AspNetHostingPermission((_level <= aspNetHostingPermission.Level) ? _level : aspNetHostingPermission.Level);
	}

	/// <summary>Returns a value indicating whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current <see cref="T:System.Security.IPermission" /> is a subset of the specified <see cref="T:System.Security.IPermission" />; otherwise, false.</returns>
	/// <param name="target">The <see cref="T:System.Security.IPermission" /> to combine with the current permission. It must be of the same type as the current <see cref="T:System.Security.IPermission" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not an <see cref="T:System.Web.AspNetHostingPermission" />. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		AspNetHostingPermission aspNetHostingPermission = Cast(target);
		if (aspNetHostingPermission == null)
		{
			return IsEmpty();
		}
		return _level <= aspNetHostingPermission._level;
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>An <see cref="T:System.Security.IPermission" /> that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not an <see cref="T:System.Web.AspNetHostingPermission" />. </exception>
	public override IPermission Union(IPermission target)
	{
		AspNetHostingPermission aspNetHostingPermission = Cast(target);
		if (aspNetHostingPermission == null)
		{
			return Copy();
		}
		return new AspNetHostingPermission((_level > aspNetHostingPermission.Level) ? _level : aspNetHostingPermission.Level);
	}

	private bool IsEmpty()
	{
		return _level == AspNetHostingPermissionLevel.None;
	}

	private AspNetHostingPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		AspNetHostingPermission obj = target as AspNetHostingPermission;
		if (obj == null)
		{
			PermissionHelper.ThrowInvalidPermission(target, typeof(AspNetHostingPermission));
		}
		return obj;
	}
}
