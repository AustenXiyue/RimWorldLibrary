using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Handles <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> subelements that do not have specific implementations or handlers registered on the machine.</summary>
public class KeyInfoNode : KeyInfoClause
{
	private XmlElement _node;

	/// <summary>Gets or sets the XML content of the current <see cref="T:System.Security.Cryptography.Xml.KeyInfoNode" />.</summary>
	/// <returns>The XML content of the current <see cref="T:System.Security.Cryptography.Xml.KeyInfoNode" />.</returns>
	public XmlElement Value
	{
		get
		{
			return _node;
		}
		set
		{
			_node = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoNode" /> class.</summary>
	public KeyInfoNode()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoNode" /> class with content taken from the specified <see cref="T:System.Xml.XmlElement" />.</summary>
	/// <param name="node">An XML element from which to take the content used to create the new instance of <see cref="T:System.Security.Cryptography.Xml.KeyInfoNode" />. </param>
	public KeyInfoNode(XmlElement node)
	{
		_node = node;
	}

	/// <summary>Returns an XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoNode" />.</summary>
	/// <returns>An XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoNode" />.</returns>
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
		return xmlDocument.ImportNode(_node, deep: true) as XmlElement;
	}

	/// <summary>Parses the input <see cref="T:System.Xml.XmlElement" /> and configures the internal state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoNode" /> to match.</summary>
	/// <param name="value">The <see cref="T:System.Xml.XmlElement" /> that specifies the state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoNode" />. </param>
	public override void LoadXml(XmlElement value)
	{
		_node = value;
	}
}
