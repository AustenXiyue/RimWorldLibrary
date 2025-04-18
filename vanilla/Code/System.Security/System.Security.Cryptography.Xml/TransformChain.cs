using System.Collections;
using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml;

/// <summary>Defines an ordered list of <see cref="T:System.Security.Cryptography.Xml.Transform" /> objects that is applied to unsigned content prior to digest calculation.</summary>
public class TransformChain
{
	private ArrayList _transforms;

	/// <summary>Gets the number of transforms in the <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> object.</summary>
	/// <returns>The number of transforms in the <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> object.</returns>
	public int Count => _transforms.Count;

	/// <summary>Gets the transform at the specified index in the <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> object.</summary>
	/// <returns>The transform at the specified index in the <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> object.</returns>
	/// <param name="index">The index into the <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> object that specifies which transform to return. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="index" /> parameter is greater than the number of transforms.</exception>
	public Transform this[int index]
	{
		get
		{
			if (index >= _transforms.Count)
			{
				throw new ArgumentException("Index was out of range.  Must be non-negative and less than the size of the collection.", "index");
			}
			return (Transform)_transforms[index];
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> class.</summary>
	public TransformChain()
	{
		_transforms = new ArrayList();
	}

	/// <summary>Adds a transform to the list of transforms to be applied to the unsigned content prior to digest calculation.</summary>
	/// <param name="transform">The transform to add to the list of transforms. </param>
	public void Add(Transform transform)
	{
		if (transform != null)
		{
			_transforms.Add(transform);
		}
	}

	/// <summary>Returns an enumerator of the transforms in the <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> object.</summary>
	/// <returns>An enumerator of the transforms in the <see cref="T:System.Security.Cryptography.Xml.TransformChain" /> object.</returns>
	public IEnumerator GetEnumerator()
	{
		return _transforms.GetEnumerator();
	}

	internal Stream TransformToOctetStream(object inputObject, Type inputType, XmlResolver resolver, string baseUri)
	{
		object obj = inputObject;
		foreach (Transform transform in _transforms)
		{
			if (obj == null || transform.AcceptsType(obj.GetType()))
			{
				transform.Resolver = resolver;
				transform.BaseURI = baseUri;
				transform.LoadInput(obj);
				obj = transform.GetOutput();
			}
			else if (obj is Stream)
			{
				if (!transform.AcceptsType(typeof(XmlDocument)))
				{
					throw new CryptographicException("The input type was invalid for this transform.");
				}
				Stream obj2 = obj as Stream;
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.PreserveWhitespace = true;
				XmlReader reader = Utils.PreProcessStreamInput(obj2, resolver, baseUri);
				xmlDocument.Load(reader);
				transform.LoadInput(xmlDocument);
				obj2.Close();
				obj = transform.GetOutput();
			}
			else if (obj is XmlNodeList)
			{
				if (!transform.AcceptsType(typeof(Stream)))
				{
					throw new CryptographicException("The input type was invalid for this transform.");
				}
				MemoryStream memoryStream = new MemoryStream(new CanonicalXml((XmlNodeList)obj, resolver, includeComments: false).GetBytes());
				transform.LoadInput(memoryStream);
				obj = transform.GetOutput();
				memoryStream.Close();
			}
			else
			{
				if (!(obj is XmlDocument))
				{
					throw new CryptographicException("The input type was invalid for this transform.");
				}
				if (!transform.AcceptsType(typeof(Stream)))
				{
					throw new CryptographicException("The input type was invalid for this transform.");
				}
				MemoryStream memoryStream2 = new MemoryStream(new CanonicalXml((XmlDocument)obj, resolver).GetBytes());
				transform.LoadInput(memoryStream2);
				obj = transform.GetOutput();
				memoryStream2.Close();
			}
		}
		if (obj is Stream)
		{
			return obj as Stream;
		}
		if (obj is XmlNodeList)
		{
			return new MemoryStream(new CanonicalXml((XmlNodeList)obj, resolver, includeComments: false).GetBytes());
		}
		if (obj is XmlDocument)
		{
			return new MemoryStream(new CanonicalXml((XmlDocument)obj, resolver).GetBytes());
		}
		throw new CryptographicException("The input type was invalid for this transform.");
	}

	internal Stream TransformToOctetStream(Stream input, XmlResolver resolver, string baseUri)
	{
		return TransformToOctetStream(input, typeof(Stream), resolver, baseUri);
	}

	internal Stream TransformToOctetStream(XmlDocument document, XmlResolver resolver, string baseUri)
	{
		return TransformToOctetStream(document, typeof(XmlDocument), resolver, baseUri);
	}

	internal XmlElement GetXml(XmlDocument document, string ns)
	{
		XmlElement xmlElement = document.CreateElement("Transforms", ns);
		foreach (Transform transform in _transforms)
		{
			if (transform != null)
			{
				XmlElement xml = transform.GetXml(document);
				if (xml != null)
				{
					xmlElement.AppendChild(xml);
				}
			}
		}
		return xmlElement;
	}

	internal void LoadXml(XmlElement value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(value.OwnerDocument.NameTable);
		xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
		XmlNodeList xmlNodeList = value.SelectNodes("ds:Transform", xmlNamespaceManager);
		if (xmlNodeList.Count == 0)
		{
			throw new CryptographicException("Malformed element {0}.", "Transforms");
		}
		_transforms.Clear();
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			XmlElement xmlElement = (XmlElement)xmlNodeList.Item(i);
			if (!(CryptoHelpers.CreateFromName(Utils.GetAttribute(xmlElement, "Algorithm", "http://www.w3.org/2000/09/xmldsig#")) is Transform transform))
			{
				throw new CryptographicException("Unknown transform has been encountered.");
			}
			transform.LoadInnerXml(xmlElement.ChildNodes);
			_transforms.Add(transform);
		}
	}
}
