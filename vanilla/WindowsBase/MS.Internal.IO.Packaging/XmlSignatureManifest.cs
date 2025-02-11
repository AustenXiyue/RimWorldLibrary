using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography;
using System.Xml;
using MS.Internal.IO.Packaging.Extensions;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

internal static class XmlSignatureManifest
{
	private const string _contentTypeQueryStringPrefix = "?ContentType=";

	internal static void ParseManifest(PackageDigitalSignatureManager manager, XmlReader reader, out List<Uri> partManifest, out List<PartManifestEntry> partEntryManifest, out List<PackageRelationshipSelector> relationshipManifest)
	{
		Invariant.Assert(manager != null);
		Invariant.Assert(reader != null);
		partManifest = new List<Uri>();
		partEntryManifest = new List<PartManifestEntry>();
		relationshipManifest = new List<PackageRelationshipSelector>();
		string strB = XTable.Get(XTable.ID.ReferenceTagName);
		int num = 0;
		while (reader.Read() && reader.MoveToContent() == XmlNodeType.Element)
		{
			if (string.CompareOrdinal(reader.NamespaceURI, "http://www.w3.org/2000/09/xmldsig#") == 0 && string.CompareOrdinal(reader.LocalName, strB) == 0 && reader.Depth == 2)
			{
				PartManifestEntry item = ParseReference(reader);
				if (item.IsRelationshipEntry)
				{
					foreach (PackageRelationshipSelector relationshipSelector in item.RelationshipSelectors)
					{
						relationshipManifest.Add(relationshipSelector);
					}
				}
				else
				{
					partManifest.Add(item.Uri);
				}
				partEntryManifest.Add(item);
				num++;
				continue;
			}
			throw new XmlException(SR.Format(SR.UnexpectedXmlTag, reader.Name));
		}
		if (num == 0)
		{
			throw new XmlException(SR.PackageSignatureCorruption);
		}
	}

	private static string ParseDigestAlgorithmTag(XmlReader reader)
	{
		if (PackagingUtilities.GetNonXmlnsAttributeCount(reader) > 1 || string.CompareOrdinal(reader.NamespaceURI, "http://www.w3.org/2000/09/xmldsig#") != 0 || reader.Depth != 3)
		{
			throw new XmlException(SR.XmlSignatureParseError);
		}
		string text = null;
		if (reader.HasAttributes)
		{
			text = reader.GetAttribute(XTable.Get(XTable.ID.AlgorithmAttrName));
		}
		if (text == null || text.Length == 0)
		{
			throw new XmlException(SR.UnsupportedHashAlgorithm);
		}
		return text;
	}

	private static string ParseDigestValueTag(XmlReader reader)
	{
		if (PackagingUtilities.GetNonXmlnsAttributeCount(reader) > 0 || string.CompareOrdinal(reader.NamespaceURI, "http://www.w3.org/2000/09/xmldsig#") != 0 || reader.Depth != 3)
		{
			throw new XmlException(SR.XmlSignatureParseError);
		}
		if (reader.HasAttributes || (reader.Read() && reader.MoveToContent() != XmlNodeType.Text))
		{
			throw new XmlException(SR.XmlSignatureParseError);
		}
		return reader.ReadString();
	}

	private static Uri ParsePartUri(XmlReader reader, out ContentType contentType)
	{
		contentType = ContentType.Empty;
		Uri uri = null;
		if (PackagingUtilities.GetNonXmlnsAttributeCount(reader) == 1)
		{
			string attribute = reader.GetAttribute(XTable.Get(XTable.ID.UriAttrName));
			if (attribute != null)
			{
				uri = ParsePartUriAttribute(attribute, out contentType);
			}
		}
		if (uri == null)
		{
			throw new XmlException(SR.Format(SR.RequiredXmlAttributeMissing, XTable.Get(XTable.ID.UriAttrName)));
		}
		return uri;
	}

