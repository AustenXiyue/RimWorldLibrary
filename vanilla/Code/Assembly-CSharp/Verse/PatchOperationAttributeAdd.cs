using System.Xml;

namespace Verse;

public class PatchOperationAttributeAdd : PatchOperationAttribute
{
	protected string value;

	protected override bool ApplyWorker(XmlDocument xml)
	{
		bool result = false;
		foreach (object item in xml.SelectNodes(xpath))
		{
			XmlNode xmlNode = item as XmlNode;
			if (xmlNode.Attributes[attribute] == null)
			{
				XmlAttribute xmlAttribute = xmlNode.OwnerDocument.CreateAttribute(attribute);
				xmlAttribute.Value = value;
				xmlNode.Attributes.Append(xmlAttribute);
				result = true;
			}
		}
		return result;
	}
}
