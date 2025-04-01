namespace System.Runtime.CompilerServices;

/// <summary>Allows you to obtain the line number in the source file at which the method is called.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class CallerLineNumberAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.CallerLineNumberAttribute" /> class.</summary>
	public CallerLineNumberAttribute()
	{
	}
}
