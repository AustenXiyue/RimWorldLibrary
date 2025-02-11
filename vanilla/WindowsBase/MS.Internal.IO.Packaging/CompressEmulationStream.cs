using System;
using System.IO;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

internal class CompressEmulationStream : Stream
{
	private bool _disposed;

	private bool _dirty;

	protected Stream _baseStream;

	protected Stream _tempStream;

	private IDeflateTransform _transformer;

	public override long Position
	{
		get
		{
			CheckDisposed();
			return _tempStream.Position;
		}
		set
		{
			CheckDisposed();
			if (value < 0)
			{
				throw new ArgumentException(SR.SeekNegative);
			}
			_tempStream.Position = value;
		}
	}

	public override long Length
	{
		get
		{
			CheckDisposed();
			return _tempStream.Length;
		}
	}

	public override bool CanRead
	{
		get
		{
			if (!_disposed)
			{
				return _baseStream.CanRead;
			}
			return false;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (!_disposed)
			{
				return _baseStream.CanSeek;
			}
			return false;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (!_disposed)
			{
				return _baseStream.CanWrite;
			}
			return false;
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		PackagingUtilities.VerifyStreamReadArgs(this, buffer, offset, count);
		return _tempStream.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		CheckDisposed();
		long num = 0L;
		if (checked(origin switch
		{
			SeekOrigin.Begin => offset, 
			SeekOrigin.Current => _tempStream.Position + offset, 
			SeekOrigin.End => _tempStream.Length + offset, 
			_ => throw new ArgumentOutOfRangeException("origin", SR.SeekOriginInvalid), 
		}) < 0)
		{
			throw new ArgumentException(SR.SeekNegative);
		}
		return _tempStream.Seek(offset, origin);
	}

	public override void SetLength(long newLength)
	{
		CheckDisposed();
		_tempStream.SetLength(newLength);
		if (newLength < _tempStream.Position)
		{
			_tempStream.Position = newLength;
		}
		_dirty = true;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		PackagingUtilities.VerifyStreamWriteArgs(this, buffer, offset, count);
		if (count != 0)
		{
			_tempStream.Write(buffer, offset, count);
			_dirty = true;
		}
	}

	public override void Flush()
	{
		CheckDisposed();
		if (_dirty)
		{
			long position = _tempStream.Position;
			_tempStream.Position = 0L;
			_baseStream.Position = 0L;
			_transformer.Compress(_tempStream, _baseStream);
			_tempStream.Position = position;
			_baseStream.Flush();
			_dirty = false;
		}
	}

	internal CompressEmulationStream(Stream baseStream, Stream tempStream, long position, IDeflateTransform transformer)
	{
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		if (baseStream == null)
		{
			throw new ArgumentNullException("baseStream");
		}
		if (!baseStream.CanSeek)
		{
			throw new InvalidOperationException(SR.SeekNotSupported);
		}
		if (!baseStream.CanRead)
		{
			throw new InvalidOperationException(SR.ReadNotSupported);
		}
		if (tempStream == null)
		{
			throw new ArgumentNullException("tempStream");
		}
		if (transformer == null)
		{
			throw new ArgumentNullException("transformer");
		}
		_baseStream = baseStream;
		_tempStream = tempStream;
		_transformer = transformer;
		_baseStream.Position = 0L;
		_tempStream.Position = 0L;
		_transformer.Decompress(baseStream, tempStream);
		_tempStream.Position = position;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !_disposed)
			{
				Flush();
				_tempStream.Close();
				_tempStream = null;
				_baseStream = null;
				_disposed = true;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	protected void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(null, SR.StreamObjectDisposed);
		}
	}
}
