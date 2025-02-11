using System.Collections;
using System.IO;
using System.Threading;

namespace System.Windows.Markup;

internal class ReadWriteStreamManager
{
	private long _readPosition;

	private long _readLength;

	private long _writePosition;

	private long _writeLength;

	private ReaderWriterLock _bufferLock;

	private WriterStream _writerStream;

	private ReaderStream _readerStream;

	private long _readerFirstBufferPosition;

	private long _writerFirstBufferPosition;

	private ArrayList _readerBufferArrayList;

	private ArrayList _writerBufferArrayList;

	private const int _bufferSize = 4096;

	internal WriterStream WriterStream => _writerStream;

	internal ReaderStream ReaderStream => _readerStream;

	internal long ReadPosition
	{
		get
		{
			return _readPosition;
		}
		set
		{
			_readPosition = value;
		}
	}

	internal long ReadLength
	{
		get
		{
			return _readLength;
		}
		set
		{
			_readLength = value;
		}
	}

	internal long WritePosition
	{
		get
		{
			return _writePosition;
		}
		set
		{
			_writePosition = value;
		}
	}

	internal long WriteLength
	{
		get
		{
			return _writeLength;
		}
		set
		{
			_writeLength = value;
		}
	}

	private int BufferSize => 4096;

	private long ReaderFirstBufferPosition
	{
		get
		{
			return _readerFirstBufferPosition;
		}
		set
		{
			_readerFirstBufferPosition = value;
		}
	}

	private long WriterFirstBufferPosition
	{
		get
		{
			return _writerFirstBufferPosition;
		}
		set
		{
			_writerFirstBufferPosition = value;
		}
	}

	private ArrayList ReaderBufferArrayList
	{
		get
		{
			return _readerBufferArrayList;
		}
		set
		{
			_readerBufferArrayList = value;
		}
	}

	private ArrayList WriterBufferArrayList
	{
		get
		{
			return _writerBufferArrayList;
		}
		set
		{
			_writerBufferArrayList = value;
		}
	}

	internal ReadWriteStreamManager()
	{
		ReaderFirstBufferPosition = 0L;
		WriterFirstBufferPosition = 0L;
		ReaderBufferArrayList = new ArrayList();
		WriterBufferArrayList = new ArrayList();
		_writerStream = new WriterStream(this);
		_readerStream = new ReaderStream(this);
		_bufferLock = new ReaderWriterLock();
	}

	internal void Write(byte[] buffer, int offset, int count)
	{
		if (count != 0)
		{
			int bufferOffset;
			int bufferIndex;
			byte[] bufferFromFilePosition = GetBufferFromFilePosition(WritePosition, reader: false, out bufferOffset, out bufferIndex);
			int num = BufferSize - bufferOffset;
			int num2 = 0;
			int num3 = 0;
			if (count > num)
			{
				num3 = num;
				num2 = count - num;
			}
			else
			{
				num2 = 0;
				num3 = count;
			}
			for (int i = 0; i < num3; i++)
			{
				bufferFromFilePosition[bufferOffset++] = buffer[offset++];
			}
			WritePosition += num3;
			if (WritePosition > WriteLength)
			{
				WriteLength = WritePosition;
			}
			if (num2 > 0)
			{
				Write(buffer, offset, num2);
			}
		}
	}

	internal long WriterSeek(long offset, SeekOrigin loc)
	{
		switch (loc)
		{
		case SeekOrigin.Begin:
			WritePosition = (int)offset;
			break;
		case SeekOrigin.Current:
			WritePosition = (int)(WritePosition + offset);
			break;
		case SeekOrigin.End:
			throw new NotSupportedException(SR.ParserWriterNoSeekEnd);
		default:
			throw new ArgumentException(SR.ParserWriterUnknownOrigin);
		}
		if (WritePosition > WriteLength || WritePosition < ReadLength)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		return WritePosition;
	}

	internal void UpdateReaderLength(long position)
	{
		if (ReadLength > position)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		ReadLength = position;
		if (ReadLength > WriteLength)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		CheckIfCanRemoveFromArrayList(position, WriterBufferArrayList, ref _writerFirstBufferPosition);
	}

	internal void WriterClose()
	{
	}

	internal int Read(byte[] buffer, int offset, int count)
	{
		if (count + ReadPosition > ReadLength)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		int bufferOffset;
		int bufferIndex;
		byte[] bufferFromFilePosition = GetBufferFromFilePosition(ReadPosition, reader: true, out bufferOffset, out bufferIndex);
		int num = BufferSize - bufferOffset;
		int num2 = 0;
		int num3 = 0;
		if (count > num)
		{
			num3 = num;
			num2 = count - num;
		}
		else
		{
			num2 = 0;
			num3 = count;
		}
		for (int i = 0; i < num3; i++)
		{
			buffer[offset++] = bufferFromFilePosition[bufferOffset++];
		}
		ReadPosition += num3;
		if (num2 > 0)
		{
			Read(buffer, offset, num2);
		}
		return count;
	}

	internal int ReadByte()
	{
		byte[] array = new byte[1];
		Read(array, 0, 1);
		return array[0];
	}

	internal long ReaderSeek(long offset, SeekOrigin loc)
	{
		switch (loc)
		{
		case SeekOrigin.Begin:
			ReadPosition = (int)offset;
			break;
		case SeekOrigin.Current:
			ReadPosition = (int)(ReadPosition + offset);
			break;
		case SeekOrigin.End:
			throw new NotSupportedException(SR.ParserWriterNoSeekEnd);
		default:
			throw new ArgumentException(SR.ParserWriterUnknownOrigin);
		}
		if (ReadPosition < ReaderFirstBufferPosition || ReadPosition >= ReadLength)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		return ReadPosition;
	}

	internal void ReaderDoneWithFileUpToPosition(long position)
	{
		CheckIfCanRemoveFromArrayList(position, ReaderBufferArrayList, ref _readerFirstBufferPosition);
	}

	private byte[] GetBufferFromFilePosition(long position, bool reader, out int bufferOffset, out int bufferIndex)
	{
		byte[] array = null;
		_bufferLock.AcquireWriterLock(-1);
		ArrayList arrayList;
		long num;
		if (reader)
		{
			arrayList = ReaderBufferArrayList;
			num = ReaderFirstBufferPosition;
		}
		else
		{
			arrayList = WriterBufferArrayList;
			num = WriterFirstBufferPosition;
		}
		bufferIndex = (int)((position - num) / BufferSize);
		bufferOffset = (int)(position - num - bufferIndex * BufferSize);
		if (arrayList.Count <= bufferIndex)
		{
			array = new byte[BufferSize];
			ReaderBufferArrayList.Add(array);
			WriterBufferArrayList.Add(array);
		}
		else
		{
			array = arrayList[bufferIndex] as byte[];
		}
		_bufferLock.ReleaseWriterLock();
		return array;
	}

	private void CheckIfCanRemoveFromArrayList(long position, ArrayList arrayList, ref long firstBufferPosition)
	{
		int num = (int)((position - firstBufferPosition) / BufferSize);
		if (num > 0)
		{
			int num2 = num;
			_bufferLock.AcquireWriterLock(-1);
			firstBufferPosition += num2 * BufferSize;
			arrayList.RemoveRange(0, num);
			_bufferLock.ReleaseWriterLock();
		}
	}
}