	private static PartManifestEntry ParseReference(XmlReader reader)
	{
		ContentType contentType = null;
		Uri uri = ParsePartUri(reader, out contentType);
		List<PackageRelationshipSelector> relationshipSelectors = null;
		string text = null;
		string text2 = null;
		List<string> transforms = null;
		bool flag = false;
		while (reader.Read() && reader.MoveToContent() == XmlNodeType.Element)
		{
			if (string.CompareOrdinal(reader.NamespaceURI, "http://www.w3.org/2000/09/xmldsig#") != 0 || reader.Depth != 3)
			{
				throw new XmlException(SR.PackageSignatureCorruption);
			}
			if (text == null && string.CompareOrdinal(reader.LocalName, XTable.Get(XTable.ID.DigestMethodTagName)) == 0)
			{
				text = ParseDigestAlgorithmTag(reader);
				continue;
			}
			if (text2 == null && string.CompareOrdinal(reader.LocalName, XTable.Get(XTable.ID.DigestValueTagName)) == 0)
			{
				text2 = ParseDigestValueTag(reader);
				continue;
			}
			if (!flag && string.CompareOrdinal(reader.LocalName, XTable.Get(XTable.ID.TransformsTagName)) == 0)
			{
				transforms = ParseTransformsTag(reader, uri, ref relationshipSelectors);
				flag = true;
				continue;
			}
			throw new XmlException(SR.PackageSignatureCorruption);
		}
		return new PartManifestEntry(uri, contentType, text, text2, transforms, relationshipSelectors);
	}

	private static List<string> ParseTransformsTag(XmlReader reader, Uri partUri, ref List<PackageRelationshipSelector> relationshipSelectors)
	{
		if (PackagingUtilities.GetNonXmlnsAttributeCount(reader) != 0)
		{
			throw new XmlException(SR.XmlSignatureParseError);
		}
		List<string> list = null;
		bool flag = false;
		int num = 0;
		while (reader.Read() && reader.MoveToContent() == XmlNodeType.Element)
		{
			string text = null;
			if (reader.Depth != 4 || string.CompareOrdinal(reader.NamespaceURI, "http://www.w3.org/2000/09/xmldsig#") != 0 || string.CompareOrdinal(reader.LocalName, XTable.Get(XTable.ID.TransformTagName)) != 0)
			{
				throw new XmlException(SR.XmlSignatureParseError);
			}
			if (PackagingUtilities.GetNonXmlnsAttributeCount(reader) == 1)
			{
				text = reader.GetAttribute(XTable.Get(XTable.ID.AlgorithmAttrName));
			}
			if (text != null && text.Length > 0)
			{
				if (string.CompareOrdinal(text, XTable.Get(XTable.ID.RelationshipsTransformName)) == 0)
				{
					if (!flag)
					{
						ParseRelationshipsTransform(reader, partUri, ref relationshipSelectors);
						if (list == null)
						{
							list = new List<string>();
						}
						list.Add(text);
						flag = true;
						num = list.Count;
						continue;
					}
					throw new XmlException(SR.MultipleRelationshipTransformsFound);
				}
				if (reader.IsEmptyElement)
				{
					if (list == null)
					{
						list = new List<string>();
					}
					if (XmlDigitalSignatureProcessor.IsValidXmlCanonicalizationTransform(text))
					{
						list.Add(text);
						continue;
					}
					throw new InvalidOperationException(SR.UnsupportedTransformAlgorithm);
				}
			}
			throw new XmlException(SR.XmlSignatureParseError);
		}
		if (list.Count == 0)
		{
			throw new XmlException(SR.XmlSignatureParseError);
		}
		if (flag && list.Count == num)
		{
			throw new XmlException(SR.RelationshipTransformNotFollowedByCanonicalizationTransform);
		}
		return list;
	}

	private static void ParseRelationshipsTransform(XmlReader reader, Uri partUri, ref List<PackageRelationshipSelector> relationshipSelectors)
	{
		Uri sourcePartUriFromRelationshipPartUri = System.IO.Packaging.PackUriHelper.GetSourcePartUriFromRelationshipPartUri(partUri);
		while (reader.Read() && reader.MoveToContent() == XmlNodeType.Element && reader.Depth == 5)
		{
			if (reader.IsEmptyElement && PackagingUtilities.GetNonXmlnsAttributeCount(reader) == 1 && string.CompareOrdinal(reader.NamespaceURI, XTable.Get(XTable.ID.OpcSignatureNamespace)) == 0)
			{
				if (string.CompareOrdinal(reader.LocalName, XTable.Get(XTable.ID.RelationshipReferenceTagName)) == 0)
				{
					string attribute = reader.GetAttribute(XTable.Get(XTable.ID.SourceIdAttrName));
					if (attribute != null && attribute.Length > 0)
					{
						if (relationshipSelectors == null)
						{
							relationshipSelectors = new List<PackageRelationshipSelector>();
						}
						relationshipSelectors.Add(new PackageRelationshipSelector(sourcePartUriFromRelationshipPartUri, PackageRelationshipSelectorType.Id, attribute));
						continue;
					}
				}
				else if (string.CompareOrdinal(reader.LocalName, XTable.Get(XTable.ID.RelationshipsGroupReferenceTagName)) == 0)
				{
					string attribute2 = reader.GetAttribute(XTable.Get(XTable.ID.SourceTypeAttrName));
					if (attribute2 != null && attribute2.Length > 0)
					{
						if (relationshipSelectors == null)
						{
							relationshipSelectors = new List<PackageRelationshipSelector>();
						}
						relationshipSelectors.Add(new PackageRelationshipSelector(sourcePartUriFromRelationshipPartUri, PackageRelationshipSelectorType.Type, attribute2));
						continue;
					}
				}
			}
			throw new XmlException(SR.Format(SR.UnexpectedXmlTag, reader.LocalName));
		}
	}

