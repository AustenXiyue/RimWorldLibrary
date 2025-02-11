namespace System.Xml;

/// <summary>Specifies the options for processing DTDs. The <see cref="T:System.Xml.DtdProcessing" /> enumeration is used by <see cref="T:System.Xml.XmlReaderSettings" />.</summary>
public enum DtdProcessing
{
	/// <summary>Specifies that when a DTD is encountered, an <see cref="T:System.Xml.XmlException" /> is thrown with a message that states that DTDs are prohibited. This is the default behavior.</summary>
	Prohibit,
	/// <summary>Causes the DOCTYPE element to be ignored. No DTD processing occurs. </summary>
	Ignore,
	/// <summary>Used for parsing DTDs.</summary>
	Parse
}
