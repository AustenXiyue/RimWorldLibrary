using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the World Wide Web Consortium (W3C) any element.</summary>
public class XmlSchemaAny : XmlSchemaParticle
{
	private string ns;

	private XmlSchemaContentProcessing processContents;

	private NamespaceList namespaceList;

	/// <summary>Gets or sets the namespaces containing the elements that can be used.</summary>
	/// <returns>Namespaces for elements that are available for use. The default is ##any.Optional.</returns>
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

	/// <summary>Gets or sets information about how an application or XML processor should handle the validation of XML documents for the elements specified by the any element.</summary>
	/// <returns>One of the <see cref="T:System.Xml.Schema.XmlSchemaContentProcessing" /> values. If no processContents attribute is specified, the default is Strict.</returns>
	[XmlAttribute("processContents")]
	[DefaultValue(XmlSchemaContentProcessing.None)]
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
	internal string ResolvedNamespace
	{
		get
		{
			if (ns == null || ns.Length == 0)
			{
				return "##any";
			}
			return ns;
		}
	}

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

	internal override string NameString
	{
		get
		{
			switch (namespaceList.Type)
			{
			case NamespaceList.ListType.Any:
				return "##any:*";
			case NamespaceList.ListType.Other:
				return "##other:*";
			case NamespaceList.ListType.Set:
			{
				StringBuilder stringBuilder = new StringBuilder();
				int num = 1;
				foreach (string item in namespaceList.Enumerate)
				{
					stringBuilder.Append(item + ":*");
					if (num < namespaceList.Enumerate.Count)
					{
						stringBuilder.Append(" ");
					}
					num++;
				}
				return stringBuilder.ToString();
			}
			default:
				return string.Empty;
			}
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaAny" /> class.</summary>
	public XmlSchemaAny()
	{
	}
}
