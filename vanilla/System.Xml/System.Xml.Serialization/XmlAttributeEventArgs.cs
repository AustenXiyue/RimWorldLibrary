using Unity;

namespace System.Xml.Serialization;

/// <summary>Provides data for the <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownAttribute" /> event.</summary>
public class XmlAttributeEventArgs : EventArgs
{
	private object o;

	private XmlAttribute attr;

	private string qnames;

	private int lineNumber;

	private int linePosition;

	/// <summary>Gets the object being deserialized.</summary>
	/// <returns>The object being deserialized.</returns>
	public object ObjectBeingDeserialized => o;

	/// <summary>Gets an object that represents the unknown XML attribute.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlAttribute" /> that represents the unknown XML attribute.</returns>
	public XmlAttribute Attr => attr;

	/// <summary>Gets the line number of the unknown XML attribute.</summary>
	/// <returns>The line number of the unknown XML attribute.</returns>
	public int LineNumber => lineNumber;

	/// <summary>Gets the position in the line of the unknown XML attribute.</summary>
	/// <returns>The position number of the unknown XML attribute.</returns>
	public int LinePosition => linePosition;

	/// <summary>Gets a comma-delimited list of XML attribute names expected to be in an XML document instance.</summary>
	/// <returns>A comma-delimited list of XML attribute names. Each name is in the following format: <paramref name="namespace" />:<paramref name="name" />.</returns>
	public string ExpectedAttributes
	{
		get
		{
			if (qnames != null)
			{
				return qnames;
			}
			return string.Empty;
		}
	}

	internal XmlAttributeEventArgs(XmlAttribute attr, int lineNumber, int linePosition, object o, string qnames)
	{
		this.attr = attr;
		this.o = o;
		this.qnames = qnames;
		this.lineNumber = lineNumber;
		this.linePosition = linePosition;
	}

	internal XmlAttributeEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
