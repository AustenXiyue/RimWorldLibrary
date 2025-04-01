using System.Collections;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;

namespace System.Security.Permissions;

/// <summary>Controls the ability to access registry variables. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class RegistryPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
{
	private const int version = 1;

	private PermissionState _state;

	private ArrayList createList;

	private ArrayList readList;

	private ArrayList writeList;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.RegistryPermission" /> class with either fully restricted or unrestricted permission as specified.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public RegistryPermission(PermissionState state)
	{
		_state = CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: true);
		createList = new ArrayList();
		readList = new ArrayList();
		writeList = new ArrayList();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.RegistryPermission" /> class with the specified access to the specified registry variables.</summary>
	/// <param name="access">One of the <see cref="T:System.Security.Permissions.RegistryPermissionAccess" /> values. </param>
	/// <param name="pathList">A list of registry variables (semicolon-separated) to which access is granted. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="access" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.RegistryPermissionAccess" />.-or- The <paramref name="pathList" /> parameter is not a valid string. </exception>
	public RegistryPermission(RegistryPermissionAccess access, string pathList)
	{
		_state = PermissionState.None;
		createList = new ArrayList();
		readList = new ArrayList();
		writeList = new ArrayList();
		AddPathList(access, pathList);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.RegistryPermission" /> class with the specified access to the specified registry variables and the specified access rights to registry control information.</summary>
	/// <param name="access">One of the <see cref="T:System.Security.Permissions.RegistryPermissionAccess" /> values.</param>
	/// <param name="control">A bitwise combination of the <see cref="T:System.Security.AccessControl.AccessControlActions" />  values.</param>
	/// <param name="pathList">A list of registry variables (semicolon-separated) to which access is granted.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="access" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.RegistryPermissionAccess" />.-or- The <paramref name="pathList" /> parameter is not a valid string. </exception>
	public RegistryPermission(RegistryPermissionAccess access, AccessControlActions control, string pathList)
	{
		if (!Enum.IsDefined(typeof(AccessControlActions), control))
		{
			throw new ArgumentException(string.Format(Locale.GetText("Invalid enum {0}"), control), "AccessControlActions");
		}
		_state = PermissionState.None;
		AddPathList(access, control, pathList);
	}

	/// <summary>Adds access for the specified registry variables to the existing state of the permission.</summary>
	/// <param name="access">One of the <see cref="T:System.Security.Permissions.RegistryPermissionAccess" /> values. </param>
	/// <param name="pathList">A list of registry variables (semicolon-separated). </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="access" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.RegistryPermissionAccess" />.-or- The <paramref name="pathList" /> parameter is not a valid string. </exception>
	public void AddPathList(RegistryPermissionAccess access, string pathList)
	{
		if (pathList == null)
		{
			throw new ArgumentNullException("pathList");
		}
		switch (access)
		{
		case RegistryPermissionAccess.AllAccess:
			AddWithUnionKey(createList, pathList);
			AddWithUnionKey(readList, pathList);
			AddWithUnionKey(writeList, pathList);
			break;
		case RegistryPermissionAccess.Create:
			AddWithUnionKey(createList, pathList);
			break;
		case RegistryPermissionAccess.Read:
			AddWithUnionKey(readList, pathList);
			break;
		case RegistryPermissionAccess.Write:
			AddWithUnionKey(writeList, pathList);
			break;
		default:
			ThrowInvalidFlag(access, context: false);
			break;
		case RegistryPermissionAccess.NoAccess:
			break;
		}
	}

	/// <summary>Adds access for the specified registry variables to the existing state of the permission, specifying registry permission access and access control actions.</summary>
	/// <param name="access">One of the <see cref="T:System.Security.Permissions.RegistryPermissionAccess" /> values. </param>
	/// <param name="control">One of the <see cref="T:System.Security.AccessControl.AccessControlActions" /> values. </param>
	/// <param name="pathList">A list of registry variables (separated by semicolons).</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="access" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.RegistryPermissionAccess" />.-or- The <paramref name="pathList" /> parameter is not a valid string. </exception>
	[MonoTODO("(2.0) Access Control isn't implemented")]
	public void AddPathList(RegistryPermissionAccess access, AccessControlActions control, string pathList)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets paths for all registry variables with the specified <see cref="T:System.Security.Permissions.RegistryPermissionAccess" />.</summary>
	/// <returns>A list of the registry variables (semicolon-separated) with the specified <see cref="T:System.Security.Permissions.RegistryPermissionAccess" />.</returns>
	/// <param name="access">One of the <see cref="T:System.Security.Permissions.RegistryPermissionAccess" /> values that represents a single type of registry variable access. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="access" /> is not a valid value of <see cref="T:System.Security.Permissions.RegistryPermissionAccess" />.-or- <paramref name="access" /> is <see cref="F:System.Security.Permissions.RegistryPermissionAccess.AllAccess" />, which represents more than one type of registry variable access, or <see cref="F:System.Security.Permissions.RegistryPermissionAccess.NoAccess" />, which does not represent any type of registry variable access. </exception>
	public string GetPathList(RegistryPermissionAccess access)
	{
		switch (access)
		{
		case RegistryPermissionAccess.NoAccess:
		case RegistryPermissionAccess.AllAccess:
			ThrowInvalidFlag(access, context: true);
			break;
		case RegistryPermissionAccess.Create:
			return GetPathList(createList);
		case RegistryPermissionAccess.Read:
			return GetPathList(readList);
		case RegistryPermissionAccess.Write:
			return GetPathList(writeList);
		default:
			ThrowInvalidFlag(access, context: false);
			break;
		}
		return null;
	}

	/// <summary>Sets new access for the specified registry variable names to the existing state of the permission.</summary>
	/// <param name="access">One of the <see cref="T:System.Security.Permissions.RegistryPermissionAccess" /> values. </param>
	/// <param name="pathList">A list of registry variables (semicolon-separated). </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="access" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.RegistryPermissionAccess" />.-or- The <paramref name="pathList" /> parameter is not a valid string. </exception>
	public void SetPathList(RegistryPermissionAccess access, string pathList)
	{
		if (pathList == null)
		{
			throw new ArgumentNullException("pathList");
		}
		switch (access)
		{
		case RegistryPermissionAccess.AllAccess:
		{
			createList.Clear();
			readList.Clear();
			writeList.Clear();
			string[] array = pathList.Split(';');
			foreach (string value4 in array)
			{
				createList.Add(value4);
				readList.Add(value4);
				writeList.Add(value4);
			}
			break;
		}
		case RegistryPermissionAccess.Create:
		{
			createList.Clear();
			string[] array = pathList.Split(';');
			foreach (string value2 in array)
			{
				createList.Add(value2);
			}
			break;
		}
		case RegistryPermissionAccess.Read:
		{
			readList.Clear();
			string[] array = pathList.Split(';');
			foreach (string value3 in array)
			{
				readList.Add(value3);
			}
			break;
		}
		case RegistryPermissionAccess.Write:
		{
			writeList.Clear();
			string[] array = pathList.Split(';');
			foreach (string value in array)
			{
				writeList.Add(value);
			}
			break;
		}
		default:
			ThrowInvalidFlag(access, context: false);
			break;
		case RegistryPermissionAccess.NoAccess:
			break;
		}
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		RegistryPermission registryPermission = new RegistryPermission(_state);
		string pathList = GetPathList(RegistryPermissionAccess.Create);
		if (pathList != null)
		{
			registryPermission.SetPathList(RegistryPermissionAccess.Create, pathList);
		}
		pathList = GetPathList(RegistryPermissionAccess.Read);
		if (pathList != null)
		{
			registryPermission.SetPathList(RegistryPermissionAccess.Read, pathList);
		}
		pathList = GetPathList(RegistryPermissionAccess.Write);
		if (pathList != null)
		{
			registryPermission.SetPathList(RegistryPermissionAccess.Write, pathList);
		}
		return registryPermission;
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="esd">The XML encoding to use to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="esd" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="esd" /> parameter is not a valid permission element.-or- The <paramref name="esd" /> parameter's version number is not valid. </exception>
	public override void FromXml(SecurityElement esd)
	{
		CodeAccessPermission.CheckSecurityElement(esd, "esd", 1, 1);
		CodeAccessPermission.CheckSecurityElement(esd, "esd", 1, 1);
		if (CodeAccessPermission.IsUnrestricted(esd))
		{
			_state = PermissionState.Unrestricted;
		}
		string text = esd.Attribute("Create");
		if (text != null && text.Length > 0)
		{
			SetPathList(RegistryPermissionAccess.Create, text);
		}
		string text2 = esd.Attribute("Read");
		if (text2 != null && text2.Length > 0)
		{
			SetPathList(RegistryPermissionAccess.Read, text2);
		}
		string text3 = esd.Attribute("Write");
		if (text3 != null && text3.Length > 0)
		{
			SetPathList(RegistryPermissionAccess.Write, text3);
		}
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	[SecuritySafeCritical]
	public override IPermission Intersect(IPermission target)
	{
		RegistryPermission registryPermission = Cast(target);
		if (registryPermission == null)
		{
			return null;
		}
		if (IsUnrestricted())
		{
			return registryPermission.Copy();
		}
		if (registryPermission.IsUnrestricted())
		{
			return Copy();
		}
		RegistryPermission registryPermission2 = new RegistryPermission(PermissionState.None);
		IntersectKeys(createList, registryPermission.createList, registryPermission2.createList);
		IntersectKeys(readList, registryPermission.readList, registryPermission2.readList);
		IntersectKeys(writeList, registryPermission.writeList, registryPermission2.writeList);
		if (!registryPermission2.IsEmpty())
		{
			return registryPermission2;
		}
		return null;
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	[SecuritySafeCritical]
	public override bool IsSubsetOf(IPermission target)
	{
		RegistryPermission registryPermission = Cast(target);
		if (registryPermission == null)
		{
			return false;
		}
		if (registryPermission.IsEmpty())
		{
			return IsEmpty();
		}
		if (IsUnrestricted())
		{
			return registryPermission.IsUnrestricted();
		}
		if (registryPermission.IsUnrestricted())
		{
			return true;
		}
		if (!KeyIsSubsetOf(createList, registryPermission.createList))
		{
			return false;
		}
		if (!KeyIsSubsetOf(readList, registryPermission.readList))
		{
			return false;
		}
		if (!KeyIsSubsetOf(writeList, registryPermission.writeList))
		{
			return false;
		}
		return true;
	}

	/// <summary>Returns a value indicating whether the current permission is unrestricted.</summary>
	/// <returns>true if the current permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return _state == PermissionState.Unrestricted;
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	[SecuritySafeCritical]
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = Element(1);
		if (_state == PermissionState.Unrestricted)
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		else
		{
			string pathList = GetPathList(RegistryPermissionAccess.Create);
			if (pathList != null)
			{
				securityElement.AddAttribute("Create", pathList);
			}
			pathList = GetPathList(RegistryPermissionAccess.Read);
			if (pathList != null)
			{
				securityElement.AddAttribute("Read", pathList);
			}
			pathList = GetPathList(RegistryPermissionAccess.Write);
			if (pathList != null)
			{
				securityElement.AddAttribute("Write", pathList);
			}
		}
		return securityElement;
	}

	/// <summary>Creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="other">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="other" /> parameter is not null and is not of the same type as the current permission. </exception>
	[SecuritySafeCritical]
	public override IPermission Union(IPermission other)
	{
		RegistryPermission registryPermission = Cast(other);
		if (registryPermission == null)
		{
			return Copy();
		}
		if (IsUnrestricted() || registryPermission.IsUnrestricted())
		{
			return new RegistryPermission(PermissionState.Unrestricted);
		}
		if (IsEmpty() && registryPermission.IsEmpty())
		{
			return null;
		}
		RegistryPermission registryPermission2 = (RegistryPermission)Copy();
		string pathList = registryPermission.GetPathList(RegistryPermissionAccess.Create);
		if (pathList != null)
		{
			registryPermission2.AddPathList(RegistryPermissionAccess.Create, pathList);
		}
		pathList = registryPermission.GetPathList(RegistryPermissionAccess.Read);
		if (pathList != null)
		{
			registryPermission2.AddPathList(RegistryPermissionAccess.Read, pathList);
		}
		pathList = registryPermission.GetPathList(RegistryPermissionAccess.Write);
		if (pathList != null)
		{
			registryPermission2.AddPathList(RegistryPermissionAccess.Write, pathList);
		}
		return registryPermission2;
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 5;
	}

	private bool IsEmpty()
	{
		if (_state == PermissionState.None && createList.Count == 0 && readList.Count == 0)
		{
			return writeList.Count == 0;
		}
		return false;
	}

	private RegistryPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		RegistryPermission obj = target as RegistryPermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(RegistryPermission));
		}
		return obj;
	}

	internal void ThrowInvalidFlag(RegistryPermissionAccess flag, bool context)
	{
		string text = null;
		text = ((!context) ? Locale.GetText("Invalid flag '{0}' in this context.") : Locale.GetText("Unknown flag '{0}'."));
		throw new ArgumentException(string.Format(text, flag), "flag");
	}

	private string GetPathList(ArrayList list)
	{
		if (IsUnrestricted())
		{
			return string.Empty;
		}
		if (list.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in list)
		{
			stringBuilder.Append(item);
			stringBuilder.Append(";");
		}
		string text = stringBuilder.ToString();
		int length = text.Length;
		if (length > 0)
		{
			return text.Substring(0, length - 1);
		}
		return string.Empty;
	}

	internal bool KeyIsSubsetOf(IList local, IList target)
	{
		bool flag = false;
		foreach (string item in local)
		{
			foreach (string item2 in target)
			{
				if (item.StartsWith(item2))
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

	internal void AddWithUnionKey(IList list, string pathList)
	{
		string[] array = pathList.Split(';');
		foreach (string text in array)
		{
			int count = list.Count;
			if (count == 0)
			{
				list.Add(text);
				continue;
			}
			for (int j = 0; j < count; j++)
			{
				string text2 = (string)list[j];
				if (text2.StartsWith(text))
				{
					list[j] = text;
				}
				else if (!text.StartsWith(text2))
				{
					list.Add(text);
				}
			}
		}
	}

	internal void IntersectKeys(IList local, IList target, IList result)
	{
		foreach (string item in local)
		{
			foreach (string item2 in target)
			{
				if (item2.Length > item.Length)
				{
					if (item2.StartsWith(item))
					{
						result.Add(item2);
					}
				}
				else if (item.StartsWith(item2))
				{
					result.Add(item);
				}
			}
		}
	}
}
