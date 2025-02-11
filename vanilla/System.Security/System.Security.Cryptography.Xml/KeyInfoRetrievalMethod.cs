using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>References <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> objects stored at a different location when using XMLDSIG or XML encryption.</summary>
public class KeyInfoRetrievalMethod : KeyInfoClause
{
	private string _uri;

	private string _type;

	/// <summary>Gets or sets the Uniform Resource Identifier (URI) of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" /> object.</summary>
	/// <returns>The Uniform Resource Identifier (URI) of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" /> object.</returns>
	public string Uri
	{
		get
		{
			return _uri;
		}
		set
		{
			_uri = value;
		}
	}

	/// <summary>Gets or sets a Uniform Resource Identifier (URI) that describes the type of data to be retrieved.</summary>
	/// <returns>A Uniform Resource Identifier (URI) that describes the type of data to be retrieved.</returns>
	public string Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" /> class.</summary>
	public KeyInfoRetrievalMethod()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" /> class with the specified Uniform Resource Identifier (URI) pointing to the referenced <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object.</summary>
	/// <param name="strUri">The Uniform Resource Identifier (URI) of the information to be referenced by the new instance of <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" />. </param>
	public KeyInfoRetrievalMethod(string strUri)
	{
		_uri = strUri;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" /> class with the specified Uniform Resource Identifier (URI) pointing to the referenced <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object and the URI that describes the type of data to retrieve.  </summary>
	/// <param name="strUri">The Uniform Resource Identifier (URI) of the information to be referenced by the new instance of <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" />.</param>
	/// <param name="typeName">The URI that describes the type of data to retrieve.</param>
	public KeyInfoRetrievalMethod(string strUri, string typeName)
	{
		_uri = strUri;
		_type = typeName;
	}

	/// <summary>Returns the XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" /> object.</summary>
	/// <returns>The XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" /> object.</returns>
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
		XmlElement xmlElement = xmlDocument.CreateElement("RetrievalMethod", "http://www.w3.org/2000/09/xmldsig#");
		if (!string.IsNullOrEmpty(_uri))
		{
			xmlElement.SetAttribute("URI", _uri);
		}
		if (!string.IsNullOrEmpty(_type))
		{
			xmlElement.SetAttribute("Type", _type);
		}
		return xmlElement;
	}

	/// <summary>Parses the input <see cref="T:System.Xml.XmlElement" /> object and configures the internal state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" /> object to match.</summary>
	/// <param name="value">The XML element that specifies the state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoRetrievalMethod" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	public override void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		_uri = Utils.GetAttribute(value, "URI", "http://www.w3.org/2000/09/xmldsig#");
		_type = Utils.GetAttribute(value, "Type", "http://www.w3.org/2000/09/xmldsig#");
	}
}