	internal static XmlNode GenerateManifest(PackageDigitalSignatureManager manager, XmlDocument xDoc, HashAlgorithm hashAlgorithm, IEnumerable<Uri> parts, IEnumerable<PackageRelationshipSelector> relationshipSelectors)
	{
		if (!hashAlgorithm.CanReuseTransform)
		{
			throw new ArgumentException(SR.HashAlgorithmMustBeReusable);
		}
		XmlNode xmlNode = xDoc.CreateNode(XmlNodeType.Element, XTable.Get(XTable.ID.ManifestTagName), "http://www.w3.org/2000/09/xmldsig#");
		if (parts != null)
		{
			foreach (Uri part in parts)
			{
				xmlNode.AppendChild(GeneratePartSigningReference(manager, xDoc, hashAlgorithm, part));
			}
		}
		int num = 0;
		if (relationshipSelectors != null)
		{
			num = GenerateRelationshipSigningReferences(manager, xDoc, hashAlgorithm, relationshipSelectors, xmlNode);
		}
		if (parts == null && num == 0)
		{
			throw new ArgumentException(SR.NothingToSign);
		}
		return xmlNode;
	}

	private static int GenerateRelationshipSigningReferences(PackageDigitalSignatureManager manager, XmlDocument xDoc, HashAlgorithm hashAlgorithm, IEnumerable<PackageRelationshipSelector> relationshipSelectors, XmlNode manifest)
	{
		Dictionary<Uri, List<PackageRelationshipSelector>> dictionary = new Dictionary<Uri, List<PackageRelationshipSelector>>();
		foreach (PackageRelationshipSelector relationshipSelector in relationshipSelectors)
		{
			Uri relationshipPartUri = System.IO.Packaging.PackUriHelper.GetRelationshipPartUri(relationshipSelector.SourceUri);
			List<PackageRelationshipSelector> list;
			if (dictionary.ContainsKey(relationshipPartUri))
			{
				list = dictionary[relationshipPartUri];
			}
			else
			{
				list = new List<PackageRelationshipSelector>();
				dictionary.Add(relationshipPartUri, list);
			}
			list.Add(relationshipSelector);
		}
		((XmlElement)manifest).SetAttribute(XTable.Get(XTable.ID.OpcSignatureNamespaceAttribute), XTable.Get(XTable.ID.OpcSignatureNamespace));
		int num = 0;
		foreach (Uri key in dictionary.Keys)
		{
			manifest.AppendChild(GenerateRelationshipSigningReference(manager, xDoc, hashAlgorithm, key, dictionary[key]));
			num++;
		}
		return num;
	}

	private static Uri ParsePartUriAttribute(string attrValue, out ContentType contentType)
	{
		contentType = ContentType.Empty;
		int num = attrValue.IndexOf('?');
		Uri result = null;
		if (num > 0)
		{
			try
			{
				string text = attrValue.Substring(num);
				if (text.Length > "?ContentType=".Length && text.StartsWith("?ContentType=", StringComparison.Ordinal))
				{
					contentType = new ContentType(text.Substring("?ContentType=".Length));
				}
				result = PackUriHelper.ValidatePartUri(new Uri(attrValue.Substring(0, num), UriKind.Relative));
			}
			catch (ArgumentException innerException)
			{
				throw new XmlException(SR.PartReferenceUriMalformed, innerException);
			}
		}
		if (contentType.ToString().Length <= 0)
		{
			throw new XmlException(SR.PartReferenceUriMalformed);
		}
		return result;
	}

