using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Controls the ability to access key containers. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class KeyContainerPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
{
	private KeyContainerPermissionAccessEntryCollection _accessEntries;

	private KeyContainerPermissionFlags _flags;

	private const int version = 1;

	/// <summary>Gets the collection of <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> objects associated with the current permission.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntryCollection" /> containing the <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> objects for this <see cref="T:System.Security.Permissions.KeyContainerPermission" />.</returns>
	public KeyContainerPermissionAccessEntryCollection AccessEntries => _accessEntries;

	/// <summary>Gets the key container permission flags that apply to all key containers associated with the permission.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.Security.Permissions.KeyContainerPermissionFlags" /> values.</returns>
	public KeyContainerPermissionFlags Flags => _flags;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.KeyContainerPermission" /> class with either restricted or unrestricted permission.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="state" /> is not a valid <see cref="T:System.Security.Permissions.PermissionState" /> value. </exception>
	public KeyContainerPermission(PermissionState state)
	{
		if (CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: true) == PermissionState.Unrestricted)
		{
			_flags = KeyContainerPermissionFlags.AllFlags;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.KeyContainerPermission" /> class with the specified access.</summary>
	/// <param name="flags">A bitwise combination of the <see cref="T:System.Security.Permissions.KeyContainerPermissionFlags" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="flags" /> is not a valid combination of the <see cref="T:System.Security.Permissions.KeyContainerPermissionFlags" /> values. </exception>
	public KeyContainerPermission(KeyContainerPermissionFlags flags)
	{
		SetFlags(flags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.KeyContainerPermission" /> class with the specified global access and specific key container access rights.</summary>
	/// <param name="flags">A bitwise combination of the <see cref="T:System.Security.Permissions.KeyContainerPermissionFlags" /> values. </param>
	/// <param name="accessList">An array of <see cref="T:System.Security.Permissions.KeyContainerPermissionAccessEntry" /> objects identifying specific key container access rights. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="flags" /> is not a valid combination of the <see cref="T:System.Security.Permissions.KeyContainerPermissionFlags" /> values. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="accessList" /> is null. </exception>
	public KeyContainerPermission(KeyContainerPermissionFlags flags, KeyContainerPermissionAccessEntry[] accessList)
	{
		SetFlags(flags);
		if (accessList != null)
		{
			_accessEntries = new KeyContainerPermissionAccessEntryCollection();
			foreach (KeyContainerPermissionAccessEntry accessEntry in accessList)
			{
				_accessEntries.Add(accessEntry);
			}
		}
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		if (_accessEntries.Count == 0)
		{
			return new KeyContainerPermission(_flags);
		}
		KeyContainerPermissionAccessEntry[] array = new KeyContainerPermissionAccessEntry[_accessEntries.Count];
		_accessEntries.CopyTo(array, 0);
		return new KeyContainerPermission(_flags, array);
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="securityElement">A <see cref="T:System.Security.SecurityElement" /> that contains the XML encoding used to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="securityElement" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="securityElement" /> is not a valid permission element.-or- The version number of <paramref name="securityElement" /> is not supported. </exception>
	[MonoTODO("(2.0) missing support for AccessEntries")]
	public override void FromXml(SecurityElement securityElement)
	{
		CodeAccessPermission.CheckSecurityElement(securityElement, "securityElement", 1, 1);
		if (CodeAccessPermission.IsUnrestricted(securityElement))
		{
			_flags = KeyContainerPermissionFlags.AllFlags;
		}
		else
		{
			_flags = (KeyContainerPermissionFlags)Enum.Parse(typeof(KeyContainerPermissionFlags), securityElement.Attribute("Flags"));
		}
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not null and does not specify a permission of the same type as the current permission. </exception>
	[MonoTODO("(2.0)")]
	public override IPermission Intersect(IPermission target)
	{
		return null;
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission to test for the subset relationship. This permission must be the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not null and does not specify a permission of the same type as the current permission. </exception>
	[MonoTODO("(2.0)")]
	public override bool IsSubsetOf(IPermission target)
	{
		return false;
	}

	/// <summary>Determines whether the current permission is unrestricted.</summary>
	/// <returns>true if the current permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return _flags == KeyContainerPermissionFlags.AllFlags;
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>A <see cref="T:System.Security.SecurityElement" /> that contains an XML encoding of the permission, including state information.</returns>
	[MonoTODO("(2.0) missing support for AccessEntries")]
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = Element(1);
		if (IsUnrestricted())
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		return securityElement;
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not null and does not specify a permission of the same type as the current permission. </exception>
	public override IPermission Union(IPermission target)
	{
		KeyContainerPermission keyContainerPermission = Cast(target);
		if (keyContainerPermission == null)
		{
			return Copy();
		}
		KeyContainerPermissionAccessEntryCollection keyContainerPermissionAccessEntryCollection = new KeyContainerPermissionAccessEntryCollection();
		KeyContainerPermissionAccessEntryEnumerator enumerator = _accessEntries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyContainerPermissionAccessEntry current = enumerator.Current;
			keyContainerPermissionAccessEntryCollection.Add(current);
		}
		enumerator = keyContainerPermission._accessEntries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyContainerPermissionAccessEntry current2 = enumerator.Current;
			if (_accessEntries.IndexOf(current2) == -1)
			{
				keyContainerPermissionAccessEntryCollection.Add(current2);
			}
		}
		if (keyContainerPermissionAccessEntryCollection.Count == 0)
		{
			return new KeyContainerPermission(_flags | keyContainerPermission._flags);
		}
		KeyContainerPermissionAccessEntry[] array = new KeyContainerPermissionAccessEntry[keyContainerPermissionAccessEntryCollection.Count];
		keyContainerPermissionAccessEntryCollection.CopyTo(array, 0);
		return new KeyContainerPermission(_flags | keyContainerPermission._flags, array);
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 16;
	}

	private void SetFlags(KeyContainerPermissionFlags flags)
	{
		if ((flags & KeyContainerPermissionFlags.AllFlags) == 0)
		{
			throw new ArgumentException(string.Format(Locale.GetText("Invalid enum {0}"), flags), "KeyContainerPermissionFlags");
		}
		_flags = flags;
	}

	private KeyContainerPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		KeyContainerPermission obj = target as KeyContainerPermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(KeyContainerPermission));
		}
		return obj;
	}
}
