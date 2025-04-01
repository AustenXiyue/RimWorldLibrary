using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the &lt;CipherReference&gt; element in XML encryption. This class cannot be inherited.</summary>
public sealed class CipherReference : EncryptedReference
{
	private byte[] _cipherValue;

	internal byte[] CipherValue
	{
		get
		{
			if (!base.CacheValid)
			{
				return null;
			}
			return _cipherValue;
		}
		set
		{
			_cipherValue = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.CipherReference" /> class.</summary>
	public CipherReference()
	{
		base.ReferenceType = "CipherReference";
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.CipherReference" /> class using the specified Uniform Resource Identifier (URI).</summary>
	/// <param name="uri">A Uniform Resource Identifier (URI) pointing to the encrypted data.</param>
	public CipherReference(string uri)
		: base(uri)
	{
		base.ReferenceType = "CipherReference";
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.CipherReference" /> class using the specified Uniform Resource Identifier (URI) and transform chain information.</summary>
	/// <param name="uri">A Uniform Resource Identifier (URI) pointing to the encrypted data.</param>
	/// <param name="transformChain">A <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> object that describes transforms to do on the encrypted data.</param>
	public CipherReference(string uri, TransformChain transformChain)
		: base(uri, transformChain)
	{
		base.ReferenceType = "CipherReference";
	}

	/// <summary>Returns the XML representation of a <see cref="T:System.Security.Cryptography.Xml.CipherReference" /> object.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlElement" /> that represents the &lt;CipherReference&gt; element in XML encryption.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="T:System.Security.Cryptography.Xml.CipherReference" /> value is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override XmlElement GetXml()
	{
		if (base.CacheValid)
		{
			return _cachedXml;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		return GetXml(xmlDocument);
	}

	internal new XmlElement GetXml(XmlDocument document)
	{
		if (base.ReferenceType == null)
		{
			throw new CryptographicException("The Reference type must be set in an EncryptedReference object.");
		}
		XmlElement xmlElement = document.CreateElement(base.ReferenceType, "http://www.w3.org/2001/04/xmlenc#");
		if (!string.IsNullOrEmpty(base.Uri))
		{
			xmlElement.SetAttribute("URI", base.Uri);
		}
		if (base.TransformChain.Count > 0)
		{
			xmlElement.AppendChild(base.TransformChain.GetXml(document, "http://www.w3.org/2001/04/xmlenc#"));
		}
		return xmlElement;
	}

	/// <summary>Loads XML information into the &lt;CipherReference&gt; element in XML encryption.</summary>
	/// <param name="value">An <see cref="T:System.Xml.XmlElement" /> object that represents an XML element to use as the reference.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> provided is null.</exception>
	public override void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		base.ReferenceType = value.LocalName;
		base.Uri = Utils.GetAttribute(value, "URI", "http://www.w3.org/2001/04/xmlenc#");
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(value.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
		XmlNode xmlNode = value.SelectSingleNode("enc:Transforms", xmlNamespaceManager);
		if (xmlNode != null)
		{
			base.TransformChain.LoadXml(xmlNode as XmlElement);
		}
		_cachedXml = value;
	}
}
