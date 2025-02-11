using System.Net;
using System.Threading;
using MS.Internal;
using MS.Internal.IO.Packaging;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.IO.Packaging;

/// <summary>Represents a response of a <see cref="T:System.IO.Packaging.PackWebRequest" />. </summary>
public sealed class PackWebResponse : WebResponse
{
	private class CachedResponse
	{
		private PackWebResponse _parent;

		private Package _cacheEntry;

		private bool _cachedPackageIsThreadSafe;

		internal WebHeaderCollection Headers => new WebHeaderCollection();

		public long ContentLength
		{
			get
			{
				if (!_parent._lengthAvailable)
				{
					GetResponseStream();
				}
				return _parent._fullStreamLength;
			}
		}

		internal CachedResponse(PackWebResponse parent, Package cacheEntry, bool cachedPackageIsThreadSafe)
		{
			_parent = parent;
			_cacheEntry = cacheEntry;
			_cachedPackageIsThreadSafe = cachedPackageIsThreadSafe;
		}

		internal Stream GetResponseStream()
		{
			lock (_cacheEntry)
			{
				if (_parent._responseStream == null && !(_parent._partName == null))
				{
					PackagePart part = _cacheEntry.GetPart(_parent._partName);
					Stream stream = part.GetSeekableStream(FileMode.Open, FileAccess.Read);
					if (!_cachedPackageIsThreadSafe)
					{
						stream = new SynchronizingStream(stream, _cacheEntry);
					}
					_parent._mimeType = new ContentType(part.ContentType);
					_parent._lengthAvailable = stream.CanSeek;
					if (stream.CanSeek)
					{
						_parent._fullStreamLength = stream.Length;
					}
					_parent._responseStream = stream;
				}
			}
			return _parent._responseStream;
		}

		internal void Close()
		{
			try
			{
				_parent._disposed = true;
				if (_parent._responseStream != null)
				{
					_parent._responseStream.Close();
				}
			}
			finally
			{
				_cacheEntry = null;
				_parent._uri = null;
				_parent._mimeType = null;
				_parent._innerUri = null;
				_parent._partName = null;
				_parent._responseStream = null;
				_parent = null;
			}
		}
	}

	private ContentType _mimeType;

	private const int _bufferSize = 4096;

	private Uri _uri;

	private Uri _innerUri;

	private Uri _partName;

	private bool _disposed;

	private WebRequest _webRequest;

	private WebResponse _fullResponse;

	private long _fullStreamLength;

	private Stream _responseStream;

	private bool _responseError;

	private Exception _responseException;

	private Timer _timeoutTimer;

	private ManualResetEvent _responseAvailable;

	private bool _lengthAvailable;

	private CachedResponse _cachedResponse;

	private object _lockObject;

	/// <summary>Gets the inner <see cref="T:System.Net.WebResponse" /> object for the response. </summary>
	/// <returns>The response data as a <see cref="T:System.Net.WebResponse" />.</returns>
	public WebResponse InnerResponse
	{
		get
		{
			CheckDisposed();
			if (FromPackageCache)
			{
				return null;
			}
			WaitForResponse();
			return _fullResponse;
		}
	}

	/// <summary>Gets the collection of Web <see cref="P:System.Net.WebResponse.Headers" /> for this response. </summary>
	/// <returns>The collection of Web response <see cref="P:System.Net.WebResponse.Headers" />.</returns>
	public override WebHeaderCollection Headers
	{
		get
		{
			CheckDisposed();
			if (FromPackageCache)
			{
				return _cachedResponse.Headers;
			}
			WaitForResponse();
			return _fullResponse.Headers;
		}
	}

	/// <summary>Gets the uniform resource identifier (URI) of the response. </summary>
	/// <returns>The URI of the response.</returns>
	public override Uri ResponseUri
	{
		get
		{
			CheckDisposed();
			if (FromPackageCache)
			{
				return _uri;
			}
			WaitForResponse();
			return PackUriHelper.Create(_fullResponse.ResponseUri, _partName);
		}
	}

	/// <summary>Gets a value indicating whether the response is from the package cache or from a Web request. </summary>
	/// <returns>true if the response is from the package cache; false if the response is from a Web request.</returns>
	public override bool IsFromCache
	{
		get
		{
			CheckDisposed();
			if (FromPackageCache)
			{
				return true;
			}
			WaitForResponse();
			return _fullResponse.IsFromCache;
		}
	}

	/// <summary>Gets the Multipurpose Internet Mail Extensions (MIME) content type of the response stream's content. </summary>
	/// <returns>The MIME type of the stream's content.</returns>
	public override string ContentType
	{
		get
		{
			CheckDisposed();
			if (!FromPackageCache)
			{
				WaitForResponse();
			}
			if (_mimeType == null)
			{
				GetResponseStream();
			}
			return _mimeType.ToString();
		}
	}

	/// <summary>Gets the content length of the response. </summary>
	/// <returns>The content length, in bytes.</returns>
	public override long ContentLength
	{
		get
		{
			CheckDisposed();
			if (FromPackageCache)
			{
				return _cachedResponse.ContentLength;
			}
			WaitForResponse();
			if (!_lengthAvailable)
			{
				_fullStreamLength = GetResponseStream().Length;
				_lengthAvailable = true;
			}
			return _fullStreamLength;
		}
	}

	private bool FromPackageCache => _cachedResponse != null;

	static PackWebResponse()
	{
	}

