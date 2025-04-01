using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Defines a copyright custom attribute for an assembly manifest.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
[ComVisible(true)]
public sealed class AssemblyCopyrightAttribute : Attribute
{
	private string m_copyright;

	/// <summary>Gets copyright information.</summary>
	/// <returns>A string containing the copyright information.</returns>
	public string Copyright => m_copyright;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyCopyrightAttribute" /> class.</summary>
	/// <param name="copyright">The copyright information. </param>
	public AssemblyCopyrightAttribute(string copyright)
	{
		m_copyright = copyright;
	}
}
