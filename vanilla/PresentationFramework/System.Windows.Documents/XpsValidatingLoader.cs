using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Windows.Markup;
using System.Xml;
using MS.Internal;
using MS.Internal.IO.Packaging;
using MS.Internal.IO.Packaging.Extensions;

namespace System.Windows.Documents;

internal class XpsValidatingLoader
{
	private static Stack<Hashtable> _validResources = new Stack<Hashtable>();

	private Hashtable _uniqueUriRef;

	private static bool _documentMode = false;

	private static string _requiredResourceRel = "http://schemas.microsoft.com/xps/2005/06/required-resource";

	private static XpsS0FixedPageSchema xpsS0FixedPageSchema = new XpsS0FixedPageSchema();

	private static XpsS0ResourceDictionarySchema xpsS0ResourceDictionarySchema = new XpsS0ResourceDictionarySchema();

	private static XpsDocStructSchema xpsDocStructSchema = new XpsDocStructSchema();

	internal static bool DocumentMode => _documentMode;

	internal XpsValidatingLoader()
	{
	}

	internal object Load(Stream stream, Uri parentUri, ParserContext pc, ContentType mimeType)
	{
		return Load(stream, parentUri, pc, mimeType, null);
	}

	internal void Validate(Stream stream, Uri parentUri, ParserContext pc, ContentType mimeType, string rootElement)
	{
		Load(stream, parentUri, pc, mimeType, rootElement);
	}

	private object Load(Stream stream, Uri parentUri, ParserContext pc, ContentType mimeType, string rootElement)
	{
		object result = null;
		List<Type> safeTypes = new List<Type> { typeof(ResourceDictionary) };
		if (!DocumentMode)
		{
			if (rootElement == null)
			{
				result = XamlReader.Load(XmlReader.Create(stream, null, pc), pc, XamlParseMode.Synchronous, useRestrictiveXamlReader: true, safeTypes);
				stream.Close();
			}
		}
		else
		{
			XpsSchema schema = XpsSchema.GetSchema(mimeType);
			Uri baseUri = pc.BaseUri;
			Uri packageUri = System.IO.Packaging.PackUriHelper.GetPackageUri(baseUri);
			Uri partUri = System.IO.Packaging.PackUriHelper.GetPartUri(baseUri);
			Package package = PreloadedPackages.GetPackage(packageUri);
			if (parentUri != null && !System.IO.Packaging.PackUriHelper.GetPackageUri(parentUri).Equals(packageUri))
			{
				throw new FileFormatException(SR.XpsValidatingLoaderUriNotInSamePackage);
			}
			schema.ValidateRelationships(new SecurityCriticalData<Package>(package), packageUri, partUri, mimeType);
			if (schema.AllowsMultipleReferencesToSameUri(mimeType))
			{
				_uniqueUriRef = null;
			}
			else
			{
				_uniqueUriRef = new Hashtable(11);
			}
			Hashtable hashtable = ((_validResources.Count > 0) ? _validResources.Peek() : null);
			if (schema.HasRequiredResources(mimeType))
			{
				hashtable = new Hashtable(11);
				foreach (System.IO.Packaging.PackageRelationship item in package.GetPart(partUri).GetRelationshipsByType(_requiredResourceRel))
				{
					Uri partUri2 = System.IO.Packaging.PackUriHelper.ResolvePartUri(partUri, item.TargetUri);
					Uri key = System.IO.Packaging.PackUriHelper.Create(packageUri, partUri2);
					PackagePart part = package.GetPart(partUri2);
					if (schema.IsValidRequiredResourceMimeType(part.ValidatedContentType()))
					{
						if (!hashtable.ContainsKey(key))
						{
							hashtable.Add(key, true);
						}
					}
					else if (!hashtable.ContainsKey(key))
					{
						hashtable.Add(key, false);
					}
				}
			}
			XpsSchemaValidator xpsSchemaValidator = new XpsSchemaValidator(this, schema, mimeType, stream, packageUri, partUri);
			_validResources.Push(hashtable);
			if (rootElement != null)
			{
				xpsSchemaValidator.XmlReader.MoveToContent();
				if (!rootElement.Equals(xpsSchemaValidator.XmlReader.Name))
				{
					throw new FileFormatException(SR.XpsValidatingLoaderUnsupportedMimeType);
				}
				while (xpsSchemaValidator.XmlReader.Read())
				{
				}
			}
			else
			{
				result = XamlReader.Load(xpsSchemaValidator.XmlReader, pc, XamlParseMode.Synchronous, useRestrictiveXamlReader: true, safeTypes);
			}
			_validResources.Pop();
		}
		return result;
	}

	internal static void AssertDocumentMode()
	{
		_documentMode = true;
	}

	internal void UriHitHandler(int node, Uri uri)
	{
		if (_uniqueUriRef != null)
		{
			if (_uniqueUriRef.Contains(uri))
			{
				if ((int)_uniqueUriRef[uri] != node)
				{
					throw new FileFormatException(SR.XpsValidatingLoaderDuplicateReference);
				}
			}
			else
			{
				_uniqueUriRef.Add(uri, node);
			}
		}
		Hashtable hashtable = _validResources.Peek();
		if (hashtable == null)
		{
			return;
		}
		if (!hashtable.ContainsKey(uri))
		{
			bool flag = false;
			foreach (Uri key in hashtable.Keys)
			{
				if (System.IO.Packaging.PackUriHelper.ComparePackUri(key, uri) == 0)
				{
					hashtable.Add(uri, hashtable[key]);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				throw new FileFormatException(SR.XpsValidatingLoaderUnlistedResource);
			}
		}
		if (!(bool)hashtable[uri])
		{
			throw new FileFormatException(SR.XpsValidatingLoaderUnsupportedMimeType);
		}
	}
}
