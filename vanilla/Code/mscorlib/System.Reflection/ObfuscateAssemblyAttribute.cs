using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Instructs obfuscation tools to use their standard obfuscation rules for the appropriate assembly type.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
[ComVisible(true)]
public sealed class ObfuscateAssemblyAttribute : Attribute
{
	private bool m_assemblyIsPrivate;

	private bool m_strip = true;

	/// <summary>Gets a <see cref="T:System.Boolean" /> value indicating whether the assembly was marked private.</summary>
	/// <returns>true if the assembly was marked private; otherwise, false. </returns>
	public bool AssemblyIsPrivate => m_assemblyIsPrivate;

	/// <summary>Gets or sets a <see cref="T:System.Boolean" /> value indicating whether the obfuscation tool should remove the attribute after processing.</summary>
	/// <returns>true if the obfuscation tool should remove the attribute after processing; otherwise, false. The default value for this property is true.</returns>
	public bool StripAfterObfuscation
	{
		get
		{
			return m_strip;
		}
		set
		{
			m_strip = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.ObfuscateAssemblyAttribute" /> class, specifying whether the assembly to be obfuscated is public or private.</summary>
	/// <param name="assemblyIsPrivate">true if the assembly is used within the scope of one application; otherwise, false.</param>
	public ObfuscateAssemblyAttribute(bool assemblyIsPrivate)
	{
		m_assemblyIsPrivate = assemblyIsPrivate;
	}
}
