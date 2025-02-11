namespace System.Xml.Linq;

/// <summary>Represents an XML comment. </summary>
public class XComment : XNode
{
	internal string value;

	/// <summary>Gets the node type for this node.</summary>
	/// <returns>The node type. For <see cref="T:System.Xml.Linq.XComment" /> objects, this value is <see cref="F:System.Xml.XmlNodeType.Comment" />.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Comment;

	/// <summary>Gets or sets the string value of this comment.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the string value of this comment.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> is null.</exception>
	public string Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Value);
			this.value = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XComment" /> class with the specified string content. </summary>
	/// <param name="value">A string that contains the contents of the new <see cref="T:System.Xml.Linq.XComment" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> parameter is null.</exception>
	public XComment(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XComment" /> class from an existing comment node. </summary>
	/// <param name="other">The <see cref="T:System.Xml.Linq.XComment" /> node to copy from.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="other" /> parameter is null.</exception>
	public XComment(XComment other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		value = other.value;
	}

	internal XComment(XmlReader r)
	{
		value = r.Value;
		r.Read();
	}

	/// <summary>Write this comment to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">An <see cref="T:System.Xml.XmlWriter" /> into which this method will write.</param>
	/// <filterpriority>2</filterpriority>
	public override void WriteTo(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteComment(value);
	}

	internal override XNode CloneNode()
	{
		return new XComment(this);
	}

	internal override bool DeepEquals(XNode node)
	{
		if (node is XComment xComment)
		{
			return value == xComment.value;
		}
		return false;
	}

	internal override int GetDeepHashCode()
	{
		return value.GetHashCode();
	}
}
