using System;
using System.Globalization;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Xml;
using Microsoft.Win32;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

internal class CustomSignedXml : SignedXml
{
	private const string _NetFxSecurityFullKeyName = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\.NETFramework\\Security";

	private const string _NetFxSecurityKey = "SOFTWARE\\Microsoft\\.NETFramework\\Security";

	private static bool s_readRequireNCNameIdentifier = false;

	private static bool s_requireNCNameIdentifier = true;

	private static bool? s_allowAmbiguousReferenceTarget = null;

	private const string _XAdESNameSpace = "http://uri.etsi.org/01903/v1.2.2#";

	private const string _XAdESTargetType = "http://uri.etsi.org/01903/v1.2.2#SignedProperties";

	public override XmlElement GetIdElement(XmlDocument document, string idValue)
	{
		XmlElement xmlElement = base.GetIdElement(document, idValue);
		if (xmlElement == null)
		{
			if (RequireNCNameIdentifier())
			{
				try
				{
					XmlConvert.VerifyNCName(idValue);
				}
				catch (XmlException)
				{
					return null;
				}
			}
			xmlElement = SelectNodeByIdFromObjects(m_signature, idValue);
		}
		return xmlElement;
	}

	private static XmlElement SelectNodeByIdFromObjects(Signature signature, string idValue)
	{
		XmlElement xmlElement = null;
		foreach (DataObject @object in signature.ObjectList)
		{
			if (string.CompareOrdinal(idValue, @object.Id) == 0)
			{
				if (xmlElement != null)
				{
					throw new XmlException(SR.DuplicateObjectId);
				}
				xmlElement = @object.GetXml();
			}
		}
		if (xmlElement == null)
		{
			xmlElement = SelectSubObjectNodeForXAdES(signature, idValue);
		}
		return xmlElement;
	}

	private static XmlElement SelectSubObjectNodeForXAdES(Signature signature, string idValue)
	{
		XmlElement result = null;
		foreach (Reference reference in signature.SignedInfo.References)
		{
			if (string.CompareOrdinal(reference.Type, "http://uri.etsi.org/01903/v1.2.2#SignedProperties") == 0 && reference.Uri.Length > 0 && reference.Uri[0] == '#')
			{
				string strA = reference.Uri.Substring(1);
				if (string.CompareOrdinal(strA, idValue) == 0)
				{
					result = SelectSubObjectNodeForXAdESInDataObjects(signature, idValue);
					break;
				}
			}
		}
		return result;
	}

	private static XmlElement SelectSubObjectNodeForXAdESInDataObjects(Signature signature, string idValue)
	{
		XmlElement result = null;
		bool flag = false;
		foreach (DataObject @object in signature.ObjectList)
		{
			if (string.CompareOrdinal(@object.Id, XTable.Get(XTable.ID.OpcAttrValue)) == 0)
			{
				continue;
			}
			XmlNodeList xmlNodeList = @object.GetXml().SelectNodes(".//*[@Id='" + idValue + "']");
			if (xmlNodeList.Count <= 0)
			{
				continue;
			}
			if (!AllowAmbiguousReferenceTargets() && (xmlNodeList.Count > 1 || flag))
			{
				throw new XmlException(SR.DuplicateObjectId);
			}
			flag = true;
			XmlNode xmlNode = xmlNodeList[0] as XmlElement;
			if (xmlNode != null)
			{
				XmlNode xmlNode2 = xmlNode;
				while (xmlNode2 != null && xmlNode2.NamespaceURI.Length == 0)
				{
					xmlNode2 = xmlNode2.ParentNode;
				}
				if (xmlNode2 != null && string.CompareOrdinal(xmlNode2.NamespaceURI, "http://uri.etsi.org/01903/v1.2.2#") == 0)
				{
					result = xmlNode as XmlElement;
				}
			}
		}
		return result;
	}

	private static long GetNetFxSecurityRegistryValue(string regValueName, long defaultValue)
	{
		using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework\\Security", writable: false))
		{
			if (registryKey != null)
			{
				object value = registryKey.GetValue(regValueName);
				if (value != null)
				{
					RegistryValueKind valueKind = registryKey.GetValueKind(regValueName);
					if (valueKind == RegistryValueKind.DWord || valueKind == RegistryValueKind.QWord)
					{
						return Convert.ToInt64(value, CultureInfo.InvariantCulture);
					}
				}
			}
		}
		return defaultValue;
	}

	private static bool RequireNCNameIdentifier()
	{
		if (s_readRequireNCNameIdentifier)
		{
			return s_requireNCNameIdentifier;
		}
		s_requireNCNameIdentifier = GetNetFxSecurityRegistryValue("SignedXmlRequireNCNameIdentifier", 1L) != 0;
		Thread.MemoryBarrier();
		s_readRequireNCNameIdentifier = true;
		return s_requireNCNameIdentifier;
	}

	private static bool AllowAmbiguousReferenceTargets()
	{
		if (s_allowAmbiguousReferenceTarget.HasValue)
		{
			return s_allowAmbiguousReferenceTarget.Value;
		}
		bool value = GetNetFxSecurityRegistryValue("SignedXmlAllowAmbiguousReferenceTargets", 0L) != 0;
		s_allowAmbiguousReferenceTarget = value;
		return s_allowAmbiguousReferenceTarget.Value;
	}
}
