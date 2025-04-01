using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace System.Threading;

/// <summary>Represents a thread synchronization event.</summary>
/// <filterpriority>2</filterpriority>
[ComVisible(true)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class EventWaitHandle : WaitHandle
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.EventWaitHandle" /> class, specifying whether the wait handle is initially signaled, and whether it resets automatically or manually.</summary>
	/// <param name="initialState">true to set the initial state to signaled; false to set it to nonsignaled.</param>
	/// <param name="mode">One of the <see cref="T:System.Threading.EventResetMode" /> values that determines whether the event resets automatically or manually.</param>
	[SecuritySafeCritical]
	public EventWaitHandle(bool initialState, EventResetMode mode)
		: this(initialState, mode, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.EventWaitHandle" /> class, specifying whether the wait handle is initially signaled if created as a result of this call, whether it resets automatically or manually, and the name of a system synchronization event.</summary>
	/// <param name="initialState">true to set the initial state to signaled if the named event is created as a result of this call; false to set it to nonsignaled.</param>
	/// <param name="mode">One of the <see cref="T:System.Threading.EventResetMode" /> values that determines whether the event resets automatically or manually.</param>
	/// <param name="name">The name of a system-wide synchronization event.</param>
	/// <exception cref="T:System.IO.IOException">A Win32 error occurred.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The named event exists and has access control security, but the user does not have <see cref="F:System.Security.AccessControl.EventWaitHandleRights.FullControl" />.</exception>
	/// <exception cref="T:System.Threading.WaitHandleCannotBeOpenedException">The named event cannot be created, perhaps because a wait handle of a different type has the same name.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is longer than 260 characters.</exception>
	[SecurityCritical]
	public EventWaitHandle(bool initialState, EventResetMode mode, string name)
	{
		if (name != null && 260 < name.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("The name can be no more than 260 characters in length.", name));
		}
		SafeWaitHandle safeWaitHandle = null;
		safeWaitHandle = mode switch
		{
			EventResetMode.ManualReset => new SafeWaitHandle(NativeEventCalls.CreateEvent_internal(manual: true, initialState, name, out var errorCode), ownsHandle: true), 
			EventResetMode.AutoReset => new SafeWaitHandle(NativeEventCalls.CreateEvent_internal(manual: false, initialState, name, out errorCode), ownsHandle: true), 
			_ => throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid.", name)), 
		};
		if (safeWaitHandle.IsInvalid)
		{
			safeWaitHandle.SetHandleAsInvalid();
			if (name != null && name.Length != 0 && 6 == errorCode)
			{
				throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("A WaitHandle with system-wide name '{0}' cannot be created. A WaitHandle of a different type might have the same name.", name));
			}
			__Error.WinIOError(errorCode, name);
		}
		SetHandleInternal(safeWaitHandle);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.EventWaitHandle" /> class, specifying whether the wait handle is initially signaled if created as a result of this call, whether it resets automatically or manually, the name of a system synchronization event, and a Boolean variable whose value after the call indicates whether the named system event was created.</summary>
	/// <param name="initialState">true to set the initial state to signaled if the named event is created as a result of this call; false to set it to nonsignaled.</param>
	/// <param name="mode">One of the <see cref="T:System.Threading.EventResetMode" /> values that determines whether the event resets automatically or manually.</param>
	/// <param name="name">The name of a system-wide synchronization event.</param>
	/// <param name="createdNew">When this method returns, contains true if a local event was created (that is, if <paramref name="name" /> is null or an empty string) or if the specified named system event was created; false if the specified named system event already existed. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.IO.IOException">A Win32 error occurred.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The named event exists and has access control security, but the user does not have <see cref="F:System.Security.AccessControl.EventWaitHandleRights.FullControl" />.</exception>
	/// <exception cref="T:System.Threading.WaitHandleCannotBeOpenedException">The named event cannot be created, perhaps because a wait handle of a different type has the same name.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is longer than 260 characters.</exception>
	[SecurityCritical]
	public EventWaitHandle(bool initialState, EventResetMode mode, string name, out bool createdNew)
		: this(initialState, mode, name, out createdNew, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.EventWaitHandle" /> class, specifying whether the wait handle is initially signaled if created as a result of this call, whether it resets automatically or manually, the name of a system synchronization event, a Boolean variable whose value after the call indicates whether the named system event was created, and the access control security to be applied to the named event if it is created.</summary>
	/// <param name="initialState">true to set the initial state to signaled if the named event is created as a result of this call; false to set it to nonsignaled.</param>
	/// <param name="mode">One of the <see cref="T:System.Threading.EventResetMode" /> values that determines whether the event resets automatically or manually.</param>
	/// <param name="name">The name of a system-wide synchronization event.</param>
	/// <param name="createdNew">When this method returns, contains true if a local event was created (that is, if <paramref name="name" /> is null or an empty string) or if the specified named system event was created; false if the specified named system event already existed. This parameter is passed uninitialized.</param>
	/// <param name="eventSecurity">An <see cref="T:System.Security.AccessControl.EventWaitHandleSecurity" /> object that represents the access control security to be applied to the named system event.</param>
	/// <exception cref="T:System.IO.IOException">A Win32 error occurred.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The named event exists and has access control security, but the user does not have <see cref="F:System.Security.AccessControl.EventWaitHandleRights.FullControl" />.</exception>
	/// <exception cref="T:System.Threading.WaitHandleCannotBeOpenedException">The named event cannot be created, perhaps because a wait handle of a different type has the same name.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is longer than 260 characters.</exception>
	[SecurityCritical]
	public EventWaitHandle(bool initialState, EventResetMode mode, string name, out bool createdNew, EventWaitHandleSecurity eventSecurity)
	{
		if (name != null && 260 < name.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("The name can be no more than 260 characters in length.", name));
		}
		SafeWaitHandle safeWaitHandle = null;
		safeWaitHandle = new SafeWaitHandle(NativeEventCalls.CreateEvent_internal(mode switch
		{
			EventResetMode.ManualReset => true, 
			EventResetMode.AutoReset => false, 
			_ => throw new ArgumentException(Environment.GetResourceString("Value of flags is invalid.", name)), 
		}, initialState, name, out var errorCode), ownsHandle: true);
		if (safeWaitHandle.IsInvalid)
		{
			safeWaitHandle.SetHandleAsInvalid();
			if (name != null && name.Length != 0 && 6 == errorCode)
			{
				throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("A WaitHandle with system-wide name '{0}' cannot be created. A WaitHandle of a different type might have the same name.", name));
			}
			__Error.WinIOError(errorCode, name);
		}
		createdNew = errorCode != 183;
		SetHandleInternal(safeWaitHandle);
	}

	[SecurityCritical]
	private EventWaitHandle(SafeWaitHandle handle)
	{
		SetHandleInternal(handle);
	}

	/// <summary>Opens the specified named synchronization event, if it already exists.</summary>
	/// <returns>An  object that represents the named system event.</returns>
	/// <param name="name">The name of the system synchronization event to open.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an empty string. -or-<paramref name="name" /> is longer than 260 characters.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.Threading.WaitHandleCannotBeOpenedException">The named system event does not exist.</exception>
	/// <exception cref="T:System.IO.IOException">A Win32 error occurred.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The named event exists, but the user does not have the security access required to use it.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityCritical]
	public static EventWaitHandle OpenExisting(string name)
	{
		return OpenExisting(name, EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize);
	}

	/// <summary>Opens the specified named synchronization event, if it already exists, with the desired security access.</summary>
	/// <returns>An object that represents the named system event.</returns>
	/// <param name="name">The name of the system synchronization event to open.</param>
	/// <param name="rights">A bitwise combination of the enumeration values that represent the desired security access.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an empty string.-or-<paramref name="name" /> is longer than 260 characters.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.Threading.WaitHandleCannotBeOpenedException">The named system event does not exist.</exception>
	/// <exception cref="T:System.IO.IOException">A Win32 error occurred.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The named event exists, but the user does not have the desired security access.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityCritical]
	public static EventWaitHandle OpenExisting(string name, EventWaitHandleRights rights)
	{
		EventWaitHandle result;
		switch (OpenExistingWorker(name, rights, out result))
		{
		case OpenExistingResult.NameNotFound:
			throw new WaitHandleCannotBeOpenedException();
		case OpenExistingResult.NameInvalid:
			throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("A WaitHandle with system-wide name '{0}' cannot be created. A WaitHandle of a different type might have the same name.", name));
		case OpenExistingResult.PathNotFound:
			__Error.WinIOError(3, "");
			return result;
		default:
			return result;
		}
	}

	/// <summary>Opens the specified named synchronization event, if it already exists, and returns a value that indicates whether the operation succeeded.</summary>
	/// <returns>true if the named synchronization event was opened successfully; otherwise, false.</returns>
	/// <param name="name">The name of the system synchronization event to open.</param>
	/// <param name="result">When this method returns, contains a <see cref="T:System.Threading.EventWaitHandle" /> object that represents the named synchronization event if the call succeeded, or null if the call failed. This parameter is treated as uninitialized.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an empty string.-or-<paramref name="name" /> is longer than 260 characters.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.IO.IOException">A Win32 error occurred.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The named event exists, but the user does not have the desired security access.</exception>
	[SecurityCritical]
	public static bool TryOpenExisting(string name, out EventWaitHandle result)
	{
		return OpenExistingWorker(name, EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize, out result) == OpenExistingResult.Success;
	}

	/// <summary>Opens the specified named synchronization event, if it already exists, with the desired security access, and returns a value that indicates whether the operation succeeded. </summary>
	/// <returns>true if the named synchronization event was opened successfully; otherwise, false.</returns>
	/// <param name="name">The name of the system synchronization event to open.</param>
	/// <param name="rights">A bitwise combination of the enumeration values that represent the desired security access.</param>
	/// <param name="result">When this method returns, contains a <see cref="T:System.Threading.EventWaitHandle" /> object that represents the named synchronization event if the call succeeded, or null if the call failed. This parameter is treated as uninitialized. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is an empty string.-or-<paramref name="name" /> is longer than 260 characters.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.IO.IOException">A Win32 error occurred.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The named event exists, but the user does not have the desired security access.</exception>
	[SecurityCritical]
	public static bool TryOpenExisting(string name, EventWaitHandleRights rights, out EventWaitHandle result)
	{
		return OpenExistingWorker(name, rights, out result) == OpenExistingResult.Success;
	}

	[SecurityCritical]
	private static OpenExistingResult OpenExistingWorker(string name, EventWaitHandleRights rights, out EventWaitHandle result)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name", Environment.GetResourceString("Parameter '{0}' cannot be null."));
		}
		if (name.Length == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Empty name is not legal."), "name");
		}
		if (name != null && 260 < name.Length)
		{
			throw new ArgumentException(Environment.GetResourceString("The name can be no more than 260 characters in length.", name));
		}
		result = null;
		int errorCode;
		SafeWaitHandle safeWaitHandle = new SafeWaitHandle(NativeEventCalls.OpenEvent_internal(name, rights, out errorCode), ownsHandle: true);
		if (safeWaitHandle.IsInvalid)
		{
			if (2 == errorCode || 123 == errorCode)
			{
				return OpenExistingResult.NameNotFound;
			}
			if (3 == errorCode)
			{
				return OpenExistingResult.PathNotFound;
			}
			if (name != null && name.Length != 0 && 6 == errorCode)
			{
				return OpenExistingResult.NameInvalid;
			}
			__Error.WinIOError(errorCode, "");
		}
		result = new EventWaitHandle(safeWaitHandle);
		return OpenExistingResult.Success;
	}

	/// <summary>Sets the state of the event to nonsignaled, causing threads to block.</summary>
	/// <returns>true if the operation succeeds; otherwise, false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="M:System.Threading.EventWaitHandle.Close" /> method was previously called on this <see cref="T:System.Threading.EventWaitHandle" />.</exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public bool Reset()
	{
		bool num = NativeEventCalls.ResetEvent(safeWaitHandle);
		if (!num)
		{
			throw new IOException();
		}
		return num;
	}

	/// <summary>Sets the state of the event to signaled, allowing one or more waiting threads to proceed.</summary>
	/// <returns>true if the operation succeeds; otherwise, false.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="M:System.Threading.EventWaitHandle.Close" /> method was previously called on this <see cref="T:System.Threading.EventWaitHandle" />.</exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	public bool Set()
	{
		bool num = NativeEventCalls.SetEvent(safeWaitHandle);
		if (!num)
		{
			throw new IOException();
		}
		return num;
	}

	/// <summary>Gets an <see cref="T:System.Security.AccessControl.EventWaitHandleSecurity" /> object that represents the access control security for the named system event represented by the current <see cref="T:System.Threading.EventWaitHandle" /> object.</summary>
	/// <returns>An <see cref="T:System.Security.AccessControl.EventWaitHandleSecurity" /> object that represents the access control security for the named system event.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The current <see cref="T:System.Threading.EventWaitHandle" /> object represents a named system event, and the user does not have <see cref="F:System.Security.AccessControl.EventWaitHandleRights.ReadPermissions" />.-or-The current <see cref="T:System.Threading.EventWaitHandle" /> object represents a named system event, and was not opened with <see cref="F:System.Security.AccessControl.EventWaitHandleRights.ReadPermissions" />.</exception>
	/// <exception cref="T:System.NotSupportedException">Not supported for Windows 98 or Windows Millennium Edition.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="M:System.Threading.EventWaitHandle.Close" /> method was previously called on this <see cref="T:System.Threading.EventWaitHandle" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public EventWaitHandleSecurity GetAccessControl()
	{
		return new EventWaitHandleSecurity(safeWaitHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	/// <summary>Sets the access control security for a named system event.</summary>
	/// <param name="eventSecurity">An <see cref="T:System.Security.AccessControl.EventWaitHandleSecurity" />  object that represents the access control security to be applied to the named system event.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="eventSecurity" /> is null.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The user does not have <see cref="F:System.Security.AccessControl.EventWaitHandleRights.ChangePermissions" />.-or-The event was not opened with <see cref="F:System.Security.AccessControl.EventWaitHandleRights.ChangePermissions" />.</exception>
	/// <exception cref="T:System.SystemException">The current <see cref="T:System.Threading.EventWaitHandle" /> object does not represent a named system event.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="M:System.Threading.EventWaitHandle.Close" /> method was previously called on this <see cref="T:System.Threading.EventWaitHandle" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public void SetAccessControl(EventWaitHandleSecurity eventSecurity)
	{
		if (eventSecurity == null)
		{
			throw new ArgumentNullException("eventSecurity");
		}
		eventSecurity.Persist(safeWaitHandle);
	}
}
