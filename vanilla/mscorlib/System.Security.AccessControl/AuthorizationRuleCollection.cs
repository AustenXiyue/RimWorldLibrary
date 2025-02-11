using System.Collections;

namespace System.Security.AccessControl;

/// <summary>Represents a collection of <see cref="T:System.Security.AccessControl.AuthorizationRule" /> objects.</summary>
public sealed class AuthorizationRuleCollection : ReadOnlyCollectionBase
{
	/// <summary>Gets the <see cref="T:System.Security.AccessControl.AuthorizationRule" /> object at the specified index of the collection.</summary>
	/// <returns>The <see cref="T:System.Security.AccessControl.AuthorizationRule" /> object at the specified index.</returns>
	/// <param name="index">The zero-based index of the <see cref="T:System.Security.AccessControl.AuthorizationRule" /> object to get.</param>
	public AuthorizationRule this[int index] => (AuthorizationRule)base.InnerList[index];

	public AuthorizationRuleCollection()
	{
	}

	internal AuthorizationRuleCollection(AuthorizationRule[] rules)
	{
		base.InnerList.AddRange(rules);
	}

	public void AddRule(AuthorizationRule rule)
	{
		base.InnerList.Add(rule);
	}

	/// <summary>Copies the contents of the collection to an array.</summary>
	/// <param name="rules">An array to which to copy the contents of the collection.</param>
	/// <param name="index">The zero-based index from which to begin copying.</param>
	public void CopyTo(AuthorizationRule[] rules, int index)
	{
		base.InnerList.CopyTo(rules, index);
	}
}
