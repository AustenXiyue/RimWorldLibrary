using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using Unity;

namespace System.Security.Principal;

/// <summary>Represents a Windows user.</summary>
[Serializable]
[ComVisible(true)]
public class WindowsIdentity : ClaimsIdentity, IIdentity, IDeserializationCallback, ISerializable, IDisposable
{
	private IntPtr _token;

	private string _type;

	private WindowsAccountType _account;

	private bool _authenticated;

	private string _name;

	private SerializationInfo _info;

	private static IntPtr invalidWindows = IntPtr.Zero;

	/// <summary>Identifies the name of the default <see cref="T:System.Security.Claims.ClaimsIdentity" /> issuer.</summary>
	/// <returns>NT_AUTHORITY.</returns>
	[NonSerialized]
	public new const string DefaultIssuer = "AD AUTHORITY";

	/// <summary>Gets the type of authentication used to identify the user.</summary>
	/// <returns>The type of authentication used to identify the user.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">Windows returned the Windows NT status code STATUS_ACCESS_DENIED.</exception>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. -or-The computer is not attached to a Windows 2003 or later domain.-or-The computer is not running Windows 2003 or later.-or-The user is not a member of the domain the computer is attached to.</exception>
	public sealed override string AuthenticationType
	{
		[SecuritySafeCritical]
		get
		{
			return _type;
		}
	}

	/// <summary>Gets a value that indicates whether the user account is identified as an anonymous account by the system.</summary>
	/// <returns>true if the user account is an anonymous account; otherwise, false.</returns>
	public virtual bool IsAnonymous => _account == WindowsAccountType.Anonymous;

	/// <summary>Gets a value indicating whether the user has been authenticated by Windows.</summary>
	/// <returns>true if the user was authenticated; otherwise, false.</returns>
	public override bool IsAuthenticated => _authenticated;

	/// <summary>Gets a value indicating whether the user account is identified as a <see cref="F:System.Security.Principal.WindowsAccountType.Guest" /> account by the system.</summary>
	/// <returns>true if the user account is a <see cref="F:System.Security.Principal.WindowsAccountType.Guest" /> account; otherwise, false.</returns>
	public virtual bool IsGuest => _account == WindowsAccountType.Guest;

	/// <summary>Gets a value indicating whether the user account is identified as a <see cref="F:System.Security.Principal.WindowsAccountType.System" /> account by the system.</summary>
	/// <returns>true if the user account is a <see cref="F:System.Security.Principal.WindowsAccountType.System" /> account; otherwise, false.</returns>
	public virtual bool IsSystem => _account == WindowsAccountType.System;

	/// <summary>Gets the user's Windows logon name.</summary>
	/// <returns>The Windows logon name of the user on whose behalf the code is being run.</returns>
	public override string Name
	{
		[SecuritySafeCritical]
		get
		{
			if (_name == null)
			{
				_name = GetTokenName(_token);
			}
			return _name;
		}
	}

	/// <summary>Gets the Windows account token for the user.</summary>
	/// <returns>The handle of the access token associated with the current execution thread.</returns>
	public virtual IntPtr Token => _token;

	/// <summary>Gets the groups the current Windows user belongs to.</summary>
	/// <returns>An object representing the groups the current Windows user belongs to.</returns>
	[MonoTODO("not implemented")]
	public IdentityReferenceCollection Groups
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the impersonation level for the user.</summary>
	/// <returns>One of the enumeration values that specifies the impersonation level. </returns>
	[MonoTODO("not implemented")]
	[ComVisible(false)]
	public TokenImpersonationLevel ImpersonationLevel
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the security identifier (SID) for the token owner.</summary>
	/// <returns>An object for the token owner.</returns>
	[ComVisible(false)]
	[MonoTODO("not implemented")]
	public SecurityIdentifier Owner
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the security identifier (SID) for the user.</summary>
	/// <returns>An object for the user.</returns>
	[MonoTODO("not implemented")]
	[ComVisible(false)]
	public SecurityIdentifier User
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public SafeAccessTokenHandle AccessToken
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets claims that have the <see cref="F:System.Security.Claims.ClaimTypes.WindowsDeviceClaim" /> property key.</summary>
	/// <returns>A collection of claims that have the <see cref="F:System.Security.Claims.ClaimTypes.WindowsDeviceClaim" /> property key.</returns>
	public virtual IEnumerable<Claim> DeviceClaims
	{
		get
		{
			//IL_0007: Expected O, but got I4
			ThrowStub.ThrowNotSupportedException();
			return (IEnumerable<Claim>)0;
		}
	}

