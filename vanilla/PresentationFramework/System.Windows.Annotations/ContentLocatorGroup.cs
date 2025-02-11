using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MS.Internal;
using MS.Internal.Annotations;

namespace System.Windows.Annotations;

/// <summary>Represents an ordered set of <see cref="T:System.Windows.Annotations.ContentLocator" /> elements that identify an item of content.</summary>
[XmlRoot(Namespace = "http://schemas.microsoft.com/windows/annotations/2003/11/core", ElementName = "ContentLocatorGroup")]
public sealed class ContentLocatorGroup : ContentLocatorBase, IXmlSerializable
{
	private AnnotationObservableCollection<ContentLocator> _locators;

	/// <summary>Gets the collection of the <see cref="T:System.Windows.Annotations.ContentLocator" /> elements that make up this <see cref="T:System.Windows.Annotations.ContentLocatorGroup" />.</summary>
	/// <returns>The collection of <see cref="T:System.Windows.Annotations.ContentLocator" /> elements that make up this <see cref="T:System.Windows.Annotations.ContentLocatorGroup" />.</returns>
	public Collection<ContentLocator> Locators => _locators;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.ContentLocatorGroup" /> class.</summary>
	public ContentLocatorGroup()
	{
		_locators = new AnnotationObservableCollection<ContentLocator>();
		_locators.CollectionChanged += OnCollectionChanged;
	}

	/// <summary>Creates a modifiable deep copy clone of this <see cref="T:System.Windows.Annotations.ContentLocatorGroup" />.</summary>
	/// <returns>A modifiable deep copy clone of this <see cref="T:System.Windows.Annotations.ContentLocatorGroup" />.</returns>
	public override object Clone()
	{
		ContentLocatorGroup contentLocatorGroup = new ContentLocatorGroup();
		foreach (ContentLocator locator in _locators)
		{
			contentLocatorGroup.Locators.Add((ContentLocator)locator.Clone());
		}
		return contentLocatorGroup;
	}

	/// <summary>Always returns null.  See Annotations Schema for schema details.</summary>
	/// <returns>Always null.  See Annotations Schema for schema details.</returns>
	public XmlSchema GetSchema()
	{
		return null;
	}

	/// <summary>Serializes the <see cref="T:System.Windows.Annotations.ContentLocatorGroup" /> to a specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">The XML writer to use to serialize the <see cref="T:System.Windows.Annotations.ContentLocatorGroup" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> is null.</exception>
	public void WriteXml(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (writer.LookupPrefix("http://schemas.microsoft.com/windows/annotations/2003/11/core") == null)
		{
			writer.WriteAttributeString("xmlns", "anc", null, "http://schemas.microsoft.com/windows/annotations/2003/11/core");
		}
		foreach (ContentLocator locator in _locators)
		{
			if (locator != null)
			{
				AnnotationResource.ListSerializer.Serialize(writer, locator);
			}
		}
	}

	/// <summary>Deserializes the <see cref="T:System.Windows.Annotations.ContentLocatorGroup" /> from a specified <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="reader">The XML reader to use to deserialize the <see cref="T:System.Windows.Annotations.ContentLocatorGroup" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	/// <exception cref="T:System.Xml.XmlException">The serialized XML for the <see cref="T:System.Windows.Annotations.ContentLocatorGroup" /> is not valid.</exception>
	public void ReadXml(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Annotation.CheckForNonNamespaceAttribute(reader, "ContentLocatorGroup");
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (!("ContentLocatorGroup" == reader.LocalName) || XmlNodeType.EndElement != reader.NodeType)
			{
				if ("ContentLocator" == reader.LocalName)
				{
					ContentLocator item = (ContentLocator)AnnotationResource.ListSerializer.Deserialize(reader);
					_locators.Add(item);
					continue;
				}
				throw new XmlException(SR.Format(SR.InvalidXmlContent, "ContentLocatorGroup"));
			}
		}
		reader.Read();
	}

	internal override ContentLocatorBase Merge(ContentLocatorBase other)
	{
		if (other == null)
		{
			return this;
		}
		ContentLocator contentLocator = null;
		if (other is ContentLocatorGroup contentLocatorGroup)
		{
			List<ContentLocatorBase> list = new List<ContentLocatorBase>(contentLocatorGroup.Locators.Count * (Locators.Count - 1));
			foreach (ContentLocator locator in Locators)
			{
				foreach (ContentLocator locator2 in contentLocatorGroup.Locators)
				{
					if (contentLocator == null)
					{
						contentLocator = locator2;
						continue;
					}
					ContentLocator contentLocator2 = (ContentLocator)locator.Clone();
					contentLocator2.Append(locator2);
					list.Add(contentLocator2);
				}
				locator.Append(contentLocator);
				contentLocator = null;
			}
			foreach (ContentLocator item in list)
			{
				Locators.Add(item);
			}
		}
		else
		{
			ContentLocator contentLocator3 = other as ContentLocator;
			Invariant.Assert(contentLocator3 != null, "other should be of type ContentLocator");
			foreach (ContentLocator locator3 in Locators)
			{
				locator3.Append(contentLocator3);
			}
		}
		return this;
	}

	private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		FireLocatorChanged("Locators");
	}
}
