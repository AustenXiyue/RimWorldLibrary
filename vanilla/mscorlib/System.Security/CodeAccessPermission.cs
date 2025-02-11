using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Security;

/// <summary>Defines the underlying structure of all code access permissions.</summary>
[Serializable]
[ComVisible(true)]
[MonoTODO("CAS support is experimental (and unsupported).")]
[SecurityPermission(SecurityAction.InheritanceDemand, ControlEvidence = true, ControlPolicy = true)]
public abstract class CodeAccessPermission : IPermission, ISecurityEncodable, IStackWalk
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.CodeAccessPermission" /> class. </summary>
	protected CodeAccessPermission()
	{
	}

	/// <summary>Declares that the calling code can access the resource protected by a permission demand through the code that calls this method, even if callers higher in the stack have not been granted permission to access the resource. Using <see cref="M:System.Security.CodeAccessPermission.Assert" /> can create security issues.</summary>
	/// <exception cref="T:System.Security.SecurityException">The calling code does not have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.Assertion" />.-or- There is already an active <see cref="M:System.Security.CodeAccessPermission.Assert" /> for the current frame. </exception>
	[SecuritySafeCritical]
	[MonoTODO("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
	public void Assert()
	{
		new PermissionSet(this).Assert();
	}

	/// <summary>When implemented by a derived class, creates and returns an identical copy of the current permission object.</summary>
	/// <returns>A copy of the current permission object.</returns>
	public abstract IPermission Copy();

	/// <summary>Forces a <see cref="T:System.Security.SecurityException" /> at run time if all callers higher in the call stack have not been granted the permission specified by the current instance.</summary>
	/// <exception cref="T:System.Security.SecurityException">A caller higher in the call stack does not have the permission specified by the current instance.-or- A caller higher in the call stack has called <see cref="M:System.Security.CodeAccessPermission.Deny" /> on the current permission object. </exception>
	[SecuritySafeCritical]
	public void Demand()
	{
		if (SecurityManager.SecurityEnabled)
		{
			new PermissionSet(this).CasOnlyDemand(3);
		}
	}

	/// <summary>Prevents callers higher in the call stack from using the code that calls this method to access the resource specified by the current instance.</summary>
	/// <exception cref="T:System.Security.SecurityException">There is already an active <see cref="M:System.Security.CodeAccessPermission.Deny" /> for the current frame. </exception>
	[Obsolete("Deny is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
	[SecuritySafeCritical]
	[MonoTODO("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
	public void Deny()
	{
		new PermissionSet(this).Deny();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Security.CodeAccessPermission" /> object is equal to the current <see cref="T:System.Security.CodeAccessPermission" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Security.CodeAccessPermission" /> object is equal to the current <see cref="T:System.Security.CodeAccessPermission" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Security.CodeAccessPermission" /> object to compare with the current <see cref="T:System.Security.CodeAccessPermission" />. </param>
	[ComVisible(false)]
	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		CodeAccessPermission codeAccessPermission = obj as CodeAccessPermission;
		if (IsSubsetOf(codeAccessPermission))
		{
			return codeAccessPermission.IsSubsetOf(this);
		}
		return false;
	}

	/// <summary>When overridden in a derived class, reconstructs a security object with a specified state from an XML encoding.</summary>
	/// <param name="elem">The XML encoding to use to reconstruct the security object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="elem" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="elem" /> parameter does not contain the XML encoding for an instance of the same type as the current instance.-or- The version number of the <paramref name="elem" /> parameter is not supported. </exception>
	public abstract void FromXml(SecurityElement elem);

	/// <summary>Gets a hash code for the <see cref="T:System.Security.CodeAccessPermission" /> object that is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Security.CodeAccessPermission" /> object.</returns>
	[ComVisible(false)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>When implemented by a derived class, creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not an instance of the same class as the current permission. </exception>
	public abstract IPermission Intersect(IPermission target);

	/// <summary>When implemented by a derived class, determines whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current permission is a subset of the specified permission; otherwise, false.</returns>
	/// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> parameter is not null and is not of the same type as the current permission. </exception>
	public abstract bool IsSubsetOf(IPermission target);

	/// <summary>Creates and returns a string representation of the current permission object.</summary>
	/// <returns>A string representation of the current permission object.</returns>
	public override string ToString()
	{
		return ToXml().ToString();
	}

	/// <summary>When overridden in a derived class, creates an XML encoding of the security object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	public abstract SecurityElement ToXml();

	/// <summary>When overridden in a derived class, creates a permission that is the union of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the union of the current permission and the specified permission.</returns>
	/// <param name="other">A permission to combine with the current permission. It must be of the same type as the current permission. </param>
	/// <exception cref="T:System.NotSupportedException">The <paramref name="other" /> parameter is not null. This method is only supported at this level when passed null. </exception>
	public virtual IPermission Union(IPermission other)
	{
		if (other != null)
		{
			throw new NotSupportedException();
		}
		return null;
	}

	/// <summary>Prevents callers higher in the call stack from using the code that calls this method to access all resources except for the resource specified by the current instance.</summary>
	/// <exception cref="T:System.Security.SecurityException">There is already an active <see cref="M:System.Security.CodeAccessPermission.PermitOnly" /> for the current frame. </exception>
	[MonoTODO("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
	[SecuritySafeCritical]
	public void PermitOnly()
	{
		new PermissionSet(this).PermitOnly();
	}

	/// <summary>Causes all previous overrides for the current frame to be removed and no longer in effect.</summary>
	/// <exception cref="T:System.InvalidOperationException">There is no previous <see cref="M:System.Security.CodeAccessPermission.Assert" />, <see cref="M:System.Security.CodeAccessPermission.Deny" />, or <see cref="M:System.Security.CodeAccessPermission.PermitOnly" /> for the current frame. </exception>
	[MonoTODO("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
	public static void RevertAll()
	{
		if (!SecurityManager.SecurityEnabled)
		{
			return;
		}
		throw new NotImplementedException();
	}

	/// <summary>Causes any previous <see cref="M:System.Security.CodeAccessPermission.Assert" /> for the current frame to be removed and no longer in effect.</summary>
	/// <exception cref="T:System.InvalidOperationException">There is no previous <see cref="M:System.Security.CodeAccessPermission.Assert" /> for the current frame. </exception>
	[MonoTODO("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
	public static void RevertAssert()
	{
		if (!SecurityManager.SecurityEnabled)
		{
			return;
		}
		throw new NotImplementedException();
	}

	/// <summary>Causes any previous <see cref="M:System.Security.CodeAccessPermission.Deny" /> for the current frame to be removed and no longer in effect.</summary>
	/// <exception cref="T:System.InvalidOperationException">There is no previous <see cref="M:System.Security.CodeAccessPermission.Deny" /> for the current frame. </exception>
	[MonoTODO("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
	public static void RevertDeny()
	{
		if (!SecurityManager.SecurityEnabled)
		{
			return;
		}
		throw new NotImplementedException();
	}

	/// <summary>Causes any previous <see cref="M:System.Security.CodeAccessPermission.PermitOnly" /> for the current frame to be removed and no longer in effect.</summary>
	/// <exception cref="T:System.InvalidOperationException">There is no previous <see cref="M:System.Security.CodeAccessPermission.PermitOnly" /> for the current frame. </exception>
	[MonoTODO("CAS support is experimental (and unsupported). Imperative mode is not implemented.")]
	public static void RevertPermitOnly()
	{
		if (!SecurityManager.SecurityEnabled)
		{
			return;
		}
		throw new NotImplementedException();
	}

	internal SecurityElement Element(int version)
	{
		SecurityElement securityElement = new SecurityElement("IPermission");
		Type type = GetType();
		securityElement.AddAttribute("class", type.FullName + ", " + type.Assembly.ToString().Replace('"', '\''));
		securityElement.AddAttribute("version", version.ToString());
		return securityElement;
	}

	internal static PermissionState CheckPermissionState(PermissionState state, bool allowUnrestricted)
	{
		if (state != 0 && state != PermissionState.Unrestricted)
		{
			throw new ArgumentException(string.Format(Locale.GetText("Invalid enum {0}"), state), "state");
		}
		return state;
	}

	internal static int CheckSecurityElement(SecurityElement se, string parameterName, int minimumVersion, int maximumVersion)
	{
		if (se == null)
		{
			throw new ArgumentNullException(parameterName);
		}
		if (se.Tag != "IPermission")
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

	internal static bool IsUnrestricted(SecurityElement se)
	{
		string text = se.Attribute("Unrestricted");
		if (text == null)
		{
			return false;
		}
		return string.Compare(text, bool.TrueString, ignoreCase: true, CultureInfo.InvariantCulture) == 0;
	}

	internal static void ThrowInvalidPermission(IPermission target, Type expected)
	{
		throw new ArgumentException(string.Format(Locale.GetText("Invalid permission type '{0}', expected type '{1}'."), target.GetType(), expected), "target");
	}
}
