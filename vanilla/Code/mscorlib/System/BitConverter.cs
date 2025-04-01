using System.Security;

namespace System;

/// <summary>Converts base data types to an array of bytes, and an array of bytes to base data types.</summary>
/// <filterpriority>2</filterpriority>
public static class BitConverter
{
	/// <summary>Indicates the byte order ("endianness") in which data is stored in this computer architecture.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly bool IsLittleEndian = AmILittleEndian();

	private unsafe static bool AmILittleEndian()
	{
		double num = 1.0;
		byte* ptr = (byte*)(&num);
		return *ptr == 0;
	}

	/// <summary>Returns the specified Boolean value as an array of bytes.</summary>
	/// <returns>An array of bytes with length 1.</returns>
	/// <param name="value">A Boolean value. </param>
	/// <filterpriority>1</filterpriority>
	public static byte[] GetBytes(bool value)
	{
		return new byte[1] { (byte)(value ? 1 : 0) };
	}

	/// <summary>Returns the specified Unicode character value as an array of bytes.</summary>
	/// <returns>An array of bytes with length 2.</returns>
	/// <param name="value">A character to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static byte[] GetBytes(char value)
	{
		return GetBytes((short)value);
	}

	/// <summary>Returns the specified 16-bit signed integer value as an array of bytes.</summary>
	/// <returns>An array of bytes with length 2.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static byte[] GetBytes(short value)
	{
		byte[] array = new byte[2];
		fixed (byte* ptr = array)
		{
			*(short*)ptr = value;
		}
		return array;
	}

	/// <summary>Returns the specified 32-bit signed integer value as an array of bytes.</summary>
	/// <returns>An array of bytes with length 4.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static byte[] GetBytes(int value)
	{
		byte[] array = new byte[4];
		fixed (byte* ptr = array)
		{
			*(int*)ptr = value;
		}
		return array;
	}

	/// <summary>Returns the specified 64-bit signed integer value as an array of bytes.</summary>
	/// <returns>An array of bytes with length 8.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static byte[] GetBytes(long value)
	{
		byte[] array = new byte[8];
		fixed (byte* ptr = array)
		{
			*(long*)ptr = value;
		}
		return array;
	}

	/// <summary>Returns the specified 16-bit unsigned integer value as an array of bytes.</summary>
	/// <returns>An array of bytes with length 2.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static byte[] GetBytes(ushort value)
	{
		return GetBytes((short)value);
	}

	/// <summary>Returns the specified 32-bit unsigned integer value as an array of bytes.</summary>
	/// <returns>An array of bytes with length 4.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static byte[] GetBytes(uint value)
	{
		return GetBytes((int)value);
	}

	/// <summary>Returns the specified 64-bit unsigned integer value as an array of bytes.</summary>
	/// <returns>An array of bytes with length 8.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static byte[] GetBytes(ulong value)
	{
		return GetBytes((long)value);
	}

	/// <summary>Returns the specified single-precision floating point value as an array of bytes.</summary>
	/// <returns>An array of bytes with length 4.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static byte[] GetBytes(float value)
	{
		return GetBytes(*(int*)(&value));
	}

	/// <summary>Returns the specified double-precision floating point value as an array of bytes.</summary>
	/// <returns>An array of bytes with length 8.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static byte[] GetBytes(double value)
	{
		return GetBytes(*(long*)(&value));
	}

