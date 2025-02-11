using System;
using System.IO;
using System.Reflection;
using System.Windows.Markup;
using System.Windows.Threading;
using MS.Internal.AppModel;

namespace MS.Internal.Navigation;

internal class BindStream : Stream, IStreamInfo
{
	private long _bytesRead;

	private long _maxBytes;

	private long _lastProgressEventByte;

	private Stream _stream;

	private Uri _uri;

	private IContentContainer _cc;

	private Dispatcher _callbackDispatcher;

	private const long _bytesInterval = 1024L;

	public override bool CanRead => _stream.CanRead;

	public override bool CanSeek => _stream.CanSeek;

	public override bool CanWrite => _stream.CanWrite;

	public override long Length => _stream.Length;

	public override long Position
	{
		get
		{
			return _stream.Position;
		}
		set
		{
			_stream.Position = value;
		}
	}

	public Stream Stream => _stream;

	Assembly IStreamInfo.Assembly
	{
		get
		{
			Assembly result = null;
			if (_stream != null && _stream is IStreamInfo streamInfo)
			{
				result = streamInfo.Assembly;
			}
			return result;
		}
	}

	internal BindStream(Stream stream, long maxBytes, Uri uri, IContentContainer cc, Dispatcher callbackDispatcher)
	{
		_bytesRead = 0L;
		_maxBytes = maxBytes;
		_lastProgressEventByte = 0L;
		_stream = stream;
		_uri = uri;
		_cc = cc;
		_callbackDispatcher = callbackDispatcher;
	}

	private void UpdateNavigationProgress()
	{
		for (long num = _lastProgressEventByte + 1024; num <= _bytesRead; num += 1024)
		{
			UpdateNavProgressHelper(num);
			_lastProgressEventByte = num;
		}
		if (_bytesRead == _maxBytes && _lastProgressEventByte < _maxBytes)
		{
			UpdateNavProgressHelper(_maxBytes);
			_lastProgressEventByte = _maxBytes;
		}
	}

	private void UpdateNavProgressHelper(long numBytes)
	{
		if (_callbackDispatcher != null && !_callbackDispatcher.CheckAccess())
		{
			_callbackDispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
			{
				_cc.OnNavigationProgress(_uri, numBytes, _maxBytes);
				return (object)null;
			}, null);
		}
		else
		{
			_cc.OnNavigationProgress(_uri, numBytes, _maxBytes);
		}
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		return _stream.BeginRead(buffer, offset, count, callback, state);
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		return _stream.BeginWrite(buffer, offset, count, callback, state);
	}

	public override void Close()
	{
		_stream.Close();
		if (_callbackDispatcher != null && !_callbackDispatcher.CheckAccess())
		{
			_callbackDispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate
			{
				_cc.OnStreamClosed(_uri);
				return (object)null;
			}, null);
		}
		else
		{
			_cc.OnStreamClosed(_uri);
		}
	}

	public override int EndRead(IAsyncResult asyncResult)
	{
		return _stream.EndRead(asyncResult);
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		_stream.EndWrite(asyncResult);
	}

	public override bool Equals(object obj)
	{
		return _stream.Equals(obj);
	}

	public override void Flush()
	{
		_stream.Flush();
	}

	public override int GetHashCode()
	{
		return _stream.GetHashCode();
	}

	[Obsolete("InitializeLifetimeService is obsolete.", false)]
	public override object InitializeLifetimeService()
	{
		return _stream.InitializeLifetimeService();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = _stream.Read(buffer, offset, count);
		_bytesRead += num;
		_maxBytes = ((_bytesRead > _maxBytes) ? _bytesRead : _maxBytes);
		if (_lastProgressEventByte + 1024 <= _bytesRead || num == 0)
		{
			UpdateNavigationProgress();
		}
		return num;
	}

	public override int ReadByte()
	{
		int num = _stream.ReadByte();
		if (num != -1)
		{
			_bytesRead++;
			_maxBytes = ((_bytesRead > _maxBytes) ? _bytesRead : _maxBytes);
		}
		if (_lastProgressEventByte + 1024 <= _bytesRead || num == -1)
		{
			UpdateNavigationProgress();
		}
		return num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return _stream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		_stream.SetLength(value);
	}

	public override string ToString()
	{
		return _stream.ToString();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		_stream.Write(buffer, offset, count);
	}

	public override void WriteByte(byte value)
	{
		_stream.WriteByte(value);
	}
}
