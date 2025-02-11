namespace System.Runtime.CompilerServices;

/// <summary>Specifies a source <see cref="T:System.Type" /> in another assembly. </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
public sealed class TypeForwardedFromAttribute : Attribute
{
	private string assemblyFullName;

	/// <summary>Gets the assembly-qualified name of the source type.</summary>
	/// <returns>The assembly-qualified name of the source type.</returns>
	public string AssemblyFullName => assemblyFullName;

	private TypeForwardedFromAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.TypeForwardedFromAttribute" /> class. </summary>
	/// <param name="assemblyFullName">The source <see cref="T:System.Type" /> in another assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFullName" /> is null or empty.</exception>
	public TypeForwardedFromAttribute(string assemblyFullName)
	{
		if (string.IsNullOrEmpty(assemblyFullName))
		{
			throw new ArgumentNullException("assemblyFullName");
		}
		this.assemblyFullName = assemblyFullName;
	}
}
