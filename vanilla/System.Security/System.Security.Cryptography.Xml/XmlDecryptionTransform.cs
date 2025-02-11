using System.Collections;
using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Specifies the order of XML Digital Signature and XML Encryption operations when both are performed on the same document.</summary>
public class XmlDecryptionTransform : Transform
{
	private Type[] _inputTypes = new Type[2]
	{
		typeof(Stream),
		typeof(XmlDocument)
	};

	private Type[] _outputTypes = new Type[1] { typeof(XmlDocument) };

	private XmlNodeList _encryptedDataList;

	private ArrayList _arrayListUri;

	private EncryptedXml _exml;

	private XmlDocument _containingDocument;

	private XmlNamespaceManager _nsm;

	private const string XmlDecryptionTransformNamespaceUrl = "http://www.w3.org/2002/07/decrypt#";

	private ArrayList ExceptUris
	{
		get
		{
			if (_arrayListUri == null)
			{
				_arrayListUri = new ArrayList();
			}
			return _arrayListUri;
		}
	}

	/// <summary>Gets or sets an <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> object that contains information about the keys necessary to decrypt an XML document.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Xml.EncryptedXml" /> object that contains information about the keys necessary to decrypt an XML document.</returns>
	public EncryptedXml EncryptedXml
	{
		get
		{
			if (_exml != null)
			{
				return _exml;
			}
			Reference reference = base.Reference;
			SignedXml signedXml = ((reference == null) ? base.SignedXml : reference.SignedXml);
			if (signedXml == null || signedXml.EncryptedXml == null)
			{
				_exml = new EncryptedXml(_containingDocument);
			}
			else
			{
				_exml = signedXml.EncryptedXml;
			}
			return _exml;
		}
		set
		{
			_exml = value;
		}
	}

	/// <summary>Gets an array of types that are valid inputs to the <see cref="M:System.Security.Cryptography.Xml.XmlDecryptionTransform.LoadInput(System.Object)" /> method of the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object.</summary>
	/// <returns>An array of valid input types for the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object; you can pass only objects of one of these types to the <see cref="M:System.Security.Cryptography.Xml.XmlDecryptionTransform.LoadInput(System.Object)" /> method of the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object.</returns>
	public override Type[] InputTypes => _inputTypes;

	/// <summary>Gets an array of types that are possible outputs from the <see cref="M:System.Security.Cryptography.Xml.XmlDecryptionTransform.GetOutput" /> methods of the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object.</summary>
	/// <returns>An array of valid output types for the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object; only objects of one of these types are returned from the <see cref="M:System.Security.Cryptography.Xml.XmlDecryptionTransform.GetOutput" /> methods of the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object.</returns>
	public override Type[] OutputTypes => _outputTypes;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> class. </summary>
	public XmlDecryptionTransform()
	{
		base.Algorithm = "http://www.w3.org/2002/07/decrypt#XML";
	}

	/// <summary>Determines whether the ID attribute of an <see cref="T:System.Xml.XmlElement" /> object matches a specified value.</summary>
	/// <returns>true if the ID attribute of the <paramref name="inputElement" /> parameter matches the <paramref name="idValue" /> parameter; otherwise, false. </returns>
	/// <param name="inputElement">An <see cref="T:System.Xml.XmlElement" /> object with an ID attribute to compare with <paramref name="idValue" />.</param>
	/// <param name="idValue">The value to compare with the ID attribute of <paramref name="inputElement" />.</param>
	protected virtual bool IsTargetElement(XmlElement inputElement, string idValue)
	{
		if (inputElement == null)
		{
			return false;
		}
		if (inputElement.GetAttribute("Id") == idValue || inputElement.GetAttribute("id") == idValue || inputElement.GetAttribute("ID") == idValue)
		{
			return true;
		}
		return false;
	}

