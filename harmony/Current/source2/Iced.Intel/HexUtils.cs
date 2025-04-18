using System;

namespace Iced.Intel;

internal static class HexUtils
{
	public static byte[] ToByteArray(string hexData)
	{
		if (hexData == null)
		{
			throw new ArgumentNullException("hexData");
		}
		if (hexData.Length == 0)
		{
			return Array2.Empty<byte>();
		}
		int num = 0;
		for (int i = 0; i < hexData.Length; i++)
		{
			if (hexData[i] != ' ')
			{
				num++;
			}
		}
		byte[] array = new byte[num / 2];
		int num2 = 0;
		int j = 0;
		while (true)
		{
			if (j < hexData.Length && char.IsWhiteSpace(hexData[j]))
			{
				j++;
				continue;
			}
			if (j >= hexData.Length)
			{
				break;
			}
			int num3 = TryParseHexChar(hexData[j++]);
			if (num3 < 0)
			{
				throw new ArgumentOutOfRangeException("hexData");
			}
			for (; j < hexData.Length && char.IsWhiteSpace(hexData[j]); j++)
			{
			}
			if (j >= hexData.Length)
			{
				throw new ArgumentOutOfRangeException("hexData");
			}
			int num4 = TryParseHexChar(hexData[j++]);
			if (num4 < 0)
			{
				throw new ArgumentOutOfRangeException("hexData");
			}
			array[num2++] = (byte)((num3 << 4) | num4);
		}
		if (num2 != array.Length)
		{
			throw new InvalidOperationException();
		}
		return array;
	}

	private static int TryParseHexChar(char c)
	{
		if ('0' <= c && c <= '9')
		{
			return c - 48;
		}
		if ('A' <= c && c <= 'F')
		{
			return c - 65 + 10;
		}
		if ('a' <= c && c <= 'f')
		{
			return c - 97 + 10;
		}
		return -1;
	}
}
