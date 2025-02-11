using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

/// <summary>Indicates that a class should be treated as if it has global scope.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Class)]
[ComVisible(true)]
public class CompilerGlobalScopeAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.CompilerGlobalScopeAttribute" /> class.</summary>
	public CompilerGlobalScopeAttribute()
	{
	}
}
