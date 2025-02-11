using System;
using System.IO;
using System.Runtime.InteropServices;
using MS.Internal.PresentationCore;

namespace MS.Internal.Ink.InkSerializedFormat;

internal static class SerializationHelper
{
	public static uint VarSize(uint Value)
	{
		if (Value < 128)
		{
			return 1u;
		}
		if (Value < 16384)
		{
			return 2u;
		}
		if (Value < 2097152)
		{
			return 3u;
		}
		if (Value < 268435456)
		{
			return 4u;
		}
		return 5u;
	}

	public static uint Encode(Stream strm, uint Value)
	{
		ulong num = 0uL;
		while (Value >= 128)
		{
			strm.WriteByte((byte)(0x80 | (Value & 0x7F)));
			Value >>= 7;
			num++;
		}
		strm.WriteByte((byte)Value);
		return (uint)(num + 1);
	}

	public static uint EncodeLarge(Stream strm, ulong ulValue)
	{
		uint num = 0u;
		while (ulValue >= 128)
		{
			strm.WriteByte((byte)(0x80 | (ulValue & 0x7F)));
			ulValue >>= 7;
			num++;
		}
		strm.WriteByte((byte)ulValue);
		return num + 1;
	}

	public static uint SignEncode(Stream strm, int Value)
	{
		ulong num = 0uL;
		if (int.MinValue == Value)
		{
			num = 4294967297uL;
		}
		else
		{
			num = (ulong)Math.Abs(Value);
			num <<= 1;
			if (Value < 0)
			{
				num |= 1;
			}
		}
		return EncodeLarge(strm, num);
	}

	public static uint Decode(Stream strm, out uint dw)
	{
		int num = 0;
		byte b = 0;
		uint num2 = 0u;
		dw = 0u;
		do
		{
			b = (byte)strm.ReadByte();
			num2++;
			dw += (uint)((b & 0x7F) << num);
			num += 7;
		}
		while ((b & 0x80) > 0 && num < 29);
		return num2;
	}

	public static uint DecodeLarge(Stream strm, out ulong ull)
	{
		int num = 0;
		byte b = 0;
		uint num2 = 0u;
		ull = 0uL;
		do
		{
			b = (byte)strm.ReadByte();
			num2++;
			long num3 = (byte)(b & 0x7F);
			ull |= (ulong)(num3 << num);
			num += 7;
		}
		while ((b & 0x80) > 0 && num < 57);
		return num2;
	}

	public static uint SignDecode(Stream strm, out int i)
	{
		i = 0;
		ulong ull = 0uL;
		uint num = DecodeLarge(strm, out ull);
		if (num != 0)
		{
			bool flag = false;
			if ((ull & 1) != 0)
			{
				flag = true;
			}
			ull >>= 1;
			long num2 = (long)ull;
			i = (int)(flag ? (-num2) : num2);
		}
		return num;
	}

	public static VarEnum ConvertToVarEnum(Type type, bool throwOnError)
	{
		if (typeof(char) == type)
		{
			return VarEnum.VT_I1;
		}
		if (typeof(char[]) == type)
		{
			return (VarEnum)8208;
		}
		if (typeof(byte) == type)
		{
			return VarEnum.VT_UI1;
		}
		if (typeof(byte[]) == type)
		{
			return (VarEnum)8209;
		}
		if (typeof(short) == type)
		{
			return VarEnum.VT_I2;
		}
		if (typeof(short[]) == type)
		{
			return (VarEnum)8194;
		}
		if (typeof(ushort) == type)
		{
			return VarEnum.VT_UI2;
		}
		if (typeof(ushort[]) == type)
		{
			return (VarEnum)8210;
		}
		if (typeof(int) == type)
		{
			return VarEnum.VT_I4;
		}
		if (typeof(int[]) == type)
		{
			return (VarEnum)8195;
		}
		if (typeof(uint) == type)
		{
			return VarEnum.VT_UI4;
		}
		if (typeof(uint[]) == type)
		{
			return (VarEnum)8211;
		}
		if (typeof(long) == type)
		{
			return VarEnum.VT_I8;
		}
		if (typeof(long[]) == type)
		{
			return (VarEnum)8212;
		}
		if (typeof(ulong) == type)
		{
			return VarEnum.VT_UI8;
		}
		if (typeof(ulong[]) == type)
		{
			return (VarEnum)8213;
		}
		if (typeof(float) == type)
		{
			return VarEnum.VT_R4;
		}
		if (typeof(float[]) == type)
		{
			return (VarEnum)8196;
		}
		if (typeof(double) == type)
		{
			return VarEnum.VT_R8;
		}
		if (typeof(double[]) == type)
		{
			return (VarEnum)8197;
		}
		if (typeof(DateTime) == type)
		{
			return VarEnum.VT_DATE;
		}
		if (typeof(DateTime[]) == type)
		{
			return (VarEnum)8199;
		}
		if (typeof(bool) == type)
		{
			return VarEnum.VT_BOOL;
		}
		if (typeof(bool[]) == type)
		{
			return (VarEnum)8203;
		}
		if (typeof(string) == type)
		{
			return VarEnum.VT_BSTR;
		}
		if (typeof(decimal) == type)
		{
			return VarEnum.VT_DECIMAL;
		}
		if (typeof(decimal[]) == type)
		{
			return (VarEnum)8206;
		}
		if (throwOnError)
		{
			throw new ArgumentException(SR.InvalidDataTypeForExtendedProperty);
		}
		return VarEnum.VT_UNKNOWN;
	}
}
