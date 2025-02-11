using System.Collections;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the &lt;Signature&gt; element of an XML signature.</summary>
public class Signature
{
	private static XmlNamespaceManager dsigNsmgr;

	private ArrayList list;

	private SignedInfo info;

	private KeyInfo key;

	private string id;

	private byte[] signature;

	private XmlElement element;

	/// <summary>Gets or sets the ID of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />.</summary>
	/// <returns>The ID of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />. The default is null.</returns>
	public string Id
	{
		get
		{
			return id;
		}
		set
		{
			element = null;
			id = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />.</returns>
	public KeyInfo KeyInfo
	{
		get
		{
			return key;
		}
		set
		{
			element = null;
			key = value;
		}
	}

	/// <summary>Gets or sets a list of objects to be signed.</summary>
	/// <returns>A list of objects to be signed.</returns>
	public IList ObjectList
	{
		get
		{
			return list;
		}
		set
		{
			list = ArrayList.Adapter(value);
		}
	}

	/// <summary>Gets or sets the value of the digital signature.</summary>
	/// <returns>A byte array that contains the value of the digital signature.</returns>
	public byte[] SignatureValue
	{
		get
		{
			return signature;
		}
		set
		{
			element = null;
			signature = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />.</summary>
	/// <returns>The <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> of the current <see cref="T:System.Security.Cryptography.Xml.Signature" />.</returns>
	public SignedInfo SignedInfo
	{
		get
		{
			return info;
		}
		set
		{
			element = null;
			info = value;
		}
	}

	static Signature()
	{
		dsigNsmgr = new XmlNamespaceManager(new NameTable());
		dsigNsmgr.AddNamespace("xd", "http://www.w3.org/2000/09/xmldsig#");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.Signature" /> class.</summary>
	public Signature()
	{
		list = new ArrayList();
	}

	/// <summary>Adds a <see cref="T:System.Security.Cryptography.Xml.DataObject" /> to the list of objects to be signed.</summary>
	/// <param name="dataObject">The <see cref="T:System.Security.Cryptography.Xml.DataObject" /> to be added to the list of objects to be signed. </param>
	public void AddObject(DataObject dataObject)
	{
		list.Add(dataObject);
	}

	/// <summary>Returns the XML representation of the <see cref="T:System.Security.Cryptography.Xml.Signature" />.</summary>
	/// <returns>The XML representation of the <see cref="T:System.Security.Cryptography.Xml.Signature" />.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.Signature.SignedInfo" /> property is null.-or- The <see cref="P:System.Security.Cryptography.Xml.Signature.SignatureValue" /> property is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public XmlElement GetXml()
	{
		return GetXml(null);
	}

	internal XmlElement GetXml(XmlDocument document)
	{
		if (element != null)
		{
			return element;
		}
		if (info == null)
		{
			throw new CryptographicException("SignedInfo");
		}
		if (signature == null)
		{
			throw new CryptographicException("SignatureValue");
		}
		if (document == null)
		{
			document = new XmlDocument();
		}
		XmlElement xmlElement = document.CreateElement("Signature", "http://www.w3.org/2000/09/xmldsig#");
		if (id != null)
		{
			xmlElement.SetAttribute("Id", id);
		}
		XmlNode xml = info.GetXml();
		XmlNode newChild = document.ImportNode(xml, deep: true);
		xmlElement.AppendChild(newChild);
		if (signature != null)
		{
			XmlElement xmlElement2 = document.CreateElement("SignatureValue", "http://www.w3.org/2000/09/xmldsig#");
			xmlElement2.InnerText = Convert.ToBase64String(signature);
			xmlElement.AppendChild(xmlElement2);
		}
		if (key != null)
		{
			xml = key.GetXml();
			newChild = document.ImportNode(xml, deep: true);
			xmlElement.AppendChild(newChild);
		}
		if (list.Count > 0)
		{
			foreach (DataObject item in list)
			{
				xml = item.GetXml();
				newChild = document.ImportNode(xml, deep: true);
				xmlElement.AppendChild(newChild);
			}
		}
		return xmlElement;
	}

	private string GetAttribute(XmlElement xel, string attribute)
	{
		return xel.Attributes[attribute]?.InnerText;
	}

	/// <summary>Loads a <see cref="T:System.Security.Cryptography.Xml.Signature" /> state from an XML element.</summary>
	/// <param name="value">The XML element from which to load the <see cref="T:System.Security.Cryptography.Xml.Signature" /> state. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="value" /> parameter does not contain a valid <see cref="P:System.Security.Cryptography.Xml.Signature.SignatureValue" />.-or- The <paramref name="value" /> parameter does not contain a valid <see cref="P:System.Security.Cryptography.Xml.Signature.SignedInfo" />. </exception>
	public void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.LocalName == "Signature" && value.NamespaceURI == "http://www.w3.org/2000/09/xmldsig#")
		{
			id = GetAttribute(value, "Id");
			int num = NextElementPos(value.ChildNodes, 0, "SignedInfo", "http://www.w3.org/2000/09/xmldsig#", required: true);
			XmlElement value2 = (XmlElement)value.ChildNodes[num];
			info = new SignedInfo();
			info.LoadXml(value2);
			num = NextElementPos(value.ChildNodes, ++num, "SignatureValue", "http://www.w3.org/2000/09/xmldsig#", required: true);
			XmlElement xmlElement = (XmlElement)value.ChildNodes[num];
			signature = Convert.FromBase64String(xmlElement.InnerText);
			num = NextElementPos(value.ChildNodes, ++num, "KeyInfo", "http://www.w3.org/2000/09/xmldsig#", required: false);
			if (num > 0)
			{
				XmlElement value3 = (XmlElement)value.ChildNodes[num];
				key = new KeyInfo();
				key.LoadXml(value3);
			}
			foreach (XmlElement item in value.SelectNodes("xd:Object", dsigNsmgr))
			{
				DataObject dataObject = new DataObject();
				dataObject.LoadXml(item);
				AddObject(dataObject);
			}
			if (info == null)
			{
				throw new CryptographicException("SignedInfo");
			}
			if (signature == null)
			{
				throw new CryptographicException("SignatureValue");
			}
			return;
		}
		throw new CryptographicException("Malformed element: Signature.");
	}

	private int NextElementPos(XmlNodeList nl, int pos, string name, string ns, bool required)
	{
		while (pos < nl.Count)
		{
			if (nl[pos].NodeType == XmlNodeType.Element)
			{
				if (nl[pos].LocalName != name || nl[pos].NamespaceURI != ns)
				{
					if (required)
					{
						throw new CryptographicException("Malformed element " + name);
					}
					return -2;
				}
				return pos;
			}
			pos++;
		}
		if (required)
		{
			throw new CryptographicException("Malformed element " + name);
		}
		return -1;
	}
}
