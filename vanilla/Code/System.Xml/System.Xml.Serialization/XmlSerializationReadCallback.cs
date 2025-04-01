namespace System.Xml.Serialization;

/// <summary>Delegate used by the <see cref="T:System.Xml.Serialization.XmlSerializer" /> class for deserialization of types from SOAP-encoded, non-root XML data. </summary>
/// <returns>The object returned by the callback.</returns>
public delegate object XmlSerializationReadCallback();
