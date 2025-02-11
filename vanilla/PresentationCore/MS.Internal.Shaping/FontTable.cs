using System.IO;

namespace MS.Internal.Shaping;

internal class FontTable
{
	public const int InvalidOffset = int.MaxValue;

	public const int NullOffset = 0;

	private byte[] m_data;

	private uint m_length;

	public bool IsPresent => m_data != null;

	public FontTable(byte[] data)
	{
		m_data = data;
		if (data != null)
		{
			m_length = (uint)data.Length;
		}
		else
		{
			m_length = 0u;
		}
	}

	public ushort GetUShort(int offset)
	{
		Invariant.Assert(m_data != null);
		if (offset + 1 >= m_length)
		{
			throw new FileFormatException();
		}
		return (ushort)((m_data[offset] << 8) + m_data[offset + 1]);
	}

	public short GetShort(int offset)
	{
		Invariant.Assert(m_data != null);
		if (offset + 1 >= m_length)
		{
			throw new FileFormatException();
		}
		return (short)((m_data[offset] << 8) + m_data[offset + 1]);
	}

	public uint GetUInt(int offset)
	{
		Invariant.Assert(m_data != null);
		if (offset + 3 >= m_length)
		{
			throw new FileFormatException();
		}
		return (uint)((m_data[offset] << 24) + (m_data[offset + 1] << 16) + (m_data[offset + 2] << 8) + m_data[offset + 3]);
	}

	public ushort GetOffset(int offset)
	{
		Invariant.Assert(m_data != null);
		if (offset + 1 >= m_length)
		{
			throw new FileFormatException();
		}
		return (ushort)((m_data[offset] << 8) + m_data[offset + 1]);
	}
}
