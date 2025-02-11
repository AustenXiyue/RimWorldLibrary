using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace MS.Internal.Xml.XPath;

internal sealed class XPathMultyIterator : ResettableIterator
{
	private readonly ResettableIterator[] arr;

	private int firstNotEmpty;

	private int position;

	public override XPathNavigator Current => arr[firstNotEmpty].Current;

	public override int CurrentPosition => position;

	public XPathMultyIterator(ArrayList inputArray)
	{
		arr = new ResettableIterator[inputArray.Count];
		for (int i = 0; i < arr.Length; i++)
		{
			ArrayList list = (ArrayList)inputArray[i];
			arr[i] = new XPathArrayIterator(list);
		}
		Init();
	}

	private void Init()
	{
		for (int i = 0; i < arr.Length; i++)
		{
			Advance(i);
		}
		int num = arr.Length - 2;
		while (firstNotEmpty <= num)
		{
			if (SiftItem(num))
			{
				num--;
			}
		}
	}

	private bool Advance(int pos)
	{
		if (!arr[pos].MoveNext())
		{
			if (firstNotEmpty != pos)
			{
				ResettableIterator resettableIterator = arr[pos];
				Array.Copy(arr, firstNotEmpty, arr, firstNotEmpty + 1, pos - firstNotEmpty);
				arr[firstNotEmpty] = resettableIterator;
			}
			firstNotEmpty++;
			return false;
		}
		return true;
	}

	private bool SiftItem(int item)
	{
		ResettableIterator resettableIterator = arr[item];
		while (item + 1 < arr.Length)
		{
			ResettableIterator resettableIterator2 = arr[item + 1];
			switch (Query.CompareNodes(resettableIterator.Current, resettableIterator2.Current))
			{
			case XmlNodeOrder.After:
				arr[item] = resettableIterator2;
				item++;
				continue;
			default:
				arr[item] = resettableIterator;
				if (!Advance(item))
				{
					return false;
				}
				resettableIterator = arr[item];
				continue;
			case XmlNodeOrder.Before:
				break;
			}
			break;
		}
		arr[item] = resettableIterator;
		return true;
	}

	public override void Reset()
	{
		firstNotEmpty = 0;
		position = 0;
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i].Reset();
		}
		Init();
	}

	public XPathMultyIterator(XPathMultyIterator it)
	{
		arr = (ResettableIterator[])it.arr.Clone();
		firstNotEmpty = it.firstNotEmpty;
		position = it.position;
	}

	public override XPathNodeIterator Clone()
	{
		return new XPathMultyIterator(this);
	}

	public override bool MoveNext()
	{
		if (firstNotEmpty >= arr.Length)
		{
			return false;
		}
		if (position != 0)
		{
			if (Advance(firstNotEmpty))
			{
				SiftItem(firstNotEmpty);
			}
			if (firstNotEmpty >= arr.Length)
			{
				return false;
			}
		}
		position++;
		return true;
	}
}
