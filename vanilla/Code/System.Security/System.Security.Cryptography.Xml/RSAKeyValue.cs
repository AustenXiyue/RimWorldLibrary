using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the &lt;RSAKeyValue&gt; element of an XML signature.</summary>
public class RSAKeyValue : KeyInfoClause
{
	private RSA _key;

	private const string KeyValueElementName = "KeyValue";

	private const string RSAKeyValueElementName = "RSAKeyValue";

	private const string ModulusElementName = "Modulus";

	private const string ExponentElementName = "Exponent";

	/// <summary>Gets or sets the instance of <see cref="T:System.Security.Cryptography.RSA" /> that holds the public key.</summary>
	/// <returns>The instance of <see cref="T:System.Security.Cryptography.RSA" /> that holds the public key.</returns>
	public RSA Key
	{
		get
		{
			return _key;
		}
		set
		{
			_key = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.RSAKeyValue" /> class with a new randomly generated <see cref="T:System.Security.Cryptography.RSA" /> public key.</summary>
	public RSAKeyValue()
	{
		_key = RSA.Create();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.RSAKeyValue" /> class with the specified <see cref="T:System.Security.Cryptography.RSA" /> public key.</summary>
	/// <param name="key">The instance of an implementation of <see cref="T:System.Security.Cryptography.RSA" /> that holds the public key. </param>
	public RSAKeyValue(RSA key)
	{
		_key = key;
	}

	/// <summary>Returns the XML representation of the <see cref="T:System.Security.Cryptography.RSA" /> key clause.</summary>
	/// <returns>The XML representation of the <see cref="T:System.Security.Cryptography.RSA" /> key clause.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override XmlElement GetXml()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		return GetXml(xmlDocument);
	}

	internal override XmlElement GetXml(XmlDocument xmlDocument)
	{
		RSAParameters rSAParameters = _key.ExportParameters(includePrivateParameters: false);
		XmlElement xmlElement = xmlDocument.CreateElement("KeyValue", "http://www.w3.org/2000/09/xmldsig#");
		XmlElement xmlElement2 = xmlDocument.CreateElement("RSAKeyValue", "http://www.w3.org/2000/09/xmldsig#");
		XmlElement xmlElement3 = xmlDocument.CreateElement("Modulus", "http://www.w3.org/2000/09/xmldsig#");
		xmlElement3.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(rSAParameters.Modulus)));
		xmlElement2.AppendChild(xmlElement3);
		XmlElement xmlElement4 = xmlDocument.CreateElement("Exponent", "http://www.w3.org/2000/09/xmldsig#");
		xmlElement4.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(rSAParameters.Exponent)));
		xmlElement2.AppendChild(xmlElement4);
		xmlElement.AppendChild(xmlElement2);
		return xmlElement;
	}

	/// <summary>Loads an <see cref="T:System.Security.Cryptography.RSA" /> key clause from an XML element.</summary>
	/// <param name="value">The XML element from which to load the <see cref="T:System.Security.Cryptography.RSA" /> key clause. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="value" /> parameter is not a valid <see cref="T:System.Security.Cryptography.RSA" /> key clause XML element. </exception>
	public override void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.LocalName != "KeyValue" || value.NamespaceURI != "http://www.w3.org/2000/09/xmldsig#")
		{
			throw new CryptographicException(string.Format("Root element must be {0} element in namespace {1}", "KeyValue", "http://www.w3.org/2000/09/xmldsig#"));
		}
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(value.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
		XmlNode xmlNode = value.SelectSingleNode(string.Format("{0}:{1}", "dsig", "RSAKeyValue"), xmlNamespaceManager);
		if (xmlNode == null)
		{
			throw new CryptographicException(string.Format("{0} must contain child element {1}", "KeyValue", "RSAKeyValue"));
		}
		try
		{
			Key.ImportParameters(new RSAParameters
			{
				Modulus = Convert.FromBase64String(xmlNode.SelectSingleNode(string.Format("{0}:{1}", "dsig", "Modulus"), xmlNamespaceManager).InnerText),
				Exponent = Convert.FromBase64String(xmlNode.SelectSingleNode(string.Format("{0}:{1}", "dsig", "Exponent"), xmlNamespaceManager).InnerText)
			});
		}
		catch (Exception inner)
		{
			throw new CryptographicException(string.Format("An error occurred parsing the {0} and {1} elements", "Modulus", "Exponent"), inner);
		}
	}
}
