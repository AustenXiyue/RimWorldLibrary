using System.Collections;

namespace System.Xml.Serialization;

internal class WorkItems
{
	private ArrayList list = new ArrayList();

	internal ImportStructWorkItem this[int index]
	{
		get
		{
			return (ImportStructWorkItem)list[index];
		}
		set
		{
			list[index] = value;
		}
	}

	internal int Count => list.Count;

	internal void Add(ImportStructWorkItem item)
	{
		list.Add(item);
	}

	internal bool Contains(StructMapping mapping)
	{
		return IndexOf(mapping) >= 0;
	}

	internal int IndexOf(StructMapping mapping)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i].Mapping == mapping)
			{
				return i;
			}
		}
		return -1;
	}

	internal void RemoveAt(int index)
	{
		list.RemoveAt(index);
	}
}
