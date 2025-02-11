using System;
using System.IO;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

internal sealed class IgnoreFlushAndCloseStream : Stream
{
	private Stream _stream;

	private bool _disposed;

	public override bool CanRead
	{
		get
		{
			if (_disposed)
			{
				return false;
			}
			return _stream.CanRead;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (_disposed)
			{
				return false;
			}
			return _stream.CanSeek;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (_disposed)
			{
				return false;
			}
			return _stream.CanWrite;
		}
	}

	public override long Length
	{
		get
		{
			ThrowIfStreamDisposed();
			return _stream.Length;
		}
	}

	public override long Position
	{
		get
		{
			ThrowIfStreamDisposed();
			return _stream.Position;
		}
		set
		{
			ThrowIfStreamDisposed();
			_stream.Position = value;
		}
	}

	internal IgnoreFlushAndCloseStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		_stream = stream;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		ThrowIfStreamDisposed();
		return _stream.Seek(offset, origin);
	}

	public override void SetLength(long newLength)
	{
		ThrowIfStreamDisposed();
		_stream.SetLength(newLength);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		ThrowIfStreamDisposed();
		return _stream.Read(buffer, offset, count);
	}

	public override void Write(byte[] buf, int offset, int count)
	{
		ThrowIfStreamDisposed();
		_stream.Write(buf, offset, count);
	}

	public override void Flush()
	{
		ThrowIfStreamDisposed();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!_disposed)
			{
				_stream = null;
				_disposed = true;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private void ThrowIfStreamDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null, SR.StreamObjectDisposed);
		}
	}
}
