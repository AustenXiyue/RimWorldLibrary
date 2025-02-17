using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Defines additional version information for an assembly manifest.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyInformationalVersionAttribute : Attribute
{
	private string m_informationalVersion;

	/// <summary>Gets version information.</summary>
	/// <returns>A string containing the version information.</returns>
	public string InformationalVersion => m_informationalVersion;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyInformationalVersionAttribute" /> class.</summary>
	/// <param name="informationalVersion">The assembly version information. </param>
	public AssemblyInformationalVersionAttribute(string informationalVersion)
	{
		m_informationalVersion = informationalVersion;
	}
}
