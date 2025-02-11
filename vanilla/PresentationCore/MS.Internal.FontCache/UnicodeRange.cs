using System;

namespace MS.Internal.FontCache;

internal struct UnicodeRange
{
	internal int firstChar;

	internal int lastChar;

	internal UnicodeRange(int first, int last)
	{
		firstChar = first;
		lastChar = last;
	}

	internal uint[] GetFullRange()
	{
		int num = Math.Min(lastChar, firstChar);
		int num2 = Math.Max(lastChar, firstChar) - num + 1;
		uint[] array = new uint[num2];
		for (int i = 0; i < num2; i++)
		{
			array[i] = checked((uint)(num + i));
		}
		return array;
	}
}
