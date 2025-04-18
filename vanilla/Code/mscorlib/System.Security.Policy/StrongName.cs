using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Security.Policy;

/// <summary>Provides the strong name of a code assembly as evidence for policy evaluation. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class StrongName : EvidenceBase, IIdentityPermissionFactory, IBuiltInEvidence
{
	private StrongNamePublicKeyBlob publickey;

	private string name;

	private Version version;

	/// <summary>Gets the simple name of the current <see cref="T:System.Security.Policy.StrongName" />.</summary>
	/// <returns>The simple name part of the <see cref="T:System.Security.Policy.StrongName" />.</returns>
	public string Name => name;

	/// <summary>Gets the <see cref="T:System.Security.Permissions.StrongNamePublicKeyBlob" /> of the current <see cref="T:System.Security.Policy.StrongName" />.</summary>
	/// <returns>The <see cref="T:System.Security.Permissions.StrongNamePublicKeyBlob" /> of the current <see cref="T:System.Security.Policy.StrongName" />.</returns>
	public StrongNamePublicKeyBlob PublicKey => publickey;

	/// <summary>Gets the <see cref="T:System.Version" /> of the current <see cref="T:System.Security.Policy.StrongName" />.</summary>
	/// <returns>The <see cref="T:System.Version" /> of the current <see cref="T:System.Security.Policy.StrongName" />.</returns>
	public Version Version => version;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.StrongName" /> class with the strong name public key blob, name, and version.</summary>
	/// <param name="blob">The <see cref="T:System.Security.Permissions.StrongNamePublicKeyBlob" /> of the software publisher. </param>
	/// <param name="name">The simple name section of the strong name. </param>
	/// <param name="version">The <see cref="T:System.Version" /> of the strong name. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="blob" /> parameter is null.-or- The <paramref name="name" /> parameter is null.-or- The <paramref name="version" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="name" /> parameter is an empty string (""). </exception>
	public StrongName(StrongNamePublicKeyBlob blob, string name, Version version)
	{
		if (blob == null)
		{
			throw new ArgumentNullException("blob");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException(Locale.GetText("Empty"), "name");
		}
		publickey = blob;
		this.name = name;
		this.version = version;
	}

	/// <summary>Creates an equivalent copy of the current <see cref="T:System.Security.Policy.StrongName" />.</summary>
	/// <returns>A new, identical copy of the current <see cref="T:System.Security.Policy.StrongName" />.</returns>
	public object Copy()
	{
		return new StrongName(publickey, name, version);
	}

	/// <summary>Creates a <see cref="T:System.Security.Permissions.StrongNameIdentityPermission" /> that corresponds to the current <see cref="T:System.Security.Policy.StrongName" />.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.StrongNameIdentityPermission" /> for the specified <see cref="T:System.Security.Policy.StrongName" />.</returns>
	/// <param name="evidence">The <see cref="T:System.Security.Policy.Evidence" /> from which to construct the <see cref="T:System.Security.Permissions.StrongNameIdentityPermission" />. </param>
	public IPermission CreateIdentityPermission(Evidence evidence)
	{
		return new StrongNameIdentityPermission(publickey, name, version);
	}

	/// <summary>Determines whether the specified strong name is equal to the current strong name.</summary>
	/// <returns>true if the specified strong name is equal to the current strong name; otherwise, false.</returns>
	/// <param name="o">The strong name to compare against the current strong name. </param>
	public override bool Equals(object o)
	{
		if (!(o is StrongName strongName))
		{
			return false;
		}
		if (name != strongName.Name)
		{
			return false;
		}
		if (!Version.Equals(strongName.Version))
		{
			return false;
		}
		return PublicKey.Equals(strongName.PublicKey);
	}

	/// <summary>Gets the hash code of the current <see cref="T:System.Security.Policy.StrongName" />.</summary>
	/// <returns>The hash code of the current <see cref="T:System.Security.Policy.StrongName" />.</returns>
	public override int GetHashCode()
	{
		return publickey.GetHashCode();
	}

	/// <summary>Creates a string representation of the current <see cref="T:System.Security.Policy.StrongName" />.</summary>
	/// <returns>A representation of the current <see cref="T:System.Security.Policy.StrongName" />.</returns>
	public override string ToString()
	{
		SecurityElement securityElement = new SecurityElement(typeof(StrongName).Name);
		securityElement.AddAttribute("version", "1");
		securityElement.AddAttribute("Key", publickey.ToString());
		securityElement.AddAttribute("Name", name);
		securityElement.AddAttribute("Version", version.ToString());
		return securityElement.ToString();
	}

	int IBuiltInEvidence.GetRequiredSize(bool verbose)
	{
		return ((!verbose) ? 1 : 5) + name.Length;
	}

	[MonoTODO("IBuiltInEvidence")]
	int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
	{
		return 0;
	}

	[MonoTODO("IBuiltInEvidence")]
	int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
	{
		return 0;
	}
}
