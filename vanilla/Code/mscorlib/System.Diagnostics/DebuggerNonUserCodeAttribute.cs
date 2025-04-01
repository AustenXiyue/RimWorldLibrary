using System.Runtime.InteropServices;

namespace System.Diagnostics;

/// <summary>Identifies a type or member that is not part of the user code for an application.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
public sealed class DebuggerNonUserCodeAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.DebuggerNonUserCodeAttribute" /> class. </summary>
	public DebuggerNonUserCodeAttribute()
	{
	}
}