	/// <summary>Gets claims that have the <see cref="F:System.Security.Claims.ClaimTypes.WindowsUserClaim" /> property key.</summary>
	/// <returns>A collection of claims that have the <see cref="F:System.Security.Claims.ClaimTypes.WindowsUserClaim" /> property key.</returns>
	public virtual IEnumerable<Claim> UserClaims
	{
		get
		{
			//IL_0007: Expected O, but got I4
			ThrowStub.ThrowNotSupportedException();
			return (IEnumerable<Claim>)0;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.WindowsIdentity" /> class for the user represented by the specified Windows account token.</summary>
	/// <param name="userToken">The account token for the user on whose behalf the code is running. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="userToken" /> is 0.-or-<paramref name="userToken" /> is duplicated and invalid for impersonation.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. -or-A Win32 error occurred.</exception>
	[SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
	public WindowsIdentity(IntPtr userToken)
		: this(userToken, null, WindowsAccountType.Normal, isAuthenticated: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.WindowsIdentity" /> class for the user represented by the specified Windows account token and the specified authentication type.</summary>
	/// <param name="userToken">The account token for the user on whose behalf the code is running. </param>
	/// <param name="type">(Informational use only.) The type of authentication used to identify the user. For more information, see Remarks.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="userToken" /> is 0.-or-<paramref name="userToken" /> is duplicated and invalid for impersonation.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. -or-A Win32 error occurred.</exception>
	[SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
	public WindowsIdentity(IntPtr userToken, string type)
		: this(userToken, type, WindowsAccountType.Normal, isAuthenticated: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.WindowsIdentity" /> class for the user represented by the specified Windows account token, the specified authentication type, and the specified Windows account type.</summary>
	/// <param name="userToken">The account token for the user on whose behalf the code is running. </param>
	/// <param name="type">(Informational use only.) The type of authentication used to identify the user. For more information, see Remarks.</param>
	/// <param name="acctType">One of the enumeration values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="userToken" /> is 0.-or-<paramref name="userToken" /> is duplicated and invalid for impersonation.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. -or-A Win32 error occurred.</exception>
	[SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
	public WindowsIdentity(IntPtr userToken, string type, WindowsAccountType acctType)
		: this(userToken, type, acctType, isAuthenticated: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.WindowsIdentity" /> class for the user represented by the specified Windows account token, the specified authentication type, the specified Windows account type, and the specified authentication status.</summary>
	/// <param name="userToken">The account token for the user on whose behalf the code is running. </param>
	/// <param name="type">(Informational use only.) The type of authentication used to identify the user. For more information, see Remarks.</param>
	/// <param name="acctType">One of the enumeration values. </param>
	/// <param name="isAuthenticated">true to indicate that the user is authenticated; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="userToken" /> is 0.-or-<paramref name="userToken" /> is duplicated and invalid for impersonation.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. -or-A Win32 error occurred.</exception>
	[SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
	public WindowsIdentity(IntPtr userToken, string type, WindowsAccountType acctType, bool isAuthenticated)
	{
		_type = type;
		_account = acctType;
		_authenticated = isAuthenticated;
		_name = null;
		SetToken(userToken);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.WindowsIdentity" /> class for the user represented by the specified User Principal Name (UPN).</summary>
	/// <param name="sUserPrincipalName">The UPN for the user on whose behalf the code is running. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">Windows returned the Windows NT status code STATUS_ACCESS_DENIED.</exception>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. -or-The computer is not attached to a Windows 2003 or later domain.-or-The computer is not running Windows 2003 or later.-or-The user is not a member of the domain the computer is attached to.</exception>
	[SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
	public WindowsIdentity(string sUserPrincipalName)
		: this(sUserPrincipalName, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.WindowsIdentity" /> class for the user represented by the specified User Principal Name (UPN) and the specified authentication type.</summary>
	/// <param name="sUserPrincipalName">The UPN for the user on whose behalf the code is running. </param>
	/// <param name="type">(Informational use only.) The type of authentication used to identify the user. For more information, see Remarks.</param>
	/// <exception cref="T:System.UnauthorizedAccessException">Windows returned the Windows NT status code STATUS_ACCESS_DENIED.</exception>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. -or-The computer is not attached to a Windows 2003 or later domain.-or-The computer is not running Windows 2003 or later.-or-The user is not a member of the domain the computer is attached to.</exception>
	[SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
	public WindowsIdentity(string sUserPrincipalName, string type)
	{
		if (sUserPrincipalName == null)
		{
			throw new NullReferenceException("sUserPrincipalName");
		}
		IntPtr userToken = GetUserToken(sUserPrincipalName);
		if (!Environment.IsUnix && userToken == IntPtr.Zero)
		{
			throw new ArgumentException("only for Windows Server 2003 +");
		}
		_authenticated = true;
		_account = WindowsAccountType.Normal;
		_type = type;
		SetToken(userToken);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.WindowsIdentity" /> class for the user represented by information in a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> stream.</summary>
	/// <param name="info">The object containing the account information for the user. </param>
	/// <param name="context">An object that indicates the stream characteristics. </param>
	/// <exception cref="T:System.NotSupportedException">A <see cref="T:System.Security.Principal.WindowsIdentity" /> cannot be serialized across processes. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. -or-A Win32 error occurred.</exception>
	[SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
	public WindowsIdentity(SerializationInfo info, StreamingContext context)
	{
		_info = info;
	}

	internal WindowsIdentity(ClaimsIdentity claimsIdentity, IntPtr userToken)
		: base(claimsIdentity)
	{
		if (userToken != IntPtr.Zero && userToken.ToInt64() > 0)
		{
			SetToken(userToken);
		}
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Security.Principal.WindowsIdentity" />. </summary>
	[ComVisible(false)]
	public void Dispose()
	{
		_token = IntPtr.Zero;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Principal.WindowsIdentity" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	[ComVisible(false)]
	protected virtual void Dispose(bool disposing)
	{
		_token = IntPtr.Zero;
	}

	/// <summary>Returns a <see cref="T:System.Security.Principal.WindowsIdentity" /> object that you can use as a sentinel value in your code to represent an anonymous user. The property value does not represent the built-in anonymous identity used by the Windows operating system.</summary>
	/// <returns>An object that represents an anonymous user.</returns>
	public static WindowsIdentity GetAnonymous()
	{
		WindowsIdentity windowsIdentity = null;
		if (Environment.IsUnix)
		{
			windowsIdentity = new WindowsIdentity("nobody");
			windowsIdentity._account = WindowsAccountType.Anonymous;
			windowsIdentity._authenticated = false;
			windowsIdentity._type = string.Empty;
		}
		else
		{
			windowsIdentity = new WindowsIdentity(IntPtr.Zero, string.Empty, WindowsAccountType.Anonymous, isAuthenticated: false);
			windowsIdentity._name = string.Empty;
		}
		return windowsIdentity;
	}

	/// <summary>Returns a <see cref="T:System.Security.Principal.WindowsIdentity" /> object that represents the current Windows user.</summary>
	/// <returns>An object that represents the current user.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlPrincipal" />
	/// </PermissionSet>
	public static WindowsIdentity GetCurrent()
	{
		return new WindowsIdentity(GetCurrentToken(), null, WindowsAccountType.Normal, isAuthenticated: true);
	}

	/// <summary>Returns a <see cref="T:System.Security.Principal.WindowsIdentity" /> object that represents the Windows identity for either the thread or the process, depending on the value of the <paramref name="ifImpersonating" /> parameter.</summary>
	/// <returns>An object that represents a Windows user.</returns>
	/// <param name="ifImpersonating">true to return the <see cref="T:System.Security.Principal.WindowsIdentity" /> only if the thread is currently impersonating; false to return the <see cref="T:System.Security.Principal.WindowsIdentity" />   of the thread if it is impersonating or the <see cref="T:System.Security.Principal.WindowsIdentity" /> of the process if the thread is not currently impersonating.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlPrincipal" />
	/// </PermissionSet>
	[MonoTODO("need icall changes")]
	public static WindowsIdentity GetCurrent(bool ifImpersonating)
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns a <see cref="T:System.Security.Principal.WindowsIdentity" /> object that represents the current Windows user, using the specified desired token access level.</summary>
	/// <returns>An object that represents the current user.</returns>
	/// <param name="desiredAccess">A bitwise combination of the enumeration values. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlPrincipal" />
	/// </PermissionSet>
	[MonoTODO("need icall changes")]
	public static WindowsIdentity GetCurrent(TokenAccessLevels desiredAccess)
	{
		throw new NotImplementedException();
	}

	/// <summary>Impersonates the user represented by the <see cref="T:System.Security.Principal.WindowsIdentity" /> object.</summary>
	/// <returns>An object that represents the Windows user prior to impersonation; this can be used to revert to the original user's context.</returns>
	/// <exception cref="T:System.InvalidOperationException">An anonymous identity attempted to perform an impersonation.</exception>
	/// <exception cref="T:System.Security.SecurityException">A Win32 error occurred.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlPrincipal" />
	/// </PermissionSet>
	public virtual WindowsImpersonationContext Impersonate()
	{
		return new WindowsImpersonationContext(_token);
	}

	/// <summary>Impersonates the user represented by the specified user token.</summary>
	/// <returns>An object that represents the Windows user prior to impersonation; this object can be used to revert to the original user's context.</returns>
	/// <param name="userToken">The handle of a Windows account token. This token is usually retrieved through a call to unmanaged code, such as a call to the Win32 API LogonUser function. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">Windows returned the Windows NT status code STATUS_ACCESS_DENIED.</exception>
	/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory available.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlPrincipal" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
	public static WindowsImpersonationContext Impersonate(IntPtr userToken)
	{
		return new WindowsImpersonationContext(userToken);
	}

	[SecuritySafeCritical]
	public static void RunImpersonated(SafeAccessTokenHandle safeAccessTokenHandle, Action action)
	{
		throw new NotImplementedException();
	}

	[SecuritySafeCritical]
	public static T RunImpersonated<T>(SafeAccessTokenHandle safeAccessTokenHandle, Func<T> func)
	{
		throw new NotImplementedException();
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and is called back by the deserialization event when deserialization is complete.</summary>
	/// <param name="sender">The source of the deserialization event. </param>
	void IDeserializationCallback.OnDeserialization(object sender)
	{
		_token = (IntPtr)_info.GetValue("m_userToken", typeof(IntPtr));
		_name = _info.GetString("m_name");
		if (_name != null)
		{
			if (GetTokenName(_token) != _name)
			{
				throw new SerializationException("Token-Name mismatch.");
			}
		}
		else
		{
			_name = GetTokenName(_token);
			if (_name == null)
			{
				throw new SerializationException("Token doesn't match a user.");
			}
		}
		_type = _info.GetString("m_type");
		_account = (WindowsAccountType)_info.GetValue("m_acctType", typeof(WindowsAccountType));
		_authenticated = _info.GetBoolean("m_isAuthenticated");
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the logical context information needed to recreate an instance of this execution context.</summary>
	/// <param name="info">An object containing the information required to serialize the <see cref="T:System.Collections.Hashtable" />. </param>
	/// <param name="context">An object containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Hashtable" />. </param>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("m_userToken", _token);
		info.AddValue("m_name", _name);
		info.AddValue("m_type", _type);
		info.AddValue("m_acctType", _account);
		info.AddValue("m_isAuthenticated", _authenticated);
	}

	internal ClaimsIdentity CloneAsBase()
	{
		return base.Clone();
	}

	internal IntPtr GetTokenInternal()
	{
		return _token;
	}

	private void SetToken(IntPtr token)
	{
		if (Environment.IsUnix)
		{
			_token = token;
			if (_type == null)
			{
				_type = "POSIX";
			}
			if (_token == IntPtr.Zero)
			{
				_account = WindowsAccountType.System;
			}
		}
		else
		{
			if (token == invalidWindows && _account != WindowsAccountType.Anonymous)
			{
				throw new ArgumentException("Invalid token");
			}
			_token = token;
			if (_type == null)
			{
				_type = "NTLM";
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string[] _GetRoles(IntPtr token);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern IntPtr GetCurrentToken();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string GetTokenName(IntPtr token);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr GetUserToken(string username);

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Principal.WindowsIdentity" /> class by using the specified <see cref="T:System.Security.Principal.WindowsIdentity" /> object.</summary>
	/// <param name="identity">The object from which to construct the new instance of <see cref="T:System.Security.Principal.WindowsIdentity" />.</param>
	[SecuritySafeCritical]
	protected WindowsIdentity(WindowsIdentity identity)
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
