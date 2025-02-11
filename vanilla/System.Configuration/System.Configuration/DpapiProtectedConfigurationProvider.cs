using System.Collections.Specialized;
using System.Xml;

namespace System.Configuration;

/// <summary>Provides a <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> object that uses the Windows data protection API (DPAPI) to encrypt and decrypt configuration data.</summary>
public sealed class DpapiProtectedConfigurationProvider : ProtectedConfigurationProvider
{
	private bool useMachineProtection;

	private const string NotSupportedReason = "DpapiProtectedConfigurationProvider depends on the Microsoft Data\nProtection API, and is unimplemented in Mono.  For portability's sake,\nit is suggested that you use the RsaProtectedConfigurationProvider.";

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Configuration.DpapiProtectedConfigurationProvider" /> object is using machine-specific or user-account-specific protection.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.DpapiProtectedConfigurationProvider" /> is using machine-specific protection; false if it is using user-account-specific protection.</returns>
	public bool UseMachineProtection => useMachineProtection;

	/// <summary>Decrypts the passed <see cref="T:System.Xml.XmlNode" /> object.</summary>
	/// <returns>A decrypted <see cref="T:System.Xml.XmlNode" /> object.</returns>
	/// <param name="encryptedNode">The <see cref="T:System.Xml.XmlNode" /> object to decrypt. </param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">
	///   <paramref name="encryptedNode" /> does not have <see cref="P:System.Xml.XmlNode.Name" /> set to "EncryptedData" and <see cref="T:System.Xml.XmlNodeType" /> set to <see cref="F:System.Xml.XmlNodeType.Element" />.- or -<paramref name="encryptedNode" /> does not have a child node named "CipherData" with a child node named "CipherValue".- or -The child node named "CipherData" is an empty node.</exception>
	[System.MonoNotSupported("DpapiProtectedConfigurationProvider depends on the Microsoft Data\nProtection API, and is unimplemented in Mono.  For portability's sake,\nit is suggested that you use the RsaProtectedConfigurationProvider.")]
	public override XmlNode Decrypt(XmlNode encryptedNode)
	{
		throw new NotSupportedException("DpapiProtectedConfigurationProvider depends on the Microsoft Data\nProtection API, and is unimplemented in Mono.  For portability's sake,\nit is suggested that you use the RsaProtectedConfigurationProvider.");
	}

	/// <summary>Encrypts the passed <see cref="T:System.Xml.XmlNode" /> object.</summary>
	/// <returns>An encrypted <see cref="T:System.Xml.XmlNode" /> object.</returns>
	/// <param name="node">The <see cref="T:System.Xml.XmlNode" /> object to encrypt. </param>
	[System.MonoNotSupported("DpapiProtectedConfigurationProvider depends on the Microsoft Data\nProtection API, and is unimplemented in Mono.  For portability's sake,\nit is suggested that you use the RsaProtectedConfigurationProvider.")]
	public override XmlNode Encrypt(XmlNode node)
	{
		throw new NotSupportedException("DpapiProtectedConfigurationProvider depends on the Microsoft Data\nProtection API, and is unimplemented in Mono.  For portability's sake,\nit is suggested that you use the RsaProtectedConfigurationProvider.");
	}

	/// <summary>Initializes the provider with default settings.</summary>
	/// <param name="name">The provider name to use for the object.</param>
	/// <param name="configurationValues">A <see cref="T:System.Collections.Specialized.NameValueCollection" /> collection of values to use when initializing the object.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">
	///   <paramref name="configurationValues" /> contains an unrecognized configuration setting.</exception>
	[System.MonoTODO]
	public override void Initialize(string name, NameValueCollection configurationValues)
	{
		base.Initialize(name, configurationValues);
		string text = configurationValues["useMachineProtection"];
		if (text != null && text.ToLowerInvariant() == "true")
		{
			useMachineProtection = true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.DpapiProtectedConfigurationProvider" /> class using default settings.</summary>
	public DpapiProtectedConfigurationProvider()
	{
	}
}
