using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MS.Internal;
using MS.Internal.Annotations;
using MS.Utility;

namespace System.Windows.Annotations;

/// <summary>Represents a user annotation in the Microsoft Annotations Framework.</summary>
[XmlRoot(Namespace = "http://schemas.microsoft.com/windows/annotations/2003/11/core", ElementName = "Annotation")]
public sealed class Annotation : IXmlSerializable
{
	private Guid _id;

	private XmlQualifiedName _typeName;

	private DateTime _created;

	private DateTime _modified;

	private ObservableCollection<string> _authors;

	private AnnotationResourceCollection _cargos;

	private AnnotationResourceCollection _anchors;

	private static Serializer _ResourceSerializer;

	private const char _Colon = ':';

	/// <summary>Gets the globally unique identifier (GUID) of the <see cref="T:System.Windows.Annotations.Annotation" />. </summary>
	/// <returns>The GUID of the annotation.</returns>
	public Guid Id => _id;

	/// <summary>Gets the <see cref="T:System.Xml.XmlQualifiedName" /> of the annotation type.</summary>
	/// <returns>The XML qualified name for this kind of annotation.</returns>
	public XmlQualifiedName AnnotationType => _typeName;

	/// <summary>Gets the date and the time that the annotation was created. </summary>
	/// <returns>The date and the time the annotation was created.</returns>
	public DateTime CreationTime => _created;

	/// <summary>Gets the date and the time that the annotation was last modified. </summary>
	/// <returns>The date and the time the annotation was last modified.</returns>
	public DateTime LastModificationTime => _modified;

	/// <summary>Gets a collection of zero or more author strings that identify who created the <see cref="T:System.Windows.Annotations.Annotation" />.</summary>
	/// <returns>A collection of zero or more author strings.</returns>
	public Collection<string> Authors => _authors;

	/// <summary>Gets a collection of zero or more <see cref="T:System.Windows.Annotations.AnnotationResource" /> anchor elements that define the data selection(s) being annotated. </summary>
	/// <returns>A collection of zero or more <see cref="T:System.Windows.Annotations.AnnotationResource" /> anchor elements.</returns>
	public Collection<AnnotationResource> Anchors => _anchors;

	/// <summary>Gets a collection of zero or more <see cref="T:System.Windows.Annotations.AnnotationResource" /> cargo elements that contain data for the annotation. </summary>
	/// <returns>A collection of zero or more <see cref="T:System.Windows.Annotations.AnnotationResource" /> cargo elements.</returns>
	public Collection<AnnotationResource> Cargos => _cargos;

	private static Serializer ResourceSerializer
	{
		get
		{
			if (_ResourceSerializer == null)
			{
				_ResourceSerializer = new Serializer(typeof(AnnotationResource));
			}
			return _ResourceSerializer;
		}
	}

	/// <summary>Occurs when an author is added, removed, or modified in the list of annotation <see cref="P:System.Windows.Annotations.Annotation.Authors" />.</summary>
	public event AnnotationAuthorChangedEventHandler AuthorChanged;

	/// <summary>Occurs when an anchor is added, removed, or modified in the list of annotation <see cref="P:System.Windows.Annotations.Annotation.Anchors" />.</summary>
	public event AnnotationResourceChangedEventHandler AnchorChanged;

	/// <summary>Occurs when a cargo is added, removed, or modified in the list of annotation <see cref="P:System.Windows.Annotations.Annotation.Cargos" />.</summary>
	public event AnnotationResourceChangedEventHandler CargoChanged;

