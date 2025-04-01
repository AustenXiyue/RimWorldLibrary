namespace System.Security;

/// <summary>Indicates the set of security rules the common language runtime should enforce for an assembly.  </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class SecurityRulesAttribute : Attribute
{
	private SecurityRuleSet m_ruleSet;

	private bool m_skipVerificationInFullTrust;

	/// <summary>Determines whether fully trusted transparent code should skip Microsoft intermediate language (MSIL) verification.</summary>
	/// <returns>true if MSIL verification should be skipped; otherwise, false. The default is false.</returns>
	public bool SkipVerificationInFullTrust
	{
		get
		{
			return m_skipVerificationInFullTrust;
		}
		set
		{
			m_skipVerificationInFullTrust = value;
		}
	}

	/// <summary>Gets the rule set to be applied.</summary>
	/// <returns>One of the enumeration values that specifies the transparency rules to be applied.</returns>
	public SecurityRuleSet RuleSet => m_ruleSet;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.SecurityRulesAttribute" /> class using the specified rule set value. </summary>
	/// <param name="ruleSet">One of the enumeration values that specifies the transparency rules set. </param>
	public SecurityRulesAttribute(SecurityRuleSet ruleSet)
	{
		m_ruleSet = ruleSet;
	}
}