	internal PackWebResponse(Uri uri, Uri innerUri, Uri partName, WebRequest innerRequest)
	{
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (innerUri == null)
		{
			throw new ArgumentNullException("innerUri");
		}
		if (innerRequest == null)
		{
			throw new ArgumentNullException("innerRequest");
		}
		_lockObject = new object();
		_uri = uri;
		_innerUri = innerUri;
		_partName = partName;
		_webRequest = innerRequest;
		_mimeType = null;
		_responseAvailable = new ManualResetEvent(initialState: false);
		if (innerRequest.Timeout != -1)
		{
			_timeoutTimer = new Timer(TimeoutCallback, null, innerRequest.Timeout, -1);
		}
		_webRequest.BeginGetResponse(ResponseCallback, this);
	}

	internal PackWebResponse(Uri uri, Uri innerUri, Uri partName, Package cacheEntry, bool cachedPackageIsThreadSafe)
	{
		_lockObject = new object();
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (innerUri == null)
		{
			throw new ArgumentNullException("innerUri");
		}
		if (partName == null)
		{
			throw new ArgumentNullException("partName");
		}
		if (cacheEntry == null)
		{
			throw new ArgumentNullException("cacheEntry");
		}
		_uri = uri;
		_innerUri = innerUri;
		_partName = partName;
		_mimeType = null;
		_cachedResponse = new CachedResponse(this, cacheEntry, cachedPackageIsThreadSafe);
	}

	/// <summary>Gets the response stream that is contained in the <see cref="T:System.IO.Packaging.PackWebResponse" />. </summary>
	/// <returns>The response stream.</returns>
	public override Stream GetResponseStream()
	{
		CheckDisposed();
		if (FromPackageCache)
		{
			return _cachedResponse.GetResponseStream();
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXGetStreamBegin);
		if (_responseStream == null)
		{
			WaitForResponse();
			long contentLength = _fullResponse.ContentLength;
			_responseStream = _fullResponse.GetResponseStream();
			if (!_responseStream.CanSeek || !_innerUri.IsFile)
			{
				_responseStream = new NetStream(_responseStream, contentLength, _innerUri, _webRequest, _fullResponse);
				_responseStream = new BufferedStream(_responseStream);
			}
			if (_partName == null)
			{
				_fullStreamLength = contentLength;
				_mimeType = WpfWebRequestHelper.GetContentType(_fullResponse);
				_responseStream = new ResponseStream(_responseStream, this);
			}
			else
			{
				Package package = Package.Open(_responseStream);
				if (!package.PartExists(_partName))
				{
					throw new WebException(SR.WebResponsePartNotFound);
				}
				PackagePart part = package.GetPart(_partName);
				Stream seekableStream = part.GetSeekableStream(FileMode.Open, FileAccess.Read);
				_mimeType = new ContentType(part.ContentType);
				_fullStreamLength = seekableStream.Length;
				_responseStream = new ResponseStream(seekableStream, this, _responseStream, package);
			}
			if (_fullStreamLength >= 0)
			{
				_lengthAvailable = true;
			}
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXGetStreamEnd);
		return _responseStream;
	}

	/// <summary>Closes the stream for this request. </summary>
	public override void Close()
	{
		Dispose(disposing: true);
	}

	private void AbortResponse()
	{
		try
		{
			if (!_responseAvailable.WaitOne(0, exitContext: false))
			{
				_webRequest.Abort();
			}
		}
		catch (NotImplementedException)
		{
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (_disposed || !disposing || _disposed)
		{
			return;
		}
		if (FromPackageCache)
		{
			_cachedResponse.Close();
			_cachedResponse = null;
			return;
		}
		lock (_lockObject)
		{
			try
			{
				AbortResponse();
				_disposed = true;
				if (_responseStream != null)
				{
					_responseStream.Close();
				}
				if (_fullResponse != null)
				{
					((IDisposable)_fullResponse).Dispose();
				}
				_responseAvailable.Close();
				if (_timeoutTimer != null)
				{
					_timeoutTimer.Dispose();
				}
			}
			finally
			{
				_timeoutTimer = null;
				_responseStream = null;
				_fullResponse = null;
				_responseAvailable = null;
			}
		}
	}

	private void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("PackWebResponse");
		}
	}

	private void ResponseCallback(IAsyncResult ar)
	{
		lock (_lockObject)
		{
			try
			{
				if (!_disposed)
				{
					if (_timeoutTimer != null)
					{
						_timeoutTimer.Dispose();
					}
					_fullResponse = WpfWebRequestHelper.EndGetResponse(_webRequest, ar);
				}
			}
			catch (WebException responseException)
			{
				_responseException = responseException;
				_responseError = true;
			}
			catch
			{
				_responseError = true;
				throw;
			}
			finally
			{
				_timeoutTimer = null;
				if (!_disposed)
				{
					_responseAvailable.Set();
				}
			}
		}
	}

	private void WaitForResponse()
	{
		_responseAvailable.WaitOne();
		if (_responseError)
		{
			if (_responseException == null)
			{
				throw new WebException(SR.WebResponseFailure);
			}
			throw _responseException;
		}
	}

	private void TimeoutCallback(object stateInfo)
	{
		lock (_lockObject)
		{
			if (_disposed)
			{
				return;
			}
			try
			{
				if (!_responseAvailable.WaitOne(0, exitContext: false))
				{
					_responseError = true;
					_responseException = new WebException(SR.Format(SR.WebRequestTimeout, null), WebExceptionStatus.Timeout);
				}
				if (_timeoutTimer != null)
				{
					_timeoutTimer.Dispose();
				}
			}
			finally
			{
				_timeoutTimer = null;
				if (!_disposed)
				{
					_responseAvailable.Set();
				}
			}
		}
	}
}
