using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the abstract base class from which all implementations of <see cref="T:System.Security.Cryptography.Xml.KeyInfo" /> subelements inherit.</summary>
public abstract class KeyInfoClause
{
	/// <summary>Initializes a new instance of <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" />.</summary>
	protected KeyInfoClause()
	{
	}

	/// <summary>When overridden in a derived class, returns an XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" />.</summary>
	/// <returns>An XML representation of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" />.</returns>
	public abstract XmlElement GetXml();

	internal virtual XmlElement GetXml(XmlDocument xmlDocument)
	{
		XmlElement xml = GetXml();
		return (XmlElement)xmlDocument.ImportNode(xml, deep: true);
	}

	/// <summary>When overridden in a derived class, parses the input <see cref="T:System.Xml.XmlElement" /> and configures the internal state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" /> to match.</summary>
	/// <param name="element">The <see cref="T:System.Xml.XmlElement" /> that specifies the state of the <see cref="T:System.Security.Cryptography.Xml.KeyInfoClause" />. </param>
	public abstract void LoadXml(XmlElement element);
}
