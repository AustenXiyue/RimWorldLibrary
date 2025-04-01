using System.Runtime.InteropServices;

namespace System.Runtime.Serialization;

/// <summary>When applied to a method, specifies that the method is called immediately after deserialization of an object in an object graph. The order of deserialization relative to other objects in the graph is non-deterministic.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[ComVisible(true)]
public sealed class OnDeserializedAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.OnDeserializedAttribute" /> class. </summary>
	public OnDeserializedAttribute()
	{
	}
}
