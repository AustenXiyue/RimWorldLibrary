using System.Security.Permissions;
using System.Threading;

namespace System.Security;

/// <summary>Encapsulates and propagates all security-related data for execution contexts transferred across threads. This class cannot be inherited.</summary>
public sealed class SecurityContext : IDisposable
{
	private SecurityContext()
	{
	}

	/// <summary>Creates a copy of the current security context.</summary>
	/// <returns>The security context for the current thread.</returns>
	/// <exception cref="T:System.InvalidOperationException">The current security context has been previously used, was marshaled across application domains, or was not acquired through the <see cref="M:System.Security.SecurityContext.Capture" /> method.</exception>
	public SecurityContext CreateCopy()
	{
		return this;
	}

	/// <summary>Captures the security context for the current thread.</summary>
	/// <returns>The security context for the current thread.</returns>
	public static SecurityContext Capture()
	{
		return new SecurityContext();
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Security.SecurityContext" /> class.</summary>
	public void Dispose()
	{
	}

	/// <summary>Determines whether the flow of the security context has been suppressed.</summary>
	/// <returns>true if the flow has been suppressed; otherwise, false. </returns>
	public static bool IsFlowSuppressed()
	{
		return false;
	}

	/// <summary>Determines whether the flow of the Windows identity portion of the current security context has been suppressed.</summary>
	/// <returns>true if the flow has been suppressed; otherwise, false. </returns>
	public static bool IsWindowsIdentityFlowSuppressed()
	{
		return false;
	}

	/// <summary>Restores the flow of the security context across asynchronous threads.</summary>
	/// <exception cref="T:System.InvalidOperationException">The security context is null or an empty string.</exception>
	public static void RestoreFlow()
	{
	}

	/// <summary>Runs the specified method in the specified security context on the current thread.</summary>
	/// <param name="securityContext">The security context to set.</param>
	/// <param name="callback">The delegate that represents the method to run in the specified security context.</param>
	/// <param name="state">The object to pass to the callback method.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="securityContext" /> is null.-or-<paramref name="securityContext" /> was not acquired through a capture operation. -or-<paramref name="securityContext" /> has already been used as the argument to a <see cref="M:System.Security.SecurityContext.Run(System.Security.SecurityContext,System.Threading.ContextCallback,System.Object)" /> method call.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.LinkDemand, Infrastructure = true)]
	[SecurityPermission(SecurityAction.Assert, ControlPrincipal = true)]
	public static void Run(SecurityContext securityContext, ContextCallback callback, object state)
	{
		callback(state);
	}

	/// <summary>Suppresses the flow of the security context across asynchronous threads.</summary>
	/// <returns>An <see cref="T:System.Threading.AsyncFlowControl" /> structure for restoring the flow.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.LinkDemand, Infrastructure = true)]
	public static AsyncFlowControl SuppressFlow()
	{
		throw new NotSupportedException();
	}

	/// <summary>Suppresses the flow of the Windows identity portion of the current security context across asynchronous threads.</summary>
	/// <returns>A structure for restoring the flow.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static AsyncFlowControl SuppressFlowWindowsIdentity()
	{
		throw new NotSupportedException();
	}
}
