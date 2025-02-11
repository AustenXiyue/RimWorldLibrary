using System.Collections.Generic;

namespace System.Windows.Ink;

internal static class IEnumerablePointHelper
{
	internal static int GetCount(IEnumerable<Point> ienum)
	{
		if (ienum is ICollection<Point> collection)
		{
			return collection.Count;
		}
		int num = 0;
		foreach (Point item in ienum)
		{
			_ = item;
			num++;
		}
		return num;
	}

	internal static Point[] GetPointArray(IEnumerable<Point> ienum)
	{
		if (ienum is Point[] result)
		{
			return result;
		}
		Point[] array = new Point[GetCount(ienum)];
		int num = 0;
		foreach (Point item in ienum)
		{
			array[num++] = item;
		}
		return array;
	}
}
