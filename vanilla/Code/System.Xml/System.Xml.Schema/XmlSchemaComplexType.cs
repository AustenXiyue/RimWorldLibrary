using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the complexType element from XML Schema as specified by the World Wide Web Consortium (W3C). This class defines a complex type that determines the set of attributes and content of an element.</summary>
public class XmlSchemaComplexType : XmlSchemaType
{
	private XmlSchemaDerivationMethod block = XmlSchemaDerivationMethod.None;

	private XmlSchemaContentModel contentModel;

	private XmlSchemaParticle particle;

	private XmlSchemaObjectCollection attributes;

	private XmlSchemaAnyAttribute anyAttribute;

	private XmlSchemaParticle contentTypeParticle = XmlSchemaParticle.Empty;

	private XmlSchemaDerivationMethod blockResolved;

	private XmlSchemaObjectTable localElements;

	private XmlSchemaObjectTable attributeUses;

	private XmlSchemaAnyAttribute attributeWildcard;

	private static XmlSchemaComplexType anyTypeLax;

	private static XmlSchemaComplexType anyTypeSkip;

	private static XmlSchemaComplexType untypedAnyType;

	private byte pvFlags;

	private const byte wildCardMask = 1;

	private const byte isMixedMask = 2;

	private const byte isAbstractMask = 4;

	[XmlIgnore]
	internal static XmlSchemaComplexType AnyType => anyTypeLax;

	[XmlIgnore]
	internal static XmlSchemaComplexType UntypedAnyType => untypedAnyType;

	[XmlIgnore]
	internal static XmlSchemaComplexType AnyTypeSkip => anyTypeSkip;

	internal static ContentValidator AnyTypeContentValidator => anyTypeLax.ElementDecl.ContentValidator;

	/// <summary>Gets or sets the information that determines if the complexType element can be used in the instance document.</summary>
	/// <returns>If true, an element cannot use this complexType element directly and must use a complex type that is derived from this complexType element. The default is false.Optional.</returns>
	[XmlAttribute("abstract")]
	[DefaultValue(false)]
	public bool IsAbstract
	{
		get
		{
			return (pvFlags & 4) != 0;
		}
		set
		{
			if (value)
			{
				pvFlags |= 4;
			}
			else
			{
				pvFlags = (byte)(pvFlags & -5);
			}
		}
	}

	/// <summary>Gets or sets the block attribute.</summary>
	/// <returns>The block attribute prevents a complex type from being used in the specified type of derivation. The default is XmlSchemaDerivationMethod.None.Optional.</returns>
	[DefaultValue(XmlSchemaDerivationMethod.None)]
	[XmlAttribute("block")]
	public XmlSchemaDerivationMethod Block
	{
		get
		{
			return block;
		}
		set
		{
			block = value;
		}
	}

	/// <summary>Gets or sets information that determines if the complex type has a mixed content model (markup within the content).</summary>
	/// <returns>true, if character data can appear between child elements of this complex type; otherwise, false. The default is false.Optional.</returns>
	[XmlAttribute("mixed")]
	[DefaultValue(false)]
	public override bool IsMixed
	{
		get
		{
			return (pvFlags & 2) != 0;
		}
		set
		{
			if (value)
			{
				pvFlags |= 2;
			}
			else
			{
				pvFlags = (byte)(pvFlags & -3);
			}
		}
	}

	/// <summary>Gets or sets the post-compilation <see cref="T:System.Xml.Schema.XmlSchemaContentModel" /> of this complex type.</summary>
	/// <returns>The content model type that is one of the <see cref="T:System.Xml.Schema.XmlSchemaSimpleContent" /> or <see cref="T:System.Xml.Schema.XmlSchemaComplexContent" /> classes.</returns>
	[XmlElement("simpleContent", typeof(XmlSchemaSimpleContent))]
	[XmlElement("complexContent", typeof(XmlSchemaComplexContent))]
	public XmlSchemaContentModel ContentModel
	{
		get
		{
			return contentModel;
		}
		set
		{
			contentModel = value;
		}
	}

