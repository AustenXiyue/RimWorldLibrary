using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the &lt;EncryptionProperty&gt; element used in XML encryption. This class cannot be inherited.</summary>
public sealed class EncryptionProperty
{
	private string _target;

	private string _id;

	private XmlElement _elemProp;

	private XmlElement _cachedXml;

	/// <summary>Gets the ID of the current <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> object.</summary>
	/// <returns>The ID of the current <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> object.</returns>
	public string Id => _id;

	/// <summary>Gets the target of the <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> object.</summary>
	/// <returns>The target of the <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> object.</returns>
	public string Target => _target;

	/// <summary>Gets or sets an <see cref="T:System.Xml.XmlElement" /> object that represents an <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> object. </summary>
	/// <returns>An <see cref="T:System.Xml.XmlElement" /> object that represents an <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> object.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Security.Cryptography.Xml.EncryptionProperty.PropertyElement" /> property was set to null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Xml.XmlElement.LocalName" /> property of the value set to the <see cref="P:System.Security.Cryptography.Xml.EncryptionProperty.PropertyElement" /> property is not "EncryptionProperty". -or-The <see cref="P:System.Xml.XmlElement.NamespaceURI" /> property of the value set to the <see cref="P:System.Security.Cryptography.Xml.EncryptionProperty.PropertyElement" /> property is not "http://www.w3.org/2001/04/xmlenc#".</exception>
	public XmlElement PropertyElement
	{
		get
		{
			return _elemProp;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.LocalName != "EncryptionProperty" || value.NamespaceURI != "http://www.w3.org/2001/04/xmlenc#")
			{
				throw new CryptographicException("Malformed encryption property element.");
			}
			_elemProp = value;
			_cachedXml = null;
		}
	}

	private bool CacheValid => _cachedXml != null;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> class. </summary>
	public EncryptionProperty()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> class using an <see cref="T:System.Xml.XmlElement" /> object. </summary>
	/// <param name="elementProperty">An <see cref="T:System.Xml.XmlElement" /> object to use for initialization.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="elementProperty" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Xml.XmlElement.LocalName" /> property of the <paramref name="elementProperty" /> parameter is not "EncryptionProperty". -or-The <see cref="P:System.Xml.XmlElement.NamespaceURI" /> property of the <paramref name="elementProperty" /> parameter is not "http://www.w3.org/2001/04/xmlenc#".</exception>
	public EncryptionProperty(XmlElement elementProperty)
	{
		if (elementProperty == null)
		{
			throw new ArgumentNullException("elementProperty");
		}
		if (elementProperty.LocalName != "EncryptionProperty" || elementProperty.NamespaceURI != "http://www.w3.org/2001/04/xmlenc#")
		{
			throw new CryptographicException("Malformed encryption property element.");
		}
		_elemProp = elementProperty;
		_cachedXml = null;
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlElement" /> object that encapsulates an instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> class.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlElement" /> object that encapsulates an instance of the <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> class.</returns>
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
		return document.ImportNode(_elemProp, deep: true) as XmlElement;
	}

	/// <summary>Parses the input <see cref="T:System.Xml.XmlElement" /> and configures the internal state of the <see cref="T:System.Security.Cryptography.Xml.EncryptionProperty" /> object to match.</summary>
	/// <param name="value">An <see cref="T:System.Xml.XmlElement" /> object to parse.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <see cref="P:System.Xml.XmlElement.LocalName" /> property of the <paramref name="value" /> parameter is not "EncryptionProperty". -or-The <see cref="P:System.Xml.XmlElement.NamespaceURI" /> property of the <paramref name="value" /> parameter is not "http://www.w3.org/2001/04/xmlenc#".</exception>
	public void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.LocalName != "EncryptionProperty" || value.NamespaceURI != "http://www.w3.org/2001/04/xmlenc#")
		{
			throw new CryptographicException("Malformed encryption property element.");
		}
		_cachedXml = value;
		_id = Utils.GetAttribute(value, "Id", "http://www.w3.org/2001/04/xmlenc#");
		_target = Utils.GetAttribute(value, "Target", "http://www.w3.org/2001/04/xmlenc#");
		_elemProp = value;
	}
}
