using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Mono.Security.X509;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents an &lt;X509Data&gt; subelement of an XMLDSIG or XML Encryption &lt;KeyInfo&gt; element.</summary>
public class KeyInfoX509Data : KeyInfoClause
{
	private byte[] x509crl;

	private ArrayList IssuerSerialList;

	private ArrayList SubjectKeyIdList;

	private ArrayList SubjectNameList;

	private ArrayList X509CertificateList;

	/// <summary>Gets a list of the X.509v3 certificates contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <returns>A list of the X.509 certificates contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</returns>
	public ArrayList Certificates => X509CertificateList;

	/// <summary>Gets or sets the Certificate Revocation List (CRL) contained within the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <returns>The Certificate Revocation List (CRL) contained within the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</returns>
	public byte[] CRL
	{
		get
		{
			return x509crl;
		}
		set
		{
			x509crl = value;
		}
	}

	/// <summary>Gets a list of <see cref="T:System.Security.Cryptography.Xml.X509IssuerSerial" /> structures that represent an issuer name and serial number pair.</summary>
	/// <returns>A list of <see cref="T:System.Security.Cryptography.Xml.X509IssuerSerial" /> structures that represent an issuer name and serial number pair.</returns>
	public ArrayList IssuerSerials => IssuerSerialList;

	/// <summary>Gets a list of the subject key identifiers (SKIs) contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <returns>A list of the subject key identifiers (SKIs) contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</returns>
	public ArrayList SubjectKeyIds => SubjectKeyIdList;

	/// <summary>Gets a list of the subject names of the entities contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <returns>A list of the subject names of the entities contained in the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</returns>
	public ArrayList SubjectNames => SubjectNameList;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> class.</summary>
	public KeyInfoX509Data()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> class from the specified ASN.1 DER encoding of an X.509v3 certificate.</summary>
	/// <param name="rgbCert">The ASN.1 DER encoding of an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> from.</param>
	public KeyInfoX509Data(byte[] rgbCert)
	{
		AddCertificate(new System.Security.Cryptography.X509Certificates.X509Certificate(rgbCert));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> class from the specified X.509v3 certificate.</summary>
	/// <param name="cert">The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> from.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="cert" /> parameter is null.</exception>
	public KeyInfoX509Data(System.Security.Cryptography.X509Certificates.X509Certificate cert)
	{
		AddCertificate(cert);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> class from the specified X.509v3 certificate.</summary>
	/// <param name="cert">The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> from.</param>
	/// <param name="includeOption">One of the <see cref="T:System.Security.Cryptography.X509Certificates.X509IncludeOption" /> values that specifies how much of the certificate chain to include.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="cert" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate has only a partial certificate chain.</exception>
	public KeyInfoX509Data(System.Security.Cryptography.X509Certificates.X509Certificate cert, X509IncludeOption includeOption)
	{
		if (cert == null)
		{
			throw new ArgumentNullException("cert");
		}
		switch (includeOption)
		{
		case X509IncludeOption.None:
		case X509IncludeOption.EndCertOnly:
			AddCertificate(cert);
			break;
		case X509IncludeOption.ExcludeRoot:
			AddCertificatesChainFrom(cert, root: false);
			break;
		case X509IncludeOption.WholeChain:
			AddCertificatesChainFrom(cert, root: true);
			break;
		}
	}

	private void AddCertificatesChainFrom(System.Security.Cryptography.X509Certificates.X509Certificate cert, bool root)
	{
		System.Security.Cryptography.X509Certificates.X509Chain x509Chain = new System.Security.Cryptography.X509Certificates.X509Chain();
		x509Chain.Build(new X509Certificate2(cert));
		X509ChainElementEnumerator enumerator = x509Chain.ChainElements.GetEnumerator();
		while (enumerator.MoveNext())
		{
			byte[] array = enumerator.Current.Certificate.RawData;
			if (!root && new Mono.Security.X509.X509Certificate(array).IsSelfSigned)
			{
				array = null;
			}
			if (array != null)
			{
				AddCertificate(new System.Security.Cryptography.X509Certificates.X509Certificate(array));
			}
		}
	}

	/// <summary>Adds the specified X.509v3 certificate to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" />.</summary>
	/// <param name="certificate">The <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="certificate" /> parameter is null.</exception>
	public void AddCertificate(System.Security.Cryptography.X509Certificates.X509Certificate certificate)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("certificate");
		}
		if (X509CertificateList == null)
		{
			X509CertificateList = new ArrayList();
		}
		X509CertificateList.Add(certificate);
	}

