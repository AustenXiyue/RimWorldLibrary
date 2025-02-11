using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security;
using System.Security.Policy;

namespace System.Runtime.Hosting;

/// <summary>Provides the base class for the activation of manifest-based assemblies. </summary>
[MonoTODO("missing manifest support")]
[ComVisible(true)]
public class ApplicationActivator
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Hosting.ApplicationActivator" /> class. </summary>
	public ApplicationActivator()
	{
	}

	/// <summary>Creates an instance of the application to be activated, using the specified activation context. </summary>
	/// <returns>An <see cref="T:System.Runtime.Remoting.ObjectHandle" /> that is a wrapper for the return value of the application execution. The return value must be unwrapped to access the real object.  </returns>
	/// <param name="activationContext">An <see cref="T:System.ActivationContext" /> that identifies the application to activate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="activationContext" /> is null. </exception>
	public virtual ObjectHandle CreateInstance(ActivationContext activationContext)
	{
		return CreateInstance(activationContext, null);
	}

	/// <summary>Creates an instance of the application to be activated, using the specified activation context  and custom activation data.  </summary>
	/// <returns>An <see cref="T:System.Runtime.Remoting.ObjectHandle" /> that is a wrapper for the return value of the application execution. The return value must be unwrapped to access the real object.</returns>
	/// <param name="activationContext">An <see cref="T:System.ActivationContext" /> that identifies the application to activate.</param>
	/// <param name="activationCustomData">Custom activation data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="activationContext" /> is null. </exception>
	public virtual ObjectHandle CreateInstance(ActivationContext activationContext, string[] activationCustomData)
	{
		if (activationContext == null)
		{
			throw new ArgumentNullException("activationContext");
		}
		return CreateInstanceHelper(new AppDomainSetup(activationContext));
	}

	/// <summary>Creates an instance of an application using the specified <see cref="T:System.AppDomainSetup" />  object.</summary>
	/// <returns>An <see cref="T:System.Runtime.Remoting.ObjectHandle" /> that is a wrapper for the return value of the application execution. The return value must be unwrapped to access the real object. </returns>
	/// <param name="adSetup">An <see cref="T:System.AppDomainSetup" /> object whose <see cref="P:System.AppDomainSetup.ActivationArguments" /> property identifies the application to activate.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.AppDomainSetup.ActivationArguments" /> property of <paramref name="adSetup " />is null. </exception>
	/// <exception cref="T:System.Security.Policy.PolicyException">The application instance failed to execute because the policy settings on the current application domain do not provide permission for this application to run.</exception>
	protected static ObjectHandle CreateInstanceHelper(AppDomainSetup adSetup)
	{
		if (adSetup == null)
		{
			throw new ArgumentNullException("adSetup");
		}
		if (adSetup.ActivationArguments == null)
		{
			throw new ArgumentException(string.Format(Locale.GetText("{0} is missing it's {1} property"), "AppDomainSetup", "ActivationArguments"), "adSetup");
		}
		HostSecurityManager hostSecurityManager = null;
		hostSecurityManager = ((AppDomain.CurrentDomain.DomainManager == null) ? new HostSecurityManager() : AppDomain.CurrentDomain.DomainManager.HostSecurityManager);
		Evidence evidence = new Evidence();
		evidence.AddHost(adSetup.ActivationArguments);
		TrustManagerContext context = new TrustManagerContext();
		if (!hostSecurityManager.DetermineApplicationTrust(evidence, null, context).IsApplicationTrustedToRun)
		{
			throw new PolicyException(Locale.GetText("Current policy doesn't allow execution of addin."));
		}
		return AppDomain.CreateDomain("friendlyName", null, adSetup).CreateInstance("assemblyName", "typeName", null);
	}
}
