namespace System.Runtime.CompilerServices;

/// <summary>Deprecated. Freezes a string literal when creating native images using the Ngen.exe (Native Image Generator). This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class StringFreezingAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.StringFreezingAttribute" /> class. </summary>
	public StringFreezingAttribute()
	{
	}
}
