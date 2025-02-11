using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Specifies a description for an assembly.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
[ComVisible(true)]
public sealed class AssemblyTitleAttribute : Attribute
{
	private string m_title;

	/// <summary>Gets assembly title information.</summary>
	/// <returns>The assembly title. </returns>
	public string Title => m_title;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyTitleAttribute" /> class.</summary>
	/// <param name="title">The assembly title. </param>
	public AssemblyTitleAttribute(string title)
	{
		m_title = title;
	}
}