	/// <summary>Returns a Unicode character converted from two bytes at a specified position in a byte array.</summary>
	/// <returns>A character formed by two bytes beginning at <paramref name="startIndex" />.</returns>
	/// <param name="value">An array. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> equals the length of <paramref name="value" /> minus 1.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	public static char ToChar(byte[] value, int startIndex)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
		}
		if ((uint)startIndex >= value.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (startIndex > value.Length - 2)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		return (char)ToInt16(value, startIndex);
	}

	/// <summary>Returns a 16-bit signed integer converted from two bytes at a specified position in a byte array.</summary>
	/// <returns>A 16-bit signed integer formed by two bytes beginning at <paramref name="startIndex" />.</returns>
	/// <param name="value">An array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> equals the length of <paramref name="value" /> minus 1.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static short ToInt16(byte[] value, int startIndex)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
		}
		if ((uint)startIndex >= value.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (startIndex > value.Length - 2)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		fixed (byte* ptr = &value[startIndex])
		{
			if (startIndex % 2 == 0)
			{
				return *(short*)ptr;
			}
			if (IsLittleEndian)
			{
				return (short)(*ptr | (ptr[1] << 8));
			}
			return (short)((*ptr << 8) | ptr[1]);
		}
	}

	/// <summary>Returns a 32-bit signed integer converted from four bytes at a specified position in a byte array.</summary>
	/// <returns>A 32-bit signed integer formed by four bytes beginning at <paramref name="startIndex" />.</returns>
	/// <param name="value">An array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> is greater than or equal to the length of <paramref name="value" /> minus 3, and is less than or equal to the length of <paramref name="value" /> minus 1.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static int ToInt32(byte[] value, int startIndex)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
		}
		if ((uint)startIndex >= value.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (startIndex > value.Length - 4)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		fixed (byte* ptr = &value[startIndex])
		{
			if (startIndex % 4 == 0)
			{
				return *(int*)ptr;
			}
			if (IsLittleEndian)
			{
				return *ptr | (ptr[1] << 8) | (ptr[2] << 16) | (ptr[3] << 24);
			}
			return (*ptr << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3];
		}
	}

	/// <summary>Returns a 64-bit signed integer converted from eight bytes at a specified position in a byte array.</summary>
	/// <returns>A 64-bit signed integer formed by eight bytes beginning at <paramref name="startIndex" />.</returns>
	/// <param name="value">An array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> is greater than or equal to the length of <paramref name="value" /> minus 7, and is less than or equal to the length of <paramref name="value" /> minus 1.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static long ToInt64(byte[] value, int startIndex)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
		}
		if ((uint)startIndex >= value.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (startIndex > value.Length - 8)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		fixed (byte* ptr = &value[startIndex])
		{
			if (startIndex % 8 == 0)
			{
				return *(long*)ptr;
			}
			if (IsLittleEndian)
			{
				int num = *ptr | (ptr[1] << 8) | (ptr[2] << 16) | (ptr[3] << 24);
				int num2 = ptr[4] | (ptr[5] << 8) | (ptr[6] << 16) | (ptr[7] << 24);
				return (uint)num | ((long)num2 << 32);
			}
			int num3 = (*ptr << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3];
			return (uint)((ptr[4] << 24) | (ptr[5] << 16) | (ptr[6] << 8) | ptr[7]) | ((long)num3 << 32);
		}
	}

	/// <summary>Returns a 16-bit unsigned integer converted from two bytes at a specified position in a byte array.</summary>
	/// <returns>A 16-bit unsigned integer formed by two bytes beginning at <paramref name="startIndex" />.</returns>
	/// <param name="value">The array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> equals the length of <paramref name="value" /> minus 1.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ushort ToUInt16(byte[] value, int startIndex)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
		}
		if ((uint)startIndex >= value.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (startIndex > value.Length - 2)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		return (ushort)ToInt16(value, startIndex);
	}

	/// <summary>Returns a 32-bit unsigned integer converted from four bytes at a specified position in a byte array.</summary>
	/// <returns>A 32-bit unsigned integer formed by four bytes beginning at <paramref name="startIndex" />.</returns>
	/// <param name="value">An array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> is greater than or equal to the length of <paramref name="value" /> minus 3, and is less than or equal to the length of <paramref name="value" /> minus 1.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static uint ToUInt32(byte[] value, int startIndex)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
		}
		if ((uint)startIndex >= value.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (startIndex > value.Length - 4)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		return (uint)ToInt32(value, startIndex);
	}

	/// <summary>Returns a 64-bit unsigned integer converted from eight bytes at a specified position in a byte array.</summary>
	/// <returns>A 64-bit unsigned integer formed by the eight bytes beginning at <paramref name="startIndex" />.</returns>
	/// <param name="value">An array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> is greater than or equal to the length of <paramref name="value" /> minus 7, and is less than or equal to the length of <paramref name="value" /> minus 1.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	[CLSCompliant(false)]
	public static ulong ToUInt64(byte[] value, int startIndex)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
		}
		if ((uint)startIndex >= value.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (startIndex > value.Length - 8)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		return (ulong)ToInt64(value, startIndex);
	}

	/// <summary>Returns a single-precision floating point number converted from four bytes at a specified position in a byte array.</summary>
	/// <returns>A single-precision floating point number formed by four bytes beginning at <paramref name="startIndex" />.</returns>
	/// <param name="value">An array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> is greater than or equal to the length of <paramref name="value" /> minus 3, and is less than or equal to the length of <paramref name="value" /> minus 1.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static float ToSingle(byte[] value, int startIndex)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
		}
		if ((uint)startIndex >= value.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (startIndex > value.Length - 4)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		int num = ToInt32(value, startIndex);
		return *(float*)(&num);
	}

	/// <summary>Returns a double-precision floating point number converted from eight bytes at a specified position in a byte array.</summary>
	/// <returns>A double precision floating point number formed by eight bytes beginning at <paramref name="startIndex" />.</returns>
	/// <param name="value">An array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="startIndex" /> is greater than or equal to the length of <paramref name="value" /> minus 7, and is less than or equal to the length of <paramref name="value" /> minus 1.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static double ToDouble(byte[] value, int startIndex)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
		}
		if ((uint)startIndex >= value.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
		}
		if (startIndex > value.Length - 8)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		long num = ToInt64(value, startIndex);
		return *(double*)(&num);
	}

	private static char GetHexValue(int i)
	{
		if (i < 10)
		{
			return (char)(i + 48);
		}
		return (char)(i - 10 + 65);
	}

	/// <summary>Converts the numeric value of each element of a specified subarray of bytes to its equivalent hexadecimal string representation.</summary>
	/// <returns>A string of hexadecimal pairs separated by hyphens, where each pair represents the corresponding element in a subarray of <paramref name="value" />; for example, "7F-2C-4A-00".</returns>
	/// <param name="value">An array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <param name="length">The number of array elements in <paramref name="value" /> to convert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or-<paramref name="startIndex" /> is greater than zero and is greater than or equal to the length of <paramref name="value" />.</exception>
	/// <exception cref="T:System.ArgumentException">The combination of <paramref name="startIndex" /> and <paramref name="length" /> does not specify a position within <paramref name="value" />; that is, the <paramref name="startIndex" /> parameter is greater than the length of <paramref name="value" /> minus the <paramref name="length" /> parameter.</exception>
	/// <filterpriority>1</filterpriority>
	public static string ToString(byte[] value, int startIndex, int length)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex < 0 || (startIndex >= value.Length && startIndex > 0))
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("StartIndex cannot be less than zero."));
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("Value must be positive."));
		}
		if (startIndex > value.Length - length)
		{
			throw new ArgumentException(Environment.GetResourceString("Destination array is not long enough to copy all the items in the collection. Check array index and length."));
		}
		if (length == 0)
		{
			return string.Empty;
		}
		if (length > 715827882)
		{
			throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("The specified length exceeds the maximum value of {0}.", 715827882));
		}
		int num = length * 3;
		char[] array = new char[num];
		int num2 = 0;
		int num3 = startIndex;
		for (num2 = 0; num2 < num; num2 += 3)
		{
			byte b = value[num3++];
			array[num2] = GetHexValue(b / 16);
			array[num2 + 1] = GetHexValue(b % 16);
			array[num2 + 2] = '-';
		}
		return new string(array, 0, array.Length - 1);
	}

	/// <summary>Converts the numeric value of each element of a specified array of bytes to its equivalent hexadecimal string representation.</summary>
	/// <returns>A string of hexadecimal pairs separated by hyphens, where each pair represents the corresponding element in <paramref name="value" />; for example, "7F-2C-4A-00".</returns>
	/// <param name="value">An array of bytes. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static string ToString(byte[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return ToString(value, 0, value.Length);
	}

	/// <summary>Converts the numeric value of each element of a specified subarray of bytes to its equivalent hexadecimal string representation.</summary>
	/// <returns>A string of hexadecimal pairs separated by hyphens, where each pair represents the corresponding element in a subarray of <paramref name="value" />; for example, "7F-2C-4A-00".</returns>
	/// <param name="value">An array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	public static string ToString(byte[] value, int startIndex)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return ToString(value, startIndex, value.Length - startIndex);
	}

	/// <summary>Returns a Boolean value converted from one byte at a specified position in a byte array.</summary>
	/// <returns>true if the byte at <paramref name="startIndex" /> in <paramref name="value" /> is nonzero; otherwise, false.</returns>
	/// <param name="value">An array of bytes. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of <paramref name="value" /> minus 1. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool ToBoolean(byte[] value, int startIndex)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Non-negative number required."));
		}
		if (startIndex > value.Length - 1)
		{
			throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("Index was out of range. Must be non-negative and less than the size of the collection."));
		}
		if (value[startIndex] != 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>Converts the specified double-precision floating point number to a 64-bit signed integer.</summary>
	/// <returns>A 64-bit signed integer whose value is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static long DoubleToInt64Bits(double value)
	{
		return *(long*)(&value);
	}

	/// <summary>Converts the specified 64-bit signed integer to a double-precision floating point number.</summary>
	/// <returns>A double-precision floating point number whose value is equivalent to <paramref name="value" />.</returns>
	/// <param name="value">The number to convert. </param>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public unsafe static double Int64BitsToDouble(long value)
	{
		return *(double*)(&value);
	}
}
