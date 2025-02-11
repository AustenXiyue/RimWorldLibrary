using System.IO;

namespace System.Windows.Markup;

internal class WriterStream : Stream
{
	private ReadWriteStreamManager _streamManager;

	public override bool CanRead => false;

	public override bool CanSeek => true;

	public override bool CanWrite => true;

	public override long Length => StreamManager.WriteLength;

	public override long Position
	{
		get
		{
			return -1L;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	private ReadWriteStreamManager StreamManager => _streamManager;

	internal WriterStream(ReadWriteStreamManager streamManager)
	{
		_streamManager = streamManager;
	}

	public override void Close()
	{
		StreamManager.WriterClose();
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override int ReadByte()
	{
		throw new NotSupportedException();
	}

	public override long Seek(long offset, SeekOrigin loc)
	{
		return StreamManager.WriterSeek(offset, loc);
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		StreamManager.Write(buffer, offset, count);
	}

	internal void UpdateReaderLength(long position)
	{
		StreamManager.UpdateReaderLength(position);
	}
}
