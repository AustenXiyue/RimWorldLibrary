namespace System.Runtime.Versioning;

/// <summary>Defines the compatibility guarantee of a component, type, or type member that may span multiple versions.</summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
public sealed class ComponentGuaranteesAttribute : Attribute
{
	private ComponentGuaranteesOptions _guarantees;

	/// <summary>Gets a value that indicates the guaranteed level of compatibility of a library, type, or type member that spans multiple versions.</summary>
	/// <returns>One of the enumeration values that specifies the level of compatibility that is guaranteed across multiple versions.</returns>
	public ComponentGuaranteesOptions Guarantees => _guarantees;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Versioning.ComponentGuaranteesAttribute" /> class with a value that indicates a library, type, or member's guaranteed level of compatibility across multiple versions.</summary>
	/// <param name="guarantees">One of the enumeration values that specifies the level of compatibility that is guaranteed across multiple versions.</param>
	public ComponentGuaranteesAttribute(ComponentGuaranteesOptions guarantees)
	{
		_guarantees = guarantees;
	}
}
