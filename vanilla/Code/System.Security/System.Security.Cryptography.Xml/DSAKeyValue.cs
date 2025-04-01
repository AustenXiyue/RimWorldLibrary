using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the <see cref="T:System.Security.Cryptography.DSA" /> private key of the &lt;KeyInfo&gt; element.</summary>
public class DSAKeyValue : KeyInfoClause
{
	private DSA _key;

	private const string KeyValueElementName = "KeyValue";

	private const string DSAKeyValueElementName = "DSAKeyValue";

	private const string PElementName = "P";

	private const string QElementName = "Q";

	private const string GElementName = "G";

	private const string JElementName = "J";

	private const string YElementName = "Y";

	private const string SeedElementName = "Seed";

	private const string PgenCounterElementName = "PgenCounter";

	/// <summary>Gets or sets the key value represented by a <see cref="T:System.Security.Cryptography.DSA" /> object.</summary>
	/// <returns>The public key represented by a <see cref="T:System.Security.Cryptography.DSA" /> object.</returns>
	public DSA Key
	{
		get
		{
			return _key;
		}
		set
		{
			_key = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.DSAKeyValue" /> class with a new, randomly-generated <see cref="T:System.Security.Cryptography.DSA" /> public key.</summary>
	public DSAKeyValue()
	{
		_key = DSA.Create();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.DSAKeyValue" /> class with the specified <see cref="T:System.Security.Cryptography.DSA" /> public key.</summary>
	/// <param name="key">The instance of an implementation of the <see cref="T:System.Security.Cryptography.DSA" /> class that holds the public key. </param>
	public DSAKeyValue(DSA key)
	{
		_key = key;
	}

	/// <summary>Returns the XML representation of a <see cref="T:System.Security.Cryptography.Xml.DSAKeyValue" /> element.</summary>
	/// <returns>The XML representation of the <see cref="T:System.Security.Cryptography.Xml.DSAKeyValue" /> element.</returns>
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
		DSAParameters dSAParameters = _key.ExportParameters(includePrivateParameters: false);
		XmlElement xmlElement = xmlDocument.CreateElement("KeyValue", "http://www.w3.org/2000/09/xmldsig#");
		XmlElement xmlElement2 = xmlDocument.CreateElement("DSAKeyValue", "http://www.w3.org/2000/09/xmldsig#");
		XmlElement xmlElement3 = xmlDocument.CreateElement("P", "http://www.w3.org/2000/09/xmldsig#");
		xmlElement3.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(dSAParameters.P)));
		xmlElement2.AppendChild(xmlElement3);
		XmlElement xmlElement4 = xmlDocument.CreateElement("Q", "http://www.w3.org/2000/09/xmldsig#");
		xmlElement4.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(dSAParameters.Q)));
		xmlElement2.AppendChild(xmlElement4);
		XmlElement xmlElement5 = xmlDocument.CreateElement("G", "http://www.w3.org/2000/09/xmldsig#");
		xmlElement5.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(dSAParameters.G)));
		xmlElement2.AppendChild(xmlElement5);
		XmlElement xmlElement6 = xmlDocument.CreateElement("Y", "http://www.w3.org/2000/09/xmldsig#");
		xmlElement6.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(dSAParameters.Y)));
		xmlElement2.AppendChild(xmlElement6);
		if (dSAParameters.J != null)
		{
			XmlElement xmlElement7 = xmlDocument.CreateElement("J", "http://www.w3.org/2000/09/xmldsig#");
			xmlElement7.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(dSAParameters.J)));
			xmlElement2.AppendChild(xmlElement7);
		}
		if (dSAParameters.Seed != null)
		{
			XmlElement xmlElement8 = xmlDocument.CreateElement("Seed", "http://www.w3.org/2000/09/xmldsig#");
			xmlElement8.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(dSAParameters.Seed)));
			xmlElement2.AppendChild(xmlElement8);
			XmlElement xmlElement9 = xmlDocument.CreateElement("PgenCounter", "http://www.w3.org/2000/09/xmldsig#");
			xmlElement9.AppendChild(xmlDocument.CreateTextNode(Convert.ToBase64String(Utils.ConvertIntToByteArray(dSAParameters.Counter))));
			xmlElement2.AppendChild(xmlElement9);
		}
		xmlElement.AppendChild(xmlElement2);
		return xmlElement;
	}

	/// <summary>Loads a <see cref="T:System.Security.Cryptography.Xml.DSAKeyValue" /> state from an XML element.</summary>
	/// <param name="value">The XML element to load the <see cref="T:System.Security.Cryptography.Xml.DSAKeyValue" /> state from. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="value" /> parameter is not a valid <see cref="T:System.Security.Cryptography.Xml.DSAKeyValue" /> XML element. </exception>
	public override void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.Name != "KeyValue" || value.NamespaceURI != "http://www.w3.org/2000/09/xmldsig#")
		{
			throw new CryptographicException(string.Format("Root element must be {0} element in namepsace {1}", "KeyValue", "http://www.w3.org/2000/09/xmldsig#"));
		}
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(value.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
		XmlNode obj = value.SelectSingleNode(string.Format("{0}:{1}", "dsig", "DSAKeyValue"), xmlNamespaceManager) ?? throw new CryptographicException(string.Format("{0} must contain child element {1}", "KeyValue", "DSAKeyValue"));
		XmlNode xmlNode = obj.SelectSingleNode(string.Format("{0}:{1}", "dsig", "Y"), xmlNamespaceManager);
		if (xmlNode == null)
		{
			throw new CryptographicException(string.Format("{0} is missing", "Y"));
		}
		XmlNode xmlNode2 = obj.SelectSingleNode(string.Format("{0}:{1}", "dsig", "P"), xmlNamespaceManager);
		XmlNode xmlNode3 = obj.SelectSingleNode(string.Format("{0}:{1}", "dsig", "Q"), xmlNamespaceManager);
		if ((xmlNode2 == null && xmlNode3 != null) || (xmlNode2 != null && xmlNode3 == null))
		{
			throw new CryptographicException(string.Format("{0} and {1} can only occour in combination", "P", "Q"));
		}
		XmlNode xmlNode4 = obj.SelectSingleNode(string.Format("{0}:{1}", "dsig", "G"), xmlNamespaceManager);
		XmlNode xmlNode5 = obj.SelectSingleNode(string.Format("{0}:{1}", "dsig", "J"), xmlNamespaceManager);
		XmlNode xmlNode6 = obj.SelectSingleNode(string.Format("{0}:{1}", "dsig", "Seed"), xmlNamespaceManager);
		XmlNode xmlNode7 = obj.SelectSingleNode(string.Format("{0}:{1}", "dsig", "PgenCounter"), xmlNamespaceManager);
		if ((xmlNode6 == null && xmlNode7 != null) || (xmlNode6 != null && xmlNode7 == null))
		{
			throw new CryptographicException(string.Format("{0} and {1} can only occur in combination", "Seed", "PgenCounter"));
		}
		try
		{
			Key.ImportParameters(new DSAParameters
			{
				P = ((xmlNode2 != null) ? Convert.FromBase64String(xmlNode2.InnerText) : null),
				Q = ((xmlNode3 != null) ? Convert.FromBase64String(xmlNode3.InnerText) : null),
				G = ((xmlNode4 != null) ? Convert.FromBase64String(xmlNode4.InnerText) : null),
				Y = Convert.FromBase64String(xmlNode.InnerText),
				J = ((xmlNode5 != null) ? Convert.FromBase64String(xmlNode5.InnerText) : null),
				Seed = ((xmlNode6 != null) ? Convert.FromBase64String(xmlNode6.InnerText) : null),
				Counter = ((xmlNode7 != null) ? Utils.ConvertByteArrayToInt(Convert.FromBase64String(xmlNode7.InnerText)) : 0)
			});
		}
		catch (Exception inner)
		{
			throw new CryptographicException("An error occurred parsing the key components", inner);
		}
	}
}
