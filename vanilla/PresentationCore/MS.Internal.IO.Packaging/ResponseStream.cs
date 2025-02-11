using System;
using System.IO;
using System.IO.Packaging;
using MS.Utility;

namespace MS.Internal.IO.Packaging;

internal class ResponseStream : Stream
{
	private bool _closed;

	private Stream _innerStream;

	private Package _container;

	private Stream _owningStream;

	private PackWebResponse _response;

	public override bool CanRead
	{
		get
		{
			if (!_closed)
			{
				return _innerStream.CanRead;
			}
			return false;
		}
	}

	public override bool CanSeek
	{
		get
		{
			if (!_closed)
			{
				return _innerStream.CanSeek;
			}
			return false;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (!_closed)
			{
				return _innerStream.CanWrite;
			}
			return false;
		}
	}

	public override long Position
	{
		get
		{
			CheckDisposed();
			return _innerStream.Position;
		}
		set
		{
			CheckDisposed();
			_innerStream.Position = value;
		}
	}

	public override long Length
	{
		get
		{
			CheckDisposed();
			return _innerStream.Length;
		}
	}

	internal ResponseStream(Stream s, PackWebResponse response, Stream owningStream, Package container)
	{
		Init(s, response, owningStream, container);
	}

	internal ResponseStream(Stream s, PackWebResponse response)
	{
		Init(s, response, null, null);
	}

	private void Init(Stream s, PackWebResponse response, Stream owningStream, Package container)
	{
		_innerStream = s;
		_response = response;
		_owningStream = owningStream;
		_container = container;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Level.Verbose, EventTrace.Event.WClientDRXReadStreamBegin, count);
		CheckDisposed();
		int num = _innerStream.Read(buffer, offset, count);
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Level.Verbose, EventTrace.Event.WClientDRXReadStreamEnd, num);
		return num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		CheckDisposed();
		return _innerStream.Seek(offset, origin);
	}

	public override void SetLength(long newLength)
	{
		CheckDisposed();
		_innerStream.SetLength(newLength);
	}

	public override void Write(byte[] buf, int offset, int count)
	{
		CheckDisposed();
		_innerStream.Write(buf, offset, count);
	}

	public override void Flush()
	{
		CheckDisposed();
		_innerStream.Flush();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !_closed)
			{
				_container = null;
				_innerStream.Close();
				if (_owningStream != null)
				{
					_owningStream.Close();
				}
			}
		}
		finally
		{
			_innerStream = null;
			_owningStream = null;
			_response = null;
			_closed = true;
			base.Dispose(disposing);
		}
	}

	private void CheckDisposed()
	{
		if (_closed)
		{
			throw new ObjectDisposedException("ResponseStream");
		}
	}
}
