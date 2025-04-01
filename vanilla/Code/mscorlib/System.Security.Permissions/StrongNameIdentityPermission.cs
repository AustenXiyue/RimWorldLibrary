using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Defines the identity permission for strong names. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class StrongNameIdentityPermission : CodeAccessPermission, IBuiltInPermission
{
	private struct SNIP
	{
		public StrongNamePublicKeyBlob PublicKey;

		public string Name;

		public Version AssemblyVersion;

		internal SNIP(StrongNamePublicKeyBlob pk, string name, Version version)
		{
			PublicKey = pk;
			Name = name;
			AssemblyVersion = version;
		}

		internal static SNIP CreateDefault()
		{
			return new SNIP(null, string.Empty, (Version)defaultVersion.Clone());
		}

		internal bool IsNameSubsetOf(string target)
		{
			if (Name == null)
			{
				return target == null;
			}
			if (target == null)
			{
				return true;
			}
			int num = Name.LastIndexOf('*');
			switch (num)
			{
			case 0:
				return true;
			case -1:
				num = Name.Length;
				break;
			}
			return string.Compare(Name, 0, target, 0, num, ignoreCase: true, CultureInfo.InvariantCulture) == 0;
		}

		internal bool IsSubsetOf(SNIP target)
		{
			if (PublicKey != null && PublicKey.Equals(target.PublicKey))
			{
				return true;
			}
			if (!IsNameSubsetOf(target.Name))
			{
				return false;
			}
			if (AssemblyVersion != null && !AssemblyVersion.Equals(target.AssemblyVersion))
			{
				return false;
			}
			if (PublicKey == null)
			{
				return target.PublicKey == null;
			}
			return false;
		}
	}

	private const int version = 1;

	private static Version defaultVersion = new Version(0, 0);

	private PermissionState _state;

	private ArrayList _list;

	/// <summary>Gets or sets the simple name portion of the strong name identity.</summary>
	/// <returns>The simple name of the identity.</returns>
	/// <exception cref="T:System.ArgumentException">The value is an empty string ("").</exception>
	/// <exception cref="T:System.NotSupportedException">The property value cannot be retrieved because it contains an ambiguous identity. </exception>
	public string Name
	{
		get
		{
			if (_list.Count > 1)
			{
				throw new NotSupportedException();
			}
			return ((SNIP)_list[0]).Name;
		}
		set
		{
			if (value != null && value.Length == 0)
			{
				throw new ArgumentException("name");
			}
			if (_list.Count > 1)
			{
				ResetToDefault();
			}
			SNIP sNIP = (SNIP)_list[0];
			sNIP.Name = value;
			_list[0] = sNIP;
		}
	}

	/// <summary>Gets or sets the public key blob that defines the strong name identity namespace.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.StrongNamePublicKeyBlob" /> that contains the public key of the identity, or null if there is no key.</returns>
	/// <exception cref="T:System.ArgumentNullException">The property value is set to null. </exception>
	/// <exception cref="T:System.NotSupportedException">The property value cannot be retrieved because it contains an ambiguous identity. </exception>
	public StrongNamePublicKeyBlob PublicKey
	{
		get
		{
			if (_list.Count > 1)
			{
				throw new NotSupportedException();
			}
			return ((SNIP)_list[0]).PublicKey;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (_list.Count > 1)
			{
				ResetToDefault();
			}
			SNIP sNIP = (SNIP)_list[0];
			sNIP.PublicKey = value;
			_list[0] = sNIP;
		}
	}

	/// <summary>Gets or sets the version number of the identity.</summary>
	/// <returns>The version of the identity.</returns>
	/// <exception cref="T:System.NotSupportedException">The property value cannot be retrieved because it contains an ambiguous identity. </exception>
	public Version Version
	{
		get
		{
			if (_list.Count > 1)
			{
				throw new NotSupportedException();
			}
			return ((SNIP)_list[0]).AssemblyVersion;
		}
		set
		{
			if (_list.Count > 1)
			{
				ResetToDefault();
			}
			SNIP sNIP = (SNIP)_list[0];
			sNIP.AssemblyVersion = value;
			_list[0] = sNIP;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.StrongNameIdentityPermission" /> class with the specified <see cref="T:System.Security.Permissions.PermissionState" />.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public StrongNameIdentityPermission(PermissionState state)
	{
		_state = CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: true);
		_list = new ArrayList();
		_list.Add(SNIP.CreateDefault());
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.StrongNameIdentityPermission" /> class for the specified strong name identity.</summary>
	/// <param name="blob">The public key defining the strong name identity namespace. </param>
	/// <param name="name">The simple name part of the strong name identity. This corresponds to the name of the assembly. </param>
	/// <param name="version">The version number of the identity. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="blob" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="name" /> parameter is an empty string ("").</exception>
	public StrongNameIdentityPermission(StrongNamePublicKeyBlob blob, string name, Version version)
	{
		if (blob == null)
		{
			throw new ArgumentNullException("blob");
		}
		if (name != null && name.Length == 0)
		{
			throw new ArgumentException("name");
		}
		_state = PermissionState.None;
		_list = new ArrayList();
		_list.Add(new SNIP(blob, name, version));
	}

	internal StrongNameIdentityPermission(StrongNameIdentityPermission snip)
	{
		_state = snip._state;
		_list = new ArrayList(snip._list.Count);
		foreach (SNIP item in snip._list)
		{
			_list.Add(new SNIP(item.PublicKey, item.Name, item.AssemblyVersion));
		}
	}

	internal void ResetToDefault()
	{
		_list.Clear();
		_list.Add(SNIP.CreateDefault());
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		if (IsEmpty())
		{
			return new StrongNameIdentityPermission(PermissionState.None);
		}
		return new StrongNameIdentityPermission(this);
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="e">The XML encoding to use to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="e" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="e" /> parameter is not a valid permission element.-or- The <paramref name="e" /> parameter's version number is not valid. </exception>
	public override void FromXml(SecurityElement e)
	{
		CodeAccessPermission.CheckSecurityElement(e, "e", 1, 1);
		_list.Clear();
		if (e.Children != null && e.Children.Count > 0)
		{
			foreach (SecurityElement child in e.Children)
			{
				_list.Add(FromSecurityElement(child));
			}
			return;
		}
		_list.Add(FromSecurityElement(e));
	}

	private SNIP FromSecurityElement(SecurityElement se)
	{
		string name = se.Attribute("Name");
		StrongNamePublicKeyBlob pk = StrongNamePublicKeyBlob.FromString(se.Attribute("PublicKeyBlob"));
		string text = se.Attribute("AssemblyVersion");
		Version version = ((text == null) ? null : new Version(text));
		return new SNIP(pk, name, version);
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission, or null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override IPermission Intersect(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		if (!(target is StrongNameIdentityPermission strongNameIdentityPermission))
		{
			throw new ArgumentException(Locale.GetText("Wrong permission type."));
		}
		if (IsEmpty() || strongNameIdentityPermission.IsEmpty())
		{
			return null;
		}
		if (!Match(strongNameIdentityPermission.Name))
		{
			return null;
		}
		string name = ((Name.Length < strongNameIdentityPermission.Name.Length) ? Name : strongNameIdentityPermission.Name);
		if (!Version.Equals(strongNameIdentityPermission.Version))
		{
			return null;
		}
		if (!PublicKey.Equals(strongNameIdentityPermission.PublicKey))
		{
			return null;
		}
		return new StrongNameIdentityPermission(PublicKey, name, Version);
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		StrongNameIdentityPermission strongNameIdentityPermission = Cast(target);
		if (strongNameIdentityPermission == null)
		{
			return IsEmpty();
		}
		if (IsEmpty())
		{
			return true;
		}
		if (IsUnrestricted())
		{
			return strongNameIdentityPermission.IsUnrestricted();
		}
		if (strongNameIdentityPermission.IsUnrestricted())
		{
			return true;
		}
		foreach (SNIP item in _list)
		{
			foreach (SNIP item2 in strongNameIdentityPermission._list)
			{
				if (!item.IsSubsetOf(item2))
				{
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = Element(1);
		if (_list.Count > 1)
		{
			foreach (SNIP item in _list)
			{
				SecurityElement securityElement2 = new SecurityElement("StrongName");
				ToSecurityElement(securityElement2, item);
				securityElement.AddChild(securityElement2);
			}
		}
		else if (_list.Count == 1)
		{
			SNIP snip2 = (SNIP)_list[0];
			if (!IsEmpty(snip2))
			{
				ToSecurityElement(securityElement, snip2);
			}
		}
		return securityElement;
	}

	private void ToSecurityElement(SecurityElement se, SNIP snip)
	{
		if (snip.PublicKey != null)
		{
			se.AddAttribute("PublicKeyBlob", snip.PublicKey.ToString());
		}
		if (snip.Name != null)
		{
			se.AddAttribute("Name", snip.Name);
		}
		if (snip.AssemblyVersion != null)
		{
			se.AddAttribute("AssemblyVersion", snip.AssemblyVersion.ToString());
		}
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. -or-The two permissions are not equal and one is a subset of the other.</exception>
	public override IPermission Union(IPermission target)
	{
		StrongNameIdentityPermission strongNameIdentityPermission = Cast(target);
		if (strongNameIdentityPermission == null || strongNameIdentityPermission.IsEmpty())
		{
			return Copy();
		}
		if (IsEmpty())
		{
			return strongNameIdentityPermission.Copy();
		}
		StrongNameIdentityPermission strongNameIdentityPermission2 = (StrongNameIdentityPermission)Copy();
		foreach (SNIP item in strongNameIdentityPermission._list)
		{
			if (!IsEmpty(item) && !Contains(item))
			{
				strongNameIdentityPermission2._list.Add(item);
			}
		}
		return strongNameIdentityPermission2;
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 12;
	}

	private bool IsUnrestricted()
	{
		return _state == PermissionState.Unrestricted;
	}

	private bool Contains(SNIP snip)
	{
		foreach (SNIP item in _list)
		{
			bool num = (item.PublicKey == null && snip.PublicKey == null) || (item.PublicKey != null && item.PublicKey.Equals(snip.PublicKey));
			bool flag = item.IsNameSubsetOf(snip.Name);
			bool flag2 = (item.AssemblyVersion == null && snip.AssemblyVersion == null) || (item.AssemblyVersion != null && item.AssemblyVersion.Equals(snip.AssemblyVersion));
			if (num && flag && flag2)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsEmpty(SNIP snip)
	{
		if (PublicKey != null)
		{
			return false;
		}
		if (Name != null && Name.Length > 0)
		{
			return false;
		}
		if (!(Version == null))
		{
			return defaultVersion.Equals(Version);
		}
		return true;
	}

	private bool IsEmpty()
	{
		if (IsUnrestricted() || _list.Count > 1)
		{
			return false;
		}
		if (PublicKey != null)
		{
			return false;
		}
		if (Name != null && Name.Length > 0)
		{
			return false;
		}
		if (!(Version == null))
		{
			return defaultVersion.Equals(Version);
		}
		return true;
	}

	private StrongNameIdentityPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		StrongNameIdentityPermission obj = target as StrongNameIdentityPermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(StrongNameIdentityPermission));
		}
		return obj;
	}

	private bool Match(string target)
	{
		if (Name == null || target == null)
		{
			return false;
		}
		int num = Name.LastIndexOf('*');
		int num2 = target.LastIndexOf('*');
		int num3 = int.MaxValue;
		return string.Compare(length: (num == -1 && num2 == -1) ? Math.Max(Name.Length, target.Length) : ((num == -1) ? num2 : ((num2 != -1) ? Math.Min(num, num2) : num)), strA: Name, indexA: 0, strB: target, indexB: 0, ignoreCase: true, culture: CultureInfo.InvariantCulture) == 0;
	}
}
