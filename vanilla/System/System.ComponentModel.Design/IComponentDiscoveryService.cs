using System.Collections;

namespace System.ComponentModel.Design;

/// <summary>Enables enumeration of components at design time.</summary>
public interface IComponentDiscoveryService
{
	/// <summary>Gets the list of available component types.</summary>
	/// <returns>The list of available component types.</returns>
	/// <param name="designerHost">The designer host providing design-time services. Can be null.</param>
	/// <param name="baseType">The base type specifying the components to retrieve. Can be null.</param>
	ICollection GetComponentTypes(IDesignerHost designerHost, Type baseType);
}
