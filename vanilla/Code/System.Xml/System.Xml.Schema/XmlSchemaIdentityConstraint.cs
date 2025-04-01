using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Class for the identity constraints: key, keyref, and unique elements.</summary>
public class XmlSchemaIdentityConstraint : XmlSchemaAnnotated
{
	private string name;

	private XmlSchemaXPath selector;

	private XmlSchemaObjectCollection fields = new XmlSchemaObjectCollection();

	private XmlQualifiedName qualifiedName = XmlQualifiedName.Empty;

	private CompiledIdentityConstraint compiledConstraint;

	/// <summary>Gets or sets the name of the identity constraint.</summary>
	/// <returns>The name of the identity constraint.</returns>
	[XmlAttribute("name")]
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Gets or sets the XPath expression selector element.</summary>
	/// <returns>The XPath expression selector element.</returns>
	[XmlElement("selector", typeof(XmlSchemaXPath))]
	public XmlSchemaXPath Selector
	{
		get
		{
			return selector;
		}
		set
		{
			selector = value;
		}
	}

	/// <summary>Gets the collection of fields that apply as children for the XML Path Language (XPath) expression selector.</summary>
	/// <returns>The collection of fields.</returns>
	[XmlElement("field", typeof(XmlSchemaXPath))]
	public XmlSchemaObjectCollection Fields => fields;

	/// <summary>Gets the qualified name of the identity constraint, which holds the post-compilation value of the QualifiedName property.</summary>
	/// <returns>The post-compilation value of the QualifiedName property.</returns>
	[XmlIgnore]
	public XmlQualifiedName QualifiedName => qualifiedName;

	[XmlIgnore]
	internal CompiledIdentityConstraint CompiledConstraint
	{
		get
		{
			return compiledConstraint;
		}
		set
		{
			compiledConstraint = value;
		}
	}

	[XmlIgnore]
	internal override string NameAttribute
	{
		get
		{
			return Name;
		}
		set
		{
			Name = value;
		}
	}

	internal void SetQualifiedName(XmlQualifiedName value)
	{
		qualifiedName = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaIdentityConstraint" /> class.</summary>
	public XmlSchemaIdentityConstraint()
	{
	}
}
