namespace System.Xml;

/// <summary>delegate for a callback method when closing the reader.</summary>
/// <param name="reader">The <see cref="T:System.Xml.XmlDictionaryReader" /> that fires the OnClose event.</param>
/// <filterpriority>2</filterpriority>
public delegate void OnXmlDictionaryReaderClose(XmlDictionaryReader reader);
