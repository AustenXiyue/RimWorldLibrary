using System;
using System.Collections.Generic;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class AlgoModule
{
	private HuffModule _huffModule;

	private MultiByteCodec _multiByteCodec;

	private DeltaDelta _deltaDelta;

	private GorillaCodec _gorillaCodec;

	private LZCodec _lzCodec;

	internal const byte NoCompression = 0;

	internal const byte DefaultCompression = 192;

	internal const byte IndexedHuffman = 128;

	internal const byte LempelZiv = 128;

	internal const byte DefaultBAACount = 8;

	internal const byte MaxBAACount = 10;

	private static readonly double[] DefaultFirstSquareRoot = new double[8] { 1.0, 1.0, 1.0, 4.0, 9.0, 16.0, 36.0, 49.0 };

	private HuffModule HuffModule
	{
		get
		{
			if (_huffModule == null)
			{
				_huffModule = new HuffModule();
			}
			return _huffModule;
		}
	}

	private MultiByteCodec MultiByteCodec
	{
		get
		{
			if (_multiByteCodec == null)
			{
				_multiByteCodec = new MultiByteCodec();
			}
			return _multiByteCodec;
		}
	}

	private DeltaDelta DeltaDelta
	{
		get
		{
			if (_deltaDelta == null)
			{
				_deltaDelta = new DeltaDelta();
			}
			return _deltaDelta;
		}
	}

	private GorillaCodec GorillaCodec
	{
		get
		{
			if (_gorillaCodec == null)
			{
				_gorillaCodec = new GorillaCodec();
			}
			return _gorillaCodec;
		}
	}

	private LZCodec LZCodec
	{
		get
		{
			if (_lzCodec == null)
			{
				_lzCodec = new LZCodec();
			}
			return _lzCodec;
		}
	}

	internal AlgoModule()
	{
	}

	internal byte GetBestDefHuff(int[] input)
	{
		if (input.Length < 3)
		{
			return 0;
		}
		DeltaDelta deltaDelta = new DeltaDelta();
		int xfData = 0;
		int extra = 0;
		deltaDelta.Transform(input[0], ref xfData, ref extra);
		deltaDelta.Transform(input[1], ref xfData, ref extra);
		double num = 0.0;
		uint num2;
		for (num2 = 2u; num2 < input.Length; num2++)
		{
			deltaDelta.Transform(input[num2], ref xfData, ref extra);
			if (extra == 0)
			{
				num += (double)xfData * (double)xfData;
			}
		}
		num *= 0.205625 / ((double)num2 - 1.0);
		int num3 = DefaultFirstSquareRoot.Length - 2;
		while (num3 > 1 && !(num > DefaultFirstSquareRoot[num3]))
		{
			num3--;
		}
		return (byte)(0x80 | (byte)(num3 + 1));
	}

	internal byte[] CompressPacketData(int[] input, byte compression)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		List<byte> list = new List<byte>();
		list.Add(0);
		if (192 == (0xC0 & compression))
		{
			compression = GetBestDefHuff(input);
		}
		if (128 == (0xC0 & compression))
		{
			DataXform dataXf = HuffModule.FindDtXf(compression);
			HuffModule.FindCodec(compression).Compress(dataXf, input, list);
			if (list.Count - 1 >> 2 > input.Length)
			{
				compression = 0;
				list.Clear();
				list.Add(0);
			}
		}
		if ((0xC0 & compression) == 0)
		{
			bool testDelDel = (compression & 0x20) != 0;
			compression = GorillaCodec.FindPacketAlgoByte(input, testDelDel);
			DeltaDelta deltaDelta = null;
			if ((compression & 0x20) != 0)
			{
				deltaDelta = DeltaDelta;
			}
			int startInputIndex = 0;
			if (deltaDelta != null)
			{
				int xfData = 0;
				int extra = 0;
				deltaDelta.ResetState();
				deltaDelta.Transform(input[0], ref xfData, ref extra);
				MultiByteCodec.SignEncode(xfData, list);
				deltaDelta.Transform(input[1], ref xfData, ref extra);
				MultiByteCodec.SignEncode(xfData, list);
				startInputIndex = 2;
			}
			int bitCount = compression & 0x1F;
			GorillaCodec.Compress(bitCount, input, startInputIndex, deltaDelta, list);
		}
		list[0] = compression;
		return list.ToArray();
	}

	internal uint DecompressPacketData(byte[] input, int[] outputBuffer)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (input.Length < 2)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Input buffer passed was shorter than expected"));
		}
		if (outputBuffer == null)
		{
			throw new ArgumentNullException("outputBuffer");
		}
		if (outputBuffer.Length == 0)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("output buffer length was zero"));
		}
		byte b = input[0];
		uint num = 1u;
		int num2 = 1;
		switch (b & 0xC0)
		{
		case 128:
		{
			DataXform dtxf = HuffModule.FindDtXf(b);
			HuffCodec huffCodec = HuffModule.FindCodec(b);
			return num + huffCodec.Uncompress(dtxf, input, num2, outputBuffer);
		}
		case 0:
		{
			int outputBufferIndex = 0;
			DeltaDelta deltaDelta = null;
			if ((b & 0x20) != 0)
			{
				deltaDelta = DeltaDelta;
			}
			int num3 = 0;
			num3 = (((b & 0x1F) != 0) ? (b & 0x1F) : 32);
			if (deltaDelta != null)
			{
				if (input.Length < 3)
				{
					throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Input buffer was too short (must be at least 3 bytes)"));
				}
				int data = 0;
				int extra = 0;
				deltaDelta.ResetState();
				uint num4 = MultiByteCodec.SignDecode(input, num2, ref data);
				num2 += (int)num4;
				num += num4;
				int num5 = deltaDelta.InverseTransform(data, extra);
				outputBuffer[outputBufferIndex++] = num5;
				num4 = MultiByteCodec.SignDecode(input, num2, ref data);
				num2 += (int)num4;
				num += num4;
				num5 = deltaDelta.InverseTransform(data, extra);
				outputBuffer[outputBufferIndex++] = num5;
			}
			return num + GorillaCodec.Uncompress(num3, input, num2, deltaDelta, outputBuffer, outputBufferIndex);
		}
		default:
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid decompression algo byte"));
		}
	}

	internal byte[] CompressPropertyData(byte[] input, byte compression)
	{
		List<byte> list = new List<byte>(input.Length + 1);
		list.Add(0);
		if (192 == (0xC0 & compression))
		{
			compression = GorillaCodec.FindPropAlgoByte(input);
		}
		if (128 == (compression & 0x80))
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("Invalid compression specified or computed by FindPropAlgoByte"));
		}
		int countPerItem = 0;
		int bitCount = 0;
		int padCount = 0;
		GorillaCodec.GetPropertyBitCount(compression, ref countPerItem, ref bitCount, ref padCount);
		GorillaEncodingType encodingType = GorillaEncodingType.Byte;
		int num = input.Length;
		switch (countPerItem)
		{
		case 4:
			encodingType = GorillaEncodingType.Int;
			num >>= 2;
			break;
		case 2:
			encodingType = GorillaEncodingType.Short;
			num >>= 1;
			break;
		}
		BitStreamReader reader = new BitStreamReader(input);
		GorillaCodec.Compress(bitCount, reader, encodingType, num, list);
		list[0] = compression;
		return list.ToArray();
	}

	internal byte[] DecompressPropertyData(byte[] input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (input.Length < 2)
		{
			throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("input.Length must be at least 2"));
		}
		byte b = input[0];
		int num = 1;
		if (128 == (b & 0x80))
		{
			if ((b & -129) != 0)
			{
				throw new ArgumentException(StrokeCollectionSerializer.ISFDebugMessage("bogus isf, we don't decompress property data with lz"));
			}
			return LZCodec.Uncompress(input, num);
		}
		int countPerItem = 0;
		int bitCount = 0;
		int padCount = 0;
		GorillaCodec.GetPropertyBitCount(b, ref countPerItem, ref bitCount, ref padCount);
		GorillaEncodingType encodingType = GorillaEncodingType.Byte;
		switch (countPerItem)
		{
		case 4:
			encodingType = GorillaEncodingType.Int;
			break;
		case 2:
			encodingType = GorillaEncodingType.Short;
			break;
		}
		int unitsToDecode = (input.Length - num << 3) / bitCount - padCount;
		BitStreamReader reader = new BitStreamReader(input, num);
		return GorillaCodec.Uncompress(bitCount, reader, encodingType, unitsToDecode);
	}
}
