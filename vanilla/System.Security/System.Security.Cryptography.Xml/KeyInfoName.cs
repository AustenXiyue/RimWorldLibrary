using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents a &lt;KeyName&gt; subelement of an XMLDSIG or XML Encryption &lt;KeyInfo&gt; element.</summary>
public class KeyInfoName : KeyInfoClause
{
	private string _keyName;

	/// <summary>Gets or sets the string identifier contained within a &lt;KeyName&gt; element.</summary>
	/// <returns>The string identifier that is the value of the &lt;KeyName&gt; element.</returns>
	public string Value
	{
		get
		{
			return _keyName;
		}
		set
		{
			_keyName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoName" /> class.</summary>
	public KeyInfoName()
		: this(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoName" /> class by specifying the string identifier that is the value of the &lt;KeyName&gt; element.</summary>
	/// <param name="keyName">The string identifier that is the value of the &lt;KeyName&gt; element.</param>
	public KeyInfoName(string keyName)
	{
		Value = keyName;
	}

	/// <summary>Returns an XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoName" /> object.</summary>
	/// <returns>An XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoName" /> object.</returns>
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
		XmlElement xmlElement = xmlDocument.CreateElement("KeyName", "http://www.w3.org/2000/09/xmldsig#");
		xmlElement.AppendChild(xmlDocument.CreateTextNode(_keyName));
		return xmlElement;
	}

	/// <summary>Parses the input <see cref="T:System.Xml.XmlElement" /> object and configures the internal state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoName" /> object to match.</summary>
	/// <param name="value">The <see cref="T:System.Xml.XmlElement" /> object that specifies the state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoName" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null. </exception>
	public override void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		_keyName = value.InnerText.Trim();
	}
}
