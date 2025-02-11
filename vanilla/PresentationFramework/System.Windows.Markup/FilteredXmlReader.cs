using System.Xml;

namespace System.Windows.Markup;

internal class FilteredXmlReader : XmlTextReader
{
	private const string uidLocalName = "Uid";

	private const string uidNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

	private const string defaultPrefix = "def";

	private string uidPrefix;

	private string uidQualifiedName;

	private bool haveUid;

	public override int AttributeCount
	{
		get
		{
			int attributeCount = base.AttributeCount;
			if (haveUid)
			{
				return attributeCount - 1;
			}
			return attributeCount;
		}
	}

	public override bool HasAttributes => AttributeCount != 0;

	public override string this[int attributeIndex] => GetAttribute(attributeIndex);

	public override string this[string attributeName] => GetAttribute(attributeName);

	public override string this[string localName, string namespaceUri] => GetAttribute(localName, namespaceUri);

	public override string GetAttribute(int attributeIndex)
	{
		throw new InvalidOperationException(SR.ParserFilterXmlReaderNoIndexAttributeAccess);
	}

	public override string GetAttribute(string attributeName)
	{
		if (attributeName == uidQualifiedName)
		{
			return null;
		}
		return base.GetAttribute(attributeName);
	}

	public override string GetAttribute(string localName, string namespaceUri)
	{
		if (localName == "Uid" && namespaceUri == "http://schemas.microsoft.com/winfx/2006/xaml")
		{
			return null;
		}
		return base.GetAttribute(localName, namespaceUri);
	}

	public override void MoveToAttribute(int attributeIndex)
	{
		throw new InvalidOperationException(SR.ParserFilterXmlReaderNoIndexAttributeAccess);
	}

	public override bool MoveToAttribute(string attributeName)
	{
		if (attributeName == uidQualifiedName)
		{
			return false;
		}
		return base.MoveToAttribute(attributeName);
	}

	public override bool MoveToAttribute(string localName, string namespaceUri)
	{
		if (localName == "Uid" && namespaceUri == "http://schemas.microsoft.com/winfx/2006/xaml")
		{
			return false;
		}
		return base.MoveToAttribute(localName, namespaceUri);
	}

	public override bool MoveToFirstAttribute()
	{
		bool previousSuccessValue = base.MoveToFirstAttribute();
		return CheckForUidOrNamespaceRedef(previousSuccessValue);
	}

	public override bool MoveToNextAttribute()
	{
		bool previousSuccessValue = base.MoveToNextAttribute();
		return CheckForUidOrNamespaceRedef(previousSuccessValue);
	}

	public override bool Read()
	{
		bool num = base.Read();
		if (num)
		{
			CheckForUidAttribute();
		}
		return num;
	}

	internal FilteredXmlReader(string xmlFragment, XmlNodeType fragmentType, XmlParserContext context)
		: base(xmlFragment, fragmentType, context)
	{
		haveUid = false;
		uidPrefix = "def";
		uidQualifiedName = uidPrefix + ":Uid";
	}

	private void CheckForUidAttribute()
	{
		if (base.GetAttribute(uidQualifiedName) != null)
		{
			haveUid = true;
		}
		else
		{
			haveUid = false;
		}
	}

	private bool CheckForUidOrNamespaceRedef(bool previousSuccessValue)
	{
		bool flag = previousSuccessValue;
		if (flag && base.LocalName == "Uid" && base.NamespaceURI == "http://schemas.microsoft.com/winfx/2006/xaml")
		{
			CheckForPrefixUpdate();
			flag = base.MoveToNextAttribute();
		}
		CheckForNamespaceRedef();
		return flag;
	}

	private void CheckForPrefixUpdate()
	{
		if (base.Prefix != uidPrefix)
		{
			uidPrefix = base.Prefix;
			uidQualifiedName = uidPrefix + ":Uid";
			CheckForUidAttribute();
		}
	}

	private void CheckForNamespaceRedef()
	{
		if (base.Prefix == "xmlns" && base.LocalName != uidPrefix && base.Value == "http://schemas.microsoft.com/winfx/2006/xaml")
		{
			throw new InvalidOperationException(SR.ParserFilterXmlReaderNoDefinitionPrefixChangeAllowed);
		}
	}
}
