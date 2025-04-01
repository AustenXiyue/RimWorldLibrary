using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Unity;

namespace Microsoft.Win32;

/// <summary>Represents a key-level node in the Windows registry. This class is a registry encapsulation.</summary>
[ComVisible(true)]
public sealed class RegistryKey : MarshalByRefObject, IDisposable
{
	private object handle;

	private SafeRegistryHandle safe_handle;

	private object hive;

	private readonly string qname;

	private readonly bool isRemoteRoot;

	private readonly bool isWritable;

	private static readonly IRegistryApi RegistryApi;

	/// <summary>Retrieves the name of the key.</summary>
	/// <returns>The absolute (qualified) name of the key.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is closed (closed keys cannot be accessed). </exception>
	public string Name => qname;

	/// <summary>Retrieves the count of subkeys of the current key.</summary>
	/// <returns>The number of subkeys of the current key.</returns>
	/// <exception cref="T:System.Security.SecurityException">The user does not have read permission for the key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <exception cref="T:System.IO.IOException">A system error occurred, for example the current key has been deleted.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int SubKeyCount
	{
		get
		{
			AssertKeyStillValid();
			return RegistryApi.SubKeyCount(this);
		}
	}

	/// <summary>Retrieves the count of values in the key.</summary>
	/// <returns>The number of name/value pairs in the key.</returns>
	/// <exception cref="T:System.Security.SecurityException">The user does not have read permission for the key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <exception cref="T:System.IO.IOException">A system error occurred, for example the current key has been deleted.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public int ValueCount
	{
		get
		{
			AssertKeyStillValid();
			return RegistryApi.ValueCount(this);
		}
	}

	/// <summary>Gets a <see cref="T:Microsoft.Win32.SafeHandles.SafeRegistryHandle" /> object that represents the registry key that the current <see cref="T:Microsoft.Win32.RegistryKey" /> object encapsulates.</summary>
	/// <returns>The handle to the registry key.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The registry key is closed. Closed keys cannot be accessed.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <exception cref="T:System.IO.IOException">A system error occurred, such as deletion of the current key.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read the key.</exception>
	[MonoTODO("Not implemented in Unix")]
	[ComVisible(false)]
	public SafeRegistryHandle Handle
	{
		get
		{
			AssertKeyStillValid();
			if (safe_handle == null)
			{
				IntPtr preexistingHandle = RegistryApi.GetHandle(this);
				safe_handle = new SafeRegistryHandle(preexistingHandle, ownsHandle: true);
			}
			return safe_handle;
		}
	}

	/// <summary>Gets the view that was used to create the registry key. </summary>
	/// <returns>The view that was used to create the registry key.-or-<see cref="F:Microsoft.Win32.RegistryView.Default" />, if no view was used.</returns>
	[MonoLimitation("View is ignored in Mono.")]
	[ComVisible(false)]
	public RegistryView View => RegistryView.Default;

	internal bool IsRoot => hive != null;

	private bool IsWritable => isWritable;

	internal RegistryHive Hive
	{
		get
		{
			if (!IsRoot)
			{
				throw new NotSupportedException();
			}
			return (RegistryHive)hive;
		}
	}

	internal object InternalHandle => handle;

	static RegistryKey()
	{
		if (Path.DirectorySeparatorChar == '\\')
		{
			RegistryApi = new Win32RegistryApi();
		}
		else
		{
			RegistryApi = new UnixRegistryApi();
		}
	}

	internal RegistryKey(RegistryHive hiveId)
		: this(hiveId, new IntPtr((int)hiveId), remoteRoot: false)
	{
	}

	internal RegistryKey(RegistryHive hiveId, IntPtr keyHandle, bool remoteRoot)
	{
		hive = hiveId;
		handle = keyHandle;
		qname = GetHiveName(hiveId);
		isRemoteRoot = remoteRoot;
		isWritable = true;
	}

	internal RegistryKey(object data, string keyName, bool writable)
	{
		handle = data;
		qname = keyName;
		isWritable = writable;
	}

	internal static bool IsEquals(RegistryKey a, RegistryKey b)
	{
		if (a.hive == b.hive && a.handle == b.handle && a.qname == b.qname && a.isRemoteRoot == b.isRemoteRoot)
		{
			return a.isWritable == b.isWritable;
		}
		return false;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:Microsoft.Win32.RegistryKey" /> class.</summary>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Close();
	}

