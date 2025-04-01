using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the abstract base class used in XML encryption from which the <see cref="T:System.Security.Cryptography.Xml.CipherReference" />, <see cref="T:System.Security.Cryptography.Xml.KeyReference" />, and <see cref="T:System.Security.Cryptography.Xml.DataReference" /> classes derive.</summary>
public abstract class EncryptedReference
{
	private string _uri;

	private string _referenceType;

	private TransformChain _transformChain;

	internal XmlElement _cachedXml;

	/// <summary>Gets or sets the Uniform Resource Identifier (URI) of an <see cref="T:System.Security.Cryptography.Xml.EncryptedReference" /> object.</summary>
	/// <returns>The Uniform Resource Identifier (URI) of the <see cref="T:System.Security.Cryptography.Xml.EncryptedReference" /> object.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Cryptography.Xml.EncryptedReference.Uri" /> property was set to null.</exception>
	public string Uri
	{
		get
		{
			return _uri;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("A Uri attribute is required for a CipherReference element.");
			}
			_uri = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets or sets the transform chain of an <see cref="T:System.Security.Cryptography.Xml.EncryptedReference" /> object.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> object that describes transforms used on the encrypted data.</returns>
	public TransformChain TransformChain
	{
		get
		{
			if (_transformChain == null)
			{
				_transformChain = new TransformChain();
			}
			return _transformChain;
		}
		set
		{
			_transformChain = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets or sets a reference type.</summary>
	/// <returns>The reference type of the encrypted data.</returns>
	protected string ReferenceType
	{
		get
		{
			return _referenceType;
		}
		set
		{
			_referenceType = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets a value that indicates whether the cache is valid.</summary>
	/// <returns>true if the cache is valid; otherwise, false.</returns>
	protected internal bool CacheValid => _cachedXml != null;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptedReference" /> class.</summary>
	protected EncryptedReference()
		: this(string.Empty, new TransformChain())
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptedReference" /> class using the specified Uniform Resource Identifier (URI).</summary>
	/// <param name="uri">The Uniform Resource Identifier (URI) that points to the data to encrypt.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="uri" /> parameter is null.</exception>
	protected EncryptedReference(string uri)
		: this(uri, new TransformChain())
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptedReference" /> class using the specified Uniform Resource Identifier (URI) and transform chain.</summary>
	/// <param name="uri">The Uniform Resource Identifier (URI) that points to the data to encrypt.</param>
	/// <param name="transformChain">A <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> object that describes transforms to be done on the data to encrypt.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="uri" /> parameter is null.</exception>
	protected EncryptedReference(string uri, TransformChain transformChain)
	{
		TransformChain = transformChain;
		Uri = uri;
		_cachedXml = null;
	}

	/// <summary>Adds a <see cref="T:System.Security.Cryptography.Xml.Transform" /> object to the current transform chain of an <see cref="T:System.Security.Cryptography.Xml.EncryptedReference" /> object.</summary>
	/// <param name="transform">A <see cref="T:System.Security.Cryptography.Xml.Transform" /> object to add to the transform chain.</param>
	public void AddTransform(Transform transform)
	{
		TransformChain.Add(transform);
	}

	/// <summary>Returns the XML representation of an <see cref="T:System.Security.Cryptography.Xml.EncryptedReference" /> object.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlElement" /> object that represents the values of the &lt;EncryptedReference&gt; element in XML encryption.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.EncryptedReference.ReferenceType" /> property is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public virtual XmlElement GetXml()
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
		if (ReferenceType == null)
		{
			throw new CryptographicException("The Reference type must be set in an EncryptedReference object.");
		}
		XmlElement xmlElement = document.CreateElement(ReferenceType, "http://www.w3.org/2001/04/xmlenc#");
		if (!string.IsNullOrEmpty(_uri))
		{
			xmlElement.SetAttribute("URI", _uri);
		}
		if (TransformChain.Count > 0)
		{
			xmlElement.AppendChild(TransformChain.GetXml(document, "http://www.w3.org/2000/09/xmldsig#"));
		}
		return xmlElement;
	}

	/// <summary>Loads an XML element into an <see cref="T:System.Security.Cryptography.Xml.EncryptedReference" /> object.</summary>
	/// <param name="value">An <see cref="T:System.Xml.XmlElement" /> object that represents an XML element.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null.</exception>
	public virtual void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		ReferenceType = value.LocalName;
		Uri = Utils.GetAttribute(value, "URI", "http://www.w3.org/2001/04/xmlenc#");
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(value.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
		XmlNode xmlNode = value.SelectSingleNode("ds:Transforms", xmlNamespaceManager);
		if (xmlNode != null)
		{
			TransformChain.LoadXml(xmlNode as XmlElement);
		}
		_cachedXml = value;
	}
}
