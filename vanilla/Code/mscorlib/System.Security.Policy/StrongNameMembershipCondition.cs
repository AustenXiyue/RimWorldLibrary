using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace System.Security.Policy;

/// <summary>Determines whether an assembly belongs to a code group by testing its strong name. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class StrongNameMembershipCondition : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable, IConstantMembershipCondition
{
	private readonly int version = 1;

	private StrongNamePublicKeyBlob blob;

	private string name;

	private Version assemblyVersion;

	/// <summary>Gets or sets the simple name of the <see cref="T:System.Security.Policy.StrongName" /> for which the membership condition tests.</summary>
	/// <returns>The simple name of the <see cref="T:System.Security.Policy.StrongName" /> for which the membership condition tests.</returns>
	/// <exception cref="T:System.ArgumentException">The value is null.-or-The value is an empty string ("").</exception>
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Version" /> of the <see cref="T:System.Security.Policy.StrongName" /> for which the membership condition tests.</summary>
	/// <returns>The <see cref="T:System.Version" /> of the <see cref="T:System.Security.Policy.StrongName" /> for which the membership condition tests.</returns>
	public Version Version
	{
		get
		{
			return assemblyVersion;
		}
		set
		{
			assemblyVersion = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Security.Permissions.StrongNamePublicKeyBlob" /> of the <see cref="T:System.Security.Policy.StrongName" /> for which the membership condition tests.</summary>
	/// <returns>The <see cref="T:System.Security.Permissions.StrongNamePublicKeyBlob" /> of the <see cref="T:System.Security.Policy.StrongName" /> for which the membership condition tests.</returns>
	/// <exception cref="T:System.ArgumentNullException">An attempt is made to set the <see cref="P:System.Security.Policy.StrongNameMembershipCondition.PublicKey" /> to null. </exception>
	public StrongNamePublicKeyBlob PublicKey
	{
		get
		{
			return blob;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PublicKey");
			}
			blob = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.StrongNameMembershipCondition" /> class with the strong name public key blob, name, and version number that determine membership.</summary>
	/// <param name="blob">The strong name public key blob of the software publisher. </param>
	/// <param name="name">The simple name section of the strong name. </param>
	/// <param name="version">The version number of the strong name. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="blob" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="name" /> parameter is null.-or-The <paramref name="name" /> parameter is an empty string ("").</exception>
	public StrongNameMembershipCondition(StrongNamePublicKeyBlob blob, string name, Version version)
	{
		if (blob == null)
		{
			throw new ArgumentNullException("blob");
		}
		this.blob = blob;
		this.name = name;
		if (version != null)
		{
			assemblyVersion = (Version)version.Clone();
		}
	}

	internal StrongNameMembershipCondition(SecurityElement e)
	{
		FromXml(e);
	}

	internal StrongNameMembershipCondition()
	{
	}

	/// <summary>Determines whether the specified evidence satisfies the membership condition.</summary>
	/// <returns>true if the specified evidence satisfies the membership condition; otherwise, false.</returns>
	/// <param name="evidence">The <see cref="T:System.Security.Policy.Evidence" /> against which to make the test. </param>
	public bool Check(Evidence evidence)
	{
		if (evidence == null)
		{
			return false;
		}
		IEnumerator hostEnumerator = evidence.GetHostEnumerator();
		while (hostEnumerator.MoveNext())
		{
			if (hostEnumerator.Current is StrongName strongName)
			{
				if (!strongName.PublicKey.Equals(blob))
				{
					return false;
				}
				if (name != null && name != strongName.Name)
				{
					return false;
				}
				if (assemblyVersion != null && !assemblyVersion.Equals(strongName.Version))
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	/// <summary>Creates an equivalent copy of the current <see cref="T:System.Security.Policy.StrongNameMembershipCondition" />.</summary>
	/// <returns>A new, identical copy of the current <see cref="T:System.Security.Policy.StrongNameMembershipCondition" /></returns>
	public IMembershipCondition Copy()
	{
		return new StrongNameMembershipCondition(blob, name, assemblyVersion);
	}

	/// <summary>Determines whether the <see cref="T:System.Security.Policy.StrongName" /> from the specified object is equivalent to the <see cref="T:System.Security.Policy.StrongName" /> contained in the current <see cref="T:System.Security.Policy.StrongNameMembershipCondition" />.</summary>
	/// <returns>true if the <see cref="T:System.Security.Policy.StrongName" /> from the specified object is equivalent to the <see cref="T:System.Security.Policy.StrongName" /> contained in the current <see cref="T:System.Security.Policy.StrongNameMembershipCondition" />; otherwise, false.</returns>
	/// <param name="o">The object to compare to the current <see cref="T:System.Security.Policy.StrongNameMembershipCondition" />. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.Policy.StrongNameMembershipCondition.PublicKey" /> property of the current object or the specified object is null. </exception>
	public override bool Equals(object o)
	{
		if (!(o is StrongNameMembershipCondition strongNameMembershipCondition))
		{
			return false;
		}
		if (!strongNameMembershipCondition.PublicKey.Equals(PublicKey))
		{
			return false;
		}
		if (name != strongNameMembershipCondition.Name)
		{
			return false;
		}
		if (assemblyVersion != null)
		{
			return assemblyVersion.Equals(strongNameMembershipCondition.Version);
		}
		return strongNameMembershipCondition.Version == null;
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Security.Policy.StrongNameMembershipCondition" />.</summary>
	/// <returns>The hash code for the current <see cref="T:System.Security.Policy.StrongNameMembershipCondition" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.Policy.StrongNameMembershipCondition.PublicKey" /> property is null. </exception>
	public override int GetHashCode()
	{
		return blob.GetHashCode();
	}

	/// <summary>Reconstructs a security object with a specified state from an XML encoding.</summary>
	/// <param name="e">The XML encoding to use to reconstruct the security object. </param>
	public void FromXml(SecurityElement e)
	{
		FromXml(e, null);
	}

	/// <summary>Reconstructs a security object with a specified state from an XML encoding.</summary>
	/// <param name="e">The XML encoding to use to reconstruct the security object. </param>
	/// <param name="level">The <see cref="T:System.Security.Policy.PolicyLevel" /> context, used to resolve <see cref="T:System.Security.NamedPermissionSet" /> references. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="e" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="e" /> parameter is not a valid membership condition element. </exception>
	public void FromXml(SecurityElement e, PolicyLevel level)
	{
		MembershipConditionHelper.CheckSecurityElement(e, "e", version, version);
		blob = StrongNamePublicKeyBlob.FromString(e.Attribute("PublicKeyBlob"));
		name = e.Attribute("Name");
		string text = e.Attribute("AssemblyVersion");
		if (text == null)
		{
			assemblyVersion = null;
		}
		else
		{
			assemblyVersion = new Version(text);
		}
	}

	/// <summary>Creates and returns a string representation of the current <see cref="T:System.Security.Policy.StrongNameMembershipCondition" />.</summary>
	/// <returns>A representation of the current <see cref="T:System.Security.Policy.StrongNameMembershipCondition" />.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("StrongName - ");
		stringBuilder.Append(blob);
		if (name != null)
		{
			stringBuilder.AppendFormat(" name = {0}", name);
		}
		if (assemblyVersion != null)
		{
			stringBuilder.AppendFormat(" version = {0}", assemblyVersion);
		}
		return stringBuilder.ToString();
	}

	/// <summary>Creates an XML encoding of the security object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	public SecurityElement ToXml()
	{
		return ToXml(null);
	}

	/// <summary>Creates an XML encoding of the security object and its current state with the specified <see cref="T:System.Security.Policy.PolicyLevel" />.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <param name="level">The <see cref="T:System.Security.Policy.PolicyLevel" /> context, which is used to resolve <see cref="T:System.Security.NamedPermissionSet" /> references. </param>
	public SecurityElement ToXml(PolicyLevel level)
	{
		SecurityElement securityElement = MembershipConditionHelper.Element(typeof(StrongNameMembershipCondition), version);
		if (blob != null)
		{
			securityElement.AddAttribute("PublicKeyBlob", blob.ToString());
		}
		if (name != null)
		{
			securityElement.AddAttribute("Name", name);
		}
		if (assemblyVersion != null)
		{
			string text = assemblyVersion.ToString();
			if (text != "0.0")
			{
				securityElement.AddAttribute("AssemblyVersion", text);
			}
		}
		return securityElement;
	}
}
