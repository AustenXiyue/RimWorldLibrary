using System.IO;

namespace System.Windows.Markup;

internal class ReaderStream : Stream
{
	private ReadWriteStreamManager _streamManager;

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Length => StreamManager.ReadLength;

	public override long Position
	{
		get
		{
			return StreamManager.ReadPosition;
		}
		set
		{
			StreamManager.ReaderSeek(value, SeekOrigin.Begin);
		}
	}

	private ReadWriteStreamManager StreamManager => _streamManager;

	internal ReaderStream(ReadWriteStreamManager streamManager)
	{
		_streamManager = streamManager;
	}

	public override void Close()
	{
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return StreamManager.Read(buffer, offset, count);
	}

	public override int ReadByte()
	{
		return StreamManager.ReadByte();
	}

	public override long Seek(long offset, SeekOrigin loc)
	{
		return StreamManager.ReaderSeek(offset, loc);
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	internal void ReaderDoneWithFileUpToPosition(long position)
	{
		StreamManager.ReaderDoneWithFileUpToPosition(position);
	}
}
