using System.Collections;

namespace System.Xml;

internal class XmlEmptyElementListEnumerator : IEnumerator
{
	public object Current => null;

	public XmlEmptyElementListEnumerator(XmlElementList list)
	{
	}

	public bool MoveNext()
	{
		return false;
	}

	public void Reset()
	{
	}
}
