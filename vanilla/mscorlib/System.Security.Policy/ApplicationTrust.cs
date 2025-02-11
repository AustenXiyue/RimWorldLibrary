using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using Mono.Security.Cryptography;

namespace System.Security.Policy;

/// <summary>Encapsulates security decisions about an application. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class ApplicationTrust : EvidenceBase, ISecurityEncodable
{
	private ApplicationIdentity _appid;

	private PolicyStatement _defaultPolicy;

	private object _xtranfo;

	private bool _trustrun;

	private bool _persist;

	private IList<StrongName> fullTrustAssemblies;

	/// <summary>Gets or sets the application identity for the application trust object.</summary>
	/// <returns>An <see cref="T:System.ApplicationIdentity" /> for the application trust object.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	///   <see cref="T:System.ApplicationIdentity" /> cannot be set because it has a value of null.</exception>
	public ApplicationIdentity ApplicationIdentity
	{
		get
		{
			return _appid;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("ApplicationIdentity");
			}
			_appid = value;
		}
	}

	/// <summary>Gets or sets the policy statement defining the default grant set.</summary>
	/// <returns>A <see cref="T:System.Security.Policy.PolicyStatement" /> describing the default grants.</returns>
	public PolicyStatement DefaultGrantSet
	{
		get
		{
			if (_defaultPolicy == null)
			{
				_defaultPolicy = GetDefaultGrantSet();
			}
			return _defaultPolicy;
		}
		set
		{
			_defaultPolicy = value;
		}
	}

	/// <summary>Gets or sets extra security information about the application.</summary>
	/// <returns>An object containing additional security information about the application.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public object ExtraInfo
	{
		get
		{
			return _xtranfo;
		}
		set
		{
			_xtranfo = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the application has the required permission grants and is trusted to run.</summary>
	/// <returns>true if the application is trusted to run; otherwise, false. The default is false.</returns>
	public bool IsApplicationTrustedToRun
	{
		get
		{
			return _trustrun;
		}
		set
		{
			_trustrun = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether application trust information is persisted.</summary>
	/// <returns>true if application trust information is persisted; otherwise, false. The default is false.</returns>
	public bool Persist
	{
		get
		{
			return _persist;
		}
		set
		{
			_persist = value;
		}
	}

	/// <summary>Gets the list of full-trust assemblies for this application trust.</summary>
	/// <returns>A list of full-trust assemblies.</returns>
	public IList<StrongName> FullTrustAssemblies => fullTrustAssemblies;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.ApplicationTrust" /> class.</summary>
	public ApplicationTrust()
	{
		fullTrustAssemblies = new List<StrongName>(0);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.ApplicationTrust" /> class with an <see cref="T:System.ApplicationIdentity" />. </summary>
	/// <param name="applicationIdentity">An <see cref="T:System.ApplicationIdentity" /> that uniquely identifies an application.</param>
	public ApplicationTrust(ApplicationIdentity applicationIdentity)
		: this()
	{
		if (applicationIdentity == null)
		{
			throw new ArgumentNullException("applicationIdentity");
		}
		_appid = applicationIdentity;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.ApplicationTrust" /> class using the provided grant set and collection of full-trust assemblies.</summary>
	/// <param name="defaultGrantSet">A default permission set that is granted to all assemblies that do not have specific grants.</param>
	/// <param name="fullTrustAssemblies">An array of strong names that represent assemblies that should be considered fully trusted in an application domain.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="defaultGrantSet" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fullTrustAssemblies" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fullTrustAssemblies" /> contains an assembly that does not have a <see cref="T:System.Security.Policy.StrongName" />.</exception>
	public ApplicationTrust(PermissionSet defaultGrantSet, IEnumerable<StrongName> fullTrustAssemblies)
	{
		if (defaultGrantSet == null)
		{
			throw new ArgumentNullException("defaultGrantSet");
		}
		_defaultPolicy = new PolicyStatement(defaultGrantSet);
		if (fullTrustAssemblies == null)
		{
			throw new ArgumentNullException("fullTrustAssemblies");
		}
		this.fullTrustAssemblies = new List<StrongName>();
		foreach (StrongName fullTrustAssembly in fullTrustAssemblies)
		{
			if (fullTrustAssembly == null)
			{
				throw new ArgumentException("fullTrustAssemblies contains an assembly that does not have a StrongName");
			}
			this.fullTrustAssemblies.Add((StrongName)fullTrustAssembly.Copy());
		}
	}

	/// <summary>Reconstructs an <see cref="T:System.Security.Policy.ApplicationTrust" /> object with a given state from an XML encoding.</summary>
	/// <param name="element">The XML encoding to use to reconstruct the <see cref="T:System.Security.Policy.ApplicationTrust" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The XML encoding used for <paramref name="element" /> is invalid.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void FromXml(SecurityElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (element.Tag != "ApplicationTrust")
		{
			throw new ArgumentException("element");
		}
		string text = element.Attribute("FullName");
		if (text != null)
		{
			_appid = new ApplicationIdentity(text);
		}
		else
		{
			_appid = null;
		}
		_defaultPolicy = null;
		SecurityElement securityElement = element.SearchForChildByTag("DefaultGrant");
		if (securityElement != null)
		{
			for (int i = 0; i < securityElement.Children.Count; i++)
			{
				SecurityElement securityElement2 = securityElement.Children[i] as SecurityElement;
				if (securityElement2.Tag == "PolicyStatement")
				{
					DefaultGrantSet.FromXml(securityElement2, null);
					break;
				}
			}
		}
		if (!bool.TryParse(element.Attribute("TrustedToRun"), out _trustrun))
		{
			_trustrun = false;
		}
		if (!bool.TryParse(element.Attribute("Persist"), out _persist))
		{
			_persist = false;
		}
		_xtranfo = null;
		SecurityElement securityElement3 = element.SearchForChildByTag("ExtraInfo");
		if (securityElement3 == null)
		{
			return;
		}
		text = securityElement3.Attribute("Data");
		if (text != null)
		{
			using (MemoryStream serializationStream = new MemoryStream(CryptoConvert.FromHex(text)))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				_xtranfo = binaryFormatter.Deserialize(serializationStream);
			}
		}
	}

	/// <summary>Creates an XML encoding of the <see cref="T:System.Security.Policy.ApplicationTrust" /> object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public SecurityElement ToXml()
	{
		SecurityElement securityElement = new SecurityElement("ApplicationTrust");
		securityElement.AddAttribute("version", "1");
		if (_appid != null)
		{
			securityElement.AddAttribute("FullName", _appid.FullName);
		}
		if (_trustrun)
		{
			securityElement.AddAttribute("TrustedToRun", "true");
		}
		if (_persist)
		{
			securityElement.AddAttribute("Persist", "true");
		}
		SecurityElement securityElement2 = new SecurityElement("DefaultGrant");
		securityElement2.AddChild(DefaultGrantSet.ToXml());
		securityElement.AddChild(securityElement2);
		if (_xtranfo != null)
		{
			byte[] input = null;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				new BinaryFormatter().Serialize(memoryStream, _xtranfo);
				input = memoryStream.ToArray();
			}
			SecurityElement securityElement3 = new SecurityElement("ExtraInfo");
			securityElement3.AddAttribute("Data", CryptoConvert.ToHex(input));
			securityElement.AddChild(securityElement3);
		}
		return securityElement;
	}

	private PolicyStatement GetDefaultGrantSet()
	{
		return new PolicyStatement(new PermissionSet(PermissionState.None));
	}
}
