using System.Security.Permissions;
using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the root class for the Xml schema object model hierarchy and serves as a base class for classes such as the <see cref="T:System.Xml.Schema.XmlSchema" /> class.</summary>
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public abstract class XmlSchemaObject
{
	private int lineNum;

	private int linePos;

	private string sourceUri;

	private XmlSerializerNamespaces namespaces;

	private XmlSchemaObject parent;

	private bool isProcessing;

	/// <summary>Gets or sets the line number in the file to which the schema element refers.</summary>
	/// <returns>The line number.</returns>
	[XmlIgnore]
	public int LineNumber
	{
		get
		{
			return lineNum;
		}
		set
		{
			lineNum = value;
		}
	}

	/// <summary>Gets or sets the line position in the file to which the schema element refers.</summary>
	/// <returns>The line position.</returns>
	[XmlIgnore]
	public int LinePosition
	{
		get
		{
			return linePos;
		}
		set
		{
			linePos = value;
		}
	}

	/// <summary>Gets or sets the source location for the file that loaded the schema.</summary>
	/// <returns>The source location (URI) for the file.</returns>
	[XmlIgnore]
	public string SourceUri
	{
		get
		{
			return sourceUri;
		}
		set
		{
			sourceUri = value;
		}
	}

	/// <summary>Gets or sets the parent of this <see cref="T:System.Xml.Schema.XmlSchemaObject" />.</summary>
	/// <returns>The parent <see cref="T:System.Xml.Schema.XmlSchemaObject" /> of this <see cref="T:System.Xml.Schema.XmlSchemaObject" />.</returns>
	[XmlIgnore]
	public XmlSchemaObject Parent
	{
		get
		{
			return parent;
		}
		set
		{
			parent = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.Serialization.XmlSerializerNamespaces" /> to use with this schema object.</summary>
	/// <returns>The <see cref="T:System.Xml.Serialization.XmlSerializerNamespaces" /> property for the schema object.</returns>
	[XmlNamespaceDeclarations]
	public XmlSerializerNamespaces Namespaces
	{
		get
		{
			if (namespaces == null)
			{
				namespaces = new XmlSerializerNamespaces();
			}
			return namespaces;
		}
		set
		{
			namespaces = value;
		}
	}

	[XmlIgnore]
	internal virtual string IdAttribute
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[XmlIgnore]
	internal virtual string NameAttribute
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[XmlIgnore]
	internal bool IsProcessing
	{
		get
		{
			return isProcessing;
		}
		set
		{
			isProcessing = value;
		}
	}

	internal virtual void OnAdd(XmlSchemaObjectCollection container, object item)
	{
	}

	internal virtual void OnRemove(XmlSchemaObjectCollection container, object item)
	{
	}

	internal virtual void OnClear(XmlSchemaObjectCollection container)
	{
	}

	internal virtual void SetUnhandledAttributes(XmlAttribute[] moreAttributes)
	{
	}

	internal virtual void AddAnnotation(XmlSchemaAnnotation annotation)
	{
	}

	internal virtual XmlSchemaObject Clone()
	{
		return (XmlSchemaObject)MemberwiseClone();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaObject" /> class.</summary>
	protected XmlSchemaObject()
	{
	}
}
