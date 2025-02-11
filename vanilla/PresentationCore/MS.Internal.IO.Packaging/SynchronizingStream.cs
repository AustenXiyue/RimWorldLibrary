using System;
using System.IO;

namespace MS.Internal.IO.Packaging;

internal class SynchronizingStream : Stream
{
	private Stream _baseStream;

	private object _syncRoot;

	public override bool CanRead
	{
		get
		{
			lock (_syncRoot)
			{
				return _baseStream != null && _baseStream.CanRead;
			}
		}
	}

	public override bool CanSeek
	{
		get
		{
			lock (_syncRoot)
			{
				return _baseStream != null && _baseStream.CanSeek;
			}
		}
	}

	public override bool CanWrite
	{
		get
		{
			lock (_syncRoot)
			{
				return _baseStream != null && _baseStream.CanWrite;
			}
		}
	}

	public override long Position
	{
		get
		{
			lock (_syncRoot)
			{
				CheckDisposed();
				return _baseStream.Position;
			}
		}
		set
		{
			lock (_syncRoot)
			{
				CheckDisposed();
				_baseStream.Position = value;
			}
		}
	}

	public override long Length
	{
		get
		{
			lock (_syncRoot)
			{
				CheckDisposed();
				return _baseStream.Length;
			}
		}
	}

	internal SynchronizingStream(Stream stream, object syncRoot)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (syncRoot == null)
		{
			throw new ArgumentNullException("syncRoot");
		}
		_baseStream = stream;
		_syncRoot = syncRoot;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		lock (_syncRoot)
		{
			CheckDisposed();
			return _baseStream.Read(buffer, offset, count);
		}
	}

	public override int ReadByte()
	{
		lock (_syncRoot)
		{
			CheckDisposed();
			return _baseStream.ReadByte();
		}
	}

	public override void WriteByte(byte b)
	{
		lock (_syncRoot)
		{
			CheckDisposed();
			_baseStream.WriteByte(b);
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		lock (_syncRoot)
		{
			CheckDisposed();
			return _baseStream.Seek(offset, origin);
		}
	}

	public override void SetLength(long newLength)
	{
		lock (_syncRoot)
		{
			CheckDisposed();
			_baseStream.SetLength(newLength);
		}
	}

	public override void Write(byte[] buf, int offset, int count)
	{
		lock (_syncRoot)
		{
			CheckDisposed();
			_baseStream.Write(buf, offset, count);
		}
	}

	public override void Flush()
	{
		lock (_syncRoot)
		{
			CheckDisposed();
			_baseStream.Flush();
		}
	}

	protected override void Dispose(bool disposing)
	{
		lock (_syncRoot)
		{
			try
			{
				if (disposing && _baseStream != null)
				{
					_baseStream.Close();
				}
			}
			finally
			{
				base.Dispose(disposing);
				_baseStream = null;
			}
		}
	}

	private void CheckDisposed()
	{
		if (_baseStream == null)
		{
			throw new ObjectDisposedException("Stream");
		}
	}
}
