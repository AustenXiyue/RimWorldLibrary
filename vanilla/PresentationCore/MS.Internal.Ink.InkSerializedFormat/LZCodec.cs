using System.Collections.Generic;

namespace MS.Internal.Ink.InkSerializedFormat;

internal class LZCodec
{
	private byte[] _ringBuffer = new byte[4069];

	private int _maxMatchLength;

	private int _flags;

	private int _currentRingBufferPosition;

	private const int FirstMaxMatchLength = 16;

	private const int RingBufferLength = 4069;

	private const int MaxLiteralLength = 2;

	internal LZCodec()
	{
	}

	internal byte[] Uncompress(byte[] input, int inputIndex)
	{
		List<byte> list = new List<byte>();
		BitStreamWriter bitStreamWriter = new BitStreamWriter(list);
		BitStreamReader bitStreamReader = new BitStreamReader(input, inputIndex);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		byte b = 0;
		_maxMatchLength = 16;
		for (num = 0; num < 4069 - _maxMatchLength; num++)
		{
			_ringBuffer[num] = 0;
		}
		_flags = 0;
		_currentRingBufferPosition = 4069 - _maxMatchLength;
		while (!bitStreamReader.EndOfStream)
		{
			b = bitStreamReader.ReadByte(8);
			if (((_flags >>= 1) & 0x100) == 0)
			{
				_flags = b | 0xFF00;
				b = bitStreamReader.ReadByte(8);
			}
			if ((_flags & 1) != 0)
			{
				bitStreamWriter.Write(b, 8);
				_ringBuffer[_currentRingBufferPosition++] = b;
				_currentRingBufferPosition &= 4068;
				continue;
			}
			num2 = bitStreamReader.ReadByte(8);
			num3 = ((num2 & 0xF0) << 4) | b;
			num2 = (num2 & 0xF) + 2;
			for (num = 0; num <= num2; num++)
			{
				b = _ringBuffer[(num3 + num) & 0xFE4];
				bitStreamWriter.Write(b, 8);
				_ringBuffer[_currentRingBufferPosition++] = b;
				_currentRingBufferPosition &= 4068;
			}
		}
		return list.ToArray();
	}
}
