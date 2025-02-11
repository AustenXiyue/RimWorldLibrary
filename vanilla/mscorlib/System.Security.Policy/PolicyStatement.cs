using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Security.Policy;

/// <summary>Represents the statement of a <see cref="T:System.Security.Policy.CodeGroup" /> describing the permissions and other information that apply to code with a particular set of evidence. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class PolicyStatement : ISecurityEncodable, ISecurityPolicyEncodable
{
	private PermissionSet perms;

	private PolicyStatementAttribute attrs;

	/// <summary>Gets or sets the <see cref="T:System.Security.PermissionSet" /> of the policy statement.</summary>
	/// <returns>The <see cref="T:System.Security.PermissionSet" /> of the policy statement.</returns>
	public PermissionSet PermissionSet
	{
		get
		{
			if (perms == null)
			{
				perms = new PermissionSet(PermissionState.None);
				perms.SetReadOnly(value: true);
			}
			return perms;
		}
		set
		{
			perms = value;
		}
	}

	/// <summary>Gets or sets the attributes of the policy statement.</summary>
	/// <returns>The attributes of the policy statement.</returns>
	public PolicyStatementAttribute Attributes
	{
		get
		{
			return attrs;
		}
		set
		{
			if ((uint)value <= 3u)
			{
				attrs = value;
				return;
			}
			throw new ArgumentException(string.Format(Locale.GetText("Invalid value for {0}."), "PolicyStatementAttribute"));
		}
	}

	/// <summary>Gets a string representation of the attributes of the policy statement.</summary>
	/// <returns>A text string representing the attributes of the policy statement.</returns>
	public string AttributeString => attrs switch
	{
		PolicyStatementAttribute.Exclusive => "Exclusive", 
		PolicyStatementAttribute.LevelFinal => "LevelFinal", 
		PolicyStatementAttribute.All => "Exclusive LevelFinal", 
		_ => string.Empty, 
	};

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.PolicyStatement" /> class with the specified <see cref="T:System.Security.PermissionSet" />.</summary>
	/// <param name="permSet">The <see cref="T:System.Security.PermissionSet" /> with which to initialize the new instance. </param>
	public PolicyStatement(PermissionSet permSet)
		: this(permSet, PolicyStatementAttribute.Nothing)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.PolicyStatement" /> class with the specified <see cref="T:System.Security.PermissionSet" /> and attributes.</summary>
	/// <param name="permSet">The <see cref="T:System.Security.PermissionSet" /> with which to initialize the new instance. </param>
	/// <param name="attributes">A bitwise combination of the <see cref="T:System.Security.Policy.PolicyStatementAttribute" /> values. </param>
	public PolicyStatement(PermissionSet permSet, PolicyStatementAttribute attributes)
	{
		if (permSet != null)
		{
			perms = permSet.Copy();
			perms.SetReadOnly(value: true);
		}
		attrs = attributes;
	}

	/// <summary>Creates an equivalent copy of the current policy statement.</summary>
	/// <returns>A new copy of the <see cref="T:System.Security.Policy.PolicyStatement" /> with <see cref="P:System.Security.Policy.PolicyStatement.PermissionSet" /> and <see cref="P:System.Security.Policy.PolicyStatement.Attributes" /> identical to those of the current <see cref="T:System.Security.Policy.PolicyStatement" />.</returns>
	public PolicyStatement Copy()
	{
		return new PolicyStatement(perms, attrs);
	}

	/// <summary>Reconstructs a security object with a given state from an XML encoding.</summary>
	/// <param name="et">The XML encoding to use to reconstruct the security object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="et" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="et" /> parameter is not a valid <see cref="T:System.Security.Policy.PolicyStatement" /> encoding. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public void FromXml(SecurityElement et)
	{
		FromXml(et, null);
	}

	/// <summary>Reconstructs a security object with a given state from an XML encoding.</summary>
	/// <param name="et">The XML encoding to use to reconstruct the security object. </param>
	/// <param name="level">The <see cref="T:System.Security.Policy.PolicyLevel" /> context for lookup of <see cref="T:System.Security.NamedPermissionSet" /> values. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="et" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="et" /> parameter is not a valid <see cref="T:System.Security.Policy.PolicyStatement" /> encoding. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public void FromXml(SecurityElement et, PolicyLevel level)
	{
		if (et == null)
		{
			throw new ArgumentNullException("et");
		}
		if (et.Tag != "PolicyStatement")
		{
			throw new ArgumentException(Locale.GetText("Invalid tag."));
		}
		string text = et.Attribute("Attributes");
		if (text != null)
		{
			attrs = (PolicyStatementAttribute)Enum.Parse(typeof(PolicyStatementAttribute), text);
		}
		SecurityElement et2 = et.SearchForChildByTag("PermissionSet");
		PermissionSet.FromXml(et2);
	}

	/// <summary>Creates an XML encoding of the security object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SecurityElement ToXml()
	{
		return ToXml(null);
	}

	/// <summary>Creates an XML encoding of the security object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <param name="level">The <see cref="T:System.Security.Policy.PolicyLevel" /> context for lookup of <see cref="T:System.Security.NamedPermissionSet" /> values. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SecurityElement ToXml(PolicyLevel level)
	{
		SecurityElement securityElement = new SecurityElement("PolicyStatement");
		securityElement.AddAttribute("version", "1");
		if (attrs != 0)
		{
			securityElement.AddAttribute("Attributes", attrs.ToString());
		}
		securityElement.AddChild(PermissionSet.ToXml());
		return securityElement;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Security.Policy.PolicyStatement" /> object is equal to the current <see cref="T:System.Security.Policy.PolicyStatement" />.</summary>
	/// <returns>true if the specified <see cref="T:System.Security.Policy.PolicyStatement" /> is equal to the current <see cref="T:System.Security.Policy.PolicyStatement" /> object; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Security.Policy.PolicyStatement" /> object to compare with the current <see cref="T:System.Security.Policy.PolicyStatement" />. </param>
	[ComVisible(false)]
	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is PolicyStatement policyStatement))
		{
			return false;
		}
		if (PermissionSet.Equals(obj))
		{
			return attrs == policyStatement.attrs;
		}
		return false;
	}

	/// <summary>Gets a hash code for the <see cref="T:System.Security.Policy.PolicyStatement" /> object that is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Security.Policy.PolicyStatement" /> object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[ComVisible(false)]
	public override int GetHashCode()
	{
		return PermissionSet.GetHashCode() ^ (int)attrs;
	}

	internal static PolicyStatement Empty()
	{
		return new PolicyStatement(new PermissionSet(PermissionState.None));
	}
}
