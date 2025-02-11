using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

/// <summary>Applies metadata to an assembly that indicates that a type is an unmanaged type.  This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Struct, Inherited = true)]
[ComVisible(true)]
public sealed class NativeCppClassAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.NativeCppClassAttribute" /> class. </summary>
	public NativeCppClassAttribute()
	{
	}
}
