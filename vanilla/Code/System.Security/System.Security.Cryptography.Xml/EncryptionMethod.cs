using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Encapsulates the encryption algorithm used for XML encryption. </summary>
public class EncryptionMethod
{
	private XmlElement _cachedXml;

	private int _keySize;

	private string _algorithm;

	private bool CacheValid => _cachedXml != null;

	/// <summary>Gets or sets the algorithm key size used for XML encryption. </summary>
	/// <returns>The algorithm key size, in bits, used for XML encryption.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <see cref="P:System.Security.Cryptography.Xml.EncryptionMethod.KeySize" /> property was set to a value that was less than 0.</exception>
	public int KeySize
	{
		get
		{
			return _keySize;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("value", "The key size should be a non negative integer.");
			}
			_keySize = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets or sets a Uniform Resource Identifier (URI) that describes the algorithm to use for XML encryption. </summary>
	/// <returns>A Uniform Resource Identifier (URI) that describes the algorithm to use for XML encryption.</returns>
	public string KeyAlgorithm
	{
		get
		{
			return _algorithm;
		}
		set
		{
			_algorithm = value;
			_cachedXml = null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptionMethod" /> class. </summary>
	public EncryptionMethod()
	{
		_cachedXml = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptionMethod" /> class specifying an algorithm Uniform Resource Identifier (URI). </summary>
	/// <param name="algorithm">The Uniform Resource Identifier (URI) that describes the algorithm represented by an instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptionMethod" /> class. </param>
	public EncryptionMethod(string algorithm)
	{
		_algorithm = algorithm;
		_cachedXml = null;
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlElement" /> object that encapsulates an instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptionMethod" /> class.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlElement" /> object that encapsulates an instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptionMethod" /> class.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public XmlElement GetXml()
	{
		if (CacheValid)
		{
			return _cachedXml;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		return GetXml(xmlDocument);
	}

	internal XmlElement GetXml(XmlDocument document)
	{
		XmlElement xmlElement = document.CreateElement("EncryptionMethod", "http://www.w3.org/2001/04/xmlenc#");
		if (!string.IsNullOrEmpty(_algorithm))
		{
			xmlElement.SetAttribute("Algorithm", _algorithm);
		}
		if (_keySize > 0)
		{
			XmlElement xmlElement2 = document.CreateElement("KeySize", "http://www.w3.org/2001/04/xmlenc#");
			xmlElement2.AppendChild(document.CreateTextNode(_keySize.ToString(null, null)));
			xmlElement.AppendChild(xmlElement2);
		}
		return xmlElement;
	}

	/// <summary>Parses the specified <see cref="T:System.Xml.XmlElement" /> object and configures the internal state of the <see cref="T:System.Security.Cryptography.Xml.EncryptionMethod" /> object to match.</summary>
	/// <param name="value">An <see cref="T:System.Xml.XmlElement" /> object to parse.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The key size expressed in the <paramref name="value" /> parameter was less than 0. </exception>
	public void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(value.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
		_algorithm = Utils.GetAttribute(value, "Algorithm", "http://www.w3.org/2001/04/xmlenc#");
		XmlNode xmlNode = value.SelectSingleNode("enc:KeySize", xmlNamespaceManager);
		if (xmlNode != null)
		{
			KeySize = Convert.ToInt32(Utils.DiscardWhiteSpaces(xmlNode.InnerText), null);
		}
		_cachedXml = value;
	}
}
