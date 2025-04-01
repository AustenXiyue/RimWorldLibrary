using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Permissions;

/// <summary>Controls access to system and user environment variables. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class EnvironmentPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
{
	private const int version = 1;

	private PermissionState _state;

	private ArrayList readList;

	private ArrayList writeList;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.EnvironmentPermission" /> class with either restricted or unrestricted permission as specified.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public EnvironmentPermission(PermissionState state)
	{
		_state = CodeAccessPermission.CheckPermissionState(state, allowUnrestricted: true);
		readList = new ArrayList();
		writeList = new ArrayList();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.EnvironmentPermission" /> class with the specified access to the specified environment variables.</summary>
	/// <param name="flag">One of the <see cref="T:System.Security.Permissions.EnvironmentPermissionAccess" /> values. </param>
	/// <param name="pathList">A list of environment variables (semicolon-separated) to which access is granted. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="pathList" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="flag" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.EnvironmentPermissionAccess" />. </exception>
	public EnvironmentPermission(EnvironmentPermissionAccess flag, string pathList)
	{
		readList = new ArrayList();
		writeList = new ArrayList();
		SetPathList(flag, pathList);
	}

	/// <summary>Adds access for the specified environment variables to the existing state of the permission.</summary>
	/// <param name="flag">One of the <see cref="T:System.Security.Permissions.EnvironmentPermissionAccess" /> values. </param>
	/// <param name="pathList">A list of environment variables (semicolon-separated). </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="pathList" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="flag" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.EnvironmentPermissionAccess" />. </exception>
	public void AddPathList(EnvironmentPermissionAccess flag, string pathList)
	{
		if (pathList == null)
		{
			throw new ArgumentNullException("pathList");
		}
		switch (flag)
		{
		case EnvironmentPermissionAccess.AllAccess:
		{
			string[] array = pathList.Split(';');
			foreach (string text2 in array)
			{
				if (!readList.Contains(text2))
				{
					readList.Add(text2);
				}
				if (!writeList.Contains(text2))
				{
					writeList.Add(text2);
				}
			}
			break;
		}
		case EnvironmentPermissionAccess.Read:
		{
			string[] array = pathList.Split(';');
			foreach (string text3 in array)
			{
				if (!readList.Contains(text3))
				{
					readList.Add(text3);
				}
			}
			break;
		}
		case EnvironmentPermissionAccess.Write:
		{
			string[] array = pathList.Split(';');
			foreach (string text in array)
			{
				if (!writeList.Contains(text))
				{
					writeList.Add(text);
				}
			}
			break;
		}
		default:
			ThrowInvalidFlag(flag, context: false);
			break;
		case EnvironmentPermissionAccess.NoAccess:
			break;
		}
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		EnvironmentPermission environmentPermission = new EnvironmentPermission(_state);
		string pathList = GetPathList(EnvironmentPermissionAccess.Read);
		if (pathList != null)
		{
			environmentPermission.SetPathList(EnvironmentPermissionAccess.Read, pathList);
		}
		pathList = GetPathList(EnvironmentPermissionAccess.Write);
		if (pathList != null)
		{
			environmentPermission.SetPathList(EnvironmentPermissionAccess.Write, pathList);
		}
		return environmentPermission;
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="esd">The XML encoding to use to reconstruct the permission. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="esd" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="esd" /> parameter is not a valid permission element.-or- The <paramref name="esd" /> parameter's version number is not valid. </exception>
	public override void FromXml(SecurityElement esd)
	{
		CodeAccessPermission.CheckSecurityElement(esd, "esd", 1, 1);
		if (CodeAccessPermission.IsUnrestricted(esd))
		{
			_state = PermissionState.Unrestricted;
		}
		string text = esd.Attribute("Read");
		if (text != null && text.Length > 0)
		{
			SetPathList(EnvironmentPermissionAccess.Read, text);
		}
		string text2 = esd.Attribute("Write");
		if (text2 != null && text2.Length > 0)
		{
			SetPathList(EnvironmentPermissionAccess.Write, text2);
		}
	}

	/// <summary>Gets all environment variables with the specified <see cref="T:System.Security.Permissions.EnvironmentPermissionAccess" />.</summary>
	/// <returns>A list of environment variables (semicolon-separated) for the selected flag.</returns>
	/// <param name="flag">One of the <see cref="T:System.Security.Permissions.EnvironmentPermissionAccess" /> values that represents a single type of environment variable access. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="flag" /> is not a valid value of <see cref="T:System.Security.Permissions.EnvironmentPermissionAccess" />.-or- <paramref name="flag" /> is <see cref="F:System.Security.Permissions.EnvironmentPermissionAccess.AllAccess" />, which represents more than one type of environment variable access, or <see cref="F:System.Security.Permissions.EnvironmentPermissionAccess.NoAccess" />, which does not represent any type of environment variable access. </exception>
	public string GetPathList(EnvironmentPermissionAccess flag)
	{
		switch (flag)
		{
		case EnvironmentPermissionAccess.NoAccess:
		case EnvironmentPermissionAccess.AllAccess:
			ThrowInvalidFlag(flag, context: true);
			break;
		case EnvironmentPermissionAccess.Read:
			return GetPathList(readList);
		case EnvironmentPermissionAccess.Write:
			return GetPathList(writeList);
		default:
			ThrowInvalidFlag(flag, context: false);
			break;
		}
		return null;
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	[SecuritySafeCritical]
	public override IPermission Intersect(IPermission target)
	{
		EnvironmentPermission environmentPermission = Cast(target);
		if (environmentPermission == null)
		{
			return null;
		}
		if (IsUnrestricted())
		{
			return environmentPermission.Copy();
		}
		if (environmentPermission.IsUnrestricted())
		{
			return Copy();
		}
		int num = 0;
		EnvironmentPermission environmentPermission2 = new EnvironmentPermission(PermissionState.None);
		string pathList = environmentPermission.GetPathList(EnvironmentPermissionAccess.Read);
		if (pathList != null)
		{
			string[] array = pathList.Split(';');
			foreach (string text in array)
			{
				if (readList.Contains(text))
				{
					environmentPermission2.AddPathList(EnvironmentPermissionAccess.Read, text);
					num++;
				}
			}
		}
		string pathList2 = environmentPermission.GetPathList(EnvironmentPermissionAccess.Write);
		if (pathList2 != null)
		{
			string[] array = pathList2.Split(';');
			foreach (string text2 in array)
			{
				if (writeList.Contains(text2))
				{
					environmentPermission2.AddPathList(EnvironmentPermissionAccess.Write, text2);
					num++;
				}
			}
		}
		if (num <= 0)
		{
			return null;
		}
		return environmentPermission2;
	}

	/// <summary>Determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	[SecuritySafeCritical]
	public override bool IsSubsetOf(IPermission target)
	{
		EnvironmentPermission environmentPermission = Cast(target);
		if (environmentPermission == null)
		{
			return false;
		}
		if (IsUnrestricted())
		{
			return environmentPermission.IsUnrestricted();
		}
		if (environmentPermission.IsUnrestricted())
		{
			return true;
		}
		foreach (string read in readList)
		{
			if (!environmentPermission.readList.Contains(read))
			{
				return false;
			}
		}
		foreach (string write in writeList)
		{
			if (!environmentPermission.writeList.Contains(write))
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
		return _state == PermissionState.Unrestricted;
	}

	/// <summary>Sets the specified access to the specified environment variables to the existing state of the permission.</summary>
	/// <param name="flag">One of the <see cref="T:System.Security.Permissions.EnvironmentPermissionAccess" /> values. </param>
	/// <param name="pathList">A list of environment variables (semicolon-separated). </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="pathList" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="flag" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.EnvironmentPermissionAccess" />. </exception>
	public void SetPathList(EnvironmentPermissionAccess flag, string pathList)
	{
		if (pathList == null)
		{
			throw new ArgumentNullException("pathList");
		}
		switch (flag)
		{
		case EnvironmentPermissionAccess.AllAccess:
		{
			readList.Clear();
			writeList.Clear();
			string[] array = pathList.Split(';');
			foreach (string value2 in array)
			{
				readList.Add(value2);
				writeList.Add(value2);
			}
			break;
		}
		case EnvironmentPermissionAccess.Read:
		{
			readList.Clear();
			string[] array = pathList.Split(';');
			foreach (string value3 in array)
			{
				readList.Add(value3);
			}
			break;
		}
		case EnvironmentPermissionAccess.Write:
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
			ThrowInvalidFlag(flag, context: false);
			break;
		case EnvironmentPermissionAccess.NoAccess:
			break;
		}
	}

	/// <summary>Creates an XML encoding of the permission and its current state.</summary>
	/// <returns>An XML encoding of the permission, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = Element(1);
		if (_state == PermissionState.Unrestricted)
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		else
		{
			string pathList = GetPathList(EnvironmentPermissionAccess.Read);
			if (pathList != null)
			{
				securityElement.AddAttribute("Read", pathList);
			}
			pathList = GetPathList(EnvironmentPermissionAccess.Write);
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
		EnvironmentPermission environmentPermission = Cast(other);
		if (environmentPermission == null)
		{
			return Copy();
		}
		if (IsUnrestricted() || environmentPermission.IsUnrestricted())
		{
			return new EnvironmentPermission(PermissionState.Unrestricted);
		}
		if (IsEmpty() && environmentPermission.IsEmpty())
		{
			return null;
		}
		EnvironmentPermission environmentPermission2 = (EnvironmentPermission)Copy();
		string pathList = environmentPermission.GetPathList(EnvironmentPermissionAccess.Read);
		if (pathList != null)
		{
			environmentPermission2.AddPathList(EnvironmentPermissionAccess.Read, pathList);
		}
		pathList = environmentPermission.GetPathList(EnvironmentPermissionAccess.Write);
		if (pathList != null)
		{
			environmentPermission2.AddPathList(EnvironmentPermissionAccess.Write, pathList);
		}
		return environmentPermission2;
	}

	int IBuiltInPermission.GetTokenIndex()
	{
		return 0;
	}

	private bool IsEmpty()
	{
		if (_state == PermissionState.None && readList.Count == 0)
		{
			return writeList.Count == 0;
		}
		return false;
	}

	private EnvironmentPermission Cast(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		EnvironmentPermission obj = target as EnvironmentPermission;
		if (obj == null)
		{
			CodeAccessPermission.ThrowInvalidPermission(target, typeof(EnvironmentPermission));
		}
		return obj;
	}

	internal void ThrowInvalidFlag(EnvironmentPermissionAccess flag, bool context)
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
}
