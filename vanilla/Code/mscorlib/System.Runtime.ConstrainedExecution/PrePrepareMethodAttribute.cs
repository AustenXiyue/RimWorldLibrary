namespace System.Runtime.ConstrainedExecution;

/// <summary>Instructs the native image generation service to prepare a method for inclusion in a constrained execution region (CER).</summary>
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
public sealed class PrePrepareMethodAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.ConstrainedExecution.PrePrepareMethodAttribute" /> class. </summary>
	public PrePrepareMethodAttribute()
	{
	}
}
