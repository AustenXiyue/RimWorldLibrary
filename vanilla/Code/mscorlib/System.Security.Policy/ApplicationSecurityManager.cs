using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Security.Policy;

/// <summary>Manages trust decisions for manifest-activated applications. </summary>
[ComVisible(true)]
public static class ApplicationSecurityManager
{
	private static IApplicationTrustManager _appTrustManager;

	private static ApplicationTrustCollection _userAppTrusts;

	/// <summary>Gets the current application trust manager.</summary>
	/// <returns>An <see cref="T:System.Security.Policy.IApplicationTrustManager" /> that represents the current trust manager.</returns>
	/// <exception cref="T:System.Security.Policy.PolicyException">The policy on this application does not have a trust manager.</exception>
	public static IApplicationTrustManager ApplicationTrustManager
	{
		[SecurityPermission(SecurityAction.Demand, ControlPolicy = true)]
		get
		{
			if (_appTrustManager == null)
			{
				_appTrustManager = new MonoTrustManager();
			}
			return _appTrustManager;
		}
	}

	/// <summary>Gets an application trust collection that contains the cached trust decisions for the user.</summary>
	/// <returns>An <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> that contains the cached trust decisions for the user.</returns>
	public static ApplicationTrustCollection UserApplicationTrusts
	{
		get
		{
			if (_userAppTrusts == null)
			{
				_userAppTrusts = new ApplicationTrustCollection();
			}
			return _userAppTrusts;
		}
	}

	/// <summary>Determines whether the user approves the specified application to execute with the requested permission set.</summary>
	/// <returns>true to execute the specified application; otherwise, false.</returns>
	/// <param name="activationContext">An <see cref="T:System.ActivationContext" /> identifying the activation context for the application.</param>
	/// <param name="context">A <see cref="T:System.Security.Policy.TrustManagerContext" />  identifying the trust manager context for the application.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="activationContext" /> parameter is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[MonoTODO("Missing application manifest support")]
	[SecurityPermission(SecurityAction.Demand, ControlPolicy = true, ControlEvidence = true)]
	public static bool DetermineApplicationTrust(ActivationContext activationContext, TrustManagerContext context)
	{
		if (activationContext == null)
		{
			throw new NullReferenceException("activationContext");
		}
		return ApplicationTrustManager.DetermineApplicationTrust(activationContext, context).IsApplicationTrustedToRun;
	}
}