	/// <summary>Writes all the attributes of the specified open registry key into the registry.</summary>
	public void Flush()
	{
		RegistryApi.Flush(this);
	}

	/// <summary>Closes the key and flushes it to disk if its contents have been modified.</summary>
	public void Close()
	{
		Flush();
		if (isRemoteRoot || !IsRoot)
		{
			RegistryApi.Close(this);
			handle = null;
			safe_handle = null;
		}
	}

	/// <summary>Sets the specified name/value pair.</summary>
	/// <param name="name">The name of the value to store. </param>
	/// <param name="value">The data to be stored. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is an unsupported data type. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is read-only, and cannot be written to; for example, the key has not been opened with write access. -or-The <see cref="T:Microsoft.Win32.RegistryKey" /> object represents a root-level node, and the operating system is Windows Millennium Edition or Windows 98.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or modify registry keys. </exception>
	/// <exception cref="T:System.IO.IOException">The <see cref="T:Microsoft.Win32.RegistryKey" /> object represents a root-level node, and the operating system is Windows 2000, Windows XP, or Windows Server 2003.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public void SetValue(string name, object value)
	{
		AssertKeyStillValid();
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (name != null)
		{
			AssertKeyNameLength(name);
		}
		if (!IsWritable)
		{
			throw new UnauthorizedAccessException("Cannot write to the registry key.");
		}
		RegistryApi.SetValue(this, name, value);
	}

	/// <summary>Sets the value of a name/value pair in the registry key, using the specified registry data type.</summary>
	/// <param name="name">The name of the value to be stored. </param>
	/// <param name="value">The data to be stored. </param>
	/// <param name="valueKind">The registry data type to use when storing the data. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The type of <paramref name="value" /> did not match the registry data type specified by <paramref name="valueKind" />, therefore the data could not be converted properly. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is read-only, and cannot be written to; for example, the key has not been opened with write access.-or-The <see cref="T:Microsoft.Win32.RegistryKey" /> object represents a root-level node, and the operating system is Windows Millennium Edition or Windows 98. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or modify registry keys. </exception>
	/// <exception cref="T:System.IO.IOException">The <see cref="T:Microsoft.Win32.RegistryKey" /> object represents a root-level node, and the operating system is Windows 2000, Windows XP, or Windows Server 2003.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ComVisible(false)]
	public void SetValue(string name, object value, RegistryValueKind valueKind)
	{
		AssertKeyStillValid();
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (name != null)
		{
			AssertKeyNameLength(name);
		}
		if (!IsWritable)
		{
			throw new UnauthorizedAccessException("Cannot write to the registry key.");
		}
		RegistryApi.SetValue(this, name, value, valueKind);
	}

	/// <summary>Retrieves a subkey as read-only.</summary>
	/// <returns>The subkey requested, or null if the operation failed.</returns>
	/// <param name="name">The name or path of the subkey to open as read-only. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read the registry key. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public RegistryKey OpenSubKey(string name)
	{
		return OpenSubKey(name, writable: false);
	}

	/// <summary>Retrieves a specified subkey, and specifies whether write access is to be applied to the key. </summary>
	/// <returns>The subkey requested, or null if the operation failed.</returns>
	/// <param name="name">Name or path of the subkey to open. </param>
	/// <param name="writable">Set to true if you need write access to the key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to access the registry key in the specified mode. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public RegistryKey OpenSubKey(string name, bool writable)
	{
		AssertKeyStillValid();
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		AssertKeyNameLength(name);
		return RegistryApi.OpenSubKey(this, name, writable);
	}

	/// <summary>Retrieves the value associated with the specified name. Returns null if the name/value pair does not exist in the registry.</summary>
	/// <returns>The value associated with <paramref name="name" />, or null if <paramref name="name" /> is not found.</returns>
	/// <param name="name">The name of the value to retrieve. This string is not case-sensitive.</param>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the registry key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.IO.IOException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value has been marked for deletion. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public object GetValue(string name)
	{
		return GetValue(name, null);
	}

