using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Describes a set of security permissions applied to code. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class SecurityPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
{
	private const int version = 1;

	private SecurityPermissionFlag flags;

	/// <summary>Gets or sets the security permission flags.</summary>
	/// <returns>The state of the current permission, represented by a bitwise OR combination of any permission bits defined by <see cref="T:System.Security.Permissions.SecurityPermissionFlag" />.</returns>
	/// <exception cref="T:System.ArgumentException">An attempt is made to set this property to an invalid value. See <see cref="T:System.Security.Permissions.SecurityPermissionFlag" /> for the valid values. </exception>
	public SecurityPermissionFlag Flags
	{
		get
		{
			return flags;
		}
		set
		{
			if ((value & SecurityPermissionFlag.AllFlags) != value)
			{
				throw new ArgumentException(string.Format(Locale.GetText("Invalid flags {0}"), value), "SecurityPermissionFlag");
			}
			flags = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.SecurityPermission" /> class with either restricted or unrestricted permission as specified.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public SecurityPermission(PermissionState state)
	{
		if (CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: true) == PermissionState.Unrestricted)
		{
			flags = SecurityPermissionFlag.AllFlags;
		}
		else
		{
			flags = SecurityPermissionFlag.NoFlags;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.SecurityPermission" /> class with the specified initial set state of the flags.</summary>
	/// <param name="flag">The initial state of the permission, represented by a bitwise OR combination of any permission bits defined by <see cref="T:System.Security.Permissions.SecurityPermissionFlag" />. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="flag" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.SecurityPermissionFlag" />. </exception>
	public SecurityPermission(SecurityPermissionFlag flag)
	{
		Flags = flag;
	}

	/// <summary>Returns a value indicating whether the current permission is unrestricted.</summary>
	/// <returns>true if the current permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return flags == SecurityPermissionFlag.AllFlags;
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		return new SecurityPermission(flags);
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission object that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override IPermission Intersect(IPermission target)
	{
		SecurityPermission securityPermission = Cast(target);
		if (securityPermission == null)
		{
			return null;
		}
		if (IsEmpty() || securityPermission.IsEmpty())
		{
			return null;
		}
		if (IsUnrestricted() && securityPermission.IsUnrestricted())
		{
			return new SecurityPermission(PermissionState.Unrestricted);
		}
		if (IsUnrestricted())
		{
			return securityPermission.Copy();
		}
		if (securityPermission.IsUnrestricted())
		{
			return Copy();
		}
		SecurityPermissionFlag securityPermissionFlag = flags & securityPermission.flags;
		if (securityPermissionFlag == SecurityPermissionFlag.NoFlags)
		{
			return null;
		}
		return new SecurityPermission(securityPermissionFlag);
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override IPermission Union(IPermission target)
	{
		SecurityPermission securityPermission = Cast(target);
		if (securityPermission == null)
		{
			return Copy();
		}
		if (IsUnrestricted() || securityPermission.IsUnrestricted())
		{
			return new SecurityPermission(PermissionState.Unrestricted);
		}
		return new SecurityPermission(flags | securityPermission.flags);
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		SecurityPermission securityPermission = Cast(target);
		if (securityPermission == null)
		{
			return IsEmpty();
		}
		if (securityPermission.IsUnrestricted())
		{
			return true;
		}
		if (IsUnrestricted())
		{
			return false;
		}
		return (flags & ~securityPermission.flags) == 0;
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="esd">The XML encoding to use to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="esd" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="esd" /> parameter is not a valid permission element.-or- The <paramref name="esd" /> parameter's version number is not supported. </exception>
	public override void FromXml(SecurityElement esd)
	{
		CodeAccessPermission.CheckSecurityElement(esd, "esd", 1, 1);
		if (CodeAccessPermission.IsUnrestricted(esd))
		{
			flags = SecurityPermissionFlag.AllFlags;
			return;
		}
		string text = esd.Attribute("Flags");
		if (text == null)
		{
			flags = SecurityPermissionFlag.NoFlags;
		}
		else
		{
			flags = (SecurityPermissionFlag)Enum.Parse(typeof(SecurityPermissionFlag), text);
		}
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = Element(1);
		if (IsUnrestricted())
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		else
		{
			securityElement.AddAttribute("Flags", flags.ToString());
		}
		return securityElement;
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 6;
	}

	private bool IsEmpty()
	{
		return flags == SecurityPermissionFlag.NoFlags;
	}

	private SecurityPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		SecurityPermission obj = target as SecurityPermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(SecurityPermission));
		}
		return obj;
	}
}
