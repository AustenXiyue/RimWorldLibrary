using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using MS.Internal.Interop;
using MS.Win32;

namespace MS.Internal.IO.Packaging;

internal class UnsafeIndexingFilterStream : Stream
{
	private IStream _oleStream;

	private bool _disposed;

	public override bool CanRead => !_disposed;

	public override bool CanSeek => !_disposed;

	public override bool CanWrite => false;

	public override long Position
	{
		get
		{
			ThrowIfStreamDisposed();
			return Seek(0L, SeekOrigin.Current);
		}
		set
		{
			ThrowIfStreamDisposed();
			if (value < 0)
			{
				throw new ArgumentException(SR.CannotSetNegativePosition);
			}
			Seek(value, SeekOrigin.Begin);
		}
	}

	public override long Length
	{
		get
		{
			ThrowIfStreamDisposed();
			_oleStream.Stat(out var statStructure, 1);
			return statStructure.cbSize;
		}
	}

	internal UnsafeIndexingFilterStream(IStream oleStream)
	{
		if (oleStream == null)
		{
			throw new ArgumentNullException("oleStream");
		}
		_oleStream = oleStream;
		_disposed = false;
	}

	public unsafe override int Read(byte[] buffer, int offset, int count)
	{
		ThrowIfStreamDisposed();
		PackagingUtilities.VerifyStreamReadArgs(this, buffer, offset, count);
		if (count == 0)
		{
			return 0;
		}
		int result = default(int);
		nint refToNumBytesRead = new IntPtr(&result);
		long position = Position;
		try
		{
			fixed (byte* value = &buffer[offset])
			{
				_oleStream.Read(new IntPtr(value), count, refToNumBytesRead);
				return result;
			}
		}
		catch (COMException innerException)
		{
			Position = position;
			throw new IOException("Read", innerException);
		}
		catch (IOException innerException2)
		{
			Position = position;
			throw new IOException("Read", innerException2);
		}
	}

	public unsafe override long Seek(long offset, SeekOrigin origin)
	{
		ThrowIfStreamDisposed();
		long result = 0L;
		nint refToNewOffsetNullAllowed = new IntPtr(&result);
		_oleStream.Seek(offset, (int)origin, refToNewOffsetNullAllowed);
		return result;
	}

	public override void SetLength(long newLength)
	{
		ThrowIfStreamDisposed();
		throw new NotSupportedException(SR.StreamDoesNotSupportWrite);
	}

	public override void Write(byte[] buf, int offset, int count)
	{
		ThrowIfStreamDisposed();
		throw new NotSupportedException(SR.StreamDoesNotSupportWrite);
	}

	public override void Flush()
	{
		ThrowIfStreamDisposed();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && _oleStream != null)
			{
				MS.Win32.UnsafeNativeMethods.SafeReleaseComObject(_oleStream);
			}
		}
		finally
		{
			_oleStream = null;
			_disposed = true;
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
