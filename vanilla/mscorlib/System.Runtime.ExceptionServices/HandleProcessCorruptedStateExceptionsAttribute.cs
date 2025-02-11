namespace System.Runtime.ExceptionServices;

/// <summary>Enables managed code to handle exceptions that indicate a corrupted process state.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class HandleProcessCorruptedStateExceptionsAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute" /> class. </summary>
	public HandleProcessCorruptedStateExceptionsAttribute()
	{
	}
}
