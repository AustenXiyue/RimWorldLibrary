using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;

namespace MS.Internal.IO.Packaging;

internal class ManagedIStream : IStream
{
	private Stream _ioStream;

	internal ManagedIStream(Stream ioStream)
	{
		if (ioStream == null)
		{
			throw new ArgumentNullException("ioStream");
		}
		_ioStream = ioStream;
	}

	void IStream.Read(byte[] buffer, int bufferSize, nint bytesReadPtr)
	{
		int val = _ioStream.Read(buffer, 0, bufferSize);
		if (bytesReadPtr != IntPtr.Zero)
		{
			Marshal.WriteInt32(bytesReadPtr, val);
		}
	}

	void IStream.Seek(long offset, int origin, nint newPositionPtr)
	{
		SeekOrigin origin2 = origin switch
		{
			0 => SeekOrigin.Begin, 
			1 => SeekOrigin.Current, 
			2 => SeekOrigin.End, 
			_ => throw new ArgumentOutOfRangeException("origin"), 
		};
		long val = _ioStream.Seek(offset, origin2);
		if (newPositionPtr != IntPtr.Zero)
		{
			Marshal.WriteInt64(newPositionPtr, val);
		}
	}

	void IStream.SetSize(long libNewSize)
	{
		_ioStream.SetLength(libNewSize);
	}

	void IStream.Stat(out STATSTG streamStats, int grfStatFlag)
	{
		streamStats = default(STATSTG);
		streamStats.type = 2;
		streamStats.cbSize = _ioStream.Length;
		streamStats.grfMode = 0;
		if (_ioStream.CanRead && _ioStream.CanWrite)
		{
			streamStats.grfMode |= 2;
			return;
		}
		if (_ioStream.CanRead)
		{
			streamStats.grfMode |= 0;
			return;
		}
		if (_ioStream.CanWrite)
		{
			streamStats.grfMode |= 1;
			return;
		}
		throw new IOException(SR.StreamObjectDisposed);
	}

	void IStream.Write(byte[] buffer, int bufferSize, nint bytesWrittenPtr)
	{
		_ioStream.Write(buffer, 0, bufferSize);
		if (bytesWrittenPtr != IntPtr.Zero)
		{
			Marshal.WriteInt32(bytesWrittenPtr, bufferSize);
		}
	}

	void IStream.Clone(out IStream streamCopy)
	{
		streamCopy = null;
		throw new NotSupportedException();
	}

	void IStream.CopyTo(IStream targetStream, long bufferSize, nint buffer, nint bytesWrittenPtr)
	{
		throw new NotSupportedException();
	}

	void IStream.Commit(int flags)
	{
		throw new NotSupportedException();
	}

	void IStream.LockRegion(long offset, long byteCount, int lockType)
	{
		throw new NotSupportedException();
	}

	void IStream.Revert()
	{
		throw new NotSupportedException();
	}

	void IStream.UnlockRegion(long offset, long byteCount, int lockType)
	{
		throw new NotSupportedException();
	}
}
