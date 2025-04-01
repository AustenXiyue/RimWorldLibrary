using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Represents access to generic isolated storage capabilities.</summary>
[Serializable]
[ComVisible(true)]
[SecurityPermission(SecurityAction.InheritanceDemand, ControlEvidence = true, ControlPolicy = true)]
public abstract class IsolatedStoragePermission : CodeAccessPermission, IUnrestrictedPermission
{
	private const int version = 1;

	internal long m_userQuota;

	internal long m_machineQuota;

	internal long m_expirationDays;

	internal bool m_permanentData;

	internal IsolatedStorageContainment m_allowed;

	/// <summary>Gets or sets the quota on the overall size of each user's total store.</summary>
	/// <returns>The size, in bytes, of the resource allocated to the user.</returns>
	public long UserQuota
	{
		get
		{
			return m_userQuota;
		}
		set
		{
			m_userQuota = value;
		}
	}

	/// <summary>Gets or sets the type of isolated storage containment allowed.</summary>
	/// <returns>One of the <see cref="T:System.Security.Permissions.IsolatedStorageContainment" /> values.</returns>
	public IsolatedStorageContainment UsageAllowed
	{
		get
		{
			return m_allowed;
		}
		set
		{
			if (!Enum.IsDefined(typeof(IsolatedStorageContainment), value))
			{
				throw new ArgumentException(string.Format(Locale.GetText("Invalid enum {0}"), value), "IsolatedStorageContainment");
			}
			m_allowed = value;
			if (m_allowed == IsolatedStorageContainment.UnrestrictedIsolatedStorage)
			{
				m_userQuota = long.MaxValue;
				m_machineQuota = long.MaxValue;
				m_expirationDays = long.MaxValue;
				m_permanentData = true;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.IsolatedStoragePermission" /> class with either restricted or unrestricted permission as specified.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	protected IsolatedStoragePermission(PermissionState state)
	{
		if (CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: true) == PermissionState.Unrestricted)
		{
			UsageAllowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;
		}
	}

	/// <summary>Returns a value indicating whether the current permission is unrestricted.</summary>
	/// <returns>true if the current permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return IsolatedStorageContainment.UnrestrictedIsolatedStorage == m_allowed;
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = Element(1);
		if (m_allowed == IsolatedStorageContainment.UnrestrictedIsolatedStorage)
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		else
		{
			securityElement.AddAttribute("Allowed", m_allowed.ToString());
			if (m_userQuota > 0)
			{
				securityElement.AddAttribute("UserQuota", m_userQuota.ToString());
			}
		}
		return securityElement;
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="esd">The XML encoding to use to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="esd" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="esd" /> parameter is not a valid permission element.-or- The <paramref name="esd" /> parameter's version number is not valid. </exception>
	public override void FromXml(SecurityElement esd)
	{
		CodeAccessPermission.CheckSecurityElement(esd, "esd", 1, 1);
		m_userQuota = 0L;
		m_machineQuota = 0L;
		m_expirationDays = 0L;
		m_permanentData = false;
		m_allowed = IsolatedStorageContainment.None;
		if (CodeAccessPermission.IsUnrestricted(esd))
		{
			UsageAllowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;
			return;
		}
		string text = esd.Attribute("Allowed");
		if (text != null)
		{
			UsageAllowed = (IsolatedStorageContainment)Enum.Parse(typeof(IsolatedStorageContainment), text);
		}
		text = esd.Attribute("UserQuota");
		if (text != null)
		{
			m_userQuota = long.Parse(text, CultureInfo.InvariantCulture);
		}
	}

	internal bool IsEmpty()
	{
		if (m_userQuota == 0L)
		{
			return m_allowed == IsolatedStorageContainment.None;
		}
		return false;
	}
}
