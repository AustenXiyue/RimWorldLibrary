using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Defines the identity permission for the Web site from which the code originates. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class SiteIdentityPermission : CodeAccessPermission, IBuiltInPermission
{
	private const int version = 1;

	private string _site;

	private static bool[] valid = new bool[94]
	{
		true, false, true, true, true, true, true, true, true, true,
		false, false, true, true, false, true, true, true, true, true,
		true, true, true, true, false, false, false, false, false, false,
		false, true, true, true, true, true, true, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, true, true, true, true, true, true, true, false, false,
		false, true, true, false, true, true, true, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, true, true, true, true, true, true, true, true, true,
		true, false, true, true
	};

	/// <summary>Gets or sets the current site.</summary>
	/// <returns>The current site.</returns>
	/// <exception cref="T:System.NotSupportedException">The site identity cannot be retrieved because it has an ambiguous identity.</exception>
	public string Site
	{
		get
		{
			if (IsEmpty())
			{
				throw new NullReferenceException("No site.");
			}
			return _site;
		}
		set
		{
			if (!IsValid(value))
			{
				throw new ArgumentException("Invalid site.");
			}
			_site = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.SiteIdentityPermission" /> class with the specified <see cref="T:System.Security.Permissions.PermissionState" />.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public SiteIdentityPermission(PermissionState state)
	{
		CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: false);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.SiteIdentityPermission" /> class to represent the specified site identity.</summary>
	/// <param name="site">The site name or wildcard expression. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="site" /> parameter is not a valid string, or does not match a valid wildcard site name. </exception>
	public SiteIdentityPermission(string site)
	{
		Site = site;
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		if (IsEmpty())
		{
			return new SiteIdentityPermission(PermissionState.None);
		}
		return new SiteIdentityPermission(_site);
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="esd">The XML encoding to use to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="esd" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="esd" /> parameter is not a valid permission element.-or- The <paramref name="esd" /> parameter's version number is not valid. </exception>
	public override void FromXml(SecurityElement esd)
	{
		CodeAccessPermission.CheckSecurityElement(esd, "esd", 1, 1);
		string text = esd.Attribute("Site");
		if (text != null)
		{
			Site = text;
		}
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override IPermission Intersect(IPermission target)
	{
		SiteIdentityPermission siteIdentityPermission = Cast(target);
		if (siteIdentityPermission == null || IsEmpty())
		{
			return null;
		}
		if (Match(siteIdentityPermission._site))
		{
			return new SiteIdentityPermission((_site.Length > siteIdentityPermission._site.Length) ? _site : siteIdentityPermission._site);
		}
		return null;
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		SiteIdentityPermission siteIdentityPermission = Cast(target);
		if (siteIdentityPermission == null)
		{
			return IsEmpty();
		}
		if (_site == null && siteIdentityPermission._site == null)
		{
			return true;
		}
		if (_site == null || siteIdentityPermission._site == null)
		{
			return false;
		}
		int num = siteIdentityPermission._site.IndexOf('*');
		if (num == -1)
		{
			return _site == siteIdentityPermission._site;
		}
		return _site.EndsWith(siteIdentityPermission._site.Substring(num + 1));
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = Element(1);
		if (_site != null)
		{
			securityElement.AddAttribute("Site", _site);
		}
		return securityElement;
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. -or-The permissions are not equal and one is not a subset of the other.</exception>
	public override IPermission Union(IPermission target)
	{
		SiteIdentityPermission siteIdentityPermission = Cast(target);
		if (siteIdentityPermission == null || siteIdentityPermission.IsEmpty())
		{
			return Copy();
		}
		if (IsEmpty())
		{
			return siteIdentityPermission.Copy();
		}
		if (Match(siteIdentityPermission._site))
		{
			return new SiteIdentityPermission((_site.Length < siteIdentityPermission._site.Length) ? _site : siteIdentityPermission._site);
		}
		throw new ArgumentException(Locale.GetText("Cannot union two different sites."), "target");
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 11;
	}

	private bool IsEmpty()
	{
		return _site == null;
	}

	private SiteIdentityPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		SiteIdentityPermission obj = target as SiteIdentityPermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(SiteIdentityPermission));
		}
		return obj;
	}

	private bool IsValid(string s)
	{
		if (s == null || s.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < s.Length; i++)
		{
			ushort num = s[i];
			switch (num)
			{
			default:
				return false;
			case 42:
				if (s.Length > 1 && (s[i + 1] != '.' || i > 0))
				{
					return false;
				}
				break;
			case 33:
			case 34:
			case 35:
			case 36:
			case 37:
			case 38:
			case 39:
			case 40:
			case 41:
			case 43:
			case 44:
			case 45:
			case 46:
			case 47:
			case 48:
			case 49:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 56:
			case 57:
			case 58:
			case 59:
			case 60:
			case 61:
			case 62:
			case 63:
			case 64:
			case 65:
			case 66:
			case 67:
			case 68:
			case 69:
			case 70:
			case 71:
			case 72:
			case 73:
			case 74:
			case 75:
			case 76:
			case 77:
			case 78:
			case 79:
			case 80:
			case 81:
			case 82:
			case 83:
			case 84:
			case 85:
			case 86:
			case 87:
			case 88:
			case 89:
			case 90:
			case 91:
			case 92:
			case 93:
			case 94:
			case 95:
			case 96:
			case 97:
			case 98:
			case 99:
			case 100:
			case 101:
			case 102:
			case 103:
			case 104:
			case 105:
			case 106:
			case 107:
			case 108:
			case 109:
			case 110:
			case 111:
			case 112:
			case 113:
			case 114:
			case 115:
			case 116:
			case 117:
			case 118:
			case 119:
			case 120:
			case 121:
			case 122:
			case 123:
			case 124:
			case 125:
			case 126:
				break;
			}
			if (!valid[num - 33])
			{
				return false;
			}
		}
		if (s.Length == 1)
		{
			return s[0] != '.';
		}
		return true;
	}

	private bool Match(string target)
	{
		if (_site == null || target == null)
		{
			return false;
		}
		int num = _site.IndexOf('*');
		int num2 = target.IndexOf('*');
		if (num == -1 && num2 == -1)
		{
			return _site == target;
		}
		if (num == -1)
		{
			return _site.EndsWith(target.Substring(num2 + 1));
		}
		if (num2 == -1)
		{
			return target.EndsWith(_site.Substring(num + 1));
		}
		string text = _site.Substring(num + 1);
		target = target.Substring(num2 + 1);
		if (text.Length > target.Length)
		{
			return text.EndsWith(target);
		}
		return target.EndsWith(text);
	}
}
