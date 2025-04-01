using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the World Wide Web Consortium (W3C) anyAttribute element.</summary>
public class XmlSchemaAnyAttribute : XmlSchemaAnnotated
{
	private string ns;

	private XmlSchemaContentProcessing processContents;

	private NamespaceList namespaceList;

	/// <summary>Gets or sets the namespaces containing the attributes that can be used.</summary>
	/// <returns>Namespaces for attributes that are available for use. The default is ##any.Optional.</returns>
	[XmlAttribute("namespace")]
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

	/// <summary>Gets or sets information about how an application or XML processor should handle the validation of XML documents for the attributes specified by the anyAttribute element.</summary>
	/// <returns>One of the <see cref="T:System.Xml.Schema.XmlSchemaContentProcessing" /> values. If no processContents attribute is specified, the default is Strict.</returns>
	[DefaultValue(XmlSchemaContentProcessing.None)]
	[XmlAttribute("processContents")]
	public XmlSchemaContentProcessing ProcessContents
	{
		get
		{
			return processContents;
		}
		set
		{
			processContents = value;
		}
	}

	[XmlIgnore]
	internal NamespaceList NamespaceList => namespaceList;

	[XmlIgnore]
	internal XmlSchemaContentProcessing ProcessContentsCorrect
	{
		get
		{
			if (processContents != 0)
			{
				return processContents;
			}
			return XmlSchemaContentProcessing.Strict;
		}
	}

	internal void BuildNamespaceList(string targetNamespace)
	{
		if (ns != null)
		{
			namespaceList = new NamespaceList(ns, targetNamespace);
		}
		else
		{
			namespaceList = new NamespaceList();
		}
	}

	internal void BuildNamespaceListV1Compat(string targetNamespace)
	{
		if (ns != null)
		{
			namespaceList = new NamespaceListV1Compat(ns, targetNamespace);
		}
		else
		{
			namespaceList = new NamespaceList();
		}
	}

	internal bool Allows(XmlQualifiedName qname)
	{
		return namespaceList.Allows(qname.Namespace);
	}

	internal static bool IsSubset(XmlSchemaAnyAttribute sub, XmlSchemaAnyAttribute super)
	{
		return NamespaceList.IsSubset(sub.NamespaceList, super.NamespaceList);
	}

	internal static XmlSchemaAnyAttribute Intersection(XmlSchemaAnyAttribute o1, XmlSchemaAnyAttribute o2, bool v1Compat)
	{
		NamespaceList namespaceList = NamespaceList.Intersection(o1.NamespaceList, o2.NamespaceList, v1Compat);
		if (namespaceList != null)
		{
			return new XmlSchemaAnyAttribute
			{
				namespaceList = namespaceList,
				ProcessContents = o1.ProcessContents,
				Annotation = o1.Annotation
			};
		}
		return null;
	}

	internal static XmlSchemaAnyAttribute Union(XmlSchemaAnyAttribute o1, XmlSchemaAnyAttribute o2, bool v1Compat)
	{
		NamespaceList namespaceList = NamespaceList.Union(o1.NamespaceList, o2.NamespaceList, v1Compat);
		if (namespaceList != null)
		{
			return new XmlSchemaAnyAttribute
			{
				namespaceList = namespaceList,
				processContents = o1.processContents,
				Annotation = o1.Annotation
			};
		}
		return null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaAnyAttribute" /> class.</summary>
	public XmlSchemaAnyAttribute()
	{
	}
}
