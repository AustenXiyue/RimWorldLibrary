using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the exclusive C14N XML canonicalization transform for a digital signature as defined by the World Wide Web Consortium (W3C), without comments.</summary>
public class XmlDsigExcC14NTransform : Transform
{
	private Type[] _inputTypes = new Type[3]
	{
		typeof(Stream),
		typeof(XmlDocument),
		typeof(XmlNodeList)
	};

	private Type[] _outputTypes = new Type[1] { typeof(Stream) };

	private bool _includeComments;

	private string _inclusiveNamespacesPrefixList;

	private ExcCanonicalXml _excCanonicalXml;

	/// <summary>Gets or sets a string that contains namespace prefixes to canonicalize using the standard canonicalization algorithm. </summary>
	/// <returns>A string that contains namespace prefixes to canonicalize using the standard canonicalization algorithm.</returns>
	public string InclusiveNamespacesPrefixList
	{
		get
		{
			return _inclusiveNamespacesPrefixList;
		}
		set
		{
			_inclusiveNamespacesPrefixList = value;
		}
	}

	/// <summary>Gets an array of types that are valid inputs to the <see cref="M:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform.LoadInput(System.Object)" /> method of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object.</summary>
	/// <returns>An array of valid input types for the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object; you can pass only objects of one of these types to the <see cref="M:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform.LoadInput(System.Object)" /> method of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object.</returns>
	public override Type[] InputTypes => _inputTypes;

	/// <summary>Gets an array of types that are possible outputs from the <see cref="M:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform.GetOutput" /> methods of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object.</summary>
	/// <returns>An array of valid output types for the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object; the <see cref="Overload:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform.GetOutput" /> methods of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object return only objects of one of these types.</returns>
	public override Type[] OutputTypes => _outputTypes;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> class. </summary>
	public XmlDsigExcC14NTransform()
		: this(includeComments: false, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> class specifying a value that determines whether to include comments. </summary>
	/// <param name="includeComments">true to include comments; otherwise, false.</param>
	public XmlDsigExcC14NTransform(bool includeComments)
		: this(includeComments, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> class specifying a list of namespace prefixes to canonicalize using the standard canonicalization algorithm. </summary>
	/// <param name="inclusiveNamespacesPrefixList">The namespace prefixes to canonicalize using the standard canonicalization algorithm.</param>
	public XmlDsigExcC14NTransform(string inclusiveNamespacesPrefixList)
		: this(includeComments: false, inclusiveNamespacesPrefixList)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> class specifying whether to include comments, and specifying a list of namespace prefixes. </summary>
	/// <param name="includeComments">true to include comments; otherwise, false.</param>
	/// <param name="inclusiveNamespacesPrefixList">The namespace prefixes to canonicalize using the standard canonicalization algorithm.</param>
	public XmlDsigExcC14NTransform(bool includeComments, string inclusiveNamespacesPrefixList)
	{
		_includeComments = includeComments;
		_inclusiveNamespacesPrefixList = inclusiveNamespacesPrefixList;
		base.Algorithm = (includeComments ? "http://www.w3.org/2001/10/xml-exc-c14n#WithComments" : "http://www.w3.org/2001/10/xml-exc-c14n#");
	}

	/// <summary>Parses the specified <see cref="T:System.Xml.XmlNodeList" /> object as transform-specific content of a &lt;Transform&gt; element and configures the internal state of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object to match the &lt;Transform&gt; element.</summary>
	/// <param name="nodeList">An <see cref="T:System.Xml.XmlNodeList" /> object that specifies transform-specific content for the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object.</param>
	public override void LoadInnerXml(XmlNodeList nodeList)
	{
		if (nodeList == null)
		{
			return;
		}
		foreach (XmlNode node in nodeList)
		{
			if (node is XmlElement xmlElement && xmlElement.LocalName.Equals("InclusiveNamespaces") && xmlElement.NamespaceURI.Equals("http://www.w3.org/2001/10/xml-exc-c14n#") && Utils.HasAttribute(xmlElement, "PrefixList", "http://www.w3.org/2000/09/xmldsig#"))
			{
				InclusiveNamespacesPrefixList = Utils.GetAttribute(xmlElement, "PrefixList", "http://www.w3.org/2000/09/xmldsig#");
				break;
			}
		}
	}

	/// <summary>When overridden in a derived class, loads the specified input into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object.</summary>
	/// <param name="obj">The input to load into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="obj" /> parameter is not a <see cref="T:System.IO.Stream" /> object.-or-The <paramref name="obj" /> parameter is not an <see cref="T:System.Xml.XmlDocument" /> object.-or-The <paramref name="obj" /> parameter is not an <see cref="T:System.Xml.XmlNodeList" /> object.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void LoadInput(object obj)
	{
		XmlResolver resolver = (base.ResolverSet ? _xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), base.BaseURI));
		if (obj is Stream)
		{
			_excCanonicalXml = new ExcCanonicalXml((Stream)obj, _includeComments, _inclusiveNamespacesPrefixList, resolver, base.BaseURI);
			return;
		}
		if (obj is XmlDocument)
		{
			_excCanonicalXml = new ExcCanonicalXml((XmlDocument)obj, _includeComments, _inclusiveNamespacesPrefixList, resolver);
			return;
		}
		if (obj is XmlNodeList)
		{
			_excCanonicalXml = new ExcCanonicalXml((XmlNodeList)obj, _includeComments, _inclusiveNamespacesPrefixList, resolver);
			return;
		}
		throw new ArgumentException("Type of input object is invalid.", "obj");
	}

