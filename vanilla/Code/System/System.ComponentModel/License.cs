using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides the abstract base class for all licenses. A license is granted to a specific instance of a component.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class License : IDisposable
{
	/// <summary>When overridden in a derived class, gets the license key granted to this component.</summary>
	/// <returns>A license key granted to this component.</returns>
	public abstract string LicenseKey { get; }

	/// <summary>When overridden in a derived class, disposes of the resources used by the license.</summary>
	public abstract void Dispose();

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.License" /> class. </summary>
	protected License()
	{
	}
}
