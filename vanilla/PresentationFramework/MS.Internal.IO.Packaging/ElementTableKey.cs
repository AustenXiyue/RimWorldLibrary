using System;

namespace MS.Internal.IO.Packaging;

internal class ElementTableKey
{
	private string _baseName;

	private string _xmlNamespace;

	public static readonly string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	public static readonly string FixedMarkupNamespace = "http://schemas.microsoft.com/xps/2005/06";

	internal string XmlNamespace => _xmlNamespace;

	internal string BaseName => _baseName;

	internal ElementTableKey(string xmlNamespace, string baseName)
	{
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (baseName == null)
		{
			throw new ArgumentNullException("baseName");
		}
		_xmlNamespace = xmlNamespace;
		_baseName = baseName;
	}

	public override bool Equals(object other)
	{
		if (other == null)
		{
			return false;
		}
		if (other.GetType() != GetType())
		{
			return false;
		}
		ElementTableKey elementTableKey = (ElementTableKey)other;
		if (string.Equals(BaseName, elementTableKey.BaseName, StringComparison.Ordinal))
		{
			return string.Equals(XmlNamespace, elementTableKey.XmlNamespace, StringComparison.Ordinal);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return XmlNamespace.GetHashCode() ^ BaseName.GetHashCode();
	}
}
