namespace System.Runtime.InteropServices;

/// <summary>Specifies the method to call when you unregister an assembly for use from COM; this allows for the execution of user-written code during the unregistration process.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[ComVisible(true)]
public sealed class ComUnregisterFunctionAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ComUnregisterFunctionAttribute" /> class.</summary>
	public ComUnregisterFunctionAttribute()
	{
	}
}
