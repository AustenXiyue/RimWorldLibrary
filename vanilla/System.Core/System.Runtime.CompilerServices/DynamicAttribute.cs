using System.Collections.Generic;

namespace System.Runtime.CompilerServices;

/// <summary>Indicates that the use of <see cref="T:System.Object" /> on a member is meant to be treated as a dynamically dispatched type.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public sealed class DynamicAttribute : Attribute
{
	private readonly bool[] _transformFlags;

	/// <summary>Specifies, in a prefix traversal of a type's construction, which <see cref="T:System.Object" /> occurrences are meant to be treated as a dynamically dispatched type.</summary>
	/// <returns>The list of <see cref="T:System.Object" /> occurrences that are meant to be treated as a dynamically dispatched type.</returns>
	public IList<bool> TransformFlags => Array.AsReadOnly(_transformFlags);

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.DynamicAttribute" /> class.</summary>
	public DynamicAttribute()
	{
		_transformFlags = new bool[1] { true };
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.DynamicAttribute" /> class.</summary>
	/// <param name="transformFlags">Specifies, in a prefix traversal of a type's construction, which <see cref="T:System.Object" /> occurrences are meant to be treated as a dynamically dispatched type.</param>
	public DynamicAttribute(bool[] transformFlags)
	{
		if (transformFlags == null)
		{
			throw new ArgumentNullException("transformFlags");
		}
		_transformFlags = transformFlags;
	}
}
