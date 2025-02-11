using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the &lt;CipherData&gt; element in XML encryption. This class cannot be inherited.</summary>
public sealed class CipherData
{
	private XmlElement _cachedXml;

	private CipherReference _cipherReference;

	private byte[] _cipherValue;

	private bool CacheValid => _cachedXml != null;

	/// <summary>Gets or sets the &lt;CipherReference&gt; element.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Xml.CipherReference" /> object.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherReference" />  property was set to null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherReference" />  property was set more than once.</exception>
	public CipherReference CipherReference
	{
		get
		{
			return _cipherReference;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (CipherValue != null)
			{
				throw new CryptographicException("A Cipher Data element should have either a CipherValue or a CipherReference element.");
			}
			_cipherReference = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets or sets the &lt;CipherValue&gt; element.</summary>
	/// <returns>A byte array that represents the &lt;CipherValue&gt; element.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherValue" />  property was set to null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherValue" />  property was set more than once.</exception>
	public byte[] CipherValue
	{
		get
		{
			return _cipherValue;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (CipherReference != null)
			{
				throw new CryptographicException("A Cipher Data element should have either a CipherValue or a CipherReference element.");
			}
			_cipherValue = (byte[])value.Clone();
			_cachedXml = null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.CipherData" /> class.</summary>
	public CipherData()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.CipherData" /> class using a byte array as the <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherValue" /> value.</summary>
	/// <param name="cipherValue">The encrypted data to use for the &lt;CipherValue&gt; element.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="cipherValue" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherValue" /> property has already been set.</exception>
	public CipherData(byte[] cipherValue)
	{
		CipherValue = cipherValue;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.CipherData" /> class using a <see cref="T:System.Security.Cryptography.Xml.CipherReference" /> object.</summary>
	/// <param name="cipherReference">The <see cref="T:System.Security.Cryptography.Xml.CipherReference" /> object to use.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="cipherValue" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherValue" /> property has already been set.</exception>
	public CipherData(CipherReference cipherReference)
	{
		CipherReference = cipherReference;
	}

	/// <summary>Gets the XML values for the <see cref="T:System.Security.Cryptography.Xml.CipherData" /> object.</summary>
	/// <returns>A <see cref="T:System.Xml.XmlElement" /> object that represents the XML information for the <see cref="T:System.Security.Cryptography.Xml.CipherData" /> object.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherValue" /> property and the <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherReference" /> property are null.</exception>
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
		XmlElement xmlElement = document.CreateElement("CipherData", "http://www.w3.org/2001/04/xmlenc#");
		if (CipherValue != null)
		{
			XmlElement xmlElement2 = document.CreateElement("CipherValue", "http://www.w3.org/2001/04/xmlenc#");
			xmlElement2.AppendChild(document.CreateTextNode(Convert.ToBase64String(CipherValue)));
			xmlElement.AppendChild(xmlElement2);
		}
		else
		{
			if (CipherReference == null)
			{
				throw new CryptographicException("A Cipher Data element should have either a CipherValue or a CipherReference element.");
			}
			xmlElement.AppendChild(CipherReference.GetXml(document));
		}
		return xmlElement;
	}

	/// <summary>Loads XML data from an <see cref="T:System.Xml.XmlElement" /> into a <see cref="T:System.Security.Cryptography.Xml.CipherData" /> object.</summary>
	/// <param name="value">An <see cref="T:System.Xml.XmlElement" /> that represents the XML data to load.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherValue" /> property and the <see cref="P:System.Security.Cryptography.Xml.CipherData.CipherReference" /> property are null.</exception>
	public void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(value.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
		XmlNode xmlNode = value.SelectSingleNode("enc:CipherValue", xmlNamespaceManager);
		XmlNode xmlNode2 = value.SelectSingleNode("enc:CipherReference", xmlNamespaceManager);
		if (xmlNode != null)
		{
			if (xmlNode2 != null)
			{
				throw new CryptographicException("A Cipher Data element should have either a CipherValue or a CipherReference element.");
			}
			_cipherValue = Convert.FromBase64String(Utils.DiscardWhiteSpaces(xmlNode.InnerText));
		}
		else
		{
			if (xmlNode2 == null)
			{
				throw new CryptographicException("A Cipher Data element should have either a CipherValue or a CipherReference element.");
			}
			_cipherReference = new CipherReference();
			_cipherReference.LoadXml((XmlElement)xmlNode2);
		}
		_cachedXml = value;
	}
}
