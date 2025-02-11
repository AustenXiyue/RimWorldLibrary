using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MS.Internal;
using MS.Internal.Annotations;

namespace System.Windows.Annotations;

/// <summary>Represents an ordered set of <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> elements that identify an item of content.</summary>
[XmlRoot(Namespace = "http://schemas.microsoft.com/windows/annotations/2003/11/core", ElementName = "ContentLocator")]
public sealed class ContentLocator : ContentLocatorBase, IXmlSerializable
{
	private AnnotationObservableCollection<ContentLocatorPart> _parts;

	/// <summary>Gets the collection of <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> elements that make up this <see cref="T:System.Windows.Annotations.ContentLocator" />.</summary>
	/// <returns>The collection of <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> elements that make up this <see cref="T:System.Windows.Annotations.ContentLocator" />.</returns>
	public Collection<ContentLocatorPart> Parts => _parts;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.ContentLocator" /> class.</summary>
	public ContentLocator()
	{
		_parts = new AnnotationObservableCollection<ContentLocatorPart>();
		_parts.CollectionChanged += OnCollectionChanged;
	}

	/// <summary>Returns a value that indicates whether the starting sequence of <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> elements in a specified <see cref="T:System.Windows.Annotations.ContentLocator" /> are identical to those in this <see cref="T:System.Windows.Annotations.ContentLocator" />.</summary>
	/// <returns>true if the starting sequence of <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> elements in this <see cref="T:System.Windows.Annotations.ContentLocator" /> matches those in the specified <paramref name="locator" />; otherwise, false.</returns>
	/// <param name="locator">The <see cref="T:System.Windows.Annotations.ContentLocator" /> with the list of <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> elements to compare with this <see cref="T:System.Windows.Annotations.ContentLocator" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="locator" /> is null.</exception>
	public bool StartsWith(ContentLocator locator)
	{
		if (locator == null)
		{
			throw new ArgumentNullException("locator");
		}
		Invariant.Assert(locator.Parts != null, "Locator has null Parts property.");
		if (Parts.Count < locator.Parts.Count)
		{
			return false;
		}
		for (int i = 0; i < locator.Parts.Count; i++)
		{
			ContentLocatorPart contentLocatorPart = locator.Parts[i];
			ContentLocatorPart contentLocatorPart2 = Parts[i];
			if (contentLocatorPart == null && contentLocatorPart2 != null)
			{
				return false;
			}
			if (!contentLocatorPart.Matches(contentLocatorPart2))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Creates a modifiable deep copy clone of this <see cref="T:System.Windows.Annotations.ContentLocator" />.</summary>
	/// <returns>A modifiable deep copy clone of this <see cref="T:System.Windows.Annotations.ContentLocator" />.</returns>
	public override object Clone()
	{
		ContentLocator contentLocator = new ContentLocator();
		ContentLocatorPart contentLocatorPart = null;
		foreach (ContentLocatorPart part in Parts)
		{
			contentLocatorPart = ((part == null) ? null : ((ContentLocatorPart)part.Clone()));
			contentLocator.Parts.Add(contentLocatorPart);
		}
		return contentLocator;
	}

	/// <summary>Always returns null.  See Annotations Schema for schema details.</summary>
	/// <returns>Always null.  See Annotations Schema for schema details</returns>
	public XmlSchema GetSchema()
	{
		return null;
	}

	/// <summary>Serializes the <see cref="T:System.Windows.Annotations.ContentLocator" /> to a specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">The XML writer to use to serialize the <see cref="T:System.Windows.Annotations.ContentLocator" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> is null.</exception>
	public void WriteXml(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		string text = writer.LookupPrefix("http://schemas.microsoft.com/windows/annotations/2003/11/core");
		if (text == null)
		{
			writer.WriteAttributeString("xmlns", "anc", null, "http://schemas.microsoft.com/windows/annotations/2003/11/core");
		}
		text = writer.LookupPrefix("http://schemas.microsoft.com/windows/annotations/2003/11/base");
		if (text == null)
		{
			writer.WriteAttributeString("xmlns", "anb", null, "http://schemas.microsoft.com/windows/annotations/2003/11/base");
		}
		foreach (ContentLocatorPart part in _parts)
		{
			text = writer.LookupPrefix(part.PartType.Namespace);
			if (string.IsNullOrEmpty(text))
			{
				text = "tmp";
			}
			writer.WriteStartElement(text, part.PartType.Name, part.PartType.Namespace);
			foreach (KeyValuePair<string, string> nameValuePair in part.NameValuePairs)
			{
				writer.WriteStartElement("Item", "http://schemas.microsoft.com/windows/annotations/2003/11/core");
				writer.WriteAttributeString("Name", nameValuePair.Key);
				writer.WriteAttributeString("Value", nameValuePair.Value);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
	}

	/// <summary>Deserializes the <see cref="T:System.Windows.Annotations.ContentLocator" /> from a specified <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="reader">The XML reader to use to deserialize the <see cref="T:System.Windows.Annotations.ContentLocator" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="reader" /> is null.</exception>
	/// <exception cref="T:System.Xml.XmlException">The serialized XML for the <see cref="T:System.Windows.Annotations.ContentLocator" /> is not valid.</exception>
	public void ReadXml(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Annotation.CheckForNonNamespaceAttribute(reader, "ContentLocator");
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (!("ContentLocator" == reader.LocalName) || XmlNodeType.EndElement != reader.NodeType)
			{
				if (XmlNodeType.Element != reader.NodeType)
				{
					throw new XmlException(SR.Format(SR.InvalidXmlContent, "ContentLocator"));
				}
				ContentLocatorPart contentLocatorPart = new ContentLocatorPart(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI));
				if (!reader.IsEmptyElement)
				{
					Annotation.CheckForNonNamespaceAttribute(reader, contentLocatorPart.PartType.Name);
					reader.Read();
					while (XmlNodeType.EndElement != reader.NodeType || !(contentLocatorPart.PartType.Name == reader.LocalName))
					{
						if ("Item" == reader.LocalName && reader.NamespaceURI == "http://schemas.microsoft.com/windows/annotations/2003/11/core")
						{
							string text = null;
							string text2 = null;
							while (reader.MoveToNextAttribute())
							{
								string localName = reader.LocalName;
								if (!(localName == "Name"))
								{
									if (localName == "Value")
									{
										text2 = reader.Value;
									}
									else if (!Annotation.IsNamespaceDeclaration(reader))
									{
										throw new XmlException(SR.Format(SR.UnexpectedAttribute, reader.LocalName, "Item"));
									}
								}
								else
								{
									text = reader.Value;
								}
							}
							if (text == null)
							{
								throw new XmlException(SR.Format(SR.RequiredAttributeMissing, "Name", "Item"));
							}
							if (text2 == null)
							{
								throw new XmlException(SR.Format(SR.RequiredAttributeMissing, "Value", "Item"));
							}
							reader.MoveToContent();
							contentLocatorPart.NameValuePairs.Add(text, text2);
							bool isEmptyElement = reader.IsEmptyElement;
							reader.Read();
							if (!isEmptyElement)
							{
								if (XmlNodeType.EndElement != reader.NodeType || !("Item" == reader.LocalName))
								{
									throw new XmlException(SR.Format(SR.InvalidXmlContent, "Item"));
								}
								reader.Read();
							}
							continue;
						}
						throw new XmlException(SR.Format(SR.InvalidXmlContent, contentLocatorPart.PartType.Name));
					}
				}
				_parts.Add(contentLocatorPart);
				reader.Read();
			}
		}
		reader.Read();
	}

	internal IList<ContentLocatorBase> DotProduct(IList<ContentLocatorPart> additionalLocatorParts)
	{
		List<ContentLocatorBase> list = null;
		if (additionalLocatorParts == null || additionalLocatorParts.Count == 0)
		{
			list = new List<ContentLocatorBase>(1);
			list.Add(this);
		}
		else
		{
			list = new List<ContentLocatorBase>(additionalLocatorParts.Count);
			for (int i = 1; i < additionalLocatorParts.Count; i++)
			{
				ContentLocator contentLocator = (ContentLocator)Clone();
				contentLocator.Parts.Add(additionalLocatorParts[i]);
				list.Add(contentLocator);
			}
			Parts.Add(additionalLocatorParts[0]);
			list.Insert(0, this);
		}
		return list;
	}

	internal override ContentLocatorBase Merge(ContentLocatorBase other)
	{
		if (other == null)
		{
			return this;
		}
		if (other is ContentLocatorGroup contentLocatorGroup)
		{
			ContentLocatorGroup contentLocatorGroup2 = new ContentLocatorGroup();
			ContentLocator contentLocator = null;
			foreach (ContentLocator locator in contentLocatorGroup.Locators)
			{
				if (contentLocator == null)
				{
					contentLocator = locator;
					continue;
				}
				ContentLocator contentLocator2 = (ContentLocator)Clone();
				contentLocator2.Append(locator);
				contentLocatorGroup2.Locators.Add(contentLocator2);
			}
			if (contentLocator != null)
			{
				Append(contentLocator);
				contentLocatorGroup2.Locators.Add(this);
			}
			if (contentLocatorGroup2.Locators.Count == 0)
			{
				return this;
			}
			return contentLocatorGroup2;
		}
		Append((ContentLocator)other);
		return this;
	}

	internal void Append(ContentLocator other)
	{
		Invariant.Assert(other != null, "Parameter 'other' is null.");
		foreach (ContentLocatorPart part in other.Parts)
		{
			Parts.Add((ContentLocatorPart)part.Clone());
		}
	}

	private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		FireLocatorChanged("Parts");
	}
}
