using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Defines a company name custom attribute for an assembly manifest.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
[ComVisible(true)]
public sealed class AssemblyCompanyAttribute : Attribute
{
	private string m_company;

	/// <summary>Gets company name information.</summary>
	/// <returns>A string containing the company name.</returns>
	public string Company => m_company;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyCompanyAttribute" /> class.</summary>
	/// <param name="company">The company name information. </param>
	public AssemblyCompanyAttribute(string company)
	{
		m_company = company;
	}
}
