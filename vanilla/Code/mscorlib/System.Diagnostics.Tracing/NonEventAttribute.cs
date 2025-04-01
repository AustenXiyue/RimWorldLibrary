namespace System.Diagnostics.Tracing;

/// <summary>Identifies a method that is not generating an event.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class NonEventAttribute : Attribute
{
	/// <summary>Creates a new instance of the <see cref="T:System.Diagnostics.Tracing.NonEventAttribute" /> class.</summary>
	public NonEventAttribute()
	{
	}
}
