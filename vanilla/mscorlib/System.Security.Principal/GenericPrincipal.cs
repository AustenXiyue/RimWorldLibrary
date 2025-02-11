using System.Runtime.InteropServices;
using System.Security.Claims;

namespace System.Security.Principal;

/// <summary>Represents a generic principal.</summary>
[Serializable]
[ComVisible(true)]
public class GenericPrincipal : ClaimsPrincipal
{
	private IIdentity m_identity;

	private string[] m_roles;

	internal string[] Roles => m_roles;

	/// <summary>Gets the <see cref="T:System.Security.Principal.GenericIdentity" /> of the user represented by the current <see cref="T:System.Security.Principal.GenericPrincipal" />.</summary>
	/// <returns>The <see cref="T:System.Security.Principal.GenericIdentity" /> of the user represented by the <see cref="T:System.Security.Principal.GenericPrincipal" />.</returns>
	public override IIdentity Identity => m_identity;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.GenericPrincipal" /> class from a user identity and an array of role names to which the user represented by that identity belongs.</summary>
	/// <param name="identity">A basic implementation of <see cref="T:System.Security.Principal.IIdentity" /> that represents any user. </param>
	/// <param name="roles">An array of role names to which the user represented by the <paramref name="identity" /> parameter belongs. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="identity" /> parameter is null. </exception>
	public GenericPrincipal(IIdentity identity, string[] roles)
	{
		if (identity == null)
		{
			throw new ArgumentNullException("identity");
		}
		m_identity = identity;
		if (roles != null)
		{
			m_roles = new string[roles.Length];
			for (int i = 0; i < roles.Length; i++)
			{
				m_roles[i] = roles[i];
			}
		}
	}

	/// <summary>Determines whether the current <see cref="T:System.Security.Principal.GenericPrincipal" /> belongs to the specified role.</summary>
	/// <returns>true if the current <see cref="T:System.Security.Principal.GenericPrincipal" /> is a member of the specified role; otherwise, false.</returns>
	/// <param name="role">The name of the role for which to check membership. </param>
	public override bool IsInRole(string role)
	{
		if (m_roles == null)
		{
			return false;
		}
		int length = role.Length;
		string[] roles = m_roles;
		foreach (string text in roles)
		{
			if (text != null && length == text.Length && string.Compare(role, 0, text, 0, length, ignoreCase: true) == 0)
			{
				return true;
			}
		}
		return false;
	}
}
