using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Xml;
using System.Xml.Schema;
using MS.Internal;

namespace System.Windows.Documents;

internal class XpsSchema
{
	private static readonly Dictionary<ContentType, XpsSchema> _schemas = new Dictionary<ContentType, XpsSchema>(new ContentType.StrongComparer());

	private Hashtable _requiredResourceMimeTypes = new Hashtable(11);

	public virtual string RootNamespaceUri => "";

	protected XpsSchema()
	{
	}

	protected static void RegisterSchema(XpsSchema schema, ContentType[] handledMimeTypes)
	{
		foreach (ContentType key in handledMimeTypes)
		{
			_schemas.Add(key, schema);
		}
	}

	protected void RegisterRequiredResourceMimeTypes(ContentType[] requiredResourceMimeTypes)
	{
		if (requiredResourceMimeTypes != null)
		{
			foreach (ContentType key in requiredResourceMimeTypes)
			{
				_requiredResourceMimeTypes.Add(key, true);
			}
		}
	}

	public virtual XmlReaderSettings GetXmlReaderSettings()
	{
		return new XmlReaderSettings
		{
			ValidationFlags = (XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessIdentityConstraints)
		};
	}

	public virtual void ValidateRelationships(SecurityCriticalData<Package> package, Uri packageUri, Uri partUri, ContentType mimeType)
	{
	}

	public virtual bool HasRequiredResources(ContentType mimeType)
	{
		return false;
	}

	public virtual bool HasUriAttributes(ContentType mimeType)
	{
		return false;
	}

	public virtual bool AllowsMultipleReferencesToSameUri(ContentType mimeType)
	{
		return true;
	}

	public virtual bool IsValidRootNamespaceUri(string namespaceUri)
	{
		return false;
	}

	public bool IsValidRequiredResourceMimeType(ContentType mimeType)
	{
		foreach (ContentType key in _requiredResourceMimeTypes.Keys)
		{
			if (key.AreTypeAndSubTypeEqual(mimeType))
			{
				return true;
			}
		}
		return false;
	}

	public virtual string[] ExtractUriFromAttr(string attrName, string attrValue)
	{
		return null;
	}

	public static XpsSchema GetSchema(ContentType mimeType)
	{
		XpsSchema value = null;
		if (!_schemas.TryGetValue(mimeType, out value))
		{
			throw new FileFormatException(SR.XpsValidatingLoaderUnsupportedMimeType);
		}
		return value;
	}
}
