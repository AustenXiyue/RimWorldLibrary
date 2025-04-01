using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the import element from XML Schema as specified by the World Wide Web Consortium (W3C). This class is used to import schema components from other schemas.</summary>
public class XmlSchemaImport : XmlSchemaExternal
{
	private string ns;

	private XmlSchemaAnnotation annotation;

	/// <summary>Gets or sets the target namespace for the imported schema as a Uniform Resource Identifier (URI) reference.</summary>
	/// <returns>The target namespace for the imported schema as a URI reference.Optional.</returns>
	[XmlAttribute("namespace", DataType = "anyURI")]
	public string Namespace
	{
		get
		{
			return ns;
		}
		set
		{
			ns = value;
		}
	}

	/// <summary>Gets or sets the annotation property.</summary>
	/// <returns>The annotation.</returns>
	[XmlElement("annotation", typeof(XmlSchemaAnnotation))]
	public XmlSchemaAnnotation Annotation
	{
		get
		{
			return annotation;
		}
		set
		{
			annotation = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaImport" /> class.</summary>
	public XmlSchemaImport()
	{
		base.Compositor = Compositor.Import;
	}

	internal override void AddAnnotation(XmlSchemaAnnotation annotation)
	{
		this.annotation = annotation;
	}
}
