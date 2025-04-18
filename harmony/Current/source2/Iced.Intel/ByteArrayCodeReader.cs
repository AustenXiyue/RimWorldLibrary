using System;

namespace Iced.Intel;

internal sealed class ByteArrayCodeReader : CodeReader
{
	private readonly byte[] data;

	private int currentPosition;

	private readonly int startPosition;

	private readonly int endPosition;

	public int Position
	{
		get
		{
			return currentPosition - startPosition;
		}
		set
		{
			if ((uint)value > (uint)Count)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException_value();
			}
			currentPosition = startPosition + value;
		}
	}

	public int Count => endPosition - startPosition;

	public bool CanReadByte => currentPosition < endPosition;

	public ByteArrayCodeReader(string hexData)
		: this(HexUtils.ToByteArray(hexData))
	{
	}

	public ByteArrayCodeReader(byte[] data)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		this.data = data;
		currentPosition = 0;
		startPosition = 0;
		endPosition = data.Length;
	}

	public ByteArrayCodeReader(byte[] data, int index, int count)
	{
		if (data == null)
		{
			ThrowHelper.ThrowArgumentNullException_data();
		}
		this.data = data;
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_count();
		}
		if ((uint)(index + count) > (uint)data.Length)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_count();
		}
		currentPosition = index;
		startPosition = index;
		endPosition = index + count;
	}

	public ByteArrayCodeReader(ArraySegment<byte> data)
	{
		if (data.Array == null)
		{
			ThrowHelper.ThrowArgumentException();
		}
		this.data = data.Array;
		endPosition = (startPosition = (currentPosition = data.Offset)) + data.Count;
	}

	public override int ReadByte()
	{
		if (currentPosition >= endPosition)
		{
			return -1;
		}
		return data[currentPosition++];
	}
}
