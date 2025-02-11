using System.Windows.Ink;

namespace MS.Internal.Ink.InkSerializedFormat;

internal static class StrokeIdGenerator
{
	internal static int[] GetStrokeIds(StrokeCollection strokes)
	{
		int[] array = new int[strokes.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = i + 1;
		}
		return array;
	}
}
