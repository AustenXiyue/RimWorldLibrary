using System;
using System.IO;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class VersionedStream : Stream
{
	private VersionedStreamOwner _versionOwner;

	private Stream _stream;

	public override long Position
	{
		get
		{
			CheckDisposed();
			return _stream.Position;
		}
		set
		{
			Seek(value, SeekOrigin.Begin);
		}
	}

	public override long Length
	{
		get
		{
			CheckDisposed();
			return _stream.Length;
		}
	}

	public override bool CanRead
	{
		get
		{
			if (_stream != null && _stream.CanRead)
			{
				return _versionOwner.IsReadable;
			}
			return false;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (_stream != null && _stream.CanSeek)
			{
				return _versionOwner.IsReadable;
			}
			return false;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (_stream != null && _stream.CanWrite)
			{
				return _versionOwner.IsUpdatable;
			}
			return false;
		}
	}

	protected Stream BaseStream => _stream;

	public override int Read(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		_versionOwner.ReadAttempt(_stream.Length > 0);
		return _stream.Read(buffer, offset, count);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		CheckDisposed();
		_versionOwner.WriteAttempt();
		_stream.Write(buffer, offset, count);
	}

	public override int ReadByte()
	{
		CheckDisposed();
		_versionOwner.ReadAttempt(_stream.Length > 0);
		return _stream.ReadByte();
	}

	public override void WriteByte(byte b)
	{
		CheckDisposed();
		_versionOwner.WriteAttempt();
		_stream.WriteByte(b);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		CheckDisposed();
		return _stream.Seek(offset, origin);
	}

	public override void SetLength(long newLength)
	{
		CheckDisposed();
		if (newLength < 0)
		{
			throw new ArgumentOutOfRangeException("newLength");
		}
		_versionOwner.WriteAttempt();
		_stream.SetLength(newLength);
	}

	public override void Flush()
	{
		CheckDisposed();
		_stream.Flush();
	}

	internal VersionedStream(Stream baseStream, VersionedStreamOwner versionOwner)
	{
		if (baseStream == null)
		{
			throw new ArgumentNullException("baseStream");
		}
		if (versionOwner == null)
		{
			throw new ArgumentNullException("versionOwner");
		}
		_stream = baseStream;
		_versionOwner = versionOwner;
	}

	protected VersionedStream(Stream baseStream)
	{
		if (baseStream == null)
		{
			throw new ArgumentNullException("baseStream");
		}
		_stream = baseStream;
		_versionOwner = (VersionedStreamOwner)this;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && _stream != null)
			{
				_stream.Close();
			}
		}
		finally
		{
			_stream = null;
			base.Dispose(disposing);
		}
	}

	protected void CheckDisposed()
	{
		if (_stream == null)
		{
			throw new ObjectDisposedException(null, SR.StreamObjectDisposed);
		}
	}
}
