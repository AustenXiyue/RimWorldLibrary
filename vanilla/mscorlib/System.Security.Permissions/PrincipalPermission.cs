using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace System.Security.Permissions;

/// <summary>Allows checks against the active principal (see <see cref="T:System.Security.Principal.IPrincipal" />) using the language constructs defined for both declarative and imperative security actions. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class PrincipalPermission : IPermission, ISecurityEncodable, IUnrestrictedPermission, IBuiltInPermission
{
	internal class PrincipalInfo
	{
		private string _name;

		private string _role;

		private bool _isAuthenticated;

		public string Name => _name;

		public string Role => _role;

		public bool IsAuthenticated => _isAuthenticated;

		public PrincipalInfo(string name, string role, bool isAuthenticated)
		{
			_name = name;
			_role = role;
			_isAuthenticated = isAuthenticated;
		}
	}

	private const int version = 1;

	private ArrayList principals;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.PrincipalPermission" /> class with the specified <see cref="T:System.Security.Permissions.PermissionState" />.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public PrincipalPermission(PermissionState state)
	{
		principals = new ArrayList();
		if (CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: true) == PermissionState.Unrestricted)
		{
			PrincipalInfo value = new PrincipalInfo(null, null, isAuthenticated: true);
			principals.Add(value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.PrincipalPermission" /> class for the specified <paramref name="name" /> and <paramref name="role" />.</summary>
	/// <param name="name">The name of the <see cref="T:System.Security.Principal.IPrincipal" /> object's user. </param>
	/// <param name="role">The role of the <see cref="T:System.Security.Principal.IPrincipal" /> object's user (for example, Administrator). </param>
	public PrincipalPermission(string name, string role)
		: this(name, role, isAuthenticated: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.PrincipalPermission" /> class for the specified <paramref name="name" />, <paramref name="role" />, and authentication status.</summary>
	/// <param name="name">The name of the <see cref="T:System.Security.Principal.IPrincipal" /> object's user. </param>
	/// <param name="role">The role of the <see cref="T:System.Security.Principal.IPrincipal" /> object's user (for example, Administrator). </param>
	/// <param name="isAuthenticated">true to signify that the user is authenticated; otherwise, false. </param>
	public PrincipalPermission(string name, string role, bool isAuthenticated)
	{
		principals = new ArrayList();
		PrincipalInfo value = new PrincipalInfo(name, role, isAuthenticated);
		principals.Add(value);
	}

	internal PrincipalPermission(ArrayList principals)
	{
		this.principals = (ArrayList)principals.Clone();
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public IPermission Copy()
	{
		return new PrincipalPermission(principals);
	}

	/// <summary>Determines at run time whether the current principal matches the principal specified by the current permission.</summary>
	/// <exception cref="T:System.Security.SecurityException">The current principal does not pass the security check for the principal specified by the current permission.-or- The current <see cref="T:System.Security.Principal.IPrincipal" /> is null. </exception>
	[SecuritySafeCritical]
	public void Demand()
	{
		IPrincipal currentPrincipal = Thread.CurrentPrincipal;
		if (currentPrincipal == null)
		{
			throw new SecurityException("no Principal");
		}
		if (principals.Count <= 0)
		{
			return;
		}
		bool flag = false;
		foreach (PrincipalInfo principal in principals)
		{
			if ((principal.Name == null || principal.Name == currentPrincipal.Identity.Name) && (principal.Role == null || currentPrincipal.IsInRole(principal.Role)) && ((principal.IsAuthenticated && currentPrincipal.Identity.IsAuthenticated) || !principal.IsAuthenticated))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			throw new SecurityException("Demand for principal refused.");
		}
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="elem">The XML encoding to use to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="elem" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="elem" /> parameter is not a valid permission element.-or- The <paramref name="elem" /> parameter's version number is not valid. </exception>
	public void FromXml(SecurityElement elem)
	{
		CheckSecurityElement(elem, "elem", 1, 1);
		principals.Clear();
		if (elem.Children == null)
		{
			return;
		}
		foreach (SecurityElement child in elem.Children)
		{
			if (child.Tag != "Identity")
			{
				throw new ArgumentException("not IPermission/Identity");
			}
			string name = child.Attribute("ID");
			string role = child.Attribute("Role");
			string text = child.Attribute("Authenticated");
			bool isAuthenticated = false;
			if (text != null)
			{
				try
				{
					isAuthenticated = bool.Parse(text);
				}
				catch
				{
				}
			}
			PrincipalInfo value = new PrincipalInfo(name, role, isAuthenticated);
			principals.Add(value);
		}
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission will be null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not an instance of the same class as the current permission. </exception>
	public IPermission Intersect(IPermission target)
	{
		PrincipalPermission principalPermission = Cast(target);
		if (principalPermission == null)
		{
			return null;
		}
		if (IsUnrestricted())
		{
			return principalPermission.Copy();
		}
		if (principalPermission.IsUnrestricted())
		{
			return Copy();
		}
		PrincipalPermission principalPermission2 = new PrincipalPermission(PermissionState.None);
		foreach (PrincipalInfo principal in principals)
		{
			foreach (PrincipalInfo principal2 in principalPermission.principals)
			{
				if (principal.IsAuthenticated == principal2.IsAuthenticated)
				{
					string text = null;
					if (principal.Name == principal2.Name || principal2.Name == null)
					{
						text = principal.Name;
					}
					else if (principal.Name == null)
					{
						text = principal2.Name;
					}
					string text2 = null;
					if (principal.Role == principal2.Role || principal2.Role == null)
					{
						text2 = principal.Role;
					}
					else if (principal.Role == null)
					{
						text2 = principal2.Role;
					}
					if (text != null || text2 != null)
					{
						PrincipalInfo value = new PrincipalInfo(text, text2, principal.IsAuthenticated);
						principalPermission2.principals.Add(value);
					}
				}
			}
		}
		if (principalPermission2.principals.Count <= 0)
		{
			return null;
		}
		return principalPermission2;
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is an object that is not of the same type as the current permission. </exception>
	public bool IsSubsetOf(IPermission target)
	{
		PrincipalPermission principalPermission = Cast(target);
		if (principalPermission == null)
		{
			return IsEmpty();
		}
		if (IsUnrestricted())
		{
			return principalPermission.IsUnrestricted();
		}
		if (principalPermission.IsUnrestricted())
		{
			return true;
		}
		foreach (PrincipalInfo principal in principals)
		{
			bool flag = false;
			foreach (PrincipalInfo principal2 in principalPermission.principals)
			{
				if ((principal.Name == principal2.Name || principal2.Name == null) && (principal.Role == principal2.Role || principal2.Role == null) && principal.IsAuthenticated == principal2.IsAuthenticated)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns a value indicating whether the current permission is unrestricted.</summary>
	/// <returns>true if the current permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		foreach (PrincipalInfo principal in principals)
		{
			if (principal.Name == null && principal.Role == null && principal.IsAuthenticated)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Creates and returns a string representing the current permission.</summary>
	/// <returns>A representation of the current permission.</returns>
	public override string ToString()
	{
		return ToXml().ToString();
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	public SecurityElement ToXml()
	{
		SecurityElement securityElement = new SecurityElement("Permission");
		Type type = GetType();
		securityElement.AddAttribute("class", type.FullName + ", " + type.Assembly.ToString().Replace('"', '\''));
		securityElement.AddAttribute("version", 1.ToString());
		foreach (PrincipalInfo principal in principals)
		{
			SecurityElement securityElement2 = new SecurityElement("Identity");
			if (principal.Name != null)
			{
				securityElement2.AddAttribute("ID", principal.Name);
			}
			if (principal.Role != null)
			{
				securityElement2.AddAttribute("Role", principal.Role);
			}
			if (principal.IsAuthenticated)
			{
				securityElement2.AddAttribute("Authenticated", "true");
			}
			securityElement.AddChild(securityElement2);
		}
		return securityElement;
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="other">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="other" /> parameter is an object that is not of the same type as the current permission. </exception>
	public IPermission Union(IPermission other)
	{
		PrincipalPermission principalPermission = Cast(other);
		if (principalPermission == null)
		{
			return Copy();
		}
		if (IsUnrestricted() || principalPermission.IsUnrestricted())
		{
			return new PrincipalPermission(PermissionState.Unrestricted);
		}
		PrincipalPermission principalPermission2 = new PrincipalPermission(principals);
		foreach (PrincipalInfo principal in principalPermission.principals)
		{
			principalPermission2.principals.Add(principal);
		}
		return principalPermission2;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Security.Permissions.PrincipalPermission" /> object is equal to the current <see cref="T:System.Security.Permissions.PrincipalPermission" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Security.Permissions.PrincipalPermission" /> is equal to the current <see cref="T:System.Security.Permissions.PrincipalPermission" /> object; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Security.Permissions.PrincipalPermission" /> object to compare with the current <see cref="T:System.Security.Permissions.PrincipalPermission" />. </param>
	[ComVisible(false)]
	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is PrincipalPermission principalPermission))
		{
			return false;
		}
		if (principals.Count != principalPermission.principals.Count)
		{
			return false;
		}
		foreach (PrincipalInfo principal in principals)
		{
			bool flag = false;
			foreach (PrincipalInfo principal2 in principalPermission.principals)
			{
				if ((principal.Name == principal2.Name || principal2.Name == null) && (principal.Role == principal2.Role || principal2.Role == null) && principal.IsAuthenticated == principal2.IsAuthenticated)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Gets a hash code for the <see cref="T:System.Security.Permissions.PrincipalPermission" /> object that is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Security.Permissions.PrincipalPermission" /> object.</returns>
	[ComVisible(false)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 8;
	}

	private PrincipalPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		PrincipalPermission obj = target as PrincipalPermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(PrincipalPermission));
		}
		return obj;
	}

	private bool IsEmpty()
	{
		return principals.Count == 0;
	}

	internal int CheckSecurityElement(SecurityElement se, string parameterName, int minimumVersion, int maximumVersion)
	{
		if (se == null)
		{
			throw new ArgumentNullException(parameterName);
		}
		if (se.Tag != "Permission")
		{
			throw new ArgumentException(string.Format(Locale.GetText("Invalid tag {0}"), se.Tag), parameterName);
		}
		int num = minimumVersion;
		string text = se.Attribute("version");
		if (text != null)
		{
			try
			{
				num = int.Parse(text);
			}
			catch (Exception innerException)
			{
				throw new ArgumentException(string.Format(Locale.GetText("Couldn't parse version from '{0}'."), text), parameterName, innerException);
			}
		}
		if (num < minimumVersion || num > maximumVersion)
		{
			throw new ArgumentException(string.Format(Locale.GetText("Unknown version '{0}', expected versions between ['{1}','{2}']."), num, minimumVersion, maximumVersion), parameterName);
		}
		return num;
	}
}