	/// <summary>Returns an XML representation of the parameters of a <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object that are suitable to be included as subelements of an XMLDSIG &lt;Transform&gt; element.</summary>
	/// <returns>A list of the XML nodes that represent the transform-specific content needed to describe the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object in an XMLDSIG &lt;Transform&gt; element.</returns>
	protected override XmlNodeList GetInnerXml()
	{
		if (InclusiveNamespacesPrefixList == null)
		{
			return null;
		}
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("Transform", "http://www.w3.org/2000/09/xmldsig#");
		if (!string.IsNullOrEmpty(base.Algorithm))
		{
			xmlElement.SetAttribute("Algorithm", base.Algorithm);
		}
		XmlElement xmlElement2 = xmlDocument.CreateElement("InclusiveNamespaces", "http://www.w3.org/2001/10/xml-exc-c14n#");
		xmlElement2.SetAttribute("PrefixList", InclusiveNamespacesPrefixList);
		xmlElement.AppendChild(xmlElement2);
		return xmlElement.ChildNodes;
	}

	/// <summary>Returns the output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object</summary>
	/// <returns>The output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object.</returns>
	public override object GetOutput()
	{
		return new MemoryStream(_excCanonicalXml.GetBytes());
	}

	/// <summary>Returns the output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object as an object of the specified type.</summary>
	/// <returns>The output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object as an object of the specified type.</returns>
	/// <param name="type">The type of the output to return. This must be one of the types in the <see cref="P:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform.OutputTypes" /> property.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="type" /> parameter is not a <see cref="T:System.IO.Stream" /> object.-or-The <paramref name="type" /> parameter does not derive from a <see cref="T:System.IO.Stream" /> object.</exception>
	public override object GetOutput(Type type)
	{
		if (type != typeof(Stream) && !type.IsSubclassOf(typeof(Stream)))
		{
			throw new ArgumentException("The input type was invalid for this transform.", "type");
		}
		return new MemoryStream(_excCanonicalXml.GetBytes());
	}

	/// <summary>Returns the digest associated with a <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object.</summary>
	/// <returns>The digest associated with a <see cref="T:System.Security.Cryptography.Xml.XmlDsigExcC14NTransform" /> object.</returns>
	/// <param name="hash">The <see cref="T:System.Security.Cryptography.HashAlgorithm" /> object used to create a digest.</param>
	public override byte[] GetDigestedOutput(HashAlgorithm hash)
	{
		return _excCanonicalXml.GetDigestedBytes(hash);
	}
}
