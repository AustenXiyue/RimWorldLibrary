using System.Reflection;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

namespace System;

/// <summary>Provides a managed equivalent of an unmanaged host.</summary>
/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permissions. See the Requirements section.</exception>
/// <filterpriority>2</filterpriority>
[ComVisible(true)]
[SecurityPermission(SecurityAction.LinkDemand, Infrastructure = true)]
[SecurityPermission(SecurityAction.InheritanceDemand, Infrastructure = true)]
public class AppDomainManager : MarshalByRefObject
{
	private ApplicationActivator _activator;

	private AppDomainManagerInitializationOptions _flags;

	/// <summary>Gets the application activator that handles the activation of add-ins and manifest-based applications for the domain.</summary>
	/// <returns>The application activator.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual ApplicationActivator ApplicationActivator
	{
		get
		{
			if (_activator == null)
			{
				_activator = new ApplicationActivator();
			}
			return _activator;
		}
	}

	/// <summary>Gets the entry assembly for an application.</summary>
	/// <returns>The entry assembly for the application.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual Assembly EntryAssembly => Assembly.GetEntryAssembly();

	/// <summary>Gets the host execution context manager that manages the flow of the execution context.</summary>
	/// <returns>The host execution context manager.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	[MonoTODO]
	public virtual HostExecutionContextManager HostExecutionContextManager
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the host security manager that participates in security decisions for the application domain.</summary>
	/// <returns>The host security manager.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual HostSecurityManager HostSecurityManager => null;

	/// <summary>Gets the initialization flags for custom application domain managers.</summary>
	/// <returns>A bitwise combination of the enumeration values that describe the initialization action to perform. The default is <see cref="F:System.AppDomainManagerInitializationOptions.None" />.</returns>
	/// <filterpriority>1</filterpriority>
	public AppDomainManagerInitializationOptions InitializationFlags
	{
		get
		{
			return _flags;
		}
		set
		{
			_flags = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.AppDomainManager" /> class. </summary>
	public AppDomainManager()
	{
		_flags = AppDomainManagerInitializationOptions.None;
	}

	/// <summary>Returns a new or existing application domain.</summary>
	/// <returns>A new or existing application domain.</returns>
	/// <param name="friendlyName">The friendly name of the domain. </param>
	/// <param name="securityInfo">An object that contains evidence mapped through the security policy to establish a top-of-stack permission set.</param>
	/// <param name="appDomainInfo">An object that contains application domain initialization information.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlAppDomain, Infrastructure" />
	/// </PermissionSet>
	public virtual AppDomain CreateDomain(string friendlyName, Evidence securityInfo, AppDomainSetup appDomainInfo)
	{
		InitializeNewDomain(appDomainInfo);
		AppDomain appDomain = CreateDomainHelper(friendlyName, securityInfo, appDomainInfo);
		if ((HostSecurityManager.Flags & HostSecurityManagerOptions.HostPolicyLevel) == HostSecurityManagerOptions.HostPolicyLevel)
		{
			PolicyLevel domainPolicy = HostSecurityManager.DomainPolicy;
			if (domainPolicy != null)
			{
				appDomain.SetAppDomainPolicy(domainPolicy);
			}
		}
		return appDomain;
	}

	/// <summary>Initializes the new application domain.</summary>
	/// <param name="appDomainInfo">An object that contains application domain initialization information.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual void InitializeNewDomain(AppDomainSetup appDomainInfo)
	{
	}

	/// <summary>Indicates whether the specified operation is allowed in the application domain.</summary>
	/// <returns>true if the host allows the operation specified by <paramref name="state" /> to be performed in the application domain; otherwise, false.</returns>
	/// <param name="state">A subclass of <see cref="T:System.Security.SecurityState" /> that identifies the operation whose security status is requested.</param>
	public virtual bool CheckSecuritySettings(SecurityState state)
	{
		return false;
	}

	/// <summary>Provides a helper method to create an application domain.</summary>
	/// <returns>A newly created application domain.</returns>
	/// <param name="friendlyName">The friendly name of the domain. </param>
	/// <param name="securityInfo">An object that contains evidence mapped through the security policy to establish a top-of-stack permission set.</param>
	/// <param name="appDomainInfo">An object that contains application domain initialization information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="friendlyName" /> is null. </exception>
	protected static AppDomain CreateDomainHelper(string friendlyName, Evidence securityInfo, AppDomainSetup appDomainInfo)
	{
		return AppDomain.CreateDomain(friendlyName, securityInfo, appDomainInfo);
	}
}
