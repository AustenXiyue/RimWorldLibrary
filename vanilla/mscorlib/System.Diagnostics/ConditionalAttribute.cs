using System.Runtime.InteropServices;

namespace System.Diagnostics;

/// <summary>Indicates to compilers that a method call or attribute should be ignored unless a specified conditional compilation symbol is defined.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
[ComVisible(true)]
public sealed class ConditionalAttribute : Attribute
{
	private string m_conditionString;

	/// <summary>Gets the conditional compilation symbol that is associated with the <see cref="T:System.Diagnostics.ConditionalAttribute" /> attribute.</summary>
	/// <returns>A string that specifies the case-sensitive conditional compilation symbol that is associated with the <see cref="T:System.Diagnostics.ConditionalAttribute" /> attribute.</returns>
	/// <filterpriority>2</filterpriority>
	public string ConditionString => m_conditionString;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.ConditionalAttribute" /> class.</summary>
	/// <param name="conditionString">A string that specifies the case-sensitive conditional compilation symbol that is associated with the attribute. </param>
	public ConditionalAttribute(string conditionString)
	{
		m_conditionString = conditionString;
	}
}