	private static XmlNode GenerateRelationshipSigningReference(PackageDigitalSignatureManager manager, XmlDocument xDoc, HashAlgorithm hashAlgorithm, Uri relationshipPartName, IEnumerable<PackageRelationshipSelector> relationshipSelectors)
	{
		string text = PackagingUtilities.RelationshipPartContentType.ToString();
		XmlElement xmlElement = xDoc.CreateElement(XTable.Get(XTable.ID.ReferenceTagName), "http://www.w3.org/2000/09/xmldsig#");
		string text2 = ((System.IO.Packaging.PackUriHelper.ComparePartUri(relationshipPartName, MS.Internal.IO.Packaging.Extensions.PackageRelationship.ContainerRelationshipPartName) != 0) ? PackUriHelper.GetStringForPartUri(relationshipPartName) : MS.Internal.IO.Packaging.Extensions.PackageRelationship.ContainerRelationshipPartName.ToString());
		XmlAttribute xmlAttribute = xDoc.CreateAttribute(XTable.Get(XTable.ID.UriAttrName));
		xmlAttribute.Value = text2 + "?ContentType=" + text;
		xmlElement.Attributes.Append(xmlAttribute);
		XmlElement xmlElement2 = xDoc.CreateElement(XTable.Get(XTable.ID.TransformsTagName), "http://www.w3.org/2000/09/xmldsig#");
		string namespaceURI = XTable.Get(XTable.ID.OpcSignatureNamespace);
		string prefix = XTable.Get(XTable.ID.OpcSignatureNamespacePrefix);
		XmlElement xmlElement3 = xDoc.CreateElement(XTable.Get(XTable.ID.TransformTagName), "http://www.w3.org/2000/09/xmldsig#");
		XmlAttribute xmlAttribute2 = xDoc.CreateAttribute(XTable.Get(XTable.ID.AlgorithmAttrName));
		xmlAttribute2.Value = XTable.Get(XTable.ID.RelationshipsTransformName);
		xmlElement3.Attributes.Append(xmlAttribute2);
		foreach (PackageRelationshipSelector relationshipSelector in relationshipSelectors)
		{
			switch (relationshipSelector.SelectorType)
			{
			case PackageRelationshipSelectorType.Id:
			{
				XmlNode xmlNode2 = xDoc.CreateElement(prefix, XTable.Get(XTable.ID.RelationshipReferenceTagName), namespaceURI);
				XmlAttribute xmlAttribute4 = xDoc.CreateAttribute(XTable.Get(XTable.ID.SourceIdAttrName));
				xmlAttribute4.Value = relationshipSelector.SelectionCriteria;
				xmlNode2.Attributes.Append(xmlAttribute4);
				xmlElement3.AppendChild(xmlNode2);
				break;
			}
			case PackageRelationshipSelectorType.Type:
			{
				XmlNode xmlNode = xDoc.CreateElement(prefix, XTable.Get(XTable.ID.RelationshipsGroupReferenceTagName), namespaceURI);
				XmlAttribute xmlAttribute3 = xDoc.CreateAttribute(XTable.Get(XTable.ID.SourceTypeAttrName));
				xmlAttribute3.Value = relationshipSelector.SelectionCriteria;
				xmlNode.Attributes.Append(xmlAttribute3);
				xmlElement3.AppendChild(xmlNode);
				break;
			}
			default:
				Invariant.Assert(condition: false, "This option should never be executed");
				break;
			}
		}
		xmlElement2.AppendChild(xmlElement3);
		string text3 = null;
		if (manager.TransformMapping.ContainsKey(text))
		{
			text3 = manager.TransformMapping[text];
			if (text3 == null || text3.Length == 0 || !XmlDigitalSignatureProcessor.IsValidXmlCanonicalizationTransform(text3))
			{
				throw new InvalidOperationException(SR.UnsupportedTransformAlgorithm);
			}
			xmlElement3 = xDoc.CreateElement(XTable.Get(XTable.ID.TransformTagName), "http://www.w3.org/2000/09/xmldsig#");
			xmlAttribute2 = xDoc.CreateAttribute(XTable.Get(XTable.ID.AlgorithmAttrName));
			xmlAttribute2.Value = text3;
			xmlElement3.Attributes.Append(xmlAttribute2);
			xmlElement2.AppendChild(xmlElement3);
		}
		xmlElement.AppendChild(xmlElement2);
		xmlElement.AppendChild(GenerateDigestMethod(manager, xDoc));
		using Stream s = XmlDigitalSignatureProcessor.GenerateRelationshipNodeStream(GetRelationships(manager, relationshipSelectors));
		xmlElement.AppendChild(GenerateDigestValueNode(xDoc, hashAlgorithm, s, text3));
		return xmlElement;
	}

