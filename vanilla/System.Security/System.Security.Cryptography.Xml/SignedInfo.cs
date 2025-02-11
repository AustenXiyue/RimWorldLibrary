using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Contains information about the canonicalization algorithm and signature algorithm used for the XML signature.</summary>
public class SignedInfo : ICollection, IEnumerable
{
	private ArrayList references;

	private string c14nMethod;

	private string id;

	private string signatureMethod;

	private string signatureLength;

	private XmlElement element;

	/// <summary>Gets or sets the canonicalization algorithm that is used before signing for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The canonicalization algorithm used before signing for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	public string CanonicalizationMethod
	{
		get
		{
			return c14nMethod;
		}
		set
		{
			c14nMethod = value;
			element = null;
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.Cryptography.Xml.Transform" /> object used for canonicalization.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Xml.Transform" /> object used for canonicalization.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">
	///   <see cref="T:System.Security.Cryptography.Xml.Transform" /> is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	[ComVisible(false)]
	public Transform CanonicalizationMethodObject
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the number of references in the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The number of references in the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported. </exception>
	public int Count
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets or sets the ID of the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The ID of the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
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

	/// <summary>Gets a value that indicates whether the collection is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise, false.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported. </exception>
	public bool IsReadOnly
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets a value that indicates whether the collection is synchronized.</summary>
	/// <returns>true if the collection is synchronized; otherwise, false.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported. </exception>
	public bool IsSynchronized
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Gets a list of the <see cref="T:System.Security.Cryptography.Xml.Reference" /> objects of the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>A list of the <see cref="T:System.Security.Cryptography.Xml.Reference" /> elements of the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	public ArrayList References => references;

	/// <summary>Gets or sets the length of the signature for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The length of the signature for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	public string SignatureLength
	{
		get
		{
			return signatureLength;
		}
		set
		{
			element = null;
			signatureLength = value;
		}
	}

	/// <summary>Gets or sets the name of the algorithm used for signature generation and validation for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The name of the algorithm used for signature generation and validation for the current <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</returns>
	public string SignatureMethod
	{
		get
		{
			return signatureMethod;
		}
		set
		{
			element = null;
			signatureMethod = value;
		}
	}

	/// <summary>Gets an object to use for synchronization.</summary>
	/// <returns>An object to use for synchronization.</returns>
	/// <exception cref="T:System.NotSupportedException">This property is not supported. </exception>
	public object SyncRoot
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> class.</summary>
	public SignedInfo()
	{
		references = new ArrayList();
		c14nMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
	}

	/// <summary>Adds a <see cref="T:System.Security.Cryptography.Xml.Reference" /> object to the list of references to digest and sign.</summary>
	/// <param name="reference">The reference to add to the list of references. </param>
	/// <exception cref="T:System.ArgumentNullException">The reference parameter is null.</exception>
	public void AddReference(Reference reference)
	{
		references.Add(reference);
	}

	/// <summary>Copies the elements of this instance into an <see cref="T:System.Array" /> object, starting at a specified index in the array.</summary>
	/// <param name="array">An <see cref="T:System.Array" /> object that holds the collection's elements. </param>
	/// <param name="index">The beginning index in the array where the elements are copied. </param>
	/// <exception cref="T:System.NotSupportedException">This method is not supported. </exception>
	public void CopyTo(Array array, int index)
	{
		throw new NotSupportedException();
	}

	/// <summary>Returns an enumerator that iterates through the collection of references.</summary>
	/// <returns>An enumerator that iterates through the collection of references.</returns>
	/// <exception cref="T:System.NotSupportedException">This method is not supported. </exception>
	public IEnumerator GetEnumerator()
	{
		return references.GetEnumerator();
	}

	/// <summary>Returns the XML representation of the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> object.</summary>
	/// <returns>The XML representation of the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> instance.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Security.Cryptography.Xml.SignedInfo.SignatureMethod" /> property is null.-or- The <see cref="P:System.Security.Cryptography.Xml.SignedInfo.References" /> property is empty. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public XmlElement GetXml()
	{
		if (element != null)
		{
			return element;
		}
		if (signatureMethod == null)
		{
			throw new CryptographicException("SignatureMethod");
		}
		if (references.Count == 0)
		{
			throw new CryptographicException("References empty");
		}
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("SignedInfo", "http://www.w3.org/2000/09/xmldsig#");
		if (id != null)
		{
			xmlElement.SetAttribute("Id", id);
		}
		if (c14nMethod != null)
		{
			XmlElement xmlElement2 = xmlDocument.CreateElement("CanonicalizationMethod", "http://www.w3.org/2000/09/xmldsig#");
			xmlElement2.SetAttribute("Algorithm", c14nMethod);
			xmlElement.AppendChild(xmlElement2);
		}
		if (signatureMethod != null)
		{
			XmlElement xmlElement3 = xmlDocument.CreateElement("SignatureMethod", "http://www.w3.org/2000/09/xmldsig#");
			xmlElement3.SetAttribute("Algorithm", signatureMethod);
			if (signatureLength != null)
			{
				XmlElement xmlElement4 = xmlDocument.CreateElement("HMACOutputLength", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement4.InnerText = signatureLength;
				xmlElement3.AppendChild(xmlElement4);
			}
			xmlElement.AppendChild(xmlElement3);
		}
		if (references.Count == 0)
		{
			throw new CryptographicException("At least one Reference element is required in SignedInfo.");
		}
		foreach (Reference reference in references)
		{
			XmlNode xml = reference.GetXml();
			XmlNode newChild = xmlDocument.ImportNode(xml, deep: true);
			xmlElement.AppendChild(newChild);
		}
		return xmlElement;
	}

	private string GetAttribute(XmlElement xel, string attribute)
	{
		return xel.Attributes[attribute]?.InnerText;
	}

	/// <summary>Loads a <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> state from an XML element.</summary>
	/// <param name="value">The XML element from which to load the <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> state. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="value" /> parameter is not a valid <see cref="T:System.Security.Cryptography.Xml.SignedInfo" /> element.-or- The <paramref name="value" /> parameter does not contain a valid <see cref="P:System.Security.Cryptography.Xml.SignedInfo.CanonicalizationMethod" /> property.-or- The <paramref name="value" /> parameter does not contain a valid <see cref="P:System.Security.Cryptography.Xml.SignedInfo.SignatureMethod" /> property.</exception>
	public void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.LocalName != "SignedInfo" || value.NamespaceURI != "http://www.w3.org/2000/09/xmldsig#")
		{
			throw new CryptographicException();
		}
		id = GetAttribute(value, "Id");
		c14nMethod = XmlSignature.GetAttributeFromElement(value, "Algorithm", "CanonicalizationMethod");
		XmlElement childElement = XmlSignature.GetChildElement(value, "SignatureMethod", "http://www.w3.org/2000/09/xmldsig#");
		if (childElement != null)
		{
			signatureMethod = childElement.GetAttribute("Algorithm");
			XmlElement childElement2 = XmlSignature.GetChildElement(childElement, "HMACOutputLength", "http://www.w3.org/2000/09/xmldsig#");
			if (childElement2 != null)
			{
				signatureLength = childElement2.InnerText;
			}
		}
		for (int i = 0; i < value.ChildNodes.Count; i++)
		{
			XmlNode xmlNode = value.ChildNodes[i];
			if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.LocalName == "Reference" && xmlNode.NamespaceURI == "http://www.w3.org/2000/09/xmldsig#")
			{
				Reference reference = new Reference();
				reference.LoadXml((XmlElement)xmlNode);
				AddReference(reference);
			}
		}
		element = value;
	}
}