	/// <summary>Adds the specified issuer name and serial number pair to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <param name="issuerName">The issuer name portion of the pair to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	/// <param name="serialNumber">The serial number portion of the pair to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	public void AddIssuerSerial(string issuerName, string serialNumber)
	{
		if (issuerName == null)
		{
			throw new ArgumentException("issuerName");
		}
		if (IssuerSerialList == null)
		{
			IssuerSerialList = new ArrayList();
		}
		X509IssuerSerial x509IssuerSerial = new X509IssuerSerial(issuerName, serialNumber);
		IssuerSerialList.Add(x509IssuerSerial);
	}

	/// <summary>Adds the specified subject key identifier (SKI) byte array to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <param name="subjectKeyId">A byte array that represents the subject key identifier (SKI) to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	public void AddSubjectKeyId(byte[] subjectKeyId)
	{
		if (SubjectKeyIdList == null)
		{
			SubjectKeyIdList = new ArrayList();
		}
		SubjectKeyIdList.Add(subjectKeyId);
	}

	/// <summary>Adds the specified subject key identifier (SKI) string to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <param name="subjectKeyId">A string that represents the subject key identifier (SKI) to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</param>
	[ComVisible(false)]
	public void AddSubjectKeyId(string subjectKeyId)
	{
		if (SubjectKeyIdList == null)
		{
			SubjectKeyIdList = new ArrayList();
		}
		byte[] value = null;
		if (subjectKeyId != null)
		{
			value = Convert.FromBase64String(subjectKeyId);
		}
		SubjectKeyIdList.Add(value);
	}

	/// <summary>Adds the subject name of the entity that was issued an X.509v3 certificate to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <param name="subjectName">The name of the entity that was issued an X.509 certificate to add to the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	public void AddSubjectName(string subjectName)
	{
		if (SubjectNameList == null)
		{
			SubjectNameList = new ArrayList();
		}
		SubjectNameList.Add(subjectName);
	}

