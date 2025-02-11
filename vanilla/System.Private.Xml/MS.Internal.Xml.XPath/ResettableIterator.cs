using System.Xml.XPath;

namespace MS.Internal.Xml.XPath;

internal abstract class ResettableIterator : XPathNodeIterator
{
	public abstract override int CurrentPosition { get; }

	public ResettableIterator()
	{
		count = -1;
	}

	protected ResettableIterator(ResettableIterator other)
	{
		count = other.count;
	}

	protected void ResetCount()
	{
		count = -1;
	}

	public abstract void Reset();
}