	/// <summary>This constructor supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	public Annotation()
	{
		_id = Guid.Empty;
		_created = DateTime.MinValue;
		_modified = DateTime.MinValue;
		Init();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.Annotation" /> class that has a specified type name and namespace.</summary>
	/// <param name="annotationType">The type name of the annotation.</param>
	public Annotation(XmlQualifiedName annotationType)
	{
		if (annotationType == null)
		{
			throw new ArgumentNullException("annotationType");
		}
		if (string.IsNullOrEmpty(annotationType.Name))
		{
			throw new ArgumentException(SR.TypeNameMustBeSpecified, "annotationType.Name");
		}
		if (string.IsNullOrEmpty(annotationType.Namespace))
		{
			throw new ArgumentException(SR.TypeNameMustBeSpecified, "annotationType.Namespace");
		}
		_id = Guid.NewGuid();
		_typeName = annotationType;
		_created = DateTime.Now;
		_modified = _created;
		Init();
	}

	/// <summary>This constructor supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="annotationType">The type name of the annotation.</param>
	/// <param name="id">The globally unique identifier (GUID) for the annotation.</param>
	/// <param name="creationTime">The date and time the annotation was first created.</param>
	/// <param name="lastModificationTime">The date and time the annotation was last modified.</param>
	public Annotation(XmlQualifiedName annotationType, Guid id, DateTime creationTime, DateTime lastModificationTime)
	{
		if (annotationType == null)
		{
			throw new ArgumentNullException("annotationType");
		}
		if (string.IsNullOrEmpty(annotationType.Name))
		{
			throw new ArgumentException(SR.TypeNameMustBeSpecified, "annotationType.Name");
		}
		if (string.IsNullOrEmpty(annotationType.Namespace))
		{
			throw new ArgumentException(SR.TypeNameMustBeSpecified, "annotationType.Namespace");
		}
		if (id.Equals(Guid.Empty))
		{
			throw new ArgumentException(SR.InvalidGuid, "id");
		}
		if (lastModificationTime.CompareTo(creationTime) < 0)
		{
			throw new ArgumentException(SR.ModificationEarlierThanCreation, "lastModificationTime");
		}
		_id = id;
		_typeName = annotationType;
		_created = creationTime;
		_modified = lastModificationTime;
		Init();
	}

	/// <summary>Always returns null.  See Annotations Schema for schema details.</summary>
	/// <returns>Always null.  See Annotations Schema for schema details.</returns>
	public XmlSchema GetSchema()
	{
		return null;
	}

	/// <summary>Serializes the annotation to a specified <see cref="T:System.Xml.XmlWriter" />. </summary>
	/// <param name="writer">The XML writer to use to serialize the annotation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Annotations.Annotation.AnnotationType" /> is not valid.</exception>
	public void WriteXml(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.SerializeAnnotationBegin);
		try
		{
			if (string.IsNullOrEmpty(writer.LookupPrefix("http://schemas.microsoft.com/windows/annotations/2003/11/core")))
			{
				writer.WriteAttributeString("xmlns", "anc", null, "http://schemas.microsoft.com/windows/annotations/2003/11/core");
			}
			if (string.IsNullOrEmpty(writer.LookupPrefix("http://schemas.microsoft.com/windows/annotations/2003/11/base")))
			{
				writer.WriteAttributeString("xmlns", "anb", null, "http://schemas.microsoft.com/windows/annotations/2003/11/base");
			}
			if (_typeName == null)
			{
				throw new InvalidOperationException(SR.CannotSerializeInvalidInstance);
			}
			writer.WriteAttributeString("Id", XmlConvert.ToString(_id));
			writer.WriteAttributeString("CreationTime", XmlConvert.ToString(_created));
			writer.WriteAttributeString("LastModificationTime", XmlConvert.ToString(_modified));
			writer.WriteStartAttribute("Type");
			writer.WriteQualifiedName(_typeName.Name, _typeName.Namespace);
			writer.WriteEndAttribute();
			if (_authors != null && _authors.Count > 0)
			{
				writer.WriteStartElement("Authors", "http://schemas.microsoft.com/windows/annotations/2003/11/core");
				foreach (string author in _authors)
				{
					if (author != null)
					{
						writer.WriteElementString("anb", "StringAuthor", "http://schemas.microsoft.com/windows/annotations/2003/11/base", author);
					}
				}
				writer.WriteEndElement();
			}
			if (_anchors != null && _anchors.Count > 0)
			{
				writer.WriteStartElement("Anchors", "http://schemas.microsoft.com/windows/annotations/2003/11/core");
				foreach (AnnotationResource anchor in _anchors)
				{
					if (anchor != null)
					{
						ResourceSerializer.Serialize(writer, anchor);
					}
				}
				writer.WriteEndElement();
			}
			if (_cargos == null || _cargos.Count <= 0)
			{
				return;
			}
			writer.WriteStartElement("Cargos", "http://schemas.microsoft.com/windows/annotations/2003/11/core");
			foreach (AnnotationResource cargo in _cargos)
			{
				if (cargo != null)
				{
					ResourceSerializer.Serialize(writer, cargo);
				}
			}
			writer.WriteEndElement();
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.SerializeAnnotationEnd);
		}
	}

	/// <summary>Deserializes the <see cref="T:System.Windows.Annotations.Annotation" /> from a specified <see cref="T:System.Xml.XmlReader" />. </summary>
	/// <param name="reader">The XML reader to use to deserialize the annotation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	/// <exception cref="T:System.Xml.XmlException">The serialized XML for the <see cref="T:System.Windows.Annotations.Annotation" /> is not valid.</exception>
	public void ReadXml(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.DeserializeAnnotationBegin);
		XmlDocument xmlDocument = null;
		try
		{
			xmlDocument = new XmlDocument();
			ReadAttributes(reader);
			if (!reader.IsEmptyElement)
			{
				reader.Read();
				while (XmlNodeType.EndElement != reader.NodeType || !("Annotation" == reader.LocalName))
				{
					if ("Anchors" == reader.LocalName)
					{
						CheckForNonNamespaceAttribute(reader, "Anchors");
						if (!reader.IsEmptyElement)
						{
							reader.Read();
							while (!("Anchors" == reader.LocalName) || XmlNodeType.EndElement != reader.NodeType)
							{
								AnnotationResource item = (AnnotationResource)ResourceSerializer.Deserialize(reader);
								_anchors.Add(item);
							}
						}
						reader.Read();
						continue;
					}
					if ("Cargos" == reader.LocalName)
					{
						CheckForNonNamespaceAttribute(reader, "Cargos");
						if (!reader.IsEmptyElement)
						{
							reader.Read();
							while (!("Cargos" == reader.LocalName) || XmlNodeType.EndElement != reader.NodeType)
							{
								AnnotationResource item2 = (AnnotationResource)ResourceSerializer.Deserialize(reader);
								_cargos.Add(item2);
							}
						}
						reader.Read();
						continue;
					}
					if ("Authors" == reader.LocalName)
					{
						CheckForNonNamespaceAttribute(reader, "Authors");
						if (!reader.IsEmptyElement)
						{
							reader.Read();
							while (!("Authors" == reader.LocalName) || XmlNodeType.EndElement != reader.NodeType)
							{
								if (!("StringAuthor" == reader.LocalName) || XmlNodeType.Element != reader.NodeType)
								{
									throw new XmlException(SR.Format(SR.InvalidXmlContent, "Annotation"));
								}
								XmlNode xmlNode = xmlDocument.ReadNode(reader);
								if (!reader.IsEmptyElement)
								{
									_authors.Add(xmlNode.InnerText);
								}
							}
						}
						reader.Read();
						continue;
					}
					throw new XmlException(SR.Format(SR.InvalidXmlContent, "Annotation"));
				}
			}
			reader.Read();
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.DeserializeAnnotationEnd);
		}
	}

	internal static bool IsNamespaceDeclaration(XmlReader reader)
	{
		Invariant.Assert(reader != null);
		if (reader.NodeType == XmlNodeType.Attribute)
		{
			if (reader.Prefix.Length == 0)
			{
				if (reader.LocalName == "xmlns")
				{
					return true;
				}
			}
			else if (reader.Prefix == "xmlns" || reader.Prefix == "xml")
			{
				return true;
			}
		}
		return false;
	}

	internal static void CheckForNonNamespaceAttribute(XmlReader reader, string elementName)
	{
		Invariant.Assert(reader != null, "No reader supplied.");
		Invariant.Assert(elementName != null, "No element name supplied.");
		while (reader.MoveToNextAttribute())
		{
			if (!IsNamespaceDeclaration(reader))
			{
				throw new XmlException(SR.Format(SR.UnexpectedAttribute, reader.LocalName, elementName));
			}
		}
		reader.MoveToContent();
	}

	private void ReadAttributes(XmlReader reader)
	{
		Invariant.Assert(reader != null, "No reader passed in.");
		while (reader.MoveToNextAttribute())
		{
			string value = reader.Value;
			if (string.IsNullOrEmpty(value))
			{
				continue;
			}
			switch (reader.LocalName)
			{
			case "Id":
				_id = XmlConvert.ToGuid(value);
				break;
			case "CreationTime":
				_created = XmlConvert.ToDateTime(value);
				break;
			case "LastModificationTime":
				_modified = XmlConvert.ToDateTime(value);
				break;
			case "Type":
			{
				string[] array = value.Split(':');
				if (array.Length == 1)
				{
					array[0] = array[0].Trim();
					if (string.IsNullOrEmpty(array[0]))
					{
						throw new FormatException(SR.Format(SR.InvalidAttributeValue, "Type"));
					}
					_typeName = new XmlQualifiedName(array[0]);
					break;
				}
				if (array.Length == 2)
				{
					array[0] = array[0].Trim();
					array[1] = array[1].Trim();
					if (string.IsNullOrEmpty(array[0]) || string.IsNullOrEmpty(array[1]))
					{
						throw new FormatException(SR.Format(SR.InvalidAttributeValue, "Type"));
					}
					_typeName = new XmlQualifiedName(array[1], reader.LookupNamespace(array[0]));
					break;
				}
				throw new FormatException(SR.Format(SR.InvalidAttributeValue, "Type"));
			}
			default:
				if (!IsNamespaceDeclaration(reader))
				{
					throw new XmlException(SR.Format(SR.UnexpectedAttribute, reader.LocalName, "Annotation"));
				}
				break;
			}
		}
		if (_id.Equals(Guid.Empty))
		{
			throw new XmlException(SR.Format(SR.RequiredAttributeMissing, "Id", "Annotation"));
		}
		if (_created.Equals(DateTime.MinValue))
		{
			throw new XmlException(SR.Format(SR.RequiredAttributeMissing, "CreationTime", "Annotation"));
		}
		if (_modified.Equals(DateTime.MinValue))
		{
			throw new XmlException(SR.Format(SR.RequiredAttributeMissing, "LastModificationTime", "Annotation"));
		}
		if (_typeName == null)
		{
			throw new XmlException(SR.Format(SR.RequiredAttributeMissing, "Type", "Annotation"));
		}
		reader.MoveToContent();
	}

	private void OnCargoChanged(object sender, PropertyChangedEventArgs e)
	{
		FireResourceEvent((AnnotationResource)sender, AnnotationAction.Modified, this.CargoChanged);
	}

	private void OnCargosChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		AnnotationAction action = AnnotationAction.Added;
		IList list = null;
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			action = AnnotationAction.Added;
			list = e.NewItems;
			break;
		case NotifyCollectionChangedAction.Remove:
			action = AnnotationAction.Removed;
			list = e.OldItems;
			break;
		case NotifyCollectionChangedAction.Replace:
			foreach (AnnotationResource oldItem in e.OldItems)
			{
				FireResourceEvent(oldItem, AnnotationAction.Removed, this.CargoChanged);
			}
			list = e.NewItems;
			break;
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, e.Action));
		case NotifyCollectionChangedAction.Move:
		case NotifyCollectionChangedAction.Reset:
			break;
		}
		if (list == null)
		{
			return;
		}
		foreach (AnnotationResource item in list)
		{
			FireResourceEvent(item, action, this.CargoChanged);
		}
	}

	private void OnAnchorChanged(object sender, PropertyChangedEventArgs e)
	{
		FireResourceEvent((AnnotationResource)sender, AnnotationAction.Modified, this.AnchorChanged);
	}

	private void OnAnchorsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		AnnotationAction action = AnnotationAction.Added;
		IList list = null;
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			action = AnnotationAction.Added;
			list = e.NewItems;
			break;
		case NotifyCollectionChangedAction.Remove:
			action = AnnotationAction.Removed;
			list = e.OldItems;
			break;
		case NotifyCollectionChangedAction.Replace:
			foreach (AnnotationResource oldItem in e.OldItems)
			{
				FireResourceEvent(oldItem, AnnotationAction.Removed, this.AnchorChanged);
			}
			list = e.NewItems;
			break;
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, e.Action));
		case NotifyCollectionChangedAction.Move:
		case NotifyCollectionChangedAction.Reset:
			break;
		}
		if (list == null)
		{
			return;
		}
		foreach (AnnotationResource item in list)
		{
			FireResourceEvent(item, action, this.AnchorChanged);
		}
	}

	private void OnAuthorsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		AnnotationAction action = AnnotationAction.Added;
		IList list = null;
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			action = AnnotationAction.Added;
			list = e.NewItems;
			break;
		case NotifyCollectionChangedAction.Remove:
			action = AnnotationAction.Removed;
			list = e.OldItems;
			break;
		case NotifyCollectionChangedAction.Replace:
			foreach (string oldItem in e.OldItems)
			{
				FireAuthorEvent(oldItem, AnnotationAction.Removed);
			}
			list = e.NewItems;
			break;
		default:
			throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, e.Action));
		case NotifyCollectionChangedAction.Move:
		case NotifyCollectionChangedAction.Reset:
			break;
		}
		if (list == null)
		{
			return;
		}
		foreach (object item in list)
		{
			FireAuthorEvent(item, action);
		}
	}

	private void FireAuthorEvent(object author, AnnotationAction action)
	{
		Invariant.Assert(action >= AnnotationAction.Added && action <= AnnotationAction.Modified, "Unknown AnnotationAction");
		_modified = DateTime.Now;
		if (this.AuthorChanged != null)
		{
			this.AuthorChanged(this, new AnnotationAuthorChangedEventArgs(this, action, author));
		}
	}

	private void FireResourceEvent(AnnotationResource resource, AnnotationAction action, AnnotationResourceChangedEventHandler handlers)
	{
		Invariant.Assert(action >= AnnotationAction.Added && action <= AnnotationAction.Modified, "Unknown AnnotationAction");
		_modified = DateTime.Now;
		handlers?.Invoke(this, new AnnotationResourceChangedEventArgs(this, action, resource));
	}

	private void Init()
	{
		_cargos = new AnnotationResourceCollection();
		_cargos.ItemChanged += OnCargoChanged;
		_cargos.CollectionChanged += OnCargosChanged;
		_anchors = new AnnotationResourceCollection();
		_anchors.ItemChanged += OnAnchorChanged;
		_anchors.CollectionChanged += OnAnchorsChanged;
		_authors = new ObservableCollection<string>();
		_authors.CollectionChanged += OnAuthorsChanged;
	}
}
