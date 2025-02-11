using System.Collections;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents an XML digital signature or XML encryption &lt;KeyInfo&gt; element.</summary>
public class KeyInfo : IEnumerable
{
	private string _id;

	private ArrayList _keyInfoClauses;

	/// <summary>Gets or sets the key information identity.</summary>
	/// <returns>The key information identity.</returns>
	public string Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	/// <summary>Gets the number of <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" /> objects contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object.</summary>
	/// <returns>The number of <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" /> objects contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object.</returns>
	public int Count => _keyInfoClauses.Count;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> class.</summary>
	public KeyInfo()
	{
		_keyInfoClauses = new ArrayList();
	}

	/// <summary>Returns the XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object.</summary>
	/// <returns>The XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public XmlElement GetXml()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		return GetXml(xmlDocument);
	}

	internal XmlElement GetXml(XmlDocument xmlDocument)
	{
		XmlElement xmlElement = xmlDocument.CreateElement("KeyInfo", "http://www.w3.org/2000/09/xmldsig#");
		if (!string.IsNullOrEmpty(_id))
		{
			xmlElement.SetAttribute("Id", _id);
		}
		for (int i = 0; i < _keyInfoClauses.Count; i++)
		{
			XmlElement xml = ((KeyInfoClause)_keyInfoClauses[i]).GetXml(xmlDocument);
			if (xml != null)
			{
				xmlElement.AppendChild(xml);
			}
		}
		return xmlElement;
	}

	/// <summary>Loads a <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> state from an XML element.</summary>
	/// <param name="value">The XML element from which to load the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> state. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	public void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		_id = Utils.GetAttribute(value, "Id", "http://www.w3.org/2000/09/xmldsig#");
		for (XmlNode xmlNode = value.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode is XmlElement xmlElement)
			{
				string text = xmlElement.NamespaceURI + " " + xmlElement.LocalName;
				if (text == "http://www.w3.org/2000/09/xmldsig# KeyValue")
				{
					foreach (XmlNode childNode in xmlElement.ChildNodes)
					{
						if (childNode is XmlElement xmlElement2)
						{
							text = text + "/" + xmlElement2.LocalName;
							break;
						}
					}
				}
				KeyInfoClause keyInfoClause = (KeyInfoClause)CryptoHelpers.CreateFromName(text);
				if (keyInfoClause == null)
				{
					keyInfoClause = new KeyInfoNode();
				}
				keyInfoClause.LoadXml(xmlElement);
				AddClause(keyInfoClause);
			}
		}
	}

	/// <summary>Adds a <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" /> that represents a particular type of <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> information to the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object.</summary>
	/// <param name="clause">The <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" /> to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object. </param>
	public void AddClause(KeyInfoClause clause)
	{
		_keyInfoClauses.Add(clause);
	}

	/// <summary>Returns an enumerator of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" /> objects in the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object.</summary>
	/// <returns>An enumerator of the subelements of <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> that can be used to iterate through the collection.</returns>
	public IEnumerator GetEnumerator()
	{
		return _keyInfoClauses.GetEnumerator();
	}

	/// <summary>Returns an enumerator of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" /> objects of the specified type in the <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> object.</summary>
	/// <returns>An enumerator of the subelements of <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> that can be used to iterate through the collection.</returns>
	/// <param name="requestedObjectType">The type of object to enumerate. </param>
	public IEnumerator GetEnumerator(Type requestedObjectType)
	{
		ArrayList arrayList = new ArrayList();
		IEnumerator enumerator = _keyInfoClauses.GetEnumerator();
		while (enumerator.MoveNext())
		{
			object current = enumerator.Current;
			if (requestedObjectType.Equals(current.GetType()))
			{
				arrayList.Add(current);
			}
		}
		return arrayList.GetEnumerator();
	}
}
