using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the object element of an XML signature that holds data to be signed.</summary>
public class DataObject
{
	private string _id;

	private string _mimeType;

	private string _encoding;

	private CanonicalXmlNodeList _elData;

	private XmlElement _cachedXml;

	/// <summary>Gets or sets the identification of the current <see cref="T:System.Security.Cryptography.Xml.DataObject" /> object.</summary>
	/// <returns>The name of the element that contains data to be used. </returns>
	public string Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets or sets the MIME type of the current <see cref="T:System.Security.Cryptography.Xml.DataObject" /> object. </summary>
	/// <returns>The MIME type of the current <see cref="T:System.Security.Cryptography.Xml.DataObject" /> object. The default is null.</returns>
	public string MimeType
	{
		get
		{
			return _mimeType;
		}
		set
		{
			_mimeType = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets or sets the encoding of the current <see cref="T:System.Security.Cryptography.Xml.DataObject" /> object.</summary>
	/// <returns>The type of encoding of the current <see cref="T:System.Security.Cryptography.Xml.DataObject" /> object.</returns>
	public string Encoding
	{
		get
		{
			return _encoding;
		}
		set
		{
			_encoding = value;
			_cachedXml = null;
		}
	}

	/// <summary>Gets or sets the data value of the current <see cref="T:System.Security.Cryptography.Xml.DataObject" /> object.</summary>
	/// <returns>The data of the current <see cref="T:System.Security.Cryptography.Xml.DataObject" />.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value used to set the property is null.</exception>
	public XmlNodeList Data
	{
		get
		{
			return _elData;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_elData = new CanonicalXmlNodeList();
			foreach (XmlNode item in value)
			{
				_elData.Add(item);
			}
			_cachedXml = null;
		}
	}

	private bool CacheValid => _cachedXml != null;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.DataObject" /> class.</summary>
	public DataObject()
	{
		_cachedXml = null;
		_elData = new CanonicalXmlNodeList();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.DataObject" /> class with the specified identification, MIME type, encoding, and data.</summary>
	/// <param name="id">The identification to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.DataObject" /> with. </param>
	/// <param name="mimeType">The MIME type of the data used to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.DataObject" />. </param>
	/// <param name="encoding">The encoding of the data used to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.DataObject" />. </param>
	/// <param name="data">The data to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.DataObject" /> with. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="data" /> parameter is null. </exception>
	public DataObject(string id, string mimeType, string encoding, XmlElement data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		_id = id;
		_mimeType = mimeType;
		_encoding = encoding;
		_elData = new CanonicalXmlNodeList();
		_elData.Add(data);
		_cachedXml = null;
	}

	/// <summary>Returns the XML representation of the <see cref="T:System.Security.Cryptography.Xml.DataObject" /> object.</summary>
	/// <returns>The XML representation of the <see cref="T:System.Security.Cryptography.Xml.DataObject" /> object.</returns>
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
		XmlElement xmlElement = document.CreateElement("Object", "http://www.w3.org/2000/09/xmldsig#");
		if (!string.IsNullOrEmpty(_id))
		{
			xmlElement.SetAttribute("Id", _id);
		}
		if (!string.IsNullOrEmpty(_mimeType))
		{
			xmlElement.SetAttribute("MimeType", _mimeType);
		}
		if (!string.IsNullOrEmpty(_encoding))
		{
			xmlElement.SetAttribute("Encoding", _encoding);
		}
		if (_elData != null)
		{
			foreach (XmlNode elDatum in _elData)
			{
				xmlElement.AppendChild(document.ImportNode(elDatum, deep: true));
			}
		}
		return xmlElement;
	}

	/// <summary>Loads a <see cref="T:System.Security.Cryptography.Xml.DataObject" /> state from an XML element.</summary>
	/// <param name="value">The XML element to load the <see cref="T:System.Security.Cryptography.Xml.DataObject" /> state from. </param>
	/// <exception cref="T:System.ArgumentNullException">The value from the XML element is null.</exception>
	public void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		_id = Utils.GetAttribute(value, "Id", "http://www.w3.org/2000/09/xmldsig#");
		_mimeType = Utils.GetAttribute(value, "MimeType", "http://www.w3.org/2000/09/xmldsig#");
		_encoding = Utils.GetAttribute(value, "Encoding", "http://www.w3.org/2000/09/xmldsig#");
		foreach (XmlNode childNode in value.ChildNodes)
		{
			_elData.Add(childNode);
		}
		_cachedXml = value;
	}
}