	/// <summary>Retrieves the value associated with the specified name. If the name is not found, returns the default value that you provide.</summary>
	/// <returns>The value associated with <paramref name="name" />, with any embedded environment variables left unexpanded, or <paramref name="defaultValue" /> if <paramref name="name" /> is not found.</returns>
	/// <param name="name">The name of the value to retrieve. This string is not case-sensitive.</param>
	/// <param name="defaultValue">The value to return if <paramref name="name" /> does not exist. </param>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the registry key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.IO.IOException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value has been marked for deletion. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public object GetValue(string name, object defaultValue)
	{
		AssertKeyStillValid();
		return RegistryApi.GetValue(this, name, defaultValue, RegistryValueOptions.None);
	}

	/// <summary>Retrieves the value associated with the specified name and retrieval options. If the name is not found, returns the default value that you provide.</summary>
	/// <returns>The value associated with <paramref name="name" />, processed according to the specified <paramref name="options" />, or <paramref name="defaultValue" /> if <paramref name="name" /> is not found.</returns>
	/// <param name="name">The name of the value to retrieve. This string is not case-sensitive.</param>
	/// <param name="defaultValue">The value to return if <paramref name="name" /> does not exist. </param>
	/// <param name="options">One of the enumeration values that specifies optional processing of the retrieved value.</param>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the registry key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.IO.IOException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value has been marked for deletion. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not a valid <see cref="T:Microsoft.Win32.RegistryValueOptions" /> value; for example, an invalid value is cast to <see cref="T:Microsoft.Win32.RegistryValueOptions" />.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	[ComVisible(false)]
	public object GetValue(string name, object defaultValue, RegistryValueOptions options)
	{
		AssertKeyStillValid();
		return RegistryApi.GetValue(this, name, defaultValue, options);
	}

	/// <summary>Retrieves the registry data type of the value associated with the specified name.</summary>
	/// <returns>The registry data type of the value associated with <paramref name="name" />.</returns>
	/// <param name="name">The name of the value whose registry data type is to be retrieved. This string is not case-sensitive.</param>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the registry key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.IO.IOException">The subkey that contains the specified value does not exist.-or-The name/value pair specified by <paramref name="name" /> does not exist.This exception is not thrown on Windows 95, Windows 98, or Windows Millennium Edition.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	[ComVisible(false)]
	public RegistryValueKind GetValueKind(string name)
	{
		return RegistryApi.GetValueKind(this, name);
	}

	/// <summary>Creates a new subkey or opens an existing subkey for write access.  </summary>
	/// <returns>The newly created subkey, or null if the operation failed. If a zero-length string is specified for <paramref name="subkey" />, the current <see cref="T:Microsoft.Win32.RegistryKey" /> object is returned.</returns>
	/// <param name="subkey">The name or path of the subkey to create or open. This string is not case-sensitive.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="subkey" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or open the registry key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> on which this method is being invoked is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <see cref="T:Microsoft.Win32.RegistryKey" /> cannot be written to; for example, it was not opened as a writable key , or the user does not have the necessary access rights. </exception>
	/// <exception cref="T:System.IO.IOException">The nesting level exceeds 510.-or-A system error occurred, such as deletion of the key, or an attempt to create a key in the <see cref="F:Microsoft.Win32.Registry.LocalMachine" /> root.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public RegistryKey CreateSubKey(string subkey)
	{
		AssertKeyStillValid();
		AssertKeyNameNotNull(subkey);
		AssertKeyNameLength(subkey);
		if (!IsWritable)
		{
			throw new UnauthorizedAccessException("Cannot write to the registry key.");
		}
		return RegistryApi.CreateSubKey(this, subkey);
	}

