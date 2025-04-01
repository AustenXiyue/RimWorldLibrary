using System.Reflection;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Unity;

namespace System.Security;

/// <summary>Allows the control and customization of security behavior for application domains.</summary>
[Serializable]
[ComVisible(true)]
public class HostSecurityManager
{
	/// <summary>When overridden in a derived class, gets the security policy for the current application domain.</summary>
	/// <returns>The security policy for the current application domain. The default is null.</returns>
	/// <exception cref="T:System.NotSupportedException">This method uses code access security (CAS) policy, which is obsolete in the .NET Framework 4. To enable CAS policy for compatibility with earlier versions of the .NET Framework, use the &lt;legacyCasPolicy&gt; element.&lt;NetFx40_LegacySecurityPolicy&gt; Element</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual PolicyLevel DomainPolicy => null;

	/// <summary>Gets the flag representing the security policy components of concern to the host.</summary>
	/// <returns>One of the enumeration values that specifies security policy components. The default is <see cref="F:System.Security.HostSecurityManagerOptions.AllFlags" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual HostSecurityManagerOptions Flags => HostSecurityManagerOptions.AllFlags;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.HostSecurityManager" /> class.</summary>
	public HostSecurityManager()
	{
	}

	/// <summary>Determines whether an application should be executed.</summary>
	/// <returns>An object that contains trust information about the application.</returns>
	/// <param name="applicationEvidence">The evidence for the application to be activated.</param>
	/// <param name="activatorEvidence">Optionally, the evidence for the activating application domain. </param>
	/// <param name="context">The trust context. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="applicationEvidence" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">An <see cref="T:System.Runtime.Hosting.ActivationArguments" /> object could not be found in the application evidence.-or-The <see cref="P:System.Runtime.Hosting.ActivationArguments.ActivationContext" /> property in the activation arguments is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Security.Policy.ApplicationTrust" /> grant set does not contain the minimum request set specified by the <see cref="T:System.ActivationContext" />.</exception>
	public virtual ApplicationTrust DetermineApplicationTrust(Evidence applicationEvidence, Evidence activatorEvidence, TrustManagerContext context)
	{
		if (applicationEvidence == null)
		{
			throw new ArgumentNullException("applicationEvidence");
		}
		ActivationArguments activationArguments = null;
		foreach (object item in applicationEvidence)
		{
			activationArguments = item as ActivationArguments;
			if (activationArguments != null)
			{
				break;
			}
		}
		if (activationArguments == null)
		{
			throw new ArgumentException(string.Format(Locale.GetText("No {0} found in {1}."), "ActivationArguments", "Evidence"), "applicationEvidence");
		}
		if (activationArguments.ActivationContext == null)
		{
			throw new ArgumentException(string.Format(Locale.GetText("No {0} found in {1}."), "ActivationContext", "ActivationArguments"), "applicationEvidence");
		}
		if (ApplicationSecurityManager.DetermineApplicationTrust(activationArguments.ActivationContext, context))
		{
			if (activationArguments.ApplicationIdentity == null)
			{
				return new ApplicationTrust();
			}
			return new ApplicationTrust(activationArguments.ApplicationIdentity);
		}
		return null;
	}

	/// <summary>Provides the application domain evidence for an assembly being loaded.</summary>
	/// <returns>The evidence to be used for the <see cref="T:System.AppDomain" />.</returns>
	/// <param name="inputEvidence">Additional evidence to add to the <see cref="T:System.AppDomain" /> evidence.</param>
	public virtual Evidence ProvideAppDomainEvidence(Evidence inputEvidence)
	{
		return inputEvidence;
	}

	/// <summary>Provides the assembly evidence for an assembly being loaded.</summary>
	/// <returns>The evidence to be used for the assembly.</returns>
	/// <param name="loadedAssembly">The loaded assembly. </param>
	/// <param name="inputEvidence">Additional evidence to add to the assembly evidence.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public virtual Evidence ProvideAssemblyEvidence(Assembly loadedAssembly, Evidence inputEvidence)
	{
		return inputEvidence;
	}

	/// <summary>Determines what permissions to grant to code based on the specified evidence.</summary>
	/// <returns>The permission set that can be granted by the security system.</returns>
	/// <param name="evidence">The evidence set used to evaluate policy.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="evidence" /> is null.</exception>
	public virtual PermissionSet ResolvePolicy(Evidence evidence)
	{
		if (evidence == null)
		{
			throw new NullReferenceException("evidence");
		}
		return SecurityManager.ResolvePolicy(evidence);
	}

	/// <summary>Requests a specific evidence type for the application domain.</summary>
	/// <returns>The requested application domain evidence.</returns>
	/// <param name="evidenceType">The evidence type.</param>
	public virtual EvidenceBase GenerateAppDomainEvidence(Type evidenceType)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Requests a specific evidence type for the assembly.</summary>
	/// <returns>The requested assembly evidence.</returns>
	/// <param name="evidenceType">The evidence type.</param>
	/// <param name="assembly">The target assembly.</param>
	public virtual EvidenceBase GenerateAssemblyEvidence(Type evidenceType, Assembly assembly)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Determines which evidence types the host can supply for the application domain, if requested.</summary>
	/// <returns>An array of evidence types.</returns>
	public virtual Type[] GetHostSuppliedAppDomainEvidenceTypes()
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Determines which evidence types the host can supply for the assembly, if requested.</summary>
	/// <returns>An array of evidence types.</returns>
	/// <param name="assembly">The target assembly.</param>
	public virtual Type[] GetHostSuppliedAssemblyEvidenceTypes(Assembly assembly)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
