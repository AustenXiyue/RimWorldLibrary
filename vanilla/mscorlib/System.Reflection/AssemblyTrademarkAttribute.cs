using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Defines a trademark custom attribute for an assembly manifest.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
[ComVisible(true)]
public sealed class AssemblyTrademarkAttribute : Attribute
{
	private string m_trademark;

	/// <summary>Gets trademark information.</summary>
	/// <returns>A String containing trademark information.</returns>
	public string Trademark => m_trademark;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyTrademarkAttribute" /> class.</summary>
	/// <param name="trademark">The trademark information. </param>
	public AssemblyTrademarkAttribute(string trademark)
	{
		m_trademark = trademark;
	}
}
