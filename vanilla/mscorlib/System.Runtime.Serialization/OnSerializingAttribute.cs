using System.Runtime.InteropServices;

namespace System.Runtime.Serialization;

/// <summary>When applied to a method, specifies that the method is during serialization of an object in an object graph. The order of serialization relative to other objects in the graph is non-deterministic.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[ComVisible(true)]
public sealed class OnSerializingAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.OnSerializingAttribute" /> class. </summary>
	public OnSerializingAttribute()
	{
	}
}