	private static XmlNode GeneratePartSigningReference(PackageDigitalSignatureManager manager, XmlDocument xDoc, HashAlgorithm hashAlgorithm, Uri partName)
	{
		PackagePart part = manager.Package.GetPart(partName);
		XmlElement xmlElement = xDoc.CreateElement(XTable.Get(XTable.ID.ReferenceTagName), "http://www.w3.org/2000/09/xmldsig#");
		XmlAttribute xmlAttribute = xDoc.CreateAttribute(XTable.Get(XTable.ID.UriAttrName));
		xmlAttribute.Value = PackUriHelper.GetStringForPartUri(partName) + "?ContentType=" + part.ContentType;
		xmlElement.Attributes.Append(xmlAttribute);
		string text = string.Empty;
		if (manager.TransformMapping.ContainsKey(part.ContentType))
		{
			text = manager.TransformMapping[part.ContentType];
			XmlElement xmlElement2 = xDoc.CreateElement(XTable.Get(XTable.ID.TransformsTagName), "http://www.w3.org/2000/09/xmldsig#");
			XmlElement xmlElement3 = xDoc.CreateElement(XTable.Get(XTable.ID.TransformTagName), "http://www.w3.org/2000/09/xmldsig#");
			XmlAttribute xmlAttribute2 = xDoc.CreateAttribute(XTable.Get(XTable.ID.AlgorithmAttrName));
			xmlAttribute2.Value = text;
			xmlElement3.Attributes.Append(xmlAttribute2);
			xmlElement2.AppendChild(xmlElement3);
			xmlElement.AppendChild(xmlElement2);
		}
		xmlElement.AppendChild(GenerateDigestMethod(manager, xDoc));
		using Stream s = part.GetSeekableStream(FileMode.Open, FileAccess.Read);
		xmlElement.AppendChild(GenerateDigestValueNode(xDoc, hashAlgorithm, s, text));
		return xmlElement;
	}

	private static XmlNode GenerateDigestMethod(PackageDigitalSignatureManager manager, XmlDocument xDoc)
	{
		XmlElement xmlElement = xDoc.CreateElement(XTable.Get(XTable.ID.DigestMethodTagName), "http://www.w3.org/2000/09/xmldsig#");
		XmlAttribute xmlAttribute = xDoc.CreateAttribute(XTable.Get(XTable.ID.AlgorithmAttrName));
		xmlAttribute.Value = manager.HashAlgorithm;
		xmlElement.Attributes.Append(xmlAttribute);
		return xmlElement;
	}

	private static XmlNode GenerateDigestValueNode(XmlDocument xDoc, HashAlgorithm hashAlgorithm, Stream s, string transformName)
	{
		XmlElement xmlElement = xDoc.CreateElement(XTable.Get(XTable.ID.DigestValueTagName), "http://www.w3.org/2000/09/xmldsig#");
		XmlText newChild = xDoc.CreateTextNode(XmlDigitalSignatureProcessor.GenerateDigestValue(s, transformName, hashAlgorithm));
		xmlElement.AppendChild(newChild);
		return xmlElement;
	}

	private static IEnumerable<System.IO.Packaging.PackageRelationship> GetRelationships(PackageDigitalSignatureManager manager, IEnumerable<PackageRelationshipSelector> relationshipSelectorsWithSameSource)
	{
		SortedDictionary<string, System.IO.Packaging.PackageRelationship> sortedDictionary = new SortedDictionary<string, System.IO.Packaging.PackageRelationship>(StringComparer.Ordinal);
		foreach (PackageRelationshipSelector item in relationshipSelectorsWithSameSource)
		{
			foreach (System.IO.Packaging.PackageRelationship item2 in item.Select(manager.Package))
			{
				if (!sortedDictionary.ContainsKey(item2.Id))
				{
					sortedDictionary.Add(item2.Id, item2);
				}
			}
		}
		return sortedDictionary.Values;
	}
}
