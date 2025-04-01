using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Threading;

namespace System.Runtime.Remoting.Messaging;

/// <summary>Provides a set of properties that are carried with the execution code path. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
[SecurityCritical]
public sealed class CallContext
{
	internal static IPrincipal Principal
	{
		[SecurityCritical]
		get
		{
			return Thread.CurrentThread.GetExecutionContextReader().LogicalCallContext.Principal;
		}
		[SecurityCritical]
		set
		{
			Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext.Principal = value;
		}
	}

	/// <summary>Gets or sets the host context associated with the current thread.</summary>
	/// <returns>The host context associated with the current thread.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	public static object HostContext
	{
		[SecurityCritical]
		get
		{
			ExecutionContext.Reader executionContextReader = Thread.CurrentThread.GetExecutionContextReader();
			object hostContext = executionContextReader.IllogicalCallContext.HostContext;
			if (hostContext == null)
			{
				hostContext = executionContextReader.LogicalCallContext.HostContext;
			}
			return hostContext;
		}
		[SecurityCritical]
		set
		{
			ExecutionContext mutableExecutionContext = Thread.CurrentThread.GetMutableExecutionContext();
			if (value is ILogicalThreadAffinative)
			{
				mutableExecutionContext.IllogicalCallContext.HostContext = null;
				mutableExecutionContext.LogicalCallContext.HostContext = value;
			}
			else
			{
				mutableExecutionContext.IllogicalCallContext.HostContext = value;
				mutableExecutionContext.LogicalCallContext.HostContext = null;
			}
		}
	}

	private CallContext()
	{
	}

	internal static object SetCurrentCallContext(LogicalCallContext ctx)
	{
		return null;
	}

	internal static LogicalCallContext SetLogicalCallContext(LogicalCallContext callCtx)
	{
		ExecutionContext mutableExecutionContext = Thread.CurrentThread.GetMutableExecutionContext();
		LogicalCallContext logicalCallContext = mutableExecutionContext.LogicalCallContext;
		mutableExecutionContext.LogicalCallContext = callCtx;
		return logicalCallContext;
	}

	/// <summary>Empties a data slot with the specified name.</summary>
	/// <param name="name">The name of the data slot to empty. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public static void FreeNamedDataSlot(string name)
	{
		ExecutionContext mutableExecutionContext = Thread.CurrentThread.GetMutableExecutionContext();
		mutableExecutionContext.LogicalCallContext.FreeNamedDataSlot(name);
		mutableExecutionContext.IllogicalCallContext.FreeNamedDataSlot(name);
	}

	/// <summary>Retrieves an object with the specified name from the logical call context.</summary>
	/// <returns>The object in the logical call context associated with the specified name.</returns>
	/// <param name="name">The name of the item in the logical call context. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	[SecurityCritical]
	public static object LogicalGetData(string name)
	{
		return Thread.CurrentThread.GetExecutionContextReader().LogicalCallContext.GetData(name);
	}

	private static object IllogicalGetData(string name)
	{
		return Thread.CurrentThread.GetExecutionContextReader().IllogicalCallContext.GetData(name);
	}

	/// <summary>Retrieves an object with the specified name from the <see cref="T:System.Runtime.Remoting.Messaging.CallContext" />.</summary>
	/// <returns>The object in the call context associated with the specified name.</returns>
	/// <param name="name">The name of the item in the call context. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public static object GetData(string name)
	{
		object obj = LogicalGetData(name);
		if (obj == null)
		{
			return IllogicalGetData(name);
		}
		return obj;
	}

	/// <summary>Stores a given object and associates it with the specified name.</summary>
	/// <param name="name">The name with which to associate the new item in the call context. </param>
	/// <param name="data">The object to store in the call context. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public static void SetData(string name, object data)
	{
		if (data is ILogicalThreadAffinative)
		{
			LogicalSetData(name, data);
			return;
		}
		ExecutionContext mutableExecutionContext = Thread.CurrentThread.GetMutableExecutionContext();
		mutableExecutionContext.LogicalCallContext.FreeNamedDataSlot(name);
		mutableExecutionContext.IllogicalCallContext.SetData(name, data);
	}

	/// <summary>Stores a given object in the logical call context and associates it with the specified name.</summary>
	/// <param name="name">The name with which to associate the new item in the logical call context. </param>
	/// <param name="data">The object to store in the logical call context. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	[SecurityCritical]
	public static void LogicalSetData(string name, object data)
	{
		ExecutionContext mutableExecutionContext = Thread.CurrentThread.GetMutableExecutionContext();
		mutableExecutionContext.IllogicalCallContext.FreeNamedDataSlot(name);
		mutableExecutionContext.LogicalCallContext.SetData(name, data);
	}

	/// <summary>Returns the headers that are sent along with the method call.</summary>
	/// <returns>The headers that are sent along with the method call.</returns>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public static Header[] GetHeaders()
	{
		return Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext.InternalGetHeaders();
	}

	/// <summary>Sets the headers that are sent along with the method call.</summary>
	/// <param name="headers">A <see cref="T:System.Runtime.Remoting.Messaging.Header" /> array of the headers that are to be sent along with the method call. </param>
	/// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityCritical]
	public static void SetHeaders(Header[] headers)
	{
		Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext.InternalSetHeaders(headers);
	}
}
