using System;
using System.Collections.Generic;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class GorillaCodec
{
	private static GorillaAlgoByte[] _gorIndexMap;

	private static byte[] _gorIndexOffset;

	static GorillaCodec()
	{
		_gorIndexMap = new GorillaAlgoByte[24]
		{
			new GorillaAlgoByte(8u, 0u),
			new GorillaAlgoByte(1u, 0u),
			new GorillaAlgoByte(1u, 1u),
			new GorillaAlgoByte(1u, 2u),
			new GorillaAlgoByte(1u, 3u),
			new GorillaAlgoByte(1u, 4u),
			new GorillaAlgoByte(1u, 5u),
			new GorillaAlgoByte(1u, 6u),
			new GorillaAlgoByte(1u, 7u),
			new GorillaAlgoByte(2u, 0u),
			new GorillaAlgoByte(2u, 1u),
			new GorillaAlgoByte(2u, 2u),
			new GorillaAlgoByte(2u, 3u),
			new GorillaAlgoByte(3u, 0u),
			new GorillaAlgoByte(3u, 1u),
			new GorillaAlgoByte(3u, 2u),
			new GorillaAlgoByte(4u, 0u),
			new GorillaAlgoByte(4u, 1u),
			new GorillaAlgoByte(5u, 0u),
			new GorillaAlgoByte(5u, 1u),
			new GorillaAlgoByte(6u, 0u),
			new GorillaAlgoByte(6u, 1u),
			new GorillaAlgoByte(7u, 0u),
			new GorillaAlgoByte(7u, 1u)
		};
		_gorIndexOffset = new byte[8] { 0, 1, 9, 13, 16, 18, 20, 22 };
	}

	internal byte FindPacketAlgoByte(int[] input, bool testDelDel)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (input.Length == 0)
		{
			return 0;
		}
		testDelDel = testDelDel && input.Length < 3;
		uint num = 1u;
		int xfData = 0;
		int extra = 0;
		DeltaDelta deltaDelta = new DeltaDelta();
		int max2;
		int min;
		int max;
		int min2 = (max2 = (min = (max = input[0])));
		if (testDelDel)
		{
			deltaDelta.Transform(input[0], ref xfData, ref extra);
			deltaDelta.Transform(input[1], ref xfData, ref extra);
			if (extra != 0)
			{
				testDelDel = false;
			}
		}
		if (testDelDel)
		{
			deltaDelta.Transform(input[2], ref xfData, ref extra);
			if (extra != 0)
			{
				testDelDel = false;
			}
			else
			{
				min = (max = xfData);
				UpdateMinMax(input[1], ref max2, ref min2);
				UpdateMinMax(input[2], ref max2, ref min2);
				num = 3u;
			}
		}
		for (uint num2 = num; num2 < input.Length; num2++)
		{
			UpdateMinMax(input[num2], ref max2, ref min2);
			if (testDelDel)
			{
				deltaDelta.Transform(input[num2], ref xfData, ref extra);
				if (extra != 0)
				{
					testDelDel = false;
				}
				else
				{
					UpdateMinMax(xfData, ref max, ref min);
				}
			}
		}
		uint num3 = (uint)Math.Max(MathHelper.AbsNoThrow(min), MathHelper.AbsNoThrow(max));
		uint num4 = (uint)Math.Max(MathHelper.AbsNoThrow(min2), MathHelper.AbsNoThrow(max2));
		if (testDelDel && num3 >> 1 < num4)
		{
			num4 = num3;
		}
		else
		{
			testDelDel = false;
		}
		int i;
		for (i = 0; num4 >> i != 0 && 31 > i; i++)
		{
		}
		i++;
		return (byte)((byte)(i & 0x1F) | (testDelDel ? 32 : 0));
	}

	internal byte FindPropAlgoByte(byte[] input)
	{
		if (input.Length == 0)
		{
			return 0;
		}
		int num = (((input.Length & 3) == 0) ? (input.Length >> 2) : 0);
		BitStreamReader bitStreamReader = null;
		if (num > 0)
		{
			bitStreamReader = new BitStreamReader(input);
		}
		int num2 = (((input.Length & 1) == 0) ? (input.Length >> 1) : 0);
		BitStreamReader bitStreamReader2 = null;
		if (num2 > 0)
		{
			bitStreamReader2 = new BitStreamReader(input);
		}
		int max = 0;
		int min = 0;
		ushort num3 = 0;
		byte b = input[0];
		uint num4 = 0u;
		for (num4 = 0u; num4 < num; num4++)
		{
			b = Math.Max(input[num4], b);
			num3 = Math.Max((ushort)bitStreamReader2.ReadUInt16Reverse(16), num3);
			UpdateMinMax((int)bitStreamReader.ReadUInt32Reverse(32), ref max, ref min);
		}
		for (; num4 < num2; num4++)
		{
			b = Math.Max(input[num4], b);
			num3 = Math.Max((ushort)bitStreamReader2.ReadUInt16Reverse(16), num3);
		}
		for (; num4 < input.Length; num4++)
		{
			b = Math.Max(input[num4], b);
		}
		int i = 1;
		for (uint num5 = b; num5 >> i != 0 && (long)i < 8L; i++)
		{
		}
		int num6 = (((~(i * input.Length) & 7) + 1) & 7) / i;
		if (num2 > 0)
		{
			int j = 1;
			for (uint num5 = num3; num5 >> j != 0 && (long)j < 16L; j++)
			{
			}
			if (j < i << 1)
			{
				i = j;
				num6 = (((~(i * num2) & 7) + 1) & 7) / i;
			}
			else
			{
				num2 = 0;
			}
		}
		if (num > 0)
		{
			int k = 0;
			for (uint num5 = (uint)Math.Max(MathHelper.AbsNoThrow(min), MathHelper.AbsNoThrow(max)); num5 >> k != 0 && 31 > k; k++)
			{
			}
			k++;
			if (k < ((0 < num2) ? (i << 1) : (i << 2)))
			{
				i = k;
				num6 = (((~(i * num) & 7) + 1) & 7) / i;
				num2 = 0;
			}
			else
			{
				num = 0;
			}
		}
		byte b2 = (byte)((0 < num) ? 64u : ((0 < num2) ? 32u : 0u));
		if (8 == i && num + num2 == 0)
		{
			return 0;
		}
		if (i > 7)
		{
			return (byte)(b2 | (byte)(16 + i));
		}
		return (byte)(b2 | (byte)(_gorIndexOffset[i] + num6));
	}

	internal void GetPropertyBitCount(byte algorithmByte, ref int countPerItem, ref int bitCount, ref int padCount)
	{
		int num = 0;
		if ((algorithmByte & 0x40) != 0)
		{
			countPerItem = 4;
			num = algorithmByte & 0x3F;
		}
		else
		{
			countPerItem = (((algorithmByte & 0x20) == 0) ? 1 : 2);
			num = algorithmByte & 0x1F;
		}
		bitCount = num - 16;
		padCount = 0;
		if (num < _gorIndexMap.Length && num >= 0)
		{
			bitCount = (int)_gorIndexMap[num].BitCount;
			padCount = (int)_gorIndexMap[num].PadCount;
		}
	}

	internal void Compress(int bitCount, int[] input, int startInputIndex, DeltaDelta dtxf, List<byte> compressedData)
	{
		if (input == null || compressedData == null)
		{
			throw new ArgumentNullException(StrokeCollectionSerializer.ISFDebugMessage("input or compressed data was null in Compress"));
		}
		if (bitCount < 0)
		{
			throw new ArgumentOutOfRangeException("bitCount");
		}
		if (bitCount == 0)
		{
			bitCount = (int)(Native.SizeOfInt << 3);
		}
		BitStreamWriter bitStreamWriter = new BitStreamWriter(compressedData);
		if (dtxf != null)
		{
			int xfData = 0;
			int extra = 0;
			for (int i = startInputIndex; i < input.Length; i++)
			{
				dtxf.Transform(input[i], ref xfData, ref extra);
				if (extra != 0)
				{
					throw new InvalidOperationException(StrokeCollectionSerializer.ISFDebugMessage("Transform returned unexpected results"));
				}
				bitStreamWriter.Write((uint)xfData, bitCount);
			}
		}
		else
		{
			for (int j = startInputIndex; j < input.Length; j++)
			{
				bitStreamWriter.Write((uint)input[j], bitCount);
			}
		}
	}

	internal void Compress(int bitCount, BitStreamReader reader, GorillaEncodingType encodingType, int unitsToEncode, List<byte> compressedData)
	{
		if (reader == null || compressedData == null)
		{
			throw new ArgumentNullException(StrokeCollectionSerializer.ISFDebugMessage("reader or compressedData was null in compress"));
		}
		if (bitCount < 0)
		{
			throw new ArgumentOutOfRangeException("bitCount");
		}
		if (unitsToEncode < 0)
		{
			throw new ArgumentOutOfRangeException("unitsToEncode");
		}
		if (bitCount == 0)
		{
			bitCount = encodingType switch
			{
				GorillaEncodingType.Int => 32, 
				GorillaEncodingType.Short => 16, 
				GorillaEncodingType.Byte => 8, 
				_ => throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("bogus GorillaEncodingType passed to compress")), 
			};
		}
		BitStreamWriter bitStreamWriter = new BitStreamWriter(compressedData);
		while (!reader.EndOfStream && unitsToEncode > 0)
		{
			int dataFromReader = GetDataFromReader(reader, encodingType);
			bitStreamWriter.Write((uint)dataFromReader, bitCount);
			unitsToEncode--;
		}
	}

	private int GetDataFromReader(BitStreamReader reader, GorillaEncodingType type)
	{
		return type switch
		{
			GorillaEncodingType.Int => (int)reader.ReadUInt32Reverse(32), 
			GorillaEncodingType.Short => (int)reader.ReadUInt16Reverse(16), 
			GorillaEncodingType.Byte => reader.ReadByte(8), 
			_ => throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("bogus GorillaEncodingType passed to GetDataFromReader")), 
		};
	}

	internal uint Uncompress(int bitCount, byte[] input, int inputIndex, DeltaDelta dtxf, int[] outputBuffer, int outputBufferIndex)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (inputIndex >= input.Length)
		{
			throw new ArgumentOutOfRangeException("inputIndex");
		}
		if (outputBuffer == null)
		{
			throw new ArgumentNullException("outputBuffer");
		}
		if (outputBufferIndex >= outputBuffer.Length)
		{
			throw new ArgumentOutOfRangeException("outputBufferIndex");
		}
		if (bitCount < 0)
		{
			throw new ArgumentOutOfRangeException("bitCount");
		}
		if (bitCount == 0)
		{
			bitCount = (int)(Native.SizeOfInt << 3);
		}
		uint num = (uint)(-1 << bitCount - 1);
		uint num2 = 0u;
		BitStreamReader bitStreamReader = new BitStreamReader(input, inputIndex);
		if (dtxf != null)
		{
			while (!bitStreamReader.EndOfStream)
			{
				num2 = bitStreamReader.ReadUInt32(bitCount);
				num2 = (((num2 & num) != 0) ? (num | num2) : num2);
				int num3 = dtxf.InverseTransform((int)num2, 0);
				outputBuffer[outputBufferIndex++] = num3;
				if (outputBufferIndex == outputBuffer.Length)
				{
					break;
				}
			}
		}
		else
		{
			while (!bitStreamReader.EndOfStream)
			{
				num2 = bitStreamReader.ReadUInt32(bitCount);
				num2 = (((num2 & num) != 0) ? (num | num2) : num2);
				outputBuffer[outputBufferIndex++] = (int)num2;
				if (outputBufferIndex == outputBuffer.Length)
				{
					break;
				}
			}
		}
		return (uint)(outputBuffer.Length * bitCount + 7 >> 3);
	}

	internal byte[] Uncompress(int bitCount, BitStreamReader reader, GorillaEncodingType encodingType, int unitsToDecode)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (bitCount < 0)
		{
			throw new ArgumentOutOfRangeException("bitCount");
		}
		if (unitsToDecode < 0)
		{
			throw new ArgumentOutOfRangeException("unitsToDecode");
		}
		int num = 0;
		uint num2 = 0u;
		switch (encodingType)
		{
		case GorillaEncodingType.Int:
			if (bitCount == 0)
			{
				bitCount = 32;
			}
			num = 32;
			num2 = (uint)(-1 << bitCount - 1);
			break;
		case GorillaEncodingType.Short:
			if (bitCount == 0)
			{
				bitCount = 16;
			}
			num = 16;
			num2 = 0u;
			break;
		case GorillaEncodingType.Byte:
			if (bitCount == 0)
			{
				bitCount = 8;
			}
			num = 8;
			num2 = 0u;
			break;
		default:
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("bogus GorillaEncodingType passed to Uncompress"));
		}
		List<byte> list = new List<byte>(num / 8 * unitsToDecode);
		BitStreamWriter bitStreamWriter = new BitStreamWriter(list);
		uint num3 = 0u;
		while (!reader.EndOfStream && unitsToDecode > 0)
		{
			num3 = reader.ReadUInt32(bitCount);
			num3 = (((num3 & num2) != 0) ? (num2 | num3) : num3);
			bitStreamWriter.WriteReverse(num3, num);
			unitsToDecode--;
		}
		return list.ToArray();
	}

	private static void UpdateMinMax(int n, ref int max, ref int min)
	{
		if (n > max)
		{
			max = n;
		}
		else if (n < min)
		{
			min = n;
		}
	}
}
