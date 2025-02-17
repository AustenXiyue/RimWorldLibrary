using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Defines a friendly default alias for an assembly manifest.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
[ComVisible(true)]
public sealed class AssemblyDefaultAliasAttribute : Attribute
{
	private string m_defaultAlias;

	/// <summary>Gets default alias information.</summary>
	/// <returns>A string containing the default alias information.</returns>
	public string DefaultAlias => m_defaultAlias;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyDefaultAliasAttribute" /> class.</summary>
	/// <param name="defaultAlias">The assembly default alias information. </param>
	public AssemblyDefaultAliasAttribute(string defaultAlias)
	{
		m_defaultAlias = defaultAlias;
	}
}
