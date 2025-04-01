namespace System.Xml.Linq;

/// <summary>Represents an XML processing instruction. </summary>
public class XProcessingInstruction : XNode
{
	internal string target;

	internal string data;

	/// <summary>Gets or sets the string value of this processing instruction.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the string value of this processing instruction.</returns>
	/// <exception cref="T:System.ArgumentNullException">The string <paramref name="value" /> is null.</exception>
	public string Data
	{
		get
		{
			return data;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Value);
			data = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
		}
	}

	/// <summary>Gets the node type for this node.</summary>
	/// <returns>The node type. For <see cref="T:System.Xml.Linq.XProcessingInstruction" /> objects, this value is <see cref="F:System.Xml.XmlNodeType.ProcessingInstruction" />.</returns>
	public override XmlNodeType NodeType => XmlNodeType.ProcessingInstruction;

	/// <summary>Gets or sets a string containing the target application for this processing instruction.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the target application for this processing instruction.</returns>
	/// <exception cref="T:System.ArgumentNullException">The string <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> does not follow the constraints of an XML name.</exception>
	public string Target
	{
		get
		{
			return target;
		}
		set
		{
			ValidateName(value);
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Name);
			target = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Name);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XProcessingInstruction" /> class. </summary>
	/// <param name="target">A <see cref="T:System.String" /> containing the target application for this <see cref="T:System.Xml.Linq.XProcessingInstruction" />.</param>
	/// <param name="data">The string data for this <see cref="T:System.Xml.Linq.XProcessingInstruction" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> or <paramref name="data" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="target" /> does not follow the constraints of an XML name.</exception>
	public XProcessingInstruction(string target, string data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		ValidateName(target);
		this.target = target;
		this.data = data;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XProcessingInstruction" /> class. </summary>
	/// <param name="other">The <see cref="T:System.Xml.Linq.XProcessingInstruction" /> node to copy from.</param>
	public XProcessingInstruction(XProcessingInstruction other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		target = other.target;
		data = other.data;
	}

	internal XProcessingInstruction(XmlReader r)
	{
		target = r.Name;
		data = r.Value;
		r.Read();
	}

	/// <summary>Writes this processing instruction to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> to write this processing instruction to.</param>
	/// <filterpriority>2</filterpriority>
	public override void WriteTo(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteProcessingInstruction(target, data);
	}

	internal override XNode CloneNode()
	{
		return new XProcessingInstruction(this);
	}

	internal override bool DeepEquals(XNode node)
	{
		if (node is XProcessingInstruction xProcessingInstruction && target == xProcessingInstruction.target)
		{
			return data == xProcessingInstruction.data;
		}
		return false;
	}

	internal override int GetDeepHashCode()
	{
		return target.GetHashCode() ^ data.GetHashCode();
	}

	private static void ValidateName(string name)
	{
		XmlConvert.VerifyNCName(name);
		if (string.Compare(name, "xml", StringComparison.OrdinalIgnoreCase) == 0)
		{
			throw new ArgumentException(Res.GetString("Argument_InvalidPIName", name));
		}
	}
}
