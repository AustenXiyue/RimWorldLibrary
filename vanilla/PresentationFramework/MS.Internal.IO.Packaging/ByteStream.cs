using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;

namespace MS.Internal.IO.Packaging;

internal sealed class ByteStream : Stream
{
	[ComImport]
	[Guid("0000000c-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface SecuritySuppressedIStream
	{
		void Read([Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pv, int cb, out int pcbRead);

		void Write([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pv, int cb, out int pcbWritten);

		void Seek(long dlibMove, int dwOrigin, out long plibNewPosition);

		void SetSize(long libNewSize);

		void CopyTo(SecuritySuppressedIStream pstm, long cb, out long pcbRead, out long pcbWritten);

		void Commit(int grfCommitFlags);

		void Revert();

		void LockRegion(long libOffset, long cb, int dwLockType);

		void UnlockRegion(long libOffset, long cb, int dwLockType);

		void Stat(out STATSTG pstatstg, int grfStatFlag);

		void Clone(out SecuritySuppressedIStream ppstm);
	}

	private MS.Internal.SecurityCriticalDataForSet<SecuritySuppressedIStream> _securitySuppressedIStream;

	private FileAccess _access;

	private long _length;

	private bool _isLengthInitialized;

	private bool _disposed;

	public override bool CanRead
	{
		get
		{
			if (!StreamDisposed)
			{
				if (FileAccess.Read != (_access & FileAccess.Read))
				{
					return FileAccess.ReadWrite == (_access & FileAccess.ReadWrite);
				}
				return true;
			}
			return false;
		}
	}

	public override bool CanSeek => !StreamDisposed;

	public override bool CanWrite => false;

	public override long Length
	{
		get
		{
			CheckDisposedStatus();
			if (!_isLengthInitialized)
			{
				_securitySuppressedIStream.Value.Stat(out var pstatstg, 1);
				_isLengthInitialized = true;
				_length = pstatstg.cbSize;
			}
			return _length;
		}
	}

	public override long Position
	{
		get
		{
			CheckDisposedStatus();
			long plibNewPosition = 0L;
			_securitySuppressedIStream.Value.Seek(0L, 1, out plibNewPosition);
			return plibNewPosition;
		}
		set
		{
			CheckDisposedStatus();
			if (!CanSeek)
			{
				throw new NotSupportedException(SR.SetPositionNotSupported);
			}
			long plibNewPosition = 0L;
			_securitySuppressedIStream.Value.Seek(value, 0, out plibNewPosition);
			if (value != plibNewPosition)
			{
				throw new IOException(SR.SeekFailed);
			}
		}
	}

	private bool StreamDisposed => _disposed;

	internal ByteStream(object underlyingStream, FileAccess openAccess)
	{
		SecuritySuppressedIStream value = underlyingStream as SecuritySuppressedIStream;
		_securitySuppressedIStream = new MS.Internal.SecurityCriticalDataForSet<SecuritySuppressedIStream>(value);
		_access = openAccess;
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		CheckDisposedStatus();
		if (!CanSeek)
		{
			throw new NotSupportedException(SR.SeekNotSupported);
		}
		long plibNewPosition = 0L;
		int num = 0;
		switch (origin)
		{
		case SeekOrigin.Begin:
			num = 0;
			if (0 > offset)
			{
				throw new ArgumentOutOfRangeException("offset", SR.SeekNegative);
			}
			break;
		case SeekOrigin.Current:
			num = 1;
			break;
		case SeekOrigin.End:
			num = 2;
			break;
		default:
			throw new InvalidEnumArgumentException("origin", (int)origin, typeof(SeekOrigin));
		}
		_securitySuppressedIStream.Value.Seek(offset, num, out plibNewPosition);
		return plibNewPosition;
	}

	public override void SetLength(long newLength)
	{
		throw new NotSupportedException(SR.SetLengthNotSupported);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		CheckDisposedStatus();
		if (!CanRead)
		{
			throw new NotSupportedException(SR.ReadNotSupported);
		}
		int pcbRead = 0;
		if (count == 0)
		{
			return pcbRead;
		}
		if (0 > count)
		{
			throw new ArgumentOutOfRangeException("count", SR.ReadCountNegative);
		}
		if (0 > offset)
		{
			throw new ArgumentOutOfRangeException("offset", SR.BufferOffsetNegative);
		}
		if (buffer.Length == 0 || buffer.Length - offset < count)
		{
			throw new ArgumentException(SR.BufferTooSmall, "buffer");
		}
		if (offset == 0)
		{
			_securitySuppressedIStream.Value.Read(buffer, count, out pcbRead);
		}
		else if (0 < offset)
		{
			byte[] array = new byte[count];
			_securitySuppressedIStream.Value.Read(array, count, out pcbRead);
			if (pcbRead > 0)
			{
				Array.Copy(array, 0, buffer, offset, pcbRead);
			}
		}
		return pcbRead;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException(SR.WriteNotSupported);
	}

	public override void Close()
	{
		_disposed = true;
	}

	internal void CheckDisposedStatus()
	{
		if (StreamDisposed)
		{
			throw new ObjectDisposedException(null, SR.StreamObjectDisposed);
		}
	}
}
