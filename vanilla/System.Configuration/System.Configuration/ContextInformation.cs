using Unity;

namespace System.Configuration;

/// <summary>Encapsulates the context information that is associated with a <see cref="T:System.Configuration.ConfigurationElement" /> object. This class cannot be inherited.</summary>
public sealed class ContextInformation
{
	private object ctx;

	private Configuration config;

	/// <summary>Gets the context of the environment where the configuration property is being evaluated.</summary>
	/// <returns>An object specifying the environment where the configuration property is being evaluated.</returns>
	public object HostingContext => ctx;

	/// <summary>Gets a value specifying whether the configuration property is being evaluated at the machine configuration level.</summary>
	/// <returns>true if the configuration property is being evaluated at the machine configuration level; otherwise, false.</returns>
	[System.MonoInternalNote("should this use HostingContext instead?")]
	public bool IsMachineLevel => config.ConfigPath == "machine";

	internal ContextInformation(Configuration config, object ctx)
	{
		this.ctx = ctx;
		this.config = config;
	}

	/// <summary>Provides an object containing configuration-section information based on the specified section name.</summary>
	/// <returns>An object containing the specified section within the configuration.</returns>
	/// <param name="sectionName">The name of the configuration section.</param>
	public object GetSection(string sectionName)
	{
		return config.GetSection(sectionName);
	}

	internal ContextInformation()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
