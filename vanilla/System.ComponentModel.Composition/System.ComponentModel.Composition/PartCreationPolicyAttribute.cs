namespace System.ComponentModel.Composition;

/// <summary>Specifies the <see cref="P:System.ComponentModel.Composition.PartCreationPolicyAttribute.CreationPolicy" /> for a part.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PartCreationPolicyAttribute : Attribute
{
	internal static PartCreationPolicyAttribute Default = new PartCreationPolicyAttribute(CreationPolicy.Any);

	internal static PartCreationPolicyAttribute Shared = new PartCreationPolicyAttribute(CreationPolicy.Shared);

	/// <summary>Gets or sets a value that indicates the creation policy of the attributed part.</summary>
	/// <returns>One of the <see cref="P:System.ComponentModel.Composition.PartCreationPolicyAttribute.CreationPolicy" /> values that indicates the creation policy of the attributed part. The default is <see cref="F:System.ComponentModel.Composition.CreationPolicy.Any" />.</returns>
	public CreationPolicy CreationPolicy { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.PartCreationPolicyAttribute" /> class with the specified creation policy.</summary>
	/// <param name="creationPolicy">The creation policy to use.</param>
	public PartCreationPolicyAttribute(CreationPolicy creationPolicy)
	{
		CreationPolicy = creationPolicy;
	}
}
