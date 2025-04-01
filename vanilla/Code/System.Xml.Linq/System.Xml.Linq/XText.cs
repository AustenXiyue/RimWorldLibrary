using System.Text;

namespace System.Xml.Linq;

/// <summary>Represents a text node. </summary>
/// <filterpriority>2</filterpriority>
public class XText : XNode
{
	internal string text;

	/// <summary>Gets the node type for this node.</summary>
	/// <returns>The node type. For <see cref="T:System.Xml.Linq.XText" /> objects, this value is <see cref="F:System.Xml.XmlNodeType.Text" />.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Text;

	/// <summary>Gets or sets the value of this node.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the value of this node.</returns>
	public string Value
	{
		get
		{
			return text;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Value);
			text = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XText" /> class. </summary>
	/// <param name="value">The <see cref="T:System.String" /> that contains the value of the <see cref="T:System.Xml.Linq.XText" /> node.</param>
	public XText(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		text = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XText" /> class from another <see cref="T:System.Xml.Linq.XText" /> object.</summary>
	/// <param name="other">The <see cref="T:System.Xml.Linq.XText" /> node to copy from.</param>
	public XText(XText other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		text = other.text;
	}

	internal XText(XmlReader r)
	{
		text = r.Value;
		r.Read();
	}

	/// <summary>Writes this node to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">An <see cref="T:System.Xml.XmlWriter" /> into which this method will write.</param>
	/// <filterpriority>2</filterpriority>
	public override void WriteTo(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (parent is XDocument)
		{
			writer.WriteWhitespace(text);
		}
		else
		{
			writer.WriteString(text);
		}
	}

	internal override void AppendText(StringBuilder sb)
	{
		sb.Append(text);
	}

	internal override XNode CloneNode()
	{
		return new XText(this);
	}

	internal override bool DeepEquals(XNode node)
	{
		if (node != null && NodeType == node.NodeType)
		{
			return text == ((XText)node).text;
		}
		return false;
	}

	internal override int GetDeepHashCode()
	{
		return text.GetHashCode();
	}
}
