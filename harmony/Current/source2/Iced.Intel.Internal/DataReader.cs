using System;

namespace Iced.Intel.Internal;

internal ref struct DataReader
{
	private readonly ReadOnlySpan<byte> data;

	private readonly char[] stringData;

	private int index;

	public int Index
	{
		readonly get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	public readonly bool CanRead => (uint)index < (uint)data.Length;

	public DataReader(ReadOnlySpan<byte> data)
		: this(data, 0)
	{
	}

	public DataReader(ReadOnlySpan<byte> data, int maxStringLength)
	{
		this.data = data;
		stringData = ((maxStringLength == 0) ? Array2.Empty<char>() : new char[maxStringLength]);
		index = 0;
	}

	public byte ReadByte()
	{
		return data[index++];
	}

	public uint ReadCompressedUInt32()
	{
		uint num = 0u;
		for (int i = 0; i < 32; i += 7)
		{
			uint num2 = ReadByte();
			if ((num2 & 0x80) == 0)
			{
				return num | (num2 << i);
			}
			num |= (num2 & 0x7F) << i;
		}
		throw new InvalidOperationException();
	}

	public string ReadAsciiString()
	{
		int num = ReadByte();
		char[] array = stringData;
		for (int i = 0; i < num; i++)
		{
			array[i] = (char)ReadByte();
		}
		return new string(array, 0, num);
	}
}
