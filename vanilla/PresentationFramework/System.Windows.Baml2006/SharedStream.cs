using System.ComponentModel;
using System.IO;

namespace System.Windows.Baml2006;

internal class SharedStream : Stream
{
	private class RefCount
	{
		public int Value;
	}

	private Stream _baseStream;

	private long _offset;

	private long _length;

	private long _position;

	private RefCount _refCount;

	public virtual int SharedCount => _refCount.Value;

	public override bool CanRead
	{
		get
		{
			CheckDisposed();
			return _baseStream.CanRead;
		}
	}

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Length => _length;

	public override long Position
	{
		get
		{
			return _position;
		}
		set
		{
			if (value < 0 || value >= _length)
			{
				throw new ArgumentOutOfRangeException("value", value, string.Empty);
			}
			_position = value;
		}
	}

	public virtual bool IsDisposed => _baseStream == null;

	public Stream BaseStream => _baseStream;

	public SharedStream(Stream baseStream)
	{
		if (baseStream == null)
		{
			throw new ArgumentNullException("baseStream");
		}
		Initialize(baseStream, 0L, baseStream.Length);
	}

	public SharedStream(Stream baseStream, long offset, long length)
	{
		if (baseStream == null)
		{
			throw new ArgumentNullException("baseStream");
		}
		Initialize(baseStream, offset, length);
	}

	private void Initialize(Stream baseStream, long offset, long length)
	{
		if (!baseStream.CanSeek)
		{
			throw new ArgumentException("can’t seek on baseStream");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (baseStream is SharedStream sharedStream)
		{
			_baseStream = sharedStream.BaseStream;
			_offset = offset + sharedStream._offset;
			_length = length;
			_refCount = sharedStream._refCount;
			_refCount.Value++;
		}
		else
		{
			_baseStream = baseStream;
			_offset = offset;
			_length = length;
			_refCount = new RefCount();
			_refCount.Value++;
		}
	}

	public override void Flush()
	{
		CheckDisposed();
		_baseStream.Flush();
	}

	public override int ReadByte()
	{
		CheckDisposed();
		Math.Min(_position + 1, _length);
		int result;
		if (Sync())
		{
			result = _baseStream.ReadByte();
			_position = _baseStream.Position - _offset;
		}
		else
		{
			result = -1;
		}
		return result;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset >= buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (offset + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		CheckDisposed();
		long num = Math.Min(_position + count, _length);
		int result = 0;
		if (Sync())
		{
			result = _baseStream.Read(buffer, offset, (int)(num - _position));
			_position = _baseStream.Position - _offset;
		}
		return result;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		long num = origin switch
		{
			SeekOrigin.Begin => offset, 
			SeekOrigin.Current => _position + offset, 
			SeekOrigin.End => _length + offset, 
			_ => throw new InvalidEnumArgumentException("origin", (int)origin, typeof(SeekOrigin)), 
		};
		if (num < 0 || num >= _length)
		{
			throw new ArgumentOutOfRangeException("offset", offset, string.Empty);
		}
		CheckDisposed();
		_position = num;
		return _position;
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && _baseStream != null)
		{
			_refCount.Value--;
			if (_refCount.Value < 1)
			{
				_baseStream.Close();
			}
			_refCount = null;
			_baseStream = null;
		}
		base.Dispose(disposing);
	}

	private void CheckDisposed()
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException("BaseStream");
		}
	}

	private bool Sync()
	{
		if (_position >= 0 && _position < _length)
		{
			if (_position + _offset != _baseStream.Position)
			{
				_baseStream.Seek(_offset + _position, SeekOrigin.Begin);
			}
			return true;
		}
		return false;
	}
}
