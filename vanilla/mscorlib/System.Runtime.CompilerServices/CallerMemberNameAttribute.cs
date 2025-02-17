namespace System.Runtime.CompilerServices;

/// <summary>Allows you to obtain the method or property name of the caller to the method.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class CallerMemberNameAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.CallerMemberNameAttribute" /> class.</summary>
	public CallerMemberNameAttribute()
	{
	}
}
