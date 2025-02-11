namespace System.Security.Permissions;

/// <summary>Controls the ability to access encrypted data and memory. This class cannot be inherited. </summary>
[Serializable]
public sealed class DataProtectionPermission : CodeAccessPermission, IUnrestrictedPermission
{
	private const int version = 1;

	private DataProtectionPermissionFlags _flags;

	/// <summary>Gets or sets the data and memory protection flags.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Security.Permissions.DataProtectionPermissionFlags" /> values.</returns>
	/// <exception cref="T:System.ArgumentException">The specified value is not a valid combination of the <see cref="T:System.Security.Permissions.DataProtectionPermissionFlags" /> values. </exception>
	public DataProtectionPermissionFlags Flags
	{
		get
		{
			return _flags;
		}
		set
		{
			if ((value & ~DataProtectionPermissionFlags.AllFlags) != 0)
			{
				throw new ArgumentException(string.Format(global::Locale.GetText("Invalid enum {0}"), value), "DataProtectionPermissionFlags");
			}
			_flags = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.DataProtectionPermission" /> class with the specified permission state. </summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="state" /> is not a valid <see cref="T:System.Security.Permissions.PermissionState" /> value. </exception>
	public DataProtectionPermission(PermissionState state)
	{
		if (System.Security.Permissions.PermissionHelper.CheckPermissionState(state, allowUnrestricted: true) == PermissionState.Unrestricted)
		{
			_flags = DataProtectionPermissionFlags.AllFlags;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.DataProtectionPermission" /> class with the specified permission flags. </summary>
	/// <param name="flag">A bitwise combination of the <see cref="T:System.Security.Permissions.DataProtectionPermissionFlags" /> values.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="flags" /> is not a valid combination of the <see cref="T:System.Security.Permissions.DataProtectionPermissionFlags" /> values. </exception>
	public DataProtectionPermission(DataProtectionPermissionFlags flag)
	{
		Flags = flag;
	}

	/// <summary>Returns a value indicating whether the current permission is unrestricted.</summary>
	/// <returns>true if the current permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return _flags == DataProtectionPermissionFlags.AllFlags;
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		return new DataProtectionPermission(_flags);
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not null and does not specify a permission of the same type as the current permission. </exception>
	public override IPermission Intersect(IPermission target)
	{
		DataProtectionPermission dataProtectionPermission = Cast(target);
		if (dataProtectionPermission == null)
		{
			return null;
		}
		if (IsUnrestricted() && dataProtectionPermission.IsUnrestricted())
		{
			return new DataProtectionPermission(PermissionState.Unrestricted);
		}
		if (IsUnrestricted())
		{
			return dataProtectionPermission.Copy();
		}
		if (dataProtectionPermission.IsUnrestricted())
		{
			return Copy();
		}
		return new DataProtectionPermission(_flags & dataProtectionPermission._flags);
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not null and does not specify a permission of the same type as the current permission. </exception>
	public override IPermission Union(IPermission target)
	{
		DataProtectionPermission dataProtectionPermission = Cast(target);
		if (dataProtectionPermission == null)
		{
			return Copy();
		}
		if (IsUnrestricted() || dataProtectionPermission.IsUnrestricted())
		{
			return new SecurityPermission(PermissionState.Unrestricted);
		}
		return new DataProtectionPermission(_flags | dataProtectionPermission._flags);
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission to test for the subset relationship. This permission must be the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not null and does not specify a permission of the same type as the current permission. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		DataProtectionPermission dataProtectionPermission = Cast(target);
		if (dataProtectionPermission == null)
		{
			return _flags == DataProtectionPermissionFlags.NoFlags;
		}
		if (dataProtectionPermission.IsUnrestricted())
		{
			return true;
		}
		if (IsUnrestricted())
		{
			return false;
		}
		return (_flags & ~dataProtectionPermission._flags) == 0;
	}

	/// <summary>Reconstructs a permission with a specific state from an XML encoding.</summary>
	/// <param name="securityElement">A <see cref="T:System.Security.SecurityElement" /> that contains the XML encoding used to reconstruct the permission.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="securityElement" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="securityElement" /> is not a valid permission element.-or- The version number of <paramref name="securityElement" /> is not supported. </exception>
	public override void FromXml(SecurityElement securityElement)
	{
		System.Security.Permissions.PermissionHelper.CheckSecurityElement(securityElement, "securityElement", 1, 1);
		_flags = (DataProtectionPermissionFlags)Enum.Parse(typeof(DataProtectionPermissionFlags), securityElement.Attribute("Flags"));
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = System.Security.Permissions.PermissionHelper.Element(typeof(DataProtectionPermission), 1);
		securityElement.AddAttribute("Flags", _flags.ToString());
		return securityElement;
	}

	private DataProtectionPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		DataProtectionPermission obj = target as DataProtectionPermission;
		if (obj == null)
		{
			System.Security.Permissions.PermissionHelper.ThrowInvalidPermission(target, typeof(DataProtectionPermission));
		}
		return obj;
	}
}
