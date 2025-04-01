using System.Runtime.InteropServices;
using System.Security.Policy;

namespace System.Runtime.Hosting;

/// <summary>Provides data for manifest-based activation of an application. This class cannot be inherited. </summary>
[Serializable]
[ComVisible(true)]
public sealed class ActivationArguments : EvidenceBase
{
	private ActivationContext _context;

	private ApplicationIdentity _identity;

	private string[] _data;

	/// <summary>Gets the activation context for manifest-based activation of an application.</summary>
	/// <returns>An object that identifies a manifest-based activation application.</returns>
	public ActivationContext ActivationContext => _context;

	/// <summary>Gets activation data from the host.</summary>
	/// <returns>An array of strings containing host-provided activation data.</returns>
	public string[] ActivationData => _data;

	/// <summary>Gets the application identity for a manifest-activated application.</summary>
	/// <returns>An object that identifies an application for manifest-based activation.</returns>
	public ApplicationIdentity ApplicationIdentity => _identity;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Hosting.ActivationArguments" /> class with the specified activation context. </summary>
	/// <param name="activationData">An object that identifies the manifest-based activation application.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="activationData" /> is null.</exception>
	public ActivationArguments(ActivationContext activationData)
	{
		if (activationData == null)
		{
			throw new ArgumentNullException("activationData");
		}
		_context = activationData;
		_identity = activationData.Identity;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Hosting.ActivationArguments" /> class with the specified application identity.</summary>
	/// <param name="applicationIdentity">An object that identifies the manifest-based activation application.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="applicationIdentity" /> is null.</exception>
	public ActivationArguments(ApplicationIdentity applicationIdentity)
	{
		if (applicationIdentity == null)
		{
			throw new ArgumentNullException("applicationIdentity");
		}
		_identity = applicationIdentity;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Hosting.ActivationArguments" /> class with the specified activation context and activation data.</summary>
	/// <param name="activationContext">An object that identifies the manifest-based activation application.</param>
	/// <param name="activationData">An array of strings containing host-provided activation data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="activationContext" /> is null.</exception>
	public ActivationArguments(ActivationContext activationContext, string[] activationData)
	{
		if (activationContext == null)
		{
			throw new ArgumentNullException("activationContext");
		}
		_context = activationContext;
		_identity = activationContext.Identity;
		_data = activationData;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Hosting.ActivationArguments" /> class with the specified application identity and activation data.</summary>
	/// <param name="applicationIdentity">An object that identifies the manifest-based activation application.</param>
	/// <param name="activationData">An array of strings containing host-provided activation data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="applicationIdentity" /> is null.</exception>
	public ActivationArguments(ApplicationIdentity applicationIdentity, string[] activationData)
	{
		if (applicationIdentity == null)
		{
			throw new ArgumentNullException("applicationIdentity");
		}
		_identity = applicationIdentity;
		_data = activationData;
	}
}
