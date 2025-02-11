namespace System.Diagnostics.CodeAnalysis;

/// <summary>Specifies that the attributed code should be excluded from code coverage information. </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, Inherited = false, AllowMultiple = false)]
public sealed class ExcludeFromCodeCoverageAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute" /> class. </summary>
	public ExcludeFromCodeCoverageAttribute()
	{
	}
}
