using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Diagnostics;

/// <summary>Enables communication with a debugger. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[ComVisible(true)]
[MonoTODO("The Debugger class is not functional")]
public sealed class Debugger
{
	/// <summary>Represents the default category of message with a constant.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly string DefaultCategory = "";

	/// <summary>Gets a value that indicates whether a debugger is attached to the process.</summary>
	/// <returns>true if a debugger is attached; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public static bool IsAttached => IsAttached_internal();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool IsAttached_internal();

	/// <summary>Signals a breakpoint to an attached debugger.</summary>
	/// <exception cref="T:System.Security.SecurityException">The <see cref="T:System.Security.Permissions.UIPermission" /> is not set to break into the debugger. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Break()
	{
	}

	/// <summary>Checks to see if logging is enabled by an attached debugger.</summary>
	/// <returns>true if a debugger is attached and logging is enabled; otherwise, false. The attached debugger is the registered managed debugger in the DbgManagedDebugger registry key. For more information on this key, see Enabling JIT-Attach Debugging.</returns>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern bool IsLogging();

	/// <summary>Launches and attaches a debugger to the process.</summary>
	/// <returns>true if the startup is successful or if the debugger is already attached; otherwise, false.</returns>
	/// <exception cref="T:System.Security.SecurityException">The <see cref="T:System.Security.Permissions.UIPermission" /> is not set to start the debugger. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[MonoTODO("Not implemented")]
	public static bool Launch()
	{
		throw new NotImplementedException();
	}

	/// <summary>Posts a message for the attached debugger.</summary>
	/// <param name="level">A description of the importance of the message. </param>
	/// <param name="category">The category of the message. </param>
	/// <param name="message">The message to show. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void Log(int level, string category, string message);

	/// <summary>Notifies a debugger that execution is about to enter a path that involves a cross-thread dependency.</summary>
	public static void NotifyOfCrossThreadDependency()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Debugger" /> class. </summary>
	[Obsolete("Call the static methods directly on this type", true)]
	public Debugger()
	{
	}
}
