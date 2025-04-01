using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Provides a text description for an assembly.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
[ComVisible(true)]
public sealed class AssemblyDescriptionAttribute : Attribute
{
	private string m_description;

	/// <summary>Gets assembly description information.</summary>
	/// <returns>A string containing the assembly description.</returns>
	public string Description => m_description;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyDescriptionAttribute" /> class.</summary>
	/// <param name="description">The assembly description. </param>
	public AssemblyDescriptionAttribute(string description)
	{
		m_description = description;
	}
}
