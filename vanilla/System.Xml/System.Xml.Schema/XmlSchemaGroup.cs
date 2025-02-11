using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the group element from XML Schema as specified by the World Wide Web Consortium (W3C). This class defines groups at the schema level that are referenced from the complex types. It groups a set of element declarations so that they can be incorporated as a group into complex type definitions.</summary>
public class XmlSchemaGroup : XmlSchemaAnnotated
{
	private string name;

	private XmlSchemaGroupBase particle;

	private XmlSchemaParticle canonicalParticle;

	private XmlQualifiedName qname = XmlQualifiedName.Empty;

	private XmlSchemaGroup redefined;

	private int selfReferenceCount;

	/// <summary>Gets or sets the name of the schema group.</summary>
	/// <returns>The name of the schema group.</returns>
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

	/// <summary>Gets or sets one of the <see cref="T:System.Xml.Schema.XmlSchemaChoice" />, <see cref="T:System.Xml.Schema.XmlSchemaAll" />, or <see cref="T:System.Xml.Schema.XmlSchemaSequence" /> classes.</summary>
	/// <returns>One of the <see cref="T:System.Xml.Schema.XmlSchemaChoice" />, <see cref="T:System.Xml.Schema.XmlSchemaAll" />, or <see cref="T:System.Xml.Schema.XmlSchemaSequence" /> classes.</returns>
	[XmlElement("sequence", typeof(XmlSchemaSequence))]
	[XmlElement("all", typeof(XmlSchemaAll))]
	[XmlElement("choice", typeof(XmlSchemaChoice))]
	public XmlSchemaGroupBase Particle
	{
		get
		{
			return particle;
		}
		set
		{
			particle = value;
		}
	}

	/// <summary>Gets the qualified name of the schema group.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlQualifiedName" /> object representing the qualified name of the schema group.</returns>
	[XmlIgnore]
	public XmlQualifiedName QualifiedName => qname;

	[XmlIgnore]
	internal XmlSchemaParticle CanonicalParticle
	{
		get
		{
			return canonicalParticle;
		}
		set
		{
			canonicalParticle = value;
		}
	}

	[XmlIgnore]
	internal XmlSchemaGroup Redefined
	{
		get
		{
			return redefined;
		}
		set
		{
			redefined = value;
		}
	}

	[XmlIgnore]
	internal int SelfReferenceCount
	{
		get
		{
			return selfReferenceCount;
		}
		set
		{
			selfReferenceCount = value;
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
		qname = value;
	}

	internal override XmlSchemaObject Clone()
	{
		return Clone(null);
	}

	internal XmlSchemaObject Clone(XmlSchema parentSchema)
	{
		XmlSchemaGroup xmlSchemaGroup = (XmlSchemaGroup)MemberwiseClone();
		if (XmlSchemaComplexType.HasParticleRef(particle, parentSchema))
		{
			xmlSchemaGroup.particle = XmlSchemaComplexType.CloneParticle(particle, parentSchema) as XmlSchemaGroupBase;
		}
		xmlSchemaGroup.canonicalParticle = XmlSchemaParticle.Empty;
		return xmlSchemaGroup;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaGroup" /> class.</summary>
	public XmlSchemaGroup()
	{
	}
}
