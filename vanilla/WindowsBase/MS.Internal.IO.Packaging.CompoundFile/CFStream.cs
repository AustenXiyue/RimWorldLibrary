using System;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class CFStream : Stream
{
	private IStream _safeIStream;

	private FileAccess access;

	private StreamInfo backReference;

	public override bool CanRead
	{
		get
		{
			if (!StreamDisposed)
			{
				if (FileAccess.Read != (access & FileAccess.Read))
				{
					return FileAccess.ReadWrite == (access & FileAccess.ReadWrite);
				}
				return true;
			}
			return false;
		}
	}

	public override bool CanSeek => !StreamDisposed;

	public override bool CanWrite
	{
		get
		{
			if (!StreamDisposed)
			{
				if (FileAccess.Write != (access & FileAccess.Write))
				{
					return FileAccess.ReadWrite == (access & FileAccess.ReadWrite);
				}
				return true;
			}
			return false;
		}
	}

	public override long Length
	{
		get
		{
			CheckDisposedStatus();
			_safeIStream.Stat(out var pstatstg, 1);
			return pstatstg.cbSize;
		}
	}

	public override long Position
	{
		get
		{
			CheckDisposedStatus();
			long plibNewPosition = 0L;
			_safeIStream.Seek(0L, 1, out plibNewPosition);
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
			_safeIStream.Seek(value, 0, out plibNewPosition);
			if (value != plibNewPosition)
			{
				throw new IOException(SR.SeekFailed);
			}
		}
	}

	internal bool StreamDisposed
	{
		get
		{
			if (!backReference.StreamInfoDisposed)
			{
				return _safeIStream == null;
			}
			return true;
		}
	}

	public override void Flush()
	{
		CheckDisposedStatus();
		_safeIStream.Commit(0);
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
		_safeIStream.Seek(offset, num, out plibNewPosition);
		return plibNewPosition;
	}

	public override void SetLength(long newLength)
	{
		CheckDisposedStatus();
		if (!CanWrite)
		{
			throw new NotSupportedException(SR.SetLengthNotSupported);
		}
		if (0 > newLength)
		{
			throw new ArgumentOutOfRangeException("newLength", SR.StreamLengthNegative);
		}
		_safeIStream.SetSize(newLength);
		if (newLength < Position)
		{
			Position = newLength;
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		CheckDisposedStatus();
		PackagingUtilities.VerifyStreamReadArgs(this, buffer, offset, count);
		int pcbRead = 0;
		if (offset == 0)
		{
			_safeIStream.Read(buffer, count, out pcbRead);
		}
		else
		{
			byte[] array = new byte[count];
			_safeIStream.Read(array, count, out pcbRead);
			if (pcbRead > 0)
			{
				Array.Copy(array, 0, buffer, offset, pcbRead);
			}
		}
		return pcbRead;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		CheckDisposedStatus();
		PackagingUtilities.VerifyStreamWriteArgs(this, buffer, offset, count);
		int pcbWritten = 0;
		if (offset == 0)
		{
			_safeIStream.Write(buffer, count, out pcbWritten);
		}
		else
		{
			byte[] array = new byte[count];
			Array.Copy(buffer, offset, array, 0, count);
			_safeIStream.Write(array, count, out pcbWritten);
		}
		if (count != pcbWritten)
		{
			throw new IOException(SR.WriteFailure);
		}
	}

	internal void CheckDisposedStatus()
	{
		if (StreamDisposed)
		{
			throw new ObjectDisposedException(null, SR.StreamObjectDisposed);
		}
	}

	internal CFStream(IStream underlyingStream, FileAccess openAccess, StreamInfo creator)
	{
		_safeIStream = underlyingStream;
		access = openAccess;
		backReference = creator;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && _safeIStream != null)
			{
				_safeIStream = null;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
