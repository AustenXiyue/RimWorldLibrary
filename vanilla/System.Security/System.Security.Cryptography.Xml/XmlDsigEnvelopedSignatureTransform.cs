using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the enveloped signature transform for an XML digital signature as defined by the W3C.</summary>
public class XmlDsigEnvelopedSignatureTransform : Transform
{
	private Type[] _inputTypes = new Type[3]
	{
		typeof(Stream),
		typeof(XmlNodeList),
		typeof(XmlDocument)
	};

	private Type[] _outputTypes = new Type[2]
	{
		typeof(XmlNodeList),
		typeof(XmlDocument)
	};

	private XmlNodeList _inputNodeList;

	private bool _includeComments;

	private XmlNamespaceManager _nsm;

	private XmlDocument _containingDocument;

	private int _signaturePosition;

	internal int SignaturePosition
	{
		set
		{
			_signaturePosition = value;
		}
	}

	/// <summary>Gets an array of types that are valid inputs to the <see cref="M:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform.LoadInput(System.Object)" /> method of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</summary>
	/// <returns>An array of valid input types for the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object; you can pass only objects of one of these types to the <see cref="M:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform.LoadInput(System.Object)" /> method of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</returns>
	public override Type[] InputTypes => _inputTypes;

	/// <summary>Gets an array of types that are possible outputs from the <see cref="M:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform.GetOutput" /> methods of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</summary>
	/// <returns>An array of valid output types for the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object; only objects of one of these types are returned from the <see cref="M:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform.GetOutput" /> methods of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</returns>
	public override Type[] OutputTypes => _outputTypes;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> class.</summary>
	public XmlDsigEnvelopedSignatureTransform()
	{
		base.Algorithm = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> class with comments, if specified.</summary>
	/// <param name="includeComments">true to include comments; otherwise, false. </param>
	public XmlDsigEnvelopedSignatureTransform(bool includeComments)
	{
		_includeComments = includeComments;
		base.Algorithm = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
	}

	/// <summary>Parses the specified <see cref="T:System.Xml.XmlNodeList" /> as transform-specific content of a &lt;Transform&gt; element and configures the internal state of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object to match the &lt;Transform&gt; element.</summary>
	/// <param name="nodeList">An <see cref="T:System.Xml.XmlNodeList" /> to load into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object. </param>
	public override void LoadInnerXml(XmlNodeList nodeList)
	{
	}

	/// <summary>Returns an XML representation of the parameters of an <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object that are suitable to be included as subelements of an XMLDSIG &lt;Transform&gt; element.</summary>
	/// <returns>A list of the XML nodes that represent the transform-specific content needed to describe the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object in an XMLDSIG &lt;Transform&gt; element.</returns>
	protected override XmlNodeList GetInnerXml()
	{
		return null;
	}

	/// <summary>Loads the specified input into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</summary>
	/// <param name="obj">The input to load into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="obj" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The containing XML document is null.</exception>
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
		else if (obj is XmlNodeList)
		{
			LoadXmlNodeListInput((XmlNodeList)obj);
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
		if (_containingDocument == null)
		{
			throw new CryptographicException("An XmlDocument context is required for enveloped transforms.");
		}
		_nsm = new XmlNamespaceManager(_containingDocument.NameTable);
		_nsm.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
	}

	private void LoadXmlNodeListInput(XmlNodeList nodeList)
	{
		if (nodeList == null)
		{
			throw new ArgumentNullException("nodeList");
		}
		_containingDocument = Utils.GetOwnerDocument(nodeList);
		if (_containingDocument == null)
		{
			throw new CryptographicException("An XmlDocument context is required for enveloped transforms.");
		}
		_nsm = new XmlNamespaceManager(_containingDocument.NameTable);
		_nsm.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
		_inputNodeList = nodeList;
	}

	private void LoadXmlDocumentInput(XmlDocument doc)
	{
		if (doc == null)
		{
			throw new ArgumentNullException("doc");
		}
		_containingDocument = doc;
		_nsm = new XmlNamespaceManager(_containingDocument.NameTable);
		_nsm.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
	}

	/// <summary>Returns the output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</summary>
	/// <returns>The output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The containing XML document is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override object GetOutput()
	{
		if (_containingDocument == null)
		{
			throw new CryptographicException("An XmlDocument context is required for enveloped transforms.");
		}
		if (_inputNodeList != null)
		{
			if (_signaturePosition == 0)
			{
				return _inputNodeList;
			}
			XmlNodeList xmlNodeList = _containingDocument.SelectNodes("//dsig:Signature", _nsm);
			if (xmlNodeList == null)
			{
				return _inputNodeList;
			}
			CanonicalXmlNodeList canonicalXmlNodeList = new CanonicalXmlNodeList();
			{
				foreach (XmlNode inputNode in _inputNodeList)
				{
					if (inputNode == null)
					{
						continue;
					}
					if (Utils.IsXmlNamespaceNode(inputNode) || Utils.IsNamespaceNode(inputNode))
					{
						canonicalXmlNodeList.Add(inputNode);
						continue;
					}
					try
					{
						XmlNode xmlNode2 = inputNode.SelectSingleNode("ancestor-or-self::dsig:Signature[1]", _nsm);
						int num = 0;
						foreach (XmlNode item in xmlNodeList)
						{
							num++;
							if (item == xmlNode2)
							{
								break;
							}
						}
						if (xmlNode2 == null || (xmlNode2 != null && num != _signaturePosition))
						{
							canonicalXmlNodeList.Add(inputNode);
						}
					}
					catch
					{
					}
				}
				return canonicalXmlNodeList;
			}
		}
		XmlNodeList xmlNodeList2 = _containingDocument.SelectNodes("//dsig:Signature", _nsm);
		if (xmlNodeList2 == null)
		{
			return _containingDocument;
		}
		if (xmlNodeList2.Count < _signaturePosition || _signaturePosition <= 0)
		{
			return _containingDocument;
		}
		xmlNodeList2[_signaturePosition - 1].ParentNode.RemoveChild(xmlNodeList2[_signaturePosition - 1]);
		return _containingDocument;
	}

	/// <summary>Returns the output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object of type <see cref="T:System.Xml.XmlNodeList" />.</summary>
	/// <returns>The output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform" /> object of type <see cref="T:System.Xml.XmlNodeList" />.</returns>
	/// <param name="type">The type of the output to return. <see cref="T:System.Xml.XmlNodeList" /> is the only valid type for this parameter. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="type" /> parameter is not an <see cref="T:System.Xml.XmlNodeList" /> object.</exception>
	public override object GetOutput(Type type)
	{
		if (type == typeof(XmlNodeList) || type.IsSubclassOf(typeof(XmlNodeList)))
		{
			if (_inputNodeList == null)
			{
				_inputNodeList = Utils.AllDescendantNodes(_containingDocument, includeComments: true);
			}
			return (XmlNodeList)GetOutput();
		}
		if (type == typeof(XmlDocument) || type.IsSubclassOf(typeof(XmlDocument)))
		{
			if (_inputNodeList != null)
			{
				throw new ArgumentException("The input type was invalid for this transform.", "type");
			}
			return (XmlDocument)GetOutput();
		}
		throw new ArgumentException("The input type was invalid for this transform.", "type");
	}
}
