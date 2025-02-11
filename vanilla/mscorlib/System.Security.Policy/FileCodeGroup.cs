using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Security.Policy;

/// <summary>Grants permission to manipulate files located in the code assemblies to code assemblies that match the membership condition. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class FileCodeGroup : CodeGroup
{
	private FileIOPermissionAccess m_access;

	/// <summary>Gets the merge logic.</summary>
	/// <returns>The string "Union".</returns>
	public override string MergeLogic => "Union";

	/// <summary>Gets a string representation of the attributes of the policy statement for the code group.</summary>
	/// <returns>Always null.</returns>
	public override string AttributeString => null;

	/// <summary>Gets the name of the named permission set for the code group.</summary>
	/// <returns>The concatenatation of the string "Same directory FileIO - " and the access type.</returns>
	public override string PermissionSetName => "Same directory FileIO - " + m_access;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.FileCodeGroup" /> class.</summary>
	/// <param name="membershipCondition">A membership condition that tests evidence to determine whether this code group applies policy. </param>
	/// <param name="access">One of the <see cref="T:System.Security.Permissions.FileIOPermissionAccess" /> values. This value is used to construct the <see cref="T:System.Security.Permissions.FileIOPermission" /> that is granted. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="membershipCondition" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The type of the <paramref name="membershipCondition" /> parameter is not valid.-or- The type of the <paramref name="access" /> parameter is not valid. </exception>
	public FileCodeGroup(IMembershipCondition membershipCondition, FileIOPermissionAccess access)
		: base(membershipCondition, null)
	{
		m_access = access;
	}

	internal FileCodeGroup(SecurityElement e, PolicyLevel level)
		: base(e, level)
	{
	}

	/// <summary>Makes a deep copy of the current code group.</summary>
	/// <returns>An equivalent copy of the current code group, including its membership conditions and child code groups.</returns>
	public override CodeGroup Copy()
	{
		FileCodeGroup fileCodeGroup = new FileCodeGroup(base.MembershipCondition, m_access);
		fileCodeGroup.Name = base.Name;
		fileCodeGroup.Description = base.Description;
		foreach (CodeGroup child in base.Children)
		{
			fileCodeGroup.AddChild(child.Copy());
		}
		return fileCodeGroup;
	}

	/// <summary>Resolves policy for the code group and its descendants for a set of evidence.</summary>
	/// <returns>A policy statement consisting of the permissions granted by the code group with optional attributes, or null if the code group does not apply (the membership condition does not match the specified evidence).</returns>
	/// <param name="evidence">The evidence for the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="evidence" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Policy.PolicyException">The current policy is null.-or- More than one code group (including the parent code group and all child code groups) is marked <see cref="F:System.Security.Policy.PolicyStatementAttribute.Exclusive" />. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override PolicyStatement Resolve(Evidence evidence)
	{
		if (evidence == null)
		{
			throw new ArgumentNullException("evidence");
		}
		if (!base.MembershipCondition.Check(evidence))
		{
			return null;
		}
		PermissionSet permissionSet = null;
		permissionSet = ((base.PolicyStatement != null) ? base.PolicyStatement.PermissionSet.Copy() : new PermissionSet(PermissionState.None));
		if (base.Children.Count > 0)
		{
			foreach (CodeGroup child in base.Children)
			{
				PolicyStatement policyStatement = child.Resolve(evidence);
				if (policyStatement != null)
				{
					permissionSet = permissionSet.Union(policyStatement.PermissionSet);
				}
			}
		}
		PolicyStatement policyStatement2 = null;
		policyStatement2 = ((base.PolicyStatement == null) ? PolicyStatement.Empty() : base.PolicyStatement.Copy());
		policyStatement2.PermissionSet = permissionSet;
		return policyStatement2;
	}

	/// <summary>Resolves matching code groups.</summary>
	/// <returns>A <see cref="T:System.Security.Policy.CodeGroup" /> that is the root of the tree of matching code groups.</returns>
	/// <param name="evidence">The evidence for the assembly. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="evidence" /> parameter is null. </exception>
	public override CodeGroup ResolveMatchingCodeGroups(Evidence evidence)
	{
		if (evidence == null)
		{
			throw new ArgumentNullException("evidence");
		}
		if (!base.MembershipCondition.Check(evidence))
		{
			return null;
		}
		FileCodeGroup fileCodeGroup = new FileCodeGroup(base.MembershipCondition, m_access);
		foreach (CodeGroup child in base.Children)
		{
			CodeGroup codeGroup = child.ResolveMatchingCodeGroups(evidence);
			if (codeGroup != null)
			{
				fileCodeGroup.AddChild(codeGroup);
			}
		}
		return fileCodeGroup;
	}

	/// <summary>Determines whether the specified code group is equivalent to the current code group.</summary>
	/// <returns>true if the specified code group is equivalent to the current code group; otherwise, false.</returns>
	/// <param name="o">The code group to compare with the current code group. </param>
	public override bool Equals(object o)
	{
		if (!(o is FileCodeGroup))
		{
			return false;
		}
		if (m_access != ((FileCodeGroup)o).m_access)
		{
			return false;
		}
		return Equals((CodeGroup)o, compareChildren: false);
	}

	/// <summary>Gets the hash code of the current code group.</summary>
	/// <returns>The hash code of the current code group.</returns>
	public override int GetHashCode()
	{
		return m_access.GetHashCode();
	}

	protected override void ParseXml(SecurityElement e, PolicyLevel level)
	{
		string text = e.Attribute("Access");
		if (text != null)
		{
			m_access = (FileIOPermissionAccess)Enum.Parse(typeof(FileIOPermissionAccess), text, ignoreCase: true);
		}
		else
		{
			m_access = FileIOPermissionAccess.NoAccess;
		}
	}

	protected override void CreateXml(SecurityElement element, PolicyLevel level)
	{
		element.AddAttribute("Access", m_access.ToString());
	}
}
