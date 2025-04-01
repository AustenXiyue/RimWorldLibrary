namespace System.Runtime.CompilerServices;

/// <summary>Prevents the Ildasm.exe (MSIL Disassembler) from disassembling an assembly. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module)]
public sealed class SuppressIldasmAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.SuppressIldasmAttribute" /> class. </summary>
	public SuppressIldasmAttribute()
	{
	}
}
