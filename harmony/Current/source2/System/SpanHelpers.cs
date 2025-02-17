using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

internal static class SpanHelpers
{
	internal struct ComparerComparable<T, TComparer> : IComparable<T> where TComparer : IComparer<T>
	{
		private readonly T _value;

		private readonly TComparer _comparer;

		public ComparerComparable(T value, TComparer comparer)
		{
			_value = value;
			_comparer = comparer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(T? other)
		{
			return _comparer.Compare(_value, other);
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 64)]
	private struct Reg64
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 32)]
	private struct Reg32
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 16)]
	private struct Reg16
	{
	}

	public static class PerTypeValues<T>
	{
		public static readonly bool IsReferenceOrContainsReferences = IsReferenceOrContainsReferencesCore(typeof(T));

		public static readonly T[] EmptyArray = ArrayEx.Empty<T>();

		public static readonly IntPtr ArrayAdjustment = MeasureArrayAdjustment();

		private static IntPtr MeasureArrayAdjustment()
		{
			T[] array = new T[1];
			return Unsafe.ByteOffset(ref Unsafe.As<Pinnable<T>>(array).Data, ref array[0]);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BinarySearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable comparable) where TComparable : IComparable<T>
	{
		if (comparable == null)
		{
			System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.comparable);
		}
		return BinarySearch(ref MemoryMarshal.GetReference(span), span.Length, comparable);
	}

	public static int BinarySearch<T, TComparable>(ref T spanStart, int length, TComparable comparable) where TComparable : IComparable<T>
	{
		int num = 0;
		int num2 = length - 1;
		while (num <= num2)
		{
			int num3 = num2 + num >>> 1;
			ref TComparable reference = ref comparable;
			TComparable val = default(TComparable);
			if (val == null)
			{
				val = reference;
				reference = ref val;
			}
			int num4 = reference.CompareTo(Unsafe.Add(ref spanStart, num3));
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 > 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return ~num;
	}

	public static int IndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
	{
		if (valueLength == 0)
		{
			return 0;
		}
		byte value2 = value;
		ref byte second = ref Unsafe.Add(ref value, 1);
		int num = valueLength - 1;
		int num2 = 0;
		while (true)
		{
			int num3 = searchSpaceLength - num2 - num;
			if (num3 <= 0)
			{
				break;
			}
			int num4 = IndexOf(ref Unsafe.Add(ref searchSpace, num2), value2, num3);
			if (num4 == -1)
			{
				break;
			}
			num2 += num4;
			if (SequenceEqual(ref Unsafe.Add(ref searchSpace, num2 + 1), ref second, num))
			{
				return num2;
			}
			num2++;
		}
		return -1;
	}

	public static int IndexOfAny(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
	{
		if (valueLength == 0)
		{
			return 0;
		}
		int num = -1;
		for (int i = 0; i < valueLength; i++)
		{
			int num2 = IndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
			if ((uint)num2 < (uint)num)
			{
				num = num2;
				searchSpaceLength = num2;
				if (num == 0)
				{
					break;
				}
			}
		}
		return num;
	}

	public static int LastIndexOfAny(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
	{
		if (valueLength == 0)
		{
			return 0;
		}
		int num = -1;
		for (int i = 0; i < valueLength; i++)
		{
			int num2 = LastIndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public static int IndexOf(ref byte searchSpace, byte value, int length)
	{
		nint num = 0;
		nint num2 = length;
		while (true)
		{
			if ((nuint)num2 >= (nuint)8u)
			{
				num2 -= 8;
				if (value != Unsafe.AddByteOffset(ref searchSpace, num))
				{
					if (value == Unsafe.AddByteOffset(ref searchSpace, num + 1))
					{
						goto IL_0109;
					}
					if (value == Unsafe.AddByteOffset(ref searchSpace, num + 2))
					{
						goto IL_010f;
					}
					if (value != Unsafe.AddByteOffset(ref searchSpace, num + 3))
					{
						if (value != Unsafe.AddByteOffset(ref searchSpace, num + 4))
						{
							if (value != Unsafe.AddByteOffset(ref searchSpace, num + 5))
							{
								if (value != Unsafe.AddByteOffset(ref searchSpace, num + 6))
								{
									if (value == Unsafe.AddByteOffset(ref searchSpace, num + 7))
									{
										break;
									}
									num += 8;
									continue;
								}
								return (int)(num + 6);
							}
							return (int)(num + 5);
						}
						return (int)(num + 4);
					}
					goto IL_0115;
				}
			}
			else
			{
				if ((nuint)num2 >= (nuint)4u)
				{
					num2 -= 4;
					if (value == Unsafe.AddByteOffset(ref searchSpace, num))
					{
						goto IL_0106;
					}
					if (value == Unsafe.AddByteOffset(ref searchSpace, num + 1))
					{
						goto IL_0109;
					}
					if (value == Unsafe.AddByteOffset(ref searchSpace, num + 2))
					{
						goto IL_010f;
					}
					if (value == Unsafe.AddByteOffset(ref searchSpace, num + 3))
					{
						goto IL_0115;
					}
					num += 4;
				}
				while (true)
				{
					if (num2 != 0)
					{
						num2--;
						if (value == Unsafe.AddByteOffset(ref searchSpace, num))
						{
							break;
						}
						num++;
						continue;
					}
					return -1;
				}
			}
			goto IL_0106;
			IL_010f:
			return (int)(num + 2);
			IL_0106:
			return (int)num;
			IL_0109:
			return (int)(num + 1);
			IL_0115:
			return (int)(num + 3);
		}
		return (int)(num + 7);
	}

	public static int LastIndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
	{
		if (valueLength == 0)
		{
			return 0;
		}
		byte value2 = value;
		ref byte second = ref Unsafe.Add(ref value, 1);
		int num = valueLength - 1;
		int num2 = 0;
		while (true)
		{
			int num3 = searchSpaceLength - num2 - num;
			if (num3 <= 0)
			{
				break;
			}
			int num4 = LastIndexOf(ref searchSpace, value2, num3);
			if (num4 == -1)
			{
				break;
			}
			if (SequenceEqual(ref Unsafe.Add(ref searchSpace, num4 + 1), ref second, num))
			{
				return num4;
			}
			num2 += num3 - num4;
		}
		return -1;
	}

	public static int LastIndexOf(ref byte searchSpace, byte value, int length)
	{
		nint num = length;
		nint num2 = length;
		while (true)
		{
			if ((nuint)num2 >= (nuint)8u)
			{
				num2 -= 8;
				num -= 8;
				if (value == Unsafe.AddByteOffset(ref searchSpace, num + 7))
				{
					break;
				}
				if (value == Unsafe.AddByteOffset(ref searchSpace, num + 6))
				{
					return (int)(num + 6);
				}
				if (value == Unsafe.AddByteOffset(ref searchSpace, num + 5))
				{
					return (int)(num + 5);
				}
				if (value == Unsafe.AddByteOffset(ref searchSpace, num + 4))
				{
					return (int)(num + 4);
				}
				if (value == Unsafe.AddByteOffset(ref searchSpace, num + 3))
				{
					goto IL_010f;
				}
				if (value == Unsafe.AddByteOffset(ref searchSpace, num + 2))
				{
					goto IL_0109;
				}
				if (value == Unsafe.AddByteOffset(ref searchSpace, num + 1))
				{
					goto IL_0103;
				}
				if (value != Unsafe.AddByteOffset(ref searchSpace, num))
				{
					continue;
				}
			}
			else
			{
				if ((nuint)num2 >= (nuint)4u)
				{
					num2 -= 4;
					num -= 4;
					if (value == Unsafe.AddByteOffset(ref searchSpace, num + 3))
					{
						goto IL_010f;
					}
					if (value == Unsafe.AddByteOffset(ref searchSpace, num + 2))
					{
						goto IL_0109;
					}
					if (value == Unsafe.AddByteOffset(ref searchSpace, num + 1))
					{
						goto IL_0103;
					}
					if (value == Unsafe.AddByteOffset(ref searchSpace, num))
					{
						goto IL_0100;
					}
				}
				do
				{
					if (num2 != 0)
					{
						num2--;
						num--;
						continue;
					}
					return -1;
				}
				while (value != Unsafe.AddByteOffset(ref searchSpace, num));
			}
			goto IL_0100;
			IL_0109:
			return (int)(num + 2);
			IL_010f:
			return (int)(num + 3);
			IL_0103:
			return (int)(num + 1);
			IL_0100:
			return (int)num;
		}
		return (int)(num + 7);
	}

	public static int IndexOfAny(ref byte searchSpace, byte value0, byte value1, int length)
	{
		nint num = 0;
		nint num2 = length;
		while (true)
		{
			if ((nuint)num2 >= (nuint)8u)
			{
				num2 -= 8;
				uint num3 = Unsafe.AddByteOffset(ref searchSpace, num);
				if (value0 != num3 && value1 != num3)
				{
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 1);
					if (value0 == num3 || value1 == num3)
					{
						goto IL_019b;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 2);
					if (value0 == num3 || value1 == num3)
					{
						goto IL_01a1;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 3);
					if (value0 != num3 && value1 != num3)
					{
						num3 = Unsafe.AddByteOffset(ref searchSpace, num + 4);
						if (value0 != num3 && value1 != num3)
						{
							num3 = Unsafe.AddByteOffset(ref searchSpace, num + 5);
							if (value0 != num3 && value1 != num3)
							{
								num3 = Unsafe.AddByteOffset(ref searchSpace, num + 6);
								if (value0 != num3 && value1 != num3)
								{
									num3 = Unsafe.AddByteOffset(ref searchSpace, num + 7);
									if (value0 == num3 || value1 == num3)
									{
										break;
									}
									num += 8;
									continue;
								}
								return (int)(num + 6);
							}
							return (int)(num + 5);
						}
						return (int)(num + 4);
					}
					goto IL_01a7;
				}
			}
			else
			{
				if ((nuint)num2 >= (nuint)4u)
				{
					num2 -= 4;
					uint num3 = Unsafe.AddByteOffset(ref searchSpace, num);
					if (value0 == num3 || value1 == num3)
					{
						goto IL_0198;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 1);
					if (value0 == num3 || value1 == num3)
					{
						goto IL_019b;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 2);
					if (value0 == num3 || value1 == num3)
					{
						goto IL_01a1;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 3);
					if (value0 == num3 || value1 == num3)
					{
						goto IL_01a7;
					}
					num += 4;
				}
				while (true)
				{
					if (num2 != 0)
					{
						num2--;
						uint num3 = Unsafe.AddByteOffset(ref searchSpace, num);
						if (value0 == num3 || value1 == num3)
						{
							break;
						}
						num++;
						continue;
					}
					return -1;
				}
			}
			goto IL_0198;
			IL_01a1:
			return (int)(num + 2);
			IL_019b:
			return (int)(num + 1);
			IL_01a7:
			return (int)(num + 3);
			IL_0198:
			return (int)num;
		}
		return (int)(num + 7);
	}

	public static int IndexOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
	{
		nint num = 0;
		nint num2 = length;
		while (true)
		{
			if ((nuint)num2 >= (nuint)8u)
			{
				num2 -= 8;
				uint num3 = Unsafe.AddByteOffset(ref searchSpace, num);
				if (value0 != num3 && value1 != num3 && value2 != num3)
				{
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 1);
					if (value0 == num3 || value1 == num3 || value2 == num3)
					{
						goto IL_020a;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 2);
					if (value0 == num3 || value1 == num3 || value2 == num3)
					{
						goto IL_0210;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 3);
					if (value0 != num3 && value1 != num3 && value2 != num3)
					{
						num3 = Unsafe.AddByteOffset(ref searchSpace, num + 4);
						if (value0 != num3 && value1 != num3 && value2 != num3)
						{
							num3 = Unsafe.AddByteOffset(ref searchSpace, num + 5);
							if (value0 != num3 && value1 != num3 && value2 != num3)
							{
								num3 = Unsafe.AddByteOffset(ref searchSpace, num + 6);
								if (value0 != num3 && value1 != num3 && value2 != num3)
								{
									num3 = Unsafe.AddByteOffset(ref searchSpace, num + 7);
									if (value0 == num3 || value1 == num3 || value2 == num3)
									{
										break;
									}
									num += 8;
									continue;
								}
								return (int)(num + 6);
							}
							return (int)(num + 5);
						}
						return (int)(num + 4);
					}
					goto IL_0216;
				}
			}
			else
			{
				if ((nuint)num2 >= (nuint)4u)
				{
					num2 -= 4;
					uint num3 = Unsafe.AddByteOffset(ref searchSpace, num);
					if (value0 == num3 || value1 == num3 || value2 == num3)
					{
						goto IL_0207;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 1);
					if (value0 == num3 || value1 == num3 || value2 == num3)
					{
						goto IL_020a;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 2);
					if (value0 == num3 || value1 == num3 || value2 == num3)
					{
						goto IL_0210;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 3);
					if (value0 == num3 || value1 == num3 || value2 == num3)
					{
						goto IL_0216;
					}
					num += 4;
				}
				while (true)
				{
					if (num2 != 0)
					{
						num2--;
						uint num3 = Unsafe.AddByteOffset(ref searchSpace, num);
						if (value0 == num3 || value1 == num3 || value2 == num3)
						{
							break;
						}
						num++;
						continue;
					}
					return -1;
				}
			}
			goto IL_0207;
			IL_0207:
			return (int)num;
			IL_0210:
			return (int)(num + 2);
			IL_020a:
			return (int)(num + 1);
			IL_0216:
			return (int)(num + 3);
		}
		return (int)(num + 7);
	}

	public static int LastIndexOfAny(ref byte searchSpace, byte value0, byte value1, int length)
	{
		nint num = length;
		nint num2 = length;
		while (true)
		{
			if ((nuint)num2 >= (nuint)8u)
			{
				num2 -= 8;
				num -= 8;
				uint num3 = Unsafe.AddByteOffset(ref searchSpace, num + 7);
				if (value0 == num3 || value1 == num3)
				{
					break;
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 6);
				if (value0 == num3 || value1 == num3)
				{
					return (int)(num + 6);
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 5);
				if (value0 == num3 || value1 == num3)
				{
					return (int)(num + 5);
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 4);
				if (value0 == num3 || value1 == num3)
				{
					return (int)(num + 4);
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 3);
				if (value0 == num3 || value1 == num3)
				{
					goto IL_01a7;
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 2);
				if (value0 == num3 || value1 == num3)
				{
					goto IL_01a1;
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 1);
				if (value0 == num3 || value1 == num3)
				{
					goto IL_019b;
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num);
				if (value0 != num3 && value1 != num3)
				{
					continue;
				}
			}
			else
			{
				uint num3;
				if ((nuint)num2 >= (nuint)4u)
				{
					num2 -= 4;
					num -= 4;
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 3);
					if (value0 == num3 || value1 == num3)
					{
						goto IL_01a7;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 2);
					if (value0 == num3 || value1 == num3)
					{
						goto IL_01a1;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 1);
					if (value0 == num3 || value1 == num3)
					{
						goto IL_019b;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num);
					if (value0 == num3 || value1 == num3)
					{
						goto IL_0198;
					}
				}
				do
				{
					if (num2 != 0)
					{
						num2--;
						num--;
						num3 = Unsafe.AddByteOffset(ref searchSpace, num);
						continue;
					}
					return -1;
				}
				while (value0 != num3 && value1 != num3);
			}
			goto IL_0198;
			IL_01a1:
			return (int)(num + 2);
			IL_019b:
			return (int)(num + 1);
			IL_0198:
			return (int)num;
			IL_01a7:
			return (int)(num + 3);
		}
		return (int)(num + 7);
	}

	public static int LastIndexOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
	{
		nint num = length;
		nint num2 = length;
		while (true)
		{
			if ((nuint)num2 >= (nuint)8u)
			{
				num2 -= 8;
				num -= 8;
				uint num3 = Unsafe.AddByteOffset(ref searchSpace, num + 7);
				if (value0 == num3 || value1 == num3 || value2 == num3)
				{
					break;
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 6);
				if (value0 == num3 || value1 == num3 || value2 == num3)
				{
					return (int)(num + 6);
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 5);
				if (value0 == num3 || value1 == num3 || value2 == num3)
				{
					return (int)(num + 5);
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 4);
				if (value0 == num3 || value1 == num3 || value2 == num3)
				{
					return (int)(num + 4);
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 3);
				if (value0 == num3 || value1 == num3 || value2 == num3)
				{
					goto IL_0217;
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 2);
				if (value0 == num3 || value1 == num3 || value2 == num3)
				{
					goto IL_0211;
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num + 1);
				if (value0 == num3 || value1 == num3 || value2 == num3)
				{
					goto IL_020b;
				}
				num3 = Unsafe.AddByteOffset(ref searchSpace, num);
				if (value0 != num3 && value1 != num3 && value2 != num3)
				{
					continue;
				}
			}
			else
			{
				uint num3;
				if ((nuint)num2 >= (nuint)4u)
				{
					num2 -= 4;
					num -= 4;
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 3);
					if (value0 == num3 || value1 == num3 || value2 == num3)
					{
						goto IL_0217;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 2);
					if (value0 == num3 || value1 == num3 || value2 == num3)
					{
						goto IL_0211;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num + 1);
					if (value0 == num3 || value1 == num3 || value2 == num3)
					{
						goto IL_020b;
					}
					num3 = Unsafe.AddByteOffset(ref searchSpace, num);
					if (value0 == num3 || value1 == num3 || value2 == num3)
					{
						goto IL_0208;
					}
				}
				do
				{
					if (num2 != 0)
					{
						num2--;
						num--;
						num3 = Unsafe.AddByteOffset(ref searchSpace, num);
						continue;
					}
					return -1;
				}
				while (value0 != num3 && value1 != num3 && value2 != num3);
			}
			goto IL_0208;
			IL_020b:
			return (int)(num + 1);
			IL_0211:
			return (int)(num + 2);
			IL_0217:
			return (int)(num + 3);
			IL_0208:
			return (int)num;
		}
		return (int)(num + 7);
	}

	public unsafe static bool SequenceEqual(ref byte first, ref byte second, nuint length)
	{
		if (!Unsafe.AreSame(ref first, ref second))
		{
			nint num = 0;
			nint num2 = (nint)length;
			if ((nuint)num2 >= (nuint)sizeof(UIntPtr))
			{
				num2 -= sizeof(UIntPtr);
				while (true)
				{
					if ((nuint)num2 > (nuint)num)
					{
						if (Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, num)) != Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, num)))
						{
							break;
						}
						num += sizeof(UIntPtr);
						continue;
					}
					return Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, num2)) == Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, num2));
				}
				goto IL_008f;
			}
			while ((nuint)num2 > (nuint)num)
			{
				if (Unsafe.AddByteOffset(ref first, num) == Unsafe.AddByteOffset(ref second, num))
				{
					num++;
					continue;
				}
				goto IL_008f;
			}
		}
		return true;
		IL_008f:
		return false;
	}

	public unsafe static int SequenceCompareTo(ref byte first, int firstLength, ref byte second, int secondLength)
	{
		if (!Unsafe.AreSame(ref first, ref second))
		{
			nint num = ((firstLength < secondLength) ? firstLength : secondLength);
			nint num2 = 0;
			nint num3 = num;
			if ((nuint)num3 > (nuint)sizeof(UIntPtr))
			{
				for (num3 -= sizeof(UIntPtr); (nuint)num3 > (nuint)num2 && !(Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, num2)) != Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, num2))); num2 += sizeof(UIntPtr))
				{
				}
			}
			for (; (nuint)num > (nuint)num2; num2++)
			{
				int num4 = Unsafe.AddByteOffset(ref first, num2).CompareTo(Unsafe.AddByteOffset(ref second, num2));
				if (num4 != 0)
				{
					return num4;
				}
			}
		}
		return firstLength - secondLength;
	}

	public unsafe static int SequenceCompareTo(ref char first, int firstLength, ref char second, int secondLength)
	{
		int result = firstLength - secondLength;
		if (!Unsafe.AreSame(ref first, ref second))
		{
			nint num = ((firstLength < secondLength) ? firstLength : secondLength);
			nint num2 = 0;
			if ((nuint)num >= (nuint)(sizeof(UIntPtr) / 2))
			{
				for (; (nuint)num >= (nuint)(num2 + sizeof(UIntPtr) / 2) && !(Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref first, num2))) != Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref second, num2)))); num2 += sizeof(UIntPtr) / 2)
				{
				}
			}
			if (sizeof(UIntPtr) > 4 && (nuint)num >= (nuint)(num2 + 2) && Unsafe.ReadUnaligned<int>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref first, num2))) == Unsafe.ReadUnaligned<int>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref second, num2))))
			{
				num2 += 2;
			}
			for (; (nuint)num2 < (nuint)num; num2++)
			{
				int num3 = Unsafe.Add(ref first, num2).CompareTo(Unsafe.Add(ref second, num2));
				if (num3 != 0)
				{
					return num3;
				}
			}
		}
		return result;
	}

	public unsafe static int IndexOf(ref char searchSpace, char value, int length)
	{
		fixed (char* ptr = &searchSpace)
		{
			char* ptr2 = ptr;
			_ = length;
			while (true)
			{
				if (length >= 4)
				{
					length -= 4;
					if (*ptr2 == value)
					{
						break;
					}
					if (ptr2[1] != value)
					{
						if (ptr2[2] != value)
						{
							if (ptr2[3] != value)
							{
								ptr2 += 4;
								continue;
							}
							ptr2++;
						}
						ptr2++;
					}
					ptr2++;
					break;
				}
				while (true)
				{
					if (length > 0)
					{
						length--;
						if (*ptr2 == value)
						{
							break;
						}
						ptr2++;
						continue;
					}
					return -1;
				}
				break;
			}
			return (int)(ptr2 - ptr);
		}
	}

	public unsafe static int LastIndexOf(ref char searchSpace, char value, int length)
	{
		fixed (char* ptr = &searchSpace)
		{
			char* ptr2 = ptr + length;
			char* ptr3 = ptr;
			while (true)
			{
				if (length >= 4)
				{
					length -= 4;
					ptr2 -= 4;
					if (ptr2[3] == value)
					{
						break;
					}
					if (ptr2[2] == value)
					{
						return (int)(ptr2 - ptr3) + 2;
					}
					if (ptr2[1] == value)
					{
						return (int)(ptr2 - ptr3) + 1;
					}
					if (*ptr2 != value)
					{
						continue;
					}
				}
				else
				{
					do
					{
						if (length > 0)
						{
							length--;
							ptr2--;
							continue;
						}
						return -1;
					}
					while (*ptr2 != value);
				}
				return (int)(ptr2 - ptr3);
			}
			return (int)(ptr2 - ptr3) + 3;
		}
	}

	public unsafe static void CopyTo<T>(ref T dst, int dstLength, ref T src, int srcLength)
	{
		IntPtr intPtr = Unsafe.ByteOffset(ref src, ref Unsafe.Add(ref src, srcLength));
		IntPtr intPtr2 = Unsafe.ByteOffset(ref dst, ref Unsafe.Add(ref dst, dstLength));
		IntPtr intPtr3 = Unsafe.ByteOffset(ref src, ref dst);
		bool num;
		if (sizeof(IntPtr) != 4)
		{
			if ((ulong)(long)intPtr3 >= (ulong)(long)intPtr)
			{
				num = (ulong)(long)intPtr3 > (ulong)(-(long)intPtr2);
				goto IL_006f;
			}
		}
		else if ((uint)(int)intPtr3 >= (uint)(int)intPtr)
		{
			num = (uint)(int)intPtr3 > (uint)(-(int)intPtr2);
			goto IL_006f;
		}
		goto IL_00da;
		IL_006f:
		if (!num && !IsReferenceOrContainsReferences<T>())
		{
			ref byte source = ref Unsafe.As<T, byte>(ref dst);
			ref byte source2 = ref Unsafe.As<T, byte>(ref src);
			ulong num2 = (ulong)(long)intPtr;
			uint num4;
			for (ulong num3 = 0uL; num3 < num2; num3 += num4)
			{
				num4 = (uint)((num2 - num3 > uint.MaxValue) ? uint.MaxValue : (num2 - num3));
				Unsafe.CopyBlock(ref Unsafe.Add(ref source, (IntPtr)(long)num3), ref Unsafe.Add(ref source2, (IntPtr)(long)num3), num4);
			}
			return;
		}
		goto IL_00da;
		IL_00da:
		bool num5 = ((sizeof(IntPtr) == 4) ? ((uint)(int)intPtr3 > (uint)(-(int)intPtr2)) : ((ulong)(long)intPtr3 > (ulong)(-(long)intPtr2)));
		int num6 = (num5 ? 1 : (-1));
		int num7 = ((!num5) ? (srcLength - 1) : 0);
		int i;
		for (i = 0; i < (srcLength & -8); i += 8)
		{
			Unsafe.Add(ref dst, num7) = Unsafe.Add(ref src, num7);
			Unsafe.Add(ref dst, num7 + num6) = Unsafe.Add(ref src, num7 + num6);
			Unsafe.Add(ref dst, num7 + num6 * 2) = Unsafe.Add(ref src, num7 + num6 * 2);
			Unsafe.Add(ref dst, num7 + num6 * 3) = Unsafe.Add(ref src, num7 + num6 * 3);
			Unsafe.Add(ref dst, num7 + num6 * 4) = Unsafe.Add(ref src, num7 + num6 * 4);
			Unsafe.Add(ref dst, num7 + num6 * 5) = Unsafe.Add(ref src, num7 + num6 * 5);
			Unsafe.Add(ref dst, num7 + num6 * 6) = Unsafe.Add(ref src, num7 + num6 * 6);
			Unsafe.Add(ref dst, num7 + num6 * 7) = Unsafe.Add(ref src, num7 + num6 * 7);
			num7 += num6 * 8;
		}
		if (i < (srcLength & -4))
		{
			Unsafe.Add(ref dst, num7) = Unsafe.Add(ref src, num7);
			Unsafe.Add(ref dst, num7 + num6) = Unsafe.Add(ref src, num7 + num6);
			Unsafe.Add(ref dst, num7 + num6 * 2) = Unsafe.Add(ref src, num7 + num6 * 2);
			Unsafe.Add(ref dst, num7 + num6 * 3) = Unsafe.Add(ref src, num7 + num6 * 3);
			num7 += num6 * 4;
			i += 4;
		}
		for (; i < srcLength; i++)
		{
			Unsafe.Add(ref dst, num7) = Unsafe.Add(ref src, num7);
			num7 += num6;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static IntPtr Add<T>(this IntPtr start, int index)
	{
		if (sizeof(IntPtr) == 4)
		{
			uint num = (uint)(index * Unsafe.SizeOf<T>());
			return (IntPtr)((byte*)(void*)start + num);
		}
		ulong num2 = (ulong)index * (ulong)Unsafe.SizeOf<T>();
		return (IntPtr)((byte*)(void*)start + num2);
	}

	public static bool IsReferenceOrContainsReferences<T>()
	{
		return PerTypeValues<T>.IsReferenceOrContainsReferences;
	}

	private static bool IsReferenceOrContainsReferencesCore(Type type)
	{
		if (type.GetTypeInfo().IsPrimitive)
		{
			return false;
		}
		if (!type.GetTypeInfo().IsValueType)
		{
			return true;
		}
		Type underlyingType = Nullable.GetUnderlyingType(type);
		if (underlyingType != null)
		{
			type = underlyingType;
		}
		if (type.GetTypeInfo().IsEnum)
		{
			return false;
		}
		foreach (FieldInfo declaredField in type.GetTypeInfo().DeclaredFields)
		{
			if (!declaredField.IsStatic && IsReferenceOrContainsReferencesCore(declaredField.FieldType))
			{
				return true;
			}
		}
		return false;
	}

	public unsafe static void ClearLessThanPointerSized(byte* ptr, UIntPtr byteLength)
	{
		if (sizeof(UIntPtr) == 4)
		{
			Unsafe.InitBlockUnaligned(ptr, 0, (uint)byteLength);
			return;
		}
		ulong num = (ulong)byteLength;
		uint num2 = (uint)(num & 0xFFFFFFFFu);
		Unsafe.InitBlockUnaligned(ptr, 0, num2);
		num -= num2;
		ptr += num2;
		while (num != 0)
		{
			num2 = (uint)((num >= uint.MaxValue) ? uint.MaxValue : num);
			Unsafe.InitBlockUnaligned(ptr, 0, num2);
			ptr += num2;
			num -= num2;
		}
	}

	public unsafe static void ClearLessThanPointerSized(ref byte b, UIntPtr byteLength)
	{
		if (sizeof(UIntPtr) == 4)
		{
			Unsafe.InitBlockUnaligned(ref b, 0, (uint)byteLength);
			return;
		}
		ulong num = (ulong)byteLength;
		uint num2 = (uint)(num & 0xFFFFFFFFu);
		Unsafe.InitBlockUnaligned(ref b, 0, num2);
		num -= num2;
		long num3 = num2;
		while (num != 0)
		{
			num2 = (uint)((num >= uint.MaxValue) ? uint.MaxValue : num);
			Unsafe.InitBlockUnaligned(ref Unsafe.Add(ref b, (IntPtr)num3), 0, num2);
			num3 += num2;
			num -= num2;
		}
	}

	public unsafe static void ClearPointerSizedWithoutReferences(ref byte b, nuint byteLength)
	{
		nint num;
		for (num = 0; num.LessThanEqual(byteLength - (nuint)sizeof(Reg64)); num += sizeof(Reg64))
		{
			Unsafe.As<byte, Reg64>(ref Unsafe.Add(ref b, num)) = default(Reg64);
		}
		if (num.LessThanEqual(byteLength - (nuint)sizeof(Reg32)))
		{
			Unsafe.As<byte, Reg32>(ref Unsafe.Add(ref b, num)) = default(Reg32);
			num += sizeof(Reg32);
		}
		if (num.LessThanEqual(byteLength - (nuint)sizeof(Reg16)))
		{
			Unsafe.As<byte, Reg16>(ref Unsafe.Add(ref b, num)) = default(Reg16);
			num += sizeof(Reg16);
		}
		if (num.LessThanEqual(byteLength - 8))
		{
			Unsafe.As<byte, long>(ref Unsafe.Add(ref b, num)) = 0L;
			num += 8;
		}
		if (sizeof(IntPtr) == 4 && num.LessThanEqual(byteLength - 4))
		{
			Unsafe.As<byte, int>(ref Unsafe.Add(ref b, num)) = 0;
		}
	}

	public static void ClearPointerSizedWithReferences(ref IntPtr ip, nuint pointerSizeLength)
	{
		nint num = 0;
		nint num2;
		while ((num2 = num + 8).LessThanEqual(pointerSizeLength))
		{
			Unsafe.Add(ref ip, num + 0) = default(IntPtr);
			Unsafe.Add(ref ip, num + 1) = default(IntPtr);
			Unsafe.Add(ref ip, num + 2) = default(IntPtr);
			Unsafe.Add(ref ip, num + 3) = default(IntPtr);
			Unsafe.Add(ref ip, num + 4) = default(IntPtr);
			Unsafe.Add(ref ip, num + 5) = default(IntPtr);
			Unsafe.Add(ref ip, num + 6) = default(IntPtr);
			Unsafe.Add(ref ip, num + 7) = default(IntPtr);
			num = num2;
		}
		if ((num2 = num + 4).LessThanEqual(pointerSizeLength))
		{
			Unsafe.Add(ref ip, num + 0) = default(IntPtr);
			Unsafe.Add(ref ip, num + 1) = default(IntPtr);
			Unsafe.Add(ref ip, num + 2) = default(IntPtr);
			Unsafe.Add(ref ip, num + 3) = default(IntPtr);
			num = num2;
		}
		if ((num2 = num + 2).LessThanEqual(pointerSizeLength))
		{
			Unsafe.Add(ref ip, num + 0) = default(IntPtr);
			Unsafe.Add(ref ip, num + 1) = default(IntPtr);
			num = num2;
		}
		if ((num + 1).LessThanEqual(pointerSizeLength))
		{
			Unsafe.Add(ref ip, num) = default(IntPtr);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static bool LessThanEqual(this IntPtr index, UIntPtr length)
	{
		if (sizeof(UIntPtr) != 4)
		{
			return (long)index <= (long)(ulong)length;
		}
		return (int)index <= (int)(uint)length;
	}

	public static int IndexOf<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
	{
		if (valueLength == 0)
		{
			return 0;
		}
		T value2 = value;
		ref T second = ref Unsafe.Add(ref value, 1);
		int num = valueLength - 1;
		int num2 = 0;
		while (true)
		{
			int num3 = searchSpaceLength - num2 - num;
			if (num3 <= 0)
			{
				break;
			}
			int num4 = IndexOf(ref Unsafe.Add(ref searchSpace, num2), value2, num3);
			if (num4 == -1)
			{
				break;
			}
			num2 += num4;
			if (SequenceEqual(ref Unsafe.Add(ref searchSpace, num2 + 1), ref second, num))
			{
				return num2;
			}
			num2++;
		}
		return -1;
	}

	public static int IndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
	{
		nuint num = 0u;
		while (true)
		{
			T val;
			if (length >= 8)
			{
				length -= 8;
				ref T reference = ref value;
				val = default(T);
				if (val == null)
				{
					val = reference;
					reference = ref val;
				}
				if (!reference.Equals(Unsafe.Add(ref searchSpace, num)))
				{
					ref T reference2 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference2;
						reference2 = ref val;
					}
					if (reference2.Equals(Unsafe.Add(ref searchSpace, num + 1)))
					{
						goto IL_0315;
					}
					ref T reference3 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference3;
						reference3 = ref val;
					}
					if (reference3.Equals(Unsafe.Add(ref searchSpace, num + 2)))
					{
						goto IL_031b;
					}
					ref T reference4 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference4;
						reference4 = ref val;
					}
					if (!reference4.Equals(Unsafe.Add(ref searchSpace, num + 3)))
					{
						ref T reference5 = ref value;
						val = default(T);
						if (val == null)
						{
							val = reference5;
							reference5 = ref val;
						}
						if (!reference5.Equals(Unsafe.Add(ref searchSpace, num + 4)))
						{
							ref T reference6 = ref value;
							val = default(T);
							if (val == null)
							{
								val = reference6;
								reference6 = ref val;
							}
							if (!reference6.Equals(Unsafe.Add(ref searchSpace, num + 5)))
							{
								ref T reference7 = ref value;
								val = default(T);
								if (val == null)
								{
									val = reference7;
									reference7 = ref val;
								}
								if (!reference7.Equals(Unsafe.Add(ref searchSpace, num + 6)))
								{
									ref T reference8 = ref value;
									val = default(T);
									if (val == null)
									{
										val = reference8;
										reference8 = ref val;
									}
									if (reference8.Equals(Unsafe.Add(ref searchSpace, num + 7)))
									{
										break;
									}
									num += 8;
									continue;
								}
								return (int)(num + 6);
							}
							return (int)(num + 5);
						}
						return (int)(num + 4);
					}
					goto IL_0321;
				}
			}
			else
			{
				if (length >= 4)
				{
					length -= 4;
					ref T reference9 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference9;
						reference9 = ref val;
					}
					if (reference9.Equals(Unsafe.Add(ref searchSpace, num)))
					{
						goto IL_0312;
					}
					ref T reference10 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference10;
						reference10 = ref val;
					}
					if (reference10.Equals(Unsafe.Add(ref searchSpace, num + 1)))
					{
						goto IL_0315;
					}
					ref T reference11 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference11;
						reference11 = ref val;
					}
					if (reference11.Equals(Unsafe.Add(ref searchSpace, num + 2)))
					{
						goto IL_031b;
					}
					ref T reference12 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference12;
						reference12 = ref val;
					}
					if (reference12.Equals(Unsafe.Add(ref searchSpace, num + 3)))
					{
						goto IL_0321;
					}
					num += 4;
				}
				while (true)
				{
					if (length > 0)
					{
						ref T reference13 = ref value;
						val = default(T);
						if (val == null)
						{
							val = reference13;
							reference13 = ref val;
						}
						if (reference13.Equals(Unsafe.Add(ref searchSpace, num)))
						{
							break;
						}
						num++;
						length--;
						continue;
					}
					return -1;
				}
			}
			goto IL_0312;
			IL_0312:
			return (int)num;
			IL_031b:
			return (int)(num + 2);
			IL_0315:
			return (int)(num + 1);
			IL_0321:
			return (int)(num + 3);
		}
		return (int)(num + 7);
	}

	public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>
	{
		int num = 0;
		while (true)
		{
			if (length - num >= 8)
			{
				T other = Unsafe.Add(ref searchSpace, num);
				if (!value0.Equals(other) && !value1.Equals(other))
				{
					other = Unsafe.Add(ref searchSpace, num + 1);
					if (value0.Equals(other) || value1.Equals(other))
					{
						goto IL_02cb;
					}
					other = Unsafe.Add(ref searchSpace, num + 2);
					if (value0.Equals(other) || value1.Equals(other))
					{
						goto IL_02cf;
					}
					other = Unsafe.Add(ref searchSpace, num + 3);
					if (!value0.Equals(other) && !value1.Equals(other))
					{
						other = Unsafe.Add(ref searchSpace, num + 4);
						if (!value0.Equals(other) && !value1.Equals(other))
						{
							other = Unsafe.Add(ref searchSpace, num + 5);
							if (!value0.Equals(other) && !value1.Equals(other))
							{
								other = Unsafe.Add(ref searchSpace, num + 6);
								if (!value0.Equals(other) && !value1.Equals(other))
								{
									other = Unsafe.Add(ref searchSpace, num + 7);
									if (value0.Equals(other) || value1.Equals(other))
									{
										break;
									}
									num += 8;
									continue;
								}
								return num + 6;
							}
							return num + 5;
						}
						return num + 4;
					}
					goto IL_02d3;
				}
			}
			else
			{
				if (length - num >= 4)
				{
					T other = Unsafe.Add(ref searchSpace, num);
					if (value0.Equals(other) || value1.Equals(other))
					{
						goto IL_02c9;
					}
					other = Unsafe.Add(ref searchSpace, num + 1);
					if (value0.Equals(other) || value1.Equals(other))
					{
						goto IL_02cb;
					}
					other = Unsafe.Add(ref searchSpace, num + 2);
					if (value0.Equals(other) || value1.Equals(other))
					{
						goto IL_02cf;
					}
					other = Unsafe.Add(ref searchSpace, num + 3);
					if (value0.Equals(other) || value1.Equals(other))
					{
						goto IL_02d3;
					}
					num += 4;
				}
				while (true)
				{
					if (num < length)
					{
						T other = Unsafe.Add(ref searchSpace, num);
						if (value0.Equals(other) || value1.Equals(other))
						{
							break;
						}
						num++;
						continue;
					}
					return -1;
				}
			}
			goto IL_02c9;
			IL_02cf:
			return num + 2;
			IL_02cb:
			return num + 1;
			IL_02d3:
			return num + 3;
			IL_02c9:
			return num;
		}
		return num + 7;
	}

	public static int IndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : IEquatable<T>
	{
		int num = 0;
		while (true)
		{
			if (length - num >= 8)
			{
				T other = Unsafe.Add(ref searchSpace, num);
				if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
				{
					other = Unsafe.Add(ref searchSpace, num + 1);
					if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
					{
						goto IL_03c2;
					}
					other = Unsafe.Add(ref searchSpace, num + 2);
					if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
					{
						goto IL_03c6;
					}
					other = Unsafe.Add(ref searchSpace, num + 3);
					if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
					{
						other = Unsafe.Add(ref searchSpace, num + 4);
						if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
						{
							other = Unsafe.Add(ref searchSpace, num + 5);
							if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
							{
								other = Unsafe.Add(ref searchSpace, num + 6);
								if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
								{
									other = Unsafe.Add(ref searchSpace, num + 7);
									if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
									{
										break;
									}
									num += 8;
									continue;
								}
								return num + 6;
							}
							return num + 5;
						}
						return num + 4;
					}
					goto IL_03ca;
				}
			}
			else
			{
				if (length - num >= 4)
				{
					T other = Unsafe.Add(ref searchSpace, num);
					if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
					{
						goto IL_03c0;
					}
					other = Unsafe.Add(ref searchSpace, num + 1);
					if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
					{
						goto IL_03c2;
					}
					other = Unsafe.Add(ref searchSpace, num + 2);
					if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
					{
						goto IL_03c6;
					}
					other = Unsafe.Add(ref searchSpace, num + 3);
					if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
					{
						goto IL_03ca;
					}
					num += 4;
				}
				while (true)
				{
					if (num < length)
					{
						T other = Unsafe.Add(ref searchSpace, num);
						if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
						{
							break;
						}
						num++;
						continue;
					}
					return -1;
				}
			}
			goto IL_03c0;
			IL_03c0:
			return num;
			IL_03c6:
			return num + 2;
			IL_03c2:
			return num + 1;
			IL_03ca:
			return num + 3;
		}
		return num + 7;
	}

	public static int IndexOfAny<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
	{
		if (valueLength == 0)
		{
			return 0;
		}
		int num = -1;
		for (int i = 0; i < valueLength; i++)
		{
			int num2 = IndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
			if ((uint)num2 < (uint)num)
			{
				num = num2;
				searchSpaceLength = num2;
				if (num == 0)
				{
					break;
				}
			}
		}
		return num;
	}

	public static int LastIndexOf<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
	{
		if (valueLength == 0)
		{
			return 0;
		}
		T value2 = value;
		ref T second = ref Unsafe.Add(ref value, 1);
		int num = valueLength - 1;
		int num2 = 0;
		while (true)
		{
			int num3 = searchSpaceLength - num2 - num;
			if (num3 <= 0)
			{
				break;
			}
			int num4 = LastIndexOf(ref searchSpace, value2, num3);
			if (num4 == -1)
			{
				break;
			}
			if (SequenceEqual(ref Unsafe.Add(ref searchSpace, num4 + 1), ref second, num))
			{
				return num4;
			}
			num2 += num3 - num4;
		}
		return -1;
	}

	public static int LastIndexOf<T>(ref T searchSpace, T value, int length) where T : IEquatable<T>
	{
		while (true)
		{
			T val;
			if (length >= 8)
			{
				length -= 8;
				ref T reference = ref value;
				val = default(T);
				if (val == null)
				{
					val = reference;
					reference = ref val;
				}
				if (reference.Equals(Unsafe.Add(ref searchSpace, length + 7)))
				{
					break;
				}
				ref T reference2 = ref value;
				val = default(T);
				if (val == null)
				{
					val = reference2;
					reference2 = ref val;
				}
				if (reference2.Equals(Unsafe.Add(ref searchSpace, length + 6)))
				{
					return length + 6;
				}
				ref T reference3 = ref value;
				val = default(T);
				if (val == null)
				{
					val = reference3;
					reference3 = ref val;
				}
				if (reference3.Equals(Unsafe.Add(ref searchSpace, length + 5)))
				{
					return length + 5;
				}
				ref T reference4 = ref value;
				val = default(T);
				if (val == null)
				{
					val = reference4;
					reference4 = ref val;
				}
				if (reference4.Equals(Unsafe.Add(ref searchSpace, length + 4)))
				{
					return length + 4;
				}
				ref T reference5 = ref value;
				val = default(T);
				if (val == null)
				{
					val = reference5;
					reference5 = ref val;
				}
				if (reference5.Equals(Unsafe.Add(ref searchSpace, length + 3)))
				{
					goto IL_02fd;
				}
				ref T reference6 = ref value;
				val = default(T);
				if (val == null)
				{
					val = reference6;
					reference6 = ref val;
				}
				if (reference6.Equals(Unsafe.Add(ref searchSpace, length + 2)))
				{
					goto IL_02f9;
				}
				ref T reference7 = ref value;
				val = default(T);
				if (val == null)
				{
					val = reference7;
					reference7 = ref val;
				}
				if (reference7.Equals(Unsafe.Add(ref searchSpace, length + 1)))
				{
					goto IL_02f5;
				}
				ref T reference8 = ref value;
				val = default(T);
				if (val == null)
				{
					val = reference8;
					reference8 = ref val;
				}
				if (!reference8.Equals(Unsafe.Add(ref searchSpace, length)))
				{
					continue;
				}
			}
			else
			{
				if (length >= 4)
				{
					length -= 4;
					ref T reference9 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference9;
						reference9 = ref val;
					}
					if (reference9.Equals(Unsafe.Add(ref searchSpace, length + 3)))
					{
						goto IL_02fd;
					}
					ref T reference10 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference10;
						reference10 = ref val;
					}
					if (reference10.Equals(Unsafe.Add(ref searchSpace, length + 2)))
					{
						goto IL_02f9;
					}
					ref T reference11 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference11;
						reference11 = ref val;
					}
					if (reference11.Equals(Unsafe.Add(ref searchSpace, length + 1)))
					{
						goto IL_02f5;
					}
					ref T reference12 = ref value;
					val = default(T);
					if (val == null)
					{
						val = reference12;
						reference12 = ref val;
					}
					if (reference12.Equals(Unsafe.Add(ref searchSpace, length)))
					{
						goto IL_02f3;
					}
				}
				ref T reference13;
				do
				{
					if (length > 0)
					{
						length--;
						reference13 = ref value;
						val = default(T);
						if (val == null)
						{
							val = reference13;
							reference13 = ref val;
						}
						continue;
					}
					return -1;
				}
				while (!reference13.Equals(Unsafe.Add(ref searchSpace, length)));
			}
			goto IL_02f3;
			IL_02f5:
			return length + 1;
			IL_02f9:
			return length + 2;
			IL_02fd:
			return length + 3;
			IL_02f3:
			return length;
		}
		return length + 7;
	}

	public static int LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, int length) where T : IEquatable<T>
	{
		while (true)
		{
			if (length >= 8)
			{
				length -= 8;
				T other = Unsafe.Add(ref searchSpace, length + 7);
				if (value0.Equals(other) || value1.Equals(other))
				{
					break;
				}
				other = Unsafe.Add(ref searchSpace, length + 6);
				if (value0.Equals(other) || value1.Equals(other))
				{
					return length + 6;
				}
				other = Unsafe.Add(ref searchSpace, length + 5);
				if (value0.Equals(other) || value1.Equals(other))
				{
					return length + 5;
				}
				other = Unsafe.Add(ref searchSpace, length + 4);
				if (value0.Equals(other) || value1.Equals(other))
				{
					return length + 4;
				}
				other = Unsafe.Add(ref searchSpace, length + 3);
				if (value0.Equals(other) || value1.Equals(other))
				{
					goto IL_02cd;
				}
				other = Unsafe.Add(ref searchSpace, length + 2);
				if (value0.Equals(other) || value1.Equals(other))
				{
					goto IL_02c9;
				}
				other = Unsafe.Add(ref searchSpace, length + 1);
				if (value0.Equals(other) || value1.Equals(other))
				{
					goto IL_02c5;
				}
				other = Unsafe.Add(ref searchSpace, length);
				if (!value0.Equals(other) && !value1.Equals(other))
				{
					continue;
				}
			}
			else
			{
				T other;
				if (length >= 4)
				{
					length -= 4;
					other = Unsafe.Add(ref searchSpace, length + 3);
					if (value0.Equals(other) || value1.Equals(other))
					{
						goto IL_02cd;
					}
					other = Unsafe.Add(ref searchSpace, length + 2);
					if (value0.Equals(other) || value1.Equals(other))
					{
						goto IL_02c9;
					}
					other = Unsafe.Add(ref searchSpace, length + 1);
					if (value0.Equals(other) || value1.Equals(other))
					{
						goto IL_02c5;
					}
					other = Unsafe.Add(ref searchSpace, length);
					if (value0.Equals(other) || value1.Equals(other))
					{
						goto IL_02c3;
					}
				}
				do
				{
					if (length > 0)
					{
						length--;
						other = Unsafe.Add(ref searchSpace, length);
						continue;
					}
					return -1;
				}
				while (!value0.Equals(other) && !value1.Equals(other));
			}
			goto IL_02c3;
			IL_02c9:
			return length + 2;
			IL_02c5:
			return length + 1;
			IL_02c3:
			return length;
			IL_02cd:
			return length + 3;
		}
		return length + 7;
	}

	public static int LastIndexOfAny<T>(ref T searchSpace, T value0, T value1, T value2, int length) where T : IEquatable<T>
	{
		while (true)
		{
			if (length >= 8)
			{
				length -= 8;
				T other = Unsafe.Add(ref searchSpace, length + 7);
				if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
				{
					break;
				}
				other = Unsafe.Add(ref searchSpace, length + 6);
				if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
				{
					return length + 6;
				}
				other = Unsafe.Add(ref searchSpace, length + 5);
				if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
				{
					return length + 5;
				}
				other = Unsafe.Add(ref searchSpace, length + 4);
				if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
				{
					return length + 4;
				}
				other = Unsafe.Add(ref searchSpace, length + 3);
				if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
				{
					goto IL_03da;
				}
				other = Unsafe.Add(ref searchSpace, length + 2);
				if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
				{
					goto IL_03d5;
				}
				other = Unsafe.Add(ref searchSpace, length + 1);
				if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
				{
					goto IL_03d0;
				}
				other = Unsafe.Add(ref searchSpace, length);
				if (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other))
				{
					continue;
				}
			}
			else
			{
				T other;
				if (length >= 4)
				{
					length -= 4;
					other = Unsafe.Add(ref searchSpace, length + 3);
					if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
					{
						goto IL_03da;
					}
					other = Unsafe.Add(ref searchSpace, length + 2);
					if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
					{
						goto IL_03d5;
					}
					other = Unsafe.Add(ref searchSpace, length + 1);
					if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
					{
						goto IL_03d0;
					}
					other = Unsafe.Add(ref searchSpace, length);
					if (value0.Equals(other) || value1.Equals(other) || value2.Equals(other))
					{
						goto IL_03cd;
					}
				}
				do
				{
					if (length > 0)
					{
						length--;
						other = Unsafe.Add(ref searchSpace, length);
						continue;
					}
					return -1;
				}
				while (!value0.Equals(other) && !value1.Equals(other) && !value2.Equals(other));
			}
			goto IL_03cd;
			IL_03d0:
			return length + 1;
			IL_03d5:
			return length + 2;
			IL_03da:
			return length + 3;
			IL_03cd:
			return length;
		}
		return length + 7;
	}

	public static int LastIndexOfAny<T>(ref T searchSpace, int searchSpaceLength, ref T value, int valueLength) where T : IEquatable<T>
	{
		if (valueLength == 0)
		{
			return 0;
		}
		int num = -1;
		for (int i = 0; i < valueLength; i++)
		{
			int num2 = LastIndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public static bool SequenceEqual<T>(ref T first, ref T second, int length) where T : IEquatable<T>
	{
		if (!Unsafe.AreSame(ref first, ref second))
		{
			nuint num = 0u;
			while (true)
			{
				T val;
				if (length >= 8)
				{
					length -= 8;
					ref T reference = ref Unsafe.Add(ref first, num);
					val = default(T);
					if (val == null)
					{
						val = reference;
						reference = ref val;
					}
					if (reference.Equals(Unsafe.Add(ref second, num)))
					{
						ref T reference2 = ref Unsafe.Add(ref first, num + 1);
						val = default(T);
						if (val == null)
						{
							val = reference2;
							reference2 = ref val;
						}
						if (reference2.Equals(Unsafe.Add(ref second, num + 1)))
						{
							ref T reference3 = ref Unsafe.Add(ref first, num + 2);
							val = default(T);
							if (val == null)
							{
								val = reference3;
								reference3 = ref val;
							}
							if (reference3.Equals(Unsafe.Add(ref second, num + 2)))
							{
								ref T reference4 = ref Unsafe.Add(ref first, num + 3);
								val = default(T);
								if (val == null)
								{
									val = reference4;
									reference4 = ref val;
								}
								if (reference4.Equals(Unsafe.Add(ref second, num + 3)))
								{
									ref T reference5 = ref Unsafe.Add(ref first, num + 4);
									val = default(T);
									if (val == null)
									{
										val = reference5;
										reference5 = ref val;
									}
									if (reference5.Equals(Unsafe.Add(ref second, num + 4)))
									{
										ref T reference6 = ref Unsafe.Add(ref first, num + 5);
										val = default(T);
										if (val == null)
										{
											val = reference6;
											reference6 = ref val;
										}
										if (reference6.Equals(Unsafe.Add(ref second, num + 5)))
										{
											ref T reference7 = ref Unsafe.Add(ref first, num + 6);
											val = default(T);
											if (val == null)
											{
												val = reference7;
												reference7 = ref val;
											}
											if (reference7.Equals(Unsafe.Add(ref second, num + 6)))
											{
												ref T reference8 = ref Unsafe.Add(ref first, num + 7);
												val = default(T);
												if (val == null)
												{
													val = reference8;
													reference8 = ref val;
												}
												if (reference8.Equals(Unsafe.Add(ref second, num + 7)))
												{
													num += 8;
													continue;
												}
											}
										}
									}
								}
							}
						}
					}
				}
				else
				{
					if (length < 4)
					{
						goto IL_0377;
					}
					length -= 4;
					ref T reference9 = ref Unsafe.Add(ref first, num);
					val = default(T);
					if (val == null)
					{
						val = reference9;
						reference9 = ref val;
					}
					if (reference9.Equals(Unsafe.Add(ref second, num)))
					{
						ref T reference10 = ref Unsafe.Add(ref first, num + 1);
						val = default(T);
						if (val == null)
						{
							val = reference10;
							reference10 = ref val;
						}
						if (reference10.Equals(Unsafe.Add(ref second, num + 1)))
						{
							ref T reference11 = ref Unsafe.Add(ref first, num + 2);
							val = default(T);
							if (val == null)
							{
								val = reference11;
								reference11 = ref val;
							}
							if (reference11.Equals(Unsafe.Add(ref second, num + 2)))
							{
								ref T reference12 = ref Unsafe.Add(ref first, num + 3);
								val = default(T);
								if (val == null)
								{
									val = reference12;
									reference12 = ref val;
								}
								if (reference12.Equals(Unsafe.Add(ref second, num + 3)))
								{
									num += 4;
									goto IL_0377;
								}
							}
						}
					}
				}
				goto IL_037d;
				IL_0377:
				while (length > 0)
				{
					ref T reference13 = ref Unsafe.Add(ref first, num);
					val = default(T);
					if (val == null)
					{
						val = reference13;
						reference13 = ref val;
					}
					if (reference13.Equals(Unsafe.Add(ref second, num)))
					{
						num++;
						length--;
						continue;
					}
					goto IL_037d;
				}
				break;
				IL_037d:
				return false;
			}
		}
		return true;
	}

	public static int SequenceCompareTo<T>(ref T first, int firstLength, ref T second, int secondLength) where T : IComparable<T>
	{
		int num = firstLength;
		if (num > secondLength)
		{
			num = secondLength;
		}
		for (int i = 0; i < num; i++)
		{
			ref T reference = ref Unsafe.Add(ref first, i);
			T val = default(T);
			if (val == null)
			{
				val = reference;
				reference = ref val;
			}
			int num2 = reference.CompareTo(Unsafe.Add(ref second, i));
			if (num2 != 0)
			{
				return num2;
			}
		}
		return firstLength.CompareTo(secondLength);
	}
}