	/// <summary>Returns an XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</summary>
	/// <returns>An XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override XmlElement GetXml()
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement("X509Data", "http://www.w3.org/2000/09/xmldsig#");
		xmlElement.SetAttribute("xmlns", "http://www.w3.org/2000/09/xmldsig#");
		if (IssuerSerialList != null && IssuerSerialList.Count > 0)
		{
			foreach (X509IssuerSerial issuerSerial in IssuerSerialList)
			{
				XmlElement xmlElement2 = xmlDocument.CreateElement("X509IssuerSerial", "http://www.w3.org/2000/09/xmldsig#");
				XmlElement xmlElement3 = xmlDocument.CreateElement("X509IssuerName", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement3.InnerText = issuerSerial.IssuerName;
				xmlElement2.AppendChild(xmlElement3);
				XmlElement xmlElement4 = xmlDocument.CreateElement("X509SerialNumber", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement4.InnerText = issuerSerial.SerialNumber;
				xmlElement2.AppendChild(xmlElement4);
				xmlElement.AppendChild(xmlElement2);
			}
		}
		if (SubjectKeyIdList != null && SubjectKeyIdList.Count > 0)
		{
			foreach (byte[] subjectKeyId in SubjectKeyIdList)
			{
				XmlElement xmlElement5 = xmlDocument.CreateElement("X509SKI", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement5.InnerText = Convert.ToBase64String(subjectKeyId);
				xmlElement.AppendChild(xmlElement5);
			}
		}
		if (SubjectNameList != null && SubjectNameList.Count > 0)
		{
			foreach (string subjectName in SubjectNameList)
			{
				XmlElement xmlElement6 = xmlDocument.CreateElement("X509SubjectName", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement6.InnerText = subjectName;
				xmlElement.AppendChild(xmlElement6);
			}
		}
		if (X509CertificateList != null && X509CertificateList.Count > 0)
		{
			foreach (System.Security.Cryptography.X509Certificates.X509Certificate x509Certificate in X509CertificateList)
			{
				XmlElement xmlElement7 = xmlDocument.CreateElement("X509Certificate", "http://www.w3.org/2000/09/xmldsig#");
				xmlElement7.InnerText = Convert.ToBase64String(x509Certificate.GetRawCertData());
				xmlElement.AppendChild(xmlElement7);
			}
		}
		if (x509crl != null)
		{
			XmlElement xmlElement8 = xmlDocument.CreateElement("X509CRL", "http://www.w3.org/2000/09/xmldsig#");
			xmlElement8.InnerText = Convert.ToBase64String(x509crl);
			xmlElement.AppendChild(xmlElement8);
		}
		return xmlElement;
	}

	/// <summary>Parses the input <see cref="T:System.Xml.XmlElement" /> object and configures the internal state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object to match.</summary>
	/// <param name="element">The <see cref="T:System.Xml.XmlElement" /> object that specifies the state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoX509Data" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="element" /> parameter does not contain an &lt;X509IssuerName&gt; node.-or-The <paramref name="element" /> parameter does not contain an &lt;X509SerialNumber&gt; node.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public override void LoadXml(XmlElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (IssuerSerialList != null)
		{
			IssuerSerialList.Clear();
		}
		if (SubjectKeyIdList != null)
		{
			SubjectKeyIdList.Clear();
		}
		if (SubjectNameList != null)
		{
			SubjectNameList.Clear();
		}
		if (X509CertificateList != null)
		{
			X509CertificateList.Clear();
		}
		x509crl = null;
		if (element.LocalName != "X509Data" || element.NamespaceURI != "http://www.w3.org/2000/09/xmldsig#")
		{
			throw new CryptographicException("element");
		}
		XmlElement[] array = null;
		array = XmlSignature.GetChildElements(element, "X509IssuerSerial");
		if (array != null)
		{
			foreach (XmlElement xel in array)
			{
				XmlElement childElement = XmlSignature.GetChildElement(xel, "X509IssuerName", "http://www.w3.org/2000/09/xmldsig#");
				XmlElement childElement2 = XmlSignature.GetChildElement(xel, "X509SerialNumber", "http://www.w3.org/2000/09/xmldsig#");
				AddIssuerSerial(childElement.InnerText, childElement2.InnerText);
			}
		}
		array = XmlSignature.GetChildElements(element, "X509SKI");
		if (array != null)
		{
			for (int j = 0; j < array.Length; j++)
			{
				byte[] subjectKeyId = Convert.FromBase64String(array[j].InnerXml);
				AddSubjectKeyId(subjectKeyId);
			}
		}
		array = XmlSignature.GetChildElements(element, "X509SubjectName");
		if (array != null)
		{
			for (int k = 0; k < array.Length; k++)
			{
				AddSubjectName(array[k].InnerXml);
			}
		}
		array = XmlSignature.GetChildElements(element, "X509Certificate");
		if (array != null)
		{
			for (int l = 0; l < array.Length; l++)
			{
				byte[] data = Convert.FromBase64String(array[l].InnerXml);
				AddCertificate(new System.Security.Cryptography.X509Certificates.X509Certificate(data));
			}
		}
		XmlElement childElement3 = XmlSignature.GetChildElement(element, "X509CRL", "http://www.w3.org/2000/09/xmldsig#");
		if (childElement3 != null)
		{
			x509crl = Convert.FromBase64String(childElement3.InnerXml);
		}
	}
}