	/// <summary>Gets or sets the compositor type as one of the <see cref="T:System.Xml.Schema.XmlSchemaGroupRef" />, <see cref="T:System.Xml.Schema.XmlSchemaChoice" />, <see cref="T:System.Xml.Schema.XmlSchemaAll" />, or <see cref="T:System.Xml.Schema.XmlSchemaSequence" /> classes.</summary>
	/// <returns>The compositor type.</returns>
	[XmlElement("group", typeof(XmlSchemaGroupRef))]
	[XmlElement("choice", typeof(XmlSchemaChoice))]
	[XmlElement("all", typeof(XmlSchemaAll))]
	[XmlElement("sequence", typeof(XmlSchemaSequence))]
	public XmlSchemaParticle Particle
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

	/// <summary>Gets the collection of attributes for the complex type.</summary>
	/// <returns>Contains <see cref="T:System.Xml.Schema.XmlSchemaAttribute" /> and <see cref="T:System.Xml.Schema.XmlSchemaAttributeGroupRef" /> classes.</returns>
	[XmlElement("attribute", typeof(XmlSchemaAttribute))]
	[XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef))]
	public XmlSchemaObjectCollection Attributes
	{
		get
		{
			if (attributes == null)
			{
				attributes = new XmlSchemaObjectCollection();
			}
			return attributes;
		}
	}

	/// <summary>Gets or sets the value for the <see cref="T:System.Xml.Schema.XmlSchemaAnyAttribute" /> component of the complex type.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.XmlSchemaAnyAttribute" /> component of the complex type.</returns>
	[XmlElement("anyAttribute")]
	public XmlSchemaAnyAttribute AnyAttribute
	{
		get
		{
			return anyAttribute;
		}
		set
		{
			anyAttribute = value;
		}
	}

	/// <summary>Gets the content model of the complex type which holds the post-compilation value.</summary>
	/// <returns>The post-compilation value of the content model for the complex type.</returns>
	[XmlIgnore]
	public XmlSchemaContentType ContentType => base.SchemaContentType;

	/// <summary>Gets the particle that holds the post-compilation value of the <see cref="P:System.Xml.Schema.XmlSchemaComplexType.ContentType" /> particle.</summary>
	/// <returns>The particle for the content type. The post-compilation value of the <see cref="P:System.Xml.Schema.XmlSchemaComplexType.ContentType" /> particle.</returns>
	[XmlIgnore]
	public XmlSchemaParticle ContentTypeParticle => contentTypeParticle;

	/// <summary>Gets the value after the type has been compiled to the post-schema-validation information set (infoset). This value indicates how the type is enforced when xsi:type is used in the instance document.</summary>
	/// <returns>The post-schema-validated infoset value. The default is BlockDefault value on the schema element.</returns>
	[XmlIgnore]
	public XmlSchemaDerivationMethod BlockResolved => blockResolved;

	/// <summary>Gets the collection of all the complied attributes of this complex type and its base types.</summary>
	/// <returns>The collection of all the attributes from this complex type and its base types. The post-compilation value of the AttributeUses property.</returns>
	[XmlIgnore]
	public XmlSchemaObjectTable AttributeUses
	{
		get
		{
			if (attributeUses == null)
			{
				attributeUses = new XmlSchemaObjectTable();
			}
			return attributeUses;
		}
	}

	/// <summary>Gets the post-compilation value for anyAttribute for this complex type and its base type(s).</summary>
	/// <returns>The post-compilation value of the anyAttribute element.</returns>
	[XmlIgnore]
	public XmlSchemaAnyAttribute AttributeWildcard => attributeWildcard;

	[XmlIgnore]
	internal XmlSchemaObjectTable LocalElements
	{
		get
		{
			if (localElements == null)
			{
				localElements = new XmlSchemaObjectTable();
			}
			return localElements;
		}
	}

	internal bool HasWildCard
	{
		get
		{
			return (pvFlags & 1) != 0;
		}
		set
		{
			if (value)
			{
				pvFlags |= 1;
			}
			else
			{
				pvFlags = (byte)(pvFlags & -2);
			}
		}
	}

	internal override XmlQualifiedName DerivedFrom
	{
		get
		{
			if (contentModel == null)
			{
				return XmlQualifiedName.Empty;
			}
			if (contentModel.Content is XmlSchemaComplexContentRestriction)
			{
				return ((XmlSchemaComplexContentRestriction)contentModel.Content).BaseTypeName;
			}
			if (contentModel.Content is XmlSchemaComplexContentExtension)
			{
				return ((XmlSchemaComplexContentExtension)contentModel.Content).BaseTypeName;
			}
			if (contentModel.Content is XmlSchemaSimpleContentRestriction)
			{
				return ((XmlSchemaSimpleContentRestriction)contentModel.Content).BaseTypeName;
			}
			if (contentModel.Content is XmlSchemaSimpleContentExtension)
			{
				return ((XmlSchemaSimpleContentExtension)contentModel.Content).BaseTypeName;
			}
			return XmlQualifiedName.Empty;
		}
	}

	static XmlSchemaComplexType()
	{
		anyTypeLax = CreateAnyType(XmlSchemaContentProcessing.Lax);
		anyTypeSkip = CreateAnyType(XmlSchemaContentProcessing.Skip);
		untypedAnyType = new XmlSchemaComplexType();
		untypedAnyType.SetQualifiedName(new XmlQualifiedName("untypedAny", "http://www.w3.org/2003/11/xpath-datatypes"));
		untypedAnyType.IsMixed = true;
		untypedAnyType.SetContentTypeParticle(anyTypeLax.ContentTypeParticle);
		untypedAnyType.SetContentType(XmlSchemaContentType.Mixed);
		untypedAnyType.ElementDecl = SchemaElementDecl.CreateAnyTypeElementDecl();
		untypedAnyType.ElementDecl.SchemaType = untypedAnyType;
		untypedAnyType.ElementDecl.ContentValidator = AnyTypeContentValidator;
	}

	private static XmlSchemaComplexType CreateAnyType(XmlSchemaContentProcessing processContents)
	{
		XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
		xmlSchemaComplexType.SetQualifiedName(DatatypeImplementation.QnAnyType);
		XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
		xmlSchemaAny.MinOccurs = 0m;
		xmlSchemaAny.MaxOccurs = decimal.MaxValue;
		xmlSchemaAny.ProcessContents = processContents;
		xmlSchemaAny.BuildNamespaceList(null);
		XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
		xmlSchemaSequence.Items.Add(xmlSchemaAny);
		xmlSchemaComplexType.SetContentTypeParticle(xmlSchemaSequence);
		xmlSchemaComplexType.SetContentType(XmlSchemaContentType.Mixed);
		xmlSchemaComplexType.ElementDecl = SchemaElementDecl.CreateAnyTypeElementDecl();
		xmlSchemaComplexType.ElementDecl.SchemaType = xmlSchemaComplexType;
		ParticleContentValidator particleContentValidator = new ParticleContentValidator(XmlSchemaContentType.Mixed);
		particleContentValidator.Start();
		particleContentValidator.OpenGroup();
		particleContentValidator.AddNamespaceList(xmlSchemaAny.NamespaceList, xmlSchemaAny);
		particleContentValidator.AddStar();
		particleContentValidator.CloseGroup();
		ContentValidator contentValidator = particleContentValidator.Finish(useDFA: true);
		xmlSchemaComplexType.ElementDecl.ContentValidator = contentValidator;
		XmlSchemaAnyAttribute xmlSchemaAnyAttribute = new XmlSchemaAnyAttribute();
		xmlSchemaAnyAttribute.ProcessContents = processContents;
		xmlSchemaAnyAttribute.BuildNamespaceList(null);
		xmlSchemaComplexType.SetAttributeWildcard(xmlSchemaAnyAttribute);
		xmlSchemaComplexType.ElementDecl.AnyAttribute = xmlSchemaAnyAttribute;
		return xmlSchemaComplexType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaComplexType" /> class.</summary>
	public XmlSchemaComplexType()
	{
	}

	internal void SetContentTypeParticle(XmlSchemaParticle value)
	{
		contentTypeParticle = value;
	}

	internal void SetBlockResolved(XmlSchemaDerivationMethod value)
	{
		blockResolved = value;
	}

	internal void SetAttributeWildcard(XmlSchemaAnyAttribute value)
	{
		attributeWildcard = value;
	}

	internal void SetAttributes(XmlSchemaObjectCollection newAttributes)
	{
		attributes = newAttributes;
	}

	internal bool ContainsIdAttribute(bool findAll)
	{
		int num = 0;
		foreach (XmlSchemaAttribute value in AttributeUses.Values)
		{
			if (value.Use == XmlSchemaUse.Prohibited)
			{
				continue;
			}
			XmlSchemaDatatype xmlSchemaDatatype = value.Datatype;
			if (xmlSchemaDatatype != null && xmlSchemaDatatype.TypeCode == XmlTypeCode.Id)
			{
				num++;
				if (num > 1)
				{
					break;
				}
			}
		}
		if (!findAll)
		{
			return num > 0;
		}
		return num > 1;
	}

	internal override XmlSchemaObject Clone()
	{
		return Clone(null);
	}

	internal XmlSchemaObject Clone(XmlSchema parentSchema)
	{
		XmlSchemaComplexType xmlSchemaComplexType = (XmlSchemaComplexType)MemberwiseClone();
		if (xmlSchemaComplexType.ContentModel != null)
		{
			if (xmlSchemaComplexType.ContentModel is XmlSchemaSimpleContent xmlSchemaSimpleContent)
			{
				XmlSchemaSimpleContent xmlSchemaSimpleContent2 = (XmlSchemaSimpleContent)xmlSchemaSimpleContent.Clone();
				if (xmlSchemaSimpleContent.Content is XmlSchemaSimpleContentExtension xmlSchemaSimpleContentExtension)
				{
					XmlSchemaSimpleContentExtension xmlSchemaSimpleContentExtension2 = (XmlSchemaSimpleContentExtension)xmlSchemaSimpleContentExtension.Clone();
					xmlSchemaSimpleContentExtension2.BaseTypeName = xmlSchemaSimpleContentExtension.BaseTypeName.Clone();
					xmlSchemaSimpleContentExtension2.SetAttributes(CloneAttributes(xmlSchemaSimpleContentExtension.Attributes));
					xmlSchemaSimpleContent2.Content = xmlSchemaSimpleContentExtension2;
				}
				else
				{
					XmlSchemaSimpleContentRestriction xmlSchemaSimpleContentRestriction = (XmlSchemaSimpleContentRestriction)xmlSchemaSimpleContent.Content;
					XmlSchemaSimpleContentRestriction xmlSchemaSimpleContentRestriction2 = (XmlSchemaSimpleContentRestriction)xmlSchemaSimpleContentRestriction.Clone();
					xmlSchemaSimpleContentRestriction2.BaseTypeName = xmlSchemaSimpleContentRestriction.BaseTypeName.Clone();
					xmlSchemaSimpleContentRestriction2.SetAttributes(CloneAttributes(xmlSchemaSimpleContentRestriction.Attributes));
					xmlSchemaSimpleContent2.Content = xmlSchemaSimpleContentRestriction2;
				}
				xmlSchemaComplexType.ContentModel = xmlSchemaSimpleContent2;
			}
			else
			{
				XmlSchemaComplexContent xmlSchemaComplexContent = (XmlSchemaComplexContent)xmlSchemaComplexType.ContentModel;
				XmlSchemaComplexContent xmlSchemaComplexContent2 = (XmlSchemaComplexContent)xmlSchemaComplexContent.Clone();
				if (xmlSchemaComplexContent.Content is XmlSchemaComplexContentExtension xmlSchemaComplexContentExtension)
				{
					XmlSchemaComplexContentExtension xmlSchemaComplexContentExtension2 = (XmlSchemaComplexContentExtension)xmlSchemaComplexContentExtension.Clone();
					xmlSchemaComplexContentExtension2.BaseTypeName = xmlSchemaComplexContentExtension.BaseTypeName.Clone();
					xmlSchemaComplexContentExtension2.SetAttributes(CloneAttributes(xmlSchemaComplexContentExtension.Attributes));
					if (HasParticleRef(xmlSchemaComplexContentExtension.Particle, parentSchema))
					{
						xmlSchemaComplexContentExtension2.Particle = CloneParticle(xmlSchemaComplexContentExtension.Particle, parentSchema);
					}
					xmlSchemaComplexContent2.Content = xmlSchemaComplexContentExtension2;
				}
				else
				{
					XmlSchemaComplexContentRestriction xmlSchemaComplexContentRestriction = xmlSchemaComplexContent.Content as XmlSchemaComplexContentRestriction;
					XmlSchemaComplexContentRestriction xmlSchemaComplexContentRestriction2 = (XmlSchemaComplexContentRestriction)xmlSchemaComplexContentRestriction.Clone();
					xmlSchemaComplexContentRestriction2.BaseTypeName = xmlSchemaComplexContentRestriction.BaseTypeName.Clone();
					xmlSchemaComplexContentRestriction2.SetAttributes(CloneAttributes(xmlSchemaComplexContentRestriction.Attributes));
					if (HasParticleRef(xmlSchemaComplexContentRestriction2.Particle, parentSchema))
					{
						xmlSchemaComplexContentRestriction2.Particle = CloneParticle(xmlSchemaComplexContentRestriction2.Particle, parentSchema);
					}
					xmlSchemaComplexContent2.Content = xmlSchemaComplexContentRestriction2;
				}
				xmlSchemaComplexType.ContentModel = xmlSchemaComplexContent2;
			}
		}
		else
		{
			if (HasParticleRef(xmlSchemaComplexType.Particle, parentSchema))
			{
				xmlSchemaComplexType.Particle = CloneParticle(xmlSchemaComplexType.Particle, parentSchema);
			}
			xmlSchemaComplexType.SetAttributes(CloneAttributes(xmlSchemaComplexType.Attributes));
		}
		xmlSchemaComplexType.ClearCompiledState();
		return xmlSchemaComplexType;
	}

	private void ClearCompiledState()
	{
		attributeUses = null;
		localElements = null;
		attributeWildcard = null;
		contentTypeParticle = XmlSchemaParticle.Empty;
		blockResolved = XmlSchemaDerivationMethod.None;
	}

	internal static XmlSchemaObjectCollection CloneAttributes(XmlSchemaObjectCollection attributes)
	{
		if (HasAttributeQNameRef(attributes))
		{
			XmlSchemaObjectCollection xmlSchemaObjectCollection = attributes.Clone();
			for (int i = 0; i < attributes.Count; i++)
			{
				XmlSchemaObject xmlSchemaObject = attributes[i];
				if (xmlSchemaObject is XmlSchemaAttributeGroupRef xmlSchemaAttributeGroupRef)
				{
					XmlSchemaAttributeGroupRef xmlSchemaAttributeGroupRef2 = (XmlSchemaAttributeGroupRef)xmlSchemaAttributeGroupRef.Clone();
					xmlSchemaAttributeGroupRef2.RefName = xmlSchemaAttributeGroupRef.RefName.Clone();
					xmlSchemaObjectCollection[i] = xmlSchemaAttributeGroupRef2;
					continue;
				}
				XmlSchemaAttribute xmlSchemaAttribute = xmlSchemaObject as XmlSchemaAttribute;
				if (!xmlSchemaAttribute.RefName.IsEmpty || !xmlSchemaAttribute.SchemaTypeName.IsEmpty)
				{
					xmlSchemaObjectCollection[i] = xmlSchemaAttribute.Clone();
				}
			}
			return xmlSchemaObjectCollection;
		}
		return attributes;
	}

	private static XmlSchemaObjectCollection CloneGroupBaseParticles(XmlSchemaObjectCollection groupBaseParticles, XmlSchema parentSchema)
	{
		XmlSchemaObjectCollection xmlSchemaObjectCollection = groupBaseParticles.Clone();
		for (int i = 0; i < groupBaseParticles.Count; i++)
		{
			XmlSchemaParticle xmlSchemaParticle = (XmlSchemaParticle)groupBaseParticles[i];
			xmlSchemaObjectCollection[i] = CloneParticle(xmlSchemaParticle, parentSchema);
		}
		return xmlSchemaObjectCollection;
	}

	internal static XmlSchemaParticle CloneParticle(XmlSchemaParticle particle, XmlSchema parentSchema)
	{
		if (particle is XmlSchemaGroupBase xmlSchemaGroupBase)
		{
			XmlSchemaObjectCollection items = CloneGroupBaseParticles(xmlSchemaGroupBase.Items, parentSchema);
			XmlSchemaGroupBase obj = (XmlSchemaGroupBase)xmlSchemaGroupBase.Clone();
			obj.SetItems(items);
			return obj;
		}
		if (particle is XmlSchemaGroupRef)
		{
			XmlSchemaGroupRef obj2 = (XmlSchemaGroupRef)particle.Clone();
			obj2.RefName = obj2.RefName.Clone();
			return obj2;
		}
		if (particle is XmlSchemaElement xmlSchemaElement && (!xmlSchemaElement.RefName.IsEmpty || !xmlSchemaElement.SchemaTypeName.IsEmpty || GetResolvedElementForm(parentSchema, xmlSchemaElement) == XmlSchemaForm.Qualified))
		{
			return (XmlSchemaElement)xmlSchemaElement.Clone(parentSchema);
		}
		return particle;
	}

	private static XmlSchemaForm GetResolvedElementForm(XmlSchema parentSchema, XmlSchemaElement element)
	{
		if (element.Form == XmlSchemaForm.None && parentSchema != null)
		{
			return parentSchema.ElementFormDefault;
		}
		return element.Form;
	}

	internal static bool HasParticleRef(XmlSchemaParticle particle, XmlSchema parentSchema)
	{
		if (particle is XmlSchemaGroupBase xmlSchemaGroupBase)
		{
			bool flag = false;
			int num = 0;
			while (num < xmlSchemaGroupBase.Items.Count && !flag)
			{
				XmlSchemaParticle xmlSchemaParticle = (XmlSchemaParticle)xmlSchemaGroupBase.Items[num++];
				flag = xmlSchemaParticle is XmlSchemaGroupRef || (xmlSchemaParticle is XmlSchemaElement xmlSchemaElement && (!xmlSchemaElement.RefName.IsEmpty || !xmlSchemaElement.SchemaTypeName.IsEmpty || GetResolvedElementForm(parentSchema, xmlSchemaElement) == XmlSchemaForm.Qualified)) || HasParticleRef(xmlSchemaParticle, parentSchema);
			}
			return flag;
		}
		if (particle is XmlSchemaGroupRef)
		{
			return true;
		}
		return false;
	}

	internal static bool HasAttributeQNameRef(XmlSchemaObjectCollection attributes)
	{
		for (int i = 0; i < attributes.Count; i++)
		{
			if (attributes[i] is XmlSchemaAttributeGroupRef)
			{
				return true;
			}
			XmlSchemaAttribute xmlSchemaAttribute = attributes[i] as XmlSchemaAttribute;
			if (!xmlSchemaAttribute.RefName.IsEmpty || !xmlSchemaAttribute.SchemaTypeName.IsEmpty)
			{
				return true;
			}
		}
		return false;
	}
}
