using System;
using System.Collections.Generic;
using MS.Internal.PresentationCore;

namespace MS.Internal.Ink;

internal class BitStreamWriter
{
	private List<byte> _targetBuffer;

	private int _remaining;

	internal BitStreamWriter(List<byte> bufferToWriteTo)
	{
		if (bufferToWriteTo == null)
		{
			throw new ArgumentNullException("bufferToWriteTo");
		}
		_targetBuffer = bufferToWriteTo;
	}

	internal void Write(uint bits, int countOfBits)
	{
		if (countOfBits <= 0 || countOfBits > 32)
		{
			throw new ArgumentOutOfRangeException("countOfBits", countOfBits, SR.CountOfBitsOutOfRange);
		}
		int num = countOfBits / 8;
		int num2 = countOfBits % 8;
		while (num >= 0)
		{
			byte bits2 = (byte)(bits >> num * 8);
			if (num2 > 0)
			{
				Write(bits2, num2);
			}
			if (num > 0)
			{
				num2 = 8;
			}
			num--;
		}
	}

	internal void WriteReverse(uint bits, int countOfBits)
	{
		if (countOfBits <= 0 || countOfBits > 32)
		{
			throw new ArgumentOutOfRangeException("countOfBits", countOfBits, SR.CountOfBitsOutOfRange);
		}
		int num = countOfBits / 8;
		if (countOfBits % 8 > 0)
		{
			num++;
		}
		for (int i = 0; i < num; i++)
		{
			byte bits2 = (byte)(bits >> i * 8);
			Write(bits2, 8);
		}
	}

	internal void Write(byte bits, int countOfBits)
	{
		if (countOfBits <= 0 || countOfBits > 8)
		{
			throw new ArgumentOutOfRangeException("countOfBits", countOfBits, SR.CountOfBitsOutOfRange);
		}
		if (_remaining > 0)
		{
			byte b = _targetBuffer[_targetBuffer.Count - 1];
			b = ((countOfBits <= _remaining) ? ((byte)(b | (byte)((bits & (255 >> 8 - countOfBits)) << _remaining - countOfBits))) : ((byte)(b | (byte)((bits & (255 >> 8 - countOfBits)) >> countOfBits - _remaining))));
			_targetBuffer[_targetBuffer.Count - 1] = b;
		}
		if (countOfBits > _remaining)
		{
			_remaining = 8 - (countOfBits - _remaining);
			byte b = (byte)(bits << _remaining);
			_targetBuffer.Add(b);
		}
		else
		{
			_remaining -= countOfBits;
		}
	}
}
