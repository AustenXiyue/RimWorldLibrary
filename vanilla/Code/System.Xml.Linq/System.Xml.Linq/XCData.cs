namespace System.Xml.Linq;

/// <summary>Represents a text node that contains CDATA. </summary>
public class XCData : XText
{
	/// <summary>Gets the node type for this node.</summary>
	/// <returns>The node type. For <see cref="T:System.Xml.Linq.XCData" /> objects, this value is <see cref="F:System.Xml.XmlNodeType.CDATA" />.</returns>
	public override XmlNodeType NodeType => XmlNodeType.CDATA;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XCData" /> class. </summary>
	/// <param name="value">A string that contains the value of the <see cref="T:System.Xml.Linq.XCData" /> node.</param>
	public XCData(string value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XCData" /> class. </summary>
	/// <param name="other">The <see cref="T:System.Xml.Linq.XCData" /> node to copy from.</param>
	public XCData(XCData other)
		: base(other)
	{
	}

	internal XCData(XmlReader r)
		: base(r)
	{
	}

	/// <summary>Writes this CDATA object to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">An <see cref="T:System.Xml.XmlWriter" /> into which this method will write.</param>
	/// <filterpriority>2</filterpriority>
	public override void WriteTo(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteCData(text);
	}

	internal override XNode CloneNode()
	{
		return new XCData(this);
	}
}
