using System.ComponentModel;
using System.Xml.XPath;

namespace System.Xml.Xsl.Runtime;

[EditorBrowsable(EditorBrowsableState.Never)]
public struct AttributeContentIterator
{
	private XPathNavigator navCurrent;

	private bool needFirst;

	public XPathNavigator Current => navCurrent;

	public void Create(XPathNavigator context)
	{
		navCurrent = XmlQueryRuntime.SyncToNavigator(navCurrent, context);
		needFirst = true;
	}

	public bool MoveNext()
	{
		if (needFirst)
		{
			needFirst = !XmlNavNeverFilter.MoveToFirstAttributeContent(navCurrent);
			return !needFirst;
		}
		return XmlNavNeverFilter.MoveToNextAttributeContent(navCurrent);
	}
}
