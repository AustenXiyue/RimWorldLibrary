namespace System.Security.Permissions;

/// <summary>Controls access to stores containing X.509 certificates. This class cannot be inherited.</summary>
[Serializable]
public sealed class StorePermission : CodeAccessPermission, IUnrestrictedPermission
{
	private const int version = 1;

	private StorePermissionFlags _flags;

	/// <summary>Gets or sets the type of <see cref="T:System.Security.Cryptography.X509Certificates.X509Store" /> access allowed by the current permission.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Security.Permissions.StorePermissionFlags" /> values.</returns>
	/// <exception cref="T:System.ArgumentException">An attempt is made to set this property to an invalid value. See <see cref="T:System.Security.Permissions.StorePermissionFlags" /> for the valid values. </exception>
	public StorePermissionFlags Flags
	{
		get
		{
			return _flags;
		}
		set
		{
			if (value != 0 && (value & StorePermissionFlags.AllFlags) == 0)
			{
				throw new ArgumentException(string.Format(global::Locale.GetText("Invalid enum {0}"), value), "StorePermissionFlags");
			}
			_flags = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.StorePermission" /> class with either fully restricted or unrestricted permission state.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="state" /> is not a valid <see cref="T:System.Security.Permissions.PermissionState" /> value. </exception>
	public StorePermission(PermissionState state)
	{
		if (PermissionHelper.CheckPermissionState(state, allowUnrestricted: true) == PermissionState.Unrestricted)
		{
			_flags = StorePermissionFlags.AllFlags;
		}
		else
		{
			_flags = StorePermissionFlags.NoFlags;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.StorePermission" /> class with the specified access.</summary>
	/// <param name="flag">A bitwise combination of the <see cref="T:System.Security.Permissions.StorePermissionFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="flag" /> is not a valid combination of <see cref="T:System.Security.Permissions.StorePermissionFlags" /> values. </exception>
	public StorePermission(StorePermissionFlags flag)
	{
		Flags = flag;
	}

	/// <summary>Returns a value indicating whether the current permission is unrestricted.</summary>
	/// <returns>true if the current permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return _flags == StorePermissionFlags.AllFlags;
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override IPermission Copy()
	{
		if (_flags == StorePermissionFlags.NoFlags)
		{
			return null;
		}
		return new StorePermission(_flags);
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> s not null and is not of the same type as the current permission. </exception>
	public override IPermission Intersect(IPermission target)
	{
		StorePermission storePermission = Cast(target);
		if (storePermission == null)
		{
			return null;
		}
		if (IsUnrestricted() && storePermission.IsUnrestricted())
		{
			return new StorePermission(PermissionState.Unrestricted);
		}
		if (IsUnrestricted())
		{
			return storePermission.Copy();
		}
		if (storePermission.IsUnrestricted())
		{
			return Copy();
		}
		StorePermissionFlags storePermissionFlags = _flags & storePermission._flags;
		if (storePermissionFlags == StorePermissionFlags.NoFlags)
		{
			return null;
		}
		return new StorePermission(storePermissionFlags);
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not null and is not of the same type as the current permission. </exception>
	public override IPermission Union(IPermission target)
	{
		StorePermission storePermission = Cast(target);
		if (storePermission == null)
		{
			return Copy();
		}
		if (IsUnrestricted() || storePermission.IsUnrestricted())
		{
			return new StorePermission(PermissionState.Unrestricted);
		}
		StorePermissionFlags storePermissionFlags = _flags | storePermission._flags;
		if (storePermissionFlags == StorePermissionFlags.NoFlags)
		{
			return null;
		}
		return new StorePermission(storePermissionFlags);
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission to test for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not null and is not of the same type as the current permission. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		StorePermission storePermission = Cast(target);
		if (storePermission == null)
		{
			return _flags == StorePermissionFlags.NoFlags;
		}
		if (storePermission.IsUnrestricted())
		{
			return true;
		}
		if (IsUnrestricted())
		{
			return false;
		}
		return (_flags & ~storePermission._flags) == 0;
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="securityElement">A <see cref="T:System.Security.SecurityElement" /> that contains the XML encoding to use to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="securityElement" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="securityElement" /> is not a valid permission element.-or- The version number in <paramref name="securityElement" /> is not valid. </exception>
	public override void FromXml(SecurityElement securityElement)
	{
		PermissionHelper.CheckSecurityElement(securityElement, "securityElement", 1, 1);
		string text = securityElement.Attribute("Flags");
		if (text == null)
		{
			_flags = StorePermissionFlags.NoFlags;
		}
		else
		{
			_flags = (StorePermissionFlags)Enum.Parse(typeof(StorePermissionFlags), text);
		}
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>A <see cref="T:System.Security.SecurityElement" /> that contains an XML encoding of the permission, including any state information.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = PermissionHelper.Element(typeof(StorePermission), 1);
		if (IsUnrestricted())
		{
			securityElement.AddAttribute("Unrestricted", bool.TrueString);
		}
		else
		{
			securityElement.AddAttribute("Flags", _flags.ToString());
		}
		return securityElement;
	}

	private StorePermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		StorePermission obj = target as StorePermission;
		if (obj == null)
		{
			PermissionHelper.ThrowInvalidPermission(target, typeof(StorePermission));
		}
		return obj;
	}
}
