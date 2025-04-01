using System.Globalization;

namespace System.Security.Permissions;

/// <summary>Defines partial-trust access to the <see cref="T:System.ComponentModel.TypeDescriptor" /> class.</summary>
[Serializable]
public sealed class TypeDescriptorPermission : CodeAccessPermission, IUnrestrictedPermission
{
	private TypeDescriptorPermissionFlags m_flags;

	/// <summary>Gets or sets the <see cref="T:System.Security.Permissions.TypeDescriptorPermissionFlags" /> for the type descriptor. </summary>
	/// <returns>The <see cref="T:System.Security.Permissions.TypeDescriptorPermissionFlags" /> for the type descriptor.</returns>
	public TypeDescriptorPermissionFlags Flags
	{
		get
		{
			return m_flags;
		}
		set
		{
			VerifyAccess(value);
			m_flags = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.TypeDescriptorPermission" /> class. </summary>
	/// <param name="state">The <see cref="T:System.Security.Permissions.PermissionState" /> to request. Only <see cref="F:System.Security.Permissions.PermissionState.Unrestricted" />  and <see cref="F:System.Security.Permissions.PermissionState.None" /> are valid.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="state" /> is not a valid permission state. Only <see cref="F:System.Security.Permissions.PermissionState.Unrestricted" />  and <see cref="F:System.Security.Permissions.PermissionState.None" /> are valid. </exception>
	public TypeDescriptorPermission(PermissionState state)
	{
		switch (state)
		{
		case PermissionState.Unrestricted:
			SetUnrestricted(unrestricted: true);
			break;
		case PermissionState.None:
			SetUnrestricted(unrestricted: false);
			break;
		default:
			throw new ArgumentException(global::SR.GetString("Invalid permission state."));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.TypeDescriptorPermission" /> class with the specified permission flags. </summary>
	/// <param name="flag">The permission flags to request.</param>
	public TypeDescriptorPermission(TypeDescriptorPermissionFlags flag)
	{
		VerifyAccess(flag);
		SetUnrestricted(unrestricted: false);
		m_flags = flag;
	}

	private void SetUnrestricted(bool unrestricted)
	{
		if (unrestricted)
		{
			m_flags = TypeDescriptorPermissionFlags.RestrictedRegistrationAccess;
		}
		else
		{
			Reset();
		}
	}

	private void Reset()
	{
		m_flags = TypeDescriptorPermissionFlags.NoFlags;
	}

	/// <summary>Gets a value that indicates whether the type descriptor may be called from partially trusted code. </summary>
	/// <returns>true if the <see cref="P:System.Security.Permissions.TypeDescriptorPermission.Flags" /> property is set to <see cref="F:System.Security.Permissions.TypeDescriptorPermissionFlags.RestrictedRegistrationAccess" />; otherwise, false. </returns>
	public bool IsUnrestricted()
	{
		return m_flags == TypeDescriptorPermissionFlags.RestrictedRegistrationAccess;
	}

	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission.</param>
	public override IPermission Union(IPermission target)
	{
		if (target == null)
		{
			return Copy();
		}
		try
		{
			TypeDescriptorPermission typeDescriptorPermission = (TypeDescriptorPermission)target;
			TypeDescriptorPermissionFlags typeDescriptorPermissionFlags = m_flags | typeDescriptorPermission.m_flags;
			if (typeDescriptorPermissionFlags == TypeDescriptorPermissionFlags.NoFlags)
			{
				return null;
			}
			return new TypeDescriptorPermission(typeDescriptorPermissionFlags);
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, global::SR.GetString("Operation on type '{0}' attempted with target of incorrect type."), GetType().FullName));
		}
	}

	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission.</param>
	public override bool IsSubsetOf(IPermission target)
	{
		if (target == null)
		{
			return m_flags == TypeDescriptorPermissionFlags.NoFlags;
		}
		try
		{
			TypeDescriptorPermission obj = (TypeDescriptorPermission)target;
			TypeDescriptorPermissionFlags flags = m_flags;
			TypeDescriptorPermissionFlags flags2 = obj.m_flags;
			return (flags & flags2) == flags;
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, global::SR.GetString("Operation on type '{0}' attempted with target of incorrect type."), GetType().FullName));
		}
	}

	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission.</param>
	public override IPermission Intersect(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		try
		{
			TypeDescriptorPermissionFlags typeDescriptorPermissionFlags = ((TypeDescriptorPermission)target).m_flags & m_flags;
			if (typeDescriptorPermissionFlags == TypeDescriptorPermissionFlags.NoFlags)
			{
				return null;
			}
			return new TypeDescriptorPermission(typeDescriptorPermissionFlags);
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, global::SR.GetString("Operation on type '{0}' attempted with target of incorrect type."), GetType().FullName));
		}
	}

	/// <returns>A copy of the current permission object.</returns>
	public override IPermission Copy()
	{
		return new TypeDescriptorPermission(m_flags);
	}

	private void VerifyAccess(TypeDescriptorPermissionFlags type)
	{
		if ((type & ~TypeDescriptorPermissionFlags.RestrictedRegistrationAccess) != 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, global::SR.GetString("Illegal enum value: {0}."), (int)type));
		}
	}

	/// <returns>An XML encoding of the security object, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = new SecurityElement("IPermission");
		securityElement.AddAttribute("class", GetType().FullName + ", " + GetType().Module.Assembly.FullName.Replace('"', '\''));
		securityElement.AddAttribute("version", "1");
		if (!IsUnrestricted())
		{
			securityElement.AddAttribute("Flags", m_flags.ToString());
		}
		else
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		return securityElement;
	}

	/// <param name="securityElement">The XML encoding to use to reconstruct the security object.</param>
	public override void FromXml(SecurityElement securityElement)
	{
		if (securityElement == null)
		{
			throw new ArgumentNullException("securityElement");
		}
		string text = securityElement.Attribute("class");
		if (text == null || text.IndexOf(GetType().FullName, StringComparison.Ordinal) == -1)
		{
			throw new ArgumentException(global::SR.GetString("The value of \"class\" attribute is invalid."), "securityElement");
		}
		string text2 = securityElement.Attribute("Unrestricted");
		if (text2 != null && string.Compare(text2, "true", StringComparison.OrdinalIgnoreCase) == 0)
		{
			m_flags = TypeDescriptorPermissionFlags.RestrictedRegistrationAccess;
			return;
		}
		m_flags = TypeDescriptorPermissionFlags.NoFlags;
		string text3 = securityElement.Attribute("Flags");
		if (text3 != null)
		{
			TypeDescriptorPermissionFlags flags = (TypeDescriptorPermissionFlags)Enum.Parse(typeof(TypeDescriptorPermissionFlags), text3);
			VerifyFlags(flags);
			m_flags = flags;
		}
	}

	internal static void VerifyFlags(TypeDescriptorPermissionFlags flags)
	{
		if ((flags & ~TypeDescriptorPermissionFlags.RestrictedRegistrationAccess) != 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, global::SR.GetString("Illegal enum value: {0}."), (int)flags));
		}
	}
}
