using System.Collections;

namespace MS.Internal.Data;

internal static class DataExtensionMethods
{
	internal static int Search(this IList list, int index, int count, object value, IComparer comparer)
	{
		if (list is ArrayList arrayList)
		{
			return arrayList.BinarySearch(index, count, value, comparer);
		}
		if (list is LiveShapingList liveShapingList)
		{
			return liveShapingList.Search(index, count, value);
		}
		return 0;
	}

	internal static int Search(this IList list, object value, IComparer comparer)
	{
		return list.Search(0, list.Count, value, comparer);
	}

	internal static void Move(this IList list, int oldIndex, int newIndex)
	{
		if (list is ArrayList arrayList)
		{
			object value = arrayList[oldIndex];
			arrayList.RemoveAt(oldIndex);
			arrayList.Insert(newIndex, value);
		}
		else if (list is LiveShapingList liveShapingList)
		{
			liveShapingList.Move(oldIndex, newIndex);
		}
	}

	internal static void Sort(this IList list, IComparer comparer)
	{
		if (list is ArrayList al)
		{
			SortFieldComparer.SortHelper(al, comparer);
		}
		else if (list is LiveShapingList liveShapingList)
		{
			liveShapingList.Sort();
		}
	}
}
