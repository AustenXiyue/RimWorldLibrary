namespace System;

internal static class IPv4AddressHelper
{
	internal const long Invalid = -1L;

	private const long MaxIPv4Value = 4294967295L;

	private const int Octal = 8;

	private const int Decimal = 10;

	private const int Hex = 16;

	private const int NumberOfLabels = 4;

	internal unsafe static string ParseCanonicalName(string str, int start, int end, ref bool isLoopback)
	{
		byte* ptr = stackalloc byte[4];
		isLoopback = Parse(str, ptr, start, end);
		return *ptr + "." + ptr[1] + "." + ptr[2] + "." + ptr[3];
	}

	internal unsafe static int ParseHostNumber(string str, int start, int end)
	{
		byte* ptr = stackalloc byte[4];
		ParseCanonical(str, ptr, start, end);
		return (*ptr << 24) + (ptr[1] << 16) + (ptr[2] << 8) + ptr[3];
	}

	internal unsafe static bool IsValid(char* name, int start, ref int end, bool allowIPv6, bool notImplicitFile, bool unknownScheme)
	{
		if (allowIPv6 || unknownScheme)
		{
			return IsValidCanonical(name, start, ref end, allowIPv6, notImplicitFile);
		}
		return ParseNonCanonical(name, start, ref end, notImplicitFile) != -1;
	}

	internal unsafe static bool IsValidCanonical(char* name, int start, ref int end, bool allowIPv6, bool notImplicitFile)
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		bool flag2 = false;
		while (start < end)
		{
			char c = name[start];
			if (allowIPv6)
			{
				if (c == ']' || c == '/' || c == '%')
				{
					break;
				}
			}
			else if (c == '/' || c == '\\' || (notImplicitFile && (c == ':' || c == '?' || c == '#')))
			{
				break;
			}
			if (c <= '9' && c >= '0')
			{
				if (!flag && c == '0')
				{
					if (start + 1 < end && name[start + 1] == '0')
					{
						return false;
					}
					flag2 = true;
				}
				flag = true;
				num2 = num2 * 10 + (name[start] - 48);
				if (num2 > 255)
				{
					return false;
				}
			}
			else
			{
				if (c != '.')
				{
					return false;
				}
				if (!flag || (num2 > 0 && flag2))
				{
					return false;
				}
				num++;
				flag = false;
				num2 = 0;
				flag2 = false;
			}
			start++;
		}
		bool num3 = num == 3 && flag;
		if (num3)
		{
			end = start;
		}
		return num3;
	}

	internal unsafe static long ParseNonCanonical(char* name, int start, ref int end, bool notImplicitFile)
	{
		int num = 10;
		long[] array = new long[4];
		long num2 = 0L;
		bool flag = false;
		int num3 = 0;
		int i;
		for (i = start; i < end; i++)
		{
			char c = name[i];
			num2 = 0L;
			num = 10;
			if (c == '0')
			{
				num = 8;
				i++;
				flag = true;
				if (i < end)
				{
					c = name[i];
					if (c == 'x' || c == 'X')
					{
						num = 16;
						i++;
						flag = false;
					}
				}
			}
			for (; i < end; i++)
			{
				c = name[i];
				int num4;
				if ((num == 10 || num == 16) && '0' <= c && c <= '9')
				{
					num4 = c - 48;
				}
				else if (num == 8 && '0' <= c && c <= '7')
				{
					num4 = c - 48;
				}
				else if (num == 16 && 'a' <= c && c <= 'f')
				{
					num4 = c + 10 - 97;
				}
				else
				{
					if (num != 16 || 'A' > c || c > 'F')
					{
						break;
					}
					num4 = c + 10 - 65;
				}
				num2 = num2 * num + num4;
				if (num2 > uint.MaxValue)
				{
					return -1L;
				}
				flag = true;
			}
			if (i >= end || name[i] != '.')
			{
				break;
			}
			if (num3 >= 3 || !flag || num2 > 255)
			{
				return -1L;
			}
			array[num3] = num2;
			num3++;
			flag = false;
		}
		if (!flag)
		{
			return -1L;
		}
		if (i < end)
		{
			char c;
			if ((c = name[i]) != '/' && c != '\\' && (!notImplicitFile || (c != ':' && c != '?' && c != '#')))
			{
				return -1L;
			}
			end = i;
		}
		array[num3] = num2;
		switch (num3)
		{
		case 0:
			if (array[0] > uint.MaxValue)
			{
				return -1L;
			}
			return array[0];
		case 1:
			if (array[1] > 16777215)
			{
				return -1L;
			}
			return (array[0] << 24) | (array[1] & 0xFFFFFF);
		case 2:
			if (array[2] > 65535)
			{
				return -1L;
			}
			return (array[0] << 24) | ((array[1] & 0xFF) << 16) | (array[2] & 0xFFFF);
		case 3:
			if (array[3] > 255)
			{
				return -1L;
			}
			return (array[0] << 24) | ((array[1] & 0xFF) << 16) | ((array[2] & 0xFF) << 8) | (array[3] & 0xFF);
		default:
			return -1L;
		}
	}

	private unsafe static bool Parse(string name, byte* numbers, int start, int end)
	{
		fixed (char* name2 = name)
		{
			int end2 = end;
			long num = ParseNonCanonical(name2, start, ref end2, notImplicitFile: true);
			*numbers = (byte)(num >> 24);
			numbers[1] = (byte)(num >> 16);
			numbers[2] = (byte)(num >> 8);
			numbers[3] = (byte)num;
		}
		return *numbers == 127;
	}

	private unsafe static bool ParseCanonical(string name, byte* numbers, int start, int end)
	{
		for (int i = 0; i < 4; i++)
		{
			byte b = 0;
			char c;
			while (start < end && (c = name[start]) != '.' && c != ':')
			{
				b = (byte)(b * 10 + (byte)(c - 48));
				start++;
			}
			numbers[i] = b;
			start++;
		}
		return *numbers == 127;
	}
}
