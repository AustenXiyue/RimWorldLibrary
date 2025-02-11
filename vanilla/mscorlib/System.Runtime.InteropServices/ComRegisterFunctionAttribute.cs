namespace System.Runtime.InteropServices;

/// <summary>Specifies the method to call when you register an assembly for use from COM; this enables the execution of user-written code during the registration process.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[ComVisible(true)]
public sealed class ComRegisterFunctionAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ComRegisterFunctionAttribute" /> class.</summary>
	public ComRegisterFunctionAttribute()
	{
	}
}
