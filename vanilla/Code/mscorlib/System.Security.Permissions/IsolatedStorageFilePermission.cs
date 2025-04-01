using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Specifies the allowed usage of a private virtual file system. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class IsolatedStorageFilePermission : IsolatedStoragePermission, IBuiltInPermission
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.IsolatedStorageFilePermission" /> class with either fully restricted or unrestricted permission as specified.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public IsolatedStorageFilePermission(PermissionState state)
		: base(state)
	{
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		return new IsolatedStorageFilePermission(PermissionState.None)
		{
			m_userQuota = m_userQuota,
			m_machineQuota = m_machineQuota,
			m_expirationDays = m_expirationDays,
			m_permanentData = m_permanentData,
			m_allowed = m_allowed
		};
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission object. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override IPermission Intersect(IPermission target)
	{
		IsolatedStorageFilePermission isolatedStorageFilePermission = Cast(target);
		if (isolatedStorageFilePermission == null)
		{
			return null;
		}
		if (IsEmpty() && isolatedStorageFilePermission.IsEmpty())
		{
			return null;
		}
		return new IsolatedStorageFilePermission(PermissionState.None)
		{
			m_userQuota = ((m_userQuota < isolatedStorageFilePermission.m_userQuota) ? m_userQuota : isolatedStorageFilePermission.m_userQuota),
			m_machineQuota = ((m_machineQuota < isolatedStorageFilePermission.m_machineQuota) ? m_machineQuota : isolatedStorageFilePermission.m_machineQuota),
			m_expirationDays = ((m_expirationDays < isolatedStorageFilePermission.m_expirationDays) ? m_expirationDays : isolatedStorageFilePermission.m_expirationDays),
			m_permanentData = (m_permanentData && isolatedStorageFilePermission.m_permanentData),
			UsageAllowed = ((m_allowed < isolatedStorageFilePermission.m_allowed) ? m_allowed : isolatedStorageFilePermission.m_allowed)
		};
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		IsolatedStorageFilePermission isolatedStorageFilePermission = Cast(target);
		if (isolatedStorageFilePermission == null)
		{
			return IsEmpty();
		}
		if (isolatedStorageFilePermission.IsUnrestricted())
		{
			return true;
		}
		if (m_userQuota > isolatedStorageFilePermission.m_userQuota)
		{
			return false;
		}
		if (m_machineQuota > isolatedStorageFilePermission.m_machineQuota)
		{
			return false;
		}
		if (m_expirationDays > isolatedStorageFilePermission.m_expirationDays)
		{
			return false;
		}
		if (m_permanentData != isolatedStorageFilePermission.m_permanentData)
		{
			return false;
		}
		if (m_allowed > isolatedStorageFilePermission.m_allowed)
		{
			return false;
		}
		return true;
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override IPermission Union(IPermission target)
	{
		IsolatedStorageFilePermission isolatedStorageFilePermission = Cast(target);
		if (isolatedStorageFilePermission == null)
		{
			return Copy();
		}
		return new IsolatedStorageFilePermission(PermissionState.None)
		{
			m_userQuota = ((m_userQuota > isolatedStorageFilePermission.m_userQuota) ? m_userQuota : isolatedStorageFilePermission.m_userQuota),
			m_machineQuota = ((m_machineQuota > isolatedStorageFilePermission.m_machineQuota) ? m_machineQuota : isolatedStorageFilePermission.m_machineQuota),
			m_expirationDays = ((m_expirationDays > isolatedStorageFilePermission.m_expirationDays) ? m_expirationDays : isolatedStorageFilePermission.m_expirationDays),
			m_permanentData = (m_permanentData || isolatedStorageFilePermission.m_permanentData),
			UsageAllowed = ((m_allowed > isolatedStorageFilePermission.m_allowed) ? m_allowed : isolatedStorageFilePermission.m_allowed)
		};
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	[MonoTODO("(2.0) new override - something must have been added ???")]
	[ComVisible(false)]
	public override SecurityElement ToXml()
	{
		return base.ToXml();
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 3;
	}

	private IsolatedStorageFilePermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		IsolatedStorageFilePermission obj = target as IsolatedStorageFilePermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(IsolatedStorageFilePermission));
		}
		return obj;
	}
}
