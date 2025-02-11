using System;
using System.Collections.Generic;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class HuffCodec
{
	private class HuffBits
	{
		private byte[] _bits = new byte[10];

		private uint _size;

		private uint _matchIndex;

		private uint _prefixCount;

		private const byte MaxBAASize = 10;

		private const byte DefaultBAACount = 8;

		private static readonly byte[][] DefaultBAAData = new byte[8][]
		{
			new byte[10] { 0, 1, 2, 4, 6, 8, 12, 16, 24, 32 },
			new byte[10] { 0, 1, 1, 2, 4, 8, 12, 16, 24, 32 },
			new byte[10] { 0, 1, 1, 1, 2, 4, 8, 14, 22, 32 },
			new byte[10] { 0, 2, 2, 3, 5, 8, 12, 16, 24, 32 },
			new byte[10] { 0, 3, 4, 5, 8, 12, 16, 24, 32, 0 },
			new byte[10] { 0, 4, 6, 8, 12, 16, 24, 32, 0, 0 },
			new byte[10] { 0, 6, 8, 12, 16, 24, 32, 0, 0, 0 },
			new byte[10] { 0, 7, 8, 12, 16, 24, 32, 0, 0, 0 }
		};

		private static readonly byte[] DefaultBAASize = new byte[8] { 10, 10, 10, 10, 9, 8, 7, 7 };

		internal HuffBits()
		{
			_size = 2u;
			_bits[0] = 0;
			_bits[1] = 32;
			_matchIndex = 0u;
			_prefixCount = 1u;
		}

		internal bool InitBits(uint defaultIndex)
		{
			if (defaultIndex < 8 && DefaultBAASize[defaultIndex] <= 10)
			{
				_size = DefaultBAASize[defaultIndex];
				_matchIndex = defaultIndex;
				_prefixCount = _size;
				_bits = DefaultBAAData[defaultIndex];
				return true;
			}
			return false;
		}

		internal uint GetSize()
		{
			return _size;
		}

		internal byte GetBitsAtIndex(uint index)
		{
			return _bits[index];
		}
	}

	private HuffBits _huffBits;

	private uint[] _mins = new uint[10];

	private const byte MaxBAASize = 10;

	internal HuffCodec(uint defaultIndex)
	{
		HuffBits huffBits = new HuffBits();
		huffBits.InitBits(defaultIndex);
		InitHuffTable(huffBits);
	}

	private void InitHuffTable(HuffBits huffBits)
	{
		_huffBits = huffBits;
		uint size = _huffBits.GetSize();
		int num = 1;
		_mins[0] = 0u;
		for (uint num2 = 1u; num2 < size; num2++)
		{
			_mins[num2] = (uint)num;
			num += 1 << _huffBits.GetBitsAtIndex(num2) - 1;
		}
	}

	internal void Compress(DataXform dataXf, int[] input, List<byte> compressedData)
	{
		BitStreamWriter writer = new BitStreamWriter(compressedData);
		if (dataXf != null)
		{
			dataXf.ResetState();
			int xfData = 0;
			int extra = 0;
			for (uint num = 0u; num < input.Length; num++)
			{
				dataXf.Transform(input[num], ref xfData, ref extra);
				Encode(xfData, extra, writer);
			}
		}
		else
		{
			for (uint num2 = 0u; num2 < input.Length; num2++)
			{
				Encode(input[num2], 0, writer);
			}
		}
	}

	internal uint Uncompress(DataXform dtxf, byte[] input, int startIndex, int[] outputBuffer)
	{
		BitStreamReader bitStreamReader = new BitStreamReader(input, startIndex);
		int extra = 0;
		int data = 0;
		int num = 0;
		if (dtxf != null)
		{
			dtxf.ResetState();
			while (!bitStreamReader.EndOfStream)
			{
				Decode(ref data, ref extra, bitStreamReader);
				int num2 = dtxf.InverseTransform(data, extra);
				outputBuffer[num++] = num2;
				if (num == outputBuffer.Length)
				{
					break;
				}
			}
		}
		else
		{
			while (!bitStreamReader.EndOfStream)
			{
				Decode(ref data, ref extra, bitStreamReader);
				outputBuffer[num++] = data;
				if (num == outputBuffer.Length)
				{
					break;
				}
			}
		}
		return (uint)(bitStreamReader.CurrentIndex + 1 - startIndex);
	}

	internal byte Encode(int data, int extra, BitStreamWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (data == 0)
		{
			writer.Write(0, 1);
			return 1;
		}
		uint size = _huffBits.GetSize();
		if (extra != 0)
		{
			byte b = (byte)(size + 1);
			int bits = (1 << (int)b) - 2;
			writer.Write((uint)bits, b);
			byte b2 = Encode(extra, 0, writer);
			byte b3 = Encode(data, 0, writer);
			return (byte)(b + b2 + b3);
		}
		uint num = (uint)MathHelper.AbsNoThrow(data);
		byte b4 = 1;
		while (b4 < size && num >= _mins[b4])
		{
			b4++;
		}
		uint bitsAtIndex = _huffBits.GetBitsAtIndex((uint)(b4 - 1));
		int bits2 = (1 << (int)b4) - 2;
		writer.Write((uint)bits2, b4);
		int num2 = (int)(bitsAtIndex - 1);
		num = (((num - _mins[b4 - 1]) & (uint)((1 << num2) - 1)) << 1) | ((data < 0) ? 1u : 0u);
		writer.Write(num, (int)bitsAtIndex);
		return (byte)(b4 + bitsAtIndex);
	}

	internal void Decode(ref int data, ref int extra, BitStreamReader reader)
	{
		byte b = 0;
		while (reader.ReadBit())
		{
			b++;
		}
		extra = 0;
		if (b == 0)
		{
			data = 0;
			return;
		}
		if (b < _huffBits.GetSize())
		{
			uint bitsAtIndex = _huffBits.GetBitsAtIndex(b);
			long num = reader.ReadUInt64((byte)bitsAtIndex);
			bool flag = (num & 1) != 0;
			num = (num >> 1) + _mins[b];
			data = (int)(flag ? (-(int)num) : num);
			return;
		}
		if (b == _huffBits.GetSize())
		{
			int data2 = 0;
			int extra2 = 0;
			Decode(ref data2, ref extra2, reader);
			extra = data2;
			int data3 = 0;
			Decode(ref data3, ref extra2, reader);
			data = data3;
			return;
		}
		throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("invalid huffman encoded data"));
	}
}
