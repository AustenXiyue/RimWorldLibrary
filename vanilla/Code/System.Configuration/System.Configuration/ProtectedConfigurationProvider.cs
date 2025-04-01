using System.Configuration.Provider;
using System.Xml;

namespace System.Configuration;

/// <summary>Is the base class to create providers for encrypting and decrypting protected-configuration data.</summary>
public abstract class ProtectedConfigurationProvider : ProviderBase
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> class using default settings.</summary>
	protected ProtectedConfigurationProvider()
	{
	}

	/// <summary>Decrypts the passed <see cref="T:System.Xml.XmlNode" /> object from a configuration file.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> object containing decrypted data.</returns>
	/// <param name="encryptedNode">The <see cref="T:System.Xml.XmlNode" /> object to decrypt.</param>
	public abstract XmlNode Decrypt(XmlNode encryptedNode);

	/// <summary>Encrypts the passed <see cref="T:System.Xml.XmlNode" /> object from a configuration file.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> object containing encrypted data.</returns>
	/// <param name="node">The <see cref="T:System.Xml.XmlNode" /> object to encrypt.</param>
	public abstract XmlNode Encrypt(XmlNode node);
}
