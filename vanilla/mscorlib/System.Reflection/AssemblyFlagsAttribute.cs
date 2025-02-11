using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Specifies a bitwise combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags for an assembly, describing just-in-time (JIT) compiler options, whether the assembly is retargetable, and whether it has a full or tokenized public key. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
[ComVisible(true)]
public sealed class AssemblyFlagsAttribute : Attribute
{
	private AssemblyNameFlags m_flags;

	/// <summary>Gets an unsigned integer value representing the combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags specified when this attribute instance was created.</summary>
	/// <returns>An unsigned integer value representing a bitwise combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags.</returns>
	[CLSCompliant(false)]
	[Obsolete("This property has been deprecated. Please use AssemblyFlags instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	public uint Flags => (uint)m_flags;

	/// <summary>Gets an integer value representing the combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags specified when this attribute instance was created.</summary>
	/// <returns>An integer value representing a bitwise combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags.</returns>
	public int AssemblyFlags => (int)m_flags;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyFlagsAttribute" /> class with the specified combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags, cast as an unsigned integer value.</summary>
	/// <param name="flags">A bitwise combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags, cast as an unsigned integer value, representing just-in-time (JIT) compiler options, longevity, whether an assembly is retargetable, and whether it has a full or tokenized public key.</param>
	[Obsolete("This constructor has been deprecated. Please use AssemblyFlagsAttribute(AssemblyNameFlags) instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	[CLSCompliant(false)]
	public AssemblyFlagsAttribute(uint flags)
	{
		m_flags = (AssemblyNameFlags)flags;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyFlagsAttribute" /> class with the specified combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags, cast as an integer value.</summary>
	/// <param name="assemblyFlags">A bitwise combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags, cast as an integer value, representing just-in-time (JIT) compiler options, longevity, whether an assembly is retargetable, and whether it has a full or tokenized public key.</param>
	[Obsolete("This constructor has been deprecated. Please use AssemblyFlagsAttribute(AssemblyNameFlags) instead. http://go.microsoft.com/fwlink/?linkid=14202")]
	public AssemblyFlagsAttribute(int assemblyFlags)
	{
		m_flags = (AssemblyNameFlags)assemblyFlags;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyFlagsAttribute" /> class with the specified combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags.</summary>
	/// <param name="assemblyFlags">A bitwise combination of <see cref="T:System.Reflection.AssemblyNameFlags" /> flags representing just-in-time (JIT) compiler options, longevity, whether an assembly is retargetable, and whether it has a full or tokenized public key.</param>
	public AssemblyFlagsAttribute(AssemblyNameFlags assemblyFlags)
	{
		m_flags = assemblyFlags;
	}
}