	/// <summary>Creates a new subkey or opens an existing subkey for write access, using the specified permission check option. </summary>
	/// <returns>The newly created subkey, or null if the operation failed. If a zero-length string is specified for <paramref name="subkey" />, the current <see cref="T:Microsoft.Win32.RegistryKey" /> object is returned.</returns>
	/// <param name="subkey">The name or path of the subkey to create or open. This string is not case-sensitive.</param>
	/// <param name="permissionCheck">One of the enumeration values that specifies whether the key is opened for read or read/write access.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="subkey" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or open the registry key. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="permissionCheck" /> contains an invalid value.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> on which this method is being invoked is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <see cref="T:Microsoft.Win32.RegistryKey" /> cannot be written to; for example, it was not opened as a writable key, or the user does not have the necessary access rights. </exception>
	/// <exception cref="T:System.IO.IOException">The nesting level exceeds 510.-or-A system error occurred, such as deletion of the key, or an attempt to create a key in the <see cref="F:Microsoft.Win32.Registry.LocalMachine" /> root.</exception>
	[MonoLimitation("permissionCheck is ignored in Mono")]
	[ComVisible(false)]
	public RegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck)
	{
		return CreateSubKey(subkey);
	}

	/// <summary>Creates a new subkey or opens an existing subkey for write access, using the specified permission check option and registry security. </summary>
	/// <returns>The newly created subkey, or null if the operation failed. If a zero-length string is specified for <paramref name="subkey" />, the current <see cref="T:Microsoft.Win32.RegistryKey" /> object is returned.</returns>
	/// <param name="subkey">The name or path of the subkey to create or open. This string is not case-sensitive.</param>
	/// <param name="permissionCheck">One of the enumeration values that specifies whether the key is opened for read or read/write access.</param>
	/// <param name="registrySecurity">The access control security for the new key.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="subkey" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or open the registry key. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="permissionCheck" /> contains an invalid value.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> on which this method is being invoked is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The current <see cref="T:Microsoft.Win32.RegistryKey" /> cannot be written to; for example, it was not opened as a writable key, or the user does not have the necessary access rights.</exception>
	/// <exception cref="T:System.IO.IOException">The nesting level exceeds 510.-or-A system error occurred, such as deletion of the key, or an attempt to create a key in the <see cref="F:Microsoft.Win32.Registry.LocalMachine" /> root.</exception>
	[ComVisible(false)]
	[MonoLimitation("permissionCheck and registrySecurity are ignored in Mono")]
	public RegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck, RegistrySecurity registrySecurity)
	{
		return CreateSubKey(subkey);
	}

	/// <summary>Creates a subkey or opens a subkey for write access, using the specified permission check and registry options. </summary>
	/// <returns>The newly created subkey, or null if the operation failed.</returns>
	/// <param name="subkey">The name or path of the subkey to create or open. </param>
	/// <param name="permissionCheck">One of the enumeration values that specifies whether the key is opened for read or read/write access.</param>
	/// <param name="options">The registry option to use; for example, that creates a volatile key. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="subkey " />is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The current <see cref="T:Microsoft.Win32.RegistryKey" /> object is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The current <see cref="T:Microsoft.Win32.RegistryKey" /> object cannot be written to; for example, it was not opened as a writable key, or the user does not have the required access rights.</exception>
	/// <exception cref="T:System.IO.IOException">The nesting level exceeds 510.-or-A system error occurred, such as deletion of the key or an attempt to create a key in the <see cref="F:Microsoft.Win32.Registry.LocalMachine" /> root. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or open the registry key.</exception>
	[MonoLimitation("permissionCheck is ignored in Mono")]
	[ComVisible(false)]
	public RegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck, RegistryOptions options)
	{
		AssertKeyStillValid();
		AssertKeyNameNotNull(subkey);
		AssertKeyNameLength(subkey);
		if (!IsWritable)
		{
			throw new UnauthorizedAccessException("Cannot write to the registry key.");
		}
		return RegistryApi.CreateSubKey(this, subkey, options);
	}

	/// <summary>Creates a subkey or opens a subkey for write access, using the specified permission check option, registry option, and registry security.</summary>
	/// <returns>The newly created subkey, or null if the operation failed.  </returns>
	/// <param name="subkey">The name or path of the subkey to create or open.</param>
	/// <param name="permissionCheck">One of the enumeration values that specifies whether the key is opened for read or read/write access.</param>
	/// <param name="registryOptions">The registry option to use.</param>
	/// <param name="registrySecurity">The access control security for the new subkey. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="subkey " />is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The current <see cref="T:Microsoft.Win32.RegistryKey" /> object is closed. Closed keys cannot be accessed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The current <see cref="T:Microsoft.Win32.RegistryKey" /> object cannot be written to; for example, it was not opened as a writable key, or the user does not have the required access rights.</exception>
	/// <exception cref="T:System.IO.IOException">The nesting level exceeds 510.-or-A system error occurred, such as deletion of the key or an attempt to create a key in the <see cref="F:Microsoft.Win32.Registry.LocalMachine" /> root. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or open the registry key.</exception>
	[MonoLimitation("permissionCheck and registrySecurity are ignored in Mono")]
	[ComVisible(false)]
	public RegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck, RegistryOptions registryOptions, RegistrySecurity registrySecurity)
	{
		return CreateSubKey(subkey, permissionCheck, registryOptions);
	}

	[ComVisible(false)]
	public RegistryKey CreateSubKey(string subkey, bool writable)
	{
		return CreateSubKey(subkey, (!writable) ? RegistryKeyPermissionCheck.ReadSubTree : RegistryKeyPermissionCheck.ReadWriteSubTree);
	}

	[ComVisible(false)]
	public RegistryKey CreateSubKey(string subkey, bool writable, RegistryOptions options)
	{
		return CreateSubKey(subkey, (!writable) ? RegistryKeyPermissionCheck.ReadSubTree : RegistryKeyPermissionCheck.ReadWriteSubTree, options);
	}

	/// <summary>Deletes the specified subkey. </summary>
	/// <param name="subkey">The name of the subkey to delete. This string is not case-sensitive.</param>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="subkey" /> has child subkeys </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="subkey" /> parameter does not specify a valid registry key </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="subkey" /> is null</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to delete the key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public void DeleteSubKey(string subkey)
	{
		DeleteSubKey(subkey, throwOnMissingSubKey: true);
	}

	/// <summary>Deletes the specified subkey, and specifies whether an exception is raised if the subkey is not found. </summary>
	/// <param name="subkey">The name of the subkey to delete. This string is not case-sensitive.</param>
	/// <param name="throwOnMissingSubKey">Indicates whether an exception should be raised if the specified subkey cannot be found. If this argument is true and the specified subkey does not exist, an exception is raised. If this argument is false and the specified subkey does not exist, no action is taken. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="subkey" /> has child subkeys. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="subkey" /> does not specify a valid registry key, and <paramref name="throwOnMissingSubKey" /> is true. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="subkey" /> is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to delete the key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public void DeleteSubKey(string subkey, bool throwOnMissingSubKey)
	{
		AssertKeyStillValid();
		AssertKeyNameNotNull(subkey);
		AssertKeyNameLength(subkey);
		if (!IsWritable)
		{
			throw new UnauthorizedAccessException("Cannot write to the registry key.");
		}
		RegistryKey registryKey = OpenSubKey(subkey);
		if (registryKey == null)
		{
			if (throwOnMissingSubKey)
			{
				throw new ArgumentException("Cannot delete a subkey tree because the subkey does not exist.");
			}
			return;
		}
		if (registryKey.SubKeyCount > 0)
		{
			throw new InvalidOperationException("Registry key has subkeys and recursive removes are not supported by this method.");
		}
		registryKey.Close();
		RegistryApi.DeleteKey(this, subkey, throwOnMissingSubKey);
	}

	/// <summary>Deletes a subkey and any child subkeys recursively. </summary>
	/// <param name="subkey">The subkey to delete. This string is not case-sensitive.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="subkey" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">Deletion of a root hive is attempted.-or-<paramref name="subkey" /> does not specify a valid registry subkey. </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error has occurred.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to delete the key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public void DeleteSubKeyTree(string subkey)
	{
		DeleteSubKeyTree(subkey, throwOnMissingSubKey: true);
	}

	/// <summary>Deletes the specified subkey and any child subkeys recursively, and specifies whether an exception is raised if the subkey is not found. </summary>
	/// <param name="subkey">The name of the subkey to delete. This string is not case-sensitive.</param>
	/// <param name="throwOnMissingSubKey">Indicates whether an exception should be raised if the specified subkey cannot be found. If this argument is true and the specified subkey does not exist, an exception is raised. If this argument is false and the specified subkey does not exist, no action is taken.</param>
	/// <exception cref="T:System.ArgumentException">An attempt was made to delete the root hive of the tree.-or-<paramref name="subkey" /> does not specify a valid registry subkey, and <paramref name="throwOnMissingSubKey" /> is true.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="subkey" /> is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is closed (closed keys cannot be accessed).</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to delete the key.</exception>
	public void DeleteSubKeyTree(string subkey, bool throwOnMissingSubKey)
	{
		AssertKeyStillValid();
		AssertKeyNameNotNull(subkey);
		AssertKeyNameLength(subkey);
		RegistryKey registryKey = OpenSubKey(subkey, writable: true);
		if (registryKey == null)
		{
			if (throwOnMissingSubKey)
			{
				throw new ArgumentException("Cannot delete a subkey tree because the subkey does not exist.");
			}
		}
		else
		{
			registryKey.DeleteChildKeysAndValues();
			registryKey.Close();
			DeleteSubKey(subkey, throwOnMissingSubKey: false);
		}
	}

	/// <summary>Deletes the specified value from this key.</summary>
	/// <param name="name">The name of the value to delete. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is not a valid reference to a value. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to delete the value. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is read-only. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public void DeleteValue(string name)
	{
		DeleteValue(name, throwOnMissingValue: true);
	}

	/// <summary>Deletes the specified value from this key, and specifies whether an exception is raised if the value is not found.</summary>
	/// <param name="name">The name of the value to delete. </param>
	/// <param name="throwOnMissingValue">Indicates whether an exception should be raised if the specified value cannot be found. If this argument is true and the specified value does not exist, an exception is raised. If this argument is false and the specified value does not exist, no action is taken. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is not a valid reference to a value and <paramref name="throwOnMissingValue" /> is true. -or- <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to delete the value. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is read-only. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public void DeleteValue(string name, bool throwOnMissingValue)
	{
		AssertKeyStillValid();
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!IsWritable)
		{
			throw new UnauthorizedAccessException("Cannot write to the registry key.");
		}
		RegistryApi.DeleteValue(this, name, throwOnMissingValue);
	}

	/// <summary>Returns the access control security for the current registry key.</summary>
	/// <returns>An object that describes the access control permissions on the registry key represented by the current <see cref="T:Microsoft.Win32.RegistryKey" />.</returns>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the necessary permissions.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed).</exception>
	/// <exception cref="T:System.InvalidOperationException">The current key has been deleted.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public RegistrySecurity GetAccessControl()
	{
		return GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	/// <summary>Returns the specified sections of the access control security for the current registry key.</summary>
	/// <returns>An object that describes the access control permissions on the registry key represented by the current <see cref="T:Microsoft.Win32.RegistryKey" />.</returns>
	/// <param name="includeSections">A bitwise combination of enumeration values that specifies the type of security information to get. </param>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the necessary permissions.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed).</exception>
	/// <exception cref="T:System.InvalidOperationException">The current key has been deleted.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public RegistrySecurity GetAccessControl(AccessControlSections includeSections)
	{
		return new RegistrySecurity(Name, includeSections);
	}

	/// <summary>Retrieves an array of strings that contains all the subkey names.</summary>
	/// <returns>An array of strings that contains the names of the subkeys for the current key.</returns>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <exception cref="T:System.IO.IOException">A system error occurred, for example the current key has been deleted.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string[] GetSubKeyNames()
	{
		AssertKeyStillValid();
		return RegistryApi.GetSubKeyNames(this);
	}

	/// <summary>Retrieves an array of strings that contains all the value names associated with this key.</summary>
	/// <returns>An array of strings that contains the value names for the current key.</returns>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the registry key. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" />  being manipulated is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <exception cref="T:System.IO.IOException">A system error occurred; for example, the current key has been deleted.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string[] GetValueNames()
	{
		AssertKeyStillValid();
		return RegistryApi.GetValueNames(this);
	}

	/// <summary>Creates a registry key from a specified handle.</summary>
	/// <returns>A registry key.</returns>
	/// <param name="handle">The handle to the registry key.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handle" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to perform this action.</exception>
	[ComVisible(false)]
	[MonoTODO("Not implemented on unix")]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public static RegistryKey FromHandle(SafeRegistryHandle handle)
	{
		if (handle == null)
		{
			throw new ArgumentNullException("handle");
		}
		return RegistryApi.FromHandle(handle);
	}

	/// <summary>Creates a registry key from a specified handle and registry view setting. </summary>
	/// <returns>A registry key.</returns>
	/// <param name="handle">The handle to the registry key.</param>
	/// <param name="view">The registry view to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="view" /> is invalid.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="handle" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to perform this action.</exception>
	[ComVisible(false)]
	[MonoTODO("Not implemented on unix")]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public static RegistryKey FromHandle(SafeRegistryHandle handle, RegistryView view)
	{
		return FromHandle(handle);
	}

	/// <summary>Opens a new <see cref="T:Microsoft.Win32.RegistryKey" /> that represents the requested key on a remote machine.</summary>
	/// <returns>The requested registry key.</returns>
	/// <param name="hKey">The HKEY to open, from the <see cref="T:Microsoft.Win32.RegistryHive" /> enumeration. </param>
	/// <param name="machineName">The remote machine. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="hKey" /> is invalid.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="machineName" /> is not found.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="machineName" /> is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the proper permissions to perform this operation. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[MonoTODO("Not implemented on unix")]
	public static RegistryKey OpenRemoteBaseKey(RegistryHive hKey, string machineName)
	{
		if (machineName == null)
		{
			throw new ArgumentNullException("machineName");
		}
		return RegistryApi.OpenRemoteBaseKey(hKey, machineName);
	}

	/// <summary>Opens a new registry key that represents the requested key on a remote machine with the specified view.</summary>
	/// <returns>The requested registry key.</returns>
	/// <param name="hKey">The HKEY to open from the <see cref="T:Microsoft.Win32.RegistryHive" /> enumeration.. </param>
	/// <param name="machineName">The remote machine.</param>
	/// <param name="view">The registry view to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="hKey" /> or <paramref name="view" /> is invalid.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="machineName" /> is not found.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="machineName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="machineName" /> is null. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the required permissions to perform this operation.</exception>
	[ComVisible(false)]
	[MonoTODO("Not implemented on unix")]
	public static RegistryKey OpenRemoteBaseKey(RegistryHive hKey, string machineName, RegistryView view)
	{
		if (machineName == null)
		{
			throw new ArgumentNullException("machineName");
		}
		return RegistryApi.OpenRemoteBaseKey(hKey, machineName);
	}

	/// <summary>Opens a new <see cref="T:Microsoft.Win32.RegistryKey" /> that represents the requested key on the local machine with the specified view.</summary>
	/// <returns>The requested registry key.</returns>
	/// <param name="hKey">The HKEY to open.</param>
	/// <param name="view">The registry view to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="hKey" /> or <paramref name="view" /> is invalid.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have the necessary registry rights.</exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to perform this action.</exception>
	[MonoLimitation("View is ignored in Mono")]
	[ComVisible(false)]
	public static RegistryKey OpenBaseKey(RegistryHive hKey, RegistryView view)
	{
		return hKey switch
		{
			RegistryHive.ClassesRoot => Registry.ClassesRoot, 
			RegistryHive.CurrentConfig => Registry.CurrentConfig, 
			RegistryHive.CurrentUser => Registry.CurrentUser, 
			RegistryHive.DynData => Registry.DynData, 
			RegistryHive.LocalMachine => Registry.LocalMachine, 
			RegistryHive.PerformanceData => Registry.PerformanceData, 
			RegistryHive.Users => Registry.Users, 
			_ => throw new ArgumentException("hKey"), 
		};
	}

	/// <summary>Retrieves the specified subkey for read or read/write access.</summary>
	/// <returns>The subkey requested, or null if the operation failed.</returns>
	/// <param name="name">The name or path of the subkey to create or open.</param>
	/// <param name="permissionCheck">One of the enumeration values that specifies whether the key is opened for read or read/write access.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="permissionCheck" /> contains an invalid value.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read the registry key. </exception>
	[ComVisible(false)]
	public RegistryKey OpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck)
	{
		return OpenSubKey(name, permissionCheck == RegistryKeyPermissionCheck.ReadWriteSubTree);
	}

	[MonoLimitation("rights are ignored in Mono")]
	[ComVisible(false)]
	public RegistryKey OpenSubKey(string name, RegistryRights rights)
	{
		return OpenSubKey(name);
	}

	/// <summary>Retrieves the specified subkey for read or read/write access, requesting the specified access rights.</summary>
	/// <returns>The subkey requested, or null if the operation failed.</returns>
	/// <param name="name">The name or path of the subkey to create or open.</param>
	/// <param name="permissionCheck">One of the enumeration values that specifies whether the key is opened for read or read/write access.</param>
	/// <param name="rights">A bitwise combination of enumeration values that specifies the desired security access.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="permissionCheck" /> contains an invalid value.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is closed (closed keys cannot be accessed). </exception>
	/// <exception cref="T:System.Security.SecurityException">
	///   <paramref name="rights" /> includes invalid registry rights values.-or-The user does not have the requested permissions. </exception>
	[ComVisible(false)]
	[MonoLimitation("rights are ignored in Mono")]
	public RegistryKey OpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck, RegistryRights rights)
	{
		return OpenSubKey(name, permissionCheck == RegistryKeyPermissionCheck.ReadWriteSubTree);
	}

	/// <summary>Applies Windows access control security to an existing registry key.</summary>
	/// <param name="registrySecurity">The access control security to apply to the current subkey. </param>
	/// <exception cref="T:System.UnauthorizedAccessException">The current <see cref="T:Microsoft.Win32.RegistryKey" /> object represents a key with access control security, and the caller does not have <see cref="F:System.Security.AccessControl.RegistryRights.ChangePermissions" /> rights.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="registrySecurity" /> is null.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being manipulated is closed (closed keys cannot be accessed).</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetAccessControl(RegistrySecurity registrySecurity)
	{
		if (registrySecurity == null)
		{
			throw new ArgumentNullException("registrySecurity");
		}
		registrySecurity.PersistModifications(Name);
	}

	/// <summary>Retrieves a string representation of this key.</summary>
	/// <returns>A string representing the key. If the specified key is invalid (cannot be found) then null is returned.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:Microsoft.Win32.RegistryKey" /> being accessed is closed (closed keys cannot be accessed). </exception>
	public override string ToString()
	{
		AssertKeyStillValid();
		return RegistryApi.ToString(this);
	}

	private void AssertKeyStillValid()
	{
		if (handle == null)
		{
			throw new ObjectDisposedException("Microsoft.Win32.RegistryKey");
		}
	}

	private void AssertKeyNameNotNull(string subKeyName)
	{
		if (subKeyName == null)
		{
			throw new ArgumentNullException("name");
		}
	}

	private void AssertKeyNameLength(string name)
	{
		if (name.Length > 255)
		{
			throw new ArgumentException("Name of registry key cannot be greater than 255 characters");
		}
	}

	private void DeleteChildKeysAndValues()
	{
		if (!IsRoot)
		{
			string[] subKeyNames = GetSubKeyNames();
			foreach (string text in subKeyNames)
			{
				RegistryKey registryKey = OpenSubKey(text, writable: true);
				registryKey.DeleteChildKeysAndValues();
				registryKey.Close();
				DeleteSubKey(text, throwOnMissingSubKey: false);
			}
			subKeyNames = GetValueNames();
			foreach (string name in subKeyNames)
			{
				DeleteValue(name, throwOnMissingValue: false);
			}
		}
	}

	internal static string DecodeString(byte[] data)
	{
		string text = Encoding.Unicode.GetString(data);
		if (text.IndexOf('\0') != -1)
		{
			text = text.TrimEnd(default(char));
		}
		return text;
	}

	internal static IOException CreateMarkedForDeletionException()
	{
		throw new IOException("Illegal operation attempted on a registry key that has been marked for deletion.");
	}

	private static string GetHiveName(RegistryHive hive)
	{
		return hive switch
		{
			RegistryHive.ClassesRoot => "HKEY_CLASSES_ROOT", 
			RegistryHive.CurrentConfig => "HKEY_CURRENT_CONFIG", 
			RegistryHive.CurrentUser => "HKEY_CURRENT_USER", 
			RegistryHive.DynData => "HKEY_DYN_DATA", 
			RegistryHive.LocalMachine => "HKEY_LOCAL_MACHINE", 
			RegistryHive.PerformanceData => "HKEY_PERFORMANCE_DATA", 
			RegistryHive.Users => "HKEY_USERS", 
			_ => throw new NotImplementedException($"Registry hive '{hive.ToString()}' is not implemented."), 
		};
	}

	internal RegistryKey()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