	/// <summary>Adds a Uniform Resource Identifier (URI) to exclude from processing.</summary>
	/// <param name="uri">A Uniform Resource Identifier (URI) to exclude from processing</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="uri" /> parameter is null.</exception>
	public void AddExceptUri(string uri)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		ExceptUris.Add(uri);
	}

	/// <summary>Parses the specified <see cref="T:System.Xml.XmlNodeList" /> object as transform-specific content of a &lt;Transform&gt; element and configures the internal state of the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object to match the &lt;Transform&gt; element.</summary>
	/// <param name="nodeList">An <see cref="T:System.Xml.XmlNodeList" /> object that specifies transform-specific content for the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="nodeList" /> parameter is null.-or-The Uniform Resource Identifier (URI) value of an <see cref="T:System.Xml.XmlNode" /> object in <paramref name="nodeList" /> was not found.-or-The length of the URI value of an <see cref="T:System.Xml.XmlNode" /> object in <paramref name="nodeList" /> is 0. -or-The first character of the URI value of an <see cref="T:System.Xml.XmlNode" /> object in <paramref name="nodeList" /> is not '#'.  </exception>
	public override void LoadInnerXml(XmlNodeList nodeList)
	{
		if (nodeList == null)
		{
			throw new CryptographicException("Unknown transform has been encountered.");
		}
		ExceptUris.Clear();
		foreach (XmlNode node in nodeList)
		{
			if (node is XmlElement { LocalName: "Except", NamespaceURI: "http://www.w3.org/2002/07/decrypt#" } xmlElement)
			{
				string attribute = Utils.GetAttribute(xmlElement, "URI", "http://www.w3.org/2002/07/decrypt#");
				if (attribute == null || attribute.Length == 0 || attribute[0] != '#')
				{
					throw new CryptographicException("A Uri attribute is required for a CipherReference element.");
				}
				string value = Utils.ExtractIdFromLocalUri(attribute);
				ExceptUris.Add(value);
			}
		}
	}

	/// <summary>Returns an XML representation of the parameters of an <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object that are suitable to be included as subelements of an XMLDSIG &lt;Transform&gt; element.</summary>
	/// <returns>A list of the XML nodes that represent the transform-specific content needed to describe the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object in an XMLDSIG &lt;Transform&gt; element.</returns>
	protected override XmlNodeList GetInnerXml()
	{
		if (ExceptUris.Count == 0)
		{
			return null;
		}
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("Transform", "http://www.w3.org/2000/09/xmldsig#");
		if (!string.IsNullOrEmpty(base.Algorithm))
		{
			xmlElement.SetAttribute("Algorithm", base.Algorithm);
		}
		foreach (string exceptUri in ExceptUris)
		{
			XmlElement xmlElement2 = xmlDocument.CreateElement("Except", "http://www.w3.org/2002/07/decrypt#");
			xmlElement2.SetAttribute("URI", exceptUri);
			xmlElement.AppendChild(xmlElement2);
		}
		return xmlElement.ChildNodes;
	}

	/// <summary>When overridden in a derived class, loads the specified input into the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object.</summary>
	/// <param name="obj">The input to load into the current <see cref="T:System.Security.Cryptography.Xml.XmlDecryptionTransform" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void LoadInput(object obj)
	{
		if (obj is Stream)
		{
			LoadStreamInput((Stream)obj);
		}
		else if (obj is XmlDocument)
		{
			LoadXmlDocumentInput((XmlDocument)obj);
		}
	}

	private void LoadStreamInput(Stream stream)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		XmlResolver xmlResolver = (base.ResolverSet ? _xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), base.BaseURI));
		XmlReader reader = Utils.PreProcessStreamInput(stream, xmlResolver, base.BaseURI);
		xmlDocument.Load(reader);
		_containingDocument = xmlDocument;
		_nsm = new XmlNamespaceManager(_containingDocument.NameTable);
		_nsm.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
		_encryptedDataList = xmlDocument.SelectNodes("//enc:EncryptedData", _nsm);
	}

	private void LoadXmlDocumentInput(XmlDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		_containingDocument = document;
		_nsm = new XmlNamespaceManager(document.NameTable);
		_nsm.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
		_encryptedDataList = document.SelectNodes("//enc:EncryptedData", _nsm);
	}

	private void ReplaceEncryptedData(XmlElement encryptedDataElement, byte[] decrypted)
	{
		XmlNode parentNode = encryptedDataElement.ParentNode;
		if (parentNode.NodeType == XmlNodeType.Document)
		{
			parentNode.InnerXml = EncryptedXml.Encoding.GetString(decrypted);
		}
		else
		{
			EncryptedXml.ReplaceData(encryptedDataElement, decrypted);
		}
	}

	private bool ProcessEncryptedDataItem(XmlElement encryptedDataElement)
	{
		if (ExceptUris.Count > 0)
		{
			for (int i = 0; i < ExceptUris.Count; i++)
			{
				if (IsTargetElement(encryptedDataElement, (string)ExceptUris[i]))
				{
					return false;
				}
			}
		}
		EncryptedData encryptedData = new EncryptedData();
		encryptedData.LoadXml(encryptedDataElement);
		SymmetricAlgorithm decryptionKey = EncryptedXml.GetDecryptionKey(encryptedData, null);
		if (decryptionKey == null)
		{
			throw new CryptographicException("Unable to retrieve the decryption key.");
		}
		byte[] decrypted = EncryptedXml.DecryptData(encryptedData, decryptionKey);
		ReplaceEncryptedData(encryptedDataElement, decrypted);
		return true;
	}

	private void ProcessElementRecursively(XmlNodeList encryptedDatas)
	{
		if (encryptedDatas == null || encryptedDatas.Count == 0)
		{
			return;
		}
		Queue queue = new Queue();
		foreach (XmlNode encryptedData in encryptedDatas)
		{
			queue.Enqueue(encryptedData);
		}
		XmlNode xmlNode = queue.Dequeue() as XmlNode;
		while (xmlNode != null)
		{
			if (xmlNode is XmlElement { LocalName: "EncryptedData", NamespaceURI: "http://www.w3.org/2001/04/xmlenc#", NextSibling: var nextSibling, ParentNode: var parentNode } xmlElement && ProcessEncryptedDataItem(xmlElement))
			{
				XmlNode xmlNode2 = parentNode.FirstChild;
				while (xmlNode2 != null && xmlNode2.NextSibling != nextSibling)
				{
					xmlNode2 = xmlNode2.NextSibling;
				}
				if (xmlNode2 != null)
				{
					XmlNodeList xmlNodeList = xmlNode2.SelectNodes("//enc:EncryptedData", _nsm);
					if (xmlNodeList.Count > 0)
					{
						foreach (XmlNode item in xmlNodeList)
						{
							queue.Enqueue(item);
						}
					}
				}
			}
			if (queue.Count != 0)
			{
				xmlNode = queue.Dequeue() as XmlNode;
				continue;
			}
			break;
		}
	}

	/// <summary>Returns the output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</summary>
	/// <returns>The output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A decryption key could not be found.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	/// </PermissionSet>
	public override object GetOutput()
	{
		if (_encryptedDataList != null)
		{
			ProcessElementRecursively(_encryptedDataList);
		}
		Utils.AddNamespaces(_containingDocument.DocumentElement, base.PropagatedNamespaces);
		return _containingDocument;
	}

	/// <summary>Returns the output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</summary>
	/// <returns>The output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</returns>
	/// <param name="type">The type of the output to return. <see cref="T:System.Xml.XmlNodeList" /> is the only valid type for this parameter.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="type" /> parameter is not an <see cref="T:System.Xml.XmlNodeList" /> object.</exception>
	public override object GetOutput(Type type)
	{
		if (type == typeof(XmlDocument))
		{
			return (XmlDocument)GetOutput();
		}
		throw new ArgumentException("The input type was invalid for this transform.", "type");
	}
}
