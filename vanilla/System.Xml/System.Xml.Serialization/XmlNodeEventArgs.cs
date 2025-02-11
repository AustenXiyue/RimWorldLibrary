using Unity;

namespace System.Xml.Serialization;

/// <summary>Provides data for the <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownNode" /> event.</summary>
public class XmlNodeEventArgs : EventArgs
{
	private object o;

	private XmlNode xmlNode;

	private int lineNumber;

	private int linePosition;

	/// <summary>Gets the object being deserialized.</summary>
	/// <returns>The <see cref="T:System.Object" /> being deserialized.</returns>
	public object ObjectBeingDeserialized => o;

	/// <summary>Gets the type of the XML node being deserialized.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNodeType" /> that represents the XML node being deserialized.</returns>
	public XmlNodeType NodeType => xmlNode.NodeType;

	/// <summary>Gets the name of the XML node being deserialized.</summary>
	/// <returns>The name of the node being deserialized.</returns>
	public string Name => xmlNode.Name;

	/// <summary>Gets the XML local name of the XML node being deserialized.</summary>
	/// <returns>The XML local name of the node being deserialized.</returns>
	public string LocalName => xmlNode.LocalName;

	/// <summary>Gets the namespace URI that is associated with the XML node being deserialized.</summary>
	/// <returns>The namespace URI that is associated with the XML node being deserialized.</returns>
	public string NamespaceURI => xmlNode.NamespaceURI;

	/// <summary>Gets the text of the XML node being deserialized.</summary>
	/// <returns>The text of the XML node being deserialized.</returns>
	public string Text => xmlNode.Value;

	/// <summary>Gets the line number of the unknown XML node.</summary>
	/// <returns>The line number of the unknown XML node.</returns>
	public int LineNumber => lineNumber;

	/// <summary>Gets the position in the line of the unknown XML node.</summary>
	/// <returns>The position number of the unknown XML node.</returns>
	public int LinePosition => linePosition;

	internal XmlNodeEventArgs(XmlNode xmlNode, int lineNumber, int linePosition, object o)
	{
		this.o = o;
		this.xmlNode = xmlNode;
		this.lineNumber = lineNumber;
		this.linePosition = linePosition;
	}

	internal XmlNodeEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
