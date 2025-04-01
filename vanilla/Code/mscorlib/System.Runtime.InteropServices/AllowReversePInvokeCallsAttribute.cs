namespace System.Runtime.InteropServices;

/// <summary>Allows an unmanaged method to call a managed method.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class AllowReversePInvokeCallsAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.AllowReversePInvokeCallsAttribute" /> class.</summary>
	public AllowReversePInvokeCallsAttribute()
	{
	}
}
