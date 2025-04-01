using Unity;

namespace System.Xml.Serialization;

/// <summary>Provides data for the <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownElement" /> event.</summary>
public class XmlElementEventArgs : EventArgs
{
	private object o;

	private XmlElement elem;

	private string qnames;

	private int lineNumber;

	private int linePosition;

	/// <summary>Gets the object the <see cref="T:System.Xml.Serialization.XmlSerializer" /> is deserializing.</summary>
	/// <returns>The object that is being deserialized by the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</returns>
	public object ObjectBeingDeserialized => o;

	/// <summary>Gets the object that represents the unknown XML element.</summary>
	/// <returns>The object that represents the unknown XML element.</returns>
	public XmlElement Element => elem;

	/// <summary>Gets the line number where the unknown element was encountered if the XML reader is an <see cref="T:System.Xml.XmlTextReader" />.</summary>
	/// <returns>The line number where the unknown element was encountered if the XML reader is an <see cref="T:System.Xml.XmlTextReader" />; otherwise, -1.</returns>
	public int LineNumber => lineNumber;

	/// <summary>Gets the place in the line where the unknown element occurs if the XML reader is an <see cref="T:System.Xml.XmlTextReader" />.</summary>
	/// <returns>The number in the line where the unknown element occurs if the XML reader is an <see cref="T:System.Xml.XmlTextReader" />; otherwise, -1.</returns>
	public int LinePosition => linePosition;

	/// <summary>Gets a comma-delimited list of XML element names expected to be in an XML document instance.</summary>
	/// <returns>A comma-delimited list of XML element names. Each name is in the following format: <paramref name="namespace" />:<paramref name="name" />.</returns>
	public string ExpectedElements
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

	internal XmlElementEventArgs(XmlElement elem, int lineNumber, int linePosition, object o, string qnames)
	{
		this.elem = elem;
		this.o = o;
		this.qnames = qnames;
		this.lineNumber = lineNumber;
		this.linePosition = linePosition;
	}

	internal XmlElementEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
