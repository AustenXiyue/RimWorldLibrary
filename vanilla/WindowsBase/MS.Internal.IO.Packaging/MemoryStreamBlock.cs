using System;
using System.IO;

namespace MS.Internal.IO.Packaging;

internal class MemoryStreamBlock : IComparable<MemoryStreamBlock>
{
	private MemoryStream _stream;

	private long _offset;

	internal MemoryStream Stream => _stream;

	internal long Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = value;
		}
	}

	internal long EndOffset => checked(_offset + ((_stream == null) ? 0 : _stream.Length));

	internal MemoryStreamBlock(MemoryStream stream, long offset)
	{
		_stream = stream;
		_offset = offset;
	}

	int IComparable<MemoryStreamBlock>.CompareTo(MemoryStreamBlock other)
	{
		if (other == null)
		{
			return 1;
		}
		if (_offset == other.Offset)
		{
			return 0;
		}
		if (_offset > other.Offset)
		{
			if (_offset < other.EndOffset)
			{
				return 0;
			}
			return 1;
		}
		if (other.Offset < EndOffset)
		{
			return 0;
		}
		return -1;
	}
}
