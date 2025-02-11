using System;
using System.IO;
using MS.Internal.PresentationCore;

namespace MS.Internal.Ink;

internal class BitStreamReader
{
	private byte[] _byteArray;

	private uint _bufferLengthInBits;

	private int _byteArrayIndex;

	private byte _partialByte;

	private int _cbitsInPartialByte;

	internal bool EndOfStream => _bufferLengthInBits == 0;

	internal int CurrentIndex => _byteArrayIndex - 1;

	internal BitStreamReader(byte[] buffer)
	{
		_byteArray = buffer;
		_bufferLengthInBits = (uint)(buffer.Length * 8);
	}

	internal BitStreamReader(byte[] buffer, int startIndex)
	{
		if (startIndex < 0 || startIndex >= buffer.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		_byteArray = buffer;
		_byteArrayIndex = startIndex;
		_bufferLengthInBits = (uint)((buffer.Length - startIndex) * 8);
	}

	internal BitStreamReader(byte[] buffer, uint bufferLengthInBits)
		: this(buffer)
	{
		if (bufferLengthInBits > buffer.Length * 8)
		{
			throw new ArgumentOutOfRangeException("bufferLengthInBits", SR.InvalidBufferLength);
		}
		_bufferLengthInBits = bufferLengthInBits;
	}

	internal long ReadUInt64(int countOfBits)
	{
		if (countOfBits > 64 || countOfBits <= 0)
		{
			throw new ArgumentOutOfRangeException("countOfBits", countOfBits, SR.CountOfBitsOutOfRange);
		}
		long num = 0L;
		while (countOfBits > 0)
		{
			int num2 = 8;
			if (countOfBits < 8)
			{
				num2 = countOfBits;
			}
			num <<= num2;
			byte b = ReadByte(num2);
			num |= b;
			countOfBits -= num2;
		}
		return num;
	}

	internal ushort ReadUInt16(int countOfBits)
	{
		if (countOfBits > 16 || countOfBits <= 0)
		{
			throw new ArgumentOutOfRangeException("countOfBits", countOfBits, SR.CountOfBitsOutOfRange);
		}
		ushort num = 0;
		while (countOfBits > 0)
		{
			int num2 = 8;
			if (countOfBits < 8)
			{
				num2 = countOfBits;
			}
			num = (ushort)(num << num2);
			byte b = ReadByte(num2);
			num |= b;
			countOfBits -= num2;
		}
		return num;
	}

	internal uint ReadUInt16Reverse(int countOfBits)
	{
		if (countOfBits > 16 || countOfBits <= 0)
		{
			throw new ArgumentOutOfRangeException("countOfBits", countOfBits, SR.CountOfBitsOutOfRange);
		}
		ushort num = 0;
		int num2 = 0;
		while (countOfBits > 0)
		{
			int num3 = 8;
			if (countOfBits < 8)
			{
				num3 = countOfBits;
			}
			ushort num4 = ReadByte(num3);
			num4 = (ushort)(num4 << num2 * 8);
			num |= num4;
			num2++;
			countOfBits -= num3;
		}
		return num;
	}

	internal uint ReadUInt32(int countOfBits)
	{
		if (countOfBits > 32 || countOfBits <= 0)
		{
			throw new ArgumentOutOfRangeException("countOfBits", countOfBits, SR.CountOfBitsOutOfRange);
		}
		uint num = 0u;
		while (countOfBits > 0)
		{
			int num2 = 8;
			if (countOfBits < 8)
			{
				num2 = countOfBits;
			}
			num <<= num2;
			byte b = ReadByte(num2);
			num |= b;
			countOfBits -= num2;
		}
		return num;
	}

	internal uint ReadUInt32Reverse(int countOfBits)
	{
		if (countOfBits > 32 || countOfBits <= 0)
		{
			throw new ArgumentOutOfRangeException("countOfBits", countOfBits, SR.CountOfBitsOutOfRange);
		}
		uint num = 0u;
		int num2 = 0;
		while (countOfBits > 0)
		{
			int num3 = 8;
			if (countOfBits < 8)
			{
				num3 = countOfBits;
			}
			uint num4 = ReadByte(num3);
			num4 <<= num2 * 8;
			num |= num4;
			num2++;
			countOfBits -= num3;
		}
		return num;
	}

	internal bool ReadBit()
	{
		return (ReadByte(1) & 1) == 1;
	}

	internal byte ReadByte(int countOfBits)
	{
		if (EndOfStream)
		{
			throw new EndOfStreamException(SR.EndOfStreamReached);
		}
		if (countOfBits > 8 || countOfBits <= 0)
		{
			throw new ArgumentOutOfRangeException("countOfBits", countOfBits, SR.CountOfBitsOutOfRange);
		}
		if (countOfBits > _bufferLengthInBits)
		{
			throw new ArgumentOutOfRangeException("countOfBits", countOfBits, SR.CountOfBitsGreatThanRemainingBits);
		}
		_bufferLengthInBits -= (uint)countOfBits;
		byte b = 0;
		if (_cbitsInPartialByte >= countOfBits)
		{
			int num = 8 - countOfBits;
			b = (byte)(_partialByte >> num);
			_partialByte = (byte)(_partialByte << countOfBits);
			_cbitsInPartialByte -= countOfBits;
		}
		else
		{
			byte b2 = _byteArray[_byteArrayIndex];
			_byteArrayIndex++;
			int num2 = 8 - countOfBits;
			b = (byte)(_partialByte >> num2);
			int num3 = Math.Abs(countOfBits - _cbitsInPartialByte - 8);
			b |= (byte)(b2 >> num3);
			_partialByte = (byte)(b2 << countOfBits - _cbitsInPartialByte);
			_cbitsInPartialByte = 8 - (countOfBits - _cbitsInPartialByte);
		}
		return b;
	}
}
