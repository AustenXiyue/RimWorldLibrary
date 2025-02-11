using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MS.Internal;
using MS.Internal.Annotations;

namespace System.Windows.Annotations;

/// <summary>Represents a content anchor or cargo resource for an <see cref="T:System.Windows.Annotations.Annotation" />.</summary>
[XmlRoot(Namespace = "http://schemas.microsoft.com/windows/annotations/2003/11/core", ElementName = "Resource")]
public sealed class AnnotationResource : IXmlSerializable, INotifyPropertyChanged2, INotifyPropertyChanged, IOwnedObject
{
	private Guid _id;

	private string _name;

	private AnnotationObservableCollection<ContentLocatorBase> _locators;

	private XmlElementCollection _contents;

	private static Serializer s_ListSerializer;

	private static Serializer s_LocatorGroupSerializer;

	private bool _owned;

	private PropertyChangedEventHandler _propertyChanged;

	/// <summary>Gets the GUID of this resource.</summary>
	/// <returns>The globally unique identifier (GUID) that identifies this resource.</returns>
	public Guid Id => _id;

	/// <summary>Gets or sets a name for this <see cref="T:System.Windows.Annotations.AnnotationResource" />.</summary>
	/// <returns>The name assigned to this <see cref="T:System.Windows.Annotations.AnnotationResource" /> to distinguish it from other <see cref="P:System.Windows.Annotations.Annotation.Anchors" /> or <see cref="P:System.Windows.Annotations.Annotation.Cargos" /> in the annotation.</returns>
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			bool flag = false;
			if (_name == null)
			{
				if (value != null)
				{
					flag = true;
				}
			}
			else if (!_name.Equals(value))
			{
				flag = true;
			}
			_name = value;
			if (flag)
			{
				FireResourceChanged("Name");
			}
		}
	}

	/// <summary>Gets the collection of <see cref="T:System.Windows.Annotations.ContentLocatorBase" /> elements contained in this resource.</summary>
	/// <returns>The collection of content locators contained in this resource.</returns>
	public Collection<ContentLocatorBase> ContentLocators => InternalLocators;

	/// <summary>Gets a collection of the <see cref="T:System.Xml.XmlElement" /> objects that define the content of this resource.</summary>
	/// <returns>The collection of the <see cref="T:System.Xml.XmlElement" /> objects that define the content of this resource.</returns>
	public Collection<XmlElement> Contents => InternalContents;

	bool IOwnedObject.Owned
	{
		get
		{
			return _owned;
		}
		set
		{
			_owned = value;
		}
	}

	internal static Serializer ListSerializer
	{
		get
		{
			if (s_ListSerializer == null)
			{
				s_ListSerializer = new Serializer(typeof(ContentLocator));
			}
			return s_ListSerializer;
		}
	}

	private AnnotationObservableCollection<ContentLocatorBase> InternalLocators
	{
		get
		{
			if (_locators == null)
			{
				_locators = new AnnotationObservableCollection<ContentLocatorBase>();
				_locators.CollectionChanged += OnLocatorsChanged;
			}
			return _locators;
		}
	}

	private XmlElementCollection InternalContents
	{
		get
		{
			if (_contents == null)
			{
				_contents = new XmlElementCollection();
				_contents.CollectionChanged += OnContentsChanged;
			}
			return _contents;
		}
	}

	private static Serializer LocatorGroupSerializer
	{
		get
		{
			if (s_LocatorGroupSerializer == null)
			{
				s_LocatorGroupSerializer = new Serializer(typeof(ContentLocatorGroup));
			}
			return s_LocatorGroupSerializer;
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			_propertyChanged = (PropertyChangedEventHandler)Delegate.Combine(_propertyChanged, value);
		}
		remove
		{
			_propertyChanged = (PropertyChangedEventHandler)Delegate.Remove(_propertyChanged, value);
		}
	}

	/// <summary>This constructor supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	public AnnotationResource()
	{
		_id = Guid.NewGuid();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.AnnotationResource" /> class with a specified name.</summary>
	/// <param name="name">A name to identify this resource from other <see cref="P:System.Windows.Annotations.Annotation.Anchors" /> and <see cref="P:System.Windows.Annotations.Annotation.Cargos" /> defined in the same annotation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	public AnnotationResource(string name)
		: this()
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		_name = name;
		_id = Guid.NewGuid();
	}

	/// <summary>This constructor supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="id">The globally unique identifier (GUID) that identifies this resource.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="id" /> is equal to Guid.Empty.</exception>
	public AnnotationResource(Guid id)
	{
		if (Guid.Empty.Equals(id))
		{
			throw new ArgumentException(SR.InvalidGuid, "id");
		}
		_id = id;
	}

	/// <summary>Always returns null.  See Annotations Schema for schema details.</summary>
	/// <returns>Always null.  See Annotations Schema for schema details.</returns>
	public XmlSchema GetSchema()
	{
		return null;
	}

	/// <summary>Serializes the <see cref="T:System.Windows.Annotations.AnnotationResource" /> to a specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">The XML writer to serialize the <see cref="T:System.Windows.Annotations.AnnotationResource" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Annotations.Annotation.AnnotationType" /> is not valid.</exception>
	public void WriteXml(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (string.IsNullOrEmpty(writer.LookupPrefix("http://schemas.microsoft.com/windows/annotations/2003/11/core")))
		{
			writer.WriteAttributeString("xmlns", "anc", null, "http://schemas.microsoft.com/windows/annotations/2003/11/core");
		}
		writer.WriteAttributeString("Id", XmlConvert.ToString(_id));
		if (_name != null)
		{
			writer.WriteAttributeString("Name", _name);
		}
		if (_locators != null)
		{
			foreach (ContentLocatorBase locator in _locators)
			{
				if (locator != null)
				{
					if (locator is ContentLocatorGroup)
					{
						LocatorGroupSerializer.Serialize(writer, locator);
					}
					else
					{
						ListSerializer.Serialize(writer, locator);
					}
				}
			}
		}
		if (_contents == null)
		{
			return;
		}
		foreach (XmlElement content in _contents)
		{
			content?.WriteTo(writer);
		}
	}

	/// <summary>Deserializes the <see cref="T:System.Windows.Annotations.AnnotationResource" /> from a specified <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="reader">The XML reader to deserialize the <see cref="T:System.Windows.Annotations.AnnotationResource" /> from.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	/// <exception cref="T:System.Xml.XmlException">The serialized XML for the <see cref="T:System.Windows.Annotations.AnnotationResource" /> is not valid.</exception>
	public void ReadXml(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		XmlDocument xmlDocument = new XmlDocument();
		ReadAttributes(reader);
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (!("Resource" == reader.LocalName) || XmlNodeType.EndElement != reader.NodeType)
			{
				if ("ContentLocatorGroup" == reader.LocalName)
				{
					ContentLocatorBase item = (ContentLocatorBase)LocatorGroupSerializer.Deserialize(reader);
					InternalLocators.Add(item);
					continue;
				}
				if ("ContentLocator" == reader.LocalName)
				{
					ContentLocatorBase item2 = (ContentLocatorBase)ListSerializer.Deserialize(reader);
					InternalLocators.Add(item2);
					continue;
				}
				if (XmlNodeType.Element == reader.NodeType)
				{
					XmlElement item3 = xmlDocument.ReadNode(reader) as XmlElement;
					InternalContents.Add(item3);
					continue;
				}
				throw new XmlException(SR.Format(SR.InvalidXmlContent, "Resource"));
			}
		}
		reader.Read();
	}

	private void ReadAttributes(XmlReader reader)
	{
		Invariant.Assert(reader != null, "No reader passed in.");
		Guid guid = Guid.Empty;
		while (reader.MoveToNextAttribute())
		{
			string value = reader.Value;
			if (value == null)
			{
				continue;
			}
			string localName = reader.LocalName;
			if (!(localName == "Id"))
			{
				if (localName == "Name")
				{
					_name = value;
				}
				else if (!Annotation.IsNamespaceDeclaration(reader))
				{
					throw new XmlException(SR.Format(SR.UnexpectedAttribute, reader.LocalName, "Resource"));
				}
			}
			else
			{
				guid = XmlConvert.ToGuid(value);
			}
		}
		if (Guid.Empty.Equals(guid))
		{
			throw new XmlException(SR.Format(SR.RequiredAttributeMissing, "Id", "Resource"));
		}
		_id = guid;
		reader.MoveToContent();
	}

	private void OnLocatorsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		FireResourceChanged("Locators");
	}

	private void OnContentsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		FireResourceChanged("Contents");
	}

	private void FireResourceChanged(string name)
	{
		if (_propertyChanged != null)
		{
			_propertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
