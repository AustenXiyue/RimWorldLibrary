using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides a base class for the container filter service.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class ContainerFilterService
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ContainerFilterService" /> class.</summary>
	protected ContainerFilterService()
	{
	}

	/// <summary>Filters the component collection.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.ComponentCollection" /> that represents a modified collection.</returns>
	/// <param name="components">The component collection to filter.</param>
	public virtual ComponentCollection FilterComponents(ComponentCollection components)
	{
		return components;
	}
}
