using System;
using System.Collections.Generic;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class MultiByteCodec
{
	internal MultiByteCodec()
	{
	}

	internal void Encode(uint data, List<byte> output)
	{
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		while (data > 127)
		{
			byte item = (byte)(0x80 | ((byte)data & 0x7F));
			output.Add(item);
			data >>= 7;
		}
		byte item2 = (byte)(data & 0x7F);
		output.Add(item2);
	}

	internal void SignEncode(int data, List<byte> output)
	{
		uint num = 0u;
		num = (uint)((data >= 0) ? (data << 1) : ((-data << 1) | 1));
		Encode(num, output);
	}

	internal uint Decode(byte[] input, int inputIndex, ref uint data)
	{
		uint num = ((input.Length - inputIndex > 5) ? 5u : ((uint)(input.Length - inputIndex)));
		uint num2 = 0u;
		data = 0u;
		for (; num2 < num && input[num2] > 127; num2++)
		{
			int num3 = (int)(num2 * 7);
			data |= (uint)((input[num2] & 0x7F) << num3);
		}
		if (num2 < num)
		{
			int num4 = (int)(num2 * 7);
			data |= (uint)((input[num2] & 0x7F) << num4);
			return num2 + 1;
		}
		throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("invalid input in MultiByteCodec.Decode"));
	}

	internal uint SignDecode(byte[] input, int inputIndex, ref int data)
	{
		if (inputIndex >= input.Length)
		{
			throw new ArgumentOutOfRangeException("inputIndex");
		}
		uint data2 = 0u;
		uint result = Decode(input, inputIndex, ref data2);
		data = (int)(((1 & data2) != 0) ? (0 - (data2 >> 1)) : (data2 >> 1));
		return result;
	}
}
